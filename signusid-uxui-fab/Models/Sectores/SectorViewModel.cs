namespace AspnetCoreMvcFull.Models.Sectores
{
  public class SectorViewModel
  {
    public List<Sector> Sector { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public string search { get; set; }
  }
}
