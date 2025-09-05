using System.ComponentModel.DataAnnotations;

namespace AspnetCoreMvcFull.Models.Roles
{
  public class RolesViewModel
  {
    public required IEnumerable<Rol> Roles { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public required string SearchTerm { get; set; }
    [Required(ErrorMessage = "El nombre del rol es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
    [MinLength(3, ErrorMessage = "El nombre debe tener al menos 3 caracteres.")]
    public string RoleName { get; set; } = "";
    [Required(ErrorMessage = "La descripción del rol es obligatoria.")]
    [StringLength(2500, ErrorMessage = "La descripción no puede superar los 2500 caracteres.")]
    [MinLength(3, ErrorMessage = "La descripción debe tener al menos 3 caracteres.")]
    public string Descripcion { get; set; } = "";
    public List<string> PermisosSeleccionados { get; set; } = new List<string>();
    //Lista de Permisos (temporal)
    public List<string> TodosLosPermisos { get; set; } = new List<string>
    {
        "Gestión de Usuarios",
        "Gestión de Roles",
        "Ver Informes",
        "Gestionar Configuraciones",
        "Administrar Permisos",
        "Acceso a la API",
        "Gestionar Contenido",
        "Monitoreo de Actividades",
        "Acceso a Datos Sensibles",
        "Gestionar Proyectos",
        "Auditoría de Seguridad",
        "Administrar Base de Datos"
    };
  }
}
