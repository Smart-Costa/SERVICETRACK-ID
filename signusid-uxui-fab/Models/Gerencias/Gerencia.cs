namespace AspnetCoreMvcFull.Models.Gerencias
{
  public class Gerencia
  {
    public Gerencia(int idLogicoA, string nombre, string descripcion)
    {
      IdLogicoA = idLogicoA;
      Nombre = nombre;
      Descripcion = descripcion;
    }

    public Gerencia(int idLogicoA, string nombre, string descripcion, Guid userSysId, int activos) : this(idLogicoA, nombre, descripcion)
    {
      UserSysId = userSysId;
      Activos = activos;
    }

    public Gerencia()
    {
      
    }

    public int IdLogicoA { get; set; }
    public string Nombre { get; set; }
    public string Descripcion { get; set; }
    public Guid UserSysId { get; set; }
    public int Activos { get; set; }
  }
}
