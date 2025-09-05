using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AspnetCoreMvcFull.Models;
using AspnetCoreMvcFull.Models.Roles;
using AspnetCoreMvcFull.Models.Users;
using Diverscan.Activos.DL;
using Diverscan.Activos.EL;
using static AspnetCoreMvcFull.Controllers.AccessController;
using System.Data.SqlClient;
using System.Data;


namespace AspnetCoreMvcFull.Controllers;


public class UsersController : Controller
{


  //METODO PARA FILTRAR LA LISTA DE USUARIOS
  // Este metodo recibe el array de usuarios que se quiere filtrar (User[])
  // y lo filtra segun los parametros: estado, input de busqueda, input de busqueda de ID
  // El estado puede ser activo inactivo
  // El input de busqueda filtra segun Nombre, Rol y Correo
  // El input de busqueda de ID filtra segun el numero de ID
  public UserModel[] filter_users_list(UserModel[] users_list, string user_state, string user_search_input)
  {
    //FILTRADO DE USUARIOS
    // Filtrar segun el dropdown ACTIVO / INACTIVO
    if (!String.IsNullOrEmpty(user_state) && user_state == "activo")
    {
      users_list = users_list
        .Where(user =>
        {
          return user.isActive == true;
        })
        .ToArray();
    }
    else if (!String.IsNullOrEmpty(user_state) && user_state == "inactivo")
    {
      users_list = users_list
        .Where(user =>
        {
          return user.isActive == false;
        })
        .ToArray();
    }

    //Filtrar segun los parametros de texto de busqueda
    // NOMBRE, APELLIDO, CORREO y ROL
    if (!string.IsNullOrEmpty(user_search_input))
    {

      users_list = users_list
        .Where(user =>
        {
          return
          user.Username.ToLower().Contains(user_search_input.ToLower()) ||
          user.Email.ToLower().Contains(user_search_input.ToLower()) ||
          user.RolName.ToLower().Contains(user_search_input.ToLower());
        })
        .ToArray();
    }


    return users_list;
  }



  //METODO PARA CREAR LA PAGINACION DE USUARIOS
  // Este metodo recibe el array de usuarios que se quiere paginar y la cantidad de usuarios por pagina
  // Retorna una lista de listas de usuarios (arrayList) donde se encuentran las paginas de usuarios
  //segun la cantidad ingresada en los parametros.
  public List<List<UserModel>> create_userspages_from_users_list(UserModel[] users_list, int users_per_page)
  {

    //Lista de paginas de usuarios divididas segun la cantidad seleccionada en la vista
    List<List<UserModel>> Users_Pages = new List<List<UserModel>>();

    //LOOP PARA DIVIDIR LA LISTA DE USUARIOS EN PAGINAS DE LA CANTIDAD SELECCIONADA
    for (int i = 0; i < users_list.Length; i = i + users_per_page)
    {
      //PAGINA CORRESPONDIENTE A ITERACION
      List<UserModel> users_page = new List<UserModel>();

      for (int j = i; j < i + users_per_page; j++)
      {
        //SI EL NUMERO DE LA ITERACION NO SOBREPASA LA CANTIDAD TOTAL DE USUARIOS, SE AGREGA A LA PAGINA CORRESPONDIENTE
        if (j < users_list.Length)
        {
          // Se agrega el usuarios correspondiente al index en j
          // De esta manera se crean paginas segun la cantidad que deben tener
          users_page.Add(users_list[j]);
        }
      }
      //SE AGREGA LA PAGINA CREADA A LA LISTA DE PAGINAS
      Users_Pages.Add(users_page);
    }

    return Users_Pages;
  }


  //METODO PARA ORDENAR ALFABETICAMENTE EL ARRAY DE USUARIOS
  // Este metodo recibe un array de Usuarios y un string donde se especifica segun que atributo se quiere ordenar
  // Los posibles atributos para odenar son: name, lastname, email, creation_date y role
  // Si no se ingresa ningun parametro se ordena por nombre por default
  public UserModel[] order_userslist_by(UserModel[] users_list, string order_by)
  {

    // se realiza un switch para determinar que tipo de orden se require
    switch (order_by)
    {

      case "name_ascending":
        // Ordenar alfabéticamente ascendentemente segun Nombre, ignorando mayúsculas y minúsculas
        users_list = users_list.OrderBy(user => user.Username, StringComparer.OrdinalIgnoreCase).ToArray();
        break;

      case "name_descending":
        // Ordenar alfabéticamente descendentemente segun Nombre, ignorando mayúsculas y minúsculas
        users_list = users_list.OrderByDescending(user => user.Username, StringComparer.OrdinalIgnoreCase).ToArray();
        break;

      case "email_ascending":
        // Ordenar alfabéticamente ascendentemente segun Email, ignorando mayúsculas y minúsculas
        users_list = users_list.OrderBy(user => user.Email, StringComparer.OrdinalIgnoreCase).ToArray();
        break;

      case "email_descending":
        // Ordenar alfabéticamente segun Email descendentemente, ignorando mayúsculas y minúsculas
        users_list = users_list.OrderByDescending(user => user.Email, StringComparer.OrdinalIgnoreCase).ToArray();
        break;

      case "role_ascending":
        // Ordenar alfabéticamente ascendentemente segun Rol, ignorando mayúsculas y minúsculas
        users_list = users_list.OrderBy(user => user.RolName, StringComparer.OrdinalIgnoreCase).ToArray();
        break;

      case "role_descending":
        // Ordenar alfabéticamente segun Rol descendentemente, ignorando mayúsculas y minúsculas
        users_list = users_list.OrderByDescending(user => user.RolName, StringComparer.OrdinalIgnoreCase).ToArray();
        break;

      case "creation_date_ascending":
        // Ordenar segun fecha de creacion, de mas antigua a mas reciente
        users_list = users_list.OrderBy(user => user.CreationDate).ToArray();
        break;

      case "creation_date_descending":
        // Ordenar segun fecha de creacion, de mas reciente a mas antigua
        users_list = users_list.OrderByDescending(user => user.CreationDate).ToArray();
        break;

      default:
        // Ordenar alfabéticamente segun Nombre, ignorando mayúsculas y minúsculas
        users_list = users_list.OrderBy(user => user.Username, StringComparer.OrdinalIgnoreCase).ToArray();
        break;
    }

    return users_list;
  }



  //Clase auxiliar para enviar lista de usuarios y roles a la vista
  public class UsuariosRolesViewModel
  {
    public UserModel[] Usuarios = [];
    public Roles[] Roles = [];
  }






  //Listado de usuarios
  [HttpGet]
  public IActionResult List(string user_state = "activo/inactivo", string user_search_input = "",
   string order_by = "name_ascending", int users_per_page = 5, int page_number = 1)
  {

    // se obtienen los usuarios del backend
    UserModel[] usersFromBackend = UserService.GetAllUsersFromBackend();

    //Se llama al metodo para filtrar los usuarios segun Estado, Nombre, Apellido, Rol y Correo
    UserModel[] filtered_users_list =
    filter_users_list(usersFromBackend, user_state, user_search_input);


    //Se orderna el array de usuarios despues de ser filtrado
    UserModel[] filtered_users_list_ordered = order_userslist_by(filtered_users_list, order_by);



    //Se llama al metodo que crea la paginacion de la lista de usuarios segun los parametros designados
    List<List<UserModel>> Users_Pages = create_userspages_from_users_list(filtered_users_list_ordered, users_per_page);

    //Definir la variable que va a contener los usuarios de la pagina a mostrar
    UserModel[] selected_users_page = [];

    //Si el numero de pagina es 0 se asigna a 1 porque la pagina 0 no existe
    if (page_number == 0) page_number = 1;

    //Si el numero de pagina seleccionado es mayor a la cantidad total de paginas, se asigna la ultima pagina, sino se mantiene
    page_number = page_number >= Users_Pages.Count ? Users_Pages.Count : page_number;


    // SI EXISTEN PAGINAS EN LA LISTA DE PAGINAS, SE ASIGNA LA PAGINA CORRESPONDIENTE
    // SI NO, LA LISTA QUEDA VACIA YA QUE NO SE ENCONTRÓ NINGÚN USUARIO
    if (Users_Pages.Count != 0 && page_number != 0)
    {

      //Se asigna la pagina correspondiente al array de usuarios que se va a utilizar
      selected_users_page = Users_Pages.ElementAt(
      // Si el numero de pagina que se seleccionó es mayor a la cantidad de paginas disponibles
      page_number > Users_Pages.Count
      // Se asigna la primera pagina ya que se excedio la cantidad maxima
      ? 0
      // Si no, se asigna el numero de pagina -1 lo que corresponde al index correcto de la pagina en la lista de paginas
      : page_number - 1)
      .ToArray();
    }




    //USO DE DICCIONARIO VIEWDATA PARA ENVIAR DATOS A LA VISTA

    //Total de paginas
    ViewData["Total_Pages"] = Users_Pages.Count;
    //Pagina actual
    ViewData["Current_Page"] = page_number;
    //Usuarios por pagina
    ViewData["Users_Per_Page"] = users_per_page;
    //Columna que dicta orden da datos
    ViewData["Order_By"] = order_by;
    //Filtro de estado de usuarios
    ViewData["User_State"] = user_state;
    //Filtro de busqueda segun nombre, apellido, correo y rol
    ViewData["User_Search_Input"] = user_search_input;

    ViewBag.NombreUbicacion = GetUltimoNombreUbicacionC() ?? "Ubicación C";
    ViewBag.NombreUbicacionA = GetUltimoNombreUbicacionA() ?? "Ubicación A";
    ViewBag.NombreUbicacionB = GetUltimoNombreUbicacionB() ?? "Ubicación B";
    UsuariosRolesViewModel usuariosRolesArrays = new UsuariosRolesViewModel
    {
      Usuarios = selected_users_page,
      Roles = RoleService.GetAllRoles()
    };




    //RETORNAR A LA VISTA CON EL ARRAY DE USUARIOS FILTRADO
    return View(usuariosRolesArrays);
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

  //METODO PARA DESACTIVAR USUARIO SELECCIONADO
  [HttpPost]
  public IActionResult DeactivateActivateUser(Guid? userIdToActivateDeactivate, string isUserToActivateDeactivateApproved)
  {
    // 0) Validaciones básicas
    if (userIdToActivateDeactivate == null)
    {
      Console.WriteLine("No se recibió un id de usuario.");
      return RedirectToAction("List");
    }

    // Normalizar estado actual (enviado desde la vista)
    bool currentIsApproved = false;
    if (!string.IsNullOrWhiteSpace(isUserToActivateDeactivateApproved))
    {
      // Acepta "true"/"false", "on"/"off", "1"/"0"
      bool.TryParse(isUserToActivateDeactivateApproved, out currentIsApproved);
      if (!currentIsApproved && (isUserToActivateDeactivateApproved == "1" ||
                                 isUserToActivateDeactivateApproved.Equals("on", StringComparison.OrdinalIgnoreCase)))
      {
        currentIsApproved = true;
      }
    }

    // Regla: máx 5 activos
    const int cap = 5;
    string cs = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

    // Intención:
    // - Si viene currentIsApproved == false  -> quieren ACTIVAR (pasar a true)
    // - Si viene currentIsApproved == true   -> quieren DESACTIVAR (pasar a false)
    bool intendedNewState = !currentIsApproved;

    if (intendedNewState) // quieren ACTIVAR
    {
      int otherApprovedCount = 0;

      using (var conn = new SqlConnection(cs))
      using (var cmd = new SqlCommand(
          @"SELECT COUNT(*) 
              FROM dbo.users 
              WHERE ISNULL(isApproved,0)=1 AND userSysId <> @id", conn))
      {
        cmd.Parameters.AddWithValue("@id", userIdToActivateDeactivate.Value);
        conn.Open();
        otherApprovedCount = (int)cmd.ExecuteScalar();
      }

      if (otherApprovedCount >= cap)
      {
        // No se puede activar por límite: mantener inactivo y avisar
        UserService.ActivateDeactivateUserByUserID(
            userIdToActivateDeactivate.ToString(),
            /* set to */ false
        );

        TempData["Alert"] = System.Text.Json.JsonSerializer.Serialize(new
        {
          Tipo = "info",
          Mensaje = $"Con tu plan actual solo puedes tener {cap} usuarios activos. " +
                      $"Este usuario se mantuvo como <strong>inactivo</strong>."
        });

        // Redirección conservando filtros
        string returnUrl1 = Request.Headers["Referer"].ToString();
        if (string.IsNullOrEmpty(returnUrl1)) return RedirectToAction("List");
        return Redirect(returnUrl1);
      }

      // Hay cupo: activar
      UserService.ActivateDeactivateUserByUserID(
          userIdToActivateDeactivate.ToString(),
          /* set to */ true
      );
    }
    else
    {
      // Quieren DESACTIVAR: siempre permitido
      UserService.ActivateDeactivateUserByUserID(
          userIdToActivateDeactivate.ToString(),
          /* set to */ false
      );
    }

    // Redirección conservando filtros
    string returnUrl = Request.Headers["Referer"].ToString();
    if (string.IsNullOrEmpty(returnUrl)) return RedirectToAction("List");
    return Redirect(returnUrl);
  }





  [HttpPost]
  public IActionResult ActivarDesactivarBatch([FromBody] BatchToggleRequest request)
  {
    if (request == null || request.Roles == null || request.Roles.Count == 0)
      return Json(new { success = false, message = "No se enviaron roles para actualizar." });

    const int CAP = 5;
    var results = new List<object>();

    foreach (var userIdStr in request.Roles)
    {
      var res = ActivarDesactivarConTope(userIdStr, request.Action, CAP);

      // (Opcional) Si ya tienes estos helpers, se mantienen
      var wasDeactivatedBoolean = GetRoleActivationStatus(userIdStr); // Estado final en BD
      var roleName = GetRoleNameById(userIdStr);

      results.Add(new
      {
        RoleId = userIdStr,
        RoleName = roleName,
        Applied = res.Applied,                // true si se aplicó el cambio, false si se bloqueó
        FinalState = res.FinalState,          // estado final isApproved en BD
        Message = res.Message,                // razón si fue bloqueado
        WasDeactivated = wasDeactivatedBoolean
      });
    }

    return Json(new { success = true, roles = results });
  }

  private static readonly string connectionString =
      System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

  /// <summary>
  /// Aplica "activar"/"desactivar" respetando un tope de activos (cap).
  /// Si action == "activar" y ya se alcanzó el tope, NO actualiza y devuelve Applied=false.
  /// </summary>
  public static (bool Applied, bool FinalState, string Message) ActivarDesactivarConTope(string userIdStr, string action, int cap)
  {
    if (string.IsNullOrWhiteSpace(userIdStr))
      return (false, false, "Id de usuario vacío.");

    // Normalizar acción
    bool setApprovedRequested = !string.Equals(action, "desactivar", StringComparison.OrdinalIgnoreCase);

    Guid userId;
    if (!Guid.TryParse(userIdStr, out userId))
      return (false, false, "Id de usuario inválido.");

    try
    {
      using (var conn = new SqlConnection(connectionString))
      {
        conn.Open();

        // 1) Estado actual del usuario
        bool currentApproved;
        using (var cmd = new SqlCommand(
            "SELECT ISNULL(isApproved,0) FROM dbo.users WHERE userSysId=@id", conn))
        {
          cmd.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = userId;
          var r = cmd.ExecuteScalar();
          currentApproved = (r != null && r != DBNull.Value) && Convert.ToBoolean(r);
        }

        // 2) Si se va a activar y estaba inactivo, validar tope
        if (setApprovedRequested && !currentApproved)
        {
          int activeCount;
          using (var cmd = new SqlCommand(
              "SELECT COUNT(*) FROM dbo.users WHERE ISNULL(isApproved,0)=1", conn))
          {
            activeCount = (int)cmd.ExecuteScalar();
          }

          if (activeCount >= cap)
          {
            // Bloqueado por tope: no aplicar
            return (false, currentApproved,
                $"Con tu plan actual solo puedes tener {cap} usuarios activos. Este usuario se mantuvo inactivo.");
          }
        }

        // 3) Aplicar (activar o desactivar). Si ya está en ese estado, igual “aplica” pero no cambia nada.
        using (var cmd = new SqlCommand(
            "UPDATE dbo.users SET isApproved=@state WHERE userSysId=@id", conn))
        {
          cmd.Parameters.Add("@state", SqlDbType.Bit).Value = setApprovedRequested;
          cmd.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = userId;
          cmd.ExecuteNonQuery();
        }

        return (true, setApprovedRequested, "");
      }
    }
    catch (Exception ex)
    {
      return (false, false, "Error al actualizar el estado del usuario: " + ex.Message);
    }
  }


  // Método para consultar el estado de ESTA_BLOQUEADO en la base de datos
  private bool GetRoleActivationStatus(string roleId)
  {
    string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;
    using (SqlConnection connection = new SqlConnection(connectionString))
    {
      connection.Open();
      using (SqlCommand command = new SqlCommand("SELECT isApproved FROM users WHERE userSysId = @idrol", connection))
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
    string query = "SELECT username FROM users WHERE userSysId = @RoleId";

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






































  //METODO PARA DESACTIVAR MULTIPLES USUARIOS
  [HttpPost]
  public IActionResult DeactivateActivateMultipleUsers(
     string users_ids_to_deactivate = "",
     string users_states_to_deactivate = "")
  {
    if (string.IsNullOrEmpty(users_ids_to_deactivate) ||
        string.IsNullOrEmpty(users_states_to_deactivate))
    {
      Console.WriteLine("No se recibió ningun id o ningun estado de usuario.");
      return RedirectToAction("List");
    }

    // 1) Parseo
    string[] idStrings = users_ids_to_deactivate.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    string[] stateStrings = users_states_to_deactivate.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    if (idStrings.Length != stateStrings.Length)
    {
      Console.WriteLine("Las longitudes de los arrays UsersId y UsersStates no coinciden.");
      return RedirectToAction("List");
    }

    Guid[] userIds = new Guid[idStrings.Length];
    bool[] desiredStates = new bool[stateStrings.Length];

    for (int i = 0; i < idStrings.Length; i++)
    {
      userIds[i] = Guid.Parse(idStrings[i]);
      desiredStates[i] = TryParseBoolFlexible(stateStrings[i]);

      // Si lo que recibes es el ESTADO ACTUAL y quieres togglear:
      // desiredStates[i] = !TryParseBoolFlexible(stateStrings[i]);
    }

    // 2) Verificación de tope (cap) y cálculo de estados finales
    const int cap = 5;
    string cs = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

    // Traer conteo actual de activos
    int activeCount;
    using (var conn = new SqlConnection(cs))
    using (var cmd = new SqlCommand("SELECT COUNT(*) FROM dbo.users WHERE ISNULL(isApproved,0)=1", conn))
    {
      conn.Open();
      activeCount = (int)cmd.ExecuteScalar();
    }

    // Para decidir caso por caso, necesitamos saber el estado actual de cada usuario
    // (si hay pocos seleccionados, un query por usuario está OK)
    bool[] currentStates = new bool[userIds.Length];
    using (var conn = new SqlConnection(cs))
    {
      conn.Open();
      for (int i = 0; i < userIds.Length; i++)
      {
        using var cmd = new SqlCommand("SELECT ISNULL(isApproved,0) FROM dbo.users WHERE userSysId=@id", conn);
        cmd.Parameters.AddWithValue("@id", userIds[i]);
        var r = cmd.ExecuteScalar();
        currentStates[i] = (r != null && r != DBNull.Value) && Convert.ToBoolean(r);
      }
    }

    bool[] finalStates = new bool[userIds.Length];
    var blockedIds = new List<Guid>();

    for (int i = 0; i < userIds.Length; i++)
    {
      bool current = currentStates[i];
      bool desired = desiredStates[i];

      if (desired && !current)
      {
        // Activación
        if (activeCount < cap)
        {
          finalStates[i] = true;
          activeCount++; // ocupamos un cupo
        }
        else
        {
          finalStates[i] = false; // no hay cupo, queda inactivo
          blockedIds.Add(userIds[i]);
        }
      }
      else if (!desired && current)
      {
        // Desactivación
        finalStates[i] = false;
        activeCount = Math.Max(0, activeCount - 1); // liberamos cupo
      }
      else
      {
        // Sin cambio real
        finalStates[i] = current;
      }
    }

    // 3) Aplicar cambios
    // Tu servicio ya usa ActivateDeactivateUserByUserID (arreglado) así que sirve:
    UserService.ActivateDeactivateMultipleUsersByUserID(
        userIds.Select(g => g.ToString()).ToArray(),
        finalStates
    );

    // 4) Mensaje si hubo bloqueados
    if (blockedIds.Count > 0)
    {
      // (Opcional) obtener usernames de los bloqueados
      List<string> blockedNames = new();
      using (var conn = new SqlConnection(cs))
      {
        conn.Open();
        foreach (var gid in blockedIds)
        {
          using var cmd = new SqlCommand("SELECT username FROM dbo.users WHERE userSysId=@id", conn);
          cmd.Parameters.AddWithValue("@id", gid);
          var name = cmd.ExecuteScalar() as string;
          if (!string.IsNullOrEmpty(name)) blockedNames.Add(name);
        }
      }

      string list = blockedNames.Count > 0
          ? $" ({string.Join(", ", blockedNames)})"
          : "";

      TempData["Alert"] = System.Text.Json.JsonSerializer.Serialize(new
      {
        Tipo = "info",
        Mensaje = $"Con tu plan actual solo puedes tener {cap} usuarios activos. " +
                    $"Se mantuvieron inactivos {blockedIds.Count}{list}."
      });
    }

    // 5) Redirección
    string returnUrl = Request.Headers["Referer"].ToString();
    if (string.IsNullOrEmpty(returnUrl)) return RedirectToAction("List");
    return Redirect(returnUrl);
  }

  // Helper para parsear "true/false", "1/0", "on/off"
  private static bool TryParseBoolFlexible(string s)
  {
    if (bool.TryParse(s, out var b)) return b;
    if (s == "1") return true;
    if (s == "0") return false;
    if (s.Equals("on", StringComparison.OrdinalIgnoreCase)) return true;
    if (s.Equals("off", StringComparison.OrdinalIgnoreCase)) return false;
    return false;
  }






  [HttpPost]
  public IActionResult AddUser(string Username, string UserEmail, string UserPassword,
     string UserConfirmPassword, string idRol, string ActivateUser)
  {
    // Validaciones básicas
    if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(UserEmail) ||
        string.IsNullOrEmpty(UserPassword) || string.IsNullOrEmpty(UserConfirmPassword) ||
        string.IsNullOrEmpty(idRol))
    {
      Console.WriteLine("No se recibieron datos validos para registrar usuario.");
      return RedirectToAction("List");
    }

    if (UserPassword != UserConfirmPassword)
    {
      Console.WriteLine("Las contraseñas no coinciden.");
      return RedirectToAction("List");
    }

    // Estado solicitado por el usuario (checkbox)
    bool isApproved = string.Equals(ActivateUser, "on", StringComparison.OrdinalIgnoreCase);

    // 🔎 Verificar límite de usuarios activos (isApproved = 1)
    const int cap = 5;
    string connectionString = System.Configuration.ConfigurationManager
        .ConnectionStrings["ServerDiverscan"].ConnectionString;

    if (isApproved)
    {
      int currentApproved = 0;
      using (var conn = new SqlConnection(connectionString))
      using (var cmd = new SqlCommand(
          "SELECT COUNT(*) FROM dbo.users WHERE ISNULL(isApproved, 0) = 1", conn))
      {
        conn.Open();
        currentApproved = (int)cmd.ExecuteScalar();
      }

      if (currentApproved >= cap)
      {
        // ✅ Forzar a inactivo si ya alcanza el límite
        isApproved = false;

        // (Opcional) Aviso al usuario
        TempData["Alert"] = System.Text.Json.JsonSerializer.Serialize(new
        {
          Tipo = "info",
          Mensaje = $"Con tu plan actual solo puedes tener {cap} usuarios activos. " +
                      $"El nuevo usuario se creó como inactivo."
        });
      }
    }

    // Registrar usuario con el estado corregido
    Guid rolGuid = Guid.Parse(idRol);
    UserService.RegisterUser(Username, UserPassword, UserEmail, isApproved, rolGuid);

    // Redirección conservando filtros previos
    string returnUrl = Request.Headers["Referer"].ToString();
    if (string.IsNullOrEmpty(returnUrl))
      return RedirectToAction("List");

    return Redirect(returnUrl);
  }







  [HttpPost]
  public IActionResult EditUser(string editUserId, string editUsername, string editUserEmail, string editIdRol, string editIsActive)
  {
    if (string.IsNullOrEmpty(editUserId) || string.IsNullOrEmpty(editUsername) ||
        string.IsNullOrEmpty(editUserEmail) || string.IsNullOrEmpty(editIdRol))
    {
      Console.WriteLine("No se recibieron datos validos para editar usuario.");
      return RedirectToAction("List");
    }

    // 1) Estado solicitado desde el checkbox
    bool isApprovedRequested = string.Equals(editIsActive, "on", StringComparison.OrdinalIgnoreCase);

    // 2) Datos para verificación en BD
    const int cap = 5;
    string connectionString = System.Configuration.ConfigurationManager
        .ConnectionStrings["ServerDiverscan"].ConnectionString;

    // 3) Consultar estado actual del usuario y cantidad de activos (excluyendo al propio usuario)
    bool currentIsApproved = false;
    int otherApprovedCount = 0;
    Guid userId = Guid.Parse(editUserId);

    using (var conn = new SqlConnection(connectionString))
    {
      conn.Open();

      // Estado actual del usuario
      using (var cmd = new SqlCommand(
          "SELECT ISNULL(isApproved, 0) FROM dbo.users WHERE userSysId = @id", conn))
      {
        cmd.Parameters.AddWithValue("@id", userId);
        object result = cmd.ExecuteScalar();
        currentIsApproved = (result != null && result != DBNull.Value) && Convert.ToBoolean(result);
      }

      // Activos distintos de este usuario
      using (var cmd = new SqlCommand(
          "SELECT COUNT(*) FROM dbo.users WHERE ISNULL(isApproved, 0) = 1 AND userSysId <> @id", conn))
      {
        cmd.Parameters.AddWithValue("@id", userId);
        otherApprovedCount = (int)cmd.ExecuteScalar();
      }
    }

    // 4) Decidir estado final a guardar
    bool isApprovedToSave = isApprovedRequested;

    if (isApprovedRequested)
    {
      if (!currentIsApproved)
      {
        // Quieren pasar de INACTIVO -> ACTIVO
        if (otherApprovedCount >= cap)
        {
          // Se alcanzó el tope: forzar a inactivo y avisar
          isApprovedToSave = false;

          TempData["Alert"] = System.Text.Json.JsonSerializer.Serialize(new
          {
            Tipo = "info",
            Mensaje = $"Con tu plan actual solo puedes tener {cap} usuarios activos. " +
                        $"Se guardaron los cambios, pero este usuario quedó como <strong>inactivo</strong>."
          });
        }
      }
      // Si ya estaba activo y lo dejan activo: permitido (no cambia el conteo)
    }
    // Si lo ponen inactivo, siempre permitido.

    // 5) Actualizar usuario
    UserService.UpdateUser(editUserId, editUsername, editUserEmail, editIdRol, isApprovedToSave);

    // 6) Redirigir a la URL previa (para conservar filtros)
    string returnUrl = Request.Headers["Referer"].ToString();
    if (string.IsNullOrEmpty(returnUrl)) return RedirectToAction("List");
    return Redirect(returnUrl);
  }



  public IActionResult ViewAccount() => View();
  public IActionResult ViewBilling() => View();
  public IActionResult ViewConnections() => View();
  public IActionResult ViewNotifications() => View();
  public IActionResult ViewSecurity() => View();
}
