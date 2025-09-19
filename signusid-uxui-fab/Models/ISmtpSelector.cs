using AspnetCoreMvcFull.Mailer;
// SmtpSelector.cs
using Microsoft.Extensions.Options;

namespace AspnetCoreMvcFull.Models
{
  // ISmtpSelector.cs
  public interface ISmtpSelector
  {
    SmtpSettings Pick(bool gd, bool sc, bool sid, out string marca);
  }
  public sealed class SmtpSelector : ISmtpSelector
  {
    private readonly IOptionsMonitor<SmtpSettings> _opts;
    public SmtpSelector(IOptionsMonitor<SmtpSettings> opts) => _opts = opts;

    public SmtpSettings Pick(bool gd, bool sc, bool sid, out string marca)
    {
      // Define tu priorización como quieras:
      // Ejemplo: SID => Signus, SC => Smartcosta, GD => Diverscan, por defecto Signus
      if (sid) { marca = "Signus"; return Clone(_opts.Get("Signus")); }
      if (sc) { marca = "Smartcosta"; return Clone(_opts.Get("Smartcosta")); }
      if (gd) { marca = "Diverscan"; return Clone(_opts.Get("Diverscan")); }

      marca = "Signus";
      return Clone(_opts.Get("Signus"));
    }

    private static SmtpSettings Clone(SmtpSettings s) => new SmtpSettings
    {
      Host = s.Host,
      Port = s.Port == 0 ? 465 : s.Port,
      User = s.User,
      Pass = s.Pass,
      From = string.IsNullOrWhiteSpace(s.From) ? s.User : s.From,
      FromName = string.IsNullOrWhiteSpace(s.FromName) ? "Notificaciones" : s.FromName,
      LogDir = s.LogDir,
      TrustServerCertificate = s.TrustServerCertificate
    };
  }

}
