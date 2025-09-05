using Microsoft.AspNetCore.Mvc;

namespace AspnetCoreMvcFull.Controllers
{
  public class InventarioController : Controller
  {
    public IActionResult Index()
    {
      return View();
    }
    public IActionResult Bodegas()
    {
      return View();
    }
    public IActionResult Repuestos()
    {
      return View();
    }
    public IActionResult Materiales()
    {
      return View();
    }
  }
}
