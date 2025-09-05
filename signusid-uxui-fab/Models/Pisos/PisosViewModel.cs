namespace AspnetCoreMvcFull.Models.Pisos
{
  public class PisosViewModel
  {
    public List<Pisos> Pisos { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public string search { get; set; }
  }
}
