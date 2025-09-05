namespace AspnetCoreMvcFull.Models.Marcas
{
  public class Marcas
  {



    public Guid MarcaId { get; set; }
    public string Nombre { get; set; }
    public string Descripcion { get; set; }
    public Guid EntryUser { get; set; }
    public DateTime EntryDate { get; set; }
    public Guid UpdateUser { get; set; }
    public DateTime UpdateDate { get; set; }
    public int Actives { get; set; }

    public Marcas(Guid marcaId, string nombre, string descripcion, Guid entryUser, DateTime entryDate, Guid updateUser, DateTime updateDate, int actives)
    {
      MarcaId = marcaId;
      Nombre = nombre;
      Descripcion = descripcion;
      EntryUser = entryUser;
      EntryDate = entryDate;
      UpdateUser = updateUser;
      UpdateDate = updateDate;
      Actives = actives;
    }

    public Marcas() { }
  }
}
