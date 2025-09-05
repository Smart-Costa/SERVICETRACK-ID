namespace AspnetCoreMvcFull.Models.GestionServicios
{
  public class GestionServiciosDTO
  {
    public Guid GestionServiciosId { get; set; }
    public string? NombreSolicitante { get; set; }
    public string? NombreActivo { get; set; }
    public string? NombreRazonServicio { get; set; }
    public string? NombreEstadoActivo { get; set; }
    public string? NombreAsignarIncidente { get; set; }
    public DateTime? FechaEstimadaCierre { get; set; }
    public DateTime Fecha { get; set; }
    public string? Descripcion { get; set; }
    public int NumeroTicket { get; set; }

    // ← estas 4 eran Guid; pásalas a Guid?
    public Guid? SolicitanteId { get; set; }
    public Guid? ActivoId { get; set; }
    public Guid? RazonServicioId { get; set; }
    public Guid? EstadoActivoId { get; set; }
    public Guid? AsignarIncidenteId { get; set; } // ya era nullable
  }

}
