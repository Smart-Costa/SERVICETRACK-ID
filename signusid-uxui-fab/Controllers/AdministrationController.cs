using AspnetCoreMvcFull.Models.Activos;
using AspnetCoreMvcFull.Models.Edificios;
using AspnetCoreMvcFull.Models.Roles;
using Diverscan.Activos.DL;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using Diverscan.Activos.EL;
using System.Drawing.Printing;
using AspnetCoreMvcFull.Models.Mensajes;
using Microsoft.Data.SqlClient.DataClassification;
using Diverscan.Activos.UIL.Admin;
using AspnetCoreMvcFull.Models.Pisos;
using AspnetCoreMvcFull.Models.Sectores;
using AspnetCoreMvcFull.Models.Oficinas;
using static AspnetCoreMvcFull.Controllers.AccessController;
using System.Text.Json.Serialization;
using System.Text.Json;
using AspnetCoreMvcFull.Models.Categorias;
using AspnetCoreMvcFull.Models.Marcas;
using OfficeOpenXml;
using static AspnetCoreMvcFull.Controllers.EmpresaController;
using System.Globalization;
using System.Configuration;
using AspnetCoreMvcFull.Models.TomasFisicas;
using AspnetCoreMvcFull.Data;
using AspnetCoreMvcFull.Models.GestionServicios;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Data.SqlTypes;


namespace AspnetCoreMvcFull.Controllers
{
  public class AdministrationController : Controller
  {


    public IActionResult Index()
    {
      return View();
    }//
    public IActionResult Reportes()
    {
      return View();
    }
    public IActionResult PlaneamientoTomaFisica(string search, int page = 1, int pageSize = 10, string sortColumn = "NombreTomaFisica", string sortDirection = "asc", string hasAssets = "")
    {
      Console.WriteLine($"Search Query: {search}");

      // Obtener la lista de tomas físicas
      var tomas = ObtenerTomasFisicas();

      // Filtrar por término de búsqueda
      if (!string.IsNullOrEmpty(search))
      {
        tomas = tomas
            .Where(t =>
                (!string.IsNullOrEmpty(t.NombreTomaFisica) && t.NombreTomaFisica.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrEmpty(t.descripcionTomaFisica) && t.descripcionTomaFisica.Contains(search, StringComparison.OrdinalIgnoreCase)))
            .ToList();
      }

      // Filtro por activos (si se aplica)
      if (hasAssets == "withAssets")
      {
        tomas = tomas.Where(t => t.Activos > 0).ToList();
      }
      else if (hasAssets == "withoutAssets")
      {
        tomas = tomas.Where(t => t.Activos == 0).ToList();
      }

      // Ordenamiento dinámico
      tomas = sortColumn switch
      {
        "descripcionTomaFisica" => sortDirection == "asc"
            ? tomas.OrderBy(t => t.descripcionTomaFisica).ToList()
            : tomas.OrderByDescending(t => t.descripcionTomaFisica).ToList(),

        "FechaInicial" => sortDirection == "asc"
            ? tomas.OrderBy(t => t.FechaInicial).ToList()
            : tomas.OrderByDescending(t => t.FechaInicial).ToList(),

        "Activos" => sortDirection == "asc"
            ? tomas.OrderBy(t => t.Activos).ToList()
            : tomas.OrderByDescending(t => t.Activos).ToList(),

        _ => sortDirection == "asc"
            ? tomas.OrderBy(t => t.NombreTomaFisica).ToList()
            : tomas.OrderByDescending(t => t.NombreTomaFisica).ToList(),
      };

      // Paginación
      var totalItems = tomas.Count;
      var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
      var itemsOnPage = tomas.Skip((page - 1) * pageSize).Take(pageSize).ToList();

      // Crear ViewModel
      var model = new TomasFisicasViewModel
      {
        Tomas = itemsOnPage,
        CurrentPage = page,
        TotalPages = totalPages,
        search = search
      };

      // ViewBags para conservar contexto
      ViewBag.SearchQuery = search;
      ViewBag.SortColumn = sortColumn;
      ViewBag.SortDirection = sortDirection;
      ViewBag.Filter = hasAssets;
      ViewData["Categories_Per_Page"] = pageSize;

      // Vista
      return View("PlaneamientoTomaFisica", model);
    }

    public List<TomasFisicas> ObtenerTomasFisicas()
    {
      var tomasFisicas = new List<TomasFisicas>();

      string query = @"
     SELECT 
         tomaFisicaId,
         fechaInicial,
         fechaFinal,
         nombre,
         descripcion,
         categoria,
         usuarioAsignado,
         ubicacionA,
         ubicacionB,
         unidadOrganizativa,
         estadoActivo,
         ubicacionC,
         ubicacionD
     FROM TomaFisicaSignus
 ";

      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

      using (SqlConnection connection = new SqlConnection(connectionString))
      {
        SqlCommand command = new SqlCommand(query, connection);
        command.CommandType = CommandType.Text;

        try
        {
          connection.Open();
          SqlDataReader reader = command.ExecuteReader();

          while (reader.Read())
          {
            TomasFisicas toma = new TomasFisicas
            {
              IdTomaFIsica = reader.GetGuid(0),
              FechaInicial = reader.IsDBNull(1) ? (DateTime?)null : reader.GetDateTime(1),
              FechaFinal = reader.IsDBNull(2) ? (DateTime?)null : reader.GetDateTime(2),
              NombreTomaFisica = reader.IsDBNull(3) ? null : reader.GetString(3),
              descripcionTomaFisica = reader.IsDBNull(4) ? null : reader.GetString(4),
              CategoriaTomaFisica = reader.IsDBNull(5) ? (Guid?)null : reader.GetGuid(5),
              UsuarioAsignadoTomaFisica = reader.IsDBNull(6) ? (Guid?)null : reader.GetGuid(6),
              UbicacionATomaFisica = reader.IsDBNull(7) ? (Guid?)null : reader.GetGuid(7),
              UbicacionBTomaFisica = reader.IsDBNull(8) ? (Guid?)null : reader.GetGuid(8),
              UnidadOrganizativaTomaFisica = reader.IsDBNull(9) ? (Guid?)null : reader.GetGuid(9),
              EstadoActivo = reader.IsDBNull(10) ? (Guid?)null : reader.GetGuid(10),
              UbicacionCTomaFisica = reader.IsDBNull(11) ? (Guid?)null : reader.GetGuid(11),
              UbicacionDTomaFisica = reader.IsDBNull(12) ? (Guid?)null : reader.GetGuid(12),
              Activos = 0 // Puedes calcular esto luego si lo necesitas
            };

            tomasFisicas.Add(toma);
          }

          reader.Close();
        }
        catch (Exception ex)
        {
          Console.WriteLine("Error al obtener tomas físicas: " + ex.Message);
        }
      }

      return tomasFisicas;
    }

    public IActionResult BorrarGestionServicio(Guid id)
    {
      var alertMessage = new AlertMessage();

      try
      {
        string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

        using (SqlConnection connection = new SqlConnection(connectionString))
        {
          connection.Open();

          string query = @"DELETE FROM GestionServicios WHERE gestionServiciosId = @Id";

          using (SqlCommand command = new SqlCommand(query, connection))
          {
            command.Parameters.AddWithValue("@Id", id);
            int rowsAffected = command.ExecuteNonQuery();

            if (rowsAffected > 0)
            {
              alertMessage.Tipo = "success";
              alertMessage.Mensaje = "Gestión de servicio eliminada correctamente.";
            }
            else
            {
              alertMessage.Tipo = "error";
              alertMessage.Mensaje = "No se encontró la gestión de servicio para eliminar.";
            }
          }
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine("Error al borrar gestión de servicio: " + ex.Message);
        alertMessage.Tipo = "error";
        alertMessage.Mensaje = "Error al borrar la gestión de servicio: " + ex.Message;
      }

      TempData["Alert"] = JsonSerializer.Serialize(alertMessage);
      return RedirectToAction("GestionServicios"); // Cambia este nombre si la vista se llama diferente
    }

    public IActionResult PlanTomaFisica()
    {
      return View();
    }
    public IActionResult RegistroEdicionActivos()
    {
      int siguienteNumeroActivo = 1; // Valor por defecto

      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

      using (var connection = new SqlConnection(connectionString))
      {
        string query = "SELECT MAX(NUMERO_ACTIVO) FROM ActivosSignusID";

        using (var command = new SqlCommand(query, connection))
        {
          connection.Open();
          var result = command.ExecuteScalar();

          if (result != DBNull.Value && result != null)
          {
            siguienteNumeroActivo = Convert.ToInt32(result) + 1;
          }
        }
      }
      ViewBag.NombreUbicacion = GetUltimoNombreUbicacionC() ?? "Ubicación C";
      ViewBag.NombreUbicacionA = GetUltimoNombreUbicacionA() ?? "Ubicación A";
      ViewBag.NombreUbicacionB = GetUltimoNombreUbicacionB() ?? "Ubicación B";

      ViewBag.SiguienteNumeroActivo = siguienteNumeroActivo;
      return View();
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
    public IActionResult CrearTomaFisica()
    {
      ViewBag.NombreUbicacion = GetUltimoNombreUbicacionC() ?? "Ubicación C";
      ViewBag.NombreUbicacionA = GetUltimoNombreUbicacionA() ?? "Ubicación A";
      ViewBag.NombreUbicacionB = GetUltimoNombreUbicacionB() ?? "Ubicación B";

      return View();
    }

    public IActionResult IncidentesActivos()
    {
      return View();
    }

    //public IActionResult GestionServicios()
    //{

    //  return View();
    //}

    [HttpGet]
    public IActionResult VerFotoPorRuta(Guid idActivo, int slot)
    {
      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

      using (var conn = new SqlConnection(connectionString))
      {
        conn.Open();

        var cmd = new SqlCommand(@"
            SELECT DESTINO_FOTO
            FROM FOTOS_ACTIVOS
            WHERE ID_ACTIVO = @ID AND CONSECUTIVO = @SLOT", conn);

        cmd.Parameters.AddWithValue("@ID", idActivo);
        cmd.Parameters.AddWithValue("@SLOT", slot);

        var resultado = cmd.ExecuteScalar();

        if (resultado == null || resultado == DBNull.Value)
          return NotFound();

        var rutaRelativa = resultado.ToString().Replace("/", "\\");
        var rutaCompleta = Path.Combine(Directory.GetCurrentDirectory(), rutaRelativa);

        if (!System.IO.File.Exists(rutaCompleta))
          return NotFound();

        var bytes = System.IO.File.ReadAllBytes(rutaCompleta);
        string contentType = "image/" + Path.GetExtension(rutaCompleta).Replace(".", "");

        return File(bytes, contentType);
      }
    }


    [HttpGet]
    public JsonResult ObtenerFotosActivo(Guid idActivo)
    {
      var lista = new List<object>();
      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

      using (var conn = new SqlConnection(connectionString))
      {
        conn.Open();
        var cmd = new SqlCommand(@"
            SELECT CONSECUTIVO, DESTINO_FOTO
            FROM FOTOS_ACTIVOS
            WHERE ID_ACTIVO = @ID", conn);

        cmd.Parameters.AddWithValue("@ID", idActivo);

        using (var reader = cmd.ExecuteReader())
        {
          while (reader.Read())
          {
            int slot = reader.GetInt32(0);
            string ruta = reader.GetString(1).Replace("\\", "/"); // Aseguramos URL válida
            lista.Add(new { slot, ruta });
          }
        }
      }

      return Json(lista);
    }





    //Gestion de Servicios
    public IActionResult GestionServicios(string search, int page = 1, int pageSize = 10, string sortColumn = "NumeroTicket", string sortDirection = "asc")
    {
      var servicios = ObtenerServicios();

      // Filtro por búsqueda
      if (!string.IsNullOrEmpty(search))
      {
        search = search.ToLower();

        servicios = servicios
            .Where(s =>
                (s.NombreActivo?.ToLower().Contains(search) ?? false) ||
                (s.NumeroTicket.ToString().Contains(search)) ||
                (s.NombreSolicitante?.ToLower().Contains(search) ?? false) ||
                (s.NombreRazonServicio?.ToLower().Contains(search) ?? false) ||
                (s.NombreEstadoActivo?.ToLower().Contains(search) ?? false) ||
                (s.NombreAsignarIncidente?.ToLower().Contains(search) ?? false) ||
                (s.Descripcion?.ToLower().Contains(search) ?? false)
            )
            .ToList();
      }
      // Ordenamiento
      servicios = sortColumn switch
      {
        "Fecha" => sortDirection == "asc"
            ? servicios.OrderBy(s => s.Fecha).ToList()
            : servicios.OrderByDescending(s => s.Fecha).ToList(),
        "FechaEstimadaCierre" => sortDirection == "asc"
            ? servicios.OrderBy(s => s.FechaEstimadaCierre).ToList()
            : servicios.OrderByDescending(s => s.FechaEstimadaCierre).ToList(),
        "NumeroTicket" => sortDirection == "asc"
            ? servicios.OrderBy(s => s.NumeroTicket).ToList()
            : servicios.OrderByDescending(s => s.NumeroTicket).ToList(),
        _ => servicios.OrderBy(s => s.NumeroTicket).ToList()
      };

      // Paginación
      var totalItems = servicios.Count;
      var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
      var itemsOnPage = servicios.Skip((page - 1) * pageSize).Take(pageSize).ToList();

      // ViewModel
      var model = new GestionServiciosTablaViewModel
      {
        Registros = itemsOnPage,
        CurrentPage = page,
        TotalPages = totalPages,
        Search = search
      };
      ViewBag.NombreUbicacion = GetUltimoNombreUbicacionC() ?? "Ubicación C";
      ViewBag.NombreUbicacionA = GetUltimoNombreUbicacionA() ?? "Ubicación A";
      ViewBag.NombreUbicacionB = GetUltimoNombreUbicacionB() ?? "Ubicación B";

      ViewBag.SortColumn = sortColumn;
      ViewBag.SortDirection = sortDirection;
      ViewBag.SearchQuery = search;

      return View("GestionServicios", model); // asegúrate de tener la vista
    }

    public IActionResult TablaGestionServicios()
    {
      var servicios = ObtenerServicios();

      var viewModel = new GestionServiciosTablaViewModel
      {
        Registros = servicios,
        CurrentPage = 1,
        TotalPages = 1,
        Search = ""
      };

      return View(viewModel);
    }

    public List<GestionServiciosDTO> ObtenerServicios()
    {
      var lista = new List<GestionServiciosDTO>();
      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

      using (SqlConnection conn = new SqlConnection(connectionString))
      {
        string query = @"
            SELECT 
                gs.gestionServiciosId,
                gs.solicitante,
                e1.name AS NombreSolicitante,
                gs.activo,
                a.NUMERO_ACTIVO AS NombreActivo,
                gs.razonServicio,
                rs.nombre AS NombreRazonServicio,
                gs.estadoActivo,
                es.nombre AS NombreEstadoActivo,
                gs.asignarIncidente,
                u.username AS NombreAsignarIncidente,
                gs.fechaEstimadaCierre,
                gs.fecha,
                gs.descripcion,
                gs.numeroTicket
            FROM GestionServicios gs
            LEFT JOIN employees e1 ON gs.solicitante = e1.employeeSysId
            LEFT JOIN users u ON gs.asignarIncidente = u.userSysId
            LEFT JOIN ActivosSignusID a ON gs.activo = a.ID_ACTIVO
            LEFT JOIN RazonServicios rs ON gs.razonServicio = rs.id_razonServicios
            LEFT JOIN EstadoActivo es ON gs.estadoActivo = es.id_estadoActivo
        ";

        SqlCommand cmd = new SqlCommand(query, conn);
        conn.Open();

        using (SqlDataReader reader = cmd.ExecuteReader())
        {
          while (reader.Read())
          {
            var item = new GestionServiciosDTO
            {
              GestionServiciosId = reader.GetGuid(0),
              SolicitanteId = reader.GetGuid(1),
              NombreSolicitante = reader["NombreSolicitante"]?.ToString(),
              ActivoId = reader.GetGuid(3),
              NombreActivo = reader["NombreActivo"]?.ToString(),
              RazonServicioId = reader.GetGuid(5),
              NombreRazonServicio = reader["NombreRazonServicio"]?.ToString(),
              EstadoActivoId = reader.GetGuid(7),
              NombreEstadoActivo = reader["NombreEstadoActivo"]?.ToString(),
              AsignarIncidenteId = reader.IsDBNull(9) ? (Guid?)null : reader.GetGuid(9),
              NombreAsignarIncidente = reader.IsDBNull(reader.GetOrdinal("NombreAsignarIncidente"))
    ? "Sin Asignar"
    : reader["NombreAsignarIncidente"].ToString(),

              FechaEstimadaCierre = reader.IsDBNull(11) ? null : reader.GetDateTime(11),
              Fecha = reader.GetDateTime(12),
              Descripcion = reader["descripcion"]?.ToString(),
              NumeroTicket = reader.GetInt32(14)
            };

            lista.Add(item);
          }
        }
      }

      return lista;
    }





    [HttpGet]
    public IActionResult VerFotoExistente(Guid idActivo, int slot)
    {
      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;
      using (var conn = new SqlConnection(connectionString))
      {
        conn.Open();
        var cmd = new SqlCommand(@"
            SELECT DESTINO_FOTO FROM FOTOS_ACTIVOS
            WHERE ID_ACTIVO = @ID AND CONSECUTIVO = @SLOT", conn);

        cmd.Parameters.AddWithValue("@ID", idActivo);
        cmd.Parameters.AddWithValue("@SLOT", slot);

        var resultado = cmd.ExecuteScalar();
        if (resultado == null || resultado == DBNull.Value)
          return NotFound();

        var rutaRelativa = resultado.ToString().Replace("/", "\\");
        var rutaCompleta = Path.Combine(Directory.GetCurrentDirectory(), rutaRelativa);

        if (!System.IO.File.Exists(rutaCompleta))
          return NotFound();

        var bytes = System.IO.File.ReadAllBytes(rutaCompleta);
        var contentType = "image/" + Path.GetExtension(rutaCompleta).Replace(".", "");

        return File(bytes, contentType);
      }
    }

    [HttpPost]
    public JsonResult SubirFotoActivo(IFormFile archivoFoto, string tipoDoc, Guid idActivo)
    {
      if (archivoFoto == null || archivoFoto.Length == 0)
        return Json(new { success = false, message = "Foto inválida" });

      try
      {
        var nombre = Path.GetFileNameWithoutExtension(archivoFoto.FileName);
        var extension = Path.GetExtension(archivoFoto.FileName);
        var nuevoNombre = $"{Guid.NewGuid()}{extension}";
        var rutaRelativa = Path.Combine("FotosActivos", idActivo.ToString(), nuevoNombre).Replace("\\", "/");
        var rutaFisica = Path.Combine(Directory.GetCurrentDirectory(), "FotosActivos", idActivo.ToString(), nuevoNombre);

        // Crear carpeta si no existe
        var carpeta = Path.GetDirectoryName(rutaFisica);
        if (!Directory.Exists(carpeta))
          Directory.CreateDirectory(carpeta);

        using (var stream = new FileStream(rutaFisica, FileMode.Create))
        {
          archivoFoto.CopyTo(stream);
        }

        string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;
        using (var conn = new SqlConnection(connectionString))
        {
          conn.Open();

          // Validar que ya no hayan más de 5 fotos
          //var checkTotal = new SqlCommand("SELECT COUNT(*) FROM FOTOS_ACTIVOS WHERE ID_ACTIVO = @ID", conn);
          //checkTotal.Parameters.AddWithValue("@ID", idActivo);
          //int cantidadActual = (int)checkTotal.ExecuteScalar();
          //if (cantidadActual >= 5)
          //  return Json(new { success = false, message = "Ya se han subido 5 fotos para este activo." });

          // Verificar si ya hay una foto en ese mismo slot
          var checkCmd = new SqlCommand(@"
                SELECT DESTINO_FOTO FROM FOTOS_ACTIVOS 
                WHERE ID_ACTIVO = @ID_ACTIVO AND CONSECUTIVO = @CONSECUTIVO", conn);

          checkCmd.Parameters.AddWithValue("@ID_ACTIVO", idActivo);
          checkCmd.Parameters.AddWithValue("@CONSECUTIVO", int.Parse(tipoDoc));

          var resultado = checkCmd.ExecuteScalar();

          if (resultado != null && resultado != DBNull.Value)
          {
            // Eliminar foto anterior
            var rutaAnterior = Path.Combine(Directory.GetCurrentDirectory(), resultado.ToString().Replace("/", "\\"));
            if (System.IO.File.Exists(rutaAnterior))
              System.IO.File.Delete(rutaAnterior);

            // Actualizar existente
            var updateCmd = new SqlCommand(@"
                    UPDATE FOTOS_ACTIVOS
                    SET NOMBRE_FOTO = @NOMBRE_FOTO,
                        DESTINO_FOTO = @DESTINO_FOTO,
                        EXTENSION = @EXTENSION
                    WHERE ID_ACTIVO = @ID_ACTIVO AND CONSECUTIVO = @CONSECUTIVO", conn);

            updateCmd.Parameters.AddWithValue("@ID_ACTIVO", idActivo);
            updateCmd.Parameters.AddWithValue("@CONSECUTIVO", int.Parse(tipoDoc));
            updateCmd.Parameters.AddWithValue("@NOMBRE_FOTO", nombre);
            updateCmd.Parameters.AddWithValue("@DESTINO_FOTO", rutaRelativa);
            updateCmd.Parameters.AddWithValue("@EXTENSION", extension);

            updateCmd.ExecuteNonQuery();
          }
          else
          {
            // Insertar nueva
            var insertCmd = new SqlCommand(@"
                    INSERT INTO FOTOS_ACTIVOS 
                    (ID_FOTO, ID_ACTIVO, NOMBRE_FOTO, DESTINO_FOTO, EXTENSION, CONSECUTIVO)
                    VALUES (NEWID(), @ID_ACTIVO, @NOMBRE_FOTO, @DESTINO_FOTO, @EXTENSION, @CONSECUTIVO)", conn);

            insertCmd.Parameters.AddWithValue("@ID_ACTIVO", idActivo);
            insertCmd.Parameters.AddWithValue("@NOMBRE_FOTO", nombre);
            insertCmd.Parameters.AddWithValue("@DESTINO_FOTO", rutaRelativa);
            insertCmd.Parameters.AddWithValue("@EXTENSION", extension);
            insertCmd.Parameters.AddWithValue("@CONSECUTIVO", int.Parse(tipoDoc));

            insertCmd.ExecuteNonQuery();
          }
        }

        return Json(new { success = true, ruta = rutaRelativa });
      }
      catch (Exception ex)
      {
        return Json(new { success = false, message = ex.Message });
      }
    }




    public IActionResult VerDocumento(string ruta)
    {
      try
      {
        if (string.IsNullOrEmpty(ruta))
          return NotFound();

        ruta = ruta.TrimStart('\\', '/'); // ✅ Limpia la ruta
        var rutaCompleta = Path.Combine(Directory.GetCurrentDirectory(), ruta);

        if (!System.IO.File.Exists(rutaCompleta))
          return NotFound();

        var tipoMime = GetMimeType(Path.GetExtension(rutaCompleta));
        var bytes = System.IO.File.ReadAllBytes(rutaCompleta);
        return File(bytes, tipoMime);
      }
      catch (Exception ex)
      {
        return Json(new { success = false, message = ex.Message });
      }
    }



    private string GetMimeType(string extension)
    {
      return extension.ToLower() switch
      {
        ".pdf" => "application/pdf",
        ".doc" => "application/msword",
        ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        _ => "application/octet-stream"
      };
    }

    public JsonResult ObtenerDocumentoActivo(Guid id, int tipo)
    {

      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;
      using (var conn = new SqlConnection(connectionString))
      {
        var cmd = new SqlCommand("SELECT DESTINO_DOCUMENTO FROM DOCUMENTOS_ACTIVOS WHERE ID_ACTIVO = @id AND CONSECUTIVO = @tipo", conn);
        cmd.Parameters.AddWithValue("@id", id);
        cmd.Parameters.AddWithValue("@tipo", tipo);
        conn.Open();

        var result = cmd.ExecuteScalar();
        if (result != null && result != DBNull.Value)
        {
          var ruta = "/" + result.ToString()!.Replace("\\", "/");
          return Json(new { success = true, url = ruta });
        }
      }
      return Json(new { success = false });
    }


    [HttpPost]
    public JsonResult SubirDocumentoActivo(IFormFile archivoDocumento, string tipoDoc, Guid idActivo)
    {
      if (archivoDocumento == null || archivoDocumento.Length == 0)
        return Json(new { success = false, message = "Archivo inválido" });

      try
      {
        var nombre = Path.GetFileNameWithoutExtension(archivoDocumento.FileName);
        var extension = Path.GetExtension(archivoDocumento.FileName);
        var nuevoNombre = $"{Guid.NewGuid()}{extension}";
        var rutaRelativa = Path.Combine("DocumentosActivo", nuevoNombre).Replace("\\", "/");
        var rutaFisica = Path.Combine(Directory.GetCurrentDirectory(), "DocumentosActivo", nuevoNombre);

        // Crear carpeta si no existe
        var carpeta = Path.GetDirectoryName(rutaFisica);
        if (!Directory.Exists(carpeta))
          Directory.CreateDirectory(carpeta);

        using (var stream = new FileStream(rutaFisica, FileMode.Create))
        {
          archivoDocumento.CopyTo(stream);
        }

        string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;
        using (var conn = new SqlConnection(connectionString))
        {
          conn.Open();

          // Verificar si ya existe documento con mismo activo + consecutivo
          var checkCmd = new SqlCommand(@"
                SELECT DESTINO_DOCUMENTO FROM DOCUMENTOS_ACTIVOS 
                WHERE ID_ACTIVO = @ID_ACTIVO AND CONSECUTIVO = @CONSECUTIVO", conn);

          checkCmd.Parameters.AddWithValue("@ID_ACTIVO", idActivo);
          checkCmd.Parameters.AddWithValue("@CONSECUTIVO", int.Parse(tipoDoc));

          var resultado = checkCmd.ExecuteScalar();

          if (resultado != null && resultado != DBNull.Value)
          {
            // 🧹 Opcional: eliminar archivo anterior del disco
            var rutaAnterior = Path.Combine(Directory.GetCurrentDirectory(), resultado.ToString().Replace("/", "\\"));
            if (System.IO.File.Exists(rutaAnterior))
              System.IO.File.Delete(rutaAnterior);

            // 🔄 Actualizar documento existente
            var updateCmd = new SqlCommand(@"
                    UPDATE DOCUMENTOS_ACTIVOS
                    SET NOMBRE_DOCUMENTO = @NOMBRE_DOCUMENTO,
                        DESTINO_DOCUMENTO = @DESTINO_DOCUMENTO,
                        EXTENSION = @EXTENSION
                    WHERE ID_ACTIVO = @ID_ACTIVO AND CONSECUTIVO = @CONSECUTIVO", conn);

            updateCmd.Parameters.AddWithValue("@ID_ACTIVO", idActivo);
            updateCmd.Parameters.AddWithValue("@CONSECUTIVO", int.Parse(tipoDoc));
            updateCmd.Parameters.AddWithValue("@NOMBRE_DOCUMENTO", nombre);
            updateCmd.Parameters.AddWithValue("@DESTINO_DOCUMENTO", rutaRelativa);
            updateCmd.Parameters.AddWithValue("@EXTENSION", extension);

            updateCmd.ExecuteNonQuery();
          }
          else
          {
            // 🆕 Insertar nuevo documento
            var insertCmd = new SqlCommand(@"
                    INSERT INTO DOCUMENTOS_ACTIVOS 
                    (ID_DOCUMENTO, ID_ACTIVO, NOMBRE_DOCUMENTO, DESTINO_DOCUMENTO, EXTENSION, CONSECUTIVO)
                    
                    VALUES (NEWID(), @ID_ACTIVO, @NOMBRE_DOCUMENTO, @DESTINO_DOCUMENTO, @EXTENSION, @CONSECUTIVO)", conn);

            insertCmd.Parameters.AddWithValue("@ID_ACTIVO", idActivo);
            insertCmd.Parameters.AddWithValue("@NOMBRE_DOCUMENTO", nombre);
            insertCmd.Parameters.AddWithValue("@DESTINO_DOCUMENTO", rutaRelativa);
            insertCmd.Parameters.AddWithValue("@EXTENSION", extension);
            insertCmd.Parameters.AddWithValue("@CONSECUTIVO", int.Parse(tipoDoc));

            insertCmd.ExecuteNonQuery();
          }
        }

        return Json(new { success = true });
      }
      catch (Exception ex)
      {
        return Json(new { success = false, message = ex.Message });
      }
    }



    [HttpGet]
    public JsonResult ObtenerCategorias()
    {
      var lista = new List<object>();

      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

      using (var connection = new SqlConnection(connectionString))
      {
        string query = "SELECT assetCategorySysId, name, description FROM assetCategory";
        using (var command = new SqlCommand(query, connection))
        {
          connection.Open();
          using (var reader = command.ExecuteReader())
          {
            while (reader.Read())
            {
              lista.Add(new
              {
                id = reader.GetGuid(0),
                nombre = reader.GetString(1),
                descripcion = reader.GetString(2)
              });
            }
          }
        }
      }

      return Json(lista);
    }
    [HttpGet]
    public IActionResult ObtenerEstados()
    {
      var lista = new List<object>();
      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

      using (var connection = new SqlConnection(connectionString))
      {
        string query = "SELECT assetStatusSysId, name, description FROM assetStatus";
        using (var command = new SqlCommand(query, connection))
        {
          connection.Open();
          using (var reader = command.ExecuteReader())
          {
            while (reader.Read())
            {
              lista.Add(new
              {
                id = reader.GetGuid(0),
                nombre = reader.GetString(1),
                descripcion = reader.GetString(2)
              });
            }
          }
        }
      }

      return Json(lista);
    }

    [HttpGet]
    public IActionResult ObtenerEstadoActivoSignus()
    {
      var lista = new List<object>();
      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

      using (var connection = new SqlConnection(connectionString))
      {
        string query = "SELECT id_estadoActivo, nombre, descripcion FROM EstadoActivo";
        using (var command = new SqlCommand(query, connection))
        {
          connection.Open();
          using (var reader = command.ExecuteReader())
          {
            while (reader.Read())
            {
              lista.Add(new
              {
                id = reader.GetGuid(0),
                nombre = reader.GetString(1),
                descripcion = reader.GetString(2)
              });
            }
          }
        }
      }

      return Json(lista);
    }


    [HttpGet]
    public IActionResult ObtenerEmpresas()
    {
      var lista = new List<object>();
      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

      using (var connection = new SqlConnection(connectionString))
      {
        string query = "SELECT ID_EMPRESA, NOMBRE FROM EMPRESA";
        using (var command = new SqlCommand(query, connection))
        {
          connection.Open();
          using (var reader = command.ExecuteReader())
          {
            while (reader.Read())
            {
              lista.Add(new
              {
                id = reader.GetGuid(0),
                nombre = reader.GetString(1)
              });
            }
          }
        }
      }

      return Json(lista);
    }
    [HttpGet]
    public IActionResult ObtenerMarcas()
    {
      var lista = new List<object>();
      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

      using (var connection = new SqlConnection(connectionString))
      {
        string query = "SELECT marcaId, nombre FROM Marca";
        using (var command = new SqlCommand(query, connection))
        {
          connection.Open();
          using (var reader = command.ExecuteReader())
          {
            while (reader.Read())
            {
              lista.Add(new
              {
                id = reader.GetGuid(0),
                nombre = reader.GetString(1)
              });
            }
          }
        }
      }

      return Json(lista);
    }
    [HttpGet]
    public IActionResult ObtenerModelosPorMarca(Guid idMarca)
    {
      var lista = new List<object>();
      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

      using (var connection = new SqlConnection(connectionString))
      {
        string query = "SELECT modeloId, nombre FROM Modelo WHERE marcaId = @marcaId";
        using (var command = new SqlCommand(query, connection))
        {
          command.Parameters.AddWithValue("@marcaId", idMarca);
          connection.Open();

          using (var reader = command.ExecuteReader())
          {
            while (reader.Read())
            {
              lista.Add(new
              {
                id = reader.GetGuid(0),
                nombre = reader.GetString(1)
              });
            }
          }
        }
      }

      return Json(lista);
    }
    [HttpGet]
    public IActionResult ObtenerCuentasContables()
    {
      var lista = new List<object>();
      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

      using (var connection = new SqlConnection(connectionString))
      {
        string query = "SELECT ID_CUENTA_CONTABLE_DEPRESIACION, NOMBRE FROM CUENTA_CONTABLE_DEPRESIACION";
        using (var command = new SqlCommand(query, connection))
        {
          connection.Open();
          using (var reader = command.ExecuteReader())
          {
            while (reader.Read())
            {
              lista.Add(new
              {
                id = reader.GetGuid(0),
                nombre = reader.GetString(1)
              });
            }
          }
        }
      }

      return Json(lista);
    }
    [HttpGet]
    public IActionResult ObtenerCentrosCostos()
    {
      var lista = new List<object>();
      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

      using (var connection = new SqlConnection(connectionString))
      {
        string query = "SELECT ID_CENTRO_COSTOS, NOMBRE FROM CENTRO_COSTOS";
        using (var command = new SqlCommand(query, connection))
        {
          connection.Open();
          using (var reader = command.ExecuteReader())
          {
            while (reader.Read())
            {
              lista.Add(new
              {
                id = reader.GetGuid(0),
                nombre = reader.GetString(1)
              });
            }
          }
        }
      }

      return Json(lista);
    }
    [HttpGet]
    public IActionResult ObtenerEmpleados()
    {
      var lista = new List<object>();
      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

      using (var connection = new SqlConnection(connectionString))
      {
        string query = "SELECT SolicitanteId, NombreCompleto FROM SolicitanteServiceTrackID";
        using (var command = new SqlCommand(query, connection))
        {
          connection.Open();
          using (var reader = command.ExecuteReader())
          {
            while (reader.Read())
            {
              lista.Add(new
              {
                id = reader.GetGuid(0),
                nombre = reader.GetString(1)
              });
            }
          }
        }
      }

      return Json(lista);
    }

    [HttpGet]
    public IActionResult ObtenerUsuarios()
    {
      var lista = new List<object>();
      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

      using (var connection = new SqlConnection(connectionString))
      {
        string query = "SELECT userSysId, username FROM users";
        using (var command = new SqlCommand(query, connection))
        {
          connection.Open();
          using (var reader = command.ExecuteReader())
          {
            while (reader.Read())
            {
              lista.Add(new
              {
                id = reader.GetGuid(0),
                nombre = reader.GetString(1)
              });
            }
          }
        }
      }

      return Json(lista);
    }




    [HttpGet]
    public IActionResult ObtenerUbicacionesA()
    {
      var lista = new List<object>();
      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

      using (var connection = new SqlConnection(connectionString))
      {
        string query = "SELECT companySysId, name, description FROM companies";
        using (var command = new SqlCommand(query, connection))
        {
          connection.Open();
          using (var reader = command.ExecuteReader())
          {
            while (reader.Read())
            {
              lista.Add(new
              {
                id = reader.GetGuid(0),
                texto = $"{reader.GetString(1)} - {reader.GetString(2)}"
              });
            }
          }
        }
      }

      return Json(lista);
    }

    [HttpGet]
    public IActionResult ObtenerUnidadOrganizativa()
    {
      var lista = new List<object>();
      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

      using (var connection = new SqlConnection(connectionString))
      {
        string query = "SELECT id_UnidadOrganizativa, nombre, descripcion FROM UnidadOrganizativa";
        using (var command = new SqlCommand(query, connection))
        {
          connection.Open();
          using (var reader = command.ExecuteReader())
          {
            while (reader.Read())
            {
              lista.Add(new
              {
                id = reader.GetGuid(0),
                texto = $"{reader.GetString(1)} - {reader.GetString(2)}"
              });
            }
          }
        }
      }

      return Json(lista);
    }

    [HttpGet]
    public IActionResult ObtenerActivosSignus()
    {
      var lista = new List<object>();
      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

      using (var connection = new SqlConnection(connectionString))
      {
        // Solo selecciona los que tienen NUMERO_ACTIVO no nulo
        string query = "SELECT ID_ACTIVO, NUMERO_ACTIVO FROM ActivosSignusID WHERE NUMERO_ACTIVO IS NOT NULL  AND EMPLEADO IS NOT NULL  AND EMPLEADO <> '00000000-0000-0000-0000-000000000000'";
        using (var command = new SqlCommand(query, connection))
        {
          connection.Open();
          using (var reader = command.ExecuteReader())
          {
            while (reader.Read())
            {
              lista.Add(new
              {
                id = reader.IsDBNull(0) ? Guid.Empty : reader.GetGuid(0),
                texto = reader.GetInt32(1)
              });
            }
          }
        }
      }

      return Json(lista);
    }

    [HttpGet]
    public IActionResult ObtenerActivosSignus2()
    {
      var lista = new List<object>();
      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

      using (var connection = new SqlConnection(connectionString))
      {
        // Solo selecciona los que tienen NUMERO_ACTIVO no nulo
        string query = "SELECT ID_ACTIVO, DESCRIPCION_LARGA FROM ActivosSignusID WHERE NUMERO_ACTIVO IS NOT NULL  AND EMPLEADO IS NOT NULL  AND EMPLEADO <> '00000000-0000-0000-0000-000000000000'";
        using (var command = new SqlCommand(query, connection))
        {
          connection.Open();
          using (var reader = command.ExecuteReader())
          {
            while (reader.Read())
            {
              lista.Add(new
              {
                id = reader.IsDBNull(0) ? Guid.Empty : reader.GetGuid(0),
                texto = reader.GetString(1)
              });
            }
          }
        }
      }

      return Json(lista);
    }


    [HttpGet]
    public IActionResult ObtenerRazonServicios()
    {
      var lista = new List<object>();
      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

      using (var connection = new SqlConnection(connectionString))
      {
        string query = "SELECT id_razonServicios, nombre, descripcion FROM RazonServicios";
        using (var command = new SqlCommand(query, connection))
        {
          connection.Open();
          using (var reader = command.ExecuteReader())
          {
            while (reader.Read())
            {
              lista.Add(new
              {
                id = reader.GetGuid(0),
                texto = $"{reader.GetString(1)} - {reader.GetString(2)}"
              });
            }
          }
        }
      }

      return Json(lista);
    }


    [HttpGet]
    public IActionResult ObtenerUbicacionesB(Guid idCompany)
    {
      var lista = new List<object>();
      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

      using (var connection = new SqlConnection(connectionString))
      {
        string query = "SELECT buildingSysId, name, description FROM buildings WHERE companySysId = @companySysId";
        using (var command = new SqlCommand(query, connection))
        {
          command.Parameters.AddWithValue("@companySysId", idCompany);
          connection.Open();
          using (var reader = command.ExecuteReader())
          {
            while (reader.Read())
            {
              lista.Add(new
              {
                id = reader.GetGuid(0),
                texto = $"{reader.GetString(1)} - {reader.GetString(2)}"
              });
            }
          }
        }
      }

      return Json(lista);
    }
    [HttpGet]
    public IActionResult ObtenerUbicacionesC(Guid idCompany, Guid idBuilding)
    {
      var lista = new List<object>();
      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

      using (var connection = new SqlConnection(connectionString))
      {
        string query = @"
            SELECT floorSysId, name, description 
            FROM floors 
            WHERE companySysId = @idCompany AND buildingSysId = @idBuilding";

        using (var command = new SqlCommand(query, connection))
        {
          command.Parameters.AddWithValue("@idCompany", idCompany);
          command.Parameters.AddWithValue("@idBuilding", idBuilding);
          connection.Open();

          using (var reader = command.ExecuteReader())
          {
            while (reader.Read())
            {
              lista.Add(new
              {
                id = reader.GetGuid(0),
                texto = $"{reader.GetString(1)} - {reader.GetString(2)}"
              });
            }
          }
        }
      }

      return Json(lista);
    }
    [HttpGet]
    public IActionResult ObtenerUbicacionesD(Guid idCompany, Guid idBuilding, Guid idFloor)
    {
      var lista = new List<object>();
      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

      using (var connection = new SqlConnection(connectionString))
      {
        string query = @"
            SELECT officeSysId, name, description
            FROM officeses
            WHERE companySysId = @idCompany 
              AND buildingSysId = @idBuilding 
              AND floorSysId = @idFloor";

        using (var command = new SqlCommand(query, connection))
        {
          command.Parameters.AddWithValue("@idCompany", idCompany);
          command.Parameters.AddWithValue("@idBuilding", idBuilding);
          command.Parameters.AddWithValue("@idFloor", idFloor);

          connection.Open();
          using (var reader = command.ExecuteReader())
          {
            while (reader.Read())
            {
              lista.Add(new
              {
                id = reader.GetGuid(0),
                texto = $"{reader.GetString(1)} - {reader.GetString(2)}"
              });
            }
          }
        }
      }

      return Json(lista);
    }
    [HttpGet]
    public IActionResult ObtenerUbicacionesSecundarias()
    {
      var lista = new List<object>();
      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;
      using (var connection = new SqlConnection(connectionString))
      {
        string query = "SELECT ID_UBICACION_SECUNDARIA, NOMBRE, DESCRIPCION FROM UBICACION_SECUNDARIA";
        using (var command = new SqlCommand(query, connection))
        {
          connection.Open();
          using (var reader = command.ExecuteReader())
          {
            while (reader.Read())
            {
              lista.Add(new
              {
                id = reader.GetGuid(0),
                texto = $"{reader.GetString(1)} - {reader.GetString(2)}"
              });
            }
          }
        }
      }

      return Json(lista);
    }
    [HttpGet]
    public JsonResult BuscarActivo(string tipo, string valor)
    {
      var activo = ObtenerActivoDesdeBD(tipo, valor);
      return Json(activo);
    }
    private Activos ObtenerActivoDesdeBD(string tipo, string valor)
    {
      string query = tipo == "numeroActivo"
          ? "SELECT * FROM ActivosSignusID WHERE NUMERO_ACTIVO = @valor"
          : "SELECT * FROM ActivosSignusID WHERE NUMERO_ETIQUETA = @valor";

      string cs = System.Configuration.ConfigurationManager
                      .ConnectionStrings["ServerDiverscan"].ConnectionString;

      using var conn = new SqlConnection(cs);
      using var cmd = new SqlCommand(query, conn);

      if (tipo == "numeroActivo")
      {
        // Si buscas por número, mándalo como INT
        if (!int.TryParse(valor, out var numActivo))
          return null; // o lanza una excepción/controla el error

        cmd.Parameters.Add("@valor", SqlDbType.Int).Value = numActivo;
      }
      else
      {
        cmd.Parameters.Add("@valor", SqlDbType.VarChar, 50).Value = valor ?? (object)DBNull.Value;
      }

      conn.Open();
      using var reader = cmd.ExecuteReader();
      if (!reader.Read()) return null;

      // Helpers locales para evitar repetición
      object V(string col) => reader[col];
      Guid? G(string col) => V(col) == DBNull.Value ? (Guid?)null : reader.GetGuid(reader.GetOrdinal(col));
      string S(string col) => V(col) == DBNull.Value ? null : V(col).ToString();
      int? I(string col) => V(col) == DBNull.Value ? (int?)null : Convert.ToInt32(V(col));
      decimal? D(string col) => V(col) == DBNull.Value ? (decimal?)null : Convert.ToDecimal(V(col), CultureInfo.InvariantCulture);
      DateTime? T(string col) => V(col) == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(V(col));

      return new Activos
      {
        // ✅ Campos NO nulos (según dijiste)
        IdActivo = reader.GetGuid(reader.GetOrdinal("ID_ACTIVO")),
        NumeroActivo = V("NUMERO_ACTIVO") != DBNull.Value ? Convert.ToInt32(V("NUMERO_ACTIVO")) : 0,
        DescripcionLarga = S("DESCRIPCION_LARGA"),
        FechaCompra = V("FECHA_COMPRA") != DBNull.Value ? Convert.ToDateTime(V("FECHA_COMPRA")) : DateTime.MinValue,

        // ✅ El resto pueden ser NULL → usa helpers/nullables
        NumeroEtiqueta = S("NUMERO_ETIQUETA"),
        DescripcionCorta = S("DESCRIPCION_CORTA"),
        Categoria = G("CATEGORIA"),
        Estado = G("ESTADO"),
        Empresa = G("EMPRESA"),
        Marca = G("MARCA"),
        Modelo = G("MODELO"),
        NumeroSerie = S("NUMERO_SERIE"),
        Costo = D("COSTO") ?? 0m, // decimal
        NumeroFactura = S("NUMERO_FACTURA"),
        FechaCapitalizacion = T("FECHA_CAPITALIZACION") ?? DateTime.MinValue, // o hazlo DateTime? en el modelo
        ValorResidual = V("VALOR_RESIDUAL") != DBNull.Value
    ? float.Parse(V("VALOR_RESIDUAL").ToString(), CultureInfo.InvariantCulture)
    : 0f,
        NumeroParteFabricante = S("NUMERO_PARTE_FABRICANTE"),
        Depreciado = S("DEPRECIADO"),
        DescripcionDepreciado = S("DESCRIPCION_DEPRECIADO"),
        AnosVidaUtil = I("ANOS_VIDA_UTIL") ?? 0,
        CuentaContableDepresiacion = G("CUENTA_CONTABLE_DEPRESIACION"),
        CentroCostos = G("CENTRO_COSTOS"),
        DescripcionEstadoUltimoInventario = S("DESCRIPCION_ESTADO_ULTIMO_INVENTARIO"),
        TagEPC = S("TAG_EPC"),
        Empleado = G("EMPLEADO"),
        UbicacionA = G("UBICACION_A"),
        UbicacionB = G("UBICACION_B"),
        UbicacionC = G("UBICACION_C"),
        UbicacionD = G("UBICACION_D"),
        UbicacionSecundaria = G("UBICACION_SECUNDARIA"),
        FechaGarantia = T("FECHA_GARANTIA") ?? DateTime.MinValue, // o DateTime?
        Color = S("COLOR"),
        TamanioMedida = S("TAMANIO_MEDIDA"),
        Observaciones = S("OBSERVACIONES"),
        Estado_Activo = I("ESTADO_ACTIVO") ?? 0
      };
    }



    private string? ValidarDatosActivo(Activos model, string estadoFormulario, SqlConnection connection)
    {
      if (estadoFormulario == "Editar")
      {
        // Validación de fecha para edición
        using (var cmdFecha = new SqlCommand("SELECT FECHA_CREACION_ACTIVO FROM ActivosSignusID WHERE ID_ACTIVO = @ID", connection))
        {
          cmdFecha.Parameters.AddWithValue("@ID", model.IdActivo);
          var result = cmdFecha.ExecuteScalar();
          DateTime fechaCreacionActivo = result != null ? Convert.ToDateTime(result) : DateTime.MinValue;

          if (model.FechaCompra > fechaCreacionActivo)
            return "La fecha de compra no puede ser posterior a la fecha de creación del activo.";
        }
      }
      else // Insertar
      {
        if (model.FechaCompra > DateTime.Now)
          return "La fecha de compra no puede ser mayor a la fecha actual.";

        // Validar que no exista ya ese NUMERO_ACTIVO
        if (model.NumeroActivo != null)
        {
          using (var cmdNumero = new SqlCommand("SELECT COUNT(*) FROM ActivosSignusID WHERE NUMERO_ACTIVO = @NumeroActivo", connection))
          {
            cmdNumero.Parameters.AddWithValue("@NumeroActivo", model.NumeroActivo);
            int count = Convert.ToInt32(cmdNumero.ExecuteScalar());
            if (count > 0)
              return $"Ya existe un activo con el número {model.NumeroActivo}.";
          }
        }

        // Validar que no exista ya esa NUMERO_ETIQUETA
        if (!string.IsNullOrWhiteSpace(model.NumeroEtiqueta))
        {
          using (var cmdEtiqueta = new SqlCommand("SELECT COUNT(*) FROM ActivosSignusID WHERE NUMERO_ETIQUETA = @NumeroEtiqueta", connection))
          {
            cmdEtiqueta.Parameters.AddWithValue("@NumeroEtiqueta", model.NumeroEtiqueta.Trim());
            int count = Convert.ToInt32(cmdEtiqueta.ExecuteScalar());
            if (count > 0)
              return $"Ya existe un activo con la etiqueta '{model.NumeroEtiqueta}'.";
          }
        }
      }

      return null; // ✅ Todo bien
    }




    [HttpPost]
    public IActionResult RegistroActivos(Activos model)
    {
      string estadoFormulario = Request.Form["EstadoFormulario"].ToString() ?? "Insertar";

      // ========= NUEVO: Normalizar números que vienen del form =========
      // Acepta "12.34" o "12,34" y los convierte a decimal usando InvariantCulture
      {
        if (Request.Form.TryGetValue("Costo", out var costoVals))
        {
          var costoRaw = costoVals.ToString();               // StringValues -> string
          var normCosto = costoRaw.Replace(',', '.');        // acepta que el usuario ponga coma
          if (decimal.TryParse(normCosto, NumberStyles.AllowDecimalPoint,
                               CultureInfo.InvariantCulture, out var costoDec))
          {
            model.Costo = costoDec;                        // tu modelo ya es decimal?
          }
        }


        if (Request.Form.TryGetValue("ValorResidual", out var vrVals))
        {
          var vrRaw = vrVals.ToString();                     // StringValues -> string
          var normVr = vrRaw.Replace(',', '.');
          if (decimal.TryParse(normVr, NumberStyles.AllowDecimalPoint,
                               CultureInfo.InvariantCulture, out var vrDec))
          {
            // Si puedes, cambia el modelo a decimal? también.
            model.ValorResidual = (double)vrDec;
          }
        }
      }
      // ========= FIN NUEVO =========

      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

      try
      {
        using (var connection = new SqlConnection(connectionString))
        {
          string query;

          if (estadoFormulario == "Editar")
          {
            query = @"UPDATE ActivosSignusID SET
                NUMERO_ACTIVO = @NUMERO_ACTIVO,
                NUMERO_ETIQUETA = @NUMERO_ETIQUETA,
                DESCRIPCION_CORTA = @DESCRIPCION_CORTA,
                DESCRIPCION_LARGA = @DESCRIPCION_LARGA,
                CATEGORIA = @CATEGORIA,
                ESTADO = @ESTADO,
                EMPRESA = @EMPRESA,
                MARCA = @MARCA,
                MODELO = @MODELO,
                NUMERO_SERIE = @NUMERO_SERIE,
                COSTO = @COSTO,
                NUMERO_FACTURA = @NUMERO_FACTURA,
                FECHA_COMPRA = @FECHA_COMPRA,
                FECHA_CAPITALIZACION = @FECHA_CAPITALIZACION,
                VALOR_RESIDUAL = @VALOR_RESIDUAL,
                DOCUMENTO = @DOCUMENTO,
                FOTOS = @FOTOS,
                NUMERO_PARTE_FABRICANTE = @NUMERO_PARTE_FABRICANTE,
                DEPRECIADO = @DEPRECIADO,
                DESCRIPCION_DEPRECIADO = @DESCRIPCION_DEPRECIADO,
                ANOS_VIDA_UTIL = @ANOS_VIDA_UTIL,
                CUENTA_CONTABLE_DEPRESIACION = @CUENTA_CONTABLE_DEPRESIACION,
                CENTRO_COSTOS = @CENTRO_COSTOS,
                DESCRIPCION_ESTADO_ULTIMO_INVENTARIO = @DESCRIPCION_ESTADO_ULTIMO_INVENTARIO,
                TAG_EPC = @TAG_EPC,
                EMPLEADO = @EMPLEADO,
                UBICACION_A = @UBICACION_A,
                UBICACION_B = @UBICACION_B,
                UBICACION_C = @UBICACION_C,
                UBICACION_D = @UBICACION_D,
                UBICACION_SECUNDARIA = @UBICACION_SECUNDARIA,
                FECHA_GARANTIA = @FECHA_GARANTIA,
                COLOR = @COLOR,
                TAMANIO_MEDIDA = @TAMANIO_MEDIDA,
                OBSERVACIONES = @OBSERVACIONES,
                ESTADO_ACTIVO = @ESTADO_ACTIVO
            WHERE ID_ACTIVO = @ID_ACTIVO;";
          }
          else
          {
            query = @"INSERT INTO ActivosSignusID (
                ID_ACTIVO, NUMERO_ACTIVO, NUMERO_ETIQUETA, DESCRIPCION_CORTA, DESCRIPCION_LARGA, 
                CATEGORIA, ESTADO, EMPRESA, MARCA, MODELO, NUMERO_SERIE, COSTO, NUMERO_FACTURA, 
                FECHA_COMPRA, FECHA_CAPITALIZACION, VALOR_RESIDUAL, DOCUMENTO, FOTOS, 
                NUMERO_PARTE_FABRICANTE, DEPRECIADO, DESCRIPCION_DEPRECIADO, ANOS_VIDA_UTIL, 
                CUENTA_CONTABLE_DEPRESIACION, CENTRO_COSTOS, DESCRIPCION_ESTADO_ULTIMO_INVENTARIO, 
                TAG_EPC, EMPLEADO, UBICACION_A, UBICACION_B, UBICACION_C, UBICACION_D, 
                UBICACION_SECUNDARIA, FECHA_GARANTIA, COLOR, TAMANIO_MEDIDA, OBSERVACIONES, ESTADO_ACTIVO
            ) VALUES (
                @ID_ACTIVO, @NUMERO_ACTIVO, @NUMERO_ETIQUETA, @DESCRIPCION_CORTA, @DESCRIPCION_LARGA, 
                @CATEGORIA, @ESTADO, @EMPRESA, @MARCA, @MODELO, @NUMERO_SERIE, @COSTO, @NUMERO_FACTURA, 
                @FECHA_COMPRA, @FECHA_CAPITALIZACION, @VALOR_RESIDUAL, @DOCUMENTO, @FOTOS, 
                @NUMERO_PARTE_FABRICANTE, @DEPRECIADO, @DESCRIPCION_DEPRECIADO, @ANOS_VIDA_UTIL, 
                @CUENTA_CONTABLE_DEPRESIACION, @CENTRO_COSTOS, @DESCRIPCION_ESTADO_ULTIMO_INVENTARIO, 
                @TAG_EPC, @EMPLEADO, @UBICACION_A, @UBICACION_B, @UBICACION_C, @UBICACION_D, 
                @UBICACION_SECUNDARIA, @FECHA_GARANTIA, @COLOR, @TAMANIO_MEDIDA, @OBSERVACIONES, @ESTADO_ACTIVO
            );";

            if (model.IdActivo == Guid.Empty)
              model.IdActivo = Guid.NewGuid();
          }

          using (var command = new SqlCommand(query, connection))
          {
            connection.Open();

            string? mensajeError = ValidarDatosActivo(model, estadoFormulario, connection);
            if (mensajeError != null)
            {
              TempData["Alert"] = JsonSerializer.Serialize(new AlertMessage
              {
                Tipo = "error",
                Mensaje = mensajeError
              });
              return RedirectToAction("RegistroEdicionActivos");
            }

            // Reutilizá este método para evitar repetir código
            AddParametrosActivos(command, model);

            command.ExecuteNonQuery();
          }

          // Éxito
          var mensaje = (estadoFormulario == "Editar")
              ? "Activo actualizado correctamente."
              : "Activo registrado exitosamente.";

          TempData["Alert"] = JsonSerializer.Serialize(new AlertMessage
          {
            Tipo = "success",
            Mensaje = mensaje
          });

          return RedirectToAction("RegistroEdicionActivos");
        }
      }
      catch (Exception ex)
      {
        TempData["Alert"] = JsonSerializer.Serialize(new AlertMessage
        {
          Tipo = "error",
          Mensaje = "Ocurrió un error al guardar el activo: " + ex.Message
        });

        return RedirectToAction("RegistroEdicionActivos");
      }
    }
    private void AddParametrosActivos(SqlCommand command, Activos model)
    {
      command.Parameters.AddWithValue("@ID_ACTIVO", model.IdActivo);
      command.Parameters.AddWithValue("@NUMERO_ACTIVO", (object?)model.NumeroActivo ?? DBNull.Value);
      command.Parameters.AddWithValue("@NUMERO_ETIQUETA", (object?)model.NumeroEtiqueta ?? DBNull.Value);
      command.Parameters.AddWithValue("@DESCRIPCION_CORTA", (object?)model.DescripcionCorta ?? DBNull.Value);
      command.Parameters.AddWithValue("@DESCRIPCION_LARGA", (object?)model.DescripcionLarga ?? DBNull.Value);
      command.Parameters.AddWithValue("@CATEGORIA", model.Categoria ?? Guid.Empty);
      command.Parameters.AddWithValue("@ESTADO", model.Estado ?? Guid.Empty);
      command.Parameters.AddWithValue("@EMPRESA", model.Empresa ?? Guid.Empty);
      command.Parameters.AddWithValue("@MARCA", model.Marca ?? Guid.Empty);
      command.Parameters.AddWithValue("@MODELO", model.Modelo ?? Guid.Empty);
      command.Parameters.AddWithValue("@DOCUMENTO", model.Documento ?? Guid.Empty);
      command.Parameters.AddWithValue("@FOTOS", model.Fotos ?? Guid.Empty);
      command.Parameters.AddWithValue("@CUENTA_CONTABLE_DEPRESIACION", model.CuentaContableDepresiacion ?? Guid.Empty);
      command.Parameters.AddWithValue("@CENTRO_COSTOS", model.CentroCostos ?? Guid.Empty);
      command.Parameters.AddWithValue("@EMPLEADO", model.Empleado ?? Guid.Empty);
      command.Parameters.AddWithValue("@UBICACION_A", model.UbicacionA ?? Guid.Empty);
      command.Parameters.AddWithValue("@UBICACION_B", model.UbicacionB ?? Guid.Empty);
      command.Parameters.AddWithValue("@UBICACION_C", model.UbicacionC ?? Guid.Empty);
      command.Parameters.AddWithValue("@UBICACION_D", model.UbicacionD ?? Guid.Empty);
      command.Parameters.AddWithValue("@UBICACION_SECUNDARIA", model.UbicacionSecundaria ?? Guid.Empty);
      command.Parameters.AddWithValue("@NUMERO_SERIE", (object?)model.NumeroSerie ?? DBNull.Value);

      // ======= CAMBIO: parámetros DECIMALES tipados =======
      // COSTO -> DECIMAL(18,2)
      {
        var p = command.Parameters.Add("@COSTO", SqlDbType.Decimal);
        p.Precision = 18; p.Scale = 2;
        p.Value = (object?)model.Costo ?? DBNull.Value;
      }

      command.Parameters.AddWithValue("@NUMERO_FACTURA", (object?)model.NumeroFactura ?? DBNull.Value);
      command.Parameters.AddWithValue("@FECHA_COMPRA", (object?)model.FechaCompra ?? DBNull.Value);
      command.Parameters.AddWithValue("@FECHA_CAPITALIZACION", DbDateOrNull(model.FechaCapitalizacion));

      // VALOR_RESIDUAL -> también como DECIMAL(18,2) (si tu columna sigue siendo FLOAT, SQL hace conversión implícita)
      {
        var p = command.Parameters.Add("@VALOR_RESIDUAL", SqlDbType.Decimal);
        p.Precision = 18; p.Scale = 2;
        if (model.ValorResidual.HasValue)
          p.Value = Convert.ToDecimal(model.ValorResidual.Value);
        else
          p.Value = DBNull.Value;
      }
      // ======= FIN CAMBIO =======

      command.Parameters.AddWithValue("@NUMERO_PARTE_FABRICANTE", (object?)model.NumeroParteFabricante ?? DBNull.Value);
      command.Parameters.AddWithValue("@DEPRECIADO", (object?)model.Depreciado ?? DBNull.Value);
      command.Parameters.AddWithValue("@DESCRIPCION_DEPRECIADO", (object?)model.DescripcionDepreciado ?? DBNull.Value);
      command.Parameters.AddWithValue("@ANOS_VIDA_UTIL", (object?)model.AnosVidaUtil ?? DBNull.Value);
      command.Parameters.AddWithValue("@DESCRIPCION_ESTADO_ULTIMO_INVENTARIO", (object?)model.DescripcionEstadoUltimoInventario ?? DBNull.Value);
      command.Parameters.AddWithValue("@TAG_EPC", (object?)model.TagEPC ?? DBNull.Value);
      command.Parameters.AddWithValue("@FECHA_GARANTIA", DbDateOrNull(model.FechaGarantia));
      command.Parameters.AddWithValue("@COLOR", (object?)model.Color ?? DBNull.Value);
      command.Parameters.AddWithValue("@TAMANIO_MEDIDA", (object?)model.TamanioMedida ?? DBNull.Value);
      command.Parameters.AddWithValue("@OBSERVACIONES", (object?)model.Observaciones ?? DBNull.Value);
      command.Parameters.AddWithValue("@ESTADO_ACTIVO", (object?)model.Estado_Activo ?? DBNull.Value);
    }

    private static object DbDateOrNull(DateTime? dt)
    {
      return (!dt.HasValue || dt.Value < SqlDateTime.MinValue.Value)
          ? (object)DBNull.Value
          : dt.Value;
    }

    public IActionResult CargaMasiva()
    {
      ViewBag.NombreUbicacion = GetUltimoNombreUbicacionC() ?? "Ubicación C";
      ViewBag.NombreUbicacionA = GetUltimoNombreUbicacionA() ?? "Ubicación A";
      ViewBag.NombreUbicacionB = GetUltimoNombreUbicacionB() ?? "Ubicación B";

      return View();
    }
    public IActionResult CamposAdicionales()
    {
      ViewBag.NombreUbicacion = GetUltimoNombreUbicacionC() ?? "Ubicación C";
      ViewBag.NombreUbicacionA = GetUltimoNombreUbicacionA() ?? "Ubicación A";
      ViewBag.NombreUbicacionB = GetUltimoNombreUbicacionB() ?? "Ubicación B";

      return View();
    }

    //*************************************************************************************************************
    //***************************************************Estados***************************************************
    //*************************************************************************************************************


    public List<EstadosActivos> ObtenerEstadosActivos()
    {
      var estadosActivos = new List<EstadosActivos>();

      string query = @"
            SELECT s.assetStatusSysId, s.name, s.description, 
                   (SELECT COUNT(*) FROM assets a WHERE a.assetStatusSysId = s.assetStatusSysId) AS assignatedAssets
            FROM assetStatus s
        ";

      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

      using (SqlConnection connection = new SqlConnection(connectionString))
      {
        SqlCommand command = new SqlCommand(query, connection);
        command.CommandType = CommandType.Text;

        try
        {
          connection.Open();
          SqlDataReader reader = command.ExecuteReader();

          while (reader.Read())
          {
            estadosActivos.Add(new EstadosActivos
            {
              assetStatusSysId = reader.GetGuid(0),
              name = reader.GetString(1),
              description = reader.GetString(2),
              assignatedAssets = reader.GetInt32(3)
            });
          }
          reader.Close();
        }
        catch (Exception ex)
        {
          Console.WriteLine("Error al obtener los estados de activos: " + ex.Message);
        }
      }

      return estadosActivos;
    }
    public IActionResult Estados(string search, int page = 1, int pageSize = 10, string sortColumn = "name", string sortDirection = "asc", string hasAssets = "")
    {
      // Depuración
      Console.WriteLine($"Search Query: {search}");
      // Crear y llenar una lista de EstadosActivos con datos de prueba
      var estadosActivos = ObtenerEstadosActivos();

      // Filtrar por búsqueda si hay un término proporcionado
      if (!string.IsNullOrEmpty(search))
      {
        estadosActivos = estadosActivos
            .Where(e => e.name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        e.description.Contains(search, StringComparison.OrdinalIgnoreCase))
            .ToList();
      }

      // Aplicar filtro
      if (hasAssets == "withAssets")
      {
        estadosActivos = estadosActivos.Where(e => e.assignatedAssets > 0).ToList();
      }
      else if (hasAssets == "withoutAssets")
      {
        estadosActivos = estadosActivos.Where(e => e.assignatedAssets == 0).ToList();
      }


      // Ordenar dinámicamente según la columna y la dirección
      estadosActivos = sortColumn switch
      {
        "description" => sortDirection == "asc"
            ? estadosActivos.OrderBy(e => e.description).ToList()
            : estadosActivos.OrderByDescending(e => e.description).ToList(),
        "assignatedAssets" => sortDirection == "asc"
            ? estadosActivos.OrderBy(e => e.assignatedAssets).ToList()
            : estadosActivos.OrderByDescending(e => e.assignatedAssets).ToList(),
        _ => sortDirection == "asc"
            ? estadosActivos.OrderBy(e => e.name).ToList()
            : estadosActivos.OrderByDescending(e => e.name).ToList(),
      };

      // Calcular la paginación
      var totalItems = estadosActivos.Count;
      var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
      var itemsOnPage = estadosActivos.Skip((page - 1) * pageSize).Take(pageSize).ToList();

      // Crear el modelo de paginación
      var model = new EstadosViewModel
      {
        Estados = itemsOnPage,
        CurrentPage = page,
        TotalPages = totalPages,
        search = search
      };

      // Pasar el término de búsqueda, columna y dirección actuales a la vista
      ViewBag.SearchQuery = search;
      ViewBag.SortColumn = sortColumn;
      ViewBag.SortDirection = sortDirection;
      ViewBag.Filter = hasAssets; // Pasar el filtro actual a la vista
      ViewBag.TotalEstados = ObtenerCantidadEstado();
      ViewData["Categories_Per_Page"] = pageSize;
      ViewBag.NombreUbicacion = GetUltimoNombreUbicacionC() ?? "Ubicación C";
      ViewBag.NombreUbicacionA = GetUltimoNombreUbicacionA() ?? "Ubicación A";
      ViewBag.NombreUbicacionB = GetUltimoNombreUbicacionB() ?? "Ubicación B";


      //   return View(model);
      return View("Estados", model);


    }




    public IActionResult BorrarEstado(Guid id)
    {
      var alertMessage = new AlertMessage();

      try
      {
        string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

        using (SqlConnection connection = new SqlConnection(connectionString))
        {
          connection.Open();

          string query = @"DELETE FROM assetStatus WHERE assetStatusSysId = @Id";

          using (SqlCommand command = new SqlCommand(query, connection))
          {
            command.Parameters.AddWithValue("@Id", id);
            int rowsAffected = command.ExecuteNonQuery();

            if (rowsAffected > 0)
            {
              alertMessage.Tipo = "success";
              alertMessage.Mensaje = "Estado eliminado correctamente.";
            }
            else
            {
              alertMessage.Tipo = "error";
              alertMessage.Mensaje = "No se encontró el estado para eliminar.";
            }
          }
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine("Error al borrar estado: " + ex.Message);
        alertMessage.Tipo = "error";
        alertMessage.Mensaje = "Error al borrar el estado: " + ex.Message;
      }


      TempData["Alert"] = JsonSerializer.Serialize(alertMessage);
      return RedirectToAction("Estados");
    }


    private static List<EstadosActivos> _statesList = new List<EstadosActivos>();
    private static int _nextStatusId = 1; // Contador global para los IDs



    public IActionResult BorrarEstadoBatch([FromQuery] List<Guid> estadosSeleccionados)

    {
      var alertMessage = new AlertMessage();

      if (estadosSeleccionados == null || !estadosSeleccionados.Any())
      {
        alertMessage.Tipo = "error";
        alertMessage.Mensaje = "No se seleccionaron estados para eliminar.";
        TempData["Alert"] = JsonSerializer.Serialize(alertMessage);
        return RedirectToAction("Estados");
      }

      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

      using (SqlConnection connection = new SqlConnection(connectionString))
      {
        connection.Open();
        SqlTransaction transaction = connection.BeginTransaction();

        try
        {
          foreach (var id in estadosSeleccionados)
          {
            string query = @"DELETE FROM assetStatus WHERE assetStatusSysId = @assetStatusSysId";

            using (SqlCommand command = new SqlCommand(query, connection, transaction))
            {
              command.Parameters.AddWithValue("@assetStatusSysId", id);
              command.ExecuteNonQuery();
            }
          }

          transaction.Commit();
          alertMessage.Tipo = "success";
          alertMessage.Mensaje = "Estados eliminados correctamente.";
        }
        catch (Exception ex)
        {
          transaction.Rollback();
          alertMessage.Tipo = "error";
          alertMessage.Mensaje = "Error al borrar los estados en batch: " + ex.Message;
        }
      }

      TempData["Alert"] = JsonSerializer.Serialize(alertMessage);
      return RedirectToAction("Estados");
    }


    private static List<EstadosActivos> _statesListEdit = new List<EstadosActivos>();
    private static int _nextStatusIdEdit = 1; // Contador global para los IDs

    [HttpPost]
    public IActionResult AgregarEstado(EstadosActivos model, string Context)
    {
      Guid id = model.assetStatusSysId;

      if (id != Guid.Empty)
      {
        return EditarEstado(model);


      }
      else
      {
        var alertMessage = new AlertMessage();


        string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

        try
        {
          using (SqlConnection connection = new SqlConnection(connectionString))
          {
            connection.Open();
            string query = @"INSERT INTO assetStatus (assetStatusSysId, [name], description, entryUser, entryDate, updateUser, updateDate, rowGuid)
                                  VALUES (@Id, @Name, @Description, @EntryUser, @EntryDate, @UpdateUser, @UpdateDate, NEWID())";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
              // Generar un nuevo GUID para el ID
              Guid newId = Guid.NewGuid();

              command.Parameters.AddWithValue("@Id", newId);
              command.Parameters.AddWithValue("@Name", model.name);
              command.Parameters.AddWithValue("@Description", model.description);
              command.Parameters.AddWithValue("@EntryUser", new Guid()); // Cambiar con el GUID del usuario si es necesario
              command.Parameters.AddWithValue("@EntryDate", DateTime.Now);
              command.Parameters.AddWithValue("@UpdateUser", new Guid()); // Cambiar con el GUID del usuario si es necesario
              command.Parameters.AddWithValue("@UpdateDate", DateTime.Now);

              command.ExecuteNonQuery();
            }
          }

          alertMessage.Tipo = "success";
          alertMessage.Mensaje = "Estado agregado correctamente.";
        }
        catch (Exception ex)
        {
          alertMessage.Tipo = "error";
          alertMessage.Mensaje = "Error al agregar el estado: " + ex.Message;
        }



        // Redirigir con el mensaje de alerta
        TempData["Alert"] = JsonSerializer.Serialize(alertMessage);

        return RedirectToAction("Estados", "Administration");



      }

    }

    [HttpPost]
    public IActionResult RegistroTomaFisica(TomasFisicas model)
    {
      var alertMessage = new AlertMessage();

      string connectionString = System.Configuration.ConfigurationManager
          .ConnectionStrings["ServerDiverscan"].ConnectionString;

      try
      {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
          connection.Open();

          string query = @"
                INSERT INTO TomaFisicaSignus (
                    tomaFisicaId,
                    fechaInicial,
                    fechaFinal,
                    nombre,
                    descripcion,
                    categoria,
                    usuarioAsignado,
                    ubicacionA,
                    ubicacionB,
                    unidadOrganizativa,
                    estadoActivo,
                    ubicacionC,
                    ubicacionD
                  
                ) VALUES (
                    @tomaFisicaId,
                    @fechaInicial,
                    @fechaFinal,
                    @nombre,
                    @descripcion,
                    @categoria,
                    @usuarioAsignado,
                    @ubicacionA,
                    @ubicacionB,
                    @unidadOrganizativa,
                    @estadoActivo,
                    @ubicacionC,
                    @ubicacionD
                );";

          using (SqlCommand command = new SqlCommand(query, connection))
          {
            Guid newId = Guid.NewGuid();

            command.Parameters.AddWithValue("@tomaFisicaId", newId);
            command.Parameters.AddWithValue("@fechaInicial", model.FechaInicial ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@fechaFinal", model.FechaFinal ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@nombre", model.NombreTomaFisica ?? "");
            command.Parameters.AddWithValue("@descripcion", model.descripcionTomaFisica ?? "");
            command.Parameters.AddWithValue("@categoria", model.CategoriaTomaFisica);
            command.Parameters.AddWithValue("@usuarioAsignado", model.UsuarioAsignadoTomaFisica);
            command.Parameters.AddWithValue("@ubicacionA", model.UbicacionATomaFisica == null || model.UbicacionATomaFisica == Guid.Empty ? Guid.Empty : model.UbicacionATomaFisica);

            command.Parameters.AddWithValue("@ubicacionB", model.UbicacionBTomaFisica == null || model.UbicacionBTomaFisica == Guid.Empty ? Guid.Empty : model.UbicacionBTomaFisica);
            command.Parameters.AddWithValue("@unidadOrganizativa", model.UnidadOrganizativaTomaFisica == null || model.UnidadOrganizativaTomaFisica == Guid.Empty ? Guid.Empty : model.UnidadOrganizativaTomaFisica);
            command.Parameters.AddWithValue("@estadoActivo", model.EstadoActivo);

            command.Parameters.AddWithValue("@ubicacionC", model.UbicacionCTomaFisica == null || model.UbicacionCTomaFisica == Guid.Empty ? Guid.Empty : model.UbicacionCTomaFisica);
            command.Parameters.AddWithValue("@ubicacionD", model.UbicacionDTomaFisica == null || model.UbicacionDTomaFisica == Guid.Empty ? Guid.Empty : model.UbicacionDTomaFisica);

            command.ExecuteNonQuery();
          }
        }

        alertMessage.Tipo = "success";
        alertMessage.Mensaje = "Datos registrados correctamente.";
      }
      catch (Exception ex)
      {
        alertMessage.Tipo = "error";
        alertMessage.Mensaje = "Error al registrar los datos: " + ex.Message;
      }

      TempData["Alert"] = JsonSerializer.Serialize(alertMessage);

      return RedirectToAction("CrearTomaFisica");
    }

    [HttpPost]
    public IActionResult RegistroGestionServicios(GestionServicios model)
    {
      string estadoFormulario = Request.Form["estadoFormulario"].ToString() ?? "Insertar";
      var alertMessage = new AlertMessage();

      string connectionString = System.Configuration.ConfigurationManager
          .ConnectionStrings["ServerDiverscan"].ConnectionString;

      try
      {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
          connection.Open();

          string query;
          int numeroTicket = 0;

          if (estadoFormulario == "Insertar")
          {
            query = @"
                INSERT INTO GestionServicios (
                    gestionServiciosId,
                    solicitante,
                    activo,
                    razonServicio,
                    estadoActivo,
                    asignarIncidente,
                    fechaEstimadaCierre,
                    descripcion
                    
                ) VALUES (
                    @gestionServiciosId,
                    @solicitante,
                    @activo,
                    @razonServicio,
                    @estadoActivo,
                    @asignarIncidente,
                    @fechaEstimadaCierre,
                    @descripcion
                    
                );";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
              Guid newId = Guid.NewGuid();
              command.Parameters.AddWithValue("@gestionServiciosId", newId);
              command.Parameters.AddWithValue("@solicitante", model.Solicitante);
              command.Parameters.AddWithValue("@activo", model.Activo);
              command.Parameters.AddWithValue("@razonServicio", model.RazonServicio);
              command.Parameters.AddWithValue("@estadoActivo", model.EstadoActivo);
              command.Parameters.AddWithValue("@asignarIncidente", model.AsignarIncidente);
              command.Parameters.AddWithValue("@fechaEstimadaCierre", model.FechaEstimadaCierre);
              command.Parameters.AddWithValue("@descripcion", model.Descripcion);
              command.ExecuteNonQuery();

         

              using (SqlCommand cmdTicket = new SqlCommand("SELECT numeroTicket FROM GestionServicios WHERE gestionServiciosId = @id", connection))
              {
                cmdTicket.Parameters.AddWithValue("@id", newId);
                object result = cmdTicket.ExecuteScalar();
                if (result != null)
                  numeroTicket = Convert.ToInt32(result);
              }

            }

            // Obtener email del solicitante
            string emailSolicitante = "";
            string nombreSolicitante = "";
            string nombreActivo = "";
            string emailAsignado = "";
            string nombreAsignado = "";

            string fechaCierreStr;
            if (model.FechaEstimadaCierre != DateTime.MinValue)
            {
              fechaCierreStr = model.FechaEstimadaCierre.ToString("dd/MM/yyyy");
            }
            else
            {
              fechaCierreStr = "Aún no tiene fecha de cierre asignada";
            }

            string nombreEstado = "";
            using (SqlCommand cmdEstado = new SqlCommand("SELECT nombre FROM EstadoActivo WHERE id_estadoActivo = @id", connection))
            {
              cmdEstado.Parameters.AddWithValue("@id", model.EstadoActivo);
              object result = cmdEstado.ExecuteScalar();
              if (result != null)
                nombreEstado = result.ToString();
            }


            string numeroPlaca = "";
            using (SqlCommand cmdPlaca = new SqlCommand("SELECT NUMERO_ACTIVO FROM ActivosSignusID WHERE ID_ACTIVO = @id", connection))
            {
              cmdPlaca.Parameters.AddWithValue("@id", model.Activo);
              object result = cmdPlaca.ExecuteScalar();
              if (result != null && result != DBNull.Value)
                numeroPlaca = result.ToString();
            }


            using (SqlCommand cmdEmail = new SqlCommand("SELECT email, name FROM employees WHERE employeeSysId = @id", connection))
            {
              cmdEmail.Parameters.AddWithValue("@id", model.Solicitante);
              using (var reader = cmdEmail.ExecuteReader())
              {
                if (reader.Read())
                {
                  emailSolicitante = reader.GetString(0);
                  nombreSolicitante = reader.GetString(1);
                }
              }
            }

            using (SqlCommand cmdEmaiAsignado = new SqlCommand("SELECT email, username FROM users WHERE userSysId = @id", connection))
            {
              cmdEmaiAsignado.Parameters.AddWithValue("@id", model.AsignarIncidente);
              using (var reader = cmdEmaiAsignado.ExecuteReader())
              {
                if (reader.Read())
                {
                  emailAsignado = reader.GetString(0);
                  nombreAsignado = reader.GetString(1);
                }
              }
            }


            // Obtener nombre del activo
            using (SqlCommand cmdActivo = new SqlCommand("SELECT DESCRIPCION_LARGA FROM ActivosSignusID WHERE ID_ACTIVO = @id", connection))
            {
              cmdActivo.Parameters.AddWithValue("@id", model.Activo);
              object result = cmdActivo.ExecuteScalar();
              if (result != null)
                nombreActivo = result.ToString();
            }
            // Obtener nombre de la razón del servicio
            string nombreRazon = "";
            using (SqlCommand cmdRazon = new SqlCommand("SELECT nombre FROM RazonServicios WHERE id_razonServicios = @id", connection))
            {
              cmdRazon.Parameters.AddWithValue("@id", model.RazonServicio);
              object result = cmdRazon.ExecuteScalar();
              if (result != null)
                nombreRazon = result.ToString();
            }


            // Enviar correo al solicitante
            if (!string.IsNullOrEmpty(emailSolicitante))
            {
              EnviarCorreoNotificacionSolicitante(emailSolicitante, nombreSolicitante, nombreActivo, nombreRazon, nombreAsignado, numeroPlaca, nombreEstado, fechaCierreStr, numeroTicket, false);
            }

            if (!string.IsNullOrEmpty(emailAsignado))
            {
              EnviarCorreoNotificacionAsignado(emailAsignado, nombreAsignado, nombreSolicitante, nombreActivo, nombreRazon, numeroPlaca, nombreEstado, fechaCierreStr, numeroTicket, false);
            }



          }
          else // Editar
          {
            query = @"
                UPDATE GestionServicios
                SET
                    solicitante = @solicitante,
                    activo = @activo,
                    razonServicio = @razonServicio,
                    estadoActivo = @estadoActivo,
                    asignarIncidente = @asignarIncidente,
                    fechaEstimadaCierre = @fechaEstimadaCierre,
                    descripcion = @descripcion
                WHERE gestionServiciosId = @gestionServiciosId;";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
              command.Parameters.AddWithValue("@gestionServiciosId", model.GestionServiciosId); // Solo se usa lo que viene del form
              command.Parameters.AddWithValue("@solicitante", model.Solicitante);
              command.Parameters.AddWithValue("@activo", model.Activo);
              command.Parameters.AddWithValue("@razonServicio", model.RazonServicio);
              command.Parameters.AddWithValue("@estadoActivo", model.EstadoActivo);
              command.Parameters.AddWithValue("@asignarIncidente", model.AsignarIncidente);
              command.Parameters.AddWithValue("@fechaEstimadaCierre", model.FechaEstimadaCierre);
              command.Parameters.AddWithValue("@descripcion", model.Descripcion);
              command.ExecuteNonQuery();
            }

            using (SqlCommand cmdTicket = new SqlCommand("SELECT numeroTicket FROM GestionServicios WHERE gestionServiciosId = @id", connection))
            {
              cmdTicket.Parameters.AddWithValue("@id", model.GestionServiciosId);
              object result = cmdTicket.ExecuteScalar();
              if (result != null)
                numeroTicket = Convert.ToInt32(result);
            }

            // Obtener email del solicitante
            string emailSolicitante = "";
            string nombreSolicitante = "";
            string nombreActivo = "";
            string emailAsignado = "";
            string nombreAsignado = "";

            string fechaCierreStr;
            if (model.FechaEstimadaCierre != DateTime.MinValue)
            {
              fechaCierreStr = model.FechaEstimadaCierre.ToString("dd/MM/yyyy");
            }
            else
            {
              fechaCierreStr = "Aún no tiene fecha de cierre asignada";
            }

            string nombreEstado = "";
            using (SqlCommand cmdEstado = new SqlCommand("SELECT nombre FROM EstadoActivo WHERE id_estadoActivo = @id", connection))
            {
              cmdEstado.Parameters.AddWithValue("@id", model.EstadoActivo);
              object result = cmdEstado.ExecuteScalar();
              if (result != null)
                nombreEstado = result.ToString();
            }

            string numeroPlaca = "";
            using (SqlCommand cmdPlaca = new SqlCommand("SELECT NUMERO_ACTIVO FROM ActivosSignusID WHERE ID_ACTIVO = @id", connection))
            {
              cmdPlaca.Parameters.AddWithValue("@id", model.Activo);
              object result = cmdPlaca.ExecuteScalar();
              if (result != null && result != DBNull.Value)
                numeroPlaca = result.ToString();
            }

            using (SqlCommand cmdEmail = new SqlCommand("SELECT email, name FROM employees WHERE employeeSysId = @id", connection))
            {
              cmdEmail.Parameters.AddWithValue("@id", model.Solicitante);
              using (var reader = cmdEmail.ExecuteReader())
              {
                if (reader.Read())
                {
                  emailSolicitante = reader.GetString(0);
                  nombreSolicitante = reader.GetString(1);
                }
              }
            }

            using (SqlCommand cmdEmaiAsignado = new SqlCommand("SELECT email, username FROM users WHERE userSysId = @id", connection))
            {
              cmdEmaiAsignado.Parameters.AddWithValue("@id", model.AsignarIncidente);
              using (var reader = cmdEmaiAsignado.ExecuteReader())
              {
                if (reader.Read())
                {
                  emailAsignado = reader.GetString(0);
                  nombreAsignado = reader.GetString(1);
                }
              }
            }

            // Obtener nombre del activo
            using (SqlCommand cmdActivo = new SqlCommand("SELECT DESCRIPCION_LARGA FROM ActivosSignusID WHERE ID_ACTIVO = @id", connection))
            {
              cmdActivo.Parameters.AddWithValue("@id", model.Activo);
              object result = cmdActivo.ExecuteScalar();
              if (result != null)
                nombreActivo = result.ToString();
            }

            // Obtener nombre de la razón del servicio
            string nombreRazon = "";
            using (SqlCommand cmdRazon = new SqlCommand("SELECT nombre FROM RazonServicios WHERE id_razonServicios = @id", connection))
            {
              cmdRazon.Parameters.AddWithValue("@id", model.RazonServicio);
              object result = cmdRazon.ExecuteScalar();
              if (result != null)
                nombreRazon = result.ToString();
            }

            // Enviar correo al solicitante
            if (!string.IsNullOrEmpty(emailSolicitante))
            {
              EnviarCorreoNotificacionSolicitante(emailSolicitante, nombreSolicitante, nombreActivo, nombreRazon, nombreAsignado, numeroPlaca, nombreEstado, fechaCierreStr, numeroTicket,true);
            }

            if (!string.IsNullOrEmpty(emailAsignado))
            {
              EnviarCorreoNotificacionAsignado(emailAsignado, nombreAsignado, nombreSolicitante, nombreActivo, nombreRazon, numeroPlaca, nombreEstado, fechaCierreStr, numeroTicket, true);
            }


          }

          alertMessage.Tipo = "success";
          alertMessage.Mensaje = (estadoFormulario == "Insertar")
              ? "Datos registrados correctamente."
              : "Datos actualizados correctamente.";
        }
      }
      catch (Exception ex)
      {
        alertMessage.Tipo = "error";
        alertMessage.Mensaje = "Error al procesar los datos: " + ex.Message;
      }


      TempData["Alert"] = JsonSerializer.Serialize(alertMessage);

      return RedirectToAction("GestionServicios");
    }

    private void EnviarCorreoNotificacionSolicitante(
        string destinatario,
        string nombreSolicitante,
        string nombreActivo,
        string razon,
        string nombreAsignado,
        string numeroPlaca,
        string nombreEstado,
        string fechaCierreStr,
        int numeroTicket,
        bool esEdicion)
    {
      try
      {
        // Validaciones básicas
        if (string.IsNullOrWhiteSpace(destinatario))
        {
          TempData["AlertCorreo"] = JsonSerializer.Serialize(new AlertMessage
          {
            Tipo = "error",
            Mensaje = "No se pudo enviar el correo al solicitante: no tiene un correo registrado."
          });
          return;
        }
        _ = new MailAddress(destinatario); // Lanza FormatException si el correo es inválido

        // SMTP de Signus
        const string host = "mail.signusid.com";
        const string smtpUser = "noreply@signusid.com";
        const string smtpPass = "Smartcosta2025$"; // Mover a config idealmente

        // Asunto y contenidos
        string asunto = esEdicion
          ? $"Ticket #{numeroTicket} - Revisión de activo actualizada"
          : $"Ticket #{numeroTicket} - Revisión de activo asignada";

        string cuerpoTxt =
    $@"Estimado/a {nombreSolicitante},

Se {(esEdicion ? "ha actualizado" : "ha registrado")} una solicitud de revisión para el activo {nombreActivo}.

Placa del activo: {numeroPlaca}
Razón del servicio: {razon}
Estado del incidente: {nombreEstado}
Asignado a: {nombreAsignado}
Fecha estimada de cierre: {fechaCierreStr}

Gracias por usar nuestro sistema.";

        string cuerpoHtml = $@"
<div style='font-family: Arial, sans-serif; color: #333; font-size: 14px;'>
  <p>Estimado/a <strong>{nombreSolicitante}</strong>,</p>
  <p>Se {(esEdicion ? "ha actualizado" : "ha registrado")} una solicitud de revisión para el activo <strong>{nombreActivo}</strong>.</p>
  <table style='width: 100%; border-collapse: collapse; margin-top: 15px;'>
    <tr><td style='padding:6px 0;'><strong>Placa del activo:</strong></td><td style='padding:6px 0;'>{numeroPlaca}</td></tr>
    <tr><td style='padding:6px 0;'><strong>Razón del servicio:</strong></td><td style='padding:6px 0;'>{razon}</td></tr>
    <tr><td style='padding:6px 0;'><strong>Estado del incidente:</strong></td><td style='padding:6px 0;'>{nombreEstado}</td></tr>
    <tr><td style='padding:6px 0;'><strong>Asignado a:</strong></td><td style='padding:6px 0;'>{nombreAsignado}</td></tr>
    <tr><td style='padding:6px 0;'><strong>Fecha estimada de cierre:</strong></td><td style='padding:6px 0;'>{fechaCierreStr}</td></tr>
  </table>
  <p style='margin-top: 20px;'>Gracias por usar nuestro sistema.</p>
</div>";

        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

        using (var mail = new MailMessage())
        {
          mail.From = new MailAddress(smtpUser, "SIGNUSID Notificaciones");
          mail.To.Add(destinatario);
          mail.Subject = asunto;
          mail.SubjectEncoding = Encoding.UTF8;

          // Si quieres que respondan a otra casilla:
          // mail.ReplyToList.Add(new MailAddress("soporte@signusid.com"));

          // Alternativas (texto + HTML) para mejor entregabilidad
          mail.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(cuerpoTxt, Encoding.UTF8, "text/plain"));
          mail.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(cuerpoHtml, Encoding.UTF8, "text/html"));

          // Intento 1: 587 (STARTTLS)
          try
          {
            using (var smtp = new SmtpClient(host, 587))
            {
              smtp.EnableSsl = true; // en 587 hace STARTTLS
              smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
              smtp.UseDefaultCredentials = false;
              smtp.Credentials = new NetworkCredential(smtpUser, smtpPass);
              smtp.Timeout = 15000;
              smtp.Send(mail);
            }
          }
          catch (SmtpException ex587)
          {
            // Intento 2: 465 (SSL implícito)
            try
            {
              using (var smtp = new SmtpClient(host, 465))
              {
                smtp.EnableSsl = true; // SSL implícito
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential(smtpUser, smtpPass);
                smtp.Timeout = 15000;
                smtp.Send(mail);
              }
            }
            catch (Exception ex465)
            {
              throw new Exception($"Fallo SMTP en 587 y 465. 587: {ex587.Message} | 465: {ex465.Message}", ex465);
            }
          }
        }
      }
      catch (FormatException)
      {
        TempData["AlertCorreo"] = JsonSerializer.Serialize(new AlertMessage
        {
          Tipo = "error",
          Mensaje = "No se pudo enviar el correo al solicitante. Verifique que el correo del solicitante sea válido."
        });
      }
      catch (SmtpException smtpEx)
      {
        TempData["AlertCorreo"] = JsonSerializer.Serialize(new AlertMessage
        {
          Tipo = "error",
          Mensaje = "No se pudo enviar el correo al solicitante. Error SMTP: " + smtpEx.Message
        });
      }
      catch (Exception ex)
      {
        TempData["AlertCorreo"] = JsonSerializer.Serialize(new AlertMessage
        {
          Tipo = "error",
          Mensaje = "Ocurrió un error inesperado al enviar el correo al solicitante: " + ex.Message
        });
      }
    }



    private void EnviarCorreoNotificacionAsignado(
        string destinatario,
        string nombreAsignado,
        string nombreSolicitante,
        string nombreActivo,
        string razon,
        string numeroPlaca,
        string nombreEstado,
        string fechaCierreStr,
        int numeroTicket,
        bool esEdicion)
    {
      try
      {
        // Validaciones básicas
        if (string.IsNullOrWhiteSpace(destinatario))
        {
          TempData["AlertCorreoAsignado"] = JsonSerializer.Serialize(new AlertMessage
          {
            Tipo = "error",
            Mensaje = "No se pudo enviar el correo al usuario asignado. No tiene un correo registrado."
          });
          return;
        }
        _ = new MailAddress(destinatario); // lanza FormatException si es inválido

        // SMTP de Signus
        const string host = "mail.signusid.com";
        const string smtpUser = "noreply@signusid.com";
        const string smtpPass = "Smartcosta2025$"; // mueve esto a config

        // Contenidos
        string asunto = esEdicion
          ? $"Ticket #{numeroTicket} - Revisión de activo actualizada"
          : $"Ticket #{numeroTicket} - Revisión de activo asignada";

        string cuerpoTxt =
    $@"Estimado/a {nombreAsignado},

Se le ha {(esEdicion ? "actualizado" : "asignado")} la revisión del activo {nombreActivo}.

Placa del activo: {numeroPlaca}
Razón del servicio: {razon}
Estado del incidente: {nombreEstado}
Solicitado por: {nombreSolicitante}
Fecha estimada de cierre: {fechaCierreStr}

Por favor, acceda al sistema para gestionar la revisión.";

        string cuerpoHtml = $@"
<div style='font-family: Arial, sans-serif; color: #333; font-size: 14px;'>
  <p>Estimado/a <strong>{nombreAsignado}</strong>,</p>
  <p>Se le ha {(esEdicion ? "actualizado" : "asignado")} la revisión del activo <strong>{nombreActivo}</strong>.</p>
  <table style='width: 100%; border-collapse: collapse; margin-top: 15px;'>
    <tr><td style='padding:6px 0;'><strong>Placa del activo:</strong></td><td style='padding:6px 0;'>{numeroPlaca}</td></tr>
    <tr><td style='padding:6px 0;'><strong>Razón del servicio:</strong></td><td style='padding:6px 0;'>{razon}</td></tr>
    <tr><td style='padding:6px 0;'><strong>Estado del incidente:</strong></td><td style='padding:6px 0;'>{nombreEstado}</td></tr>
    <tr><td style='padding:6px 0;'><strong>Solicitado por:</strong></td><td style='padding:6px 0;'>{nombreSolicitante}</td></tr>
    <tr><td style='padding:6px 0;'><strong>Fecha estimada de cierre:</strong></td><td style='padding:6px 0;'>{fechaCierreStr}</td></tr>
  </table>
  <p style='margin-top:20px;'>Por favor, acceda al sistema para gestionar la revisión.</p>
</div>";

        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

        using (var mail = new MailMessage())
        {
          mail.From = new MailAddress(smtpUser, "SIGNUSID Notificaciones");
          mail.To.Add(destinatario);
          mail.Subject = asunto;
          mail.SubjectEncoding = Encoding.UTF8;

          // Si quieres que respondan a otra casilla:
          // mail.ReplyToList.Add(new MailAddress("soporte@signusid.com"));

          // Texto plano + HTML
          mail.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(cuerpoTxt, Encoding.UTF8, "text/plain"));
          mail.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(cuerpoHtml, Encoding.UTF8, "text/html"));

          // Intento 1: 587 (STARTTLS)
          try
          {
            using (var smtp = new SmtpClient(host, 587))
            {
              smtp.EnableSsl = true; // en 587 hace STARTTLS
              smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
              smtp.UseDefaultCredentials = false;
              smtp.Credentials = new NetworkCredential(smtpUser, smtpPass);
              smtp.Timeout = 15000;
              smtp.Send(mail);
            }
          }
          catch (SmtpException ex587)
          {
            // Intento 2: 465 (SSL implícito)
            try
            {
              using (var smtp = new SmtpClient(host, 465))
              {
                smtp.EnableSsl = true; // SSL implícito
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential(smtpUser, smtpPass);
                smtp.Timeout = 15000;
                smtp.Send(mail);
              }
            }
            catch (Exception ex465)
            {
              throw new Exception($"Fallo SMTP en 587 y 465. 587: {ex587.Message} | 465: {ex465.Message}", ex465);
            }
          }
        }
      }
      catch (FormatException)
      {
        TempData["AlertCorreoAsignado"] = JsonSerializer.Serialize(new AlertMessage
        {
          Tipo = "error",
          Mensaje = "No se pudo enviar el correo al usuario asignado. El correo no es válido."
        });
      }
      catch (SmtpException smtpEx)
      {
        TempData["AlertCorreoAsignado"] = JsonSerializer.Serialize(new AlertMessage
        {
          Tipo = "error",
          Mensaje = "Error SMTP al enviar correo al asignado: " + smtpEx.Message
        });
      }
      catch (Exception ex)
      {
        TempData["AlertCorreoAsignado"] = JsonSerializer.Serialize(new AlertMessage
        {
          Tipo = "error",
          Mensaje = "Ocurrió un error inesperado al enviar el correo al asignado: " + ex.Message
        });
      }
    }




    [HttpPost]
    public IActionResult RegistroIncidentesActivos(GestionServicios model)
    {
      var alertMessage = new AlertMessage();

      string connectionString = System.Configuration.ConfigurationManager
          .ConnectionStrings["ServerDiverscan"].ConnectionString;

      try
      {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
          connection.Open();

          string query = @"
                INSERT INTO GestionServicios (
                    gestionServiciosId,
                    solicitante,
                    activo,
                    razonServicio,
                    estadoActivo,
                    asignarIncidente,
                    fechaEstimadaCierre,
                    descripcion,
TelefonoMovil
                ) VALUES (
                    @gestionServiciosId,
                    @solicitante,
                    @activo,
                    @razonServicio,
                    @estadoActivo,
                    @asignarIncidente,
                    @fechaEstimadaCierre,
                    @descripcion,
@TelefonoMovil
                );";

          using (SqlCommand command = new SqlCommand(query, connection))
          {
            Guid newId = Guid.NewGuid();
            command.Parameters.AddWithValue("@gestionServiciosId", newId);
            command.Parameters.AddWithValue("@solicitante", model.Solicitante);
            command.Parameters.AddWithValue("@activo", model.Activo);
            command.Parameters.AddWithValue("@razonServicio", model.RazonServicio);
            command.Parameters.AddWithValue("@estadoActivo", model.EstadoActivo);
            command.Parameters.AddWithValue("@asignarIncidente", model.AsignarIncidente);
            command.Parameters.AddWithValue("@fechaEstimadaCierre",
    model.FechaEstimadaCierre == DateTime.MinValue ? DBNull.Value : (object)model.FechaEstimadaCierre);
            command.Parameters.AddWithValue("@descripcion", model.Descripcion);
            command.Parameters.AddWithValue("@TelefonoMovil", string.IsNullOrWhiteSpace(model.TelefonoMovil) ? DBNull.Value : (object)model.TelefonoMovil);
            command.ExecuteNonQuery();
          }

          alertMessage.Tipo = "success";
          alertMessage.Mensaje = "Datos registrados correctamente.";
        }
      }
      catch (Exception ex)
      {
        alertMessage.Tipo = "error";
        alertMessage.Mensaje = "Error al procesar los datos: " + ex.Message;
      }

      TempData["Alert"] = JsonSerializer.Serialize(alertMessage);

      return RedirectToAction("IncidentesActivos");
    }



    [HttpPost]
    public IActionResult EditarEstado(EstadosActivos model)
    {
      var alertMessage = new AlertMessage();

      //if (ModelState.IsValid)
      //{
      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

      try
      {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
          connection.Open();
          string query = @"UPDATE assetStatus 
                                 SET [name] = @Name,
                                     description = @Description,
                                     updateUser = @UpdateUser,
                                     updateDate = @UpdateDate
                                 WHERE assetStatusSysId = @assetStatusSysId";

          using (SqlCommand command = new SqlCommand(query, connection))
          {
            command.Parameters.AddWithValue("@assetStatusSysId", model.assetStatusSysId);
            command.Parameters.AddWithValue("@Name", model.name);
            command.Parameters.AddWithValue("@Description", model.description);
            command.Parameters.AddWithValue("@UpdateUser", new Guid()); // Reemplaza con el ID real del usuario
            command.Parameters.AddWithValue("@UpdateDate", DateTime.Now);

            int rowsAffected = command.ExecuteNonQuery();

            if (rowsAffected > 0)
            {
              alertMessage.Tipo = "success";
              alertMessage.Mensaje = "Estado actualizado correctamente.";
            }
            else
            {
              alertMessage.Tipo = "warning";
              alertMessage.Mensaje = "No se encontraron cambios para actualizar.";
            }
          }
        }
      }
      catch (Exception ex)
      {
        alertMessage.Tipo = "error";
        alertMessage.Mensaje = "Error al actualizar el estado: " + ex.Message;
      }
      //}
      //else
      //{
      //  alertMessage.Tipo = "error";
      //  alertMessage.Mensaje = "Datos no válidos. Por favor, verifica los campos.";
      //}

      // Redirigir con el mensaje de alerta
      TempData["Alert"] = JsonSerializer.Serialize(alertMessage);
      Console.WriteLine("PathBase: " + Request.PathBase);
      // O mira en debugger: var pathBase = Request.PathBase

      return Redirect(Url.Action("Estados", "Administration")!);


    }

    [HttpPost]
    public async Task<IActionResult> SincronizarEstados(IFormFile excelFile)
    {
      if (excelFile == null || excelFile.Length == 0)
      {
        TempData["Alert2"] = "Por favor seleccione un archivo válido.";
        return RedirectToAction("Estados");
      }

      try
      {
        // Guardar el archivo temporalmente
        var fileName = Path.GetFileName(excelFile.FileName);
        var tempPath = Path.Combine(Path.GetTempPath(), fileName);

        using (var stream = new FileStream(tempPath, FileMode.Create))
        {
          await excelFile.CopyToAsync(stream);
        }
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        using (var package = new ExcelPackage(new FileInfo(tempPath)))
        {
          var sheet = package.Workbook.Worksheets.FirstOrDefault(s => s.Name.ToLower().Contains("estado"));

          if (sheet == null)
          {
            TempData["Alert2"] = "No se encontró la hoja de estados en el archivo.";
            return RedirectToAction("Estados");
          }

          int totalCols = sheet.Dimension.End.Column;
          int totalRows = sheet.Dimension.End.Row;

          for (int row = 3; row <= totalRows; row++)
          {
            var rowData = new List<string>();
            bool hasData = false;

            for (int col = 1; col <= totalCols; col++)
            {
              string value = sheet.Cells[row, col].Value?.ToString() ?? "";
              rowData.Add(value);

              if (!string.IsNullOrWhiteSpace(value))
                hasData = true;
            }

            if (hasData)
              await SaveDataToDatabaseAsyncEstados(rowData);
          }

          TempData["Alert2"] = "Archivo procesado exitosamente.";
        }
      }
      catch (Exception ex)
      {
        TempData["Alert2"] = "Error al procesar el archivo: " + ex.Message;
      }

      return RedirectToAction("Estados");
    }


    private async Task SaveDataToDatabaseAsyncEstados(List<string> rowData)
    {
      // Lee la cadena de conexión desde el archivo app.config, esta cadena se puede modificar.
      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

      _currentUser user = new _currentUser();
      user.Id = Guid.NewGuid();

      using (var connection = new SqlConnection(connectionString))
      using (var command = new SqlCommand("InsertAssetStatusxExcel", connection))
      {
        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.AddWithValue("@name", rowData[0]);
        command.Parameters.AddWithValue("@description", rowData[1]);
        command.Parameters.AddWithValue("@Usuario", Guid.Empty);
        var outputParam = new SqlParameter("@Resultado", SqlDbType.VarChar, 400)
        {
          Direction = ParameterDirection.Output
        };
        command.Parameters.Add(outputParam);

        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();

        string resultado = outputParam.Value?.ToString() ?? "Sin resultado.";
        TempData["Alert2"] = resultado;
      }
    }

    public int ObtenerCantidadEstado()
    {
      int total = 0;
      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

      using (SqlConnection connection = new SqlConnection(connectionString))
      {
        string query = "SELECT COUNT(assetStatusSysId) FROM assetStatus";

        using (SqlCommand command = new SqlCommand(query, connection))
        {
          connection.Open();
          total = (int)command.ExecuteScalar();
        }
      }

      return total;
    }


    //*************************************************************************************************************
    //***************************************************FIN Estados***********************************************
    //*************************************************************************************************************


    //*************************************************************************************************************
    //***************************************************Modelos***************************************************
    //*************************************************************************************************************

    public List<ModelosActivos> ObtenerModelososActivos()
    {
      var modelosActivos = new List<ModelosActivos>();

      string query = @"
        SELECT m.modeloId, m.marcaId, m.nombre, m.descripcion, 
               ma.nombre AS marcaNombre,
               (SELECT COUNT(*) FROM ActivosNueva a WHERE a.modelo = m.modeloId) AS assignatedAssets
        FROM Modelo m
        INNER JOIN Marca ma ON m.marcaId = ma.marcaId
    ";

      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

      using (SqlConnection connection = new SqlConnection(connectionString))
      {
        SqlCommand command = new SqlCommand(query, connection);
        command.CommandType = CommandType.Text;

        try
        {
          connection.Open();
          SqlDataReader reader = command.ExecuteReader();

          while (reader.Read())
          {
            modelosActivos.Add(new ModelosActivos
            {
              modeloID = reader.GetGuid(0),
              marcaID = reader.GetGuid(1),
              name = reader.GetString(2),
              description = reader.GetString(3),
              marca = reader.GetString(4),
              assignatedAssets = reader.GetInt32(5)
            });
          }
          reader.Close();
        }
        catch (Exception ex)
        {
          Console.WriteLine("Error al obtener los modelos de activos: " + ex.Message);
        }
      }

      return modelosActivos;
    }


    public IActionResult Modelos(string search, int page = 1, int pageSize = 20, string sortColumn = "name", string sortDirection = "asc", string hasAssets = "")
    {
      var modelosActivos = ObtenerModelososActivos();
      // Filtrar por búsqueda si hay un término proporcionado
      if (!string.IsNullOrEmpty(search))
      {
        modelosActivos = modelosActivos
            .Where(e => e.name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        e.description.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        e.marca.Contains(search, StringComparison.OrdinalIgnoreCase))
            .ToList();
      }

      // Aplicar filtro
      if (hasAssets == "withAssets")
      {
        modelosActivos = modelosActivos.Where(e => e.assignatedAssets > 0).ToList();
      }
      else if (hasAssets == "withoutAssets")
      {
        modelosActivos = modelosActivos.Where(e => e.assignatedAssets == 0).ToList();
      }


      // Ordenar dinámicamente 
      modelosActivos = sortColumn switch
      {
        "description" => sortDirection == "asc"
            ? modelosActivos.OrderBy(e => e.description).ToList()
            : modelosActivos.OrderByDescending(e => e.description).ToList(),
        "assignatedAssets" => sortDirection == "asc"
            ? modelosActivos.OrderBy(e => e.assignatedAssets).ToList()
            : modelosActivos.OrderByDescending(e => e.assignatedAssets).ToList(),
        "marca" => sortDirection == "asc"
            ? modelosActivos.OrderBy(e => e.marca).ToList()
            : modelosActivos.OrderByDescending(e => e.marca).ToList(),
        _ => sortDirection == "asc"
            ? modelosActivos.OrderBy(e => e.name).ToList()
            : modelosActivos.OrderByDescending(e => e.name).ToList(),
      };


      // Calcular la paginación
      var totalItems = modelosActivos.Count;
      var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
      var itemsOnPage = modelosActivos.Skip((page - 1) * pageSize).Take(pageSize).ToList();

      // Crear el modelo de paginación
      var model = new ModelosViewModel
      {
        Modelos = itemsOnPage,
        CurrentPage = page,
        TotalPages = totalPages
      };

      // Pasar el término de búsqueda, columna y dirección actuales a la vista
      ViewBag.SearchQuery = search;
      ViewBag.SortColumn = sortColumn;
      ViewBag.SortDirection = sortDirection;
      ViewBag.Filter = hasAssets; // Pasar el filtro actual a la vista
      ViewBag.TotalModelos = ObtenerCantidadModelos();
      ViewData["Categories_Per_Page"] = pageSize;
      ViewBag.NombreUbicacion = GetUltimoNombreUbicacionC() ?? "Ubicación C";
      ViewBag.NombreUbicacionA = GetUltimoNombreUbicacionA() ?? "Ubicación A";
      ViewBag.NombreUbicacionB = GetUltimoNombreUbicacionB() ?? "Ubicación B";


      return View(model);
    }

    [HttpGet]
    public IActionResult EliminarRegistroModelo(Guid modeloID)
    {
      var alertMessage = new AlertMessage();
      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

      using (SqlConnection connection = new SqlConnection(connectionString))
      {
        try
        {
          connection.Open();

          // 🔹 **Verificar si el modelo tiene activos asociados en la tabla ActivosNueva**
          string checkQuery = "SELECT COUNT(*) FROM ActivosNueva WHERE modelo = @modeloId";
          using (SqlCommand checkCommand = new SqlCommand(checkQuery, connection))
          {
            checkCommand.Parameters.AddWithValue("@modeloId", modeloID);
            int count = (int)checkCommand.ExecuteScalar();

            if (count > 0)
            {
              alertMessage.Tipo = "warning";
              alertMessage.Mensaje = "No se puede eliminar el modelo porque tiene activos asociados.";

              TempData["Alert"] = JsonSerializer.Serialize(alertMessage);
              return RedirectToAction("Modelos");
            }
          }

          // 🔹 **Si el modelo no tiene activos, proceder con la eliminación**
          string deleteQuery = "DELETE FROM Modelo WHERE modeloId = @modeloId";
          using (SqlCommand deleteCommand = new SqlCommand(deleteQuery, connection))
          {
            deleteCommand.CommandType = CommandType.Text;
            deleteCommand.Parameters.AddWithValue("@modeloId", modeloID);

            int rowsAffected = deleteCommand.ExecuteNonQuery();

            if (rowsAffected > 0)
            {
              alertMessage.Tipo = "success";
              alertMessage.Mensaje = "Modelo eliminado correctamente.";
            }
            else
            {
              alertMessage.Tipo = "warning";
              alertMessage.Mensaje = "No se encontró el modelo con el ID especificado.";
            }
          }

          connection.Close();
        }
        catch (Exception ex)
        {
          alertMessage.Tipo = "error";
          alertMessage.Mensaje = "Error al eliminar el modelo: " + ex.Message;
        }
      }

      // 🔹 **Redirigir a la vista "Modelos" con el mensaje de alerta**
      TempData["Alert"] = JsonSerializer.Serialize(alertMessage);
      return RedirectToAction("Modelos");
    }


    int totalEliminados = 0;
    [HttpPost]
    public IActionResult EliminarRegistroBatchModelos(string modeloIDs)
    {
      var alertMessage = new AlertMessage();

      if (string.IsNullOrEmpty(modeloIDs))
      {
        alertMessage.Tipo = "warning";
        alertMessage.Mensaje = "No se seleccionaron modelos para eliminar.";
        TempData["Alert"] = JsonSerializer.Serialize(alertMessage);
        return RedirectToAction("Modelos");
      }

      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

      var modeloIDList = modeloIDs.Split(',')
                                  .Select(id => Guid.TryParse(id, out var guid) ? guid : (Guid?)null)
                                  .Where(guid => guid.HasValue)
                                  .Select(guid => guid.Value)
                                  .ToList();

      if (modeloIDList.Count == 0)
      {
        alertMessage.Tipo = "warning";
        alertMessage.Mensaje = "Los IDs proporcionados no son válidos.";
        TempData["Alert"] = JsonSerializer.Serialize(alertMessage);
        return RedirectToAction("Modelos");
      }

      using (SqlConnection connection = new SqlConnection(connectionString))
      {
        connection.Open();
        int totalEliminados = 0;
        List<Guid> modelosConActivos = new List<Guid>(); // Lista para modelos que no se pueden eliminar

        foreach (var modeloID in modeloIDList)
        {
          // 🔹 **Verificar si el modelo está en la tabla ActivosNueva**
          string checkQuery = "SELECT COUNT(*) FROM ActivosNueva WHERE modelo = @modeloId";

          using (SqlCommand checkCommand = new SqlCommand(checkQuery, connection))
          {
            checkCommand.Parameters.AddWithValue("@modeloId", modeloID);
            int count = (int)checkCommand.ExecuteScalar();

            if (count > 0)
            {
              modelosConActivos.Add(modeloID);
              continue; // No intentar eliminar este modelo
            }
          }

          // 🔹 **Si el modelo no tiene activos, proceder con la eliminación**
          string deleteQuery = "DELETE FROM Modelo WHERE modeloId = @modeloId";

          using (SqlCommand deleteCommand = new SqlCommand(deleteQuery, connection))
          {
            deleteCommand.CommandType = CommandType.Text;
            deleteCommand.Parameters.AddWithValue("@modeloId", modeloID);

            try
            {
              int rowsAffected = deleteCommand.ExecuteNonQuery();
              if (rowsAffected > 0)
              {
                totalEliminados++;
              }
            }
            catch (Exception ex)
            {
              alertMessage.Tipo = "error";
              alertMessage.Mensaje = $"Error al eliminar uno de los modelos: {ex.Message}";
              TempData["Alert"] = JsonSerializer.Serialize(alertMessage);
              return RedirectToAction("Modelos");
            }
          }
        }

        connection.Close();

        // 🔹 **Construir el mensaje de resultado**
        if (totalEliminados > 0)
        {
          alertMessage.Tipo = "success";
          alertMessage.Mensaje = $"{totalEliminados} modelos eliminados exitosamente.";
        }

        if (modelosConActivos.Count > 0)
        {
          alertMessage.Tipo = "warning";
          alertMessage.Mensaje = $"Algunos modelos no pudieron eliminarse porque tienen activos asociados.";
        }

        if (totalEliminados == 0 && modelosConActivos.Count == 0)
        {
          alertMessage.Tipo = "warning";
          alertMessage.Mensaje = "No se eliminó ningún modelo.";
        }
      }

      TempData["Alert"] = JsonSerializer.Serialize(alertMessage);
      return RedirectToAction("Modelos");
    }



    [HttpGet]
    public JsonResult GetMarca()
    {
      List<Marcas> sector = new List<Marcas>();

      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

      string query = "SELECT marcaId, nombre FROM Marca";

      using (SqlConnection connection = new SqlConnection(connectionString))
      {
        SqlCommand command = new SqlCommand(query, connection);
        connection.Open();

        using (SqlDataReader reader = command.ExecuteReader())
        {
          while (reader.Read())
          {
            sector.Add(new Marcas
            {
              MarcaId = reader.GetGuid(0),
              Nombre = reader.GetString(1)
            });
          }
        }
      }

      if (sector.Count == 0)
      {
        return Json(new { message = "No hay marcas disponibles." });
      }

      return Json(sector);

    }



    private static List<MarcaActivos> _marcaList = new List<MarcaActivos>();
    private static int _nextMarcaId = 1; // Contador global para los IDs

    [HttpPost]
    public IActionResult AgregarMarca(MarcaActivos model)
    {


      // Validar el modelo
      if (ModelState.IsValid)
      {
        model.idMarca = _nextMarcaId++; // Asigna un ID único al rol
        _marcaList.Add(model);

        // Devolver respuesta en formato JSON
        return Json(new { success = true, message = "Marca agregado exitosamente", marca = _marcaList });
      }

      return Json(new { success = false, message = "Datos inválidos" });
    }

    private static List<ModelosActivos> _modeloList = new List<ModelosActivos>();
    private static int _nextModeloId = 1; // Contador global para los IDs

    [HttpPost]
    public ActionResult AgregarModelo(string marcaID, string nombre, string descripcion)
    {
      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

      // Obtener los valores de sesión o de usuario para los campos de 'entryUser' y 'updateUser'
      Guid entryUser = Guid.NewGuid(); // Deberías obtener este valor desde la sesión o contexto del usuario logueado
      Guid updateUser = entryUser; // Puede ser el mismo usuario si no se especifica otra cosa
      DateTime currentDate = DateTime.Now;

      // Crear un nuevo modeloId de tipo Guid
      Guid modeloId = Guid.NewGuid();

      // La consulta SQL de inserción
      string query = @"
          INSERT INTO [dbo].[Modelo] (modeloId, marcaId, nombre, descripcion, entryUser, entryDate, updateUser, updateDate)
          VALUES (@modeloId, @marcaId, @nombre, @descripcion, @entryUser, @entryDate, @updateUser, @updateDate)";

      using (SqlConnection conn = new SqlConnection(connectionString))
      {
        // Crear el comando con la consulta SQL y la conexión
        SqlCommand cmd = new SqlCommand(query, conn);

        // Agregar parámetros a la consulta SQL
        cmd.Parameters.AddWithValue("@modeloId", modeloId);
        cmd.Parameters.AddWithValue("@marcaId", string.IsNullOrEmpty(marcaID) ? (object)DBNull.Value : new Guid(marcaID));
        cmd.Parameters.AddWithValue("@nombre", nombre);
        cmd.Parameters.AddWithValue("@descripcion", descripcion ?? string.Empty);
        cmd.Parameters.AddWithValue("@entryUser", entryUser);
        cmd.Parameters.AddWithValue("@entryDate", currentDate);
        cmd.Parameters.AddWithValue("@updateUser", updateUser);
        cmd.Parameters.AddWithValue("@updateDate", currentDate);

        try
        {
          // Abrir la conexión
          conn.Open();

          // Ejecutar la consulta
          int rowsAffected = cmd.ExecuteNonQuery();

          // Crear el mensaje que se pasará a la vista
          var alertMessage = new AlertMessage
          {
            Tipo = rowsAffected > 0 ? "success" : "error",
            Mensaje = rowsAffected > 0 ? "Modelo agregado exitosamente." : "Hubo un error al agregar el modelo."
          };

          // Redirigir a la vista de Modelos pasando el mensaje serializado
          TempData["Alert"] = JsonSerializer.Serialize(alertMessage);
          return RedirectToAction("Modelos");
        }
        catch (Exception ex)
        {
          // Crear el mensaje de error
          var alertMessage = new AlertMessage
          {
            Tipo = "error",
            Mensaje = "Error: " + ex.Message
          };

          // Redirigir a la vista de Modelos pasando el mensaje serializado
          TempData["Alert"] = JsonSerializer.Serialize(alertMessage);
          return RedirectToAction("Modelos");
        }
      }
    }


    //
    private static List<ModelosActivos> _modeloListEdit = new List<ModelosActivos>();
    private static int _nextModeloIdEdit = 1; // Contador global para los IDs

    [HttpPost]
    public ActionResult EditarModelo(Guid modeloId, string marcaID, string nombre, string descripcion)
    {
      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

      // Obtener los valores de sesión o de usuario para los campos de 'entryUser' y 'updateUser'
      Guid updateUser = Guid.NewGuid(); // Este debe ser el usuario que está haciendo el cambio
      DateTime currentDate = DateTime.Now;

      // La consulta SQL de actualización
      string query = @"
        UPDATE [dbo].[Modelo]
        SET marcaId = @marcaId,
            nombre = @nombre,
            descripcion = @descripcion,
            updateUser = @updateUser,
            updateDate = @updateDate
        WHERE modeloId = @modeloId";

      using (SqlConnection conn = new SqlConnection(connectionString))
      {
        // Crear el comando con la consulta SQL y la conexión
        SqlCommand cmd = new SqlCommand(query, conn);

        // Agregar parámetros a la consulta SQL
        cmd.Parameters.AddWithValue("@modeloId", modeloId);
        cmd.Parameters.AddWithValue("@marcaId", string.IsNullOrEmpty(marcaID) ? (object)DBNull.Value : new Guid(marcaID));
        cmd.Parameters.AddWithValue("@nombre", nombre);
        cmd.Parameters.AddWithValue("@descripcion", descripcion ?? string.Empty);
        cmd.Parameters.AddWithValue("@updateUser", updateUser);
        cmd.Parameters.AddWithValue("@updateDate", currentDate);

        try
        {
          // Abrir la conexión
          conn.Open();

          // Ejecutar la consulta
          int rowsAffected = cmd.ExecuteNonQuery();

          // Crear el mensaje que se pasará a la vista
          var alertMessage = new AlertMessage
          {
            Tipo = rowsAffected > 0 ? "success" : "error",
            Mensaje = rowsAffected > 0 ? "Modelo actualizado exitosamente." : "Hubo un error al actualizar el modelo."
          };

          // Redirigir a la vista de Modelos pasando el mensaje serializado
          TempData["Alert"] = JsonSerializer.Serialize(alertMessage);
          return RedirectToAction("Modelos");
        }
        catch (Exception ex)
        {
          // Crear el mensaje de error
          var alertMessage = new AlertMessage
          {
            Tipo = "error",
            Mensaje = "Error: " + ex.Message
          };

          // Redirigir a la vista de Modelos pasando el mensaje serializado
          TempData["Alert"] = JsonSerializer.Serialize(alertMessage);
          return RedirectToAction("Modelos");
        }
      }
    }


    [HttpPost]
    public async Task<IActionResult> SincronizarModelos(IFormFile excelFile)
    {
      if (excelFile == null || excelFile.Length == 0)
      {
        TempData["Alert2"] = "Por favor seleccione un archivo válido.";
        return RedirectToAction("Modelos");
      }

      try
      {
        // Guardar el archivo temporalmente
        var fileName = Path.GetFileName(excelFile.FileName);
        var tempPath = Path.Combine(Path.GetTempPath(), fileName);

        using (var stream = new FileStream(tempPath, FileMode.Create))
        {
          await excelFile.CopyToAsync(stream);
        }
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        using (var package = new ExcelPackage(new FileInfo(tempPath)))
        {
          var sheet = package.Workbook.Worksheets.FirstOrDefault(s => s.Name.ToLower().Contains("modelo"));

          if (sheet == null)
          {
            TempData["Alert2"] = "No se encontró la hoja de modelos en el archivo.";
            return RedirectToAction("Modelos");
          }

          int totalCols = sheet.Dimension.End.Column;
          int totalRows = sheet.Dimension.End.Row;

          for (int row = 3; row <= totalRows; row++)
          {
            var rowData = new List<string>();
            bool hasData = false;

            for (int col = 1; col <= totalCols; col++)
            {
              string value = sheet.Cells[row, col].Value?.ToString() ?? "";
              rowData.Add(value);

              if (!string.IsNullOrWhiteSpace(value))
                hasData = true;
            }

            if (hasData)
              await SaveDataToDatabaseAsyncModelos(rowData);
          }

          TempData["Alert2"] = "Archivo procesado exitosamente.";
        }
      }
      catch (Exception ex)
      {
        TempData["Alert2"] = "Error al procesar el archivo: " + ex.Message;
      }

      return RedirectToAction("Modelos");
    }


    private async Task SaveDataToDatabaseAsyncModelos(List<string> rowData)
    {
      // Lee la cadena de conexión desde el archivo app.config, esta cadena se puede modificar.
      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

      _currentUser user = new _currentUser();
      user.Id = Guid.NewGuid();

      using (var connection = new SqlConnection(connectionString))
      using (var command = new SqlCommand("InsertModeloExcel", connection))
      {
        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.AddWithValue("@modeloId", Guid.NewGuid());
        command.Parameters.AddWithValue("@marca", rowData[0]);
        command.Parameters.AddWithValue("@nombre", rowData[1]);
        command.Parameters.AddWithValue("@descripcion", rowData[2]);
        command.Parameters.AddWithValue("@entryUser", Guid.Empty);
        command.Parameters.AddWithValue("@entryDate", DateTime.Now);
        var outputParam = new SqlParameter("@Resultado", SqlDbType.VarChar, 400)
        {
          Direction = ParameterDirection.Output
        };
        command.Parameters.Add(outputParam);

        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();

        string resultado = outputParam.Value?.ToString() ?? "Sin resultado.";
        TempData["Alert2"] = resultado;
      }
    }

    public int ObtenerCantidadModelos()
    {
      int total = 0;
      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

      using (SqlConnection connection = new SqlConnection(connectionString))
      {
        string query = "SELECT COUNT(modeloId) FROM Modelo";

        using (SqlCommand command = new SqlCommand(query, connection))
        {
          connection.Open();
          total = (int)command.ExecuteScalar();
        }
      }

      return total;
    }


    #region Categorias
    //CLASES TEMPORALES PARA LEER LA DATA DEL JSON
    public class Category
    {

      [JsonPropertyName("name")]
      public string Name { get; set; } = "";

      [JsonPropertyName("description")]
      public string Description { get; set; } = "";

      [JsonPropertyName("actives")]
      public int Actives { get; set; }

    }

    public class CategoryList
    {
      public List<Category> Categories { get; set; } = new List<Category>();
    }



    //METODO PARA OBTENER LOS DATOS TEMPORALES DEL JSON
    // Este metodo consula especificamente el archivo que contiene los datos JSON temporales para mostrar y los devuelve
    // en tipo Array de Categorias Category[]
    public Category[] Get_dummy_categories_data_from_JSON()
    {

      /*
        PROCESOS PARA LEER LA DATA DEL JSON
        NO SERAN NECESARIOS UNA VEZ QUE SE UTILICE LA API
      */
      //Definir la ruta del archivo JSON
      string jsonPath = "Data/Categories/DummyData.json";
      //Si la ruta no existe notificar en consola
      if (!System.IO.File.Exists(jsonPath))
      {
        Console.WriteLine($"El archivo no existe en la ruta: {jsonPath}");
      }
      //Leer la data del archivo JSON
      string jsonContent = System.IO.File.ReadAllText(jsonPath);

      // Deserializa el JSON para convertirlo en variable de tipo CategoryList
      var data = JsonSerializer.Deserialize<CategoryList>(jsonContent);

      //Definir la variable Category Array que se va a utilizar
      Category[] categories_list = [];

      //Si la data es valida entonces convertir de tipo List<Category> a Category[]
      if (data != null && data.Categories.Count != 0)
      {
        categories_list = data.Categories.ToArray();
      }
      /*
        PROCESOS PARA LEER LA DATA DEL JSON
        NO SERAN NECESARIOS UNA VEZ QUE SE UTILICE LA API
      */

      return categories_list;

    }





    //METODO PARA FILTRAR LA LISTA DE Category
    // Este metodo recibe el array de Categorias que se quiere filtrar (Category[])
    // y lo filtra segun los parametros: nombre
    // El input de busqueda filtra segun Nombre
    // Tambien Filtrado segun categoria con/sin activos asignados
    public Categoria[] filter_categories_list(Categoria[] categories_list, string categories_search_input, string category_actives_state)
    {

      //Filtrar segun los parametros de texto de busqueda
      // NOMBRE
      if (!string.IsNullOrEmpty(categories_search_input))
      {
        categories_list = categories_list
          .Where(category =>
          {
            return
            category.Name.ToLower().Contains(categories_search_input.ToLower());
          })
          .ToArray();
      }
      //Filtrar segun el dropdown CON ACTIVOS / SIN ACTIVOS
      if (!String.IsNullOrEmpty(category_actives_state) && category_actives_state == "with_actives")
      {
        categories_list = categories_list
          .Where(category =>
          {
            return category.Actives > 0;

          })
          .ToArray();
      }
      else if (!String.IsNullOrEmpty(category_actives_state) && category_actives_state == "without_actives")
      {
        categories_list = categories_list
          .Where(category =>
          {
            return category.Actives == 0;
          })
          .ToArray();
      }

      return categories_list;
    }


    //METODO PARA CREAR LA PAGINACION DE CATEGORIAS
    // Este metodo recibe el array de categorias que se quiere paginar y la cantidad de categorias por pagina
    // Retorna una lista de listas de Categorias (arrayList) donde se encuentran las paginas de caegorias
    //segun la cantidad ingresada en los parametros.
    public List<List<Categoria>> create_categoriespages_from_categories_list(Categoria[] categories_list, int categories_per_page)
    {

      //Lista de paginas de categorias divididas segun la cantidad seleccionada en la vista
      List<List<Categoria>> Categories_Pages = new List<List<Categoria>>();

      //LOOP PARA DIVIDIR LA LISTA DE CATEGORIAS EN PAGINAS DE LA CANTIDAD SELECCIONADA
      for (int i = 0; i < categories_list.Length; i = i + categories_per_page)
      {
        //PAGINA CORRESPONDIENTE A ITERACION
        List<Categoria> categories_page = new List<Categoria>();

        for (int j = i; j < i + categories_per_page; j++)
        {
          //SI EL NUMERO DE LA ITERACION NO SOBREPASA LA CANTIDAD TOTAL DE CATEGORIAS, SE AGREGA A LA PAGINA CORRESPONDIENTE
          if (j < categories_list.Length)
          {
            // Se agrega la categoria correspondiente al index en j
            // De esta manera se crean paginas segun la cantidad que deben tener
            categories_page.Add(categories_list[j]);
          }
        }
        //SE AGREGA LA PAGINA CREADA A LA LISTA DE PAGINAS
        Categories_Pages.Add(categories_page);
      }

      return Categories_Pages;
    }


    //METODO PARA ORDENAR ALFABETICAMENTE EL ARRAY DE PERMISOS
    // Este metodo recibe un array de Permisos y un string donde se especifica segun que atributo se quiere ordenar
    // Los posibles atributos para odenar son: name, description y creation_date
    // Si no se ingresa ningun parametro se ordena por nombre por default
    public Categoria[] order_categorieslist_by(Categoria[] categories_list, string order_by)
    {

      // se realiza un switch para determinar que tipo de orden se require
      switch (order_by)
      {

        case "name_ascending":
          // Ordenar alfabéticamente ascendentemente segun Nombre, ignorando mayúsculas y minúsculas
          categories_list = categories_list.OrderBy(category => category.Name, StringComparer.OrdinalIgnoreCase).ToArray();
          break;

        case "name_descending":
          // Ordenar alfabéticamente descendentemente segun Nombre, ignorando mayúsculas y minúsculas
          categories_list = categories_list.OrderByDescending(category => category.Name, StringComparer.OrdinalIgnoreCase).ToArray();
          break;

        default:
          // Ordenar alfabéticamente segun Nombre, ignorando mayúsculas y minúsculas
          categories_list = categories_list.OrderBy(category => category.Name, StringComparer.OrdinalIgnoreCase).ToArray();
          break;
      }

      return categories_list;
    }



    [HttpGet]
    public IActionResult ListCategories(string categories_search_input = "", string order_by = "name_ascending",
    int categories_per_page = 10, int page_number = 1, string category_actives_state = "")
    {


      //Se llama al metodo para obtener los datos del JSON
      Categoria[] categories_list_from_JSON = GetCategories().ToArray();


      //Se llama al metodo para filtrar las categorias segun Nombre
      Categoria[] filtered_categories_list =
      filter_categories_list(categories_list_from_JSON, categories_search_input, category_actives_state);


      //Se orderna el array de categorias despues de ser filtrado
      Categoria[] filtered_categories_list_ordered = order_categorieslist_by(filtered_categories_list, order_by);



      //Se llama al metodo que crea la paginacion de la lista de categorias segun los parametros designados
      List<List<Categoria>> Categories_Pages = create_categoriespages_from_categories_list(filtered_categories_list_ordered, categories_per_page);

      //Definir la variable que va a contener las categorias de la pagina a mostrar
      Categoria[] selected_categories_page = [];

      //Si el numero de pagina es 0 se asigna a 1 porque la pagina 0 no existe
      if (page_number == 0) page_number = 1;

      //Si el numero de pagina seleccionado es mayor a la cantidad total de paginas, se asigna la ultima pagina, si no se mantiene
      page_number = page_number >= Categories_Pages.Count ? Categories_Pages.Count : page_number;


      // SI EXISTEN PAGINAS EN LA LISTA DE PAGINAS, SE ASIGNA LA PAGINA CORRESPONDIENTE
      // SI NO, LA LISTA QUEDA VACIA YA QUE NO SE ENCONTRÓ NINGÚN PERMISO
      if (Categories_Pages.Count != 0 && page_number != 0)
      {

        //Se asigna la pagina correspondiente al array de categorias que se va a utilizar
        selected_categories_page = Categories_Pages.ElementAt(
        // Si el numero de pagina que se seleccionó es mayor a la cantidad de paginas disponibles
        page_number > Categories_Pages.Count
        // Se asigna la primera pagina ya que se excedio la cantidad maxima
        ? 0
        // Si no, se asigna el numero de pagina -1 lo que corresponde al index correcto de la pagina en la lista de paginas
        : page_number - 1)
        .ToArray();
      }




      //USO DE DICCIONARIO VIEWDATA PARA ENVIAR DATOS A LA VISTA

      //Total de paginas
      ViewData["Total_Pages"] = Categories_Pages.Count;
      //Pagina actual
      ViewData["Current_Page"] = page_number;
      //Categorias por pagina
      ViewData["Categories_Per_Page"] = categories_per_page;
      //Columna que dicta orden da datos
      ViewData["Order_By"] = order_by;
      //Filtro de busqueda segun nombre
      ViewData["Categories_Search_Input"] = categories_search_input;
      //Filtro de categoria segun activos asociados o no
      ViewData["Category_Actives_State"] = category_actives_state;

      ViewBag.TotalCategorias = ObtenerCantidadCategorias();
      ViewBag.NombreUbicacion = GetUltimoNombreUbicacionC() ?? "Ubicación C";
      ViewBag.NombreUbicacionA = GetUltimoNombreUbicacionA() ?? "Ubicación A";
      ViewBag.NombreUbicacionB = GetUltimoNombreUbicacionB() ?? "Ubicación B";



      //RETORNAR A LA VISTA CON EL ARRAY DE CATEGORIAS FILTRADOS Y ORDERNADOS DE LA PAGINA SELECCIONADA
      return View(selected_categories_page);

    }



    public List<Categoria> GetCategories()
    {
      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;
      List<Categoria> categories = new List<Categoria>();

      using (SqlConnection connection = new SqlConnection(connectionString))
      {
        string query = @"
            SELECT c.*, 
                   ISNULL(COUNT(a.assetCategorySysId), 0) AS Actives
            FROM assetCategory c
            LEFT JOIN assets a ON c.assetCategorySysId = a.assetCategorySysId
            GROUP BY c.assetCategorySysId, c.name, c.description, c.entryUser, c.entryDate, 
                     c.updateUser, c.updateDate, c.rowGuid, c.valorvidaUtil, 
                     c.vidaUtilProcomer, c.companyIdExtern, c.depSysId";

        SqlCommand command = new SqlCommand(query, connection);

        try
        {
          connection.Open();
          SqlDataReader reader = command.ExecuteReader();

          while (reader.Read())
          {
            Categoria category = new Categoria
            {
              AssetCategorySysId = reader.GetGuid(reader.GetOrdinal("assetCategorySysId")),
              Name = reader.GetString(reader.GetOrdinal("name")),
              Description = reader.GetString(reader.GetOrdinal("description")),
              EntryUser = reader.GetGuid(reader.GetOrdinal("entryUser")),
              EntryDate = reader.GetDateTime(reader.GetOrdinal("entryDate")),
              UpdateUser = reader.GetGuid(reader.GetOrdinal("updateUser")),
              UpdateDate = reader.GetDateTime(reader.GetOrdinal("updateDate")),
              RowGuid = reader.GetGuid(reader.GetOrdinal("rowGuid")),
              ValorVidaUtil = reader.IsDBNull(reader.GetOrdinal("valorvidaUtil")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("valorvidaUtil")),
              VidaUtilProcomer = reader.IsDBNull(reader.GetOrdinal("vidaUtilProcomer")) ? null : reader.GetString(reader.GetOrdinal("vidaUtilProcomer")),
              CompanyIdExtern = reader.IsDBNull(reader.GetOrdinal("companyIdExtern")) ? null : reader.GetString(reader.GetOrdinal("companyIdExtern")),
              DepSysId = reader.IsDBNull(reader.GetOrdinal("depSysId")) ? (Guid?)null : reader.GetGuid(reader.GetOrdinal("depSysId")),
              Actives = reader.GetInt32(reader.GetOrdinal("Actives")) // Contar activos
            };
            categories.Add(category);
          }
          reader.Close();
        }
        catch (Exception ex)
        {
          Console.WriteLine("Error al obtener categorías: " + ex.Message);
        }
      }
      return categories;
    }



    [HttpPost]
    public IActionResult DeleteCategory(string category_name = "")
    {
      var alertMessage = new AlertMessage();

      if (String.IsNullOrEmpty(category_name))
      {
        Console.WriteLine("No se recibió una categoría");
        alertMessage.Tipo = "error";
        alertMessage.Mensaje = "Debe proporcionar un nombre de categoría válido.";
      }
      else
      {
        Console.WriteLine("Intentando eliminar categoría: " + category_name);

        // Lista de categorías protegidas
        HashSet<string> categoriasProtegidas = new HashSet<string>
        {
            "Terrenos", "Construcciones", "Maquinaria", "Mobiliario",
            "Equipos Electrónicos", "Transporte", "Otros"
        };

        if (categoriasProtegidas.Contains(category_name.Trim()))
        {
          Console.WriteLine($"La categoría '{category_name}' está protegida y no se puede eliminar.");
          alertMessage.Tipo = "error";
          alertMessage.Mensaje = $"La categoría '{category_name}' está protegida y no se puede eliminar.";
        }
        else
        {
          string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

          using (SqlConnection connection = new SqlConnection(connectionString))
          {
            try
            {
              connection.Open();

              string query = "DELETE FROM assetCategory WHERE name = @name";

              using (SqlCommand command = new SqlCommand(query, connection))
              {
                command.Parameters.AddWithValue("@name", category_name.Trim());
                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                  Console.WriteLine($"Categoría '{category_name}' eliminada exitosamente.");
                  alertMessage.Tipo = "success";
                  alertMessage.Mensaje = $"Categoría '{category_name}' eliminada exitosamente.";
                }
                else
                {
                  Console.WriteLine($"No se encontró la categoría: {category_name}");
                  alertMessage.Tipo = "error";
                  alertMessage.Mensaje = $"No se encontró la categoría '{category_name}'.";
                }
              }
            }
            catch (Exception ex)
            {
              Console.WriteLine("Error al eliminar la categoría: " + ex.Message);
              alertMessage.Tipo = "error";
              alertMessage.Mensaje = "Error al eliminar la categoría: " + ex.Message;
            }
          }
        }
      }


      TempData["Alert"] = JsonSerializer.Serialize(alertMessage);
      return RedirectToAction("ListCategories");
    }

    [HttpPost]
    public IActionResult DeleteMultipleCategories(string categories_names_string = "")
    {
      var alertMessage = new AlertMessage();

      if (String.IsNullOrEmpty(categories_names_string))
      {
        Console.WriteLine("No se recibió ninguna categoría");
        alertMessage.Tipo = "error";
        alertMessage.Mensaje = "No se recibió ninguna categoría para eliminar.";
      }
      else
      {
        Console.WriteLine("Intentando eliminar categorías:");

        string[] categories_names = categories_names_string.Split('$');

        // Lista de categorías protegidas
        HashSet<string> categoriasProtegidas = new HashSet<string>
        {
            "Terrenos", "Construcciones", "Maquinaria", "Mobiliario",
            "Equipos Electrónicos", "Transporte", "Otros"
        };

        string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

        using (SqlConnection connection = new SqlConnection(connectionString))
        {
          connection.Open();
          SqlTransaction transaction = connection.BeginTransaction();

          try
          {
            bool algunaEliminada = false;

            foreach (string category_name in categories_names)
            {
              if (!string.IsNullOrWhiteSpace(category_name))
              {
                if (categoriasProtegidas.Contains(category_name.Trim()))
                {
                  Console.WriteLine($"La categoría '{category_name}' está protegida y no se puede eliminar.");
                  continue; // Salta la categoría protegida
                }

                Console.WriteLine($"Eliminando: {category_name}");

                string query = "DELETE FROM assetCategory WHERE name = @name";

                using (SqlCommand command = new SqlCommand(query, connection, transaction))
                {
                  command.Parameters.AddWithValue("@name", category_name.Trim());
                  int rowsAffected = command.ExecuteNonQuery();

                  if (rowsAffected > 0)
                  {
                    algunaEliminada = true;
                  }
                  else
                  {
                    Console.WriteLine($"No se encontró la categoría: {category_name}");
                  }
                }
              }
            }

            if (algunaEliminada)
            {
              transaction.Commit();
              Console.WriteLine("Categorías eliminadas exitosamente.");
              alertMessage.Tipo = "success";
              alertMessage.Mensaje = "Categorías eliminadas exitosamente.";
            }
            else
            {
              transaction.Rollback();
              Console.WriteLine("No se eliminaron categorías.");
              alertMessage.Tipo = "error";
              alertMessage.Mensaje = "No se eliminaron categorías.";
            }
          }
          catch (Exception ex)
          {
            transaction.Rollback();
            Console.WriteLine("Error al eliminar categorías: " + ex.Message);
            alertMessage.Tipo = "error";
            alertMessage.Mensaje = "Error al eliminar categorías: " + ex.Message;
          }
        }
      }

      TempData["Alert"] = JsonSerializer.Serialize(alertMessage);
      return RedirectToAction("ListCategories");
    }






    [HttpPost]
    public IActionResult AddCategory(string add_category_name = "", string add_category_description = "")
    {
      var alertMessage = new AlertMessage();

      if (String.IsNullOrEmpty(add_category_name) || String.IsNullOrEmpty(add_category_description))
      {
        Console.WriteLine("No se recibieron datos válidos para registrar una categoría");
        alertMessage.Tipo = "error";
        alertMessage.Mensaje = "Debe proporcionar un nombre y una descripción válidos para la categoría.";
      }
      else
      {
        string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

        using (SqlConnection connection = new SqlConnection(connectionString))
        {
          try
          {
            connection.Open();

            // Verificar si la categoría ya existe
            string checkQuery = "SELECT COUNT(1) FROM assetCategory WHERE name = @name";
            using (SqlCommand checkCommand = new SqlCommand(checkQuery, connection))
            {
              checkCommand.Parameters.AddWithValue("@name", add_category_name.Trim());
              int count = (int)checkCommand.ExecuteScalar();

              if (count > 0)
              {
                Console.WriteLine($"La categoría '{add_category_name}' ya existe. No se puede duplicar.");
                alertMessage.Tipo = "error";
                alertMessage.Mensaje = $"La categoría '{add_category_name}' ya existe. No se puede duplicar.";
                TempData["Alert"] = JsonSerializer.Serialize(alertMessage);
                return RedirectToAction("ListCategories");
              }
            }

            // Insertar nueva categoría
            string query = @"
                    INSERT INTO assetCategory 
                    (assetCategorySysId, name, description, entryUser, entryDate, updateUser, updateDate, rowGuid, valorvidaUtil, vidaUtilProcomer, companyIdExtern, depSysId) 
                    VALUES 
                    (@assetCategorySysId, @name, @description, @entryUser, @entryDate, @updateUser, @updateDate, @rowGuid, @valorvidaUtil, @vidaUtilProcomer, @companyIdExtern, @depSysId)";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
              command.Parameters.AddWithValue("@assetCategorySysId", Guid.NewGuid());
              command.Parameters.AddWithValue("@name", add_category_name.Trim());
              command.Parameters.AddWithValue("@description", add_category_description.Trim());
              command.Parameters.AddWithValue("@entryUser", Guid.NewGuid());
              command.Parameters.AddWithValue("@entryDate", DateTime.Now);
              command.Parameters.AddWithValue("@updateUser", Guid.NewGuid());
              command.Parameters.AddWithValue("@updateDate", DateTime.Now);
              command.Parameters.AddWithValue("@rowGuid", Guid.NewGuid());
              command.Parameters.AddWithValue("@valorvidaUtil", DBNull.Value);
              command.Parameters.AddWithValue("@vidaUtilProcomer", DBNull.Value);
              command.Parameters.AddWithValue("@companyIdExtern", DBNull.Value);
              command.Parameters.AddWithValue("@depSysId", DBNull.Value);

              int rowsAffected = command.ExecuteNonQuery();

              if (rowsAffected > 0)
              {
                Console.WriteLine($"Categoría '{add_category_name}' registrada exitosamente.");
                alertMessage.Tipo = "success";
                alertMessage.Mensaje = $"Categoría '{add_category_name}' registrada exitosamente.";
              }
              else
              {
                Console.WriteLine("No se pudo registrar la categoría.");
                alertMessage.Tipo = "error";
                alertMessage.Mensaje = "No se pudo registrar la categoría. Intente nuevamente.";
              }
            }
          }
          catch (Exception ex)
          {
            Console.WriteLine("Error al registrar la categoría: " + ex.Message);
            alertMessage.Tipo = "error";
            alertMessage.Mensaje = "Error al registrar la categoría: " + ex.Message;
          }
        }
      }

      TempData["Alert"] = JsonSerializer.Serialize(alertMessage);
      return RedirectToAction("ListCategories");
    }


    [HttpPost]
    public IActionResult EditCategory(Guid id_category, string category_to_edit, string edit_category_name = "", string edit_category_description = "")
    {
      var alertMessage = new AlertMessage();

      // Lista de nombres restringidos
      var restrictedNames = new List<string>
    {
        "Terrenos", "Construcciones", "Maquinaria", "Mobiliario",
        "Equipos Electrónicos", "Transporte", "Otros"
    };

      if (id_category == Guid.Empty || string.IsNullOrEmpty(category_to_edit) || string.IsNullOrEmpty(edit_category_name) || string.IsNullOrEmpty(edit_category_description))
      {
        Console.WriteLine("No se recibieron datos válidos para editar una categoría");
        alertMessage.Tipo = "error";
        alertMessage.Mensaje = "No se recibieron datos válidos para editar la categoría.";
      }
      else if (restrictedNames.Contains(category_to_edit))
      {
        Console.WriteLine($"No se puede editar la categoría protegida: {category_to_edit}");
        alertMessage.Tipo = "error";
        alertMessage.Mensaje = $"La categoría '{category_to_edit}' es fija y no puede ser modificada.";
      }
      else
      {
        Console.WriteLine($"Editar Categoría: {category_to_edit}");
        Console.WriteLine($"Nombre nuevo: {edit_category_name}");
        Console.WriteLine($"Descripción nueva: {edit_category_description}");

        string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

        try
        {
          using (SqlConnection connection = new SqlConnection(connectionString))
          {
            connection.Open();
            string query = @"UPDATE assetCategory 
                                 SET name = @name, 
                                     description = @description, 
                                     updateUser = @updateUser, 
                                     updateDate = @updateDate
                                 WHERE assetCategorySysId = @categorySysId";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
              command.Parameters.AddWithValue("@categorySysId", id_category);
              command.Parameters.AddWithValue("@name", edit_category_name);
              command.Parameters.AddWithValue("@description", edit_category_description);
              command.Parameters.AddWithValue("@updateUser", Guid.NewGuid());
              command.Parameters.AddWithValue("@updateDate", DateTime.Now);

              int rowsAffected = command.ExecuteNonQuery();

              if (rowsAffected > 0)
              {
                Console.WriteLine("Categoría actualizada correctamente.");
                alertMessage.Tipo = "success";
                alertMessage.Mensaje = "Categoría actualizada correctamente.";
              }
              else
              {
                Console.WriteLine("No se encontró la categoría para actualizar.");
                alertMessage.Tipo = "error";
                alertMessage.Mensaje = "No se encontró la categoría para actualizar.";
              }
            }
          }
        }
        catch (Exception ex)
        {
          Console.WriteLine("Error al actualizar la categoría: " + ex.Message);
          alertMessage.Tipo = "error";
          alertMessage.Mensaje = "Error al actualizar la categoría: " + ex.Message;
        }
      }

      TempData["Alert"] = JsonSerializer.Serialize(alertMessage);
      return RedirectToAction("ListCategories");
    }

    [HttpPost]
    public async Task<IActionResult> SincronizarCategorias(IFormFile excelFile)
    {
      if (excelFile == null || excelFile.Length == 0)
      {
        TempData["Alert2"] = "Por favor seleccione un archivo válido.";
        return RedirectToAction("ListCategories");
      }

      try
      {
        // Guardar el archivo temporalmente
        var fileName = Path.GetFileName(excelFile.FileName);
        var tempPath = Path.Combine(Path.GetTempPath(), fileName);

        using (var stream = new FileStream(tempPath, FileMode.Create))
        {
          await excelFile.CopyToAsync(stream);
        }
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        using (var package = new ExcelPackage(new FileInfo(tempPath)))
        {
          var sheet = package.Workbook.Worksheets.FirstOrDefault(s => s.Name.ToLower().Contains("categorias"));

          if (sheet == null)
          {
            TempData["Alert2"] = "No se encontró la hoja de categorias en el archivo.";
            return RedirectToAction("ListCategories");
          }

          int totalCols = sheet.Dimension.End.Column;
          int totalRows = sheet.Dimension.End.Row;

          for (int row = 3; row <= totalRows; row++)
          {
            var rowData = new List<string>();
            bool hasData = false;

            for (int col = 1; col <= totalCols; col++)
            {
              string value = sheet.Cells[row, col].Value?.ToString() ?? "";
              rowData.Add(value);

              if (!string.IsNullOrWhiteSpace(value))
                hasData = true;
            }

            if (hasData)
              await SaveDataToDatabaseAsyncCategorias(rowData);
          }

          TempData["Alert2"] = "Archivo procesado exitosamente.";
        }
      }
      catch (Exception ex)
      {
        TempData["Alert2"] = "Error al procesar el archivo: " + ex.Message;
      }

      return RedirectToAction("ListCategories");
    }


    private async Task SaveDataToDatabaseAsyncCategorias(List<string> rowData)
    {
      // Lee la cadena de conexión desde el archivo app.config, esta cadena se puede modificar.
      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

      _currentUser user = new _currentUser();
      user.Id = Guid.NewGuid();

      using (var connection = new SqlConnection(connectionString))
      using (var command = new SqlCommand("InsertAssetCategoryxExcel", connection))
      {
        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.AddWithValue("@name", rowData[0]);
        command.Parameters.AddWithValue("@description", rowData[1]);
        command.Parameters.AddWithValue("@usuario", Guid.Empty);
        var outputParam = new SqlParameter("@Resultado", SqlDbType.VarChar, 400)
        {
          Direction = ParameterDirection.Output
        };
        command.Parameters.Add(outputParam);

        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();

        string resultado = outputParam.Value?.ToString() ?? "Sin resultado.";
        TempData["Alert2"] = resultado;
      }
    }

    public int ObtenerCantidadCategorias()
    {
      int total = 0;
      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

      using (SqlConnection connection = new SqlConnection(connectionString))
      {
        string query = "SELECT COUNT(assetCategorySysId) FROM assetCategory";

        using (SqlCommand command = new SqlCommand(query, connection))
        {
          connection.Open();
          total = (int)command.ExecuteScalar();
        }
      }

      return total;
    }

    public IActionResult DescargarPlantillaCategorias()
    {
      var rutaArchivo = Path.Combine(Directory.GetCurrentDirectory(), "Plantillas", "PlantillaCategorias.xlsx");

      if (!System.IO.File.Exists(rutaArchivo))
      {
        return NotFound("La plantilla no fue encontrada.");
      }

      var contenido = System.IO.File.ReadAllBytes(rutaArchivo);
      var nombreArchivo = "PlantillaCategorias.xlsx";
      return File(contenido, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", nombreArchivo);
    }

    public IActionResult DescargarPlantillaEstados()
    {
      var rutaArchivo = Path.Combine(Directory.GetCurrentDirectory(), "Plantillas", "PlantillaEstados.xlsx");

      if (!System.IO.File.Exists(rutaArchivo))
      {
        return NotFound("La plantilla no fue encontrada.");
      }

      var contenido = System.IO.File.ReadAllBytes(rutaArchivo);
      var nombreArchivo = "PlantillaEstados.xlsx";
      return File(contenido, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", nombreArchivo);
    }

    public IActionResult DescargarPlantillaMarcas()
    {
      var rutaArchivo = Path.Combine(Directory.GetCurrentDirectory(), "Plantillas", "PlantillaMarcas.xlsx");

      if (!System.IO.File.Exists(rutaArchivo))
      {
        return NotFound("La plantilla no fue encontrada.");
      }

      var contenido = System.IO.File.ReadAllBytes(rutaArchivo);
      var nombreArchivo = "PlantillaMarcas.xlsx";
      return File(contenido, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", nombreArchivo);
    }

    public IActionResult DescargarPlantillaModelos()
    {
      var rutaArchivo = Path.Combine(Directory.GetCurrentDirectory(), "Plantillas", "PlantillaModelos.xlsx");

      if (!System.IO.File.Exists(rutaArchivo))
      {
        return NotFound("La plantilla no fue encontrada.");
      }

      var contenido = System.IO.File.ReadAllBytes(rutaArchivo);
      var nombreArchivo = "PlantillaModelos.xlsx";
      return File(contenido, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", nombreArchivo);
    }

    #endregion Fin Categorias
    #region Marcas
    public class Brand
    {

      [JsonPropertyName("name")]
      public string Name { get; set; } = "";

      [JsonPropertyName("description")]
      public string Description { get; set; } = "";

      [JsonPropertyName("actives")]
      public int Actives { get; set; }

    }

    public class BrandList
    {
      public List<Brand> Brands { get; set; } = new List<Brand>();
    }



    //METODO PARA OBTENER LOS DATOS TEMPORALES DEL JSON
    // Este metodo consula especificamente el archivo que contiene los datos JSON temporales para mostrar y los devuelve
    // en tipo Array de Marcas Brand[]
    public Brand[] Get_dummy_brands_data_from_JSON()
    {

      /*
        PROCESOS PARA LEER LA DATA DEL JSON
        NO SERAN NECESARIOS UNA VEZ QUE SE UTILICE LA API
      */
      //Definir la ruta del archivo JSON
      string jsonPath = "Data/Marcas/DummyData.json";
      //Si la ruta no existe notificar en consola
      if (!System.IO.File.Exists(jsonPath))
      {
        Console.WriteLine($"El archivo no existe en la ruta: {jsonPath}");
      }
      //Leer la data del archivo JSON
      string jsonContent = System.IO.File.ReadAllText(jsonPath);

      // Deserializa el JSON para convertirlo en variable de tipo BrandList
      var data = JsonSerializer.Deserialize<BrandList>(jsonContent);

      //Definir la variable Brand Array que se va a utilizar
      Brand[] brands_list = [];

      //Si la data es valida entonces convertir de tipo List<Brand> a Brand[]
      if (data != null && data.Brands.Count != 0)
      {
        brands_list = data.Brands.ToArray();
      }
      /*
        PROCESOS PARA LEER LA DATA DEL JSON
        NO SERAN NECESARIOS UNA VEZ QUE SE UTILICE LA API
      */

      return brands_list;

    }



    //METODO PARA FILTRAR LA LISTA DE MARCAS
    // Este metodo recibe el array de Marcas que se quiere filtrar (Brand[])
    // y lo filtra segun los parametros: nombre
    // El input de busqueda filtra segun Nombre
    public Marcas[] filter_brands_list(Marcas[] brands_list, string brand_search_input, string brand_actives_state)
    {

      //Filtrar segun los parametros de texto de busqueda
      // NOMBRE
      if (!string.IsNullOrEmpty(brand_search_input))
      {
        brands_list = brands_list
          .Where(brand =>
          {
            return
            brand.Nombre.ToLower().Contains(brand_search_input.ToLower());
          })
          .ToArray();
      }
      //Filtrar segun el dropdown CON ACTIVOS / SIN ACTIVOS
      if (!String.IsNullOrEmpty(brand_actives_state) && brand_actives_state == "with_actives")
      {
        brands_list = brands_list
          .Where(brand =>
          {
            return brand.Actives > 0;

          })
          .ToArray();
      }
      else if (!String.IsNullOrEmpty(brand_actives_state) && brand_actives_state == "without_actives")
      {
        brands_list = brands_list
          .Where(brand =>
          {
            return brand.Actives == 0;
          })
          .ToArray();
      }

      return brands_list;
    }



    //METODO PARA CREAR LA PAGINACION DE MARCAS
    // Este metodo recibe el array de marcas que se quiere paginar y la cantidad de marcas por pagina
    // Retorna una lista de listas de Marcas (arrayList) donde se encuentran las paginas de marcas
    //segun la cantidad ingresada en los parametros.
    public List<List<Marcas>> create_brandspages_from_brands_list(Marcas[] brands_list, int brands_per_page)
    {

      //Lista de paginas de marcas divididas segun la cantidad seleccionada en la vista
      List<List<Marcas>> Brands_Pages = new List<List<Marcas>>();

      //LOOP PARA DIVIDIR LA LISTA DE MARCAS EN PAGINAS DE LA CANTIDAD SELECCIONADA
      for (int i = 0; i < brands_list.Length; i = i + brands_per_page)
      {
        //PAGINA CORRESPONDIENTE A ITERACION
        List<Marcas> brands_page = new List<Marcas>();

        for (int j = i; j < i + brands_per_page; j++)
        {
          //SI EL NUMERO DE LA ITERACION NO SOBREPASA LA CANTIDAD TOTAL DE MARCAS, SE AGREGA A LA PAGINA CORRESPONDIENTE
          if (j < brands_list.Length)
          {
            // Se agrega la marca correspondiente al index en j
            // De esta manera se crean paginas segun la cantidad que deben tener
            brands_page.Add(brands_list[j]);
          }
        }
        //SE AGREGA LA PAGINA CREADA A LA LISTA DE PAGINAS
        Brands_Pages.Add(brands_page);
      }

      return Brands_Pages;
    }


    //METODO PARA ORDENAR ALFABETICAMENTE EL ARRAY DE MARCAS
    // Este metodo recibe un array de Marcas y un string donde se especifica segun que atributo se quiere ordenar
    // Los posibles atributos para odenar son: name
    // Si no se ingresa ningun parametro se ordena por nombre por default
    public Marcas[] order_brandslist_by(Marcas[] brands_list, string order_by)
    {

      // se realiza un switch para determinar que tipo de orden se require
      switch (order_by)
      {

        case "name_ascending":
          // Ordenar alfabéticamente ascendentemente segun Nombre, ignorando mayúsculas y minúsculas
          brands_list = brands_list.OrderBy(brand => brand.Nombre, StringComparer.OrdinalIgnoreCase).ToArray();
          break;

        case "name_descending":
          // Ordenar alfabéticamente descendentemente segun Nombre, ignorando mayúsculas y minúsculas
          brands_list = brands_list.OrderByDescending(brand => brand.Nombre, StringComparer.OrdinalIgnoreCase).ToArray();
          break;

        default:
          // Ordenar alfabéticamente segun Nombre, ignorando mayúsculas y minúsculas
          brands_list = brands_list.OrderBy(brand => brand.Nombre, StringComparer.OrdinalIgnoreCase).ToArray();
          break;
      }

      return brands_list;
    }




    [HttpGet]
    public IActionResult ListBrands(string brand_search_input = "", string order_by = "name_ascending",
    int brands_per_page = 10, int page_number = 1, string brand_actives_state = "")
    {

      //Se llama al metodo para obtener los datos del JSON
      Marcas[] brands_list_from_JSON = GetMarcas().ToArray();

      //Se llama al metodo para filtrar las marcas segun Nombre
      Marcas[] filtered_brands_list =
      filter_brands_list(brands_list_from_JSON, brand_search_input, brand_actives_state);


      //Se orderna el array de marcas despues de ser filtrado
      Marcas[] filtered_brands_list_ordered = order_brandslist_by(filtered_brands_list, order_by);



      //Se llama al metodo que crea la paginacion de la lista de marcas segun los parametros designados
      List<List<Marcas>> Brands_Pages = create_brandspages_from_brands_list(filtered_brands_list_ordered, brands_per_page);

      //Definir la variable que va a contener las marcas de la pagina a mostrar
      Marcas[] selected_brands_page = [];

      //Si el numero de pagina es 0 se asigna a 1 porque la pagina 0 no existe
      if (page_number == 0) page_number = 1;

      //Si el numero de pagina seleccionado es mayor a la cantidad total de paginas, se asigna la ultima pagina, si no se mantiene
      page_number = page_number >= Brands_Pages.Count ? Brands_Pages.Count : page_number;


      // SI EXISTEN PAGINAS EN LA LISTA DE PAGINAS, SE ASIGNA LA PAGINA CORRESPONDIENTE
      // SI NO, LA LISTA QUEDA VACIA YA QUE NO SE ENCONTRÓ NINGÚN PERMISO
      if (Brands_Pages.Count != 0 && page_number != 0)
      {

        //Se asigna la pagina correspondiente al array de marcas que se va a utilizar
        selected_brands_page = Brands_Pages.ElementAt(
        // Si el numero de pagina que se seleccionó es mayor a la cantidad de paginas disponibles
        page_number > Brands_Pages.Count
        // Se asigna la primera pagina ya que se excedio la cantidad maxima
        ? 0
        // Si no, se asigna el numero de pagina -1 lo que corresponde al index correcto de la pagina en la lista de paginas
        : page_number - 1)
        .ToArray();
      }




      //USO DE DICCIONARIO VIEWDATA PARA ENVIAR DATOS A LA VISTA

      //Total de paginas
      ViewData["Total_Pages"] = Brands_Pages.Count;
      //Pagina actual
      ViewData["Current_Page"] = page_number;
      //Marcas por pagina
      ViewData["Brands_Per_Page"] = brands_per_page;
      //Columna que dicta orden da datos
      ViewData["Order_By"] = order_by;
      //Filtro de busqueda segun nombre
      ViewData["Brand_Search_Input"] = brand_search_input;
      //Filtro de marca segun activos asociados o no
      ViewData["Brand_Actives_State"] = brand_actives_state;
      ViewBag.NombreUbicacion = GetUltimoNombreUbicacionC() ?? "Ubicación C";
      ViewBag.NombreUbicacionA = GetUltimoNombreUbicacionA() ?? "Ubicación A";
      ViewBag.NombreUbicacionB = GetUltimoNombreUbicacionB() ?? "Ubicación B";

      ViewBag.TotalMarcas = ObtenerCantidadMarcas();


      //RETORNAR A LA VISTA CON EL ARRAY DE MARCAS FILTRADOS Y ORDERNADOS DE LA PAGINA SELECCIONADA
      return View(selected_brands_page);


    }

    public List<Marcas> GetMarcas()
    {
      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;
      List<Marcas> marcas = new List<Marcas>();

      using (SqlConnection connection = new SqlConnection(connectionString))
      {
        string query = @"
            SELECT c.*, 
                   ISNULL(COUNT(a.marca), 0) AS Actives
            FROM Marca c
            LEFT JOIN ActivosNueva a ON c.marcaId = a.marca
            GROUP BY c.[marcaId], c.[nombre], c.[descripcion], c.entryUser, c.entryDate, 
                     c.updateUser, c.updateDate";

        SqlCommand command = new SqlCommand(query, connection);

        try
        {
          connection.Open();
          SqlDataReader reader = command.ExecuteReader();

          while (reader.Read())
          {
            Marcas marca = new Marcas(
                reader.GetGuid(reader.GetOrdinal("marcaId")),
                reader.GetString(reader.GetOrdinal("nombre")),
                reader.IsDBNull(reader.GetOrdinal("descripcion")) ? null : reader.GetString(reader.GetOrdinal("descripcion")),
                reader.GetGuid(reader.GetOrdinal("entryUser")),
                reader.GetDateTime(reader.GetOrdinal("entryDate")),
                reader.GetGuid(reader.GetOrdinal("updateUser")),
                reader.GetDateTime(reader.GetOrdinal("updateDate")),
                reader.GetInt32(reader.GetOrdinal("Actives"))
            );
            marcas.Add(marca);
          }
          reader.Close();
        }
        catch (Exception ex)
        {
          Console.WriteLine("Error al obtener marcas: " + ex.Message);
        }
      }
      return marcas;
    }




    [HttpPost]
    public IActionResult DeleteBrand(Guid brand_id, string brand_name = "")
    {
      var alertMessage = new AlertMessage();

      if (brand_id == Guid.Empty)
      {
        Console.WriteLine("ID de marca no válido");
        alertMessage.Tipo = "error";
        alertMessage.Mensaje = "ID de marca no válido.";
      }
      else
      {
        string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

        using (SqlConnection connection = new SqlConnection(connectionString))
        {
          string query = "DELETE FROM Marca WHERE marcaId = @MarcaId";
          SqlCommand command = new SqlCommand(query, connection);
          command.Parameters.AddWithValue("@MarcaId", brand_id);

          try
          {
            connection.Open();
            int rowsAffected = command.ExecuteNonQuery();

            if (rowsAffected > 0)
            {
              Console.WriteLine("Marca eliminada correctamente: " + brand_name);
              alertMessage.Tipo = "success";
              alertMessage.Mensaje = "Marca eliminada correctamente.";
            }
            else
            {
              Console.WriteLine("No se encontró la marca con ID: " + brand_id);
              alertMessage.Tipo = "error";
              alertMessage.Mensaje = $"No se encontró la marca con ID: {brand_id}.";
            }
          }
          catch (Exception ex)
          {
            Console.WriteLine("Error al eliminar la marca: " + ex.Message);
            alertMessage.Tipo = "error";
            alertMessage.Mensaje = "Error al eliminar la marca: " + ex.Message;
          }
        }
      }


      TempData["Alert"] = JsonSerializer.Serialize(alertMessage);
      return RedirectToAction("ListBrands");
    }



    [HttpPost]
    public IActionResult DeleteMultipleBrands(string brands_ids_string = "")
    {
      var alertMessage = new AlertMessage();

      if (string.IsNullOrEmpty(brands_ids_string))
      {
        Console.WriteLine("No se recibió ninguna marca");
        alertMessage.Tipo = "error";
        alertMessage.Mensaje = "No se recibió ninguna marca.";
      }
      else
      {
        // Eliminar la coma al final del string, si existe
        brands_ids_string = brands_ids_string.TrimEnd(',');

        // Convertir la cadena de GUIDs en un array de GUIDs
        string[] brandIdsString = brands_ids_string.Split(',');
        List<Guid> brandIds = new List<Guid>();

        foreach (string brandId in brandIdsString)
        {
          if (Guid.TryParse(brandId, out Guid parsedGuid))
          {
            brandIds.Add(parsedGuid);
          }
          else
          {
            Console.WriteLine("ID de marca no válido: " + brandId);
            alertMessage.Tipo = "error";
            alertMessage.Mensaje = $"ID de marca no válido: {brandId}.";
            TempData["Alert"] = JsonSerializer.Serialize(alertMessage);
            return RedirectToAction("ListBrands");
          }
        }

        if (brandIds.Count == 0)
        {
          Console.WriteLine("No se pudo procesar ninguna marca válida.");
          alertMessage.Tipo = "error";
          alertMessage.Mensaje = "No se pudo procesar ninguna marca válida.";
          TempData["Alert"] = JsonSerializer.Serialize(alertMessage);
          return RedirectToAction("ListBrands");
        }

        string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

        using (SqlConnection connection = new SqlConnection(connectionString))
        {
          connection.Open();

          foreach (var brandId in brandIds)
          {
            // Crear la consulta para eliminar cada marca por su GUID
            string query = "DELETE FROM Marca WHERE marcaId = @MarcaId";
            SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@MarcaId", brandId);

            try
            {
              int rowsAffected = command.ExecuteNonQuery();

              if (rowsAffected > 0)
              {
                Console.WriteLine("Marca eliminada correctamente con ID: " + brandId);
                alertMessage.Tipo = "success";
                alertMessage.Mensaje = "Marca(s) eliminada(s) correctamente.";
              }
              else
              {
                Console.WriteLine("No se encontró la marca con ID: " + brandId);
                alertMessage.Tipo = "error";
                alertMessage.Mensaje = $"No se encontró la marca con ID: {brandId}.";
              }
            }
            catch (Exception ex)
            {
              Console.WriteLine("Error al eliminar la marca con ID " + brandId + ": " + ex.Message);
              alertMessage.Tipo = "error";
              alertMessage.Mensaje = "Error al eliminar marcas: " + ex.Message;
            }
          }
        }
      }
      TempData["Alert"] = JsonSerializer.Serialize(alertMessage);
      return RedirectToAction("ListBrands");
    }




    [HttpPost]
    public IActionResult AddBrand(string add_brand_name = "", string add_brand_description = "")
    {
      var alertMessage = new AlertMessage();

      if (String.IsNullOrEmpty(add_brand_name) || String.IsNullOrEmpty(add_brand_description))
      {
        Console.WriteLine("No se recibieron datos válidos para registrar una marca.");
        alertMessage.Tipo = "error";
        alertMessage.Mensaje = "No se recibieron datos válidos para registrar la marca.";
        TempData["Alert"] = JsonSerializer.Serialize(alertMessage);
        return RedirectToAction("ListBrands");
      }

      // Datos de la marca
      Guid marcaId = Guid.NewGuid(); // Generar un nuevo GUID para la marca
      Guid entryUser = Guid.NewGuid(); // Supongamos que esto es el GUID del usuario que agrega la marca (esto puede venir de sesión o autenticación)
      DateTime entryDate = DateTime.Now; // Fecha y hora actuales
      Guid updateUser = Guid.NewGuid(); // Al igual que entryUser, se puede obtener del contexto de usuario actual
      DateTime updateDate = DateTime.Now; // Fecha y hora actuales

      // Cadena de conexión
      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

      using (SqlConnection connection = new SqlConnection(connectionString))
      {
        string query = "INSERT INTO Marca (marcaId, nombre, descripcion, entryUser, entryDate, updateUser, updateDate) " +
                       "VALUES (@MarcaId, @Nombre, @Descripcion, @EntryUser, @EntryDate, @UpdateUser, @UpdateDate)";

        SqlCommand command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@MarcaId", marcaId);
        command.Parameters.AddWithValue("@Nombre", add_brand_name);
        command.Parameters.AddWithValue("@Descripcion", add_brand_description);
        command.Parameters.AddWithValue("@EntryUser", entryUser);
        command.Parameters.AddWithValue("@EntryDate", entryDate);
        command.Parameters.AddWithValue("@UpdateUser", updateUser);
        command.Parameters.AddWithValue("@UpdateDate", updateDate);

        try
        {
          connection.Open();
          int rowsAffected = command.ExecuteNonQuery();

          if (rowsAffected > 0)
          {
            Console.WriteLine("Marca '" + add_brand_name + "' registrada correctamente.");
            alertMessage.Tipo = "success";
            alertMessage.Mensaje = "Marca registrada correctamente.";
          }
          else
          {
            Console.WriteLine("Error al registrar la marca.");
            alertMessage.Tipo = "error";
            alertMessage.Mensaje = "Error al registrar la marca.";
          }
        }
        catch (Exception ex)
        {
          Console.WriteLine("Error al registrar la marca: " + ex.Message);
          alertMessage.Tipo = "error";
          alertMessage.Mensaje = "Error al registrar la marca: " + ex.Message;
        }
      }

      TempData["Alert"] = JsonSerializer.Serialize(alertMessage);
      return RedirectToAction("ListBrands");
    }

    [HttpPost]
    public JsonResult AddBrand2(string add_brand_name = "", string add_brand_description = "")
    {
      if (String.IsNullOrEmpty(add_brand_name) || String.IsNullOrEmpty(add_brand_description))
      {
        return Json(new { success = false, message = "Datos inválidos para registrar la marca." });
      }

      Guid marcaId = Guid.NewGuid();
      Guid entryUser = Guid.NewGuid();
      DateTime entryDate = DateTime.Now;
      Guid updateUser = Guid.NewGuid();
      DateTime updateDate = DateTime.Now;

      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

      using (SqlConnection connection = new SqlConnection(connectionString))
      {
        string query = "INSERT INTO Marca (marcaId, nombre, descripcion, entryUser, entryDate, updateUser, updateDate) " +
                       "VALUES (@MarcaId, @Nombre, @Descripcion, @EntryUser, @EntryDate, @UpdateUser, @UpdateDate)";

        SqlCommand command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@MarcaId", marcaId);
        command.Parameters.AddWithValue("@Nombre", add_brand_name);
        command.Parameters.AddWithValue("@Descripcion", add_brand_description);
        command.Parameters.AddWithValue("@EntryUser", entryUser);
        command.Parameters.AddWithValue("@EntryDate", entryDate);
        command.Parameters.AddWithValue("@UpdateUser", updateUser);
        command.Parameters.AddWithValue("@UpdateDate", updateDate);

        try
        {
          connection.Open();
          command.ExecuteNonQuery();
          Console.WriteLine("Marca '" + add_brand_name + "' registrada correctamente.");

          // Devuelves la marca agregada
          return Json(new { success = true, id = marcaId, name = add_brand_name });
        }
        catch (Exception ex)
        {
          Console.WriteLine("Error al registrar la marca: " + ex.Message);
          return Json(new { success = false, message = "Error al registrar la marca." });
        }
      }
    }

    [HttpPost]
    public IActionResult EditBrand(Guid brand_id_to_edit, string brand_to_edit, string edit_brand_name = "", string edit_brand_description = "")
    {
      var alertMessage = new AlertMessage();

      if (string.IsNullOrEmpty(brand_to_edit) || string.IsNullOrEmpty(edit_brand_name) || string.IsNullOrEmpty(edit_brand_description))
      {
        Console.WriteLine("No se recibieron datos válidos para editar la marca.");
        alertMessage.Tipo = "error";
        alertMessage.Mensaje = "No se recibieron datos válidos para editar la marca.";
        TempData["Alert"] = JsonSerializer.Serialize(alertMessage);
        return RedirectToAction("ListBrands");
      }

      // Datos de auditoría
      Guid updateUser = Guid.NewGuid(); // Supongamos que este es el GUID del usuario que actualiza la marca
      DateTime updateDate = DateTime.Now; // Fecha y hora actuales

      // Cadena de conexión
      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

      using (SqlConnection connection = new SqlConnection(connectionString))
      {
        string query = "UPDATE Marca SET nombre = @Nombre, descripcion = @Descripcion, updateUser = @UpdateUser, updateDate = @UpdateDate WHERE marcaId = @MarcaId";

        SqlCommand command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@MarcaId", brand_id_to_edit);
        command.Parameters.AddWithValue("@Nombre", edit_brand_name);
        command.Parameters.AddWithValue("@Descripcion", edit_brand_description);
        command.Parameters.AddWithValue("@UpdateUser", updateUser);
        command.Parameters.AddWithValue("@UpdateDate", updateDate);

        try
        {
          connection.Open();
          int rowsAffected = command.ExecuteNonQuery();

          if (rowsAffected > 0)
          {
            Console.WriteLine("Marca '" + brand_to_edit + "' actualizada correctamente a '" + edit_brand_name + "'.");
            alertMessage.Tipo = "success";
            alertMessage.Mensaje = "Marca actualizada correctamente.";
          }
          else
          {
            Console.WriteLine("No se encontró la marca a editar o no se realizaron cambios.");
            alertMessage.Tipo = "warning";
            alertMessage.Mensaje = "No se encontraron cambios para aplicar o la marca no existe.";
          }
        }
        catch (Exception ex)
        {
          Console.WriteLine("Error al editar la marca: " + ex.Message);
          alertMessage.Tipo = "error";
          alertMessage.Mensaje = "Error al editar la marca: " + ex.Message;
        }
      }

      TempData["Alert"] = JsonSerializer.Serialize(alertMessage);
      return RedirectToAction("ListBrands");
    }




    [HttpPost]
    public async Task<IActionResult> SincronizarMarcas(IFormFile excelFile)
    {
      if (excelFile == null || excelFile.Length == 0)
      {
        TempData["Alert2"] = "Por favor seleccione un archivo válido.";
        return RedirectToAction("ListBrands");
      }

      try
      {
        // Guardar el archivo temporalmente
        var fileName = Path.GetFileName(excelFile.FileName);
        var tempPath = Path.Combine(Path.GetTempPath(), fileName);

        using (var stream = new FileStream(tempPath, FileMode.Create))
        {
          await excelFile.CopyToAsync(stream);
        }
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        using (var package = new ExcelPackage(new FileInfo(tempPath)))
        {
          var sheet = package.Workbook.Worksheets.FirstOrDefault(s => s.Name.ToLower().Contains("marca"));

          if (sheet == null)
          {
            TempData["Alert2"] = "No se encontró la hoja de marcas en el archivo.";
            return RedirectToAction("ListBrands");
          }

          int totalCols = sheet.Dimension.End.Column;
          int totalRows = sheet.Dimension.End.Row;

          for (int row = 3; row <= totalRows; row++)
          {
            var rowData = new List<string>();
            bool hasData = false;

            for (int col = 1; col <= totalCols; col++)
            {
              string value = sheet.Cells[row, col].Value?.ToString() ?? "";
              rowData.Add(value);

              if (!string.IsNullOrWhiteSpace(value))
                hasData = true;
            }

            if (hasData)
              await SaveDataToDatabaseAsyncMarcas(rowData);
          }

          TempData["Alert2"] = "Archivo procesado exitosamente.";
        }
      }
      catch (Exception ex)
      {
        TempData["Alert2"] = "Error al procesar el archivo: " + ex.Message;
      }

      return RedirectToAction("ListBrands");
    }


    private async Task SaveDataToDatabaseAsyncMarcas(List<string> rowData)
    {
      // Lee la cadena de conexión desde el archivo app.config, esta cadena se puede modificar.
      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

      _currentUser user = new _currentUser();
      user.Id = Guid.NewGuid();

      using (var connection = new SqlConnection(connectionString))
      using (var command = new SqlCommand("InsertMarcaExcel", connection))
      {
        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.AddWithValue("@marcaId", Guid.NewGuid());
        command.Parameters.AddWithValue("@nombre", rowData[0]);
        command.Parameters.AddWithValue("@descripcion", rowData[1]);
        command.Parameters.AddWithValue("@entryUser", Guid.Empty);
        command.Parameters.AddWithValue("@entryDate", DateTime.Now);
        var outputParam = new SqlParameter("@Resultado", SqlDbType.VarChar, 400)
        {
          Direction = ParameterDirection.Output
        };
        command.Parameters.Add(outputParam);

        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();

        string resultado = outputParam.Value?.ToString() ?? "Sin resultado.";
        TempData["Alert2"] = resultado;
      }
    }

    public int ObtenerCantidadMarcas()
    {
      int total = 0;
      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

      using (SqlConnection connection = new SqlConnection(connectionString))
      {
        string query = "SELECT COUNT(marcaId) FROM Marca";

        using (SqlCommand command = new SqlCommand(query, connection))
        {
          connection.Open();
          total = (int)command.ExecuteScalar();
        }
      }

      return total;
    }




    #endregion Fin Marcas

  }
}

