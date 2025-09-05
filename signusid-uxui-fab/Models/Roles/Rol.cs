
using System.ComponentModel.DataAnnotations;

namespace AspnetCoreMvcFull.Models.Roles
{
  public class Rol
  {
    public Guid RoleId { get; set; } // Cambiado a Guid
    [Required(ErrorMessage = "El nombre del rol es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
    [MinLength(3, ErrorMessage = "El nombre debe tener al menos 3 caracteres.")]
    public required string RoleName { get; set; }

    [Required(ErrorMessage = "La descripción del rol es obligatoria.")]
    [StringLength(2500, ErrorMessage = "La descripción no puede superar los 2500 caracteres.")]
    [MinLength(3, ErrorMessage = "La descripción debe tener al menos 3 caracteres.")]
    public required string Descripcion { get; set; }

    public bool Bloqueado { get; set; } // Cambiado a bool
    public int TotalUsers { get; set; }
    public List<string> Permisos { get; set; } = new List<string>();
  }

}
