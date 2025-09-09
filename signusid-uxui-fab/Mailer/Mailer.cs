using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Utils;
using System.Security.Authentication;

namespace AspnetCoreMvcFull.Mailer
{
  // Config básica
  public sealed class SmtpSettings
  {
    public string Host { get; set; } = "";
    public int Port { get; set; } = 465; // HostGator: 465 (SSL). Para Outlook: 587 (STARTTLS).
    public string User { get; set; } = "";
    public string Pass { get; set; } = "";
    public string From { get; set; } = "";  // si vacío, usa User
    public string FromName { get; set; } = "ServiceTrackID Notificaciones";

    // Opcional: carpeta donde guardar logs SMTP (si null usa C:\temp)
    public string? LogDir { get; set; }
  }

  // Un envío
  public sealed class MailJob
  {
    public string To { get; init; } = "";
    public string Subject { get; init; } = "";
    public string PlainText { get; init; } = "";
    public string Html { get; init; } = "";
    public string? Cc { get; init; }
    public string? Bcc { get; init; }
    public string? ReplyTo { get; init; }
  }

  // Resultado por mensaje
  public sealed class MailResult
  {
    public MailJob Job { get; init; } = default!;
    public bool AcceptedBySmtp { get; init; }   // true si el servidor respondió 250 OK tras DATA
    public string? Error { get; init; }         // texto de error si falló el envío en SMTP
  }

  public static class Mailer
  {
    // Serializa lotes (insert -> edit pegados) y aplica una pausa global
    private static readonly SemaphoreSlim GlobalGate = new(1, 1);
    private static DateTime _lastBatchSentUtc = DateTime.MinValue;

    // Pausa entre mensajes del mismo lote (anti-burst)
    private const int InterSendDelayMs = 800;

    /// <summary>
    /// Envía varios correos en UNA conexión SMTP, registra log y devuelve resultados por mensaje.
    /// </summary>
    public static async Task<(string logPath, List<MailResult> results)> SendBatchAsync(
      SmtpSettings cfg,
      IEnumerable<MailJob> jobs,
      CancellationToken ct = default)
    {
      if (string.IsNullOrWhiteSpace(cfg.Host)) throw new InvalidOperationException("SMTP Host vacío.");
      if (string.IsNullOrWhiteSpace(cfg.User)) throw new InvalidOperationException("SMTP User vacío.");

      var list = jobs?.ToList() ?? new();
      if (list.Count == 0)
        return (logPath: "", results: new List<MailResult>());

      var from = string.IsNullOrWhiteSpace(cfg.From) ? cfg.User : cfg.From;

      // Gap global entre lotes (evita ráfagas insert→edit)
      await GlobalGate.WaitAsync(ct);
      try
      {
        var neededGap = TimeSpan.FromMilliseconds(1500 * list.Count); // ~1.5s por correo
        var now = DateTime.UtcNow;
        var elapsed = now - _lastBatchSentUtc;
        if (elapsed < neededGap)
        {
          var wait = neededGap - elapsed;
          await Task.Delay(wait, ct);
        }
      }
      finally
      {
        GlobalGate.Release();
      }

      // Archivo de log para este lote
      string logDir = string.IsNullOrWhiteSpace(cfg.LogDir) ? @"C:\temp" : cfg.LogDir!;
      Directory.CreateDirectory(logDir);
      string logFile = Path.Combine(logDir, $"smtp_{DateTime.UtcNow:yyyyMMdd_HHmmss_fff}.log");

      var results = new List<MailResult>();

      // Preferir 465 (HostGator). Para Outlook, más abajo te muestro cómo fijar 587.
      foreach (var port in PreferredPorts(cfg))
      {
        using var logger = new ProtocolLogger(logFile);
        using var smtp = new MailKit.Net.Smtp.SmtpClient(logger) { Timeout = 15000 };

        // Si el cert del servidor es válido, comenta la línea de abajo.
        smtp.ServerCertificateValidationCallback = (s, cert, chain, errors) => true;

        try
        {
          await ConnectAndAuthAsync(smtp, cfg, port, ct);

          for (int i = 0; i < list.Count; i++)
          {
            var job = list[i];
            var msg = BuildMimeMessage(from, cfg.FromName, job);

            try
            {
              await smtp.SendAsync(msg, ct);
              results.Add(new MailResult { Job = job, AcceptedBySmtp = true });
            }
            catch (Exception ex)
            {
              // Si se cayó la sesión, reintenta reconectando una sola vez
              if (!smtp.IsConnected || !smtp.IsAuthenticated)
              {
                await SafeReconnectAsync(smtp, cfg, port, ct);
                try
                {
                  await smtp.SendAsync(msg, ct);
                  results.Add(new MailResult { Job = job, AcceptedBySmtp = true });
                }
                catch (Exception ex2)
                {
                  results.Add(new MailResult { Job = job, AcceptedBySmtp = false, Error = ex2.Message });
                }
              }
              else
              {
                results.Add(new MailResult { Job = job, AcceptedBySmtp = false, Error = ex.Message });
              }
            }

            if (i < list.Count - 1)
              await Task.Delay(InterSendDelayMs, ct);
          }

          await smtp.DisconnectAsync(true, ct);

          // Marca fin de lote para calcular el gap del siguiente
          await GlobalGate.WaitAsync(ct);
          try { _lastBatchSentUtc = DateTime.UtcNow; }
          finally { GlobalGate.Release(); }

          return (logFile, results); // OK (no probamos más puertos)
        }
        catch
        {
          // Si falla este puerto, probamos el otro (465 -> 587 o viceversa)
          if (smtp.IsConnected) { try { await smtp.DisconnectAsync(true, ct); } catch { } }
        }
      }

      // Si no funcionó ningún puerto, devolvemos lo que haya y el path del log
      throw new InvalidOperationException($"No se pudo enviar por SMTP (revisa el log: {logFile}).");
    }

    // ===== Helpers =====

    // Para HostGator: intenta 465 y luego 587. Para Outlook, usa 587 (ver nota al final).
    private static IEnumerable<int> PreferredPorts(SmtpSettings cfg)
    {
      // Office 365: solo 587
      if (cfg.Host.EndsWith("office365.com", StringComparison.OrdinalIgnoreCase))
        return new[] { 587 };

      // HostGator u otros: conserva preferencia
      if (cfg.Port == 465) return new[] { 465, 587 };
      if (cfg.Port == 587) return new[] { 587, 465 };
      return new[] { 465, 587 };
    }

    private static async Task ConnectAndAuthAsync(MailKit.Net.Smtp.SmtpClient smtp, SmtpSettings cfg, int port, CancellationToken ct)
    {
      // Opcional pero recomendado: limitar protocolos
      smtp.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;

      if (cfg.Host.EndsWith("office365.com", StringComparison.OrdinalIgnoreCase))
      {
        // O365 SIEMPRE por 587 + STARTTLS
        await smtp.ConnectAsync(cfg.Host, port, SecureSocketOptions.StartTls, ct);
      }
      else
      {
        var ssl = (port == 465) ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls;
        await smtp.ConnectAsync(cfg.Host, port, ssl, ct);
      }

      smtp.AuthenticationMechanisms.Remove("XOAUTH2"); // seguimos con user/pass
      await smtp.AuthenticateAsync(cfg.User, cfg.Pass, ct);
    }
    private static async Task SafeReconnectAsync(MailKit.Net.Smtp.SmtpClient smtp, SmtpSettings cfg, int port, CancellationToken ct)
    {
      if (smtp.IsConnected) { try { await smtp.DisconnectAsync(true, ct); } catch { } }
      await ConnectAndAuthAsync(smtp, cfg, port, ct);
    }

    private static MimeMessage BuildMimeMessage(string from, string fromName, MailJob j)
    {
      var msg = new MimeMessage();
      msg.From.Add(new MailboxAddress(fromName ?? "", from));
      foreach (var a in Split(j.To)) msg.To.Add(MailboxAddress.Parse(a));
      foreach (var a in Split(j.Cc)) msg.Cc.Add(MailboxAddress.Parse(a));
      foreach (var a in Split(j.Bcc)) msg.Bcc.Add(MailboxAddress.Parse(a));
      if (!string.IsNullOrWhiteSpace(j.ReplyTo)) msg.ReplyTo.Add(MailboxAddress.Parse(j.ReplyTo));

      // IDs únicos y fecha actual para evitar deduplicaciones
      msg.MessageId = MimeUtils.GenerateMessageId("signusid.com");
      msg.Date = DateTimeOffset.Now;

      msg.Subject = j.Subject;
      msg.Headers.Add("X-ServiceTrackID", DateTime.UtcNow.ToString("yyyyMMddHHmmssfff"));

      var body = new BodyBuilder { TextBody = j.PlainText ?? "", HtmlBody = j.Html ?? "" };
      msg.Body = body.ToMessageBody();
      return msg;
    }

    private static IEnumerable<string> Split(string? emails)
    {
      if (string.IsNullOrWhiteSpace(emails)) yield break;
      foreach (var raw in emails.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries))
      {
        var e = raw.Trim();
        if (!string.IsNullOrWhiteSpace(e)) yield return e;
      }
    }
  }
}
