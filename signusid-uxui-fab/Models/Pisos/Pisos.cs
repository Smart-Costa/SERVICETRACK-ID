namespace AspnetCoreMvcFull.Models.Pisos
{
  public class Pisos
  {
    public Guid buildingSysId { get; set; }
    public Guid companySysId { get; set; }
    public string name { get; set; }
    public string description { get; set; }
    public Guid entryUser {  get; set; }
    public DateTime entryDate { get; set; }
    public Guid updateUser { get; set; }
    public DateTime updateDate { get; set; }
    public Guid rowGuid { get; set; }
    public string companyIdExtern { get; set; }
    public int Activos { get; set; }
    public string Edificio { get; set; }
  }
}
