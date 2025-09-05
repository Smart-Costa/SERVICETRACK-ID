namespace AspnetCoreMvcFull.Models.Edificios
{
  public class Edificios
  {
    public Guid companySysId { get; set; }
    public string name { get; set; }
    public string description { get; set; }
    public Guid entryUser { get; set; }
    public DateTime entryDate { get; set; }
    public Guid updateUser { get; set; }
    public Guid  rowGuid { get; set; }
    public int Activos { get; set; }

  }
}
