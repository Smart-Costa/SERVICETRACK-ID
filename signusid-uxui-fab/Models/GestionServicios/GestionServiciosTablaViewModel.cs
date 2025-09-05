namespace AspnetCoreMvcFull.Models.GestionServicios
{
  public class GestionServiciosTablaViewModel
  {
    public List<GestionServiciosDTO> Registros { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public string Search { get; set; }
  }
}
