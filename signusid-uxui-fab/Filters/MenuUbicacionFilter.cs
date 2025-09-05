using Microsoft.AspNetCore.Mvc.Filters;
using System.Data.SqlClient;

public class MenuUbicacionFilter : IActionFilter
{
  private readonly string _cs;

  public MenuUbicacionFilter(IConfiguration cfg)
  {
    _cs = cfg.GetConnectionString("ServerDiverscan");
  }

  public void OnActionExecuting(ActionExecutingContext context)
  {
    try
    {
      using var conn = new SqlConnection(_cs);
      conn.Open();

      using var cmd = new SqlCommand(
          "SELECT TOP 1 NOMBRE_UBICACION_C FROM UBICACION_C_NOMBRE ORDER BY ID DESC", conn);
      var r = cmd.ExecuteScalar();

      var controller = (Microsoft.AspNetCore.Mvc.Controller)context.Controller;
      controller.ViewBag.NombreUbicacion = (r == null || r == DBNull.Value) ? "Ubicación C" : r.ToString();
    }
    catch
    {
      var controller = (Microsoft.AspNetCore.Mvc.Controller)context.Controller;
      controller.ViewBag.NombreUbicacion ??= "Ubicación C";
    }
  }

  public void OnActionExecuted(ActionExecutedContext context) { }
}
