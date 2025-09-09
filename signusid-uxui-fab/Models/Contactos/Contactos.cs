using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AspnetCoreMvcFull.Models.Contactos
{
  [Table("Contactos", Schema = "dbo")]
  public class Contactos
  {
    [Key]
    [Column("Id_Contacto")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid IdContacto { get; set; }

    [Column("Nombre")]
    [StringLength(100)]
    public string? Nombre { get; set; }

    [Column("Apellidos")]
    [StringLength(200)]
    public string? Apellidos { get; set; }

    [Column("Direccion")]
    [StringLength(200)]
    public string? Direccion { get; set; }

    [Column("IdentificacionContacto")]
    [StringLength(200)]
    public string? IdentificacionContacto { get; set; }

    [Column("Empresa")]
    [StringLength(200)]
    public string? Empresa { get; set; }

    [Column("Direccion_Opcional")]
    [StringLength(200)]
    public string? DireccionOpcional { get; set; }

    [Column("Email")]
    [StringLength(30)]
    public string? Email { get; set; }


    [Column("Telefono")]
    [StringLength(30)]
    public string? Telefono { get; set; }

    [Column("Ciudad")]
    [StringLength(100)]
    public string? Ciudad { get; set; }

    [Column("Estado")]
    [StringLength(100)]
    public string? Estado { get; set; }

    [Column("Puesto")]
    [StringLength(100)]
    public string? Puesto { get; set; }

    [Column("Telefono_Movil")]
    [StringLength(30)]
    public string? TelefonoMovil { get; set; }

    [Column("Codigo_Postal")]
    [StringLength(100)]
    public string? CodigoPostal { get; set; }


    [Column("Pais")]
    [StringLength(200)]
    public string? Pais { get; set; }

    [Column("Estatus")]
    [StringLength(200)]
    public string? Estatus { get; set; }

    [Column("Identificacion")]
    [StringLength(200)]
    public string? Identificacion { get; set; }
  }
}
