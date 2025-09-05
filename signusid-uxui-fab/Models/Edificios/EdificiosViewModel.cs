using AspnetCoreMvcFull.Models.Activos;

namespace AspnetCoreMvcFull.Models.Edificios
{
  public class EdificiosViewModel
  {
    public List<Edificios> Edificios { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public string search { get; set; }
  }
}
