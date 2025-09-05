namespace AspnetCoreMvcFull.Models.Oficinas
{
  public class Oficina
  {
    public Guid OfficeSysId { get; set; }
    public Guid CompanySysId { get; set; }
    public Guid BuildingSysId { get; set; }
    public Guid BusinessUnitSysId { get; set; }
    public Guid FloorSysId { get; set; }
    public Guid DeptSysId { get; set; }
    public Guid TagSysId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public Guid EntryUser { get; set; }
    public DateTime EntryDate { get; set; }
    public Guid UpdateUser { get; set; }
    public DateTime UpdateDate { get; set; }
    public Guid RowGuid { get; set; }
    public bool IsEnable { get; set; }
    public string CompanyIdExtern { get; set; }
    public int Activos { get; set; }
    public string Edificios { get; set; }
    public string Pisos { get; set; }
    public string Sector {  get; set; }

    public Oficina(Guid officeSysId, Guid companySysId, Guid buildingSysId, Guid businessUnitSysId, Guid floorSysId, Guid deptSysId, Guid tagSysId, string name, string description, Guid entryUser, DateTime entryDate, Guid updateUser, DateTime updateDate, Guid rowGuid, bool isEnable)
    {
      OfficeSysId = officeSysId;
      CompanySysId = companySysId;
      BuildingSysId = buildingSysId;
      BusinessUnitSysId = businessUnitSysId;
      FloorSysId = floorSysId;
      DeptSysId = deptSysId;
      TagSysId = tagSysId;
      Name = name;
      Description = description;
      EntryUser = entryUser;
      EntryDate = entryDate;
      UpdateUser = updateUser;
      UpdateDate = updateDate;
      RowGuid = rowGuid;
      IsEnable = isEnable;
   

    }

    public Oficina()
    {
     
    }
  }
}
