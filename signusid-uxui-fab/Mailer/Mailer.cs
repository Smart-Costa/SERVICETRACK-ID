using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Utils;
using System.Security.Authentication;

namespace AspnetCoreMvcFull.Mailer
{
  // ===== Config =====
  // --- NUEVO en tu modelo ---
  public sealed class SmtpSettings
  {
    public string Host { get; set; } = "";
    public int Port { get; set; } = 465;                   // 465 SSL por defecto
    public string User { get; set; } = "";
    public string Pass { get; set; } = "";
    public string From { get; set; } = "";
    public string FromName { get; set; } = "ServiceTrackID Notificaciones";
    public string? LogDir { get; set; }
    public bool TrustServerCertificate { get; set; } = false; // true solo para localhost/lab
  }


  // ===== Job / Resultado =====
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

  public sealed class MailResult
  {
    public MailJob Job { get; init; } = default!;
    public bool AcceptedBySmtp { get; init; }
    public string? Error { get; init; }
  }

  public static class Mailer
  {
    // ===== Logging a C:\logErroresCorreo\log.txt =====
    private const string ErrorLogDirDefault = @"C:\logErroresCorreo";
    private const string ErrorLogFileName = "log.txt";
    private const long MaxLogBytes = 5 * 1024 * 1024; // 5 MB
    private static readonly object _logLock = new();

    private static string EnsureErrorLogPath(string? dirFromCfg)
    {
      var dir = string.IsNullOrWhiteSpace(dirFromCfg) ? ErrorLogDirDefault : dirFromCfg!;
      try { Directory.CreateDirectory(dir); } catch { }
      return Path.Combine(dir, ErrorLogFileName);
    }
    private static void RotateIfNeeded(string path)
    {
      try
      {
        var fi = new FileInfo(path);
        if (fi.Exists && fi.Length >= MaxLogBytes)
        {
          var backup = Path.Combine(fi.DirectoryName!,
            $"log_{DateTime.UtcNow:yyyyMMdd_HHmmss_fff}.txt");
          File.Move(path, backup, overwrite: false);
        }
      }
      catch { }
    }
    private static void LogLine(string? dirFromCfg, string level, string msg)
    {
      try
      {
        var path = EnsureErrorLogPath(dirFromCfg);
        lock (_logLock)
        {
          RotateIfNeeded(path);
          File.AppendAllText(path,
            $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}Z] [{level}] {msg}{Environment.NewLine}");
        }
      }
      catch { }
    }
    private static void LogInfo(string? d, string m) => LogLine(d, "INFO", m);
    private static void LogWarn(string? d, string m) => LogLine(d, "WARN", m);
    private static void LogError(string? d, string m) => LogLine(d, "ERROR", m);

    // ===== Throttle suave entre mensajes =====
    private static readonly SemaphoreSlim GlobalGate = new(1, 1);
    private static DateTime _lastBatchSentUtc = DateTime.MinValue;
    private const int InterSendDelayMs = 1000; // 1s entre correos del mismo lote

    /// Envía varios correos en UNA conexión, guarda protocolo en C:\temp, y logea a C:\logErroresCorreo\log.txt
    public static async Task<(string logPath, List<MailResult> results)> SendBatchAsync(
   SmtpSettings cfg,
   IEnumerable<MailJob> jobs,
   CancellationToken ct = default)
    {
      if (string.IsNullOrWhiteSpace(cfg.Host))
        throw new InvalidOperationException("SMTP Host vacío.");

      var list = jobs?.ToList() ?? new();
      if (list.Count == 0) return ("", new());

      var from = string.IsNullOrWhiteSpace(cfg.From)
        ? (string.IsNullOrWhiteSpace(cfg.User) ? "noreply@localhost" : cfg.User)
        : cfg.From;

      // Gap global anti-ráfaga
      await GlobalGate.WaitAsync(ct);
      try
      {
        var neededGap = TimeSpan.FromMilliseconds(1500 * list.Count);
        var elapsed = DateTime.UtcNow - _lastBatchSentUtc;
        if (elapsed < neededGap)
          await Task.Delay(neededGap - elapsed, ct);
      }
      finally { GlobalGate.Release(); }

      LogInfo(cfg.LogDir, $"Batch start Host={cfg.Host} PortPref={cfg.Port} From={from} Jobs={list.Count}");

      // Protocol logger (MailKit) → C:\temp
      string protoDir = @"C:\temp";
      Directory.CreateDirectory(protoDir);
      string logFile = Path.Combine(protoDir, $"smtp_{DateTime.UtcNow:yyyyMMdd_HHmmss_fff}.log");

      var results = new List<MailResult>();

      using var logger = new ProtocolLogger(logFile);
      using var smtp = new MailKit.Net.Smtp.SmtpClient(logger);

      // TLS fuerte y sin XOAUTH2 (no usamos OAuth)
      smtp.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;
      smtp.AuthenticationMechanisms.Remove("XOAUTH2");
      smtp.Timeout = 20000;

      // Solo confiar en el cert si el perfil lo pide (lab/pruebas). En prod, dejar false.
      if (cfg.TrustServerCertificate)
        smtp.ServerCertificateValidationCallback = (s, cert, chain, errors) => true;

      try
      {
        LogInfo(cfg.LogDir, $"Connect try Host={cfg.Host} Port={cfg.Port}");
        await ConnectAndAuthAsync(smtp, cfg, cfg.Port, ct);   // usa la versión que te pasé (465=SSL, 587=StartTls)
        LogInfo(cfg.LogDir, $"Connected & Authenticated Host={cfg.Host} Port={cfg.Port}");

        for (int i = 0; i < list.Count; i++)
        {
          var job = list[i];
          var msg = BuildMimeMessage(from, cfg.FromName, job);

          try
          {
            await smtp.SendAsync(msg, ct);
            results.Add(new MailResult { Job = job, AcceptedBySmtp = true });
            LogInfo(cfg.LogDir, $"250 OK to='{job.To}' subj='{job.Subject}'");
          }
          catch (Exception ex)
          {
            LogWarn(cfg.LogDir, $"Send failed first try to='{job.To}' subj='{job.Subject}' ex='{ex.Message}'");

            if (!smtp.IsConnected || !smtp.IsAuthenticated)
            {
              await SafeReconnectAsync(smtp, cfg, cfg.Port, ct);
              try
              {
                await smtp.SendAsync(msg, ct);
                results.Add(new MailResult { Job = job, AcceptedBySmtp = true });
                LogInfo(cfg.LogDir, $"250 OK (after reconnect) to='{job.To}' subj='{job.Subject}'");
              }
              catch (Exception ex2)
              {
                results.Add(new MailResult { Job = job, AcceptedBySmtp = false, Error = ex2.Message });
                LogError(cfg.LogDir, $"FAILED to='{job.To}' subj='{job.Subject}' ex='{ex2}'");
              }
            }
            else
            {
              results.Add(new MailResult { Job = job, AcceptedBySmtp = false, Error = ex.Message });
              LogError(cfg.LogDir, $"FAILED (connected) to='{job.To}' subj='{job.Subject}' ex='{ex}'");
            }
          }

          if (i < list.Count - 1)
            await Task.Delay(InterSendDelayMs, ct);
        }

        await smtp.DisconnectAsync(true, ct);
        LogInfo(cfg.LogDir, $"Batch end OK. ProtocolLog='{logFile}'");

        await GlobalGate.WaitAsync(ct);
        try { _lastBatchSentUtc = DateTime.UtcNow; }
        finally { GlobalGate.Release(); }

        return (logFile, results);
      }
      catch (Exception ex)
      {
        LogError(cfg.LogDir, $"Batch FAILED Host={cfg.Host} ex='{ex.Message}'. ProtocolLog='{logFile}'");
        if (smtp.IsConnected) { try { await smtp.DisconnectAsync(true, ct); } catch { } }
        throw;
      }
    }


    private static async Task ConnectAndAuthAsync(SmtpClient smtp, SmtpSettings cfg, int port, CancellationToken ct)
    {
      if (string.IsNullOrWhiteSpace(cfg.Host))
        throw new InvalidOperationException("SMTP Host vacío.");

      // Elegir modo TLS correcto según puerto
      var secure = SecureSocketOptions.Auto;
      if (port == 465) secure = SecureSocketOptions.SslOnConnect;
      else if (port == 587) secure = SecureSocketOptions.StartTls;

      await smtp.ConnectAsync(cfg.Host, port, secure, ct);

      // Si hay credenciales, autenticamos (recomendado para 465/587)
      if (!string.IsNullOrWhiteSpace(cfg.User))
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
      if (!string.IsNullOrWhiteSpace(j.ReplyTo))
        msg.ReplyTo.Add(MailboxAddress.Parse(j.ReplyTo));

      // Message-Id con el dominio del From (mejor para antispam)
      var fromDomain = "localhost";
      var at = from.IndexOf('@');
      if (at > 0 && at < from.Length - 1) fromDomain = from[(at + 1)..];

      msg.MessageId = MimeUtils.GenerateMessageId(fromDomain);
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
