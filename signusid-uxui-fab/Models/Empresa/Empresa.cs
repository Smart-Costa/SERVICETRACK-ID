using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AspnetCoreMvcFull.Models.Empresa
{
  [Table("EMPRESA", Schema = "dbo")]
  public class Empresa
  {
    [Key]
    [Column("ID_EMPRESA")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid IdEmpresa { get; set; }

    [Column("NOMBRE")]
    [StringLength(100)]
    public string? Nombre { get; set; }

    [Column("DESCRIPCION")]
    [StringLength(100)]
    public string? Descripcion { get; set; }

    [Column("DIRECCION_EMPRESA")]
    [StringLength(200)]
    public string? DireccionEmpresa { get; set; }

    [Column("IDENTIFICACION_E_RELACIONADA")]
    [StringLength(200)]
    public string? IdentificacionERelacionada { get; set; }

    [Column("EMPRESA_RELACIONADA")]
    [StringLength(200)]
    public string? EmpresaRelacionada { get; set; }

    [Column("DIRECCION_E_RELACIONADA")]
    [StringLength(200)]
    public string? DireccionERelacionada { get; set; }

    [Column("EMAIL")]
    [StringLength(30)]
    public string? Email { get; set; }

    [Column("TELEFONO")]
    [StringLength(30)]
    public string? Telefono { get; set; }

    [Column("CIUDAD")]
    [StringLength(100)]
    public string? Ciudad { get; set; }

    [Column("ESTADO")]
    [StringLength(100)]
    public string? Estado { get; set; }

    [Column("FORMA_PAGO")]
    [StringLength(100)]
    public string? FormaPago { get; set; }

    [Column("CONDICION_FINANCIERA")]
    [StringLength(200)]
    public string? CondicionFinanciera { get; set; }

    [Column("CODIGO_POSTAL")]
    [StringLength(100)]
    public string? CodigoPostal { get; set; }

    [Column("PAIS")]
    [StringLength(200)]
    public string? Pais { get; set; }

    [Column("ESTATUS")]
    [StringLength(200)]
    public string? Estatus { get; set; }

    [Column("IDENTIFICACION")]
    [StringLength(200)]
    public string? Identificacion { get; set; }
  }
}
