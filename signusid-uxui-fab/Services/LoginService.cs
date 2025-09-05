using System.Data;
using Diverscan.Activos.DL;

public class LoginService
{

  //Metodo para obtener el estado de aprobación del usuario de la Base de Datos mediante el username recibido
  public static bool get_Aproval_By_Username(string username)
  {
    //Variable que almacena los datos del usuario encontrado de la Base de Datos
    //Es de tipo DataTable para extraer los datos segun las columnas
    DataTable? UserTableResult = null;

    //Variable que almacenará el estado de aprovación del usuario
    bool isUserAproved = false;

    //Se llama al metodo que valida el usuario y obtiene todos sus datos de la Base de datos y se asigna el resultado a la
    // variable UserTableResult
    UserTableResult = AccessUsers.ValidateUser(username, "WebAssetCtrl");

    // Si el resultado no es nulo y tiene por lo menos un registro, entonces se encontró la data del usuario
    if ((UserTableResult != null) && (UserTableResult.Rows.Count > 0))
    {
      // se asigna el estado de aprovación del usuario
      isUserAproved = (bool)UserTableResult.Rows[0]["isApproved"];
    }

    return isUserAproved;

  }


  //Metodo para obtener la contraseña encriptada de la Base de Datos mediante el username recibido
  public static string get_Password_By_Username(string username)
  {
    //Variable que almacena los datos del usuario encontrado de la Base de Datos
    //Es de tipo DataTable para extraer los datos segun las columnas
    DataTable? UserTableResult = null;

    //Variable que almacenará el la contraseña del usuario
    string? userPassword = "";

    //Se llama al metodo que valida el usuario y obtiene todos sus datos de la Base de datos y se asigna el resultado a la
    // variable UserTableResult
    UserTableResult = AccessUsers.ValidateUser(username, "WebAssetCtrl");

    // Si el resultado no es nulo y tiene por lo menos un registro, entonces se encontró la data del usuario
    if ((UserTableResult != null) && (UserTableResult.Rows.Count > 0))
    {
      // se asigna la contraseña y el estado de aprovación del usuario a las variables definidas
      userPassword = UserTableResult.Rows[0]["password"].ToString();
    }

    return (userPassword != null) ? userPassword : "";

  }


  // Metodo principal, maneja el login recibiendo el usuario y la contraseña
  public static bool logInUser(string username, string password)
  {
    // Mediante el username obtiene la contraseña encriptada de la BdD
    string encrypted_password = get_Password_By_Username(username);
    // Desencripta la contraseña utilizando el metodo para desencriptar de la Clase Encrypt
    string decrypted_password = Encrypt.Decrypting(encrypted_password, true);

    //Compara la contraseña ingresada con la desencriptada
    if (password != decrypted_password)
    {
      //Si no coinciden retorna falso
      return false;
    }
    // se almacena el estado de aprobacion del usuario
    bool user_aproval = get_Aproval_By_Username(username);
    // si el usuario no esta aprobado se rechaza el inicio de sesion
    if (!user_aproval)
    {
      return false;
    }

    //Si coinciden retorna true
    return true;

  }




}
