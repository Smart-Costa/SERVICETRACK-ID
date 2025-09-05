namespace AspnetCoreMvcFull.Models.Activos
{
  public class ModelosActivos
  {
    public Guid modeloID { get; set; }
    public Guid marcaID { get; set; }
    public string marca { get; set; }
    public string name { get; set; }
    public string description { get; set; }
    public int assignatedAssets { get; set; }
  }
}
