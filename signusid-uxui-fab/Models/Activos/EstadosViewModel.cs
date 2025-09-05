namespace AspnetCoreMvcFull.Models.Activos
{
  public class EstadosViewModel
  {
    public List<EstadosActivos> Estados { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public string search { get; set; }
  }

}
