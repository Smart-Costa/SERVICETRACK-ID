using System.Data.SqlClient;
using AspnetCoreMvcFull.Models.Users;
using Diverscan.Activos.DL;
using Diverscan.Activos.EL;

class UserService
{

  //Obtener usuarios
  public static UserModel[] GetAllUsersFromBackend()
  {
    //Obtener usuarios desde el backend de ActiveID
    //El metodo AccessUsers.GetAllUsers recibe un booleano que tiene la funcion de:
    // true: solo usuarios activos
    // false: solo usuarios inactivos
    // en este caso se traen ambos
    User[] ActiveUsersFromBackend = AccessUsers.GetAllUsers(true) ?? Array.Empty<User>();
    User[] InactiveUsersFromBackend = AccessUsers.GetAllUsers(false) ?? Array.Empty<User>();

    //Crear lista de Usuarios con el UserModel
    List<UserModel> usersList = [];

    // AQUI SE HACE LOOP EN AMBOS ARRAYS PARA JUNTAR USUARIOS ACTIVOS E INACTIVOS
    //Hacer la transformacion de Usuarios de ActiveID a UserModel
    foreach (User user in ActiveUsersFromBackend)
    {
      string? rolName = RoleService.GetRolNameByGuid(user.Rol);
      UserModel newUser = new UserModel();
      newUser.Username = user.Username;
      newUser.Email = user.Email;
      newUser.CreationDate = user.CreationDate;
      newUser.Password = user.Password;
      newUser.RolName = rolName != null ? rolName : "";
      newUser.RolId = user.Rol;
      newUser.UserSysId = user.UserSysId;
      newUser.isActive = user.IsApproved;

      usersList.Add(newUser);
    }

    //Hacer la transformacion de Usuarios de ActiveID a UserModel
    foreach (User user in InactiveUsersFromBackend)
    {
      string? rolName = RoleService.GetRolNameByGuid(user.Rol);
      UserModel newUser = new UserModel();
      newUser.Username = user.Username;
      newUser.Email = user.Email;
      newUser.CreationDate = user.CreationDate;
      newUser.Password = user.Password;
      newUser.RolName = rolName != null ? rolName : "";
      newUser.RolId = user.Rol;
      newUser.UserSysId = user.UserSysId;
      newUser.isActive = user.IsApproved;

      usersList.Add(newUser);
    }


    //Retornar lista de usuarios transformada a UserModel
    return usersList.ToArray();
  }

  //Desactivar usuarios
  public static void DeactivateUserByUserID(Guid? UserId)
  {
    try
    {
      // se transforma el id de usuario de Guid a string
      string userid = UserId?.ToString() ?? "";
      //si el string es vacio significa que el Guid que se recibió no fue válido
      if (userid == "")
      {
        Console.WriteLine("Error: UserID no valido");
        return;
      }
      // se crea el comando sql donde se va a desactivar al usuario
      SqlCommand deactivateUserCommand = AccessDb.CreateCommand();
      deactivateUserCommand.CommandType = System.Data.CommandType.Text;
      deactivateUserCommand.CommandText
      = $"""
      UPDATE users
      SET isApproved = 0
      WHERE userSysId = '{userid}'
      """;
      // se ejecuta el comando utilizando el metodo de ActiveID
      AccessDb.ExecuteCommand(deactivateUserCommand);
    }
    catch (Exception e)
    {
      Console.WriteLine(e.Message);
    }

  }


  //Activar usuarios
  public static void ActivateUserByUserID(Guid? UserId)
  {
    try
    {
      // se transforma el id de usuario de Guid a string
      string userid = UserId?.ToString() ?? "";
      //si el string es vacio significa que el Guid que se recibió no fue válido
      if (userid == "")
      {
        Console.WriteLine("Error: UserID no valido");
        return;
      }
      // se crea el comando sql donde se va a activar al usuario
      SqlCommand deactivateUserCommand = AccessDb.CreateCommand();
      deactivateUserCommand.CommandType = System.Data.CommandType.Text;
      deactivateUserCommand.CommandText
      = $"""
      UPDATE users
      SET isApproved = 1
      WHERE userSysId = '{userid}'
      """;
      // se ejecuta el comando utilizando el metodo de ActiveID
      AccessDb.ExecuteCommand(deactivateUserCommand);
    }
    catch (Exception e)
    {
      Console.WriteLine(e.Message);
    }

  }


  //Activar/Desactivar usuarios
  public static void ActivateDeactivateUserByUserID(string? userId, bool setApproved)
  {
    try
    {
      if (string.IsNullOrWhiteSpace(userId))
      {
        Console.WriteLine("Error: UserID no válido");
        return;
      }

      // 1 = activo, 0 = inactivo (sin invertir)
      // si setApproved es true => 1, si es false => 0
      using (var cmd = AccessDb.CreateCommand())
      {
        cmd.CommandType = System.Data.CommandType.Text;
        cmd.CommandText = @"
UPDATE dbo.users
SET isApproved = @isApproved
WHERE userSysId = @userId";

        cmd.Parameters.Add("@isApproved", System.Data.SqlDbType.Bit).Value = setApproved; // bool OK para bit
        cmd.Parameters.Add("@userId", System.Data.SqlDbType.UniqueIdentifier).Value = Guid.Parse(userId);

        AccessDb.ExecuteCommand(cmd); // asumo que ejecuta y cierra conexión
      }
    }
    catch (Exception e)
    {
      Console.WriteLine(e.Message);
    }
  }


  //Activar/Desactivar multiples usuarios mediante userId
  public static void ActivateDeactivateMultipleUsersByUserID(string[] UserIds, bool[] isUserActiveArray)
  {
    try
    {
      // si las longitudes de los arrays no coinciden significa que los datos recibidos pueden ser erroneos
      if (UserIds.Length != isUserActiveArray.Length)
      {
        Console.WriteLine("Las longitudes de los arrays UsersId y UsersStates no coinciden.");
      }

      //se hace loop en los dos arrays que deben tener la mimsa longitud
      for (int i = 0; i < UserIds.Length; i++)
      {
        //se llama al metodo de desactivar usuario segun su id y estado
        ActivateDeactivateUserByUserID(UserIds[i], isUserActiveArray[i]);
      }
    }
    catch (Exception e)
    {
      Console.WriteLine(e.Message);
    }

  }


  //Crear nuevo usuario
  public static void RegisterUser(string username, string password, string email, bool isApproved, Guid rol)
  {

    if (String.IsNullOrEmpty(username) || String.IsNullOrEmpty(password) || String.IsNullOrEmpty(email))
    {
      Console.WriteLine("No se recibieron datos validos en UserService para registrar un usuario.");
      return;
    }

    //valores no necesarios para el registro básico
    string passwordQuestion = "";
    string passwordAnswer = "";
    string comments = "";
    string applicationName = "WebAssetCtrl"; //nombre de la aplicacion
    Guid userSysId = Guid.NewGuid(); // generar id unico de usuario
    Guid idem = Guid.NewGuid(); //generar id unico
    //llamar al metodo para agregar usuario a backend ActiveID
    AccessUsers.CreateUser(username, password, email, passwordQuestion, passwordAnswer,
    isApproved, comments, applicationName, rol, userSysId, idem);

  }


  //Actualizar usuario segun Id
  public static void UpdateUser(string userId, string username, string email, string idRol, bool isActive)
  {
    //error en caso de no recibir datos validos para editar usuario
    if (String.IsNullOrEmpty(userId) || String.IsNullOrEmpty(username) || String.IsNullOrEmpty(email)
    || String.IsNullOrEmpty(idRol))
    {
      Console.WriteLine("No se recibieron datos validos en UserService para editar usuario.");
      return;
    }

    //en caso de datos validos, llamar al metodo para actualizar usuario de ActiveID
    AccessUsers.Update(Guid.Parse(userId), username, email, Guid.Parse(idRol));

    //ACTUALIZAR ESTADO DEL USUARIO
    // para actualizar el estado, como no es parte del metodo Update de ActiveID, se debe llamar al metodo de UserService
    if (isActive) // si se quiere activar
    {
      ActivateUserByUserID(Guid.Parse(userId));
    }
    else // si se quiere desactivar
    {
      DeactivateUserByUserID(Guid.Parse(userId));
    }

  }

}
