namespace AspnetCoreMvcFull.Models.GestionServicios
{
  public class GestionServiciosViewModel
  {
    public List<GestionServicios> Registros { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public string Search { get; set; }
  }
}
