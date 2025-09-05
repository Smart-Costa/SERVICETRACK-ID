using Diverscan.Activos.DL;
using Diverscan.Activos.EL;

public static class RoleService
{


  //Obtener todos los roles del backend ActiveID]
  public static Roles[] GetAllRoles()
  {
    //Obtener lista de roles del Backend ActiveID
    Roles[] listaRoles = AccessoRoles.Listar();
    return listaRoles;

  }

  //metodo para obtener rol con Guid
  public static string? GetRolNameByGuid(Guid RolId)
  {
    //se llama al metodo de ActiveID para obtener el nombre del rol segun su Guid
    var rolNombre = AccessoRoles.Obtenernombreporid(RolId);
    //se retorna el nombre en formato string
    return (rolNombre != null) ? rolNombre.ToString() : "";

  }


}
