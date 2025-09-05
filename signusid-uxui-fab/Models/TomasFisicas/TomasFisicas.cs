namespace AspnetCoreMvcFull.Models.TomasFisicas
{
  public class TomasFisicas
  {
    public Guid IdTomaFIsica { get; set; }


    public DateTime? FechaInicial
    { get; set; }

    public DateTime? FechaFinal
    { get; set; }

    public string NombreTomaFisica { get; set; }

    public string descripcionTomaFisica { get; set; }

    public Guid? CategoriaTomaFisica { get; set; }

    public Guid? UsuarioAsignadoTomaFisica { get; set; }

    public Guid? UbicacionATomaFisica { get; set; }

    public Guid? UbicacionBTomaFisica { get; set; }

    public Guid? UnidadOrganizativaTomaFisica { get; set; }
    public Guid? EstadoActivo { get; set; }
    public Guid? UbicacionCTomaFisica { get; set; }
    public Guid? UbicacionDTomaFisica { get; set; }
    public int Activos {  get; set; }


  }
}
