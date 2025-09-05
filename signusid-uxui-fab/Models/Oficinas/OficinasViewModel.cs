namespace AspnetCoreMvcFull.Models.Oficinas
{
  public class OficinasViewModel
  {
    public List<Oficina> Oficinas { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public string search { get; set; }
  }
}
