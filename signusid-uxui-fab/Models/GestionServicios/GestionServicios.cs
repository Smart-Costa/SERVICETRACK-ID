using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AspnetCoreMvcFull.Models.GestionServicios
{
  public class GestionServicios
  {
    public Guid GestionServiciosId { get; set; }
    public Guid Solicitante { get; set; }
    public Guid Activo { get; set; }
    public Guid RazonServicio { get; set; }
    public Guid EstadoActivo { get; set; }
    public Guid AsignarIncidente { get; set; }
    public DateTime FechaEstimadaCierre { get; set; }
    public DateTime Fecha { get; set; }
    public string Descripcion { get; set; }
    public int NumeroTicket { get; set; }
    public string TelefonoMovil { get; set; }
  }
}
