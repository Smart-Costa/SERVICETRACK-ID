using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AspnetCoreMvcFull.Models;
using System.Data.SqlClient;

namespace AspnetCoreMvcFull.Controllers;

public class DashboardsController : Controller
{
  public IActionResult Index()
  {
    ViewBag.NombreUbicacion = GetUltimoNombreUbicacionC() ?? "Ubicación C";
    ViewBag.NombreUbicacionA = GetUltimoNombreUbicacionA() ?? "Ubicación A";
    ViewBag.NombreUbicacionB = GetUltimoNombreUbicacionB() ?? "Ubicación B";
    return View();
  }

  public IActionResult CRM() => View();

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

}
