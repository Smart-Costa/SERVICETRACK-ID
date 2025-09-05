using System.Configuration;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AspnetCoreMvcFull.Models;
using AspnetCoreMvcFull.Models.Roles;
using AspnetCoreMvcFull.Models.Permissions;
using System.Text.Json;
using System.Text.Json.Serialization;
using Diverscan.Activos.DL;
using Diverscan.Activos.EL;
using System.Data.SqlClient;
using AspnetCoreMvcFull.Models.Mensajes;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data;



namespace AspnetCoreMvcFull.Controllers;

public class AccessController : Controller
{

  //CLASES TEMPORALES PARA LEER LA DATA DEL JSON
  public class PermissionEntity
  {
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("description")]
    public string Description { get; set; } = "";

    [JsonPropertyName("roles")]
    public string[] Roles { get; set; } = [];

    [JsonPropertyName("creation_date")]
    public DateTime CreationDate { get; set; }
  }

  public class PermissionEntityList
  {
    public List<PermissionEntity> Permissions { get; set; } = new List<PermissionEntity>();
  }

  //METODO PARA OBTENER LOS DATOS TEMPORALES DEL JSON
  // Este metodo consula especificamente el archivo que contiene los datos JSON temporales para mostrar y los devuelve
  // en tipo Array de Permisos PermissionEntity[]
  public PermissionEntity[] Get_dummy_permissions_data_from_JSON()
  {

    /*
      PROCESOS PARA LEER LA DATA DEL JSON
      NO SERAN NECESARIOS UNA VEZ QUE SE UTILICE LA API
    */
    //Definir la ruta del archivo JSON
    string jsonPath = "Data/Permisos/DummyData.json";
    //Si la ruta no existe notificar en consola
    if (!System.IO.File.Exists(jsonPath))
    {
      Console.WriteLine($"El archivo no existe en la ruta: {jsonPath}");
    }
    //Leer la data del archivo JSON
    string jsonContent = System.IO.File.ReadAllText(jsonPath);

    // Deserializa el JSON para convertirlo en variable de tipo PermissionEntityList
    var data = JsonSerializer.Deserialize<PermissionEntityList>(jsonContent);

    //Definir la variable Empleados Array que se va a utilizar
    PermissionEntity[] permissions_list = [];

    //Si la data es valida entonces convertir de tipo List<PermissionEntity> a PermissionEntity[]
    if (data != null && data.Permissions.Count != 0)
    {
      permissions_list = data.Permissions.ToArray();
    }
    /*
      PROCESOS PARA LEER LA DATA DEL JSON
      NO SERAN NECESARIOS UNA VEZ QUE SE UTILICE LA API
    */

    return permissions_list;

  }

  //METODO PARA FILTRAR LA LISTA DE PERMISOS
  // Este metodo recibe el array de permisos que se quiere filtrar (PermissionEntity[])
  // y lo filtra segun los parametros: nombre y descripcion
  // El input de busqueda filtra segun Nombre y Descripcion
  public PermissionEntity[] filter_permission_list(PermissionEntity[] permission_list,
   string permission_search_input)
  {

    //Filtrar segun los parametros de texto de busqueda
    // NOMBRE, APELLIDO, CORREO
    if (!string.IsNullOrEmpty(permission_search_input))
    {
      permission_list = permission_list
        .Where(permission =>
        {
          return
          permission.Name.ToLower().Contains(permission_search_input.ToLower()) ||
          permission.Roles.Any(role => role.ToLower().Contains(permission_search_input.ToLower()));
        })
        .ToArray();
    }

    return permission_list;
  }

  //METODO PARA CREAR LA PAGINACION DE PERMISOS
  // Este metodo recibe el array de permisos que se quiere paginar y la cantidad de permisos por pagina
  // Retorna una lista de listas de Permisos (arrayList) donde se encuentran las paginas de permisos
  //segun la cantidad ingresada en los parametros.
  public List<List<PermissionEntity>> create_permissionpages_from_permission_list(PermissionEntity[] permission_list, int permissions_per_page)
  {

    //Lista de paginas de empleados divididas segun la cantidad seleccionada en la vista
    List<List<PermissionEntity>> Permissions_Pages = new List<List<PermissionEntity>>();

    //LOOP PARA DIVIDIR LA LISTA DE EMLEADOS EN PAGINAS DE LA CANTIDAD SELECCIONADA
    for (int i = 0; i < permission_list.Length; i = i + permissions_per_page)
    {
      //PAGINA CORRESPONDIENTE A ITERACION
      List<PermissionEntity> permission_page = new List<PermissionEntity>();

      for (int j = i; j < i + permissions_per_page; j++)
      {
        //SI EL NUMERO DE LA ITERACION NO SOBREPASA LA CANTIDAD TOTAL DE EMPLEADOS, SE AGREGA A LA PAGINA CORRESPONDIENTE
        if (j < permission_list.Length)
        {
          // Se agrega el empleado correspondiente al index en j
          // De esta manera se crean paginas segun la cantidad que deben tener
          permission_page.Add(permission_list[j]);
        }
      }
      //SE AGREGA LA PAGINA CREADA A LA LISTA DE PAGINAS
      Permissions_Pages.Add(permission_page);
    }

    return Permissions_Pages;
  }


  //METODO PARA ORDENAR ALFABETICAMENTE EL ARRAY DE PERMISOS
  // Este metodo recibe un array de Permisos y un string donde se especifica segun que atributo se quiere ordenar
  // Los posibles atributos para odenar son: name, description y creation_date
  // Si no se ingresa ningun parametro se ordena por nombre por default
  public PermissionEntity[] order_permissionlist_by(PermissionEntity[] permission_list, string order_by)
  {

    // se realiza un switch para determinar que tipo de orden se require
    switch (order_by)
    {

      case "name_ascending":
        // Ordenar alfabéticamente ascendentemente segun Nombre, ignorando mayúsculas y minúsculas
        permission_list = permission_list.OrderBy(permission => permission.Name, StringComparer.OrdinalIgnoreCase).ToArray();
        break;

      case "name_descending":
        // Ordenar alfabéticamente descendentemente segun Nombre, ignorando mayúsculas y minúsculas
        permission_list = permission_list.OrderByDescending(permission => permission.Name, StringComparer.OrdinalIgnoreCase).ToArray();
        break;

      case "description_ascending":
        // Ordenar alfabéticamente ascendentemente segun Descripcion, ignorando mayúsculas y minúsculas
        permission_list = permission_list.OrderBy(permission => permission.Description, StringComparer.OrdinalIgnoreCase).ToArray();
        break;

      case "description_descending":
        // Ordenar alfabéticamente segun Descripcion descendentemente, ignorando mayúsculas y minúsculas
        permission_list = permission_list.OrderByDescending(permission => permission.Description, StringComparer.OrdinalIgnoreCase).ToArray();
        break;

      case "creation_date_ascending":
        // Ordenar segun fecha de creacion, de mas antigua a mas reciente
        permission_list = permission_list.OrderBy(permission => permission.CreationDate).ToArray();
        break;

      case "creation_date_descending":
        // Ordenar segun fecha de creacion, de mas reciente a mas antigua
        permission_list = permission_list.OrderByDescending(permission => permission.CreationDate).ToArray();
        break;

      default:
        // Ordenar alfabéticamente segun Nombre, ignorando mayúsculas y minúsculas
        permission_list = permission_list.OrderBy(permission => permission.Name, StringComparer.OrdinalIgnoreCase).ToArray();
        break;
    }

    return permission_list;
  }


  [HttpGet]
  public IActionResult Permission(string permission_search_input = "", string order_by = "name_ascending", int permissions_per_page = 5,
  int page_number = 1)
  {


    //Se llama al metodo para obtener los datos del JSON
    PermissionEntity[] permission_list_from_JSON = Get_dummy_permissions_data_from_JSON();

    //Se llama al metodo para filtrar los permisos segun Nombre y Descripcion
    PermissionEntity[] filtered_permission_list =
    filter_permission_list(permission_list_from_JSON, permission_search_input);


    //Se orderna el array de permisos despues de ser filtrado
    PermissionEntity[] filtered_permission_list_ordered = order_permissionlist_by(filtered_permission_list, order_by);



    //Se llama al metodo que crea la paginacion de la lista de permisos segun los parametros designados
    List<List<PermissionEntity>> Permissions_Pages = create_permissionpages_from_permission_list(filtered_permission_list_ordered, permissions_per_page);

    //Definir la variable que va a contener los permisos de la pagina a mostrar
    PermissionEntity[] selected_permission_page = [];

    //Si el numero de pagina es 0 se asigna a 1 porque la pagina 0 no existe
    if (page_number == 0) page_number = 1;

    //Si el numero de pagina seleccionado es mayor a la cantidad total de paginas, se asigna la ultima pagina, si no se mantiene
    page_number = page_number >= Permissions_Pages.Count ? Permissions_Pages.Count : page_number;


    // SI EXISTEN PAGINAS EN LA LISTA DE PAGINAS, SE ASIGNA LA PAGINA CORRESPONDIENTE
    // SI NO, LA LISTA QUEDA VACIA YA QUE NO SE ENCONTRÓ NINGÚN PERMISO
    if (Permissions_Pages.Count != 0 && page_number != 0)
    {

      //Se asigna la pagina correspondiente al array de permisos que se va a utilizar
      selected_permission_page = Permissions_Pages.ElementAt(
      // Si el numero de pagina que se seleccionó es mayor a la cantidad de paginas disponibles
      page_number > Permissions_Pages.Count
      // Se asigna la primera pagina ya que se excedio la cantidad maxima
      ? 0
      // Si no, se asigna el numero de pagina -1 lo que corresponde al index correcto de la pagina en la lista de paginas
      : page_number - 1)
      .ToArray();
    }




    //USO DE DICCIONARIO VIEWDATA PARA ENVIAR DATOS A LA VISTA

    //Total de paginas
    ViewData["Total_Pages"] = Permissions_Pages.Count;
    //Pagina actual
    ViewData["Current_Page"] = page_number;
    //Empleados por pagina
    ViewData["Permissions_Per_Page"] = permissions_per_page;
    //Columna que dicta orden da datos
    ViewData["Order_By"] = order_by;
    //Filtro de busqueda segun nombre, apellido y correo
    ViewData["Permission_Search_Input"] = permission_search_input;
    ViewBag.NombreUbicacion = GetUltimoNombreUbicacionC() ?? "Ubicación C";
    ViewBag.NombreUbicacionA = GetUltimoNombreUbicacionA() ?? "Ubicación A";
    ViewBag.NombreUbicacionB = GetUltimoNombreUbicacionB() ?? "Ubicación B";



    //RETORNAR A LA VISTA CON EL ARRAY DE PERMISOS FILTRADOS Y ORDERNADOS DE LA PAGINA SELECCIONADA
    return View(selected_permission_page);
  }

  private const int PageSize = 20; // Número de roles por página
  public IActionResult Roles(int page = 1, string searchTerm = "")
  {
   
    // Lee la cadena de conexión desde el archivo app.config, esta cadena se puede modificar.
    string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

    // Accede a los roles desde la base de datos
    var rolesBD = AccessoRoles.Listar();

    //  Lista para almacenar los roles con el TotalUsers actualizado
    List<Rol> roles = new List<Rol>();

    using (SqlConnection connection = new SqlConnection(connectionString))
    {
      connection.Open();

      // Consulta para contar el número de usuarios por rol
      string query = @"
            SELECT 
                ru.idRol, 
                COUNT(u.userSysId) AS TotalUsers
            FROM 
                RolesUsuario ru
            JOIN 
                users u ON ru.idRol = u.idRol
            GROUP BY 
                ru.idRol";


      // Ejecuta la consulta
      SqlCommand cmd = new SqlCommand(query, connection);
      SqlDataReader reader = cmd.ExecuteReader();

      // Diccionario para almacenar el número de usuarios por rol
      Dictionary<Guid, int> rolUsuarios = new Dictionary<Guid, int>();

      while (reader.Read())
      {
        Guid rolId = reader.GetGuid(0);
        int totalUsers = reader.GetInt32(1);
        rolUsuarios[rolId] = totalUsers;
      }

      reader.Close();

      // Se mapean los roles con el total de usuarios desde el diccionario
      roles = rolesBD.Select(r => new Rol
      {
        RoleId = r.IdRol,
        RoleName = r.Nombre,
        Descripcion = r.Descripcion,
        Bloqueado = r.EstaBloqueado,
        TotalUsers = rolUsuarios.ContainsKey(r.IdRol) ? rolUsuarios[r.IdRol] : 0  // Asigna el TotalUsers desde el diccionario
      }).ToList();
    }

    // Filtrar los roles por nombre si se pasa un término de búsqueda
    if (!string.IsNullOrEmpty(searchTerm))
    {
      roles = roles.Where(r => r.RoleName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    var totalRoles = roles.Count;
    var totalPages = (int)Math.Ceiling((double)totalRoles / PageSize); // Calcula el número total de páginas

    // Se usan las bibliotecas skip y take para la paginación
    // Obtiene los roles para la página solicitada
    var rolesOnPage = roles
        .OrderBy(r => r.RoleName) // Ordena los roles
        .Skip((page - 1) * PageSize) // Salta los roles anteriores
        .Take(PageSize) // Toma solo los roles correspondientes a la página actual
        .ToList();

    // Crea un objeto con toda la información que quieres pasar a la vista
    var model = new RolesViewModel
    {
      Roles = rolesOnPage,
      CurrentPage = page,
      TotalPages = totalPages,
      SearchTerm = searchTerm
    };

    ViewData["AlertMensaje"] ??= string.Empty;
    ViewData["AlertTipo"] ??= string.Empty;
    ViewBag.NombreUbicacion = GetUltimoNombreUbicacionC() ?? "Ubicación C";
    ViewBag.NombreUbicacionA = GetUltimoNombreUbicacionA() ?? "Ubicación A";
    ViewBag.NombreUbicacionB = GetUltimoNombreUbicacionB() ?? "Ubicación B";



    return View(model);
  }

  private string GetUltimoNombreUbicacionA()

  {

    string connectionString = System.Configuration.ConfigurationManager

        .ConnectionStrings["ServerDiverscan"].ConnectionString;

    const string sql = @"

     SELECT TOP (1) NOMBRE_UBICACION_A

     FROM dbo.UBICACION_A_NOMBRE

     WHERE NOMBRE_UBICACION_A IS NOT NULL

       AND LTRIM(RTRIM(NOMBRE_UBICACION_A)) <> ''

     ORDER BY ID DESC;";

    try

    {

      using (var conn = new SqlConnection(connectionString))

      using (var cmd = new SqlCommand(sql, conn))

      {

        conn.Open();

        var result = cmd.ExecuteScalar();

        var nombre = result == null || result == DBNull.Value ? null : result.ToString();

        return string.IsNullOrWhiteSpace(nombre) ? null : nombre.Trim();

      }

    }

    catch

    {

      // Si hay error, devolvemos null para que se use el fallback

      return null;

    }

  }



  private string GetUltimoNombreUbicacionB()

  {

    string connectionString = System.Configuration.ConfigurationManager

        .ConnectionStrings["ServerDiverscan"].ConnectionString;

    const string sql = @"

     SELECT TOP (1) NOMBRE_UBICACION_B

     FROM dbo.UBICACION_B_NOMBRE

     WHERE NOMBRE_UBICACION_B IS NOT NULL

       AND LTRIM(RTRIM(NOMBRE_UBICACION_B)) <> ''

     ORDER BY ID DESC;";

    try

    {

      using (var conn = new SqlConnection(connectionString))

      using (var cmd = new SqlCommand(sql, conn))

      {

        conn.Open();

        var result = cmd.ExecuteScalar();

        var nombre = result == null || result == DBNull.Value ? null : result.ToString();

        return string.IsNullOrWhiteSpace(nombre) ? null : nombre.Trim();

      }

    }

    catch

    {

      // Ante cualquier error, devolvemos null para usar el fallback

      return null;

    }

  }

  private string GetUltimoNombreUbicacionC()

  {

    string connectionString = System.Configuration.ConfigurationManager

        .ConnectionStrings["ServerDiverscan"].ConnectionString;

    const string sql = @"

    SELECT TOP (1) NOMBRE_UBICACION_C

    FROM dbo.UBICACION_C_NOMBRE

    WHERE NOMBRE_UBICACION_C IS NOT NULL

      AND LTRIM(RTRIM(NOMBRE_UBICACION_C)) <> ''

    ORDER BY ID DESC;";

    try

    {

      using (var conn = new SqlConnection(connectionString))

      using (var cmd = new SqlCommand(sql, conn))

      {

        conn.Open();

        var result = cmd.ExecuteScalar();

        var nombre = result == null || result == DBNull.Value ? null : result.ToString();

        return string.IsNullOrWhiteSpace(nombre) ? null : nombre.Trim();

      }

    }

    catch

    {

      // Si algo falla, devolvemos null para que aplique el fallback

      return null;

    }

  }

  //Funcion que activa y desactiva un rol individualmente
  public IActionResult ActivarDesactivar(Guid idRol, string nameRol, bool bloqueado)
  {
    // Instancia de AlertMessage para almacenar el resultado
    AlertMessage alertMessage = new AlertMessage();

    try
    {
      // Lógica para activar/desactivar el rol
      AccessoRoles.ActivarDesactivar(idRol.ToString());
      if (bloqueado)
      {
        // Configurar mensaje de éxito
        alertMessage.Tipo = "success";
        alertMessage.Mensaje = "El rol " + nameRol + " se ha activado correctamente.";
      }
      else
      {
        // Configurar mensaje de éxito
        alertMessage.Tipo = "success";
        alertMessage.Mensaje = "El rol " + nameRol + " se ha desactivado correctamente.";
      }
 
    }
    catch (Exception ex)
    {
      // Configurar mensaje de error
      alertMessage.Tipo = "error";
      alertMessage.Mensaje = $"Ocurrió un error al actualizar el rol: {ex.Message}";
    }

    // Pasar el mensaje como parámetro a la vista

    TempData["Alert"] = JsonSerializer.Serialize(alertMessage);
    return RedirectToAction("Roles");


  }
  public class BatchToggleRequest
  {
    public List<string> Roles { get; set; }
    public string Action { get; set; }
  }

  //Funcion que activa y desactiva roles en batch
  [HttpPost]
  public IActionResult ActivarDesactivarBatch([FromBody] BatchToggleRequest request)
  {
    if (request == null || request.Roles == null || request.Roles.Count == 0)
    {
      return Json(new { success = false, message = "No se enviaron roles para actualizar." });
    }

    var results = new List<object>();

    foreach (var roleId in request.Roles)
    {
      // Llamamos al método para activar/desactivar el rol
     ActivarDesactivar(roleId, request.Action);

      // Consultamos el valor de ESTA_BLOQUEADO para el roleId después de la actualización
      bool wasDeactivatedBoolean = GetRoleActivationStatus(roleId);

      var roleName = GetRoleNameById(roleId);

      results.Add(new
      {
        RoleId = roleId,
        RoleName = roleName,
        WasDeactivated = wasDeactivatedBoolean
      });
    }

    return Json(new { success = true, roles = results });
  }

  private static string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

  public static void ActivarDesactivar(string roleId, string action)
  {
    try
    {
      using (SqlConnection connection = new SqlConnection(connectionString))
      {
        connection.Open();

        string query = @"UPDATE RolesUsuario 
                                 SET ESTA_BLOQUEADO = @estado 
                                 WHERE idRol = @idrol";

        using (SqlCommand command = new SqlCommand(query, connection))
        {
          // Determinar el valor de bloqueo según la acción
          int estado = (action == "desactivar") ? 1 : 0;

          // Agregar parámetros
          command.Parameters.Add("@idrol", SqlDbType.VarChar).Value = roleId;
          command.Parameters.Add("@estado", SqlDbType.Bit).Value = estado;

          command.ExecuteNonQuery();
        }
      }
    }
    catch (Exception ex)
    {
      Console.WriteLine("Error al actualizar el estado del rol: " + ex.Message);
      throw;
    }
  }

  // Método para consultar el estado de ESTA_BLOQUEADO en la base de datos
  private bool GetRoleActivationStatus(string roleId)
  {
    string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;
    using (SqlConnection connection = new SqlConnection(connectionString))
    {
      connection.Open();
      using (SqlCommand command = new SqlCommand("SELECT ESTA_BLOQUEADO FROM RolesUsuario WHERE idRol = @idrol", connection))
      {
        command.Parameters.AddWithValue("@idrol", new Guid(roleId));

        var result = command.ExecuteScalar();
        if (result != null)
        {
          bool isDeactivated = Convert.ToBoolean(result); // Convertimos el valor a booleano
          return isDeactivated; // Si es 1, significa que está desactivado (true), si es 0 está activo (false)
        }
        return false; // En caso de error o si no encuentra el rol, retornamos false
      }
    }
  }




  // Método que obtiene el nombre del rol por ID
  public static string GetRoleNameById(string roleId)
  {
    // Lee la cadena de conexión desde el archivo app.config, esta cadena se puede modificar.
    string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;
    // Cadena SQL para obtener el nombre del rol
    string query = "SELECT Nombre FROM RolesUsuario WHERE idRol = @RoleId";

    // Variable para almacenar el nombre del rol
    string roleName = string.Empty;

    // Conexión y comando para ejecutar la consulta
    using (SqlConnection connection = new SqlConnection(connectionString))
    {
      try
      {
        // Abrimos la conexión
        connection.Open();

        // Creamos el comando con la consulta y la conexión
        using (SqlCommand command = new SqlCommand(query, connection))
        {
          // Agregamos el parámetro para el roleId
          command.Parameters.AddWithValue("@RoleId", roleId);

          // Ejecutamos la consulta y leemos el resultado
          object result = command.ExecuteScalar();

          // Si el resultado no es null, asignamos el valor a roleName
          if (result != null)
          {
            roleName = result.ToString();
          }
        }
      }
      catch (Exception ex)
      {
        // Manejo de excepciones
        Console.WriteLine("Error al obtener el nombre del rol: " + ex.Message);
      }
    }

    // Devolvemos el nombre del rol o una cadena vacía si no se encuentra
    return roleName;
  }



  private static List<Rol> _rolesList = new List<Rol>();
  private static int _nextRoleId = 1; // Contador global para los IDs

  [HttpPost]
  public IActionResult SaveRole(Rol model, List<string> PermisosSeleccionados)
  {
    var alertMessage = new AlertMessage();
    try
    {
      // Verificar si los permisos llegaron correctamente
      if (PermisosSeleccionados != null && PermisosSeleccionados.Any())
      {
        // Filtrar los permisos concatenados
        var permisosFiltrados = PermisosSeleccionados.Where(p => !p.Contains(",")).ToList();
        model.Permisos = permisosFiltrados;
      }

      // Validar el modelo
      if (ModelState.IsValid)
      {
        // Crear un nuevo objeto de rol con los datos ingresados
        var rol = new Diverscan.Activos.EL.Roles
        {
          IdRol = Guid.NewGuid(),
          Nombre = model.RoleName,
          Descripcion = model.Descripcion,
          EstaBloqueado = false
        };

        // Llamar al método que interactúa con el procedimiento almacenado
        var registroExitoso = AccessoRoles.Registrar(rol);

        // Validar el resultado del procedimiento
        if (registroExitoso)
        {
          alertMessage.Tipo = "success";
          alertMessage.Mensaje = "Rol agregado exitosamente.";
        }
        else
        {
          alertMessage.Tipo = "error";
          alertMessage.Mensaje = "Ya existe un rol con ese nombre.";
        }
      }
      else
      {
        alertMessage.Tipo = "error";
        // Obtener el primer mensaje de error de validación
        alertMessage.Mensaje = ModelState.Values
            .SelectMany(v => v.Errors)
            .FirstOrDefault()?.ErrorMessage ?? "Datos inválidos. Por favor, verifica los campos.";
      }
    }
    catch (Exception ex)
    {
      alertMessage.Tipo = "error";
      alertMessage.Mensaje = $"Ocurrió un error inesperado: {ex.Message}";
    }

    // Devolver la vista principal con el mensaje
    TempData["Alert"] = JsonSerializer.Serialize(alertMessage);
    return RedirectToAction("Roles");
  }

  private static List<Rol> _rolesListEdit = new List<Rol>();

  [HttpPost]
  public IActionResult UpdateRole(Rol model, List<string> PermisosSeleccionados)
  {
    var alertMessage = new AlertMessage();
    try
    {
      // Verificar si los permisos llegaron correctamente
      if (PermisosSeleccionados != null && PermisosSeleccionados.Any())
      {
        // Filtrar los permisos concatenados
        var permisosFiltrados = PermisosSeleccionados.Where(p => !p.Contains(",")).ToList();
        model.Permisos = permisosFiltrados;
      }

      // Validar el modelo
      if (ModelState.IsValid)
      {
        // Crear un nuevo objeto de rol con los datos ingresados
        var rol = new Diverscan.Activos.EL.Roles
        {
          IdRol = model.RoleId,
          Nombre = model.RoleName,
          Descripcion = model.Descripcion,
          EstaBloqueado = model.Bloqueado
        };

        // Llamar al método que interactúa con el procedimiento almacenado
        var registroExitoso = AccessoRoles.Editar(rol);

        // Validar el resultado del procedimiento
        if (registroExitoso)
        {
          alertMessage.Tipo = "success";
          alertMessage.Mensaje = "Rol editado exitosamente.";

          AccessoRoles.EliminarRelación(model.RoleId);
        }
        else
        {
          alertMessage.Tipo = "error";
          alertMessage.Mensaje = "Ya existe un rol con ese nombre.";
        }
      }
      else
      {
        alertMessage.Tipo = "error";
        // Obtener el primer mensaje de error de validación
        alertMessage.Mensaje = ModelState.Values
            .SelectMany(v => v.Errors)
            .FirstOrDefault()?.ErrorMessage ?? "Datos inválidos. Por favor, verifica los campos.";
      }
    }
    catch (Exception ex)
    {
      alertMessage.Tipo = "error";
      alertMessage.Mensaje = $"Ocurrió un error inesperado: {ex.Message}";
    }

    // Devolver la vista principal con el mensaje
    TempData["Alert"] = JsonSerializer.Serialize(alertMessage);
    return RedirectToAction("Roles");
  }

}
