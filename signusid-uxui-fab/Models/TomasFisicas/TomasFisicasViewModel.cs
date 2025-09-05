using AspnetCoreMvcFull.Models.Activos;

namespace AspnetCoreMvcFull.Models.TomasFisicas
{
  public class TomasFisicasViewModel
  {
    public List<TomasFisicas> Tomas { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public string search { get; set; }
  }
}
