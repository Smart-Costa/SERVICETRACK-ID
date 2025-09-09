using AspnetCoreMvcFull.Models.Edificios;
using AspnetCoreMvcFull.Models.Empresa;
using AspnetCoreMvcFull.Models.Empresa;
using AspnetCoreMvcFull.Models.Contactos;
using AspnetCoreMvcFull.Models.Gerencias;
using AspnetCoreMvcFull.Models.Mensajes;
using AspnetCoreMvcFull.Models.Oficinas;
using AspnetCoreMvcFull.Models.Pisos;
using AspnetCoreMvcFull.Models.Sectores;
using Diverscan.Activos.DL;
using Diverscan.Activos.DL.LogicaA;
using Diverscan.Activos.EL;
using Diverscan.Activos.UIL.Admin;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;
using OfficeOpenXml;
using OfficeOpenXml.Packaging.Ionic.Zlib;
using System.Configuration;
using System.Data;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlClient;
using System.Text;
using System.Text.Json;
using System.Text.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization;
using static AspnetCoreMvcFull.Controllers.AccessController;

namespace AspnetCoreMvcFull.Controllers
{
  public class EmpresaController : Controller
  {
    private readonly IMemoryCache _cache;
    public EmpresaController(IMemoryCache cache)
    {
      _cache = cache;
    }
    public IActionResult Index()
    {
      return View();
    }

    #region CONTACTOS
    [HttpGet]
    public async Task<IActionResult> Contactos(string? q, int page = 1, int pageSize = 5, bool registrado = false)
    {
      var (items, total) = await SearchContactosAsync(q, page, pageSize);

      var vm = new ContactosIndexVm
      {
        Items = items,
        CurrentPage = page,
        PageSize = pageSize,
        TotalItems = total,
        Query = q,
        FormData = new Contactos()
      };
      if (registrado)
      {
        ViewBag.AlertTitle = "Éxito";
        ViewBag.Alert = "Contacto registrado correctamente";
      }
      return View(vm);
    }


    public static async Task<(List<Contactos> items, int total)> SearchContactosAsync(
       string? q, int page, int pageSize)
    {
      string connectionString =
          System.Configuration.ConfigurationManager
              .ConnectionStrings["ServerDiverscan"].ConnectionString;

      // Sanitiza
      if (page <= 0) page = 1;
      if (pageSize <= 0) pageSize = 5;

      var where = "";
      var hasQ = !string.IsNullOrWhiteSpace(q);
      if (hasQ)
        where = "WHERE (Nombre LIKE @q OR Apellidos LIKE @q OR IdentificacionContacto LIKE @q OR Empresa LIKE @q)";

      var sqlCount = $@"SELECT COUNT(*) FROM dbo.EMPRESA WITH (NOLOCK) {where};";

      var sqlPage = $@"
SELECT
    Id_Contacto, Nombre, Telefono, Telefono_Movil, Email
FROM dbo.Contactos WITH (NOLOCK)
{where}
ORDER BY Nombre
OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY;";

      var items = new List<Contactos>();
      int total = 0;

      using var cn = new SqlConnection(connectionString);
      await cn.OpenAsync();

      // COUNT
      using (var cmd = new SqlCommand(sqlCount, cn))
      {
        if (hasQ) cmd.Parameters.Add("@q", SqlDbType.VarChar, 200).Value = $"%{q!.Trim()}%";
        total = Convert.ToInt32(await cmd.ExecuteScalarAsync());
      }

      // PAGE
      using (var cmd = new SqlCommand(sqlPage, cn))
      {
        if (hasQ) cmd.Parameters.Add("@q", SqlDbType.VarChar, 200).Value = $"%{q!.Trim()}%";
        cmd.Parameters.Add("@skip", SqlDbType.Int).Value = (page - 1) * pageSize;
        cmd.Parameters.Add("@take", SqlDbType.Int).Value = pageSize;

        using var rd = await cmd.ExecuteReaderAsync();
        while (await rd.ReadAsync())
        {
          items.Add(new Contactos
          {
            IdContacto = rd.GetGuid(rd.GetOrdinal("Id_Contacto")),
            Nombre = rd["Nombre"] as string,
            Telefono = rd["Telefono"] as string,
            TelefonoMovil = rd["Telefono_Movil"] as string,
            Email = rd["Email"] as string
          });
        }
      }

      return (items, total);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GuardarContactos(AspnetCoreMvcFull.Models.Contactos.Contactos model)
    {
      var idCreado = await InsertContactoAsync(model);

      // Redirigimos con un flag en la query (PRG pattern) — NO usar TempData aquí
      return RedirectToAction("Contactos", new { registrado = true });
    }


    public static async Task<Guid> InsertContactoAsync(Contactos model)
    {
      // tu cadena de conexión
      string connectionString =
          System.Configuration.ConfigurationManager
              .ConnectionStrings["ServerDiverscan"].ConnectionString;

      // Generamos el ID (tu tabla no muestra DEFAULT NEWID())
      Guid id = Guid.NewGuid();

      const string SQL = @"
INSERT INTO dbo.Contactos
(
    Id_Contacto,
    Nombre,
    Apellidos,
    Direccion,
    IdentificacionContacto,
    Empresa,
    Direccion_Opcional,
    Email,
    Telefono,
    Ciudad,
    Estado,
    Puesto,
    Telefono_Movil,
    Codigo_Postal,
    Pais,
    Estatus,
    Identificacion
)
VALUES
(
    @Id_Contacto,
    @Nombre,
    @Apellidos,
    @Direccion,
    @IdentificacionContacto,
    @Empresa,
    @Direccion_Opcional,
    @Email,
    @Telefono,
    @Ciudad,
    @Estado,
    @Puesto,
    @Telefono_Movil,
    @Codigo_Postal,
    @Pais,
    @Estatus,
    @Identificacion
);";

      using (var cn = new SqlConnection(connectionString))
      using (var cmd = new SqlCommand(SQL, cn))
      {
        cmd.Parameters.Add("@Id_Contacto", SqlDbType.UniqueIdentifier).Value = id;

        // Usa los tamaños reales de tus columnas (todo es varchar en la BD)
        cmd.Parameters.Add("@Nombre", SqlDbType.VarChar, 100).Value = Db(model.Nombre);
        cmd.Parameters.Add("@Apellidos", SqlDbType.VarChar, 200).Value = Db(model.Apellidos);
        cmd.Parameters.Add("@Direccion", SqlDbType.VarChar, 200).Value = Db(model.Direccion);
        cmd.Parameters.Add("@IdentificacionContacto", SqlDbType.VarChar, 200).Value = Db(model.IdentificacionContacto);
        cmd.Parameters.Add("@Empresa", SqlDbType.VarChar, 200).Value = Db(model.Empresa);
        cmd.Parameters.Add("@Direccion_Opcional", SqlDbType.VarChar, 200).Value = Db(model.DireccionOpcional);
        cmd.Parameters.Add("@Email", SqlDbType.VarChar, 30).Value = Db(model.Email);
        cmd.Parameters.Add("@Telefono", SqlDbType.VarChar, 30).Value = Db(model.Telefono);
        cmd.Parameters.Add("@Ciudad", SqlDbType.VarChar, 100).Value = Db(model.Ciudad);
        cmd.Parameters.Add("@Estado", SqlDbType.VarChar, 100).Value = Db(model.Estado);
        cmd.Parameters.Add("@Puesto", SqlDbType.VarChar, 100).Value = Db(model.Puesto);
        cmd.Parameters.Add("@Telefono_Movil", SqlDbType.VarChar, 30).Value = Db(model.TelefonoMovil);
        cmd.Parameters.Add("@Codigo_Postal", SqlDbType.VarChar, 100).Value = Db(model.CodigoPostal);
        cmd.Parameters.Add("@Pais", SqlDbType.VarChar, 200).Value = Db(model.Pais);
        cmd.Parameters.Add("@Estatus", SqlDbType.VarChar, 200).Value = Db(model.Estatus);
        cmd.Parameters.Add("@Identificacion", SqlDbType.VarChar, 200).Value = Db(model.Identificacion);

        await cn.OpenAsync();
        await cmd.ExecuteNonQueryAsync();
      }

      return id;

      // Convierte null/"" en DBNull.Value (la tabla permite NULL)
      static object Db(string? s) => string.IsNullOrWhiteSpace(s) ? DBNull.Value : s.Trim();
    }

    #endregion FIN CONTACTOS

    [HttpGet]
    public async Task<IActionResult> Empresas(string? q, string? todos, string? estatus, string? cmd, int page = 1, int pageSize = 5)
    {
      if (!string.IsNullOrWhiteSpace(cmd))
        page = 1;

      if (page <= 0) page = 1;
      if (pageSize <= 0) pageSize = 5;

      var (items, total) = await SearchEmpresasAsync(q, todos, estatus, page, pageSize);

      var vm = new EmpresaIndexVm
      {
        Items = items,
        CurrentPage = page,
        PageSize = pageSize,
        TotalItems = total,
        Query = q,
        Todos = todos,
        EstatusFilter = estatus,
        FormData = new Empresa()
      };
      vm.EmpresasOptions = await GetEmpresasOptionsAsync(vm.FormData?.IdEmpresa);

      return View(vm);
    }


    [HttpGet]
    public async Task<IActionResult> Info(Guid id)
    {
      var cs = System.Configuration.ConfigurationManager
                  .ConnectionStrings["ServerDiverscan"].ConnectionString;

      const string SQL = @"SELECT IDENTIFICACION FROM dbo.EMPRESA WITH (NOLOCK) WHERE ID_EMPRESA = @ID;";
      using var cn = new SqlConnection(cs);
      using var cmd = new SqlCommand(SQL, cn);
      cmd.Parameters.Add("@ID", SqlDbType.UniqueIdentifier).Value = id;
      await cn.OpenAsync();
      var obj = await cmd.ExecuteScalarAsync();
      var identificacion = obj == null || obj == DBNull.Value ? null : (string)obj;
      return Json(new { id, identificacion });
    }

    public static async Task<Empresa?> GetEmpresaByIdAsync(Guid id)
    {
      string cs = System.Configuration.ConfigurationManager
          .ConnectionStrings["ServerDiverscan"].ConnectionString;

      const string SQL = @"
SELECT
    ID_EMPRESA,
    NOMBRE,
    DESCRIPCION,
    DIRECCION_EMPRESA,
    IDENTIFICACION_E_RELACIONADA,
    EMPRESA_RELACIONADA,
    DIRECCION_E_RELACIONADA,
    EMAIL,
    TELEFONO,
    CIUDAD,
    ESTADO,
    FORMA_PAGO,
    CONDICION_FINANCIERA,
    CODIGO_POSTAL,
    PAIS,
    ESTATUS,
    IDENTIFICACION,
    ID_EMPRESA_RELACIONADA       
FROM dbo.EMPRESA WITH (NOLOCK)
WHERE ID_EMPRESA = @ID;";

      using var cn = new SqlConnection(cs);
      using var cmd = new SqlCommand(SQL, cn);
      cmd.Parameters.Add("@ID", SqlDbType.UniqueIdentifier).Value = id;

      await cn.OpenAsync();
      using var rd = await cmd.ExecuteReaderAsync();
      if (!await rd.ReadAsync()) return null;

      var emp = new Empresa
      {
        IdEmpresa = rd.GetGuid(rd.GetOrdinal("ID_EMPRESA")),
        Nombre = rd["NOMBRE"] as string,
        Descripcion = rd["DESCRIPCION"] as string,
        DireccionEmpresa = rd["DIRECCION_EMPRESA"] as string,
        IdentificacionERelacionada = rd["IDENTIFICACION_E_RELACIONADA"] as string,
        EmpresaRelacionada = rd["EMPRESA_RELACIONADA"] as string,
        DireccionERelacionada = rd["DIRECCION_E_RELACIONADA"] as string,
        Email = rd["EMAIL"] as string,
        Telefono = rd["TELEFONO"] as string,
        Ciudad = rd["CIUDAD"] as string,
        Estado = rd["ESTADO"] as string,
        FormaPago = rd["FORMA_PAGO"] as string,
        CondicionFinanciera = rd["CONDICION_FINANCIERA"] as string,
        CodigoPostal = rd["CODIGO_POSTAL"] as string,
        Pais = rd["PAIS"] as string,
        Estatus = rd["ESTATUS"] as string,
        Identificacion = rd["IDENTIFICACION"] as string,
        // 👇 mapear la relación
        EmpresaRelacionadaId = rd.IsDBNull(rd.GetOrdinal("ID_EMPRESA_RELACIONADA"))
                                 ? (Guid?)null
                                 : rd.GetGuid(rd.GetOrdinal("ID_EMPRESA_RELACIONADA"))
      };

      return emp;
    }


    [HttpGet]
    public async Task<IActionResult> Editar(
        Guid id,
        string? q = null,
        string? todos = null,
        string? estatus = null,   // ← agregar
        int page = 1,
        int pageSize = 5)
    {
      var empresa = await GetEmpresaByIdAsync(id);
      if (empresa == null)
      {
        TempData["AlertTitle"] = "Aviso";
        TempData["Alert"] = "No se encontró la empresa seleccionada.";
        return RedirectToAction("Empresas", new { q, todos, estatus, page, pageSize });
      }

      // Llamada correcta con 'estatus'
      var (items, total) = await SearchEmpresasAsync(q, todos, estatus, page, pageSize);

      var vm = new AspnetCoreMvcFull.Models.Empresa.EmpresaIndexVm
      {
        FormData = empresa,
        Items = items,
        CurrentPage = page,
        PageSize = pageSize,
        TotalItems = total,
        Query = q,
        Todos = todos,
        EstatusFilter = estatus   // ← para que el select quede seleccionado
      };

      vm.EmpresasOptions = await GetEmpresasOptionsAsync(empresa.IdEmpresa);
      return View("Empresas", vm);
    }




    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarEmpresa(Guid id, int page = 1, int pageSize = 5, string? q = null, string? todos = null)
    {
      // 1) Validar referencias
      var (contratos, traficos) = await CountReferenciasEmpresaAsync(id);
      if (contratos > 0 || traficos > 0)
      {
        TempData["AlertTitle"] = "No se puede eliminar";
        TempData["Alert"] = $"La empresa tiene {contratos} contrato(s) y {traficos} registro(s) en Control de Tráfico relacionados.";
        return RedirectToAction("Empresas", new { page, pageSize, q, todos });
      }

      // 2) Eliminar
      var rows = await DeleteEmpresaAsync(id);
      if (rows > 0)
      {
        TempData["AlertTitle"] = "Éxito";
        TempData["Alert"] = "Empresa eliminada correctamente";
      }
      else
      {
        TempData["AlertTitle"] = "Aviso";
        TempData["Alert"] = "No se pudo eliminar la empresa (puede que no exista o esté referenciada).";
      }

      // 3) Volver a la misma página con filtros
      return RedirectToAction("Empresas", new { page, pageSize, q, todos });
    }


    [HttpGet]
    public async Task<IActionResult> Historial(Guid id, int top = 200)
    {
      var rows = new List<HistorialRow>();
      try
      {
        // Cadena en appsettings.json → ConnectionStrings:ServiceTrackID
        var cs = System.Configuration.ConfigurationManager
                      .ConnectionStrings["ServerDiverscan"].ConnectionString;
        await using var cn = new SqlConnection(cs);
        await cn.OpenAsync();

        // Join explícito (por si luego quieres mostrar campos de EMPRESA)
        var sql = @"
SELECT TOP (@top)
  ct.Ticket,
  ct.FechaCreacionUtc,
  ct.FechaProximoServicio,
  CONVERT(varchar(8), ct.HoraServicio, 108) AS HoraServicio,
  ct.TelefonoServicio,
  ct.EmailServicio,
  ct.CanalEmail,
  ct.CanalWeb,
  ct.CanalPresencial,
  ct.CanalTelefono,
  ct.CanalChatbot,
  ct.GD,
  ct.SC,
  ct.SID
FROM dbo.CONTROL_TRAFICO ct
INNER JOIN dbo.EMPRESA e ON e.ID_EMPRESA = ct.EmpresaId
WHERE e.ID_EMPRESA = @id
ORDER BY ct.Ticket DESC;";

        await using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.Add(new SqlParameter("@id", SqlDbType.UniqueIdentifier) { Value = id });
        cmd.Parameters.Add(new SqlParameter("@top", SqlDbType.Int) { Value = top });

        await using var rd = await cmd.ExecuteReaderAsync();
        while (await rd.ReadAsync())
        {
          rows.Add(new HistorialRow
          {
            ticket = rd["Ticket"] as int? ?? default,
            fechaCreacionUtc = rd["FechaCreacionUtc"] as DateTime?,
            fechaProximoServicio = rd["FechaProximoServicio"] as DateTime?,
            horaServicio = rd["HoraServicio"] as string ?? string.Empty,
            telefonoServicio = rd["TelefonoServicio"] as string ?? string.Empty,
            emailServicio = rd["EmailServicio"] as string ?? string.Empty,
            canalEmail = rd["CanalEmail"] as bool? ?? false,
            canalWeb = rd["CanalWeb"] as bool? ?? false,
            canalPresencial = rd["CanalPresencial"] as bool? ?? false,
            canalTelefono = rd["CanalTelefono"] as bool? ?? false,
            canalChatbot = rd["CanalChatbot"] as bool? ?? false,
            gd = rd["GD"] as bool? ?? false,
            sc = rd["SC"] as bool? ?? false,
            sid = rd["SID"] as bool? ?? false,
          });
        }
        return Json(new { ok = true, rows });
      }
      catch (Exception ex)
      {
        return Json(new { ok = false, error = ex.Message });
      }
    }

    public class HistorialRow
    {
      public int ticket { get; set; }
      public DateTime? fechaCreacionUtc { get; set; }
      public DateTime? fechaProximoServicio { get; set; }
      public string horaServicio { get; set; } = "";
      public string telefonoServicio { get; set; } = "";
      public string emailServicio { get; set; } = "";
      public bool canalEmail { get; set; }
      public bool canalWeb { get; set; }
      public bool canalPresencial { get; set; }
      public bool canalTelefono { get; set; }
      public bool canalChatbot { get; set; }
      public bool gd { get; set; }
      public bool sc { get; set; }
      public bool sid { get; set; }
    }

    public static async Task<(List<Empresa> items, int total)> SearchEmpresasAsync(
    string? q, string? todos, string? estatus, int page, int pageSize)
    {
      string cs = System.Configuration.ConfigurationManager
                      .ConnectionStrings["ServerDiverscan"].ConnectionString;

      if (page <= 0) page = 1;
      if (pageSize <= 0) pageSize = 5;

      var whereParts = new List<string>();

      // CAMBIO: ampliar 'q' para buscar también por IDENTIFICACION_E_RELACIONADA e IDENTIFICACION
      if (!string.IsNullOrWhiteSpace(q))
        whereParts.Add(@"(
      [NOMBRE] LIKE @q
      OR [PAIS] LIKE @q
      OR [FORMA_PAGO] LIKE @q
      OR [CONDICION_FINANCIERA] LIKE @q
      OR [IDENTIFICACION_E_RELACIONADA] LIKE @q
      OR [IDENTIFICACION] LIKE @q
    )");

      // 'todos' se mantiene como estaba; si quisieras incluir también IDENTIFICACION* avísame y lo agrego
      if (!string.IsNullOrWhiteSpace(todos))
        whereParts.Add(@"(
      [NOMBRE] LIKE @todos
      OR [PAIS] LIKE @todos
      OR [FORMA_PAGO] LIKE @todos
      OR [CONDICION_FINANCIERA] LIKE @todos
    )");

      if (!string.IsNullOrWhiteSpace(estatus))
        whereParts.Add("[ESTATUS] = @estatus"); // igualdad exacta

      var where = whereParts.Count > 0 ? "WHERE " + string.Join(" AND ", whereParts) : "";

      var sqlCount = $@"SELECT COUNT(*) FROM [dbo].[EMPRESA] WITH (NOLOCK) {where};";

      var sqlPage = $@"
SELECT [ID_EMPRESA], [NOMBRE], [PAIS], [FORMA_PAGO], [CONDICION_FINANCIERA], [TELEFONO]
FROM [dbo].[EMPRESA] WITH (NOLOCK)
{where}
ORDER BY [NOMBRE]
OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY;";

      var items = new List<Empresa>();
      int total = 0;

      using var cn = new SqlConnection(cs);
      await cn.OpenAsync();

      // COUNT
      using (var cmd = new SqlCommand(sqlCount, cn))
      {
        if (!string.IsNullOrWhiteSpace(q)) cmd.Parameters.Add("@q", SqlDbType.VarChar, 200).Value = $"%{q!.Trim()}%";
        if (!string.IsNullOrWhiteSpace(todos)) cmd.Parameters.Add("@todos", SqlDbType.VarChar, 200).Value = $"%{todos!.Trim()}%";
        if (!string.IsNullOrWhiteSpace(estatus)) cmd.Parameters.Add("@estatus", SqlDbType.VarChar, 200).Value = estatus!.Trim();
        total = Convert.ToInt32(await cmd.ExecuteScalarAsync());
      }

      // PAGE
      using (var cmd = new SqlCommand(sqlPage, cn))
      {
        if (!string.IsNullOrWhiteSpace(q)) cmd.Parameters.Add("@q", SqlDbType.VarChar, 200).Value = $"%{q!.Trim()}%";
        if (!string.IsNullOrWhiteSpace(todos)) cmd.Parameters.Add("@todos", SqlDbType.VarChar, 200).Value = $"%{todos!.Trim()}%";
        if (!string.IsNullOrWhiteSpace(estatus)) cmd.Parameters.Add("@estatus", SqlDbType.VarChar, 200).Value = estatus!.Trim();

        cmd.Parameters.Add("@skip", SqlDbType.Int).Value = (page - 1) * pageSize;
        cmd.Parameters.Add("@take", SqlDbType.Int).Value = pageSize;

        using var rd = await cmd.ExecuteReaderAsync();
        while (await rd.ReadAsync())
        {
          items.Add(new Empresa
          {
            IdEmpresa = rd.GetGuid(rd.GetOrdinal("ID_EMPRESA")),
            Nombre = rd["NOMBRE"] as string,
            Pais = rd["PAIS"] as string,
            FormaPago = rd["FORMA_PAGO"] as string,
            CondicionFinanciera = rd["CONDICION_FINANCIERA"] as string,
            Telefono = rd["TELEFONO"] as string
          });
        }
      }

      return (items, total);
    }



    private async Task<IEnumerable<SelectListItem>> GetEmpresasOptionsAsync(Guid? excluirId = null)
    {
      var cs = System.Configuration.ConfigurationManager
                  .ConnectionStrings["ServerDiverscan"].ConnectionString;

      const string SQL = @"
SELECT ID_EMPRESA, NOMBRE
FROM dbo.EMPRESA WITH (NOLOCK)
ORDER BY NOMBRE;";

      var list = new List<SelectListItem>();

      using var cn = new SqlConnection(cs);
      using var cmd = new SqlCommand(SQL, cn);
      await cn.OpenAsync();
      using var rd = await cmd.ExecuteReaderAsync();
      while (await rd.ReadAsync())
      {
        var id = rd.GetGuid(0);
        if (excluirId.HasValue && id == excluirId.Value) continue; // evita autorelación en el selector

        var nombre = rd.IsDBNull(1) ? "" : rd.GetString(1);
        list.Add(new SelectListItem { Value = id.ToString(), Text = nombre });
      }
      return list;
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmarActualizacionDuplicado(
       Guid id, string token, int page = 1, int pageSize = 5, string? q = null, string? todos = null)
    {
      if (string.IsNullOrWhiteSpace(token) || !_cache.TryGetValue("dup:" + token, out string payload))
      {
        TempData["AlertTitle"] = "Aviso";
        TempData["Alert"] = "No se encontró la información a actualizar (puede haber expirado).";
        return RedirectToAction("Empresas", new { page, pageSize, q, todos });
      }

      _cache.Remove("dup:" + token);

      var incoming = System.Text.Json.JsonSerializer.Deserialize<AspnetCoreMvcFull.Models.Empresa.Empresa>(payload ?? "");
      if (incoming == null)
      {
        TempData["AlertTitle"] = "Aviso";
        TempData["Alert"] = "No se pudo procesar la información.";
        return RedirectToAction("Empresas", new { page, pageSize, q, todos });
      }

      incoming.IdEmpresa = id;
      var rows = await UpdateEmpresaAsync(incoming);

      TempData["AlertTitle"] = rows > 0 ? "Éxito" : "Aviso";
      TempData["Alert"] = rows > 0 ? "Empresa actualizada correctamente" : "No se pudo actualizar la empresa.";

      return RedirectToAction("Empresas", new { page, pageSize, q, todos });
    }




    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GuardarEmpresa(AspnetCoreMvcFull.Models.Empresa.Empresa model)
    {
      var estado = (Request.Form["estadoFomrulario"].ToString() ?? "").Trim();

      int page = int.TryParse(Request.Form["page"], out var p) ? p : 1;
      int pageSize = int.TryParse(Request.Form["pageSize"], out var ps) ? ps : 5;
      string q = Request.Form["q"].ToString() ?? "";
      string todos = Request.Form["todos"].ToString() ?? "";

      // Inserción -> chequear duplicado por nombre
      if (!string.Equals(estado, "Editar", StringComparison.OrdinalIgnoreCase))
      {
        var nombre = (model.Nombre ?? "").Trim();
        if (!string.IsNullOrEmpty(nombre))
        {
          var existenteId = await GetEmpresaIdByNombreAsync(nombre);
          if (existenteId.HasValue)
          {
            // 1) Cachear el JSON 5 min
            var token = Guid.NewGuid().ToString("N");
            _cache.Set("dup:" + token,
                       System.Text.Json.JsonSerializer.Serialize(model),
                       new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) });

            // 2) Redirigir con parámetros chiquitos
            return RedirectToAction("Empresas", new
            {
              page,
              pageSize,
              q,
              todos,
              dupId = existenteId.Value,   // Guid existente
              dupName = nombre,            // nombre existente
              dupToken = token             // token para recuperar del cache
            });
          }
        }

        // No hay duplicado → insertar normal
        var idCreado = await InsertEmpresaAsync(model);
 

        TempData["AlertTitle"] = "Éxito";
        TempData["Alert"] = "Empresa registrada correctamente";
        return RedirectToAction("Empresas", new { page, pageSize, q, todos });
      }


      // Editar
      if (model.IdEmpresa != Guid.Empty)
      {
        var rows = await UpdateEmpresaAsync(model);
        TempData["AlertTitle"] = rows > 0 ? "Éxito" : "Aviso";
        TempData["Alert"] = rows > 0 ? "Empresa actualizada correctamente" : "No se encontró la empresa a actualizar.";
      }
      else
      {
        TempData["AlertTitle"] = "Aviso";
        TempData["Alert"] = "No se proporcionó un ID válido para editar.";
      }

      return RedirectToAction("Empresas", new { page, pageSize, q, todos });
    }

    private static async Task EnsureRelacionBidireccionalAsync(Guid aId, Guid bId)
    {
      if (aId == Guid.Empty || bId == Guid.Empty || aId == bId) return;

      string cs = System.Configuration.ConfigurationManager
                    .ConnectionStrings["ServerDiverscan"].ConnectionString;

      // Establece B -> A. (Simple. Si B ya apuntaba a otra, lo sobreescribe.)
      const string SQL = @"UPDATE dbo.EMPRESA SET ID_EMPRESA_RELACIONADA = @A WHERE ID_EMPRESA = @B;";

      using var cn = new SqlConnection(cs);
      using var cmd = new SqlCommand(SQL, cn);
      cmd.Parameters.Add("@A", SqlDbType.UniqueIdentifier).Value = aId;
      cmd.Parameters.Add("@B", SqlDbType.UniqueIdentifier).Value = bId;
      await cn.OpenAsync();
      await cmd.ExecuteNonQueryAsync();
    }


    public static async Task<(int contratos, int traficos)> CountReferenciasEmpresaAsync(Guid id)
    {
      string cs = System.Configuration.ConfigurationManager
          .ConnectionStrings["ServerDiverscan"].ConnectionString;

      const string SQL = @"
SELECT
    (SELECT COUNT(*) FROM dbo.CONTRATOS WITH (NOLOCK) WHERE ID_EMPRESA = @ID) AS contratos,
    (SELECT COUNT(*) FROM dbo.CONTROL_TRAFICO WITH (NOLOCK) WHERE EmpresaId = @ID) AS traficos;";

      using var cn = new SqlConnection(cs);
      using var cmd = new SqlCommand(SQL, cn);
      cmd.Parameters.Add("@ID", SqlDbType.UniqueIdentifier).Value = id;

      await cn.OpenAsync();
      using var rd = await cmd.ExecuteReaderAsync();
      if (await rd.ReadAsync())
      {
        int c = Convert.ToInt32(rd["contratos"]);
        int t = Convert.ToInt32(rd["traficos"]);
        return (c, t);
      }
      return (0, 0);
    }

   public static async Task<int> DeleteEmpresaAsync(Guid id)
{
    string cs = System.Configuration.ConfigurationManager
        .ConnectionStrings["ServerDiverscan"].ConnectionString;

    using var cn = new SqlConnection(cs);
    await cn.OpenAsync();
    using var tx = cn.BeginTransaction();

    try
    {
        // 0) Verifica que exista
        const string SQL_EXISTS = "SELECT 1 FROM dbo.EMPRESA WITH (NOLOCK) WHERE ID_EMPRESA = @ID;";
        using (var cmdExists = new SqlCommand(SQL_EXISTS, cn, tx))
        {
            cmdExists.Parameters.Add("@ID", SqlDbType.UniqueIdentifier).Value = id;
            var exists = await cmdExists.ExecuteScalarAsync();
            if (exists == null)
            {
                tx.Rollback();
                return 0; // no encontrada
            }
        }

        // 1) Bloquear si está usada en otras tablas (regla de negocio)
        const string SQL_CHECKS = @"
SELECT 
  (SELECT COUNT(*) FROM dbo.CONTRATOS       WITH (NOLOCK) WHERE ID_EMPRESA = @ID) AS C_Contratos,
  (SELECT COUNT(*) FROM dbo.CONTROL_TRAFICO WITH (NOLOCK) WHERE EmpresaId  = @ID) AS C_Trafico;";
        using (var cmdCheck = new SqlCommand(SQL_CHECKS, cn, tx))
        {
            cmdCheck.Parameters.Add("@ID", SqlDbType.UniqueIdentifier).Value = id;
            using var rd = await cmdCheck.ExecuteReaderAsync();
            int cContratos = 0, cTrafico = 0;
            if (await rd.ReadAsync())
            {
                cContratos = Convert.ToInt32(rd["C_Contratos"]);
                cTrafico   = Convert.ToInt32(rd["C_Trafico"]);
            }
            rd.Close();

            if (cContratos > 0 || cTrafico > 0)
            {
                tx.Rollback();
                return -2; // referenciada en otras tablas
            }
        }

        // 2) Romper autorrelación: poner en NULL a los que apunten a esta empresa
        const string SQL_NULL_SELF = @"
UPDATE dbo.EMPRESA
SET ID_EMPRESA_RELACIONADA = NULL
WHERE ID_EMPRESA_RELACIONADA = @ID;";
        using (var cmdNull = new SqlCommand(SQL_NULL_SELF, cn, tx))
        {
            cmdNull.Parameters.Add("@ID", SqlDbType.UniqueIdentifier).Value = id;
            await cmdNull.ExecuteNonQueryAsync();
        }

        // 3) Borrar la empresa
        const string SQL_DELETE = "DELETE FROM dbo.EMPRESA WHERE ID_EMPRESA = @ID;";
        int rows;
        using (var cmdDel = new SqlCommand(SQL_DELETE, cn, tx))
        {
            cmdDel.Parameters.Add("@ID", SqlDbType.UniqueIdentifier).Value = id;
            rows = await cmdDel.ExecuteNonQueryAsync(); // 1 si eliminó
        }

        tx.Commit();
        return rows;
    }
    catch
    {
        try { tx.Rollback(); } catch { /* noop */ }
        throw;
    }
}



    public static async Task<int> UpdateEmpresaAsync(Empresa model)
    {
      string connectionString = System.Configuration.ConfigurationManager
          .ConnectionStrings["ServerDiverscan"].ConnectionString;

      const string SQL = @"
UPDATE dbo.EMPRESA
SET
    NOMBRE                     = @NOMBRE,
    DESCRIPCION                = @DESCRIPCION,
    DIRECCION_EMPRESA          = @DIRECCION_EMPRESA,
    IDENTIFICACION_E_RELACIONADA = @IDENTIFICACION_E_RELACIONADA,
    EMPRESA_RELACIONADA        = @EMPRESA_RELACIONADA,
    DIRECCION_E_RELACIONADA    = @DIRECCION_E_RELACIONADA,
    EMAIL                      = @EMAIL,
    TELEFONO                   = @TELEFONO,
    CIUDAD                     = @CIUDAD,
    ESTADO                     = @ESTADO,
    FORMA_PAGO                 = @FORMA_PAGO,
    CONDICION_FINANCIERA       = @CONDICION_FINANCIERA,
    CODIGO_POSTAL              = @CODIGO_POSTAL,
    PAIS                       = @PAIS,
    ESTATUS                    = @ESTATUS,
    IDENTIFICACION             = @IDENTIFICACION,
    ID_EMPRESA_RELACIONADA = @ID_EMPRESA_RELACIONADA

WHERE ID_EMPRESA = @ID_EMPRESA;";

      using (var cn = new SqlConnection(connectionString))
      using (var cmd = new SqlCommand(SQL, cn))
      {
        cmd.Parameters.Add("@ID_EMPRESA", SqlDbType.UniqueIdentifier).Value = model.IdEmpresa;

        cmd.Parameters.Add("@NOMBRE", SqlDbType.VarChar, 100).Value = Db(model.Nombre);
        cmd.Parameters.Add("@DESCRIPCION", SqlDbType.VarChar, 100).Value = Db(model.Descripcion);
        cmd.Parameters.Add("@DIRECCION_EMPRESA", SqlDbType.VarChar, 200).Value = Db(model.DireccionEmpresa);
        cmd.Parameters.Add("@IDENTIFICACION_E_RELACIONADA", SqlDbType.VarChar, 200).Value = Db(model.IdentificacionERelacionada);
        cmd.Parameters.Add("@EMPRESA_RELACIONADA", SqlDbType.VarChar, 200).Value = Db(model.EmpresaRelacionada);
        cmd.Parameters.Add("@DIRECCION_E_RELACIONADA", SqlDbType.VarChar, 200).Value = Db(model.DireccionERelacionada);
        cmd.Parameters.Add("@EMAIL", SqlDbType.VarChar, 30).Value = Db(model.Email);
        cmd.Parameters.Add("@TELEFONO", SqlDbType.VarChar, 30).Value = Db(model.Telefono);
        cmd.Parameters.Add("@CIUDAD", SqlDbType.VarChar, 100).Value = Db(model.Ciudad);
        cmd.Parameters.Add("@ESTADO", SqlDbType.VarChar, 100).Value = Db(model.Estado);
        cmd.Parameters.Add("@FORMA_PAGO", SqlDbType.VarChar, 100).Value = Db(model.FormaPago);
        cmd.Parameters.Add("@CONDICION_FINANCIERA", SqlDbType.VarChar, 200).Value = Db(model.CondicionFinanciera);
        cmd.Parameters.Add("@CODIGO_POSTAL", SqlDbType.VarChar, 100).Value = Db(model.CodigoPostal);
        cmd.Parameters.Add("@PAIS", SqlDbType.VarChar, 200).Value = Db(model.Pais);
        cmd.Parameters.Add("@ESTATUS", SqlDbType.VarChar, 200).Value = Db(model.Estatus);
        cmd.Parameters.Add("@IDENTIFICACION", SqlDbType.VarChar, 200).Value = Db(model.Identificacion);
        cmd.Parameters.Add("@ID_EMPRESA_RELACIONADA", SqlDbType.UniqueIdentifier)
   .Value = model.EmpresaRelacionadaId.HasValue ? model.EmpresaRelacionadaId.Value : (object)DBNull.Value;


        await cn.OpenAsync();
        var rows = await cmd.ExecuteNonQueryAsync();
        return rows; // >0 si actualizó
      }
      if (model.EmpresaRelacionadaId.HasValue)
      {
        await EnsureRelacionBidireccionalAsync(model.IdEmpresa, model.EmpresaRelacionadaId.Value);
      }


      static object Db(string? s) => string.IsNullOrWhiteSpace(s) ? DBNull.Value : s.Trim();
    }

    public static async Task<Guid> InsertEmpresaAsync(Empresa model)
    {
      string connectionString = System.Configuration.ConfigurationManager
          .ConnectionStrings["ServerDiverscan"].ConnectionString;

      Guid id = Guid.NewGuid();

      const string SQL = @"
INSERT INTO dbo.EMPRESA
(
  ID_EMPRESA, NOMBRE, DESCRIPCION, DIRECCION_EMPRESA,
  IDENTIFICACION_E_RELACIONADA, EMPRESA_RELACIONADA, DIRECCION_E_RELACIONADA,
  EMAIL, TELEFONO, CIUDAD, ESTADO, FORMA_PAGO, CONDICION_FINANCIERA,
  CODIGO_POSTAL, PAIS, ESTATUS, IDENTIFICACION,
  ID_EMPRESA_RELACIONADA
)
VALUES
(
  @ID_EMPRESA, @NOMBRE, @DESCRIPCION, @DIRECCION_EMPRESA,
  @IDENTIFICACION_E_RELACIONADA, @EMPRESA_RELACIONADA, @DIRECCION_E_RELACIONADA,
  @EMAIL, @TELEFONO, @CIUDAD, @ESTADO, @FORMA_PAGO, @CONDICION_FINANCIERA,
  @CODIGO_POSTAL, @PAIS, @ESTATUS, @IDENTIFICACION,
  @ID_EMPRESA_RELACIONADA
);";

      using (var cn = new SqlConnection(connectionString))
      using (var cmd = new SqlCommand(SQL, cn))
      {
        cmd.Parameters.Add("@ID_EMPRESA", SqlDbType.UniqueIdentifier).Value = id;

        cmd.Parameters.Add("@NOMBRE", SqlDbType.VarChar, 100).Value = Db(model.Nombre);
        cmd.Parameters.Add("@DESCRIPCION", SqlDbType.VarChar, 100).Value = Db(model.Descripcion);
        cmd.Parameters.Add("@DIRECCION_EMPRESA", SqlDbType.VarChar, 200).Value = Db(model.DireccionEmpresa);
        cmd.Parameters.Add("@IDENTIFICACION_E_RELACIONADA", SqlDbType.VarChar, 200).Value = Db(model.IdentificacionERelacionada);
        cmd.Parameters.Add("@EMPRESA_RELACIONADA", SqlDbType.VarChar, 200).Value = Db(model.EmpresaRelacionada);
        cmd.Parameters.Add("@DIRECCION_E_RELACIONADA", SqlDbType.VarChar, 200).Value = Db(model.DireccionERelacionada);
        cmd.Parameters.Add("@EMAIL", SqlDbType.VarChar, 30).Value = Db(model.Email);
        cmd.Parameters.Add("@TELEFONO", SqlDbType.VarChar, 30).Value = Db(model.Telefono);
        cmd.Parameters.Add("@CIUDAD", SqlDbType.VarChar, 100).Value = Db(model.Ciudad);
        cmd.Parameters.Add("@ESTADO", SqlDbType.VarChar, 100).Value = Db(model.Estado);
        cmd.Parameters.Add("@FORMA_PAGO", SqlDbType.VarChar, 100).Value = Db(model.FormaPago);
        cmd.Parameters.Add("@CONDICION_FINANCIERA", SqlDbType.VarChar, 200).Value = Db(model.CondicionFinanciera);
        cmd.Parameters.Add("@CODIGO_POSTAL", SqlDbType.VarChar, 100).Value = Db(model.CodigoPostal);
        cmd.Parameters.Add("@PAIS", SqlDbType.VarChar, 200).Value = Db(model.Pais);
        cmd.Parameters.Add("@ESTATUS", SqlDbType.VarChar, 200).Value = Db(model.Estatus);
        cmd.Parameters.Add("@IDENTIFICACION", SqlDbType.VarChar, 200).Value = Db(model.Identificacion);
        cmd.Parameters.Add("@ID_EMPRESA_RELACIONADA", SqlDbType.UniqueIdentifier)
   .Value = model.EmpresaRelacionadaId.HasValue ? model.EmpresaRelacionadaId.Value : (object)DBNull.Value;

        await cn.OpenAsync();
        await cmd.ExecuteNonQueryAsync();
      }

      if (model.EmpresaRelacionadaId.HasValue)
      {
        await EnsureRelacionBidireccionalAsync(id, model.EmpresaRelacionadaId.Value);
      }

      return id;

      static object Db(string? s) => string.IsNullOrWhiteSpace(s) ? DBNull.Value : s.Trim();
    }

    public static async Task<Guid?> GetEmpresaIdByNombreAsync(string nombre)
    {
      string cs = System.Configuration.ConfigurationManager
          .ConnectionStrings["ServerDiverscan"].ConnectionString;

      const string SQL = @"
SELECT TOP (1) ID_EMPRESA
FROM dbo.EMPRESA WITH (NOLOCK)
WHERE UPPER(LTRIM(RTRIM(NOMBRE))) = UPPER(LTRIM(RTRIM(@NOMBRE)));";

      using var cn = new SqlConnection(cs);
      using var cmd = new SqlCommand(SQL, cn);
      cmd.Parameters.Add("@NOMBRE", SqlDbType.VarChar, 100).Value = (nombre ?? "").Trim();

      await cn.OpenAsync();
      var obj = await cmd.ExecuteScalarAsync();
      if (obj == null || obj == DBNull.Value) return null;
      return (Guid)obj;
    }


    private static readonly HttpClient _http = new HttpClient();

    // GET /Empresa/ObtenerPaises
    [HttpGet]
    public async Task<IActionResult> ObtenerPaises()
    {
      // RestCountries: países (sin clave). Tomamos nombre en español si existe.
      var url = "https://restcountries.com/v3.1/all?fields=name,translations";
      var json = await _http.GetStringAsync(url);
      using var doc = JsonDocument.Parse(json);

      var items = doc.RootElement
          .EnumerateArray()
          .Select(el =>
          {
            var nombre = el.GetProperty("name").GetProperty("common").GetString() ?? "";
            if (el.TryGetProperty("translations", out var tr) &&
                  tr.TryGetProperty("spa", out var spa) &&
                  spa.TryGetProperty("common", out var es) &&
                  es.GetString() is string esName && !string.IsNullOrWhiteSpace(esName))
            {
              nombre = esName;
            }
            return nombre.Trim();
          })
          .Where(s => !string.IsNullOrWhiteSpace(s))
          .Distinct(StringComparer.OrdinalIgnoreCase)
          .OrderBy(s => s)
          .Select(s => new { id = s, texto = s })
          .ToList();

      return Json(items);
    }

    // GET /Empresa/ObtenerEstados?pais=Costa%20Rica
    [HttpGet]
    public async Task<IActionResult> ObtenerEstados([FromQuery] string pais)
    {
      // countriesnow.space: estados por país (sin clave)
      var url = "https://countriesnow.space/api/v0.1/countries/states";
      var body = new { country = pais ?? "" };
      var resp = await _http.PostAsync(
          url,
          new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
      );
      var json = await resp.Content.ReadAsStringAsync();
      using var doc = JsonDocument.Parse(json);

      var items = new List<object>();
      if (doc.RootElement.TryGetProperty("data", out var data) &&
          data.TryGetProperty("states", out var states) &&
          states.ValueKind == JsonValueKind.Array)
      {
        foreach (var st in states.EnumerateArray())
        {
          var name = st.GetProperty("name").GetString() ?? "";
          if (!string.IsNullOrWhiteSpace(name))
            items.Add(new { id = name, texto = name });
        }
      }
      // Algunos países no tienen estados en la API → devolver vacío
      return Json(items);
    }

    // GET /Empresa/ObtenerCiudades?pais=Costa%20Rica&estado=Alajuela
    [HttpGet]
    public async Task<IActionResult> ObtenerCiudades([FromQuery] string pais, [FromQuery] string estado)
    {
      var url = "https://countriesnow.space/api/v0.1/countries/state/cities";
      var body = new { country = pais ?? "", state = estado ?? "" };
      var resp = await _http.PostAsync(
          url,
          new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
      );
      var json = await resp.Content.ReadAsStringAsync();
      using var doc = JsonDocument.Parse(json);

      var items = new List<object>();
      if (doc.RootElement.TryGetProperty("data", out var cities) &&
          cities.ValueKind == JsonValueKind.Array)
      {
        foreach (var c in cities.EnumerateArray())
        {
          var name = c.GetString() ?? "";
          if (!string.IsNullOrWhiteSpace(name))
            items.Add(new { id = name, texto = name });
        }
      }
      return Json(items);
    }

    public IActionResult Polizas()
    {
      ViewBag.NombreUbicacion = GetUltimoNombreUbicacionC() ?? "Ubicación C";
      ViewBag.NombreUbicacionA = GetUltimoNombreUbicacionA() ?? "Ubicación A";
      ViewBag.NombreUbicacionB = GetUltimoNombreUbicacionB() ?? "Ubicación B";

      return View();
    }
    #region Empleado
    //CLASES TEMPORALES PARA LEER LA DATA DEL JSON
    public class Employee
    {
      [JsonPropertyName("id")]
      public int Id { get; set; }

      [JsonPropertyName("name")]
      public string Name { get; set; } = "";

      [JsonPropertyName("last_name")]
      public string LastName { get; set; } = "";

      [JsonPropertyName("creation_date")]
      public DateTime CreationDate { get; set; }

      [JsonPropertyName("email")]
      public string Email { get; set; } = "";

      [JsonPropertyName("state")]
      public string State { get; set; } = "";
      public String Cedula { get; set; }
      public int NumeroEmpleado { get; set; }
    }

    public class EmployeeList
    {
      public List<Employee> Employees { get; set; } = new List<Employee>();
    }






    //METODO PARA OBTENER LOS DATOS TEMPORALES DEL JSON
    // Este metodo consula especificamente el archivo que contiene los datos JSON temporales para mostrar y los devuelve
    // en tipo Array de Empleados Employee[]
    public Employee[] get_dummy_employee_data_from_JSON()
    {

      /*
        PROCESOS PARA LEER LA DATA DEL JSON
        NO SERAN NECESARIOS UNA VEZ QUE SE UTILICE LA API
      */
      //Definir la ruta del archivo JSON
      string jsonPath = "Data/Empleados/DummyData.json";
      //Si la ruta no existe notificar en consola
      if (!System.IO.File.Exists(jsonPath))
      {
        Console.WriteLine($"El archivo no existe en la ruta: {jsonPath}");
      }
      //Leer la data del archivo JSON
      string jsonContent = System.IO.File.ReadAllText(jsonPath);


      // Deserializa el JSON para convertirlo en variable de tipo Root
      var data = JsonSerializer.Deserialize<EmployeeList>(jsonContent);

      //Definir la variable Empleados Array que se va a utilizar
      Employee[] listaEmpleados = [];

      //Si la data es valida entonces convertir de tipo List<Employee> a Employee[]
      if (data != null && data.Employees.Count != 0)
      {
        listaEmpleados = data.Employees.ToArray();
      }
      /*
        PROCESOS PARA LEER LA DATA DEL JSON
        NO SERAN NECESARIOS UNA VEZ QUE SE UTILICE LA API
      */

      return listaEmpleados;

    }


    //METODO PARA FILTRAR LA LISTA DE EMPLEADOS
    // Este metodo recibe el array de empleados que se quiere filtrar (Employee[])
    // y lo filtra segun los parametros: estado, input de busqueda, input de busqueda de ID
    // El estado puede ser activo inactivo
    // El input de busqueda filtra segun Nombre, Apellido y Correo
    // El input de busqueda de ID filtra segun el numero de ID
    public Employee[] filter_employee_list(Employee[] employee_list,
string employee_state, string employee_search_input, string employee_ID_search_input)
    {
      // FILTRADO DE EMPLEADOS SEGÚN ESTADO (Active / Inactive)
      if (!string.IsNullOrEmpty(employee_state) && employee_state == "activo")
      {
        employee_list = employee_list
          .Where(empleado =>
          {
            return empleado.State == "Activo";
          })
          .ToArray();
      }
      else if (!string.IsNullOrEmpty(employee_state) && employee_state == "inactivo")
      {
        employee_list = employee_list
          .Where(empleado =>
          {
            return empleado.State == "Inactivo";
          })
          .ToArray();
      }

      // FILTRADO SEGÚN TEXTO DE BÚSQUEDA (nombre, apellido, correo)
      // FILTRADO SEGÚN TEXTO DE BÚSQUEDA (nombre, apellido, correo)
      if (!string.IsNullOrEmpty(employee_search_input))
      {
        string lower_search_input = employee_search_input.ToLower();

        employee_list = employee_list
          .Where(empleado =>
          {
            // Combinar los campos
            string combined = $"{empleado.Name} {empleado.LastName} {empleado.Email}".ToLower();

            return combined.Contains(lower_search_input);
          })
          .ToArray();
      }


      // FILTRADO SEGÚN ID
      if (!string.IsNullOrEmpty(employee_ID_search_input))
      {
        employee_list = employee_list
          .Where(empleado =>
          {
            return empleado.Id.ToString().Contains(employee_ID_search_input);
          })
          .ToArray();
      }

      return employee_list;
    }



    //METODO PARA CREAR LA PAGINACION DE EMPLEADOS
    // Este metodo recibe el array de empleados que se quiere paginar y la cantidad de empleados por pagina
    // Retorna una lista de listas de Empleados (arrayList) donde se encuentran las paginas de empleados
    //segun la cantidad ingresada en los parametros.
    public List<List<Employee>> create_employeepages_from_employee_list(Employee[] employee_list, int employees_per_page)
    {

      //Lista de paginas de empleados divididas segun la cantidad seleccionada en la vista
      List<List<Employee>> Empleados_Pages = new List<List<Employee>>();

      //LOOP PARA DIVIDIR LA LISTA DE EMLEADOS EN PAGINAS DE LA CANTIDAD SELECCIONADA
      for (int i = 0; i < employee_list.Length; i = i + employees_per_page)
      {
        //PAGINA CORRESPONDIENTE A ITERACION
        List<Employee> employee_page = new List<Employee>();

        for (int j = i; j < i + employees_per_page; j++)
        {
          //SI EL NUMERO DE LA ITERACION NO SOBREPASA LA CANTIDAD TOTAL DE EMPLEADOS, SE AGREGA A LA PAGINA CORRESPONDIENTE
          if (j < employee_list.Length)
          {
            // Se agrega el empleado correspondiente al index en j
            // De esta manera se crean paginas segun la cantidad que deben tener
            employee_page.Add(employee_list[j]);
          }
        }
        //SE AGREGA LA PAGINA CREADA A LA LISTA DE PAGINAS
        Empleados_Pages.Add(employee_page);
      }

      return Empleados_Pages;
    }
    //METODO PARA ORDERNAR ALFABETICAMENTE LA LISTA DE EMPLEADOS


    //METODO PARA ORDENAR ALFABETICAMENTE EL ARRAY DE EMPLEADOS
    // Este metodo recibe un array de Empleados y un string donde se especifica segun que atributo se quiere ordenar
    // Los posibles atributos para odenar son: name, email y creation_date
    // Si no se ingresa ningun parametro se ordena por nombre por default
    public Employee[] order_employeelist_by(Employee[] employee_list, string order_by)
    {

      // se realiza un switch para determinar que tipo de orden se require
      switch (order_by)
      {

        case "name_ascending":
          // Ordenar alfabéticamente ascendiente segun Nombre, ignorando mayúsculas y minúsculas
          employee_list = employee_list.OrderBy(employee => employee.Name, StringComparer.OrdinalIgnoreCase).ToArray();
          break;

        case "name_descending":
          // Ordenar alfabéticamente descendiente segun Nombre, ignorando mayúsculas y minúsculas
          employee_list = employee_list.OrderByDescending(employee => employee.Name, StringComparer.OrdinalIgnoreCase).ToArray();
          break;

        case "email_ascending":
          // Ordenar alfabéticamente ascendiente segun Email, ignorando mayúsculas y minúsculas
          employee_list = employee_list.OrderBy(employee => employee.Email, StringComparer.OrdinalIgnoreCase).ToArray();
          break;

        case "email_descending":
          // Ordenar alfabéticamente descendiente segun Email, ignorando mayúsculas y minúsculas
          employee_list = employee_list.OrderByDescending(employee => employee.Email, StringComparer.OrdinalIgnoreCase).ToArray();
          break;

        case "creation_date_ascending":
          // Ordenar segun fecha de creacion, de mas antigua a mas reciente
          employee_list = employee_list.OrderBy(employee => employee.CreationDate).ToArray();
          break;

        case "creation_date_descending":
          // Ordenar segun fecha de creacion, de mas reciente a mas antigua
          employee_list = employee_list.OrderByDescending(employee => employee.CreationDate).ToArray();
          break;

        default:
          // Ordenar alfabéticamente segun Nombre, ignorando mayúsculas y minúsculas
          employee_list = employee_list.OrderBy(employee => employee.Name, StringComparer.OrdinalIgnoreCase).ToArray();
          break;
      }

      return employee_list;
    }



    [HttpGet]
    public IActionResult BusinessEmployeeList(string employee_state, string employee_search_input = "",
     string employee_ID_search_input = "", string order_by = "name_ascending", int employees_per_page = 5, int page_number = 1)
    {


      //Se llama al metodo para obtener los datos del JSON
      Employee[] employee_list_from_JSON = GetEmployees().ToArray();

      //Se llama al metodo para filtrar los empleados segun Estado, Nombre, Apellido, Correo e ID
      Employee[] filtered_employee_list =
      filter_employee_list(employee_list_from_JSON, employee_state, employee_search_input, employee_ID_search_input);


      //Se orderna el array de usuarios despues de ser filtrado
      Employee[] filtered_employee_list_ordered = order_employeelist_by(filtered_employee_list, order_by);



      //Se llama al metodo que crea la paginacion de la lista de empleados segun los parametros designados
      List<List<Employee>> Empleados_Pages = create_employeepages_from_employee_list(filtered_employee_list_ordered, employees_per_page);

      //Definir la variable que va a contener los empleados de la pagina a mostrar
      Employee[] selected_employee_page = [];

      //Si el numero de pagina es 0 se asigna a 1 porque la pagina 0 no existe
      if (page_number == 0) page_number = 1;

      //Si el numero de pagina seleccionado es mayor a la cantidad total de paginas, se asigna la ultima pagina, si no se mantiene
      page_number = page_number >= Empleados_Pages.Count ? Empleados_Pages.Count : page_number;


      // SI EXISTEN PAGINAS EN LA LISTA DE PAGINAS, SE ASIGNA LA PAGINA CORRESPONDIENTE
      // SI NO, LA LISTA QUEDA VACIA YA QUE NO SE ENCONTRÓ NINGÚN EMPLEADO
      if (Empleados_Pages.Count != 0 && page_number != 0)
      {

        //Se asigna la pagina correspondiente al array de empleados que se va a utilizar
        selected_employee_page = Empleados_Pages.ElementAt(
        // Si el numero de pagina que se seleccionó es mayor a la cantidad de paginas disponibles
        page_number > Empleados_Pages.Count
        // Se asigna la primera pagina ya que se excedio la cantidad maxima
        ? 0
        // Si no, se asigna el numero de pagina -1 lo que corresponde al index correcto de la pagina en la lista de paginas
        : page_number - 1)
        .ToArray();
      }




      //USO DE DICCIONARIO VIEWDATA PARA ENVIAR DATOS A LA VISTA

      //Total de paginas
      ViewData["Total_Pages"] = Empleados_Pages.Count;
      //Pagina actual
      ViewData["Current_Page"] = page_number;
      //Empleados por pagina
      ViewData["Employees_Per_Page"] = employees_per_page;
      //Columna que dicta orden da datos
      ViewData["Order_By"] = order_by;
      //Filtro de estado de empleados
      ViewData["Employee_State"] = employee_state;
      //Filtro de busqueda segun ID de empleado
      ViewData["Employee_ID_Search_Input"] = employee_ID_search_input;
      //Filtro de busqueda segun nombre, apellido y correo
      ViewData["Employee_Search_Input"] = employee_search_input;

      ViewBag.TotalEmpleados = ObtenerCantidadEmpleados();
      ViewBag.NombreUbicacionA = GetUltimoNombreUbicacionA() ?? "Ubicación A";
      ViewBag.NombreUbicacion = GetUltimoNombreUbicacionC() ?? "Ubicación C";
      ViewBag.NombreUbicacionB = GetUltimoNombreUbicacionB() ?? "Ubicación B";
      //RETORNAR A LA VISTA CON EL ARRAY DE EMPLEADOS FILTRADO
      return View(selected_employee_page);
    }

    public List<Employee> GetEmployees()
    {
      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;
      List<Employee> employees = new List<Employee>();

      using (SqlConnection connection = new SqlConnection(connectionString))
      {
        string query = @"
          SELECT
    e.EmployeesIndex AS Id,
    e.name AS Name,
    e.lastName AS LastName,
    e.entryDate AS CreationDate,
    e.email AS Email,
	e.id as Cedula,
	e.employeeNo AS NumeroEmpleado,
    CASE WHEN e.active = 1 THEN 'Activo' ELSE 'Inactivo' END AS State
FROM employees e
ORDER BY e.name";

        SqlCommand command = new SqlCommand(query, connection);

        try
        {
          connection.Open();
          SqlDataReader reader = command.ExecuteReader();

          while (reader.Read())
          {
            Employee employee = new Employee
            {
              Id = reader.GetInt32(reader.GetOrdinal("Id")),
              Name = reader.GetString(reader.GetOrdinal("Name")),
              LastName = reader.GetString(reader.GetOrdinal("LastName")),
              CreationDate = reader.GetDateTime(reader.GetOrdinal("CreationDate")),
              Email = reader.GetString(reader.GetOrdinal("Email")),
              State = reader.GetString(reader.GetOrdinal("State")),
              Cedula = reader.GetString(reader.GetOrdinal("Cedula")),
              NumeroEmpleado = reader.GetInt32(reader.GetOrdinal("NumeroEmpleado"))
            };
            employees.Add(employee);
          }
          reader.Close();
        }
        catch (Exception ex)
        {
          Console.WriteLine("Error al obtener empleados: " + ex.Message);
        }
      }
      return employees;
    }








    [HttpPost]
    public IActionResult BusinessEmployeeRegister(string? txtAddName, string? txtAddLastname,
        string? txtAddCorreoEmpleado, string? txtAddIdEmpleado, string? txtAddIdentificacionE, string? add_employee_state)
    {
      // Validación de campos
      if (String.IsNullOrEmpty(txtAddName) || String.IsNullOrEmpty(txtAddCorreoEmpleado) || String.IsNullOrEmpty(txtAddIdEmpleado)
          || String.IsNullOrEmpty(txtAddLastname) || String.IsNullOrEmpty(txtAddIdentificacionE) || String.IsNullOrEmpty(add_employee_state))
      {
        Console.WriteLine("Error: Campos incompletos");
        return RedirectToAction("BusinessEmployeeList");
      }

      // Conversión de "activo"/"inactivo" a bool
      bool estadoEmpleado = add_employee_state.ToLower() == "activo";

      // Mostrar datos en consola
      Console.WriteLine("Nombre del Empleado: " + txtAddName);
      Console.WriteLine("Apellidos del Empleado: " + txtAddLastname);
      Console.WriteLine("Correo del Empleado: " + txtAddCorreoEmpleado);
      Console.WriteLine("Identificacion Corporativa del Empleado: " + txtAddIdEmpleado);
      Console.WriteLine("Cedula del Empleado: " + txtAddIdentificacionE);
      Console.WriteLine("Estado del Empleado: " + estadoEmpleado);

      // Llamada al método para insertar el empleado en la base de datos
      AddEmployee(txtAddName, txtAddLastname, txtAddCorreoEmpleado, txtAddIdEmpleado, txtAddIdentificacionE, estadoEmpleado);

      // Redirigir a la vista de empleados
      return RedirectToAction("BusinessEmployeeList");
    }


    public void AddEmployee(string name, string lastName, string email, string idEmpleado, string identificacionE, bool estado)
    {
      // Ajusta la cadena de conexión
      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

      using (SqlConnection connection = new SqlConnection(connectionString))
      {
        try
        {
          connection.Open();

          string query = @"
                INSERT INTO Employees 
                    (employeeSysId, name, lastName, email, id, active, companySysId, managementSysId, deptSysId, employeeNo, tagSysId, phone1, cell, hireDate, entryUser, entryDate, updateUser, updateDate, rowGuid)
                VALUES 
                    (@employeeSysId, @name, @lastName, @email, @id, @active, @companySysId, @managementSysId, @deptSysId, @employeeNo, @tagSysId, @phone1, @cell, @hireDate, @entryUser, @entryDate, @updateUser, @updateDate, @rowGuid)";

          using (SqlCommand command = new SqlCommand(query, connection))
          {
            // Genera un nuevo GUID para el empleado
            Guid employeeSysId = Guid.NewGuid();

            // Aquí debes completar los demás campos obligatorios. Ajusta según tu lógica.
            Guid companySysId = Guid.NewGuid();  // Ajusta este valor real
            Guid managementSysId = Guid.NewGuid();  // Ajusta este valor real
            Guid deptSysId = Guid.NewGuid();  // Ajusta este valor real
            int employeeNo = Convert.ToInt32(idEmpleado);  // Ajusta este valor real
            Guid tagSysId = Guid.NewGuid();  // Ajusta este valor real
            string phone1 = "00000000";  // Ajusta este valor real
            string cell = "00000000";  // Ajusta este valor real
            DateTime hireDate = DateTime.Now;  // Ajusta este valor real
            Guid entryUser = Guid.NewGuid();  // Ajusta este valor real
            DateTime entryDate = DateTime.Now;
            Guid updateUser = Guid.NewGuid();  // Ajusta este valor real
            DateTime updateDate = DateTime.Now;
            Guid rowGuid = Guid.NewGuid();

            command.Parameters.AddWithValue("@employeeSysId", employeeSysId);
            command.Parameters.AddWithValue("@name", name);
            command.Parameters.AddWithValue("@lastName", lastName);
            command.Parameters.AddWithValue("@email", email);
            command.Parameters.AddWithValue("@id", identificacionE);
            command.Parameters.AddWithValue("@active", estado);
            command.Parameters.AddWithValue("@companySysId", companySysId);
            command.Parameters.AddWithValue("@managementSysId", managementSysId);
            command.Parameters.AddWithValue("@deptSysId", deptSysId);
            command.Parameters.AddWithValue("@employeeNo", employeeNo);
            command.Parameters.AddWithValue("@tagSysId", tagSysId);
            command.Parameters.AddWithValue("@phone1", phone1);
            command.Parameters.AddWithValue("@cell", cell);
            command.Parameters.AddWithValue("@hireDate", hireDate);
            command.Parameters.AddWithValue("@entryUser", entryUser);
            command.Parameters.AddWithValue("@entryDate", entryDate);
            command.Parameters.AddWithValue("@updateUser", updateUser);
            command.Parameters.AddWithValue("@updateDate", updateDate);
            command.Parameters.AddWithValue("@rowGuid", rowGuid);

            int rowsAffected = command.ExecuteNonQuery();

            Console.WriteLine("Empleado insertado correctamente. Filas afectadas: " + rowsAffected);
          }
        }
        catch (Exception ex)
        {
          Console.WriteLine("Error al insertar empleado: " + ex.Message);
        }
      }
    }









    [HttpPost]
    public IActionResult BusinessEmployeeEdit(string? employeeSysId, string? txtEditName, string? txtEditLastname,
       string? txtEditCorreoEmpleado, string? txtEditIdEmpleado, string? txtEditIdentificacionE, string? edit_employee_state)
    {
      // Validación de campos
      if (String.IsNullOrEmpty(employeeSysId) || String.IsNullOrEmpty(txtEditName) ||
          String.IsNullOrEmpty(txtEditIdEmpleado) || String.IsNullOrEmpty(txtEditLastname) || String.IsNullOrEmpty(txtEditIdentificacionE) ||
          String.IsNullOrEmpty(edit_employee_state))
      {
        Console.WriteLine("Error: Campos incompletos");
        return RedirectToAction("BusinessEmployeeList");
      }

      // Conversión de "activo"/"inactivo" a bool
      bool estadoEmpleado = edit_employee_state.ToLower() == "activo";

      // Mostrar datos en consola
      Console.WriteLine("ID Empleado: " + employeeSysId);
      Console.WriteLine("Nombre: " + txtEditName);
      Console.WriteLine("Apellidos: " + txtEditLastname);
      Console.WriteLine("Correo: " + txtEditCorreoEmpleado);
      Console.WriteLine("ID Corporativo: " + txtEditIdEmpleado);
      Console.WriteLine("Identificación: " + txtEditIdentificacionE);
      Console.WriteLine("Estado: " + estadoEmpleado);

      // Llamada al método para actualizar
      UpdateEmployee(employeeSysId, txtEditName, txtEditLastname, txtEditCorreoEmpleado, txtEditIdEmpleado, txtEditIdentificacionE, estadoEmpleado);

      // Redirigir a la lista
      return RedirectToAction("BusinessEmployeeList");
    }




    public void UpdateEmployee(string employeeSysIdStr, string name, string lastName, string email, string idEmpleado, string identificacionE, bool estado)
    {
      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

      using (SqlConnection connection = new SqlConnection(connectionString))
      {
        try
        {
          connection.Open();

          string query = @"
                UPDATE Employees 
                SET name = @name,
                    lastName = @lastName,
                    email = @email,
                    id = @id,
                    active = @active,
                    updateUser = @updateUser,
                    updateDate = @updateDate,
                    employeeNo = @idEmpleado
                WHERE EmployeesIndex = @employeeSysId";

          using (SqlCommand command = new SqlCommand(query, connection))
          {
            // Convertir el id a Guid
            //Guid employeeSysId = Guid.Parse(employeeSysIdStr);

            // Otros campos de auditoría
            Guid updateUser = Guid.NewGuid();  // Ajusta este valor real
            DateTime updateDate = DateTime.Now;

            command.Parameters.AddWithValue("@employeeSysId", employeeSysIdStr);
            command.Parameters.AddWithValue("@name", name);
            command.Parameters.AddWithValue("@lastName", lastName);
            command.Parameters.AddWithValue("@email", email);
            command.Parameters.AddWithValue("@id", identificacionE);
            command.Parameters.AddWithValue("@active", estado);
            command.Parameters.AddWithValue("@updateUser", updateUser);
            command.Parameters.AddWithValue("@updateDate", updateDate);
            command.Parameters.AddWithValue("@idEmpleado", idEmpleado);

            int rowsAffected = command.ExecuteNonQuery();

            Console.WriteLine("Empleado actualizado correctamente. Filas afectadas: " + rowsAffected);
          }
        }
        catch (Exception ex)
        {
          Console.WriteLine("Error al actualizar empleado: " + ex.Message);
        }
      }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ToggleEmployeeState([FromBody] ToggleEmployeeRequest request)
    {
      // Cambiar el estado
      bool nuevoEstado = request.currentState == "Activo" ? false : true;

      // Llamar al método que hicimos
      UpdateEmployeeState(request.employeeId, nuevoEstado);

      return Ok();
    }

    public class ToggleEmployeeRequest
    {
      public string employeeId { get; set; }
      public string currentState { get; set; }
    }

    public void UpdateEmployeeState(string employeeSysIdStr, bool estado)
    {
      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

      using (SqlConnection connection = new SqlConnection(connectionString))
      {
        try
        {
          connection.Open();

          string query = @"
                UPDATE Employees 
                SET active = @active,
                    updateUser = @updateUser,
                    updateDate = @updateDate
                WHERE EmployeesIndex = @employeeSysId";

          using (SqlCommand command = new SqlCommand(query, connection))
          {
            Guid updateUser = Guid.NewGuid(); // O usa el usuario real logueado
            DateTime updateDate = DateTime.Now;

            command.Parameters.AddWithValue("@employeeSysId", employeeSysIdStr);
            command.Parameters.AddWithValue("@active", estado);
            command.Parameters.AddWithValue("@updateUser", updateUser);
            command.Parameters.AddWithValue("@updateDate", updateDate);

            int rowsAffected = command.ExecuteNonQuery();
            Console.WriteLine("Estado de empleado actualizado. Filas afectadas: " + rowsAffected);
          }
        }
        catch (Exception ex)
        {
          Console.WriteLine("Error al actualizar estado de empleado: " + ex.Message);
        }
      }
    }

    [HttpPost]
    public async Task<IActionResult> SincronizarEmpleados(IFormFile excelFile)
    {
      if (excelFile == null || excelFile.Length == 0)
      {
        TempData["Alert"] = "Por favor seleccione un archivo válido.";
        return RedirectToAction("BusinessEmployeeList");
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
          var sheet = package.Workbook.Worksheets.FirstOrDefault(s => s.Name.ToLower().Contains("empleados"));

          if (sheet == null)
          {
            TempData["Alert"] = "No se encontró la hoja de empleados en el archivo.";
            return RedirectToAction("BusinessEmployeeList");
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
              await SaveDataToDatabaseAsync(rowData);
          }

          TempData["Alert"] = "Archivo procesado exitosamente.";
        }
      }
      catch (Exception ex)
      {
        TempData["Alert"] = "Error al procesar el archivo: " + ex.Message;
      }

      return RedirectToAction("BusinessEmployeeList");
    }

    private async Task SaveDataToDatabaseAsync(List<string> rowData)
    {
      // Lee la cadena de conexión desde el archivo app.config, esta cadena se puede modificar.
      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

      using (var connection = new SqlConnection(connectionString))
      using (var command = new SqlCommand("InsertEmployeexExcel", connection))
      {
        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.AddWithValue("@nombre", rowData[0]);
        command.Parameters.AddWithValue("@apellido", rowData[1]);
        command.Parameters.AddWithValue("@id", Convert.ToInt32(rowData[2]));
        command.Parameters.AddWithValue("@email", rowData[3]);
        command.Parameters.AddWithValue("@activo", rowData[4]);
        command.Parameters.AddWithValue("@Usuario", Guid.Empty);
        var outputParam = new SqlParameter("@Resultado", SqlDbType.VarChar, 400)
        {
          Direction = ParameterDirection.Output
        };
        command.Parameters.Add(outputParam);

        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();

        string resultado = outputParam.Value?.ToString() ?? "Sin resultado.";
        TempData["Alert"] = resultado;
      }
    }

    public int ObtenerCantidadEmpleados()
    {
      int total = 0;
      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

      using (SqlConnection connection = new SqlConnection(connectionString))
      {
        string query = "SELECT COUNT(employeeSysId) FROM employees";

        using (SqlCommand command = new SqlCommand(query, connection))
        {
          connection.Open();
          total = (int)command.ExecuteScalar();
        }
      }

      return total;
    }



    #endregion Fin Empleado
    #region Edificios

    [HttpPost]
    public JsonResult GuardarNombreUbicacionA(string nombre)
    {
      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

      try
      {
        using (SqlConnection conn = new SqlConnection(connectionString))
        {
          conn.Open();
          string query = "INSERT INTO UBICACION_A_NOMBRE (NOMBRE_UBICACION_A) VALUES (@nombre)";
          SqlCommand cmd = new SqlCommand(query, conn);
          cmd.Parameters.AddWithValue("@nombre", nombre);
          cmd.ExecuteNonQuery();
        }

        return Json(new { success = true, nombre = nombre });
      }
      catch (Exception ex)
      {
        return Json(new { success = false, error = ex.Message });
      }
    }

    [HttpPost]
    public JsonResult GuardarNombreUbicacionB(string nombre)
    {
      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;
      nombre = (nombre ?? "").Trim();

      if (string.IsNullOrWhiteSpace(nombre))
      {
        AlertMessage alert = new AlertMessage
        {
          Tipo = "error",
          Mensaje = "El nombre no puede estar vacío."
        };
        TempData["Alert"] = JsonSerializer.Serialize(alert);
        return Json(new { success = true });
      }

      try
      {
        using (SqlConnection conn = new SqlConnection(connectionString))
        {
          conn.Open();

          // 1️⃣ Consultar el último nombre de la ubicación A
          string sqlUltimaA = @"
                SELECT TOP 1 NOMBRE_UBICACION_A
                FROM UBICACION_A_NOMBRE
                ORDER BY ID DESC"; // Cambia 'ID' por la PK real o columna de fecha

          string ultimoNombreA = null;
          using (SqlCommand cmd = new SqlCommand(sqlUltimaA, conn))
          {
            var result = cmd.ExecuteScalar();
            if (result != null && result != DBNull.Value)
              ultimoNombreA = result.ToString().Trim();
          }

          // 2️⃣ Validar si coincide
          if (!string.IsNullOrEmpty(ultimoNombreA) &&
              string.Equals(ultimoNombreA, nombre, StringComparison.OrdinalIgnoreCase))
          {
            AlertMessage alert = new AlertMessage
            {
              Tipo = "error",
              Mensaje = "El nombre no puede ser igual al de la ubicación predecesora."
            };
            TempData["Alert"] = JsonSerializer.Serialize(alert);
            return Json(new { success = true });
          }

          // 3️⃣ Insertar en Ubicación B
          string insertSql = "INSERT INTO UBICACION_B_NOMBRE (NOMBRE_UBICACION_B) VALUES (@nombre)";
          using (SqlCommand cmdInsert = new SqlCommand(insertSql, conn))
          {
            cmdInsert.Parameters.AddWithValue("@nombre", nombre);
            cmdInsert.ExecuteNonQuery();
          }
        }

        AlertMessage successAlert = new AlertMessage
        {
          Tipo = "success",
          Mensaje = $"Ubicación B '{nombre}' creada correctamente."
        };
        TempData["Alert"] = JsonSerializer.Serialize(successAlert);

        return Json(new { success = true, nombre });
      }
      catch (Exception ex)
      {
        AlertMessage alert = new AlertMessage
        {
          Tipo = "error",
          Mensaje = "Error: " + ex.Message
        };
        TempData["Alert"] = JsonSerializer.Serialize(alert);

        return Json(new { success = true });
      }
    }



    [HttpPost]
    public JsonResult GuardarNombreUbicacionC(string nombre)
    {
      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;
      nombre = (nombre ?? "").Trim();

      if (string.IsNullOrWhiteSpace(nombre))
      {
        AlertMessage alert = new AlertMessage
        {
          Tipo = "error",
          Mensaje = "El nombre no puede estar vacío."
        };
        TempData["Alert"] = JsonSerializer.Serialize(alert);
        return Json(new { success = true });
      }

      try
      {
        using (SqlConnection conn = new SqlConnection(connectionString))
        {
          conn.Open();

          // 1️⃣ Consultar el último nombre de la ubicación B
          string sqlUltimaB = @"
                SELECT TOP 1 NOMBRE_UBICACION_B
                FROM UBICACION_B_NOMBRE
                ORDER BY ID DESC"; // Cambia 'ID' por la PK real o columna de fecha

          string ultimoNombreB = null;
          using (SqlCommand cmd = new SqlCommand(sqlUltimaB, conn))
          {
            var result = cmd.ExecuteScalar();
            if (result != null && result != DBNull.Value)
              ultimoNombreB = result.ToString().Trim();
          }

          // 2️⃣ Validar si coincide
          if (!string.IsNullOrEmpty(ultimoNombreB) &&
              string.Equals(ultimoNombreB, nombre, StringComparison.OrdinalIgnoreCase))
          {
            AlertMessage alert = new AlertMessage
            {
              Tipo = "error",
              Mensaje = "El nombre no puede ser igual al de la ubicación predecesora."
            };
            TempData["Alert"] = JsonSerializer.Serialize(alert);
            return Json(new { success = true });
          }

          // 3️⃣ Insertar en Ubicación C
          string insertSql = "INSERT INTO UBICACION_C_NOMBRE (NOMBRE_UBICACION_C) VALUES (@nombre)";
          using (SqlCommand cmdInsert = new SqlCommand(insertSql, conn))
          {
            cmdInsert.Parameters.AddWithValue("@nombre", nombre);
            cmdInsert.ExecuteNonQuery();
          }
        }

        AlertMessage successAlert = new AlertMessage
        {
          Tipo = "success",
          Mensaje = $"Ubicación C '{nombre}' creada correctamente."
        };
        TempData["Alert"] = JsonSerializer.Serialize(successAlert);

        return Json(new { success = true, nombre });
      }
      catch (Exception ex)
      {
        AlertMessage alert = new AlertMessage
        {
          Tipo = "error",
          Mensaje = "Error: " + ex.Message
        };
        TempData["Alert"] = JsonSerializer.Serialize(alert);

        return Json(new { success = true });
      }
    }




    [HttpGet]
    public IActionResult Edificios(string search, string hasAssets, string searchType = "Name", int page = 1, int pageSize = 20, string sortColumn = "name", string sortDirection = "asc")
    {
      IEnumerable<Company> companies;

      if (searchType == "description")
      {
        companies = AccessCompanies.GetCompanyByDescription(search ?? "");
      }
      else
      {
        companies = AccessCompanies.GetCompanyByName(search ?? "");
      }

      List<Edificios> edificiosList = new List<Edificios>();

      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;
      Dictionary<Guid, int> activosCount = new Dictionary<Guid, int>();

      using (SqlConnection conn = new SqlConnection(connectionString))
      {
        conn.Open();
        SqlCommand cmd = new SqlCommand("SELECT companySysId, COUNT(*) AS Activos FROM assets GROUP BY companySysId", conn);
        SqlDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
          activosCount[reader.GetGuid(0)] = reader.GetInt32(1);
        }
      }

      foreach (var company in companies)
      {
        edificiosList.Add(new Edificios
        {
          companySysId = company.CompanySysId,
          name = company.Name,
          description = company.Description,
          entryUser = company.EntryUser,
          entryDate = company.EntryDate,
          updateUser = company.UpdateUser,
          rowGuid = company.RowGuid,
          Activos = activosCount.ContainsKey(company.CompanySysId) ? activosCount[company.CompanySysId] : 0
        });
      }

      // Aplicar filtro
      if (hasAssets == "withAssets")
      {
        edificiosList = edificiosList.Where(e => e.Activos > 0).ToList();
      }
      else if (hasAssets == "withoutAssets")
      {
        edificiosList = edificiosList.Where(e => e.Activos == 0).ToList();
      }

      // Ordenar dinámicamente según la columna y la dirección
      edificiosList = sortColumn switch
      {
        "description" => sortDirection == "asc"
            ? edificiosList.OrderBy(e => e.description).ToList()
            : edificiosList.OrderByDescending(e => e.description).ToList(),
        "Activos" => sortDirection == "asc"
            ? edificiosList.OrderBy(e => e.Activos).ToList()
            : edificiosList.OrderByDescending(e => e.Activos).ToList(),
        _ => sortDirection == "asc"
            ? edificiosList.OrderBy(e => e.name).ToList()
            : edificiosList.OrderByDescending(e => e.name).ToList(),
      };



      // Calcular la paginación
      var totalItems = edificiosList.Count;
      var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
      var itemsOnPage = edificiosList.Skip((page - 1) * pageSize).Take(pageSize).ToList();

      // Crear el modelo de paginación
      var model = new EdificiosViewModel
      {
        Edificios = itemsOnPage,
        CurrentPage = page,
        TotalPages = totalPages,
        search = search
      };

      // Leer el último nombre personalizado de la tabla
      using (SqlConnection conn = new SqlConnection(connectionString))
      {
        conn.Open();
        SqlCommand cmd = new SqlCommand("SELECT TOP 1 NOMBRE_UBICACION_A FROM UBICACION_A_NOMBRE ORDER BY ID DESC", conn);
        var result = cmd.ExecuteScalar();
        ViewBag.NombreUbicacionA = result != null ? result.ToString() : "Ubicación A";
      }


      // Pasar el término de búsqueda, columna y dirección actuales a la vista
      ViewBag.SearchQuery = search;
      ViewBag.SortColumn = sortColumn;
      ViewBag.SortDirection = sortDirection;
      ViewBag.Filter = hasAssets; // Pasar el filtro actual a la vista
      ViewBag.SearchType = searchType;
      ViewBag.TotalEdificios = ObtenerCantidadEdificios();
      ViewData["Categories_Per_Page"] = pageSize;
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

    public ActionResult EliminarIndividual(Guid id)
    {
      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;
      AlertMessage alert = new AlertMessage();

      if (id == Guid.Empty)
      {
        alert.Tipo = "error";
        alert.Mensaje = "El edificio: 'Sin Asignar' no se puede eliminar.";
      }
      else
      {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
          connection.Open();

          // Verificaciones
          if (ExisteEnTabla("floors", id, connection))
          {
            alert.Tipo = "error";
            alert.Mensaje = "El edificio no se puede eliminar porque está asociado a pisos.";
          }
          else if (ExisteEnTabla("departments", id, connection))
          {
            alert.Tipo = "error";
            alert.Mensaje = "El edificio no se puede eliminar porque está asociado a sectores.";
          }
          else if (ExisteEnTabla("assets", id, connection))
          {
            alert.Tipo = "error";
            alert.Mensaje = "El edificio no se puede eliminar porque está asociado a activos.";
          }
          else if (ExisteEnTabla("buildings", id, connection))
          {
            alert.Tipo = "error";
            alert.Mensaje = "El edificio no se puede eliminar porque está asociado a pisos.";
          }
          else if (ExisteEnTabla("officeses", id, connection))
          {
            alert.Tipo = "error";
            alert.Mensaje = "El edificio no se puede eliminar porque está asociado a oficinas.";
          }
          else
          {
            SqlCommand deleteCommand = new SqlCommand("DELETE FROM companies WHERE companySysId = @id", connection);
            deleteCommand.Parameters.AddWithValue("@id", id);
            deleteCommand.ExecuteNonQuery();

            alert.Tipo = "success";
            alert.Mensaje = "El edificio ha sido eliminado correctamente.";
          }
        }
      }
      TempData["Alert"] = JsonSerializer.Serialize(alert);
      return RedirectToAction("Edificios");
    }

    private bool ExisteEnTabla(string tabla, Guid id, SqlConnection connection)
    {
      SqlCommand command = new SqlCommand($"SELECT COUNT(*) FROM {tabla} WHERE companySysId = @id", connection);
      command.Parameters.AddWithValue("@id", id);
      int count = (int)command.ExecuteScalar();
      return count > 0;
    }

    public ActionResult EliminarBatch(string ids)
    {
      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;
      AlertMessage alert = new AlertMessage();

      // Convertir la lista de ids desde el string recibido
      var idsList = ids.Split(',').Select(id => Guid.Parse(id)).ToList();

      // Verificar si alguno de los IDs es vacío
      if (idsList.Any(id => id == Guid.Empty))
      {
        alert.Tipo = "error";
        alert.Mensaje = "El edificio: 'Sin Asignar' no se puede eliminar.";
        TempData["Alert"] = JsonSerializer.Serialize(alert);
        return RedirectToAction("Edificios");
      }

      using (SqlConnection connection = new SqlConnection(connectionString))
      {
        connection.Open();

        foreach (var id in idsList)
        {
          // Obtener el nombre del edificio desde la tabla 'companies' usando el id
          string nombreEdificio = ObtenerNombreEdificio(id, connection);
          if (string.IsNullOrEmpty(nombreEdificio))
          {
            alert.Tipo = "error";
            alert.Mensaje = $"El edificio con id {id} no existe.";
            TempData["Alert"] = JsonSerializer.Serialize(alert);
            return RedirectToAction("Edificios");
          }

          // Verificaciones por cada id
          if (ExisteEnTabla("floors", id, connection))
          {
            alert.Tipo = "error";
            alert.Mensaje = $"El edificio <strong>{nombreEdificio}</strong> no se puede eliminar porque está asociado a pisos.";
            TempData["Alert"] = JsonSerializer.Serialize(alert);
            return RedirectToAction("Edificios");
          }
          else if (ExisteEnTabla("departments", id, connection))
          {
            alert.Tipo = "error";
            alert.Mensaje = $"El edificio <strong>{nombreEdificio}</strong> no se puede eliminar porque está asociado a sectores.";
            TempData["Alert"] = JsonSerializer.Serialize(alert);
            return RedirectToAction("Edificios");
          }
          else if (ExisteEnTabla("assets", id, connection))
          {
            alert.Tipo = "error";
            alert.Mensaje = $"El edificio <strong>{nombreEdificio}</strong> no se puede eliminar porque está asociado a activos.";
            TempData["Alert"] = JsonSerializer.Serialize(alert);
            return RedirectToAction("Edificios");
          }
          else if (ExisteEnTabla("buildings", id, connection))
          {
            alert.Tipo = "error";
            alert.Mensaje = $"El edificio <strong>{nombreEdificio}</strong> no se puede eliminar porque está asociado a pisos.";
            TempData["Alert"] = JsonSerializer.Serialize(alert);
            return RedirectToAction("Edificios");
          }
          else if (ExisteEnTabla("officeses", id, connection))
          {
            alert.Tipo = "error";
            alert.Mensaje = $"El edificio <strong>{nombreEdificio}</strong> no se puede eliminar porque está asociado a oficinas.";
            TempData["Alert"] = JsonSerializer.Serialize(alert);
            return RedirectToAction("Edificios");
          }
          else
          {
            // Eliminar el registro de la tabla 'companies' para cada id
            SqlCommand deleteCommand = new SqlCommand("DELETE FROM companies WHERE companySysId = @id", connection);
            deleteCommand.Parameters.AddWithValue("@id", id);
            deleteCommand.ExecuteNonQuery();
          }
        }

        alert.Tipo = "success";
        alert.Mensaje = "Los edificios seleccionados han sido eliminados correctamente.";
      }

      TempData["Alert"] = JsonSerializer.Serialize(alert);
      return RedirectToAction("Edificios");
    }

    // Función para obtener el nombre del edificio a partir del id
    private string ObtenerNombreEdificio(Guid id, SqlConnection connection)
    {
      SqlCommand command = new SqlCommand("SELECT name FROM companies WHERE companySysId = @id", connection);
      command.Parameters.AddWithValue("@id", id);
      var result = command.ExecuteScalar();
      return result != null ? result.ToString() : null;

    }

    [HttpPost]
    public IActionResult InsertarEdificio(Edificios model)
    {
      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;
      AlertMessage alertMessage;

      using (SqlConnection connection = new SqlConnection(connectionString))
      {
        // Verificar si ya existe un edificio con el mismo nombre y descripción
        string checkQuery = @"SELECT COUNT(*) FROM companies WHERE name = @name";

        using (SqlCommand checkCmd = new SqlCommand(checkQuery, connection))
        {
          checkCmd.Parameters.Add("@name", SqlDbType.VarChar, 50).Value = model.name;
          checkCmd.Parameters.Add("@description", SqlDbType.VarChar, 150).Value = model.description;

          connection.Open();
          int count = (int)checkCmd.ExecuteScalar(); // Devuelve la cantidad de registros que coinciden
          connection.Close();

          if (count > 0)
          {
            // Si ya existe un registro con el mismo name y description, no permitir la inserción
            alertMessage = new AlertMessage
            {
              Tipo = "error",
              Mensaje = "Ya existe un edificio con el mismo nombre en el sistema."
            };

            TempData["Alert"] = JsonSerializer.Serialize(alertMessage);
            return RedirectToAction("Edificios");
          }
        }

        // Si el name ya existe pero con una descripción diferente, se puede insertar
        string insertQuery = @"INSERT INTO companies 
                                (companySysId, name, description, entryUser, entryDate, updateUser, updateDate, rowGuid) 
                                VALUES 
                                (@companySysId, @name, @description, @entryUser, @entryDate, @updateUser, @updateDate, @rowGuid)";

        using (SqlCommand cmd = new SqlCommand(insertQuery, connection))
        {
          cmd.Parameters.Add("@companySysId", SqlDbType.UniqueIdentifier).Value = Guid.NewGuid();
          cmd.Parameters.Add("@name", SqlDbType.VarChar, 50).Value = model.name;
          cmd.Parameters.Add("@description", SqlDbType.VarChar, 150).Value = model.description;
          cmd.Parameters.Add("@entryUser", SqlDbType.UniqueIdentifier).Value = Guid.NewGuid();
          cmd.Parameters.Add("@entryDate", SqlDbType.DateTime).Value = DateTime.Now;
          cmd.Parameters.Add("@updateUser", SqlDbType.UniqueIdentifier).Value = Guid.NewGuid();
          cmd.Parameters.Add("@updateDate", SqlDbType.DateTime).Value = DateTime.Now;
          cmd.Parameters.Add("@rowGuid", SqlDbType.UniqueIdentifier).Value = Guid.NewGuid();

          try
          {
            connection.Open();
            cmd.ExecuteNonQuery();

            alertMessage = new AlertMessage
            {
              Tipo = "success",
              Mensaje = "El edificio se ha insertado correctamente."
            };
          }
          catch (Exception ex)
          {
            alertMessage = new AlertMessage
            {
              Tipo = "error",
              Mensaje = "Error al insertar el edificio: " + ex.Message
            };
          }
        }
      }

      TempData["Alert"] = JsonSerializer.Serialize(alertMessage);
      return RedirectToAction("Edificios");
    }

    [HttpPost]
    public IActionResult EditarEdificio(Edificios model)
    {
      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;
      AlertMessage alertMessage;

      // Verificar si el companySysId es el restringido
      if (model.companySysId == Guid.Empty)
      {
        alertMessage = new AlertMessage
        {
          Tipo = "error",
          Mensaje = "Este edificio no se puede editar."
        };
        TempData["Alert"] = JsonSerializer.Serialize(alertMessage);
        return RedirectToAction("Edificios");
      }

      using (SqlConnection connection = new SqlConnection(connectionString))
      {
        // Verificar si ya existe otro edificio con el mismo nombre y descripción (excluyendo el actual)
        string checkQuery = @"SELECT COUNT(*) FROM companies 
                              WHERE name = @name
                              AND companySysId != @companySysId";

        using (SqlCommand checkCmd = new SqlCommand(checkQuery, connection))
        {
          checkCmd.Parameters.Add("@name", SqlDbType.VarChar, 50).Value = model.name;
          checkCmd.Parameters.Add("@companySysId", SqlDbType.UniqueIdentifier).Value = model.companySysId;

          connection.Open();
          int count = (int)checkCmd.ExecuteScalar();
          connection.Close();

          if (count > 0)
          {
            alertMessage = new AlertMessage
            {
              Tipo = "error",
              Mensaje = "Ya existe otro edificio con el mismo nombre en el sistema."
            };
            TempData["Alert"] = JsonSerializer.Serialize(alertMessage);
            return RedirectToAction("Edificios");
          }
        }

        // Proceder con la actualización
        string updateQuery = @"UPDATE companies 
                               SET name = @name, 
                                   description = @description, 
                                   updateUser = @updateUser, 
                                   updateDate = @updateDate 
                               WHERE companySysId = @companySysId";

        using (SqlCommand cmd = new SqlCommand(updateQuery, connection))
        {
          cmd.Parameters.Add("@name", SqlDbType.VarChar, 50).Value = model.name;
          cmd.Parameters.Add("@description", SqlDbType.VarChar, 150).Value = model.description;
          cmd.Parameters.Add("@updateUser", SqlDbType.UniqueIdentifier).Value = Guid.NewGuid();
          cmd.Parameters.Add("@updateDate", SqlDbType.DateTime).Value = DateTime.Now;
          cmd.Parameters.Add("@companySysId", SqlDbType.UniqueIdentifier).Value = model.companySysId;

          try
          {
            connection.Open();
            int rowsAffected = cmd.ExecuteNonQuery();
            connection.Close();

            if (rowsAffected > 0)
            {
              alertMessage = new AlertMessage
              {
                Tipo = "success",
                Mensaje = "El edificio se ha actualizado correctamente."
              };
            }
            else
            {
              alertMessage = new AlertMessage
              {
                Tipo = "warning",
                Mensaje = "No se encontró el edificio para actualizar."
              };
            }
          }
          catch (Exception ex)
          {
            alertMessage = new AlertMessage
            {
              Tipo = "error",
              Mensaje = "Error al actualizar el edificio: " + ex.Message
            };
          }
        }
      }

      TempData["Alert"] = JsonSerializer.Serialize(alertMessage);
      return RedirectToAction("Edificios");
    }


    [HttpPost]
    public async Task<IActionResult> SincronizarEdificios(IFormFile excelFile)
    {
      if (excelFile == null || excelFile.Length == 0)
      {
        TempData["Alert2"] = "Por favor seleccione un archivo válido.";
        return RedirectToAction("Edificios");
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
          var sheet = package.Workbook.Worksheets.FirstOrDefault(s => s.Name.ToLower().Contains("edificio"));

          if (sheet == null)
          {
            TempData["Alert2"] = "No se encontró la hoja de edificios en el archivo.";
            return RedirectToAction("Edificios");
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
              await SaveDataToDatabaseAsyncEdificios(rowData);
          }

          TempData["Alert2"] = "Archivo procesado exitosamente.";
        }
      }
      catch (Exception ex)
      {
        TempData["Alert2"] = "Error al procesar el archivo: " + ex.Message;
      }

      return RedirectToAction("Edificios");
    }

    private async Task SaveDataToDatabaseAsyncEdificios(List<string> rowData)
    {
      // Lee la cadena de conexión desde el archivo app.config, esta cadena se puede modificar.
      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

      using (var connection = new SqlConnection(connectionString))
      using (var command = new SqlCommand("InsertCompanyExcel", connection))
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

    public int ObtenerCantidadEdificios()
    {
      int total = 0;
      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

      using (SqlConnection connection = new SqlConnection(connectionString))
      {
        string query = "SELECT COUNT(companySysId) FROM companies";

        using (SqlCommand command = new SqlCommand(query, connection))
        {
          connection.Open();
          total = (int)command.ExecuteScalar();
        }
      }

      return total;
    }

    public IActionResult DescargarPlantillaEdificios()
    {
      var rutaArchivo = Path.Combine(Directory.GetCurrentDirectory(), "Plantillas", "PlantillaEdificios.xlsx");

      if (!System.IO.File.Exists(rutaArchivo))
      {
        return NotFound("La plantilla no fue encontrada.");
      }

      var contenido = System.IO.File.ReadAllBytes(rutaArchivo);
      var nombreArchivo = "PlantillaEdificios.xlsx";
      return File(contenido, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", nombreArchivo);
    }
    public IActionResult DescargarPlantillaPisos()
    {
      var rutaArchivo = Path.Combine(Directory.GetCurrentDirectory(), "Plantillas", "PlantillaPisos.xlsx");

      if (!System.IO.File.Exists(rutaArchivo))
      {
        return NotFound("La plantilla no fue encontrada.");
      }

      var contenido = System.IO.File.ReadAllBytes(rutaArchivo);
      var nombreArchivo = "PlantillaPisos.xlsx";
      return File(contenido, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", nombreArchivo);
    }
    public IActionResult DescargarPlantillaSectores()
    {
      var rutaArchivo = Path.Combine(Directory.GetCurrentDirectory(), "Plantillas", "PlantillaSectores.xlsx");

      if (!System.IO.File.Exists(rutaArchivo))
      {
        return NotFound("La plantilla no fue encontrada.");
      }

      var contenido = System.IO.File.ReadAllBytes(rutaArchivo);
      var nombreArchivo = "PlantillaSectores.xlsx";
      return File(contenido, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", nombreArchivo);
    }
    public IActionResult DescargarPlantillaOficinas()
    {
      var rutaArchivo = Path.Combine(Directory.GetCurrentDirectory(), "Plantillas", "PlantillaOficinas.xlsx");

      if (!System.IO.File.Exists(rutaArchivo))
      {
        return NotFound("La plantilla no fue encontrada.");
      }

      var contenido = System.IO.File.ReadAllBytes(rutaArchivo);
      var nombreArchivo = "PlantillaOficinas.xlsx";
      return File(contenido, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", nombreArchivo);
    }
    public IActionResult DescargarPlantillaEmpleados()
    {
      var rutaArchivo = Path.Combine(Directory.GetCurrentDirectory(), "Plantillas", "PlantillaEmpleados.xlsx");

      if (!System.IO.File.Exists(rutaArchivo))
      {
        return NotFound("La plantilla no fue encontrada.");
      }

      var contenido = System.IO.File.ReadAllBytes(rutaArchivo);
      var nombreArchivo = "PlantillaEmpleados.xlsx";
      return File(contenido, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", nombreArchivo);
    }


    #endregion Fin Edificios

    #region Pisos
    [HttpGet]
    public IActionResult Pisos(string search, string hasAssets, string searchType, int page = 1, int pageSize = 20, string sortColumn = "name", string sortDirection = "asc")
    {
      IEnumerable<Building> buildings;

      if (searchType == "description")
      {
        buildings = AccessBuildings.GetBuildingsByDescription(search ?? "");
      }
      else
      {
        buildings = AccessBuildings.GetBuildingsByName(search ?? "");
      }

      List<Pisos> pisosList = new List<Pisos>();

      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;
      Dictionary<Guid, int> activosCount = new Dictionary<Guid, int>();
      Dictionary<Guid, string> companyNames = new Dictionary<Guid, string>();

      using (SqlConnection conn = new SqlConnection(connectionString))
      {
        conn.Open();

        // Obtener el número de activos por edificio
        using (SqlCommand cmd = new SqlCommand("SELECT buildingSysId, COUNT(*) AS Activos FROM assets GROUP BY buildingSysId", conn))
        {
          SqlDataReader reader = cmd.ExecuteReader();
          while (reader.Read())
          {
            activosCount[reader.GetGuid(0)] = reader.GetInt32(1);
          }
          reader.Close();
        }

        // Obtener los nombres de los edificios desde la tabla companies
        using (SqlCommand cmd = new SqlCommand("SELECT companySysId, name FROM companies", conn))
        {
          SqlDataReader reader = cmd.ExecuteReader();
          while (reader.Read())
          {
            companyNames[reader.GetGuid(0)] = reader.GetString(1);
          }
          reader.Close();
        }


      }
      // Leer el último nombre personalizado de la tabla
      using (SqlConnection conn = new SqlConnection(connectionString))
      {
        conn.Open();
        SqlCommand cmd = new SqlCommand("SELECT TOP 1 NOMBRE_UBICACION_B FROM UBICACION_B_NOMBRE ORDER BY ID DESC", conn);
        var result = cmd.ExecuteScalar();
        ViewBag.NombreUbicacionB = result != null ? result.ToString() : "Ubicación B";
      }
      foreach (var building in buildings)
      {
        pisosList.Add(new Pisos
        {
          buildingSysId = building.BuildingSysId,
          name = building.Name,
          description = building.Description,
          entryUser = building.EntryUser,
          entryDate = building.EntryDate,
          updateUser = building.UpdateUser,
          rowGuid = building.RowGuid,
          Activos = activosCount.ContainsKey(building.BuildingSysId) ? activosCount[building.BuildingSysId] : 0,
          Edificio = companyNames.ContainsKey(building.CompanySysId) ? companyNames[building.CompanySysId] : "Desconocido"
        });
      }

      // Aplicar filtro de activos
      if (hasAssets == "withAssets")
      {
        pisosList = pisosList.Where(e => e.Activos > 0).ToList();
      }
      else if (hasAssets == "withoutAssets")
      {
        pisosList = pisosList.Where(e => e.Activos == 0).ToList();
      }

      // Ordenar dinámicamente según la columna y dirección
      pisosList = sortColumn switch
      {
        "description" => sortDirection == "asc"
            ? pisosList.OrderBy(e => e.description).ToList()
            : pisosList.OrderByDescending(e => e.description).ToList(),
        "Activos" => sortDirection == "asc"
            ? pisosList.OrderBy(e => e.Activos).ToList()
            : pisosList.OrderByDescending(e => e.Activos).ToList(),
        "Edificio" => sortDirection == "asc"
            ? pisosList.OrderBy(e => e.Edificio).ToList()
            : pisosList.OrderByDescending(e => e.Edificio).ToList(),
        _ => sortDirection == "asc"
            ? pisosList.OrderBy(e => e.name).ToList()
            : pisosList.OrderByDescending(e => e.name).ToList(),
      };

      // Calcular la paginación
      var totalItems = pisosList.Count;
      var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
      var itemsOnPage = pisosList.Skip((page - 1) * pageSize).Take(pageSize).ToList();

      var model = new PisosViewModel
      {
        Pisos = itemsOnPage,
        CurrentPage = page,
        TotalPages = totalPages,
        search = search
      };

      ViewBag.SearchQuery = search;
      ViewBag.SortColumn = sortColumn;
      ViewBag.SortDirection = sortDirection;
      ViewBag.Filter = hasAssets;
      ViewBag.SearchType = searchType;
      ViewBag.TotalPisos = ObtenerCantidadPisos();
      ViewData["Categories_Per_Page"] = pageSize;
      ViewBag.NombreUbicacion = GetUltimoNombreUbicacionC() ?? "Ubicación C";
      ViewBag.NombreUbicacionA = GetUltimoNombreUbicacionA() ?? "Ubicación A";
      ViewBag.NombreUbicacionB = GetUltimoNombreUbicacionB() ?? "Ubicación B";
      return View(model);
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

    public ActionResult EliminarIndividualPisos(Guid id)
    {
      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;
      AlertMessage alert = new AlertMessage();
      if (id == Guid.Empty)
      {
        alert.Tipo = "error";
        alert.Mensaje = "El piso: 'Sin Asignar' no se puede eliminar.";
      }
      else
      {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
          connection.Open();

          // Verificaciones
          if (ExisteEnTablaPisos("floors", id, connection))
          {
            alert.Tipo = "error";
            alert.Mensaje = "El piso no se puede eliminar porque está asociado a sectores.";
          }
          else if (ExisteEnTablaPisos("officeses", id, connection))
          {
            alert.Tipo = "error";
            alert.Mensaje = "El piso no se puede eliminar porque está asociado a oficinas.";
          }
          else if (ExisteEnTablaPisos("assets", id, connection))
          {
            alert.Tipo = "error";
            alert.Mensaje = "El piso no se puede eliminar porque está asociado a activos.";
          }
          else
          {
            SqlCommand deleteCommand = new SqlCommand("DELETE FROM buildings WHERE buildingSysId = @id", connection);
            deleteCommand.Parameters.AddWithValue("@id", id);
            deleteCommand.ExecuteNonQuery();

            alert.Tipo = "success";
            alert.Mensaje = "El piso ha sido eliminado correctamente.";
          }
        }
      }

      TempData["Alert"] = JsonSerializer.Serialize(alert);
      return RedirectToAction("Pisos");
    }

    private bool ExisteEnTablaPisos(string tabla, Guid id, SqlConnection connection)
    {
      SqlCommand command = new SqlCommand($"SELECT COUNT(*) FROM {tabla} WHERE buildingSysId = @id", connection);
      command.Parameters.AddWithValue("@id", id);
      int count = (int)command.ExecuteScalar();
      return count > 0;
    }


    public ActionResult EliminarBatchPisos(string ids)
    {
      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;
      AlertMessage alert = new AlertMessage();

      // Convertir la lista de ids desde el string recibido
      var idsList = ids.Split(',').Select(id => Guid.Parse(id)).ToList();

      // Verificar si alguno de los IDs es vacío
      if (idsList.Any(id => id == Guid.Empty))
      {
        alert.Tipo = "error";
        alert.Mensaje = "El piso: 'Sin Asignar' no se puede eliminar.";

        TempData["Alert"] = JsonSerializer.Serialize(alert);
        return RedirectToAction("Pisos");
      }

      using (SqlConnection connection = new SqlConnection(connectionString))
      {
        connection.Open();

        foreach (var id in idsList)
        {
          // Obtener el nombre del edificio desde la tabla 'companies' usando el id
          string nombrePiso = ObtenerNombrePiso(id, connection);
          if (string.IsNullOrEmpty(nombrePiso))
          {
            alert.Tipo = "error";
            alert.Mensaje = $"El piso con id {id} no existe.";

            TempData["Alert"] = JsonSerializer.Serialize(alert);
            return RedirectToAction("Pisos");
          }
          // Verificaciones por cada id
          if (ExisteEnTablaPisos("floors", id, connection))
          {
            alert.Tipo = "error";
            alert.Mensaje = $"El piso <strong>{nombrePiso}</strong> no se puede eliminar porque está asociado a sectores.";

            TempData["Alert"] = JsonSerializer.Serialize(alert);
            return RedirectToAction("Pisos");
          }
          else if (ExisteEnTablaPisos("officeses", id, connection))
          {
            alert.Tipo = "error";
            alert.Mensaje = $"El piso <strong>{nombrePiso}</strong> no se puede eliminar porque está asociado a oficinas.";

            TempData["Alert"] = JsonSerializer.Serialize(alert);
            return RedirectToAction("Pisos");
          }
          else if (ExisteEnTablaPisos("assets", id, connection))
          {
            alert.Tipo = "error";
            alert.Mensaje = $"El piso <strong>{nombrePiso}</strong> no se puede eliminar porque está asociado a activos.";

            TempData["Alert"] = JsonSerializer.Serialize(alert);
            return RedirectToAction("Pisos");
          }
          else
          {
            // Eliminar el registro de la tabla 'buildings' para cada id
            SqlCommand deleteCommand = new SqlCommand("DELETE FROM buildings WHERE buildingSysId = @id", connection);
            deleteCommand.Parameters.AddWithValue("@id", id);
            deleteCommand.ExecuteNonQuery();
          }
        }

        alert.Tipo = "success";
        alert.Mensaje = "Los Pisos seleccionados han sido eliminados correctamente.";
      }

      var alertJson = System.Text.Json.JsonSerializer.Serialize(alert);

      TempData["Alert"] = JsonSerializer.Serialize(alert);
      return RedirectToAction("Pisos");
    }

    // Función para obtener el nombre del piso a partir del id
    private string ObtenerNombrePiso(Guid id, SqlConnection connection)
    {
      SqlCommand command = new SqlCommand("SELECT name FROM buildings WHERE buildingSysId = @id", connection);
      command.Parameters.AddWithValue("@id", id);
      var result = command.ExecuteScalar();
      return result != null ? result.ToString() : null;

    }

    [HttpGet]
    public JsonResult GetCompanies()
    {
      List<Edificios> companies = new List<Edificios>();

      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

      string query = "SELECT companySysId, name, description FROM companies";

      using (SqlConnection connection = new SqlConnection(connectionString))
      {
        SqlCommand command = new SqlCommand(query, connection);
        connection.Open();

        using (SqlDataReader reader = command.ExecuteReader())
        {
          while (reader.Read())
          {
            companies.Add(new Edificios
            {
              companySysId = reader.GetGuid(0),
              name = reader.GetString(1),
              description = reader.GetString(2)
            });
          }
        }
      }

      if (companies.Count == 0)
      {
        return Json(new { message = "No hay empresas disponibles." });
      }

      return Json(companies);
    }

    [HttpGet]
    public JsonResult GetPisos()
    {
      List<Pisos> pisos = new List<Pisos>();

      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

      string query = "SELECT buildingSysId, name, description FROM buildings";

      using (SqlConnection connection = new SqlConnection(connectionString))
      {
        SqlCommand command = new SqlCommand(query, connection);
        connection.Open();

        using (SqlDataReader reader = command.ExecuteReader())
        {
          while (reader.Read())
          {
            pisos.Add(new Pisos
            {
              buildingSysId = reader.GetGuid(0),
              name = reader.GetString(1),
              description = reader.GetString(2)
            });
          }
        }
      }

      if (pisos.Count == 0)
      {
        return Json(new { message = "No hay pisos disponibles." });
      }

      return Json(pisos);
    }

    [HttpGet]
    public JsonResult GetSectores()
    {
      List<Sector> sector = new List<Sector>();

      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

      string query = "SELECT floorSysId, name, description FROM floors";

      using (SqlConnection connection = new SqlConnection(connectionString))
      {
        SqlCommand command = new SqlCommand(query, connection);
        connection.Open();

        using (SqlDataReader reader = command.ExecuteReader())
        {
          while (reader.Read())
          {
            sector.Add(new Sector
            {
              floorSysId = reader.GetGuid(0),
              name = reader.GetString(1),
              description = reader.GetString(2)
            });
          }
        }
      }

      if (sector.Count == 0)
      {
        return Json(new { message = "No hay pisos disponibles." });
      }

      return Json(sector);
    }

    public IActionResult InsertarPiso(Guid companySysId, string name, string description)
    {
      if (string.IsNullOrWhiteSpace(description))
      {
        AlertMessage errorMessage = new AlertMessage
        {
          Tipo = "error",
          Mensaje = "El campo descripción es obligatorio."
        };

        TempData["Alert"] = JsonSerializer.Serialize(errorMessage);
        return RedirectToAction("Pisos");
      }

      string checkQueryName = @"
        SELECT COUNT(1)
        FROM [buildings]
        WHERE [companySysId] = @companySysId AND [name] = @name;";

      string checkQueryNameDescription = @"
        SELECT COUNT(1)
        FROM [buildings]
        WHERE [name] = @name AND [description] = @description;";

      string insertQuery = @"
        INSERT INTO [buildings]
           ([buildingSysId]
           ,[companySysId]
           ,[name]
           ,[description]
           ,[entryUser]
           ,[entryDate]
           ,[updateUser]
           ,[updateDate]
           ,[rowGuid])
        VALUES
           (NEWID()
           ,@companySysId
           ,@name
           ,@description
           ,@entryUser
           ,GETDATE()
           ,@entryUser
           ,GETDATE()
           ,NEWID());";

      try
      {
        string _connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
          connection.Open();

          // Verificar si ya existe el nombre en el mismo edificio
          using (SqlCommand checkCommand = new SqlCommand(checkQueryName, connection))
          {
            checkCommand.Parameters.AddWithValue("@companySysId", companySysId);
            checkCommand.Parameters.AddWithValue("@name", name);
            int countName = (int)checkCommand.ExecuteScalar();

            if (countName > 0)
            {
              AlertMessage duplicateMessage = new AlertMessage
              {
                Tipo = "error",
                Mensaje = "Ya existe un piso con el mismo nombre en este edificio."
              };

              TempData["Alert"] = JsonSerializer.Serialize(duplicateMessage);
              return RedirectToAction("Pisos");
            }
          }

          // Verificar si ya existe la misma combinación de nombre y descripción en cualquier edificio
          using (SqlCommand checkCommandDesc = new SqlCommand(checkQueryNameDescription, connection))
          {
            checkCommandDesc.Parameters.AddWithValue("@name", name);
            checkCommandDesc.Parameters.AddWithValue("@description", description);
            int countNameDescription = (int)checkCommandDesc.ExecuteScalar();

            if (countNameDescription > 0)
            {
              AlertMessage duplicateMessage = new AlertMessage
              {
                Tipo = "error",
                Mensaje = "Ya existe un piso con el mismo nombre y descripción en el sistema. Debe cambiar la descripción."
              };

              TempData["Alert"] = JsonSerializer.Serialize(duplicateMessage);
              return RedirectToAction("Pisos");
            }
          }

          // Insertar si pasa todas las validaciones
          using (SqlCommand insertCommand = new SqlCommand(insertQuery, connection))
          {
            insertCommand.Parameters.AddWithValue("@companySysId", companySysId);
            insertCommand.Parameters.AddWithValue("@name", name);
            insertCommand.Parameters.AddWithValue("@description", description);
            insertCommand.Parameters.AddWithValue("@entryUser", Guid.NewGuid());
            insertCommand.ExecuteNonQuery();
          }
        }

        AlertMessage successMessage = new AlertMessage
        {
          Tipo = "success",
          Mensaje = "El piso se ha insertado correctamente."
        };
        TempData["Alert"] = JsonSerializer.Serialize(successMessage);
        return RedirectToAction("Pisos");
      }
      catch (Exception ex)
      {
        AlertMessage errorMessage = new AlertMessage
        {
          Tipo = "error",
          Mensaje = $"Ocurrió un error al insertar el piso: {ex.Message}"
        };
        TempData["Alert"] = JsonSerializer.Serialize(errorMessage);
        return RedirectToAction("Pisos");
      }
    }



    public IActionResult EditarPiso(Guid buildingSysId, Guid companySysId, string name, string description)
    {
      if (buildingSysId == Guid.Empty)
      {
        // No se puede editar este piso
        AlertMessage errorMessage = new AlertMessage
        {
          Tipo = "error",
          Mensaje = "No se puede editar este piso."
        };

        TempData["Alert"] = JsonSerializer.Serialize(errorMessage);
        return RedirectToAction("Pisos");
      }

      if (string.IsNullOrWhiteSpace(description))
      {
        // Description is required
        AlertMessage errorMessage = new AlertMessage
        {
          Tipo = "error",
          Mensaje = "El campo descripción es obligatorio."
        };

        TempData["Alert"] = JsonSerializer.Serialize(errorMessage);
        return RedirectToAction("Pisos");
      }

      string checkQuery = @"
    SELECT COUNT(1)
    FROM [buildings]
    WHERE [companySysId] = @companySysId 
    AND [name] = @name 
    AND [buildingSysId] <> @buildingSysId;"; // Excluye el mismo piso para evitar falsa duplicación

      string checkQueryNameDescription = @"
  SELECT COUNT(1)
  FROM [buildings]
  WHERE [name] = @name 
  AND [description] = @description
  AND [buildingSysId] <> @buildingSysId;";


      string updateQuery = @"
    UPDATE [buildings]
    SET [companySysId] = @companySysId,
        [name] = @name,
        [description] = @description,
        [updateUser] = @updateUser,
        [updateDate] = GETDATE()
    WHERE [buildingSysId] = @buildingSysId;";

      try
      {
        string _connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
          using (SqlCommand checkCommand = new SqlCommand(checkQuery, connection))
          {
            checkCommand.Parameters.AddWithValue("@companySysId", companySysId);
            checkCommand.Parameters.AddWithValue("@name", name);
            checkCommand.Parameters.AddWithValue("@buildingSysId", buildingSysId);

            connection.Open();
            int count = (int)checkCommand.ExecuteScalar();

            if (count > 0)
            {
              // Duplicate name within the same companySysId
              AlertMessage duplicateMessage = new AlertMessage
              {
                Tipo = "error",
                Mensaje = "Ya existe un piso con el mismo nombre en este edificio."
              };

              TempData["Alert"] = JsonSerializer.Serialize(duplicateMessage);
              return RedirectToAction("Pisos");
            }
          }

          // Verificar si ya existe la misma combinación de nombre y descripción en cualquier edificio
          using (SqlCommand checkCommandDesc = new SqlCommand(checkQueryNameDescription, connection))
          {
            checkCommandDesc.Parameters.AddWithValue("@name", name);
            checkCommandDesc.Parameters.AddWithValue("@description", description);
            checkCommandDesc.Parameters.AddWithValue("@buildingSysId", buildingSysId); // <-- este es nuevo

            int countNameDescription = (int)checkCommandDesc.ExecuteScalar();

            if (countNameDescription > 0)
            {
              AlertMessage duplicateMessage = new AlertMessage
              {
                Tipo = "error",
                Mensaje = "Ya existe un piso con el mismo nombre y descripción en el sistema. Debe cambiar la descripción."
              };
              TempData["Alert"] = JsonSerializer.Serialize(duplicateMessage);
              return RedirectToAction("Pisos");
            }
          }

          // Update the building
          using (SqlCommand updateCommand = new SqlCommand(updateQuery, connection))
          {
            updateCommand.Parameters.AddWithValue("@companySysId", companySysId);
            updateCommand.Parameters.AddWithValue("@name", name);
            updateCommand.Parameters.AddWithValue("@description", description);
            updateCommand.Parameters.AddWithValue("@updateUser", Guid.NewGuid());
            updateCommand.Parameters.AddWithValue("@buildingSysId", buildingSysId);

            updateCommand.ExecuteNonQuery();
          }
        }

        // Success message
        AlertMessage successMessage = new AlertMessage
        {
          Tipo = "success",
          Mensaje = "El piso se ha actualizado correctamente."
        };

        TempData["Alert"] = JsonSerializer.Serialize(successMessage);
        return RedirectToAction("Pisos");
      }
      catch (Exception ex)
      {
        // Error message
        AlertMessage errorMessage = new AlertMessage
        {
          Tipo = "error",
          Mensaje = $"Ocurrió un error al actualizar el piso: {ex.Message}"
        };


        TempData["Alert"] = JsonSerializer.Serialize(errorMessage);
        return RedirectToAction("Pisos");
      }
    }



    [HttpPost]
    public async Task<IActionResult> SincronizarPisos(IFormFile excelFile)
    {
      if (excelFile == null || excelFile.Length == 0)
      {
        TempData["Alert2"] = "Por favor seleccione un archivo válido.";
        return RedirectToAction("Pisos");
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
          var sheet = package.Workbook.Worksheets.FirstOrDefault(s => s.Name.ToLower().Contains("piso"));

          if (sheet == null)
          {
            TempData["Alert2"] = "No se encontró la hoja de pisos en el archivo.";
            return RedirectToAction("Pisos");
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
              await SaveDataToDatabaseAsyncPisos(rowData);
          }

          TempData["Alert2"] = "Archivo procesado exitosamente.";
        }
      }
      catch (Exception ex)
      {
        TempData["Alert2"] = "Error al procesar el archivo: " + ex.Message;
      }

      return RedirectToAction("Pisos");
    }

    private async Task SaveDataToDatabaseAsyncPisos(List<string> rowData)
    {
      // Lee la cadena de conexión desde el archivo app.config, esta cadena se puede modificar.
      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

      using (var connection = new SqlConnection(connectionString))
      using (var command = new SqlCommand("InsertBuildingxExcel", connection))
      {
        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.AddWithValue("@name", rowData[1]);
        command.Parameters.AddWithValue("@description", rowData[2]);
        command.Parameters.AddWithValue("@entryUser", Guid.Empty);
        command.Parameters.AddWithValue("@companySysId", rowData[0]);
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

    public int ObtenerCantidadPisos()
    {
      int total = 0;
      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

      using (SqlConnection connection = new SqlConnection(connectionString))
      {
        string query = "SELECT COUNT(buildingSysId) FROM buildings";

        using (SqlCommand command = new SqlCommand(query, connection))
        {
          connection.Open();
          total = (int)command.ExecuteScalar();
        }
      }

      return total;
    }



    #endregion Fin Pisos
    #region Sectores
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
    [HttpGet]
    public IActionResult Sectores(string search, string hasAssets, string searchType, int page = 1, int pageSize = 20, string sortColumn = "name", string sortDirection = "asc")
    {
      IEnumerable<Floor> floors;
      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

      using (SqlConnection conn = new SqlConnection(connectionString))
      {
        conn.Open();
        string query = @"SELECT [floorSysId],
                        [buildingSysId],
                        [companySysId],
                        [name],
                        [description],
                        [entryUser],
                        [entryDate],
                        [updateUser],
                        [updateDate],
                        [rowGuid]
                FROM [floors]
                WHERE 1 = 1";

        if (!string.IsNullOrEmpty(search))
        {
          if (searchType == "description")
          {
            query += " AND [description] LIKE @searchPattern";
          }
          else
          {
            query += " AND [name] LIKE @searchPattern";
          }
        }


        using (SqlCommand cmd = new SqlCommand(query, conn))
        {
          cmd.Parameters.AddWithValue("@search", search ?? (object)DBNull.Value);
          cmd.Parameters.AddWithValue("@searchType", searchType ?? "name");
          cmd.Parameters.AddWithValue("@searchPattern", "%" + (search ?? "") + "%");

          SqlDataReader reader = cmd.ExecuteReader();

          List<Floor> floorList = new List<Floor>();
          while (reader.Read())
          {
            floorList.Add(new Floor(
                reader.GetGuid(0),   // FloorSysId
                reader.GetGuid(1),   // BuildingSysId
                reader.GetString(3), // Name
                reader.GetString(4), // Description
                reader.GetGuid(5),   // EntryUser
                reader.GetDateTime(6), // EntryDate
                reader.GetGuid(7),   // UpdateUser
                reader.GetDateTime(8), // UpdateDate
                reader.GetGuid(9),   // RowGuid
                reader.GetGuid(2)    // CompanySysId
            ));
          }

          floors = floorList;
          reader.Close();
        }
      }

      List<Sector> sectorList = new List<Sector>();


      Dictionary<Guid, int> activosCount = new Dictionary<Guid, int>();
      Dictionary<Guid, string> companyNames = new Dictionary<Guid, string>();
      Dictionary<Guid, string> buildingNames = new Dictionary<Guid, string>();

      using (SqlConnection conn = new SqlConnection(connectionString))
      {
        conn.Open();

        // Obtener el número de activos por edificio
        using (SqlCommand cmd = new SqlCommand("SELECT floorSysId, COUNT(*) AS Activos FROM assets GROUP BY floorSysId", conn)) //ActivosNueva
        {
          SqlDataReader reader = cmd.ExecuteReader();
          while (reader.Read())
          {
            activosCount[reader.GetGuid(0)] = reader.GetInt32(1);
          }
          reader.Close();
        }

        // Obtener los nombres de los edificios desde la tabla companies
        using (SqlCommand cmd = new SqlCommand("SELECT companySysId, name FROM companies", conn))
        {
          SqlDataReader reader = cmd.ExecuteReader();
          while (reader.Read())
          {
            companyNames[reader.GetGuid(0)] = reader.GetString(1);
          }
          reader.Close();
        }

        // Obtener los nombres de los pisos desde la tabla companies
        using (SqlCommand cmd = new SqlCommand("SELECT buildingSysId, name FROM buildings", conn))
        {
          SqlDataReader reader = cmd.ExecuteReader();
          while (reader.Read())
          {
            buildingNames[reader.GetGuid(0)] = reader.GetString(1);
          }
          reader.Close();
        }
      }

      using (SqlConnection conn = new SqlConnection(connectionString))
      {
        conn.Open();

        // C
        using (var cmd = new SqlCommand(
            "SELECT TOP 1 NOMBRE_UBICACION_C FROM UBICACION_C_NOMBRE ORDER BY ID DESC", conn))
        {
          var r = cmd.ExecuteScalar();
          ViewBag.NombreUbicacion = (r == null || r == DBNull.Value) ? "Ubicación C" : r.ToString();
        }

        // A
        using (var cmd = new SqlCommand(
            "SELECT TOP 1 NOMBRE_UBICACION_A FROM UBICACION_A_NOMBRE ORDER BY ID DESC", conn))
        {
          var r = cmd.ExecuteScalar();
          ViewBag.NombreUbicacionA = (r == null || r == DBNull.Value) ? "Ubicación A" : r.ToString();
        }

        // B
        using (var cmd = new SqlCommand(
            "SELECT TOP 1 NOMBRE_UBICACION_B FROM UBICACION_B_NOMBRE ORDER BY ID DESC", conn))
        {
          var r = cmd.ExecuteScalar();
          ViewBag.NombreUbicacionB = (r == null || r == DBNull.Value) ? "Ubicación B" : r.ToString();
        }
      }

      foreach (var floor in floors)
      {
        sectorList.Add(new Sector
        {
          floorSysId = floor.FloorSysId,
          buildingSysId = floor.BuildingSysId,
          name = floor.Name,
          description = floor.Description,
          entryUser = floor.EntryUser,
          entryDate = floor.EntryDate,
          updateUser = floor.UpdateUser,
          rowGuid = floor.RowGuid,
          Activos = activosCount.ContainsKey(floor.FloorSysId) ? activosCount[floor.FloorSysId] : 0,
          Edificio = companyNames.ContainsKey(floor.CompanySysId) ? companyNames[floor.CompanySysId] : "Desconocido",
          Piso = buildingNames.ContainsKey(floor.BuildingSysId) ? buildingNames[floor.BuildingSysId] : "Desconocido"
        });
      }

      // Aplicar filtro de activos
      if (hasAssets == "withAssets")
      {
        sectorList = sectorList.Where(e => e.Activos > 0).ToList();
      }
      else if (hasAssets == "withoutAssets")
      {
        sectorList = sectorList.Where(e => e.Activos == 0).ToList();
      }

      // Ordenar dinámicamente según la columna y dirección
      sectorList = sortColumn switch
      {
        "description" => sortDirection == "asc"
            ? sectorList.OrderBy(e => e.description).ToList()
            : sectorList.OrderByDescending(e => e.description).ToList(),
        "Activos" => sortDirection == "asc"
            ? sectorList.OrderBy(e => e.Activos).ToList()
            : sectorList.OrderByDescending(e => e.Activos).ToList(),
        "Edificio" => sortDirection == "asc"
            ? sectorList.OrderBy(e => e.Edificio).ToList()
            : sectorList.OrderByDescending(e => e.Edificio).ToList(),
        "Piso" => sortDirection == "asc"
          ? sectorList.OrderBy(e => e.Piso).ToList()
          : sectorList.OrderByDescending(e => e.Piso).ToList(),
        _ => sortDirection == "asc"
            ? sectorList.OrderBy(e => e.name).ToList()
            : sectorList.OrderByDescending(e => e.name).ToList(),
      };

      // Calcular la paginación
      var totalItems = sectorList.Count;
      var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
      var itemsOnPage = sectorList.Skip((page - 1) * pageSize).Take(pageSize).ToList();

      var model = new SectorViewModel
      {
        Sector = itemsOnPage,
        CurrentPage = page,
        TotalPages = totalPages,
        search = search
      };

      ViewBag.SearchQuery = search;
      ViewBag.SortColumn = sortColumn;
      ViewBag.SortDirection = sortDirection;
      ViewBag.Filter = hasAssets;
      ViewBag.SearchType = searchType;
      ViewBag.TotalSectores = ObtenerCantidadSectores();
      ViewBag.NombreUbicacionA = GetUltimoNombreUbicacionA() ?? "Ubicación A";
      ViewBag.NombreUbicacionB = GetUltimoNombreUbicacionB() ?? "Ubicación B";
      return View(model);
    }


    public ActionResult EliminarIndividualSectores(Guid id)
    {
      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;
      AlertMessage alert = new AlertMessage();
      if (id == Guid.Empty)
      {
        alert.Tipo = "error";
        alert.Mensaje = "El sector: 'Sin Asignar' no se puede eliminar.";
      }
      else
      {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
          connection.Open();

          // Verificaciones
          if (ExisteEnTablaSectores("officeses", id, connection))
          {
            alert.Tipo = "error";
            alert.Mensaje = "El sector no se puede eliminar porque está asociado a oficinas.";
          }
          else if (ExisteEnTablaSectores("assets", id, connection))
          {
            alert.Tipo = "error";
            alert.Mensaje = "El sector no se puede eliminar porque está asociado a activos.";
          }
          else
          {
            SqlCommand deleteCommand = new SqlCommand("DELETE FROM floors WHERE floorSysId = @id", connection);
            deleteCommand.Parameters.AddWithValue("@id", id);
            deleteCommand.ExecuteNonQuery();

            alert.Tipo = "success";
            alert.Mensaje = "El sector ha sido eliminado correctamente.";
          }
        }
      }

      TempData["Alert"] = JsonSerializer.Serialize(alert);
      return RedirectToAction("Sectores");
    }

    private bool ExisteEnTablaSectores(string tabla, Guid id, SqlConnection connection)
    {
      SqlCommand command = new SqlCommand($"SELECT COUNT(*) FROM {tabla} WHERE floorSysId = @id", connection);
      command.Parameters.AddWithValue("@id", id);
      int count = (int)command.ExecuteScalar();
      return count > 0;
    }

    public ActionResult EliminarBatchSectores(string ids)
    {
      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;
      AlertMessage alert = new AlertMessage();

      // Convertir la lista de ids desde el string recibido
      var idsList = ids.Split(',').Select(id => Guid.Parse(id)).ToList();

      // Verificar si alguno de los IDs es vacío
      if (idsList.Any(id => id == Guid.Empty))
      {
        alert.Tipo = "error";
        alert.Mensaje = "El sector: 'Sin Asignar' no se puede eliminar.";

        TempData["Alert"] = JsonSerializer.Serialize(alert);
        return RedirectToAction("Sectores");
      }

      using (SqlConnection connection = new SqlConnection(connectionString))
      {
        connection.Open();

        foreach (var id in idsList)
        {
          // Obtener el nombre del edificio desde la tabla 'companies' usando el id
          string nombrePiso = ObtenerNombreSector(id, connection);
          if (string.IsNullOrEmpty(nombrePiso))
          {
            alert.Tipo = "error";
            alert.Mensaje = $"El sector con id {id} no existe.";

            TempData["Alert"] = JsonSerializer.Serialize(alert);
            return RedirectToAction("Sectores");
          }
          // Verificaciones por cada id
          if (ExisteEnTablaSectores("officeses", id, connection))
          {
            alert.Tipo = "error";
            alert.Mensaje = $"El sector <strong>{nombrePiso}</strong> no se puede eliminar porque está asociado a oficinas.";

            TempData["Alert"] = JsonSerializer.Serialize(alert);
            return RedirectToAction("Sectores");
          }
          else if (ExisteEnTablaSectores("assets", id, connection))
          {
            alert.Tipo = "error";
            alert.Mensaje = $"El sector <strong>{nombrePiso}</strong> no se puede eliminar porque está asociado a activos.";

            TempData["Alert"] = JsonSerializer.Serialize(alert);
            return RedirectToAction("Sectores");
          }
          else
          {
            // Eliminar el registro de la tabla 'floors' para cada id
            SqlCommand deleteCommand = new SqlCommand("DELETE FROM floors WHERE floorSysId = @id", connection);
            deleteCommand.Parameters.AddWithValue("@id", id);
            deleteCommand.ExecuteNonQuery();
          }
        }

        alert.Tipo = "success";
        alert.Mensaje = "Los Sectores seleccionados han sido eliminados correctamente.";
      }


      TempData["Alert"] = JsonSerializer.Serialize(alert);
      return RedirectToAction("Sectores");
    }

    // Función para obtener el nombre del piso a partir del id
    private string ObtenerNombreSector(Guid id, SqlConnection connection)
    {
      SqlCommand command = new SqlCommand("SELECT name FROM floors WHERE floorSysId = @id", connection);
      command.Parameters.AddWithValue("@id", id);
      var result = command.ExecuteScalar();
      return result != null ? result.ToString() : null;

    }
    public IActionResult InsertarSector(Guid companySysId, Guid buildingSysId, string name, string description)
    {
      if (string.IsNullOrWhiteSpace(description))
      {
        AlertMessage errorMessage = new AlertMessage
        {
          Tipo = "error",
          Mensaje = "El campo descripción es obligatorio."
        };


        TempData["Alert"] = JsonSerializer.Serialize(errorMessage);
        return RedirectToAction("Sectores");
      }

      string checkQuery = @"
        SELECT COUNT(1)
        FROM [floors]
        WHERE [buildingSysId] = @buildingSysId AND [companySysId] = @companySysId AND [name] = @name;";

      string insertQuery = @"
        INSERT INTO [floors]
           ([floorSysId]
           ,[buildingSysId]
           ,[companySysId]
           ,[name]
           ,[description]
           ,[entryUser]
           ,[entryDate]
           ,[updateUser]
           ,[updateDate]
           ,[rowGuid])
        VALUES
           (NEWID()
           ,@buildingSysId
           ,@companySysId
           ,@name
           ,@description
           ,@entryUser
           ,GETDATE()
           ,@entryUser
           ,GETDATE()
           ,NEWID());";

      try
      {
        string _connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
          using (SqlCommand checkCommand = new SqlCommand(checkQuery, connection))
          {
            checkCommand.Parameters.AddWithValue("@buildingSysId", buildingSysId);
            checkCommand.Parameters.AddWithValue("@companySysId", companySysId);
            checkCommand.Parameters.AddWithValue("@name", name);

            connection.Open();
            int count = (int)checkCommand.ExecuteScalar();

            if (count > 0)
            {
              AlertMessage duplicateMessage = new AlertMessage
              {
                Tipo = "error",
                Mensaje = "Ya existe un sector con el mismo nombre en este piso y edificio."
              };

              TempData["Alert"] = JsonSerializer.Serialize(duplicateMessage);
              return RedirectToAction("Sectores");
            }
          }

          using (SqlCommand insertCommand = new SqlCommand(insertQuery, connection))
          {
            insertCommand.Parameters.AddWithValue("@buildingSysId", buildingSysId);
            insertCommand.Parameters.AddWithValue("@companySysId", companySysId);
            insertCommand.Parameters.AddWithValue("@name", name);
            insertCommand.Parameters.AddWithValue("@description", description);
            insertCommand.Parameters.AddWithValue("@entryUser", Guid.NewGuid());

            insertCommand.ExecuteNonQuery();
          }
        }

        AlertMessage successMessage = new AlertMessage
        {
          Tipo = "success",
          Mensaje = "El sector se ha insertado correctamente."
        };

        TempData["Alert"] = JsonSerializer.Serialize(successMessage);
        return RedirectToAction("Sectores");
      }
      catch (Exception ex)
      {
        AlertMessage errorMessage = new AlertMessage
        {
          Tipo = "error",
          Mensaje = $"Ocurrió un error al insertar el sector: {ex.Message}"
        };

        TempData["Alert"] = JsonSerializer.Serialize(errorMessage);
        return RedirectToAction("Sectores");
      }
    }

    public IActionResult EditarSector(Guid floorSysId, Guid companySysId, Guid buildingSysId, string name, string description)
    {
      if (string.IsNullOrWhiteSpace(description))
      {
        AlertMessage errorMessage = new AlertMessage
        {
          Tipo = "error",
          Mensaje = "El campo descripción es obligatorio."
        };

        TempData["Alert"] = JsonSerializer.Serialize(errorMessage);
        return RedirectToAction("Sectores");
      }

      // Verificar si el companySysId es el restringido
      if (floorSysId == Guid.Empty)
      {
        AlertMessage errorMessage = new AlertMessage
        {
          Tipo = "error",
          Mensaje = "No se puede editar el sector: Sin Asignar."
        };
        TempData["Alert"] = JsonSerializer.Serialize(errorMessage);
        return RedirectToAction("Sectores");
      }

      string checkQuery = @"
        SELECT COUNT(1)
        FROM [floors]
        WHERE [buildingSysId] = @buildingSysId 
          AND [companySysId] = @companySysId 
          AND [name] = @name 
          AND [floorSysId] != @floorSysId;";

      string updateQuery = @"
        UPDATE [floors]
        SET [buildingSysId] = @buildingSysId,
            [companySysId] = @companySysId,
            [name] = @name,
            [description] = @description,
            [updateUser] = @entryUser,
            [updateDate] = GETDATE()
        WHERE [floorSysId] = @floorSysId;";

      try
      {
        string _connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
          connection.Open();

          // Verificar duplicados
          using (SqlCommand checkCommand = new SqlCommand(checkQuery, connection))
          {
            checkCommand.Parameters.AddWithValue("@buildingSysId", buildingSysId);
            checkCommand.Parameters.AddWithValue("@companySysId", companySysId);
            checkCommand.Parameters.AddWithValue("@name", name);
            checkCommand.Parameters.AddWithValue("@floorSysId", floorSysId);

            int count = (int)checkCommand.ExecuteScalar();

            if (count > 0)
            {
              AlertMessage duplicateMessage = new AlertMessage
              {
                Tipo = "error",
                Mensaje = "Ya existe un sector con el mismo nombre en este piso y edificio."
              };
              TempData["Alert"] = JsonSerializer.Serialize(duplicateMessage);
              return RedirectToAction("Sectores");
            }
          }

          // Ejecutar la actualización
          using (SqlCommand updateCommand = new SqlCommand(updateQuery, connection))
          {
            updateCommand.Parameters.AddWithValue("@floorSysId", floorSysId);
            updateCommand.Parameters.AddWithValue("@buildingSysId", buildingSysId);
            updateCommand.Parameters.AddWithValue("@companySysId", companySysId);
            updateCommand.Parameters.AddWithValue("@name", name);
            updateCommand.Parameters.AddWithValue("@description", description);
            updateCommand.Parameters.AddWithValue("@entryUser", Guid.NewGuid());

            updateCommand.ExecuteNonQuery();
          }
        }

        AlertMessage successMessage = new AlertMessage
        {
          Tipo = "success",
          Mensaje = "El sector se ha actualizado correctamente."
        };

        TempData["Alert"] = JsonSerializer.Serialize(successMessage);
        return RedirectToAction("Sectores");
      }
      catch (Exception ex)
      {
        AlertMessage errorMessage = new AlertMessage
        {
          Tipo = "error",
          Mensaje = $"Ocurrió un error al actualizar el sector: {ex.Message}"
        };
        TempData["Alert"] = JsonSerializer.Serialize(errorMessage);
        return RedirectToAction("Sectores");
      }
    }




    [HttpPost]
    public async Task<IActionResult> SincronizarSectores(IFormFile excelFile)
    {
      if (excelFile == null || excelFile.Length == 0)
      {
        TempData["Alert2"] = "Por favor seleccione un archivo válido.";
        return RedirectToAction("Sectores");
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
          var sheet = package.Workbook.Worksheets.FirstOrDefault(s => s.Name.ToLower().Contains("sector"));

          if (sheet == null)
          {
            TempData["Alert2"] = "No se encontró la hoja de sectores en el archivo.";
            return RedirectToAction("Sectores");
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
              await SaveDataToDatabaseAsyncSectores(rowData);
          }

          TempData["Alert2"] = "Archivo procesado exitosamente.";
        }
      }
      catch (Exception ex)
      {
        TempData["Alert2"] = "Error al procesar el archivo: " + ex.Message;
      }

      return RedirectToAction("Sectores");
    }

    public class _currentUser
    {
      public Guid Id { get; set; }
    }

    private async Task SaveDataToDatabaseAsyncSectores(List<string> rowData)
    {
      // Lee la cadena de conexión desde el archivo app.config, esta cadena se puede modificar.
      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

      _currentUser user = new _currentUser();
      user.Id = Guid.NewGuid();

      using (var connection = new SqlConnection(connectionString))
      using (var command = new SqlCommand("InsertFloorxExcel", connection))
      {
        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.AddWithValue("@piso", rowData[1]);
        command.Parameters.AddWithValue("@edificio", rowData[0]);
        command.Parameters.AddWithValue("@name", rowData[2]);
        command.Parameters.AddWithValue("@description", rowData[3]);
        command.Parameters.AddWithValue("@entryUser", Guid.Empty);
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

    public int ObtenerCantidadSectores()
    {
      int total = 0;
      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

      using (SqlConnection connection = new SqlConnection(connectionString))
      {
        string query = "SELECT COUNT(floorSysId) FROM floors";

        using (SqlCommand command = new SqlCommand(query, connection))
        {
          connection.Open();
          total = (int)command.ExecuteScalar();
        }
      }

      return total;
    }



    #endregion Fin Sectores


    #region Oficinas
    [HttpGet]
    public IActionResult Oficinas(string search, string hasAssets, string searchType, int page = 1, int pageSize = 20, string sortColumn = "name", string sortDirection = "asc")
    {
      IEnumerable<Oficina> floors;
      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

      using (SqlConnection conn = new SqlConnection(connectionString))
      {
        conn.Open();
        string query = @"SELECT
                        [officeSysId],
                        [companySysId],
                        [buildingSysId],
                        [businessUnitSysId],
                        [floorSysId],
                        [deptSysId],
                        [tagSysId],
                        [name],
                        [description],
                        [entryUser],
                        [entryDate],
                        [updateUser],
                        [updateDate],
                        [rowGuid],
                        [IsEnable]
                FROM [officeses]
                WHERE 1 = 1";

        if (!string.IsNullOrEmpty(search))
        {
          if (searchType == "description")
          {
            query += " AND [description] LIKE @searchPattern";
          }
          else
          {
            query += " AND [name] LIKE @searchPattern";
          }
        }


        using (SqlCommand cmd = new SqlCommand(query, conn))
        {
          cmd.Parameters.AddWithValue("@search", search ?? (object)DBNull.Value);
          cmd.Parameters.AddWithValue("@searchType", searchType ?? "name");
          cmd.Parameters.AddWithValue("@searchPattern", "%" + (search ?? "") + "%");

          SqlDataReader reader = cmd.ExecuteReader();

          List<Oficina> floorList = new List<Oficina>();
          while (reader.Read())
          {
            floorList.Add(new Oficina(
                reader.GetGuid(0),
                reader.GetGuid(1),
                reader.GetGuid(2),
                reader.GetGuid(3),
                reader.GetGuid(4),
                reader.GetGuid(5),
                reader.GetGuid(6),
                reader.GetString(7),
                reader.GetString(8),
                reader.GetGuid(9),
                reader.GetDateTime(10),
                reader.GetGuid(11),
                reader.GetDateTime(12),
                reader.GetGuid(13),
                reader.GetBoolean(14)
            ));
          }

          floors = floorList;
          reader.Close();
        }
      }

      List<Oficina> sectorList = new List<Oficina>();


      Dictionary<Guid, int> activosCount = new Dictionary<Guid, int>();
      Dictionary<Guid, string> companyNames = new Dictionary<Guid, string>();
      Dictionary<Guid, string> buildingNames = new Dictionary<Guid, string>();
      Dictionary<Guid, string> floorNames = new Dictionary<Guid, string>();

      using (SqlConnection conn = new SqlConnection(connectionString))
      {
        conn.Open();

        // Obtener el número de activos por edificio
        using (SqlCommand cmd = new SqlCommand("SELECT officeSysId, COUNT(*) AS Activos FROM assets GROUP BY officeSysId", conn)) //ActivosNueva
        {
          SqlDataReader reader = cmd.ExecuteReader();
          while (reader.Read())
          {
            activosCount[reader.GetGuid(0)] = reader.GetInt32(1);
          }
          reader.Close();
        }

        // Obtener los nombres de los edificios desde la tabla companies
        using (SqlCommand cmd = new SqlCommand("SELECT companySysId, name FROM companies", conn))
        {
          SqlDataReader reader = cmd.ExecuteReader();
          while (reader.Read())
          {
            companyNames[reader.GetGuid(0)] = reader.GetString(1);
          }
          reader.Close();
        }

        // Obtener los nombres de los pisos desde la tabla companies
        using (SqlCommand cmd = new SqlCommand("SELECT buildingSysId, name FROM buildings", conn))
        {
          SqlDataReader reader = cmd.ExecuteReader();
          while (reader.Read())
          {
            buildingNames[reader.GetGuid(0)] = reader.GetString(1);
          }
          reader.Close();
        }

        // Obtener los nombres de los sectores desde la tabla companies
        using (SqlCommand cmd = new SqlCommand("SELECT floorSysId, name FROM floors", conn))
        {
          SqlDataReader reader = cmd.ExecuteReader();
          while (reader.Read())
          {
            floorNames[reader.GetGuid(0)] = reader.GetString(1);
          }
          reader.Close();
        }
      }

      foreach (var floor in floors)
      {
        sectorList.Add(new Oficina
        {
          OfficeSysId = floor.OfficeSysId,
          FloorSysId = floor.FloorSysId,
          BuildingSysId = floor.BuildingSysId,
          Name = floor.Name,
          Description = floor.Description,
          EntryUser = floor.EntryUser,
          EntryDate = floor.EntryDate,
          UpdateUser = floor.UpdateUser,
          RowGuid = floor.RowGuid,
          IsEnable = floor.IsEnable,
          Activos = activosCount.ContainsKey(floor.OfficeSysId) ? activosCount[floor.OfficeSysId] : 0,
          Edificios = companyNames.ContainsKey(floor.CompanySysId) ? companyNames[floor.CompanySysId] : "Desconocido",
          Pisos = buildingNames.ContainsKey(floor.BuildingSysId) ? buildingNames[floor.BuildingSysId] : "Desconocido",
          Sector = floorNames.ContainsKey(floor.FloorSysId) ? floorNames[floor.FloorSysId] : "Desconocido"
        });
      }

      // Aplicar filtro de activos
      if (hasAssets == "withAssets")
      {
        sectorList = sectorList.Where(e => e.Activos > 0).ToList();
      }
      else if (hasAssets == "withoutAssets")
      {
        sectorList = sectorList.Where(e => e.Activos == 0).ToList();
      }

      // Ordenar dinámicamente según la columna y dirección
      sectorList = sortColumn switch
      {
        "description" => sortDirection == "asc"
            ? sectorList.OrderBy(e => e.Description).ToList()
            : sectorList.OrderByDescending(e => e.Description).ToList(),
        "Activos" => sortDirection == "asc"
            ? sectorList.OrderBy(e => e.Activos).ToList()
            : sectorList.OrderByDescending(e => e.Activos).ToList(),
        "Edificios" => sortDirection == "asc"
            ? sectorList.OrderBy(e => e.Edificios).ToList()
            : sectorList.OrderByDescending(e => e.Edificios).ToList(),
        "Pisos" => sortDirection == "asc"
          ? sectorList.OrderBy(e => e.Pisos).ToList()
          : sectorList.OrderByDescending(e => e.Pisos).ToList(),
        "Sector" => sortDirection == "asc"
       ? sectorList.OrderBy(e => e.Pisos).ToList()
       : sectorList.OrderByDescending(e => e.Pisos).ToList(),
        _ => sortDirection == "asc"
            ? sectorList.OrderBy(e => e.Name).ToList()
            : sectorList.OrderByDescending(e => e.Name).ToList(),
      };

      // Calcular la paginación
      var totalItems = sectorList.Count;
      var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
      var itemsOnPage = sectorList.Skip((page - 1) * pageSize).Take(pageSize).ToList();

      var model = new OficinasViewModel
      {
        Oficinas = itemsOnPage,
        CurrentPage = page,
        TotalPages = totalPages,
        search = search
      };

      ViewBag.SearchQuery = search;
      ViewBag.SortColumn = sortColumn;
      ViewBag.SortDirection = sortDirection;
      ViewBag.Filter = hasAssets;
      ViewBag.SearchType = searchType;
      ViewBag.TotalOficinas = ObtenerCantidadOficinas();
      ViewBag.NombreUbicacionA = GetUltimoNombreUbicacionA() ?? "Ubicación A";
      ViewBag.NombreUbicacion = GetUltimoNombreUbicacionC() ?? "Ubicación C";
      ViewBag.NombreUbicacionB = GetUltimoNombreUbicacionB() ?? "Ubicación B";
      return View(model);
    }

    public ActionResult EliminarIndividualOficinas(Guid id)
    {
      AlertMessage alert = new AlertMessage();

      try
      {
        string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

        using (SqlConnection connection = new SqlConnection(connectionString))
        {
          connection.Open();

          // Obtener el estado actual
          string querySelect = "SELECT IsEnable FROM officeses WHERE officeSysId = @OfficeSysId";
          bool currentStatus;

          using (SqlCommand cmdSelect = new SqlCommand(querySelect, connection))
          {
            cmdSelect.Parameters.AddWithValue("@OfficeSysId", id);
            object result = cmdSelect.ExecuteScalar();

            if (result == null)
            {
              alert.Tipo = "error";
              alert.Mensaje = "Oficina no encontrada.";

              TempData["Alert"] = JsonSerializer.Serialize(alert);
              return RedirectToAction("Oficinas");
            }

            currentStatus = (bool)result;
          }

          // Cambiar el estado
          bool newStatus = !currentStatus;

          string queryUpdate = @"
                UPDATE officeses
                SET IsEnable = @NewStatus, updateDate = GETDATE()
                WHERE officeSysId = @OfficeSysId";

          using (SqlCommand cmdUpdate = new SqlCommand(queryUpdate, connection))
          {
            cmdUpdate.Parameters.AddWithValue("@NewStatus", newStatus);
            cmdUpdate.Parameters.AddWithValue("@OfficeSysId", id);

            int rowsAffected = cmdUpdate.ExecuteNonQuery();

            if (rowsAffected > 0)
            {
              alert.Tipo = "success";
              alert.Mensaje = newStatus ? "Oficina activada correctamente." : "Oficina desactivada correctamente.";
            }
            else
            {
              alert.Tipo = "error";
              alert.Mensaje = "No se pudo actualizar la oficina.";
            }
          }
        }
      }
      catch (Exception ex)
      {
        alert.Tipo = "error";
        alert.Mensaje = "Error al actualizar la oficina: " + ex.Message;
      }

      // Serializar el mensaje de alerta y redirigir
      TempData["Alert"] = JsonSerializer.Serialize(alert);
      return RedirectToAction("Oficinas");
    }


    [HttpPost]
    public IActionResult ActivarDesactivarBatchOficinas([FromBody] BatchToggleRequest request)
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

          string query = @"UPDATE [officeses] 
                                 SET [IsEnable] = @estado 
                                 WHERE [officeSysId] = @idrol";

          using (SqlCommand command = new SqlCommand(query, connection))
          {
            // Determinar el valor de bloqueo según la acción
            int estado = (action == "desactivar") ? 0 : 1;

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
        using (SqlCommand command = new SqlCommand("SELECT IsEnable FROM officeses WHERE officeSysId = @idrol", connection))
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
      string query = "SELECT name FROM officeses WHERE officeSysId = @RoleId";

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

    public IActionResult InsertarOficina(Guid companySysId, Guid buildingSysId, Guid floorSysId, string name, string description, Guid entryUser)
    {
      if (string.IsNullOrWhiteSpace(description))
      {
        AlertMessage errorMessage = new AlertMessage
        {
          Tipo = "error",
          Mensaje = "La descripción es requerida para crear una oficina."
        };
        TempData["Alert"] = JsonSerializer.Serialize(errorMessage);
        return RedirectToAction("Oficinas");
      }

      try
      {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
          connection.Open();

          // Verificar si ya existe una oficina con el mismo nombre en la misma ubicación
          string checkQuery = @"SELECT COUNT(1) 
                                  FROM officeses 
                                  WHERE name = @name 
                                  AND companySysId = @companySysId 
                                  AND buildingSysId = @buildingSysId 
                                  AND floorSysId = @floorSysId";

          using (SqlCommand checkCommand = new SqlCommand(checkQuery, connection))
          {
            checkCommand.Parameters.AddWithValue("@name", name);
            checkCommand.Parameters.AddWithValue("@companySysId", companySysId);
            checkCommand.Parameters.AddWithValue("@buildingSysId", buildingSysId);
            checkCommand.Parameters.AddWithValue("@floorSysId", floorSysId);

            int existingCount = (int)checkCommand.ExecuteScalar();

            if (existingCount > 0)
            {
              AlertMessage errorMessage = new AlertMessage
              {
                Tipo = "error",
                Mensaje = "Ya existe una oficina con este nombre en el mismo edificio, piso y sector."
              };
              TempData["Alert"] = JsonSerializer.Serialize(errorMessage);
              return RedirectToAction("Oficinas");
            }
          }

          // Si no existe, insertar la nueva oficina
          string query = @"INSERT INTO officeses (
                                officeSysId, companySysId, buildingSysId, businessUnitSysId, floorSysId, deptSysId, tagSysId, 
                                name, description, entryUser, entryDate, updateUser, updateDate, rowGuid, IsEnable)
                              VALUES (
                                @officeSysId, @companySysId, @buildingSysId, @businessUnitSysId, @floorSysId, @deptSysId, @tagSysId, 
                                @name, @description, @entryUser, @entryDate, @updateUser, @updateDate, @rowGuid, @IsEnable)";

          using (SqlCommand command = new SqlCommand(query, connection))
          {
            command.Parameters.AddWithValue("@officeSysId", Guid.NewGuid());
            command.Parameters.AddWithValue("@companySysId", companySysId);
            command.Parameters.AddWithValue("@buildingSysId", buildingSysId);
            command.Parameters.AddWithValue("@businessUnitSysId", Guid.NewGuid());
            command.Parameters.AddWithValue("@floorSysId", floorSysId);
            command.Parameters.AddWithValue("@deptSysId", Guid.NewGuid());
            command.Parameters.AddWithValue("@tagSysId", Guid.NewGuid());
            command.Parameters.AddWithValue("@name", name);
            command.Parameters.AddWithValue("@description", description);
            command.Parameters.AddWithValue("@entryUser", entryUser);
            command.Parameters.AddWithValue("@entryDate", DateTime.Now);
            command.Parameters.AddWithValue("@updateUser", entryUser);
            command.Parameters.AddWithValue("@updateDate", DateTime.Now);
            command.Parameters.AddWithValue("@rowGuid", Guid.NewGuid());
            command.Parameters.AddWithValue("@IsEnable", true);

            command.ExecuteNonQuery();
          }
        }

        AlertMessage successMessage = new AlertMessage
        {
          Tipo = "success",
          Mensaje = "Oficina agregada correctamente."
        };

        TempData["Alert"] = JsonSerializer.Serialize(successMessage);
        return RedirectToAction("Oficinas");
      }
      catch (Exception ex)
      {
        AlertMessage errorMessage = new AlertMessage
        {
          Tipo = "error",
          Mensaje = "Error al insertar la oficina: " + ex.Message
        };

        TempData["Alert"] = JsonSerializer.Serialize(errorMessage);
        return RedirectToAction("Oficinas");
      }
    }

    public IActionResult EditarOficina(Guid officeSysId, Guid companySysId, Guid buildingSysId, Guid floorSysId, string name, string description, Guid updateUser)
    {
      if (string.IsNullOrWhiteSpace(description))
      {
        AlertMessage errorMessage = new AlertMessage
        {
          Tipo = "error",
          Mensaje = "La descripción es requerida para editar la oficina."
        };
        TempData["Alert"] = JsonSerializer.Serialize(errorMessage);
        return RedirectToAction("Oficinas");

      }

      // Verificar si el companySysId es el restringido
      if (officeSysId == Guid.Empty)
      {
        AlertMessage errorMessage = new AlertMessage
        {
          Tipo = "error",
          Mensaje = "No se puede editar el sector: Sin Asignar."
        };
        TempData["Alert"] = JsonSerializer.Serialize(errorMessage);
        return RedirectToAction("Oficinas");
      }

      try
      {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
          connection.Open();

          // Verificar si ya existe una oficina con el mismo nombre en la misma ubicación (excepto la misma oficina que estamos editando)
          string checkQuery = @"SELECT COUNT(1) 
                                   FROM officeses 
                                   WHERE name = @name 
                                   AND companySysId = @companySysId 
                                   AND buildingSysId = @buildingSysId 
                                   AND floorSysId = @floorSysId
                                   AND officeSysId != @officeSysId"; // Excluye la oficina que estamos editando

          using (SqlCommand checkCommand = new SqlCommand(checkQuery, connection))
          {
            checkCommand.Parameters.AddWithValue("@name", name);
            checkCommand.Parameters.AddWithValue("@companySysId", companySysId);
            checkCommand.Parameters.AddWithValue("@buildingSysId", buildingSysId);
            checkCommand.Parameters.AddWithValue("@floorSysId", floorSysId);
            checkCommand.Parameters.AddWithValue("@officeSysId", officeSysId);

            int existingCount = (int)checkCommand.ExecuteScalar();

            if (existingCount > 0)
            {
              AlertMessage errorMessage = new AlertMessage
              {
                Tipo = "error",
                Mensaje = "Ya existe una oficina con este nombre en el mismo edificio, piso y sector."
              };
              TempData["Alert"] = JsonSerializer.Serialize(errorMessage);
              return RedirectToAction("Oficinas");
            }
          }

          // Si no existe, actualizar la oficina
          string updateQuery = @"UPDATE officeses
                                   SET companySysId = @companySysId,
                                       buildingSysId = @buildingSysId,
                                       floorSysId = @floorSysId,
                                       name = @name,
                                       description = @description,
                                       updateUser = @updateUser,
                                       updateDate = @updateDate
                                   WHERE officeSysId = @officeSysId";

          using (SqlCommand command = new SqlCommand(updateQuery, connection))
          {
            command.Parameters.AddWithValue("@companySysId", companySysId);
            command.Parameters.AddWithValue("@buildingSysId", buildingSysId);
            command.Parameters.AddWithValue("@floorSysId", floorSysId);
            command.Parameters.AddWithValue("@name", name);
            command.Parameters.AddWithValue("@description", description);
            command.Parameters.AddWithValue("@updateUser", updateUser);
            command.Parameters.AddWithValue("@updateDate", DateTime.Now);
            command.Parameters.AddWithValue("@officeSysId", officeSysId);

            command.ExecuteNonQuery();
          }
        }

        AlertMessage successMessage = new AlertMessage
        {
          Tipo = "success",
          Mensaje = "Oficina editada correctamente."
        };

        TempData["Alert"] = JsonSerializer.Serialize(successMessage);
        return RedirectToAction("Oficinas");
      }
      catch (Exception ex)
      {
        AlertMessage errorMessage = new AlertMessage
        {
          Tipo = "error",
          Mensaje = "Error al editar la oficina: " + ex.Message
        };

        TempData["Alert"] = JsonSerializer.Serialize(errorMessage);
        return RedirectToAction("Oficinas");
      }
    }


    [HttpPost]
    public async Task<IActionResult> SincronizarOficinas(IFormFile excelFile)
    {
      if (excelFile == null || excelFile.Length == 0)
      {
        TempData["Alert2"] = "Por favor seleccione un archivo válido.";
        return RedirectToAction("Oficinas");
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
          var sheet = package.Workbook.Worksheets.FirstOrDefault(s => s.Name.ToLower().Contains("oficina"));

          if (sheet == null)
          {
            TempData["Alert2"] = "No se encontró la hoja de oficinas en el archivo.";
            return RedirectToAction("Oficinas");
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
              await SaveDataToDatabaseAsyncOficinas(rowData);
          }

          TempData["Alert2"] = "Archivo procesado exitosamente.";
        }
      }
      catch (Exception ex)
      {
        TempData["Alert2"] = "Error al procesar el archivo: " + ex.Message;
      }

      return RedirectToAction("Oficinas");
    }


    private async Task SaveDataToDatabaseAsyncOficinas(List<string> rowData)
    {
      // Lee la cadena de conexión desde el archivo app.config, esta cadena se puede modificar.
      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

      _currentUser user = new _currentUser();
      user.Id = Guid.NewGuid();

      using (var connection = new SqlConnection(connectionString))
      using (var command = new SqlCommand("InsertOfficexExcel", connection))
      {
        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.AddWithValue("@edificio", rowData[0]);
        command.Parameters.AddWithValue("@piso", rowData[1]);
        command.Parameters.AddWithValue("@sector", rowData[2]);
        command.Parameters.AddWithValue("@name", rowData[3]);
        command.Parameters.AddWithValue("@description", rowData[4]);
        command.Parameters.AddWithValue("@entryUser", Guid.Empty);
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

    public int ObtenerCantidadOficinas()
    {
      int total = 0;
      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

      using (SqlConnection connection = new SqlConnection(connectionString))
      {
        string query = "SELECT COUNT(officeSysId) FROM officeses";

        using (SqlCommand command = new SqlCommand(query, connection))
        {
          connection.Open();
          total = (int)command.ExecuteScalar();
        }
      }

      return total;
    }

    #endregion Fin Oficinas
    #region Gerencia
    [HttpGet]
    public IActionResult Gerencia(string search, string hasAssets, string searchType, int page = 1, int pageSize = 20, string sortColumn = "name", string sortDirection = "asc")
    {
      IEnumerable<Gerencia> gerencias;

      if (searchType == "description")
      {
        gerencias = ObtenerTodasLogicaA();
      }
      else
      {
        gerencias = ObtenerTodasLogicaA();
      }

      List<Gerencia> gerenciasList = new List<Gerencia>();

      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;
      Dictionary<int, int> activosCount = new Dictionary<int, int>();

      using (SqlConnection conn = new SqlConnection(connectionString))
      {
        conn.Open();
        SqlCommand cmd = new SqlCommand("SELECT Logico_A, COUNT(*) AS Activos FROM assets GROUP BY Logico_A", conn);
        SqlDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
          activosCount[reader.GetInt32(0)] = reader.GetInt32(1);
        }
      }

      foreach (var gerencia in gerencias)
      {
        gerenciasList.Add(new Gerencia
        {
          IdLogicoA = gerencia.IdLogicoA,
          Nombre = gerencia.Nombre,
          Descripcion = gerencia.Descripcion,
          UserSysId = gerencia.UserSysId,
          Activos = activosCount.ContainsKey(gerencia.IdLogicoA) ? activosCount[gerencia.IdLogicoA] : 0
        });
      }

      // Aplicar filtro
      if (hasAssets == "withAssets")
      {
        gerenciasList = gerenciasList.Where(e => e.Activos > 0).ToList();
      }
      else if (hasAssets == "withoutAssets")
      {
        gerenciasList = gerenciasList.Where(e => e.Activos == 0).ToList();
      }

      // Ordenar dinámicamente según la columna y la dirección
      gerenciasList = sortColumn switch
      {
        "description" => sortDirection == "asc"
            ? gerenciasList.OrderBy(e => e.Descripcion).ToList()
            : gerenciasList.OrderByDescending(e => e.Descripcion).ToList(),
        "Activos" => sortDirection == "asc"
            ? gerenciasList.OrderBy(e => e.Activos).ToList()
            : gerenciasList.OrderByDescending(e => e.Activos).ToList(),
        _ => sortDirection == "asc"
            ? gerenciasList.OrderBy(e => e.Nombre).ToList()
            : gerenciasList.OrderByDescending(e => e.Nombre).ToList(),
      };



      // Calcular la paginación
      var totalItems = gerenciasList.Count;
      var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
      var itemsOnPage = gerenciasList.Skip((page - 1) * pageSize).Take(pageSize).ToList();

      // Crear el modelo de paginación
      var model = new GerenciaViewModel
      {
        Gerencias = itemsOnPage,
        CurrentPage = page,
        TotalPages = totalPages,
        search = search
      };

      // Pasar el término de búsqueda, columna y dirección actuales a la vista
      ViewBag.SearchQuery = search;
      ViewBag.SortColumn = sortColumn;
      ViewBag.SortDirection = sortDirection;
      ViewBag.Filter = hasAssets; // Pasar el filtro actual a la vista
      ViewBag.SearchType = searchType;
      ViewBag.TotalGerencias = ObtenerCantidadGerencia();
      return View(model);

    }


    public List<Gerencia> ObtenerTodasLogicaA()
    {
      var listaLogicaA_E = new List<Gerencia>();
      string _connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

      using (var connection = new SqlConnection(_connectionString))
      using (var command = new SqlCommand("GetLogicoA", connection))
      {
        command.CommandType = CommandType.StoredProcedure;
        connection.Open();

        using (var reader = command.ExecuteReader())
        {
          while (reader.Read())
          {
            int idLogicaA = Convert.ToInt32(reader["IdLogicoA"]);
            string nombre = reader["Nombre"].ToString();
            string descripcion = reader["Descripcion"].ToString();

            listaLogicaA_E.Add(new Gerencia(idLogicaA, nombre, descripcion));
          }
        }
      }

      return listaLogicaA_E;
    }


    public ActionResult EliminarIndividualGerencia(Guid id)
    {
      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;
      AlertMessage alert = new AlertMessage();

      if (id == Guid.Empty)
      {
        alert.Tipo = "error";
        alert.Mensaje = "El edificio: 'Sin Asignar' no se puede eliminar.";
      }
      else
      {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
          connection.Open();

          // Verificaciones
          if (ExisteEnTablaGerencia("floors", id, connection))
          {
            alert.Tipo = "error";
            alert.Mensaje = "El edificio no se puede eliminar porque está asociado a pisos.";
          }
          else if (ExisteEnTablaGerencia("departments", id, connection))
          {
            alert.Tipo = "error";
            alert.Mensaje = "El edificio no se puede eliminar porque está asociado a sectores.";
          }
          else if (ExisteEnTablaGerencia("assets", id, connection))
          {
            alert.Tipo = "error";
            alert.Mensaje = "El edificio no se puede eliminar porque está asociado a activos.";
          }
          else if (ExisteEnTablaGerencia("buildings", id, connection))
          {
            alert.Tipo = "error";
            alert.Mensaje = "El edificio no se puede eliminar porque está asociado a pisos.";
          }
          else if (ExisteEnTablaGerencia("officeses", id, connection))
          {
            alert.Tipo = "error";
            alert.Mensaje = "El edificio no se puede eliminar porque está asociado a oficinas.";
          }
          else
          {
            SqlCommand deleteCommand = new SqlCommand("DELETE FROM companies WHERE companySysId = @id", connection);
            deleteCommand.Parameters.AddWithValue("@id", id);
            deleteCommand.ExecuteNonQuery();

            alert.Tipo = "success";
            alert.Mensaje = "El edificio ha sido eliminado correctamente.";
          }
        }
      }
      TempData["Alert"] = JsonSerializer.Serialize(alert);
      return RedirectToAction("Gerencia");
    }

    private bool ExisteEnTablaGerencia(string tabla, Guid id, SqlConnection connection)
    {
      SqlCommand command = new SqlCommand($"SELECT COUNT(*) FROM {tabla} WHERE companySysId = @id", connection);
      command.Parameters.AddWithValue("@id", id);
      int count = (int)command.ExecuteScalar();
      return count > 0;
    }

    public ActionResult EliminarBatchGerencia(string ids)
    {
      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;
      AlertMessage alert = new AlertMessage();

      // Convertir la lista de ids desde el string recibido
      var idsList = ids.Split(',').Select(id => Guid.Parse(id)).ToList();

      // Verificar si alguno de los IDs es vacío
      if (idsList.Any(id => id == Guid.Empty))
      {
        alert.Tipo = "error";
        alert.Mensaje = "El edificio: 'Sin Asignar' no se puede eliminar.";
        TempData["Alert"] = JsonSerializer.Serialize(alert);
        return RedirectToAction("Gerencia");
      }

      using (SqlConnection connection = new SqlConnection(connectionString))
      {
        connection.Open();

        foreach (var id in idsList)
        {
          // Obtener el nombre del edificio desde la tabla 'companies' usando el id
          string nombreEdificio = ObtenerNombreGerencia(id, connection);
          if (string.IsNullOrEmpty(nombreEdificio))
          {
            alert.Tipo = "error";
            alert.Mensaje = $"El edificio con id {id} no existe.";
            TempData["Alert"] = JsonSerializer.Serialize(alert);
            return RedirectToAction("Edificios");
          }

          // Verificaciones por cada id
          if (ExisteEnTablaGerencia("floors", id, connection))
          {
            alert.Tipo = "error";
            alert.Mensaje = $"El edificio <strong>{nombreEdificio}</strong> no se puede eliminar porque está asociado a pisos.";
            TempData["Alert"] = JsonSerializer.Serialize(alert);
            return RedirectToAction("Gerencia");
          }
          else if (ExisteEnTablaGerencia("departments", id, connection))
          {
            alert.Tipo = "error";
            alert.Mensaje = $"El edificio <strong>{nombreEdificio}</strong> no se puede eliminar porque está asociado a sectores.";
            TempData["Alert"] = JsonSerializer.Serialize(alert);
            return RedirectToAction("Gerencia");
          }
          else if (ExisteEnTablaGerencia("assets", id, connection))
          {
            alert.Tipo = "error";
            alert.Mensaje = $"El edificio <strong>{nombreEdificio}</strong> no se puede eliminar porque está asociado a activos.";
            TempData["Alert"] = JsonSerializer.Serialize(alert);
            return RedirectToAction("Gerencia");
          }
          else if (ExisteEnTablaGerencia("buildings", id, connection))
          {
            alert.Tipo = "error";
            alert.Mensaje = $"El edificio <strong>{nombreEdificio}</strong> no se puede eliminar porque está asociado a pisos.";
            TempData["Alert"] = JsonSerializer.Serialize(alert);
            return RedirectToAction("Gerencia");
          }
          else if (ExisteEnTablaGerencia("officeses", id, connection))
          {
            alert.Tipo = "error";
            alert.Mensaje = $"El edificio <strong>{nombreEdificio}</strong> no se puede eliminar porque está asociado a oficinas.";
            TempData["Alert"] = JsonSerializer.Serialize(alert);
            return RedirectToAction("Gerencia");
          }
          else
          {
            // Eliminar el registro de la tabla 'companies' para cada id
            SqlCommand deleteCommand = new SqlCommand("DELETE FROM companies WHERE companySysId = @id", connection);
            deleteCommand.Parameters.AddWithValue("@id", id);
            deleteCommand.ExecuteNonQuery();
          }
        }

        alert.Tipo = "success";
        alert.Mensaje = "Los edificios seleccionados han sido eliminados correctamente.";
      }

      TempData["Alert"] = JsonSerializer.Serialize(alert);
      return RedirectToAction("Gerencia");
    }

    // Función para obtener el nombre del edificio a partir del id
    private string ObtenerNombreGerencia(Guid id, SqlConnection connection)
    {
      SqlCommand command = new SqlCommand("SELECT name FROM companies WHERE companySysId = @id", connection);
      command.Parameters.AddWithValue("@id", id);
      var result = command.ExecuteScalar();
      return result != null ? result.ToString() : null;

    }

    [HttpPost]
    public IActionResult InsertarGerencia(Edificios model)
    {
      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;
      AlertMessage alertMessage;

      using (SqlConnection connection = new SqlConnection(connectionString))
      {
        // Verificar si ya existe un edificio con el mismo nombre y descripción
        string checkQuery = @"SELECT COUNT(*) FROM companies WHERE name = @name";

        using (SqlCommand checkCmd = new SqlCommand(checkQuery, connection))
        {
          checkCmd.Parameters.Add("@name", SqlDbType.VarChar, 50).Value = model.name;
          checkCmd.Parameters.Add("@description", SqlDbType.VarChar, 150).Value = model.description;

          connection.Open();
          int count = (int)checkCmd.ExecuteScalar(); // Devuelve la cantidad de registros que coinciden
          connection.Close();

          if (count > 0)
          {
            // Si ya existe un registro con el mismo name y description, no permitir la inserción
            alertMessage = new AlertMessage
            {
              Tipo = "error",
              Mensaje = "Ya existe un edificio con el mismo nombre en el sistema."
            };

            TempData["Alert"] = JsonSerializer.Serialize(alertMessage);
            return RedirectToAction("Gerencia");
          }
        }

        // Si el name ya existe pero con una descripción diferente, se puede insertar
        string insertQuery = @"INSERT INTO companies 
                                (companySysId, name, description, entryUser, entryDate, updateUser, updateDate, rowGuid) 
                                VALUES 
                                (@companySysId, @name, @description, @entryUser, @entryDate, @updateUser, @updateDate, @rowGuid)";

        using (SqlCommand cmd = new SqlCommand(insertQuery, connection))
        {
          cmd.Parameters.Add("@companySysId", SqlDbType.UniqueIdentifier).Value = Guid.NewGuid();
          cmd.Parameters.Add("@name", SqlDbType.VarChar, 50).Value = model.name;
          cmd.Parameters.Add("@description", SqlDbType.VarChar, 150).Value = model.description;
          cmd.Parameters.Add("@entryUser", SqlDbType.UniqueIdentifier).Value = Guid.NewGuid();
          cmd.Parameters.Add("@entryDate", SqlDbType.DateTime).Value = DateTime.Now;
          cmd.Parameters.Add("@updateUser", SqlDbType.UniqueIdentifier).Value = Guid.NewGuid();
          cmd.Parameters.Add("@updateDate", SqlDbType.DateTime).Value = DateTime.Now;
          cmd.Parameters.Add("@rowGuid", SqlDbType.UniqueIdentifier).Value = Guid.NewGuid();

          try
          {
            connection.Open();
            cmd.ExecuteNonQuery();

            alertMessage = new AlertMessage
            {
              Tipo = "success",
              Mensaje = "El edificio se ha insertado correctamente."
            };
          }
          catch (Exception ex)
          {
            alertMessage = new AlertMessage
            {
              Tipo = "error",
              Mensaje = "Error al insertar el edificio: " + ex.Message
            };
          }
        }
      }

      TempData["Alert"] = JsonSerializer.Serialize(alertMessage);
      return RedirectToAction("Gerencia");
    }

    [HttpPost]
    public IActionResult EditarGerencia(Edificios model)
    {
      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;
      AlertMessage alertMessage;

      // Verificar si el companySysId es el restringido
      if (model.companySysId == Guid.Empty)
      {
        alertMessage = new AlertMessage
        {
          Tipo = "error",
          Mensaje = "Este edificio no se puede editar."
        };
        TempData["Alert"] = JsonSerializer.Serialize(alertMessage);
        return RedirectToAction("Edificios");
      }

      using (SqlConnection connection = new SqlConnection(connectionString))
      {
        // Verificar si ya existe otro edificio con el mismo nombre y descripción (excluyendo el actual)
        string checkQuery = @"SELECT COUNT(*) FROM companies 
                              WHERE name = @name
                              AND companySysId != @companySysId";

        using (SqlCommand checkCmd = new SqlCommand(checkQuery, connection))
        {
          checkCmd.Parameters.Add("@name", SqlDbType.VarChar, 50).Value = model.name;
          checkCmd.Parameters.Add("@companySysId", SqlDbType.UniqueIdentifier).Value = model.companySysId;

          connection.Open();
          int count = (int)checkCmd.ExecuteScalar();
          connection.Close();

          if (count > 0)
          {
            alertMessage = new AlertMessage
            {
              Tipo = "error",
              Mensaje = "Ya existe otro edificio con el mismo nombre en el sistema."
            };
            TempData["Alert"] = JsonSerializer.Serialize(alertMessage);
            return RedirectToAction("Gerencia");
          }
        }

        // Proceder con la actualización
        string updateQuery = @"UPDATE companies 
                               SET name = @name, 
                                   description = @description, 
                                   updateUser = @updateUser, 
                                   updateDate = @updateDate 
                               WHERE companySysId = @companySysId";

        using (SqlCommand cmd = new SqlCommand(updateQuery, connection))
        {
          cmd.Parameters.Add("@name", SqlDbType.VarChar, 50).Value = model.name;
          cmd.Parameters.Add("@description", SqlDbType.VarChar, 150).Value = model.description;
          cmd.Parameters.Add("@updateUser", SqlDbType.UniqueIdentifier).Value = Guid.NewGuid();
          cmd.Parameters.Add("@updateDate", SqlDbType.DateTime).Value = DateTime.Now;
          cmd.Parameters.Add("@companySysId", SqlDbType.UniqueIdentifier).Value = model.companySysId;

          try
          {
            connection.Open();
            int rowsAffected = cmd.ExecuteNonQuery();
            connection.Close();

            if (rowsAffected > 0)
            {
              alertMessage = new AlertMessage
              {
                Tipo = "success",
                Mensaje = "El edificio se ha actualizado correctamente."
              };
            }
            else
            {
              alertMessage = new AlertMessage
              {
                Tipo = "warning",
                Mensaje = "No se encontró el edificio para actualizar."
              };
            }
          }
          catch (Exception ex)
          {
            alertMessage = new AlertMessage
            {
              Tipo = "error",
              Mensaje = "Error al actualizar el edificio: " + ex.Message
            };
          }
        }
      }

      TempData["Alert"] = JsonSerializer.Serialize(alertMessage);
      return RedirectToAction("Gerencia");
    }


    [HttpPost]
    public async Task<IActionResult> SincronizarGerencia(IFormFile excelFile)
    {
      if (excelFile == null || excelFile.Length == 0)
      {
        TempData["Alert2"] = "Por favor seleccione un archivo válido.";
        return RedirectToAction("Gerencia");
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
          var sheet = package.Workbook.Worksheets.FirstOrDefault(s => s.Name.ToLower().Contains("gerencia"));

          if (sheet == null)
          {
            TempData["Alert2"] = "No se encontró la hoja de gerencia en el archivo.";
            return RedirectToAction("Gerencia");
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
              await SaveDataToDatabaseAsyncGerencia(rowData);
          }

          TempData["Alert2"] = "Archivo procesado exitosamente.";
        }
      }
      catch (Exception ex)
      {
        TempData["Alert2"] = "Error al procesar el archivo: " + ex.Message;
      }

      return RedirectToAction("Gerencia");
    }

    private async Task SaveDataToDatabaseAsyncGerencia(List<string> rowData)
    {
      // Lee la cadena de conexión desde el archivo app.config, esta cadena se puede modificar.
      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

      using (var connection = new SqlConnection(connectionString))
      using (var command = new SqlCommand("InsertCompanyExcel", connection))
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

    public int ObtenerCantidadGerencia()
    {
      int total = 0;
      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

      using (SqlConnection connection = new SqlConnection(connectionString))
      {
        string query = "SELECT COUNT(IdLogicoA) FROM LogicoA";

        using (SqlCommand command = new SqlCommand(query, connection))
        {
          connection.Open();
          total = (int)command.ExecuteScalar();
        }
      }

      return total;
    }

    #endregion Fin Gerencia
  }
}
