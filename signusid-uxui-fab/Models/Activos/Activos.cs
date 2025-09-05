namespace AspnetCoreMvcFull.Models.Activos
{
  public class Activos
  {
    public Guid IdActivo { get; set; }

    
    public int? NumeroActivo
    { get; set; }

    public string NumeroEtiqueta { get; set; }

    public string DescripcionCorta { get; set; }

    
    public string DescripcionLarga
    { get; set; }

    public Guid? Categoria { get; set; }

    public Guid? Estado { get; set; }

    public Guid? Empresa { get; set; }

    public Guid? Marca { get; set; }

    public Guid? Modelo { get; set; }

    public string NumeroSerie { get; set; }

    public decimal? Costo { get; set; }

    public string NumeroFactura { get; set; }

    
    public DateTime? FechaCompra
    { get; set; }

    public DateTime? FechaCapitalizacion { get; set; }

    public double? ValorResidual { get; set; }

    public Guid? Documento { get; set; }

    public Guid? Fotos { get; set; }

    public string NumeroParteFabricante { get; set; }

    public string Depreciado { get; set; }

    public string DescripcionDepreciado { get; set; }

    public int? AnosVidaUtil { get; set; }

    public Guid? CuentaContableDepresiacion { get; set; }

    public Guid? CentroCostos { get; set; }

    public string DescripcionEstadoUltimoInventario { get; set; }

    public string TagEPC { get; set; }

    public Guid? Empleado { get; set; }

    public Guid? UbicacionA { get; set; }

    public Guid? UbicacionB { get; set; }

    public Guid? UbicacionC { get; set; }

    public Guid? UbicacionD { get; set; }

    public Guid? UbicacionSecundaria { get; set; }

    public DateTime? FechaGarantia { get; set; }

    public string Color { get; set; }

    public string TamanioMedida { get; set; }

    public string Observaciones { get; set; }

    public int Estado_Activo { get; set; }
  }
}
