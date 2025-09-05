using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AspnetCoreMvcFull.Models;
using Diverscan.Activos.DL;
using System.Data;


namespace AspnetCoreMvcFull.Controllers;

public class AuthController : Controller
{

  public IActionResult ForgotPasswordBasic() => View();
  public IActionResult ForgotPasswordCover() => View();
  public IActionResult LoginBasic() => View();


  // Metodo para mostrar la vista del login
  [HttpGet]
  public IActionResult Login()
  {
    return View(false);
  }

  // Metodo para iniciar sesion
  [HttpPost]
  public IActionResult Login(string username, string password)
  {
    //variable para almacenar el estado del login
    bool loginSuccesful = false;
    //En caso de que no se ingresaran credenciales validas se retorna sin realizar ningun proceso
    if (String.IsNullOrEmpty(username) || String.IsNullOrEmpty(password))
    {
      return View(loginSuccesful);
    }

    // El metodo logInUser de la clase LoginService recibe dos parametros: nombre de usuario y contraseña
    // el metodo hace los siguientes procesos:
    // 1. validar existencia de usuario
    // 2. obtener contraseña encriptada y estado de aprobacion del usuario
    // 3. desencriptar la contraseña
    // 4. Comparar la contraseña inicial a la desencriptada
    // 5. Comprobar que estado de aprobacion sea verdadero
    // 5. retornar true o false dependiendo de si se tuvo exito o no
    loginSuccesful = LoginService.logInUser(username, password);


    // Enviar mensaje de error a la vista en caso de que las credenciales hayan sido invalidas
    if (!loginSuccesful)
    {
      ViewData["login_error_msg"] = "Credenciales invalidas.";
      return View(loginSuccesful);
    }
    //En caso de exito, redireccionar al listado de usuarios
    return RedirectToAction("GestionServicios", "GestionServicio");

  }


  public IActionResult RegisterBasic() => View();
  public IActionResult RegisterCover() => View();
  public IActionResult RegisterMultiSteps() => View();
  public IActionResult ResetPasswordBasic() => View();
  public IActionResult ResetPasswordCover() => View();
  public IActionResult TwoStepsBasic() => View();
  public IActionResult TwoStepsCover() => View();
  public IActionResult VerifyEmailBasic() => View();
  public IActionResult VerifyEmailCover() => View();
}
