using AspnetCoreMvcFull.Mailer;
using AspnetCoreMvcFull.Models.Common;
using AspnetCoreMvcFull.Models.Contro_de_Trafico;
using AspnetCoreMvcFull.Models.Contro_de_Trafico;
using AspnetCoreMvcFull.Models.GestionServicios;
using AspnetCoreMvcFull.Models.Mensajes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlClient;
using System.Drawing.Printing;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AspnetCoreMvcFull.Controllers
{
  public class GestionServicioController : Controller
  {
    public IActionResult Index()
    {
      return View();
    }
    public IActionResult Proyectos()
    {
      return View();
    }
    public IActionResult Garantias()
    {
      return View();
    }
    public IActionResult Contratos()
    {
      return View();
    }
    public IActionResult TimeTracker()
    {
      return View();
    }
    [HttpGet]

    public IActionResult ControlTrafico(int page = 1, int pageSize = 10, string? q = null)
    {
      // Selects
      ViewBag.Solicitantes = ListarParaSelectSolicitante();
      ViewBag.Contratos = ListarParaSelectContrato();
      ViewBag.Empresas = ListarParaSelectEmpresa();
      ViewBag.Asignados = ListarParaSelectUsers(soloAprobados: true);
      ViewBag.RazonesServicio = ListarParaSelectRazon();

      var modeloTabla = ListarControlTraficoPaginado(page, pageSize, q);
      return View(modeloTabla);
    }

    [HttpGet]
    public IActionResult ObtenerEmpresasService()
    {
      var lista = new List<object>();

      // Ojo: si usas ASP.NET Core, asegúrate de tener la connstring en appsettings y enlazarla.
      string connectionString = System.Configuration.ConfigurationManager
          .ConnectionStrings["ServerDiverscan"]?.ConnectionString;

      if (string.IsNullOrWhiteSpace(connectionString))
        return Json(lista); // o devuelve StatusCode(500) si prefieres.

      using (var connection = new SqlConnection(connectionString))
      using (var command = new SqlCommand(
          "SELECT ID_EMPRESA, NOMBRE, DESCRIPCION FROM EMPRESA", connection))
      {
        connection.Open();
        using (var reader = command.ExecuteReader())
        {
          while (reader.Read())
          {
            // ID: si por alguna razón viniera NULL, lo dejamos como Guid.Empty (o puedes 'continue;' para omitir)
            Guid id = reader.IsDBNull(0) ? Guid.Empty : reader.GetGuid(0);

            string nombre = reader.IsDBNull(1) ? null : reader.GetString(1);
            string descripcion = reader.IsDBNull(2) ? null : reader.GetString(2);

            // Arma el texto sin guiones colgantes si falta alguno
            var partes = new List<string>();
            if (!string.IsNullOrWhiteSpace(nombre)) partes.Add(nombre.Trim());
            if (!string.IsNullOrWhiteSpace(descripcion)) partes.Add(descripcion.Trim());

            string texto = partes.Count > 0 ? string.Join(" - ", partes) : "(Sin datos)";

            lista.Add(new
            {
              id = id,          // el Guid se serializa bien a JSON
              texto = texto
            });
          }
        }
      }

      return Json(lista);
    }


    [HttpGet]
    public IActionResult ObtenerContratosServices(Guid idCompany)
    {
      var lista = new List<object>();
      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

      using (var connection = new SqlConnection(connectionString))
      {
        string query = "SELECT ID_CONTRATO, NOMBRE, NUMERO FROM CONTRATOS WHERE ID_EMPRESA = @companySysId";
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
    public IActionResult ContratosPorEmpresa(Guid empresaId)
    {
      var list = new List<object>();
      string cs = System.Configuration.ConfigurationManager
               .ConnectionStrings["ServerDiverscan"].ConnectionString;
      using var cn = new SqlConnection(cs);
      using var cmd = new SqlCommand(@"
        SELECT ID_CONTRATO, LTRIM(RTRIM(ISNULL(NUMERO,''))) AS Numero,
               LTRIM(RTRIM(ISNULL(NOMBRE,''))) AS Nombre
        FROM dbo.CONTRATOS
        WHERE ID_EMPRESA = @EmpresaId
        ORDER BY CASE WHEN NULLIF(Numero,'') IS NULL THEN 1 ELSE 0 END, Numero,
                 CASE WHEN NULLIF(Nombre,'') IS NULL THEN 1 ELSE 0 END, Nombre", cn);
      cmd.Parameters.Add("@EmpresaId", SqlDbType.UniqueIdentifier).Value = empresaId;
      cn.Open();
      using var rd = cmd.ExecuteReader();
      while (rd.Read())
      {
        var id = rd.GetGuid(0).ToString();
        var num = rd.IsDBNull(1) ? "" : rd.GetString(1);
        var nom = rd.IsDBNull(2) ? "" : rd.GetString(2);
        var text = !string.IsNullOrWhiteSpace(num) && !string.IsNullOrWhiteSpace(nom) ? $"{num} — {nom}"
                 : (!string.IsNullOrWhiteSpace(num) ? num : nom);
        list.Add(new { value = id, text });
      }
      return Json(list);
    }

    public static List<SelectListItem> ListarParaSelectContrato(Guid? empresaId = null)
    {
      string cs = System.Configuration.ConfigurationManager
                   .ConnectionStrings["ServerDiverscan"].ConnectionString;

      var items = new List<SelectListItem>();

      using var cn = new SqlConnection(cs);

      // ASUME que CONTRATOS tiene columna ID_EMPRESA.
      // Si en tu DB la relación está en otra tabla (p. ej. CONTRATO_EMPRESA),
      // cambia el WHERE por un JOIN a esa tabla.
      string sql = @"
      SELECT 
          c.ID_CONTRATO,
          LTRIM(RTRIM(ISNULL(c.NOMBRE, ''))) AS Nombre,
          LTRIM(RTRIM(ISNULL(c.NUMERO,  ''))) AS Numero
      FROM dbo.CONTRATOS c
      WHERE (@EmpresaId IS NULL OR c.ID_EMPRESA = @EmpresaId)
      ORDER BY 
          CASE WHEN NULLIF(Numero,'') IS NULL THEN 1 ELSE 0 END, Numero,
          CASE WHEN NULLIF(Nombre,'') IS NULL THEN 1 ELSE 0 END, Nombre;";

      using var cmd = new SqlCommand(sql, cn);
      cmd.Parameters.Add("@EmpresaId", SqlDbType.UniqueIdentifier).Value =
          (object?)empresaId ?? DBNull.Value;

      cn.Open();
      using var rd = cmd.ExecuteReader();
      while (rd.Read())
      {
        var id = rd.GetGuid(0);
        var nombre = rd.IsDBNull(1) ? "" : rd.GetString(1);
        var numero = rd.IsDBNull(2) ? "" : rd.GetString(2);

        string text;
        if (!string.IsNullOrWhiteSpace(numero) && !string.IsNullOrWhiteSpace(nombre))
          text = $"{numero} — {nombre}";
        else if (!string.IsNullOrWhiteSpace(numero))
          text = numero;
        else if (!string.IsNullOrWhiteSpace(nombre))
          text = nombre;
        else
          text = id.ToString();

        items.Add(new SelectListItem { Value = id.ToString(), Text = text });
      }

      return items;
    }



    [HttpGet]
    public IActionResult ObtenerControlTrafico(int ticket)
    {
      string cs = System.Configuration.ConfigurationManager
          .ConnectionStrings["ServerDiverscan"].ConnectionString;

      const string sql = @"
SELECT TOP (1)
    ct.Ticket,
    ct.CanalEmail, ct.CanalWeb, ct.CanalPresencial, ct.CanalTelefono, ct.CanalChatbot,
    ct.SolicitanteId, ct.ContratoId, ct.EmpresaId,
    c.ID_EMPRESA AS EmpresaIdContrato,
    c.NUMERO     AS ContratoNumero,
    c.NOMBRE     AS ContratoNombre,
    ct.AsignadoAId, ct.RazonServicioId,
    ct.TelefonoServicio, ct.EmailServicio, ct.DireccionServicio,
    ct.LugarServicio, ct.FechaProximoServicio, ct.HoraServicio, ct.DescripcionIncidente,  
    ct.HoraServicio, ct.DescripcionIncidente,
    ct.GD, ct.SC, ct.SID
    FROM dbo.CONTROL_TRAFICO ct
    LEFT JOIN dbo.CONTRATOS c ON c.ID_CONTRATO = ct.ContratoId
    WHERE ct.Ticket = @Ticket;";
      using var cn = new SqlConnection(cs);
      using var cmd = new SqlCommand(sql, cn);
      cmd.Parameters.Add("@Ticket", SqlDbType.Int).Value = ticket;

      cn.Open();
      using var rd = cmd.ExecuteReader();
      if (!rd.Read())
        return NotFound(new { ok = false, message = "No existe el ticket" });

      string? fecha = rd.IsDBNull(rd.GetOrdinal("FechaProximoServicio"))
          ? null
          : rd.GetDateTime(rd.GetOrdinal("FechaProximoServicio")).ToString("yyyy-MM-dd");

      string? hora = rd.IsDBNull(rd.GetOrdinal("HoraServicio"))
          ? null
          : ((TimeSpan)rd["HoraServicio"]).ToString(@"hh\:mm");

      // preferimos la empresa ligada al contrato (si existe)
      string? empresaId = rd.IsDBNull(rd.GetOrdinal("EmpresaId")) ? null : rd.GetGuid(rd.GetOrdinal("EmpresaId")).ToString();
      string? empresaIdContrato = rd.IsDBNull(rd.GetOrdinal("EmpresaIdContrato")) ? null : rd.GetGuid(rd.GetOrdinal("EmpresaIdContrato")).ToString();
      string? empresaPreferida = !string.IsNullOrWhiteSpace(empresaIdContrato) ? empresaIdContrato : empresaId;

      bool? getBitNullable(SqlDataReader r, string col)
  => r.IsDBNull(r.GetOrdinal(col)) ? (bool?)null : r.GetBoolean(r.GetOrdinal(col));

      string? numero = rd.IsDBNull(rd.GetOrdinal("ContratoNumero")) ? null : rd.GetString(rd.GetOrdinal("ContratoNumero"));
      string? nombre = rd.IsDBNull(rd.GetOrdinal("ContratoNombre")) ? null : rd.GetString(rd.GetOrdinal("ContratoNombre"));
      string? contratoLabel = null;
      if (!string.IsNullOrWhiteSpace(numero) && !string.IsNullOrWhiteSpace(nombre))
        contratoLabel = $"{numero} — {nombre}";
      else
        contratoLabel = numero ?? nombre;

      var data = new
      {
        ok = true,
        ticket = rd.GetInt32(rd.GetOrdinal("Ticket")),

        canalEmail = rd.GetBoolean(rd.GetOrdinal("CanalEmail")),
        canalWeb = rd.GetBoolean(rd.GetOrdinal("CanalWeb")),
        canalPresencial = rd.GetBoolean(rd.GetOrdinal("CanalPresencial")),
        canalTelefono = rd.GetBoolean(rd.GetOrdinal("CanalTelefono")),
        canalChatbot = rd.GetBoolean(rd.GetOrdinal("CanalChatbot")),

        solicitanteId = rd.IsDBNull(rd.GetOrdinal("SolicitanteId")) ? null : rd.GetGuid(rd.GetOrdinal("SolicitanteId")).ToString(),
        contratoId = rd.IsDBNull(rd.GetOrdinal("ContratoId")) ? null : rd.GetGuid(rd.GetOrdinal("ContratoId")).ToString(),

        // devolvemos ambos por si quieres mostrarlos, pero usamos empresaPreferida en el front
        empresaId = empresaPreferida,
        empresaIdRaw = empresaId,
        contratoLabel = contratoLabel,
        empresaIdContrato = empresaIdContrato,

        asignadoAId = rd.IsDBNull(rd.GetOrdinal("AsignadoAId")) ? null : rd.GetGuid(rd.GetOrdinal("AsignadoAId")).ToString(),
        razonServicioId = rd.IsDBNull(rd.GetOrdinal("RazonServicioId")) ? null : rd.GetGuid(rd.GetOrdinal("RazonServicioId")).ToString(),

        telefonoServicio = rd["TelefonoServicio"] as string,
        emailServicio = rd["EmailServicio"] as string,
        direccionServicio = rd["DireccionServicio"] as string,
        descripcionIncidente = rd["DescripcionIncidente"] as string,

        lugarServicio = rd.IsDBNull(rd.GetOrdinal("LugarServicio")) ? (byte)0 : Convert.ToByte(rd["LugarServicio"]),

        fechaProximoServicio = fecha,
        horaServicio = hora,
        gd = getBitNullable(rd, "GD"),
        sc = getBitNullable(rd, "SC"),
        sid = getBitNullable(rd, "SID"),
      };

      return Ok(data);
    }


    private PagedResult<ControlTraficoPostDto> ListarControlTraficoPaginado(int page, int pageSize, string? q)
    {
      if (page < 1) page = 1;
      if (pageSize < 1) pageSize = 10;
      int skip = (page - 1) * pageSize;

      var result = new PagedResult<ControlTraficoPostDto>
      {
        CurrentPage = page,
        PageSize = pageSize,
        Query = q
      };

      string cs = System.Configuration.ConfigurationManager
          .ConnectionStrings["ServerDiverscan"].ConnectionString;

      // Normalizamos términos
      string qTrim = (q ?? "").Trim();
      bool hasQ = !string.IsNullOrWhiteSpace(qTrim);
      int ticketNum;
      bool hasTicket = int.TryParse(qTrim, out ticketNum);
      string qLike = $"%{qTrim}%";

      // WHERE común para COUNT y PAGE
      const string whereBlock = @"
WHERE
    (@hasQ = 0)
    OR (
         ct.DescripcionIncidente LIKE @qLike
      OR c.NUMERO               LIKE @qLike
      OR rs.nombre              LIKE @qLike
      OR u.username             LIKE @qLike
      OR (@hasTicket = 1 AND ct.Ticket = @ticketNum)
    )";

      string sqlCount = $@"
SELECT COUNT(*)
FROM dbo.CONTROL_TRAFICO ct
LEFT JOIN dbo.CONTRATOS       c  ON c.ID_CONTRATO        = ct.ContratoId
LEFT JOIN dbo.RazonServicios  rs ON rs.id_razonServicios = ct.RazonServicioId
LEFT JOIN dbo.[users]         u  ON u.userSysId          = ct.AsignadoAId
{whereBlock};";

      string sqlPage = $@"
SELECT
    ct.ControlTraficoId,
    ISNULL(c.NUMERO,'')   AS ContratoNumero,
    ISNULL(rs.nombre,'')  AS RazonNombre,
    ISNULL(u.username,'') AS AsignadoUsername,
    ct.FechaCreacionUtc,
    ct.FechaProximoServicio,
    ct.HoraServicio,
    ISNULL(ct.Ticket,0)   AS Ticket,
    CASE 
        WHEN ISNULL(ct.GD,0)  = 1 THEN 'GD'
        WHEN ISNULL(ct.SC,0)  = 1 THEN 'SC'
        WHEN ISNULL(ct.SID,0) = 1 THEN 'S-ID'
        ELSE ''
    END AS FlagElegido
FROM dbo.CONTROL_TRAFICO ct
LEFT JOIN dbo.CONTRATOS       c  ON c.ID_CONTRATO        = ct.ContratoId
LEFT JOIN dbo.RazonServicios  rs ON rs.id_razonServicios = ct.RazonServicioId
LEFT JOIN dbo.[users]         u  ON u.userSysId          = ct.AsignadoAId
{whereBlock}
ORDER BY ct.FechaCreacionUtc DESC
OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;";


      using (var cn = new SqlConnection(cs))
      {
        cn.Open();

        // COUNT
        using (var cmd = new SqlCommand(sqlCount, cn))
        {
          cmd.Parameters.Add("@hasQ", SqlDbType.Bit).Value = hasQ ? 1 : 0;
          cmd.Parameters.Add("@qLike", SqlDbType.NVarChar, 1000).Value = hasQ ? (object)qLike : DBNull.Value;
          cmd.Parameters.Add("@hasTicket", SqlDbType.Bit).Value = hasTicket ? 1 : 0;
          cmd.Parameters.Add("@ticketNum", SqlDbType.Int).Value = hasTicket ? ticketNum : (object)DBNull.Value;

          result.TotalItems = (int)cmd.ExecuteScalar();
        }

        // PAGE
        var items = new List<ControlTraficoPostDto>();
        using (var cmd = new SqlCommand(sqlPage, cn))
        {
          cmd.Parameters.Add("@hasQ", SqlDbType.Bit).Value = hasQ ? 1 : 0;
          cmd.Parameters.Add("@qLike", SqlDbType.NVarChar, 1000).Value = hasQ ? (object)qLike : DBNull.Value;
          cmd.Parameters.Add("@hasTicket", SqlDbType.Bit).Value = hasTicket ? 1 : 0;
          cmd.Parameters.Add("@ticketNum", SqlDbType.Int).Value = hasTicket ? ticketNum : (object)DBNull.Value;

          cmd.Parameters.Add("@Skip", SqlDbType.Int).Value = skip;
          cmd.Parameters.Add("@Take", SqlDbType.Int).Value = pageSize;

          using var rd = cmd.ExecuteReader();
          while (rd.Read())
          {
            string? fechaProx = rd.IsDBNull(rd.GetOrdinal("FechaProximoServicio"))
                ? null
                : ((DateTime)rd["FechaProximoServicio"]).ToString("yyyy-MM-dd");

            string? fechaCreacion = rd.IsDBNull(rd.GetOrdinal("FechaCreacionUtc"))
                ? null
                : ((DateTime)rd["FechaCreacionUtc"]).ToString("yyyy-MM-dd");

            string? horaStr = rd.IsDBNull(rd.GetOrdinal("HoraServicio"))
                ? null
                : ((TimeSpan)rd["HoraServicio"]).ToString(@"hh\:mm");

            items.Add(new ControlTraficoPostDto
            {
              ContratoId = rd["ContratoNumero"]?.ToString() ?? "",
              RazonServicioId = rd["RazonNombre"]?.ToString() ?? "",
              AsignadoAId = rd["AsignadoUsername"]?.ToString() ?? "",
              FechaProximoServicio = fechaProx,
              HoraServicio = horaStr,
              Fecha = fechaCreacion,
              ticket = rd.GetInt32(rd.GetOrdinal("Ticket")),
              Proveedor = rd["FlagElegido"]?.ToString() ?? ""
            });
          }
        }

        result.Items = items;
      }

      return result;
    }


    [HttpPost]
    public IActionResult EliminarControlTrafico([FromForm] int ticket)
    {
      if (ticket <= 0)
      {
        TempData["Alert"] = System.Text.Json.JsonSerializer.Serialize(new AlertMessage
        {
          Tipo = "warning",
          Mensaje = "Ticket inválido."
        });
        return RedirectToAction("ControlTrafico");
      }

      string cs = System.Configuration.ConfigurationManager
          .ConnectionStrings["ServerDiverscan"].ConnectionString;

      const string sql = @"DELETE FROM dbo.CONTROL_TRAFICO WHERE Ticket = @Ticket;";

      int affected = 0;
      using (var cn = new SqlConnection(cs))
      using (var cmd = new SqlCommand(sql, cn))
      {
        cmd.Parameters.Add("@Ticket", SqlDbType.Int).Value = ticket;
        cn.Open();
        affected = cmd.ExecuteNonQuery();
      }

      TempData["Alert"] = System.Text.Json.JsonSerializer.Serialize(new AlertMessage
      {
        Tipo = (affected > 0 ? "success" : "warning"),
        Mensaje = (affected > 0 ? $"Ticket #{ticket} eliminado." : $"No se encontró el ticket #{ticket}.")
      });

      return RedirectToAction("ControlTrafico");
    }

    private readonly SmtpSettings _smtp;
    private readonly string _cs;

    public GestionServicioController(IConfiguration cfg)
    {
      _cs = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString
              ?? cfg.GetConnectionString("ServerDiverscan");
      _smtp = cfg.GetSection("Smtp").Get<SmtpSettings>() ?? new SmtpSettings();
    }

    [HttpPost]
    public async Task<IActionResult> InsertarControTrafico([FromForm] ControlTraficoPostDto dto)
    {
      bool esEditar = string.Equals(dto.EstadoFormulario, "Editar", StringComparison.OrdinalIgnoreCase);

      // ==== 1) Validaciones / parse comunes ====
      if (!Guid.TryParse(dto.SolicitanteId, out var solicitanteId))
        return BadRequest("Solicitante es requerido.");

      if (!Guid.TryParse(dto.EmpresaId, out var empresaId))
        return BadRequest("Empresa es requerida.");

      if (!Guid.TryParse(dto.RazonServicioId, out var razonId))
        return BadRequest("Razón de servicios es requerida.");

      Guid? contratoId = null;
      if (!string.IsNullOrWhiteSpace(dto.ContratoId) && Guid.TryParse(dto.ContratoId, out var ctmp))
        contratoId = ctmp;

      Guid? asignadoId = null;
      if (!string.IsNullOrWhiteSpace(dto.AsignadoAId) && Guid.TryParse(dto.AsignadoAId, out var tmp))
        asignadoId = tmp;

      byte lugar = (dto.LugarServicio == "1") ? (byte)1 : (byte)0;

      DateTime? fechaProx = TryParseDate(dto.FechaProximoServicio);
      TimeSpan? horaServ = TryParseTime(dto.HoraServicio);

      const string SQL_INSERT = @"
INSERT INTO dbo.CONTROL_TRAFICO
(
  CanalEmail, CanalWeb, CanalPresencial, CanalTelefono, CanalChatbot,
  SolicitanteId, ContratoId, EmpresaId, AsignadoAId, RazonServicioId,
  TelefonoServicio, EmailServicio, DireccionServicio,
  LugarServicio, FechaProximoServicio, HoraServicio, DescripcionIncidente,
  GD, SC, SID,             
  UsuarioCreacionId
)
OUTPUT INSERTED.ControlTraficoId, INSERTED.Ticket
VALUES
(
  @CanalEmail, @CanalWeb, @CanalPresencial, @CanalTelefono, @CanalChatbot,
  @SolicitanteId, @ContratoId, @EmpresaId, @AsignadoAId, @RazonServicioId,
  @TelefonoServicio, @EmailServicio, @DireccionServicio,
  @LugarServicio, @FechaProximoServicio, @HoraServicio, @DescripcionIncidente,
  @GD, @SC, @SID,            
  @UsuarioCreacionId
);";

      const string SQL_UPDATE = @"
UPDATE dbo.CONTROL_TRAFICO
SET
  CanalEmail = @CanalEmail,
  CanalWeb = @CanalWeb,
  CanalPresencial = @CanalPresencial,
  CanalTelefono = @CanalTelefono,
  CanalChatbot = @CanalChatbot,
  SolicitanteId = @SolicitanteId,
  ContratoId = @ContratoId,
  EmpresaId = @EmpresaId,
  AsignadoAId = @AsignadoAId,
  RazonServicioId = @RazonServicioId,
  TelefonoServicio = @TelefonoServicio,
  EmailServicio = @EmailServicio,
  DireccionServicio = @DireccionServicio,
  LugarServicio = @LugarServicio,
  FechaProximoServicio = @FechaProximoServicio,
  HoraServicio = @HoraServicio,
  DescripcionIncidente = @DescripcionIncidente,
  GD = @GD, SC = @SC, SID = @SID     
WHERE Ticket = @Ticket;";


      // Valores que se insertan/actualizan (también van al correo)
      var emailSrv = (dto.EmailServicio ?? "").Trim();
      var telSrv = (dto.TelefonoServicio ?? "").Trim();
      var brief = dto.DescripcionIncidente ?? "";

      using var cn = new SqlConnection(_cs);
      using var cmd = new SqlCommand(esEditar ? SQL_UPDATE : SQL_INSERT, cn);

      // ==== 2) Parámetros comunes ====
      cmd.Parameters.Add("@CanalEmail", SqlDbType.Bit).Value = dto.CanalEmail;
      cmd.Parameters.Add("@CanalWeb", SqlDbType.Bit).Value = dto.CanalWeb;
      cmd.Parameters.Add("@CanalPresencial", SqlDbType.Bit).Value = dto.CanalPresencial;
      cmd.Parameters.Add("@CanalTelefono", SqlDbType.Bit).Value = dto.CanalTelefono;
      cmd.Parameters.Add("@CanalChatbot", SqlDbType.Bit).Value = dto.CanalChatbot;
      cmd.Parameters.Add("@GD", SqlDbType.Bit).Value = dto.GD;
      cmd.Parameters.Add("@SC", SqlDbType.Bit).Value = dto.SC;
      cmd.Parameters.Add("@SID", SqlDbType.Bit).Value = dto.SID;


      cmd.Parameters.Add("@SolicitanteId", SqlDbType.UniqueIdentifier).Value = solicitanteId;
      cmd.Parameters.Add("@ContratoId", SqlDbType.UniqueIdentifier).Value = (object?)contratoId ?? DBNull.Value;
      cmd.Parameters.Add("@EmpresaId", SqlDbType.UniqueIdentifier).Value = empresaId;
      cmd.Parameters.Add("@AsignadoAId", SqlDbType.UniqueIdentifier).Value = (object?)asignadoId ?? DBNull.Value;
      cmd.Parameters.Add("@RazonServicioId", SqlDbType.UniqueIdentifier).Value = razonId;

      cmd.Parameters.Add("@TelefonoServicio", SqlDbType.VarChar, 20).Value = telSrv;
      cmd.Parameters.Add("@EmailServicio", SqlDbType.VarChar, 254).Value = emailSrv;
      cmd.Parameters.Add("@DireccionServicio", SqlDbType.NVarChar, 400).Value = (object?)dto.DireccionServicio ?? DBNull.Value;

      cmd.Parameters.Add("@LugarServicio", SqlDbType.TinyInt).Value = lugar;
      cmd.Parameters.Add("@FechaProximoServicio", SqlDbType.DateTime).Value = (object?)fechaProx ?? DBNull.Value;
      cmd.Parameters.Add("@HoraServicio", SqlDbType.Time).Value = (object?)horaServ ?? DBNull.Value;
      cmd.Parameters.Add("@DescripcionIncidente", SqlDbType.NVarChar, 1000).Value = (object?)brief ?? DBNull.Value;

      cn.Open();

      if (esEditar)
      {
        // === 3A) Snapshot ANTES del UPDATE ===
        var before = GetSnapshotByTicket(_cs, dto.ticket);
        if (before == null)
        {
          TempData["Alert"] = System.Text.Json.JsonSerializer.Serialize(new AlertMessage
          {
            Tipo = "warning",
            Mensaje = $"No se encontró el ticket #{dto.ticket}."
          });
          return RedirectToAction("ControlTrafico");
        }

        cmd.Parameters.Add("@Ticket", SqlDbType.Int).Value = dto.ticket;
        int rows = cmd.ExecuteNonQuery();

        TempData["Alert"] = System.Text.Json.JsonSerializer.Serialize(new AlertMessage
        {
          Tipo = (rows > 0 ? "success" : "warning"),
          Mensaje = (rows > 0 ? $"Ticket #{dto.ticket} actualizado." : $"No se encontró el ticket #{dto.ticket}.")
        });

        if (rows <= 0) return RedirectToAction("ControlTrafico");

        // === 3B) “Después” (desde el DTO actual) ===
        var after_asignadoId = asignadoId;
        var after_fechaProx = fechaProx;
        var after_horaServ = horaServ;
        var after_tel = telSrv;
        var after_emailServ = emailSrv;
        var after_brief = brief;
        var after_empresaId = empresaId;
        var after_contratoId = contratoId;

        // Nombres / valores “bonitos”
        var beforeEmpresaNombre = GetEmpresaNombre(_cs, before.EmpresaId);
        var afterEmpresaNombre = GetEmpresaNombre(_cs, after_empresaId);

        string? beforeContratoNum = before.ContratoId.HasValue ? GetContratoNumero(_cs, before.ContratoId.Value) : null;
        string? afterContratoNum = after_contratoId.HasValue ? GetContratoNumero(_cs, after_contratoId.Value) : null;

        string beforeFechaTxt = FormatFechaCR(before.FechaProximoServicio);
        string afterFechaTxt = FormatFechaCR(after_fechaProx);
        string beforeHoraTxt = FormatHoraCR(before.HoraServicio);
        string afterHoraTxt = FormatHoraCR(after_horaServ);

        var beforeAssigned = before.AsignadoAId.HasValue ? GetUserById(_cs, before.AsignadoAId.Value) : (null, null);
        var afterAssigned = after_asignadoId.HasValue ? GetUserById(_cs, after_asignadoId.Value) : (null, null);

        bool reasignado = (before.AsignadoAId != after_asignadoId);

        // === 3C) Resumen de cambios ===
        var (plainChanges, htmlChanges) = BuildChangesSummary(new[]
        {
            ("Agente asignado",  beforeAssigned.Item2,  afterAssigned.Item2),
            ("Empresa",          beforeEmpresaNombre,   afterEmpresaNombre),
            ("Contrato",         beforeContratoNum,     afterContratoNum),
            ("Email de contacto",before.EmailServicio,  after_emailServ),
            ("Teléfono",         before.TelefonoServicio, after_tel),
            ("Fecha agendada",   beforeFechaTxt,        afterFechaTxt),
            ("Hora agendada",    beforeHoraTxt,         afterHoraTxt),
            ("Descripción",      before.DescripcionIncidente, after_brief),
        });

        // === 3D) Correos en UNA conexión ===
        var jobs = new List<AspnetCoreMvcFull.Mailer.MailJob>();

        // 1) Solicitante
        var soli = GetSolicitanteById(_cs, before.SolicitanteId);
        if (!string.IsNullOrWhiteSpace(soli.Email))
        {
          var (subUser, txtUser, htmlUser) = BuildMailToUserEdit(
              nombreSolicitante: NullOrND(soli.Nombre),
              nombreAsignado: NullOrND(afterAssigned.Item2),
              ticket: dto.ticket,
              fechaProx: after_fechaProx,
              horaServ: after_horaServ,
              changesPlain: plainChanges,
              changesHtml: htmlChanges
          );
          jobs.Add(new AspnetCoreMvcFull.Mailer.MailJob
          {
            To = soli.Email!,
            Subject = subUser,
            PlainText = txtUser,
            Html = htmlUser
            //, Bcc = _smtp.User
          });
        }

        // 2) Asignado (después del update)  **CORREGIDO: enviar a afterAssigned.Item1**
        if (after_asignadoId.HasValue && !string.IsNullOrWhiteSpace(afterAssigned.Item1))
        {
          var (subAss, txtAss, htmlAss) = BuildMailToAssignedEdit(
              assignedUserName: NullOrND(afterAssigned.Item2),
              ticket: dto.ticket,
              changesPlain: plainChanges,
              changesHtml: htmlChanges,
              esReasignacion: reasignado
          );
          jobs.Add(new AspnetCoreMvcFull.Mailer.MailJob
          {
            To = afterAssigned.Item1!,
            Subject = subAss,
            PlainText = txtAss,
            Html = htmlAss
            //, Bcc = _smtp.User
          });
        }
        try
        {
          var (logPath, results) = await AspnetCoreMvcFull.Mailer.Mailer.SendBatchAsync(_smtp, jobs);

          var fallidos = results.Where(r => !r.AcceptedBySmtp).ToList();
          if (fallidos.Count > 0)
          {
            // Hubo correos que el servidor NO aceptó (no 250 OK). Muestra a quiénes
            TempData["MailError"] = $"SMTP rechazó {fallidos.Count} correo(s). Log: {logPath}. " +
              string.Join(" | ", fallidos.Select(f => $"{f.Job.To}: {f.Error}"));
          }
          else
          {
            // El servidor aceptó todos (si alguno no llega, el problema es post-SMTP, ya dentro del MTA)
            TempData["MailInfo"] = $"Servidor SMTP aceptó todos los correos. Log: {logPath}";
          }
        }
        catch (Exception ex)
        {
          // Fallo global de conexión/auth/etc. (ni siquiera se pudo completar el lote)
          TempData["MailError"] = $"Fallo global de SMTP: {ex.Message}";
        }


        if (jobs.Count > 0)
        {
          try { await AspnetCoreMvcFull.Mailer.Mailer.SendBatchAsync(_smtp, jobs); }
          catch (Exception ex)
          {
            TempData["AlertCorreoEdit"] = System.Text.Json.JsonSerializer.Serialize(new AlertMessage
            {
              Tipo = "warning",
              Mensaje = "No se pudieron enviar algunas notificaciones: " + ex.Message
            });
          }
        }

        return RedirectToAction("ControlTrafico");
      }
      else
      {
        // ==== 3B) INSERT ====
        cmd.Parameters.Add("@UsuarioCreacionId", SqlDbType.UniqueIdentifier).Value = DBNull.Value;

        Guid nuevoId = Guid.Empty; int ticket = 0;
        using (var r = cmd.ExecuteReader())
          if (r.Read()) { nuevoId = r.GetGuid(0); ticket = r.GetInt32(1); }

        TempData["Alert"] = System.Text.Json.JsonSerializer.Serialize(new AlertMessage
        {
          Tipo = "success",
          Mensaje = $"Registro creado. Ticket #{ticket}."
        });

        // ==== 4) CORREOS (post-insert) en una sola conexión ====
        var jobs = new List<AspnetCoreMvcFull.Mailer.MailJob>();

        var soli = GetSolicitanteById(_cs, solicitanteId);
        var empresaNombre = GetEmpresaNombre(_cs, empresaId);
        var contratoNumero = contratoId.HasValue ? GetContratoNumero(_cs, contratoId.Value) : null;

        string showEmail = NullOrND(emailSrv);
        string showPhone = NullOrND(telSrv);
        string showCompany = NullOrND(empresaNombre);
        string showContract = NullOrND(contratoNumero);
        string showBrief = NullOrND(brief);

        // 4.2) Solicitante
        if (!string.IsNullOrWhiteSpace(soli.Email))
        {
          var assigned = asignadoId.HasValue ? GetUserById(_cs, asignadoId.Value) : (Email: "", UserName: "Sin asignar");
          var (subUser, txtUser, htmlUser) = BuildMailToUser(
              nombreSolicitante: NullOrND(soli.Nombre),
              nombreAsignado: NullOrND(assigned.UserName),
              ticket: ticket,
              fechaProx: fechaProx,
              horaServ: horaServ
          );
          jobs.Add(new AspnetCoreMvcFull.Mailer.MailJob
          {
            To = soli.Email!,
            Subject = subUser,
            PlainText = txtUser,
            Html = htmlUser
            //, Bcc = _smtp.User
          });
        }

        // 4.3) Asignado (si aplica)
        if (asignadoId.HasValue)
        {
          var assigned = GetUserById(_cs, asignadoId.Value);
          if (!string.IsNullOrWhiteSpace(assigned.Email))
          {
            var (subAss, txtAss, htmlAss) = BuildMailToAssigned(
                assignedUserName: NullOrND(assigned.UserName),
                nombreSolicitante: NullOrND(soli.Nombre),
                email: showEmail,
                phone: showPhone,
                company: showCompany,
                contractNumber: showContract,
                brief: showBrief,
                ticket: ticket
            );
            jobs.Add(new AspnetCoreMvcFull.Mailer.MailJob
            {
              To = assigned.Email!,
              Subject = subAss,
              PlainText = txtAss,
              Html = htmlAss
              //, Bcc = _smtp.User
            });
          }
        }

        if (jobs.Count > 0)
        {
          try { await AspnetCoreMvcFull.Mailer.Mailer.SendBatchAsync(_smtp, jobs); }
          catch (Exception ex)
          {
            TempData["AlertCorreoInsert"] = System.Text.Json.JsonSerializer.Serialize(new AlertMessage
            {
              Tipo = "warning",
              Mensaje = "No se pudieron enviar algunas notificaciones: " + ex.Message
            });
          }
        }

        return RedirectToAction("ControlTrafico");
      }
    }





    private static (string subject, string txt, string html) BuildMailToUser_EditSameLook(
    string nombreSolicitante,
    string nombreAsignado,
    int ticket,
    DateTime? fechaProx,
    TimeSpan? horaServ)
    {
      string fechaTxt = FormatFechaCR(fechaProx);
      string horaTxt = FormatHoraCR(horaServ);

      string subject = $"Ticket #{ticket} actualizado";

      // Texto plano (mismo estilo que insert)
      string plain =
  $@"Estimado: {nombreSolicitante}

Su incidente, No. {ticket}, ha sido actualizado y se encuentra asignado al Agente de Servicio “{nombreAsignado}”, quien se estará poniendo en contacto con usted para la respectiva evaluación.

Por favor, mantener a mano la información que será requerida como números de serie, modelo, marca, registros del problema y otras evidencias que nos permitan efectuar el diagnóstico respectivo y en el tiempo asignado para tal fin.

Le recordamos que todo servicio que no se encuentre bajo garantía o contrato, podrá requerir la aprobación de la cotización de diagnóstico previo a la ejecución del respectivo servicio.

Su Servicio se encuentra agendado para:

{fechaTxt}
{horaTxt}

Atención: Cuando una evaluación de la situación no se pueda llevar a cabo por disponibilidad o la no atención de la visita o la llamada, este incidente se podría estar cerrando sin ninguna responsabilidad de nuestra parte, por lo que le solicitamos notificar con al menos 2 horas de tiempo previo a la fecha y hora establecida, para que el servicio le sea re-agendado nuevamente.

Estamos para servirle.

Saludos.
Servicio al Cliente.";

      // HTML (mismo look del insert)
      string html = $@"
<div style='font-family:Arial,sans-serif;font-size:14px;color:#333;line-height:1.5'>
  <p>Estimado: <strong>{WebUtility.HtmlEncode(nombreSolicitante)}</strong></p>

  <p>Su incidente, <strong>No. {ticket}</strong>, ha sido <strong>actualizado</strong> y se encuentra asignado al Agente de Servicio
     <strong>“{WebUtility.HtmlEncode(nombreAsignado)}”</strong>, quien se estará poniendo en contacto con usted para la respectiva evaluación.</p>

  <p>Por favor, mantener a mano la información que será requerida como números de serie, modelo, marca, registros del problema y otras evidencias que nos permitan efectuar el diagnóstico respectivo y en el tiempo asignado para tal fin.</p>

  <p>Le recordamos que todo servicio que no se encuentre bajo garantía o contrato, podrá requerir la aprobación de la cotización de diagnóstico previo a la ejecución del respectivo servicio.</p>

  <p style='margin:16px 0 6px'><strong>Su Servicio se encuentra agendado para:</strong></p>
  <p style='margin:0'>{WebUtility.HtmlEncode(fechaTxt)}</p>
  <p style='margin:0'>{WebUtility.HtmlEncode(horaTxt)}</p>

  <p style='margin-top:16px'><strong>Atención:</strong> Cuando una evaluación de la situación no se pueda llevar a cabo por disponibilidad o la no atención de la visita o la llamada, este incidente se podría estar cerrando sin ninguna responsabilidad de nuestra parte, por lo que le solicitamos notificar con al menos 2 horas de tiempo previo a la fecha y hora establecida, para que el servicio le sea re-agendado nuevamente.</p>

  <p style='margin-top:16px'>Estamos para servirle.</p>
  <p>Saludos.<br/>Servicio al Cliente.</p>
</div>";

      return (subject, plain, html);
    }





    const string SQL_SELECT_BY_TICKET = @"
SELECT 
  ControlTraficoId,
  SolicitanteId,
  EmpresaId,
  ContratoId,
  AsignadoAId,
  RazonServicioId,
  TelefonoServicio,
  EmailServicio,
  DireccionServicio,
  LugarServicio,
  FechaProximoServicio,
  HoraServicio,
  DescripcionIncidente
FROM dbo.CONTROL_TRAFICO
WHERE Ticket = @Ticket;";


    private sealed class TicketSnapshot
    {
      public Guid ControlTraficoId { get; set; }
      public Guid SolicitanteId { get; set; }
      public Guid EmpresaId { get; set; }
      public Guid? ContratoId { get; set; }
      public Guid? AsignadoAId { get; set; }
      public Guid RazonServicioId { get; set; }
      public string? TelefonoServicio { get; set; }
      public string? EmailServicio { get; set; }
      public string? DireccionServicio { get; set; }
      public byte? LugarServicio { get; set; }
      public DateTime? FechaProximoServicio { get; set; }
      public TimeSpan? HoraServicio { get; set; }
      public string? DescripcionIncidente { get; set; }
    }

    private static TicketSnapshot? GetSnapshotByTicket(string cs, int ticket)
    {
      using var cn = new SqlConnection(cs);
      cn.Open();
      using var cmd = new SqlCommand(SQL_SELECT_BY_TICKET, cn);
      cmd.Parameters.AddWithValue("@Ticket", ticket);
      using var r = cmd.ExecuteReader();
      if (!r.Read()) return null;
      return new TicketSnapshot
      {
        ControlTraficoId = r.GetGuid(r.GetOrdinal("ControlTraficoId")),
        SolicitanteId = r.GetGuid(r.GetOrdinal("SolicitanteId")),
        EmpresaId = r.GetGuid(r.GetOrdinal("EmpresaId")),
        ContratoId = r["ContratoId"] as Guid?,
        AsignadoAId = r["AsignadoAId"] as Guid?,
        RazonServicioId = r.GetGuid(r.GetOrdinal("RazonServicioId")),
        TelefonoServicio = r["TelefonoServicio"] as string,
        EmailServicio = r["EmailServicio"] as string,
        DireccionServicio = r["DireccionServicio"] as string,
        LugarServicio = r["LugarServicio"] as byte?,
        FechaProximoServicio = r["FechaProximoServicio"] as DateTime?,
        HoraServicio = r["HoraServicio"] as TimeSpan?,
        DescripcionIncidente = r["DescripcionIncidente"] as string
      };
    }

    private static (string plain, string html) BuildChangesSummary(
      (string label, string? oldVal, string? newVal)[] changes)
    {
      var sbPlain = new StringBuilder();
      var sbHtml = new StringBuilder("<ul style='margin:8px 0 0 18px'>");

      foreach (var c in changes)
      {
        if ((c.oldVal ?? "") == (c.newVal ?? "")) continue;
        var oldV = string.IsNullOrWhiteSpace(c.oldVal) ? "N/D" : c.oldVal;
        var newV = string.IsNullOrWhiteSpace(c.newVal) ? "N/D" : c.newVal;

        sbPlain.AppendLine($"• {c.label}: {oldV}  →  {newV}");
        sbHtml.Append("<li><strong>")
              .Append(WebUtility.HtmlEncode(c.label))
              .Append(":</strong> ")
              .Append(WebUtility.HtmlEncode(oldV))
              .Append(" → ")
              .Append(WebUtility.HtmlEncode(newV))
              .Append("</li>");
      }

      sbHtml.Append("</ul>");
      return (sbPlain.ToString().TrimEnd(), sbHtml.ToString());
    }


    private static (string subject, string txt, string html) BuildMailToUserEdit(
  string nombreSolicitante,
  string nombreAsignado,
  int ticket,
  DateTime? fechaProx,
  TimeSpan? horaServ,
  string changesPlain,
  string changesHtml)
    {
      string subject = $"Ticket #{ticket} actualizado";
      string fechaTxt = FormatFechaCR(fechaProx);
      string horaTxt = FormatHoraCR(horaServ);

      string plain =
    $@"Estimado: {nombreSolicitante}

Su incidente, No. {ticket}, ha sido actualizado. 
Agente de Servicio asignado: {nombreAsignado}.

Cambios aplicados:
{(string.IsNullOrWhiteSpace(changesPlain) ? "• (Sin cambios relevantes mostrables)" : changesPlain)}

Su Servicio se encuentra agendado para:
{fechaTxt}
{horaTxt}

Estamos para servirle.

Saludos.
Servicio al Cliente.";

      string html = $@"
<div style='font-family:Arial,sans-serif;font-size:14px;color:#333;line-height:1.5'>
  <p>Estimado: <strong>{WebUtility.HtmlEncode(nombreSolicitante)}</strong></p>
  <p>Su incidente, <strong>No. {ticket}</strong>, ha sido <strong>actualizado</strong>.<br/>
     Agente de Servicio asignado: <strong>{WebUtility.HtmlEncode(nombreAsignado)}</strong>.</p>

  <p style='margin:10px 0 6px'><strong>Cambios aplicados:</strong></p>
  {(string.IsNullOrWhiteSpace(changesHtml) ? "<p>(Sin cambios relevantes mostrables)</p>" : changesHtml)}

  <p style='margin:16px 0 6px'><strong>Su Servicio se encuentra agendado para:</strong></p>
  <p style='margin:0'>{WebUtility.HtmlEncode(fechaTxt)}</p>
  <p style='margin:0'>{WebUtility.HtmlEncode(horaTxt)}</p>

  <p style='margin-top:16px'>Estamos para servirle.</p>
  <p>Saludos.<br/>Servicio al Cliente.</p>
</div>";
      return (subject, plain, html);
    }

    private static (string subject, string txt, string html) BuildMailToAssignedEdit(
      string assignedUserName,
      int ticket,
      string changesPlain,
      string changesHtml,
      bool esReasignacion)
    {
      string subject = esReasignacion
        ? $"[Ticket #{ticket}] Te ha sido asignado"
        : $"[Ticket #{ticket}] Actualizado";

      string plain =
    $@"{(esReasignacion ? "Este ticket te ha sido asignado." : "Este ticket ha sido actualizado.")}

Ticket: {ticket}

Cambios:
{(string.IsNullOrWhiteSpace(changesPlain) ? "• (Sin cambios relevantes mostrables)" : changesPlain)}

Por favor, gestiona este caso en ServiceTrackID.";

      string html = $@"
<div style='font-family:Arial,sans-serif;font-size:14px;color:#333;line-height:1.5'>
  <h3 style='margin:0 0 8px'>{(esReasignacion ? "Ticket asignado" : "Ticket actualizado")}</h3>
  <p><strong>Ticket:</strong> #{ticket}</p>
  <p style='margin:10px 0 6px'><strong>Cambios:</strong></p>
  {(string.IsNullOrWhiteSpace(changesHtml) ? "<p>(Sin cambios relevantes mostrables)</p>" : changesHtml)}
  <p style='margin-top:14px'>Por favor, gestiona este caso en ServiceTrackID.</p>
</div>";
      return (subject, plain, html);
    }




    private static (string? Nombre, string? Email, string? Telefono) GetSolicitanteById(string cs, Guid solicitanteId)
    {
      using var cn = new SqlConnection(cs);
      cn.Open();
      using var cmd = new SqlCommand(
        "SELECT TOP 1 NombreCompleto, Email, Telefono FROM dbo.SolicitanteServiceTrackID WHERE SolicitanteId=@id", cn);
      cmd.Parameters.AddWithValue("@id", solicitanteId);
      using var r = cmd.ExecuteReader();
      if (r.Read())
      {
        return (r["NombreCompleto"] as string, r["Email"] as string, r["Telefono"] as string);
      }
      return (null, null, null);
    }

    private static string? GetEmpresaNombre(string cs, Guid empresaId)
    {
      using var cn = new SqlConnection(cs);
      cn.Open();
      using var cmd = new SqlCommand("SELECT TOP 1 NOMBRE FROM dbo.EMPRESA WHERE ID_EMPRESA=@id", cn);
      cmd.Parameters.AddWithValue("@id", empresaId);
      var o = cmd.ExecuteScalar();
      return (o == null || o == DBNull.Value) ? null : Convert.ToString(o);
    }

    private static string? GetContratoNumero(string cs, Guid contratoId)
    {
      using var cn = new SqlConnection(cs);
      cn.Open();
      using var cmd = new SqlCommand("SELECT TOP 1 NUMERO FROM dbo.CONTRATOS WHERE ID_CONTRATO=@id", cn);
      cmd.Parameters.AddWithValue("@id", contratoId);
      var o = cmd.ExecuteScalar();
      return (o == null || o == DBNull.Value) ? null : Convert.ToString(o);
    }

    private static (string? Email, string? UserName) GetUserById(string cs, Guid userSysId)
    {
      using var cn = new SqlConnection(cs);
      cn.Open();
      using var cmd = new SqlCommand("SELECT TOP 1 email, username FROM dbo.users WHERE userSysId=@id", cn);
      cmd.Parameters.AddWithValue("@id", userSysId);
      using var r = cmd.ExecuteReader();
      if (r.Read())
      {
        return (r["email"] as string, r["username"] as string);
      }
      return (null, null);
    }

    private static string NullOrND(string? s) => string.IsNullOrWhiteSpace(s) ? "N/D" : s.Trim();

    private static (string subject, string txt, string html) BuildMailToUser(
        string nombreSolicitante,
        string nombreAsignado,
        int ticket,
        DateTime? fechaProx,   // fecha agendada
        TimeSpan? horaServ     // hora agendada
    )
    {
      string fechaTxt = FormatFechaCR(fechaProx);  // ej. 23/Setiembre/2025
      string horaTxt = FormatHoraCR(horaServ);    // ej. 14:20 horas

      string subject = $"Ticket #{ticket} asignado";

      // === Texto plano ===
      string plain =
  $@"Estimado: {nombreSolicitante}

Su incidente, No. {ticket}, ha sido asignado al Agente de Servicio “{nombreAsignado}”, quien se estará poniendo en contacto con usted para la respectiva evaluación.

Por favor, mantener a mano la información que será requerida como números de serie, modelo, marca, registros del problema y otras evidencias que nos permitan efectuar el diagnóstico respectivo y en el tiempo asignado para tal fin.

Le recordamos que todo servicio que no se encuentre bajo garantía o contrato, podrá requerir la aprobación de la cotización de diagnóstico previo a la ejecución del respectivo servicio.

Su Servicio se encuentra agendado para:

{fechaTxt}
{horaTxt}

Atención: Cuando una evaluación de la situación no se pueda llevar a cabo por disponibilidad o la no atención de la visita o la llamada, este incidente se podría estar cerrando sin ninguna responsabilidad de nuestra parte, por lo que le solicitamos notificar con al menos 2 horas de tiempo previo a la fecha y hora establecida para que el servicio le sea re-agendado nuevamente.

Estamos para servirle.

Saludos.
Servicio al Cliente.";

      // === HTML ===
      string html = $@"
<div style='font-family:Arial,sans-serif;font-size:14px;color:#333;line-height:1.5'>
  <p>Estimado: <strong>{WebUtility.HtmlEncode(nombreSolicitante)}</strong></p>

  <p>Su incidente, <strong>No. {ticket}</strong>, ha sido asignado al Agente de Servicio
     <strong>“{WebUtility.HtmlEncode(nombreAsignado)}”</strong>, quien se estará poniendo en contacto con usted para la respectiva evaluación.</p>

  <p>Por favor, mantener a mano la información que será requerida como números de serie, modelo, marca, registros del problema y otras evidencias que nos permitan efectuar el diagnóstico respectivo y en el tiempo asignado para tal fin.</p>

  <p>Le recordamos que todo servicio que no se encuentre bajo garantía o contrato, podrá requerir la aprobación de la cotización de diagnóstico previo a la ejecución del respectivo servicio.</p>

  <p style='margin:16px 0 6px'><strong>Su Servicio se encuentra agendado para:</strong></p>
  <p style='margin:0'>{WebUtility.HtmlEncode(fechaTxt)}</p>
  <p style='margin:0'>{WebUtility.HtmlEncode(horaTxt)}</p>

  <p style='margin-top:16px'><strong>Atención:</strong> Cuando una evaluación de la situación no se pueda llevar a cabo por disponibilidad o la no atención de la visita o la llamada, este incidente se podría estar cerrando sin ninguna responsabilidad de nuestra parte, por lo que le solicitamos notificar con al menos 2 horas de tiempo previo a la fecha y hora establecida para que el servicio le sea re-agendado nuevamente.</p>

  <p style='margin-top:16px'>Estamos para servirle.</p>
  <p>Saludos.<br/>Servicio al Cliente.</p>
</div>";

      return (subject, plain, html);
    }

    private static string FormatFechaCR(DateTime? fecha)
    {
      if (fecha == null) return "N/D";
      // Cultura es-CR devuelve “septiembre”; en CR se usa mucho “setiembre”.
      var cr = new System.Globalization.CultureInfo("es-CR");
      string s = fecha.Value.ToString("dd/MMMM/yyyy", cr);
      // Ajuste visual opcional:
      s = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s);
      s = s.Replace("Septiembre", "Setiembre").Replace("De ", "de "); // limpio capitalización
      return s;
    }

    private static string FormatHoraCR(TimeSpan? hora)
    {
      if (hora == null) return "N/D";
      return $"{hora.Value:hh\\:mm} horas";
    }

    private static (string subject, string txt, string html) BuildMailToAssigned(
      string assignedUserName, string nombreSolicitante, string email, string phone,
      string company, string contractNumber, string brief, int ticket)
    {
      string subject = $"[Nuevo Ticket #{ticket}] Asignado";

      string plain =
    $@"Se ha creado un nuevo ticket.

Ticket: {ticket}
Asignado a: {assignedUserName}
Solicitante: {nombreSolicitante}

Email: {email}
Phone: {phone}
Company: {company}
Contract Number: {contractNumber}

Brief Description:
{brief}

Por favor, gestione este caso en ServiceTrackID.";

      string html = $@"
<div style='font-family:Arial,sans-serif;font-size:14px;color:#333'>
  <h3 style='margin:0 0 10px'>Nuevo Ticket asignado</h3>
  <table style='width:100%;border-collapse:collapse'>
    <tr><td style='padding:4px 0'><strong>Ticket:</strong></td><td style='padding:4px 0'>#{ticket}</td></tr>
    <tr><td style='padding:4px 0'><strong>Asignado a:</strong></td><td style='padding:4px 0'>{WebUtility.HtmlEncode(assignedUserName)}</td></tr>
    <tr><td style='padding:4px 0'><strong>Solicitante:</strong></td><td style='padding:4px 0'>{WebUtility.HtmlEncode(nombreSolicitante)}</td></tr>
    <tr><td style='padding:4px 0'><strong>Email:</strong></td><td style='padding:4px 0'>{WebUtility.HtmlEncode(email)}</td></tr>
    <tr><td style='padding:4px 0'><strong>Phone:</strong></td><td style='padding:4px 0'>{WebUtility.HtmlEncode(phone)}</td></tr>
    <tr><td style='padding:4px 0'><strong>Company:</strong></td><td style='padding:4px 0'>{WebUtility.HtmlEncode(company)}</td></tr>
    <tr><td style='padding:4px 0'><strong>Contract Number:</strong></td><td style='padding:4px 0'>{WebUtility.HtmlEncode(contractNumber)}</td></tr>
    <tr><td style='padding:4px 0;vertical-align:top'><strong>Brief Description:</strong></td>
        <td style='padding:4px 0;white-space:pre-wrap'>{WebUtility.HtmlEncode(brief)}</td></tr>
  </table>
  <p style='margin-top:14px'>Por favor, gestione este caso en ServiceTrackID.</p>
</div>";
      return (subject, plain, html);
    }














    // ====== Helpers de parse ======
    private static DateTime? TryParseDate(string? raw)
    {
      if (string.IsNullOrWhiteSpace(raw)) return null;
      raw = raw.Trim();

      // Acepta dd/MM/yyyy, d/M/yy, MM/dd/yyyy, ISO yyyy-MM-dd
      var formats = new[]
      {
            "dd/MM/yyyy","d/M/yyyy","dd/MM/yy","d/M/yy",
            "MM/dd/yyyy","M/d/yyyy","MM/dd/yy","M/d/yy",
            "yyyy-MM-dd","yyyy/M/d"
        };
      if (DateTime.TryParseExact(raw, formats, System.Globalization.CultureInfo.InvariantCulture,
          System.Globalization.DateTimeStyles.None, out var dt))
        return dt.Date;

      // Fallback general (usa cultura del server)
      if (DateTime.TryParse(raw, out dt)) return dt.Date;

      return null;
    }

    private static TimeSpan? TryParseTime(string? s)
    {
      if (string.IsNullOrWhiteSpace(s)) return null;
      s = s.Trim();

      // "1624" -> "16:24"
      if (System.Text.RegularExpressions.Regex.IsMatch(s, @"^\d{3,4}$"))
      {
        s = s.PadLeft(4, '0');
        s = s[..2] + ":" + s[2..4];
      }

      if (TimeSpan.TryParse(s, out var ts)) return ts;
      // Intento explícito HH:mm
      if (DateTime.TryParseExact(s, "HH:mm", null, System.Globalization.DateTimeStyles.None, out var dt))
        return dt.TimeOfDay;

      return null;
    }


    public static List<SelectListItem> ListarParaSelectRazon()
    {
      string cs = System.Configuration.ConfigurationManager
           .ConnectionStrings["ServerDiverscan"].ConnectionString;
      var items = new List<SelectListItem>();

      using var cn = new SqlConnection(cs);
      using var cmd = new SqlCommand(@"
            SELECT 
                id_razonServicios,
                LTRIM(RTRIM(ISNULL(nombre,'')))      AS Nombre,
                LTRIM(RTRIM(ISNULL(descripcion,''))) AS Descripcion
            FROM dbo.RazonServicios
            ORDER BY 
                CASE WHEN NULLIF(nombre,'') IS NULL THEN 1 ELSE 0 END, nombre, descripcion
        ", cn);

      cn.Open();
      using var rd = cmd.ExecuteReader();
      while (rd.Read())
      {
        var id = rd.GetGuid(0);
        var nom = rd.IsDBNull(1) ? "" : rd.GetString(1);
        var desc = rd.IsDBNull(2) ? "" : rd.GetString(2);

        var text = string.IsNullOrWhiteSpace(nom) ? id.ToString() : nom;
        if (!string.IsNullOrWhiteSpace(desc)) text += $" — {desc}";

        items.Add(new SelectListItem { Value = id.ToString(), Text = text });
      }

      return items;
    }

    public static List<SelectListItem> ListarParaSelectUsers(bool soloAprobados = true)
    {
      string cs = System.Configuration.ConfigurationManager
                 .ConnectionStrings["ServerDiverscan"].ConnectionString;
      var items = new List<SelectListItem>();

      using var cn = new SqlConnection(cs);
      using var cmd = new SqlCommand(@"
            SELECT 
                userSysId,
                LTRIM(RTRIM(ISNULL(username,''))) AS Username,
                LTRIM(RTRIM(ISNULL(email,'')))    AS Email
            FROM dbo.[users]
            WHERE (@solo = 0 OR ISNULL(isApproved,0) = 1)
            ORDER BY Username
        ", cn);

      cmd.Parameters.AddWithValue("@solo", soloAprobados ? 1 : 0);

      cn.Open();
      using var rd = cmd.ExecuteReader();
      while (rd.Read())
      {
        var id = rd.GetGuid(0);
        var user = rd.IsDBNull(1) ? "" : rd.GetString(1);
        var mail = rd.IsDBNull(2) ? "" : rd.GetString(2);

        var text = string.IsNullOrWhiteSpace(mail) ? user : $"{user} ({mail})";
        if (string.IsNullOrWhiteSpace(text)) text = id.ToString();

        items.Add(new SelectListItem { Value = id.ToString(), Text = text });
      }

      return items;
    }

    public static List<SelectListItem> ListarParaSelectSolicitante()
    {
      string cs = System.Configuration.ConfigurationManager
                 .ConnectionStrings["ServerDiverscan"].ConnectionString;
      var items = new List<SelectListItem>();

      using var cn = new SqlConnection(cs);
      using var cmd = new SqlCommand(@"
            SELECT 
                SolicitanteId,
                LTRIM(RTRIM(ISNULL(NULLIF(NombreCompleto, ''), Email))) AS Nombre,
                Email
            FROM dbo.SolicitanteServiceTrackID
            ORDER BY LTRIM(RTRIM(ISNULL(NULLIF(NombreCompleto, ''), Email)))
        ", cn);

      cn.Open();
      using var rd = cmd.ExecuteReader();
      while (rd.Read())
      {
        var id = rd.GetGuid(0);
        var nom = rd.IsDBNull(1) ? "" : rd.GetString(1);
        var mail = rd.IsDBNull(2) ? "" : rd.GetString(2);

        // Texto a mostrar: "Nombre (email)" cuando haya ambos
        var text = string.IsNullOrWhiteSpace(mail) ? nom : $"{nom} ({mail})";

        items.Add(new SelectListItem
        {
          Value = id.ToString(),
          Text = string.IsNullOrWhiteSpace(text) ? id.ToString() : text
        });
      }

      return items;
    }

    public static List<SelectListItem> ListarParaSelectContrato()
    {
      string cs = System.Configuration.ConfigurationManager
                .ConnectionStrings["ServerDiverscan"].ConnectionString;
      var items = new List<SelectListItem>();

      using var cn = new SqlConnection(cs);
      using var cmd = new SqlCommand(@"
            SELECT 
                ID_CONTRATO,
                LTRIM(RTRIM(ISNULL(NOMBRE, ''))) AS Nombre,
                LTRIM(RTRIM(ISNULL(NUMERO,  ''))) AS Numero
            FROM dbo.CONTRATOS
            ORDER BY 
                CASE WHEN NULLIF(Numero,'') IS NULL THEN 1 ELSE 0 END, Numero,
                CASE WHEN NULLIF(Nombre,'') IS NULL THEN 1 ELSE 0 END, Nombre
        ", cn);

      cn.Open();
      using var rd = cmd.ExecuteReader();
      while (rd.Read())
      {
        var id = rd.GetGuid(0);
        var nombre = rd.IsDBNull(1) ? "" : rd.GetString(1);
        var numero = rd.IsDBNull(2) ? "" : rd.GetString(2);

        string text;
        if (!string.IsNullOrWhiteSpace(numero) && !string.IsNullOrWhiteSpace(nombre))
          text = $"{numero} — {nombre}";
        else if (!string.IsNullOrWhiteSpace(numero))
          text = numero;
        else if (!string.IsNullOrWhiteSpace(nombre))
          text = nombre;
        else
          text = id.ToString(); // último recurso

        items.Add(new SelectListItem { Value = id.ToString(), Text = text });
      }

      return items;
    }

    // EMPRESAS → Text = NOMBRE
    public static List<SelectListItem> ListarParaSelectEmpresa()
    {
      string cs = System.Configuration.ConfigurationManager
          .ConnectionStrings["ServerDiverscan"].ConnectionString;

      var items = new List<SelectListItem>();
      using var cn = new SqlConnection(cs);
      using var cmd = new SqlCommand(@"
        SELECT ID_EMPRESA, LTRIM(RTRIM(ISNULL(NOMBRE,''))) AS Nombre
        FROM dbo.EMPRESA
        ORDER BY CASE WHEN NULLIF(Nombre,'') IS NULL THEN 1 ELSE 0 END, Nombre
    ", cn);

      cn.Open();
      using var rd = cmd.ExecuteReader();
      while (rd.Read())
      {
        var id = rd.GetGuid(0).ToString();
        var name = rd.IsDBNull(1) ? "" : rd.GetString(1);
        var text = string.IsNullOrWhiteSpace(name) ? "(Sin nombre)" : name;

        items.Add(new SelectListItem { Value = id, Text = text });
      }
      return items;
    }


    public IActionResult AdministracionTickets()
    {
      return View();
    }

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

    public List<GestionServiciosDTO> ObtenerServicios()
    {
      var lista = new List<GestionServiciosDTO>();
      string cs = System.Configuration.ConfigurationManager
                    .ConnectionStrings["ServerDiverscan"].ConnectionString;

      using (var conn = new SqlConnection(cs))
      using (var cmd = new SqlCommand(@"
    SELECT 
        gs.gestionServiciosId,
        gs.solicitante,
        s.NombreCompleto AS NombreSolicitante,      -- <- viene de la nueva tabla
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
    FROM dbo.GestionServicios gs
    LEFT JOIN dbo.SolicitanteServiceTrackID s ON gs.solicitante = s.SolicitanteId  -- <- nuevo JOIN
    LEFT JOIN dbo.users            u  ON gs.asignarIncidente = u.userSysId
    LEFT JOIN dbo.ActivosSignusID  a  ON gs.activo           = a.ID_ACTIVO
    LEFT JOIN dbo.RazonServicios   rs ON gs.razonServicio    = rs.id_razonServicios
    LEFT JOIN dbo.EstadoActivo     es ON gs.estadoActivo     = es.id_estadoActivo
", conn))
      {
        conn.Open();
        using (var reader = cmd.ExecuteReader())
        {
          int iGestionId = reader.GetOrdinal("gestionServiciosId");
          int iSolic = reader.GetOrdinal("solicitante");
          int iNomSolic = reader.GetOrdinal("NombreSolicitante");   // alias del SELECT
          int iActivo = reader.GetOrdinal("activo");
          int iNomActivo = reader.GetOrdinal("NombreActivo");
          int iRazon = reader.GetOrdinal("razonServicio");
          int iNomRazon = reader.GetOrdinal("NombreRazonServicio");
          int iEstado = reader.GetOrdinal("estadoActivo");
          int iNomEstado = reader.GetOrdinal("NombreEstadoActivo");
          int iAsig = reader.GetOrdinal("asignarIncidente");
          int iNomAsig = reader.GetOrdinal("NombreAsignarIncidente");
          int iFecEst = reader.GetOrdinal("fechaEstimadaCierre");
          int iFecha = reader.GetOrdinal("fecha");
          int iDesc = reader.GetOrdinal("descripcion");
          int iTicket = reader.GetOrdinal("numeroTicket");

          while (reader.Read())
          {
            var item = new GestionServiciosDTO
            {
              GestionServiciosId = reader.GetGuid(iGestionId),

              // GUIDs nullable (siguen viniendo de gs.*)
              SolicitanteId = reader.IsDBNull(iSolic) ? (Guid?)null : reader.GetGuid(iSolic),
              ActivoId = reader.IsDBNull(iActivo) ? (Guid?)null : reader.GetGuid(iActivo),
              RazonServicioId = reader.IsDBNull(iRazon) ? (Guid?)null : reader.GetGuid(iRazon),
              EstadoActivoId = reader.IsDBNull(iEstado) ? (Guid?)null : reader.GetGuid(iEstado),
              AsignarIncidenteId = reader.IsDBNull(iAsig) ? (Guid?)null : reader.GetGuid(iAsig),

              // Nombres mostrables (NombreSolicitante viene de s.NombreCompleto)
              NombreSolicitante = reader.IsDBNull(iNomSolic) ? null : reader.GetString(iNomSolic),
              NombreActivo = reader.IsDBNull(iNomActivo) ? null : reader.GetString(iNomActivo),
              NombreRazonServicio = reader.IsDBNull(iNomRazon) ? null : reader.GetString(iNomRazon),
              NombreEstadoActivo = reader.IsDBNull(iNomEstado) ? null : reader.GetString(iNomEstado),
              NombreAsignarIncidente = reader.IsDBNull(iNomAsig) ? "Sin Asignar" : reader.GetString(iNomAsig),

              // Fechas
              FechaEstimadaCierre = reader.IsDBNull(iFecEst) ? (DateTime?)null : reader.GetDateTime(iFecEst),
              Fecha = reader.GetDateTime(iFecha),

              // Otros
              Descripcion = reader.IsDBNull(iDesc) ? null : reader.GetString(iDesc),
              NumeroTicket = reader.GetInt32(iTicket)
            };

            lista.Add(item);
          }
        }
      }

      return lista;
    }


  }
}
