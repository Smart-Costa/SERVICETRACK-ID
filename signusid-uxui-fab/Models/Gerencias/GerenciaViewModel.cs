namespace AspnetCoreMvcFull.Models.Gerencias
{
  public class GerenciaViewModel
  {
    public List<Gerencia> Gerencias { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public string search { get; set; }
  }
}
