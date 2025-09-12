namespace AspnetCoreMvcFull.Models.GestionServicios
{
  public class ControlTraficoListItem
  {
    public Guid ControlTraficoId { get; set; }

    public Guid? ContratoId { get; set; }
    public string? ContratoNumero { get; set; }          // CONTRATOS.NUMERO (para mostrar)

    public Guid? RazonServicioId { get; set; }
    public string? RazonServicioNombre { get; set; }     // RazonServicios.nombre

    public DateTime? FechaCreacionUtc { get; set; }      // CONTROL_TRAFICO.FechaCreacionUtc

    public Guid? AsignadoAId { get; set; }
    public string? AsignadoAUsername { get; set; }       // users.username

    public int Ticket { get; set; }                      // CONTROL_TRAFICO.Ticket

    public DateTime? FechaCierre { get; set; }           // CONTROL_TRAFICO.FechaCierre

    public Guid? EstadoIncidenteId { get; set; }
    public string? EstadoIncidenteNombre { get; set; }   // EstadoActivo.nombre
  }

  public class ControlTraficoTablaViewModel
  {
    public List<ControlTraficoListItem> Registros { get; set; } = new();
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public string? Search { get; set; }
    public string SortColumn { get; set; } = "Ticket";
    public string SortDirection { get; set; } = "asc";
  }

}
