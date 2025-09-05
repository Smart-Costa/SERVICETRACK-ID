using AspnetCoreMvcFull.Models.Activos;
using AspnetCoreMvcFull.Models.Roles;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Text.Json.Serialization;

namespace AspnetCoreMvcFull.Controllers
{
  public class ActivosController : Controller
  {
    public IActionResult Index()
    {
      return View();
    }
    [HttpGet]
    public IActionResult SeleccionarArchivo()
    {
      return View();
    }
    public IActionResult ImportarDatos()
    {
      return View();
    }
    public IActionResult Activos()
    {
      return View();
    }
    public class ImportRequest
    {
      [JsonPropertyName("items")] // asegura el bind aunque sea minúscula en JS
      public List<Dictionary<string, string>> Items { get; set; }
    }

    // ==== HELPERS (mismos que venías usando) ====
    private static string GetVal(Dictionary<string, string> row, string key)
      => (row != null && key != null && row.TryGetValue(key, out var v)) ? v : null;

    private static int? ToInt(string s)
    {
      if (string.IsNullOrWhiteSpace(s)) return null;
      return int.TryParse(s.Trim(), out var n) ? n : (int?)null;
    }

    private static double? ToDouble(string s)
    {
      if (string.IsNullOrWhiteSpace(s)) return null;
      return double.TryParse(s.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var d)
        ? d : (double?)null;
    }

    private static bool? ToBool(string s)
    {
      if (string.IsNullOrWhiteSpace(s)) return null;
      var t = s.Trim().ToLowerInvariant();
      if (t == "1" || t == "true" || t == "sí" || t == "si") return true;
      if (t == "0" || t == "false" || t == "no") return false;
      return null;
    }

    private static Guid? ToGuid(string s)
    {
      if (string.IsNullOrWhiteSpace(s)) return null;
      return Guid.TryParse(s.Trim(), out var g) ? g : (Guid?)null;
    }

    private static DateTime? ParseDate_ddMMyyyy(string s)
    {
      if (string.IsNullOrWhiteSpace(s)) return null;
      // Acepta dd/MM/yyyy y variantes ISO que ya estuviste usando
      if (DateTime.TryParseExact(
            s.Trim(),
            new[] { "dd/MM/yyyy", "d/M/yyyy", "yyyy-MM-dd", "yyyy/M/d" },
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var dt))
      {
        return dt;
      }
      return null;
    }

    // --- helpers simples ---
    private static string TrimTo(string s, int max)
    {
      if (string.IsNullOrWhiteSpace(s)) return null;
      s = s.Trim();
      return s.Length > max ? s.Substring(0, max) : s;
    }

    // Devuelve el GUID de la categoría (name + description).
    // Si no existe, la crea y devuelve el nuevo GUID.
    // Usa la misma conexión y transacción del batch.
    private Guid? GetOrCreateAssetCategory(string name, string description, SqlConnection cn, SqlTransaction tx, Guid? userId = null)
    {
      // respeta longitudes de la tabla
      name = TrimTo(name, 50);
      description = TrimTo(description, 150);

      // si no hay suficientes datos, no seteamos categoría
      if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(description))
        return null;

      // 1) Buscar existente (exact match)
      using (var check = new SqlCommand(
        @"SELECT TOP 1 assetCategorySysId 
        FROM dbo.assetCategory 
       WHERE [name] = @name AND [description] = @desc", cn, tx))
      {
        check.Parameters.Add("@name", SqlDbType.VarChar, 50).Value = name;
        check.Parameters.Add("@desc", SqlDbType.VarChar, 150).Value = description;

        var existing = check.ExecuteScalar();
        if (existing != null && existing != DBNull.Value)
          return (Guid)existing;
      }

      // 2) Crear nueva
      var id = Guid.NewGuid();
      var entry = userId ?? Guid.NewGuid();   // si tienes el usuario logueado, pásalo aquí
      var update = entry;

      using (var insert = new SqlCommand(
        @"INSERT INTO dbo.assetCategory
        (assetCategorySysId, [name], [description], entryUser, updateUser)
      VALUES
        (@id, @name, @desc, @entryUser, @updateUser);", cn, tx))
      {
        insert.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = id;
        insert.Parameters.Add("@name", SqlDbType.VarChar, 50).Value = name;
        insert.Parameters.Add("@desc", SqlDbType.VarChar, 150).Value = description;
        insert.Parameters.Add("@entryUser", SqlDbType.UniqueIdentifier).Value = entry;
        insert.Parameters.Add("@updateUser", SqlDbType.UniqueIdentifier).Value = update;

        insert.ExecuteNonQuery();
      }

      return id;
    }

    // Devuelve el GUID del estado (name + description).
    // Si no existe, lo crea y devuelve el nuevo GUID.
    // Reutiliza la misma conexión y transacción del batch.
    private Guid? GetOrCreateAssetStatus(string name, string description, SqlConnection cn, SqlTransaction tx, Guid? userId = null)
    {
      // respeta longitudes de la tabla
      name = TrimTo(name, 50);
      description = TrimTo(description, 150);

      // si faltan datos, no seteamos estado
      if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(description))
        return null;

      // 1) Buscar existente (coincidencia exacta)
      using (var check = new SqlCommand(
        @"SELECT TOP 1 assetStatusSysId
        FROM dbo.assetStatus
       WHERE [name] = @name AND [description] = @desc", cn, tx))
      {
        check.Parameters.Add("@name", SqlDbType.VarChar, 50).Value = name;
        check.Parameters.Add("@desc", SqlDbType.VarChar, 150).Value = description;

        var existing = check.ExecuteScalar();
        if (existing != null && existing != DBNull.Value)
          return (Guid)existing;
      }

      // 2) Crear nuevo
      var id = Guid.NewGuid();
      var entry = userId ?? Guid.NewGuid(); // si tienes el usuario actual, pásalo aquí
      var update = entry;

      using (var insert = new SqlCommand(
        @"INSERT INTO dbo.assetStatus
        (assetStatusSysId, [name], [description], entryUser, updateUser)
      VALUES
        (@id, @name, @desc, @entryUser, @updateUser);", cn, tx))
      {
        insert.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = id;
        insert.Parameters.Add("@name", SqlDbType.VarChar, 50).Value = name;
        insert.Parameters.Add("@desc", SqlDbType.VarChar, 150).Value = description;
        insert.Parameters.Add("@entryUser", SqlDbType.UniqueIdentifier).Value = entry;
        insert.Parameters.Add("@updateUser", SqlDbType.UniqueIdentifier).Value = update;

        insert.ExecuteNonQuery();
      }

      return id;
    }

    // Busca una empresa por NOMBRE; si no existe, la crea y devuelve su ID.
    // Usa la misma conexión y transacción del batch.
    private Guid? GetOrCreateEmpresa(string nombre, SqlConnection cn, SqlTransaction tx, string descripcion = null)
    {
      nombre = TrimTo(nombre, 100);
      descripcion = TrimTo(descripcion, 100);

      if (string.IsNullOrWhiteSpace(nombre))
        return null;

      // 1) ¿Ya existe?
      using (var check = new SqlCommand(
        @"SELECT TOP 1 ID_EMPRESA
        FROM dbo.EMPRESA
       WHERE NOMBRE = @nombre", cn, tx))
      {
        check.Parameters.Add("@nombre", SqlDbType.VarChar, 100).Value = nombre;
        var existing = check.ExecuteScalar();
        if (existing != null && existing != DBNull.Value)
          return (Guid)existing;
      }

      // 2) Crear nueva
      var id = Guid.NewGuid();
      using (var insert = new SqlCommand(
        @"INSERT INTO dbo.EMPRESA (ID_EMPRESA, NOMBRE, DESCRIPCION)
      VALUES (@id, @nombre, @descripcion);", cn, tx))
      {
        insert.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = id;
        insert.Parameters.Add("@nombre", SqlDbType.VarChar, 100).Value = nombre;
        insert.Parameters.Add("@descripcion", SqlDbType.VarChar, 100).Value = (object)descripcion ?? DBNull.Value;
        insert.ExecuteNonQuery();
      }

      return id;
    }

    // Busca una marca por NOMBRE; si no existe, la crea y devuelve su marcaId.
    // Si tienes un Guid de usuario actual, pásalo en userId; si no, se usa Guid.Empty.
    private Guid? GetOrCreateMarca(string nombre, SqlConnection cn, SqlTransaction tx, string descripcion = null, Guid? userId = null)
    {
      nombre = TrimTo(nombre, 255);           // nvarchar(255)
                                              // descripcion puede ser nvarchar(max); solo la normalizamos un poco
      descripcion = string.IsNullOrWhiteSpace(descripcion) ? null : descripcion.Trim();

      if (string.IsNullOrWhiteSpace(nombre))
        return null;

      // 1) ¿Existe ya por nombre?
      using (var check = new SqlCommand(
        @"SELECT TOP 1 marcaId
        FROM dbo.Marca
       WHERE nombre = @nombre", cn, tx))
      {
        check.Parameters.Add("@nombre", SqlDbType.NVarChar, 255).Value = nombre;
        var existing = check.ExecuteScalar();
        if (existing != null && existing != DBNull.Value)
          return (Guid)existing;
      }

      // 2) Crear nueva
      var id = Guid.NewGuid();
      var user = userId ?? Guid.Empty; // o el Guid del usuario logueado si lo tienes

      using (var insert = new SqlCommand(
        @"INSERT INTO dbo.Marca
        (marcaId, nombre, descripcion, entryUser, entryDate, updateUser, updateDate)
      VALUES
        (@id, @nombre, @descripcion, @entryUser, GETDATE(), @updateUser, GETDATE());", cn, tx))
      {
        insert.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = id;
        insert.Parameters.Add("@nombre", SqlDbType.NVarChar, 255).Value = nombre;
        // nvarchar(max) => tamaño -1
        var pDesc = insert.Parameters.Add("@descripcion", SqlDbType.NVarChar, -1);
        pDesc.Value = (object)descripcion ?? DBNull.Value;

        insert.Parameters.Add("@entryUser", SqlDbType.UniqueIdentifier).Value = user;
        insert.Parameters.Add("@updateUser", SqlDbType.UniqueIdentifier).Value = user;

        insert.ExecuteNonQuery();
      }

      return id;
    }

    // Busca un modelo por NOMBRE (y opcionalmente por MARCA). Si no existe, lo crea.
    // - nombre: nvarchar(255)
    // - marcaId: fk opcional a dbo.Marca.marcaId (puede ser null)
    // - descripcion: nvarchar(max) opcional
    // - userId: si tienes el usuario actual, pásalo; si no, se usa Guid.Empty
    private Guid? GetOrCreateModelo(string nombre, Guid? marcaId, SqlConnection cn, SqlTransaction tx, string descripcion = null, Guid? userId = null)
    {
      nombre = TrimTo(nombre, 255);
      descripcion = string.IsNullOrWhiteSpace(descripcion) ? null : descripcion.Trim();

      if (string.IsNullOrWhiteSpace(nombre))
        return null;

      // 1) ¿Existe ya? (scoped por marcaId si viene)
      object existing = null;
      if (marcaId.HasValue)
      {
        using (var check = new SqlCommand(
          @"SELECT TOP 1 modeloId
          FROM dbo.Modelo
         WHERE nombre = @nombre AND marcaId = @marcaId", cn, tx))
        {
          check.Parameters.Add("@nombre", SqlDbType.NVarChar, 255).Value = nombre;
          check.Parameters.Add("@marcaId", SqlDbType.UniqueIdentifier).Value = marcaId.Value;
          existing = check.ExecuteScalar();
        }
      }
      else
      {
        using (var check = new SqlCommand(
          @"SELECT TOP 1 modeloId
          FROM dbo.Modelo
         WHERE nombre = @nombre AND marcaId IS NULL", cn, tx))
        {
          check.Parameters.Add("@nombre", SqlDbType.NVarChar, 255).Value = nombre;
          existing = check.ExecuteScalar();
        }
      }

      if (existing != null && existing != DBNull.Value)
        return (Guid)existing;

      // 2) Crear
      var id = Guid.NewGuid();
      var user = userId ?? Guid.Empty;

      using (var insert = new SqlCommand(
        @"INSERT INTO dbo.Modelo
        (modeloId, marcaId, nombre, descripcion, entryUser, entryDate, updateUser, updateDate)
      VALUES
        (@id, @marcaId, @nombre, @descripcion, @entryUser, GETDATE(), @updateUser, GETDATE());", cn, tx))
      {
        insert.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = id;

        var pMarca = insert.Parameters.Add("@marcaId", SqlDbType.UniqueIdentifier);
        pMarca.Value = (object)marcaId ?? DBNull.Value;

        insert.Parameters.Add("@nombre", SqlDbType.NVarChar, 255).Value = nombre;

        var pDesc = insert.Parameters.Add("@descripcion", SqlDbType.NVarChar, -1);
        pDesc.Value = (object)descripcion ?? DBNull.Value;

        insert.Parameters.Add("@entryUser", SqlDbType.UniqueIdentifier).Value = user;
        insert.Parameters.Add("@updateUser", SqlDbType.UniqueIdentifier).Value = user;

        insert.ExecuteNonQuery();
      }

      return id;
    }

    // Crea o retorna un empleado por Nombre/Apellido (opcionalmente scoped por companySysId).
    // empleadoStr: puede traer "Nombre Apellido" (o solo "Nombre") o incluso un GUID.
    // Si viene un GUID válido, mejor úsalo antes de llamar a este helper.
    private Guid? GetOrCreateEmpleadoByNames(
  string nombreRaw,
  string apellidosRaw,
  SqlConnection cn,
  SqlTransaction tx,
  Guid? companySysId = null,
  Guid? userId = null
)
    {
      var nombre = (nombreRaw ?? "").Trim();
      var apellidos = (apellidosRaw ?? "").Trim();

      // Si trae GUID en el nombre y apellidos viene vacío, úsalo directo
      if (!string.IsNullOrWhiteSpace(nombre) && string.IsNullOrWhiteSpace(apellidos))
      {
        if (Guid.TryParse(nombre, out var g))
          return g;
      }

      // Si no hay nada, no asignamos empleado
      if (string.IsNullOrWhiteSpace(nombre) && string.IsNullOrWhiteSpace(apellidos))
        return null;

      nombre = TrimTo(nombre, 50);
      apellidos = TrimTo(apellidos, 50);

      var company = companySysId ?? Guid.Empty; // si tienes mapping a companies, pásalo aquí
      var user = userId ?? Guid.Empty;

      // 1) Buscar existente
      object existing;
      using (var check = new SqlCommand(@"
      SELECT TOP 1 employeeSysId
      FROM dbo.employees
      WHERE name = @name AND lastName = @lastName AND companySysId = @company", cn, tx))
      {
        check.Parameters.Add("@name", SqlDbType.VarChar, 50).Value = nombre;
        check.Parameters.Add("@lastName", SqlDbType.VarChar, 50).Value = apellidos;
        check.Parameters.Add("@company", SqlDbType.UniqueIdentifier).Value = company;

        existing = check.ExecuteScalar();
      }
      if (existing != null && existing != DBNull.Value)
        return (Guid)existing;

      // 2) Crear (mínimos requeridos; el resto tiene defaults en la tabla)
      var id = Guid.NewGuid();
      using (var insert = new SqlCommand(@"
      INSERT INTO dbo.employees
        (employeeSysId, companySysId, name, lastName, entryUser, updateUser)
      VALUES
        (@id, @company, @name, @lastName, @entryUser, @updateUser);", cn, tx))
      {
        insert.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = id;
        insert.Parameters.Add("@company", SqlDbType.UniqueIdentifier).Value = company;
        insert.Parameters.Add("@name", SqlDbType.VarChar, 50).Value = nombre;
        insert.Parameters.Add("@lastName", SqlDbType.VarChar, 50).Value = apellidos;
        insert.Parameters.Add("@entryUser", SqlDbType.UniqueIdentifier).Value = user;
        insert.Parameters.Add("@updateUser", SqlDbType.UniqueIdentifier).Value = user;

        insert.ExecuteNonQuery();
      }

      return id;
    }

    private Guid? GetOrCreateCuentaContableDepreciacion(
  string nombreRaw,
  SqlConnection cn,
  SqlTransaction tx,
  Dictionary<string, Guid> cache = null
)
    {
      var nombre = (nombreRaw ?? "").Trim();
      if (string.IsNullOrWhiteSpace(nombre)) return null;

      // Si vino GUID directo
      if (Guid.TryParse(nombre, out var g)) return g;

      nombre = TrimTo(nombre, 100);

      // Cache por nombre
      if (cache != null && cache.TryGetValue(nombre, out var cachedId))
        return cachedId;

      // Buscar existente
      object existing;
      using (var check = new SqlCommand(@"
      SELECT TOP 1 ID_CUENTA_CONTABLE_DEPRESIACION
      FROM dbo.CUENTA_CONTABLE_DEPRESIACION
      WHERE NOMBRE = @nombre;", cn, tx))
      {
        check.Parameters.Add("@nombre", SqlDbType.VarChar, 100).Value = nombre;
        existing = check.ExecuteScalar();
      }
      if (existing != null && existing != DBNull.Value)
      {
        var idFound = (Guid)existing;
        cache?.TryAdd(nombre, idFound);
        return idFound;
      }

      // Crear nuevo registro (puedes ajustar DESCRIPCION si quieres que vaya null)
      var idNew = Guid.NewGuid();
      using (var insert = new SqlCommand(@"
      INSERT INTO dbo.CUENTA_CONTABLE_DEPRESIACION
        (ID_CUENTA_CONTABLE_DEPRESIACION, NOMBRE, DESCRIPCION)
      VALUES
        (@id, @nombre, @descripcion);", cn, tx))
      {
        insert.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = idNew;
        insert.Parameters.Add("@nombre", SqlDbType.VarChar, 100).Value = nombre;
        // Si prefieres null, usa DBNull.Value; aquí la dejo igual al nombre como descripción por defecto.
        insert.Parameters.Add("@descripcion", SqlDbType.VarChar, 100).Value = (object)nombre ?? DBNull.Value;

        insert.ExecuteNonQuery();
      }

      cache?.TryAdd(nombre, idNew);
      return idNew;
    }

    private Guid? GetOrCreateCentroCostos(
  string nombreRaw,
  SqlConnection cn,
  SqlTransaction tx,
  Dictionary<string, Guid> cache = null
)
    {
      var nombre = (nombreRaw ?? "").Trim();
      if (string.IsNullOrWhiteSpace(nombre)) return null;

      // Si viene GUID directo
      if (Guid.TryParse(nombre, out var g)) return g;

      nombre = TrimTo(nombre, 100);

      // Cache
      if (cache != null && cache.TryGetValue(nombre, out var cachedId))
        return cachedId;

      // Buscar existente
      object existing;
      using (var check = new SqlCommand(@"
      SELECT TOP 1 ID_CENTRO_COSTOS
      FROM dbo.CENTRO_COSTOS
      WHERE NOMBRE = @nombre;", cn, tx))
      {
        check.Parameters.Add("@nombre", SqlDbType.VarChar, 100).Value = nombre;
        existing = check.ExecuteScalar();
      }
      if (existing != null && existing != DBNull.Value)
      {
        var idFound = (Guid)existing;
        cache?.TryAdd(nombre, idFound);
        return idFound;
      }

      // Insertar nuevo
      var idNew = Guid.NewGuid();
      using (var insert = new SqlCommand(@"
      INSERT INTO dbo.CENTRO_COSTOS (ID_CENTRO_COSTOS, NOMBRE, DESCRIPCION)
      VALUES (@id, @nombre, @descripcion);", cn, tx))
      {
        insert.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = idNew;
        insert.Parameters.Add("@nombre", SqlDbType.VarChar, 100).Value = nombre;
        // Puedes dejar DESCRIPCION = null si prefieres:
        insert.Parameters.Add("@descripcion", SqlDbType.VarChar, 100).Value = (object)nombre ?? DBNull.Value;

        insert.ExecuteNonQuery();
      }

      cache?.TryAdd(nombre, idNew);
      return idNew;
    }

    private Guid? GetOrCreateUbicacionA_Company(
  string raw,
  SqlConnection cn,
  SqlTransaction tx,
  Dictionary<string, Guid> cache = null
)
    {
      var name = (raw ?? "").Trim();
      if (string.IsNullOrWhiteSpace(name)) return null;

      // ¿Vino GUID directo?
      if (Guid.TryParse(name, out var g)) return g;

      name = TrimTo(name, 50);
      var description = TrimTo(name, 150); // usamos el mismo texto

      // Cache del lote
      if (cache != null && cache.TryGetValue(name, out var cachedId))
        return cachedId;

      // Buscar existente por name
      object existing;
      using (var check = new SqlCommand(@"
      SELECT TOP 1 companySysId
      FROM dbo.companies
      WHERE name = @name;", cn, tx))
      {
        check.Parameters.Add("@name", SqlDbType.VarChar, 50).Value = name;
        existing = check.ExecuteScalar();
      }

      if (existing != null && existing != DBNull.Value)
      {
        var idFound = (Guid)existing;
        cache?.TryAdd(name, idFound);
        return idFound;
      }

      // Insertar nuevo
      var idNew = Guid.NewGuid();
      using (var insert = new SqlCommand(@"
      INSERT INTO dbo.companies
        (companySysId, [name], [description], entryUser, updateUser)
      VALUES
        (@id, @name, @desc, @entryUser, @updateUser);", cn, tx))
      {
        insert.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = idNew;
        insert.Parameters.Add("@name", SqlDbType.VarChar, 50).Value = name;
        insert.Parameters.Add("@desc", SqlDbType.VarChar, 150).Value = description;
        insert.Parameters.Add("@entryUser", SqlDbType.UniqueIdentifier).Value = Guid.Empty;
        insert.Parameters.Add("@updateUser", SqlDbType.UniqueIdentifier).Value = Guid.Empty;

        insert.ExecuteNonQuery();
      }

      cache?.TryAdd(name, idNew);
      return idNew;
    }

    private Guid? GetOrCreateUbicacionB_Building(
  string raw,
  Guid? companyId,                // Ubicación A requerida para crear/buscar B
  SqlConnection cn,
  SqlTransaction tx,
  Dictionary<string, Guid> cache  // clave compuesta "A|nombre"
)
    {
      var nameRaw = (raw ?? "").Trim();
      if (string.IsNullOrWhiteSpace(nameRaw)) return null;

      // Si vino GUID directo
      if (Guid.TryParse(nameRaw, out var g)) return g;

      if (!companyId.HasValue)
        throw new Exception($"Se indicó una Ubicación B ('{nameRaw}'), pero la Ubicación A no está definida.");

      var name = TrimTo(nameRaw, 50);
      var description = TrimTo(name, 150);

      // Cache por (companyId|name)
      var key = $"{companyId.Value}|{name}";
      if (cache != null && cache.TryGetValue(key, out var cached))
        return cached;

      // Buscar existente
      object existing;
      using (var check = new SqlCommand(@"
      SELECT TOP 1 buildingSysId
      FROM dbo.buildings
      WHERE companySysId = @company AND [name] = @name;", cn, tx))
      {
        check.Parameters.Add("@company", SqlDbType.UniqueIdentifier).Value = companyId.Value;
        check.Parameters.Add("@name", SqlDbType.VarChar, 50).Value = name;
        existing = check.ExecuteScalar();
      }

      if (existing != null && existing != DBNull.Value)
      {
        var idFound = (Guid)existing;
        cache?.TryAdd(key, idFound);
        return idFound;
      }

      // Insertar nuevo
      var idNew = Guid.NewGuid();
      using (var insert = new SqlCommand(@"
      INSERT INTO dbo.buildings
        (buildingSysId, companySysId, [name], [description], entryUser, updateUser)
      VALUES
        (@id, @company, @name, @desc, @entryUser, @updateUser);", cn, tx))
      {
        insert.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = idNew;
        insert.Parameters.Add("@company", SqlDbType.UniqueIdentifier).Value = companyId.Value;
        insert.Parameters.Add("@name", SqlDbType.VarChar, 50).Value = name;
        insert.Parameters.Add("@desc", SqlDbType.VarChar, 150).Value = description;
        insert.Parameters.Add("@entryUser", SqlDbType.UniqueIdentifier).Value = Guid.Empty;
        insert.Parameters.Add("@updateUser", SqlDbType.UniqueIdentifier).Value = Guid.Empty;

        insert.ExecuteNonQuery();
      }

      cache?.TryAdd(key, idNew);
      return idNew;
    }

    private Guid? GetOrCreateUbicacionC_Floor(
  string raw,
  Guid? companyId,                 // Ubicación A
  Guid? buildingId,                // Ubicación B
  SqlConnection cn,
  SqlTransaction tx,
  Dictionary<string, Guid> cache   // clave compuesta "A|B|nombre"
)
    {
      var nameRaw = (raw ?? "").Trim();
      if (string.IsNullOrWhiteSpace(nameRaw)) return null;

      // GUID directo
      if (Guid.TryParse(nameRaw, out var g)) return g;

      if (!companyId.HasValue || !buildingId.HasValue)
        throw new Exception($"Se indicó una Ubicación C ('{nameRaw}'), pero faltan Ubicación A y/o B.");

      var name = TrimTo(nameRaw, 50);
      var description = TrimTo(name, 150);

      // Cache por (companyId|buildingId|name)
      var key = $"{companyId.Value}|{buildingId.Value}|{name}";
      if (cache != null && cache.TryGetValue(key, out var cached))
        return cached;

      // Buscar existente
      object existing;
      using (var check = new SqlCommand(@"
      SELECT TOP 1 floorSysId
      FROM dbo.floors
      WHERE companySysId = @company AND buildingSysId = @building AND [name] = @name;", cn, tx))
      {
        check.Parameters.Add("@company", SqlDbType.UniqueIdentifier).Value = companyId.Value;
        check.Parameters.Add("@building", SqlDbType.UniqueIdentifier).Value = buildingId.Value;
        check.Parameters.Add("@name", SqlDbType.VarChar, 50).Value = name;
        existing = check.ExecuteScalar();
      }

      if (existing != null && existing != DBNull.Value)
      {
        var idFound = (Guid)existing;
        cache?.TryAdd(key, idFound);
        return idFound;
      }

      // Insertar nuevo
      var idNew = Guid.NewGuid();
      using (var insert = new SqlCommand(@"
      INSERT INTO dbo.floors
        (floorSysId, buildingSysId, companySysId, [name], [description], entryUser, updateUser)
      VALUES
        (@id, @building, @company, @name, @desc, @entryUser, @updateUser);", cn, tx))
      {
        insert.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = idNew;
        insert.Parameters.Add("@building", SqlDbType.UniqueIdentifier).Value = buildingId.Value;
        insert.Parameters.Add("@company", SqlDbType.UniqueIdentifier).Value = companyId.Value;
        insert.Parameters.Add("@name", SqlDbType.VarChar, 50).Value = name;
        insert.Parameters.Add("@desc", SqlDbType.VarChar, 150).Value = description;
        insert.Parameters.Add("@entryUser", SqlDbType.UniqueIdentifier).Value = Guid.Empty;
        insert.Parameters.Add("@updateUser", SqlDbType.UniqueIdentifier).Value = Guid.Empty;

        insert.ExecuteNonQuery();
      }

      cache?.TryAdd(key, idNew);
      return idNew;
    }

    private Guid? GetOrCreateUbicacionD_Office(
  string raw,
  Guid? companyId,               // Ubicación A
  Guid? buildingId,              // Ubicación B
  Guid? floorId,                 // Ubicación C
  SqlConnection cn,
  SqlTransaction tx,
  Dictionary<string, Guid> cache // clave compuesta "A|B|C|nombre"
)
    {
      var nameRaw = (raw ?? "").Trim();
      if (string.IsNullOrWhiteSpace(nameRaw)) return null;

      // GUID directo
      if (Guid.TryParse(nameRaw, out var g)) return g;

      // Validaciones de jerarquía
      if (!companyId.HasValue || !buildingId.HasValue || !floorId.HasValue)
        throw new Exception($"Se indicó una Ubicación D ('{nameRaw}'), pero faltan Ubicación {(!companyId.HasValue ? "A" : !buildingId.HasValue ? "B" : "C")}.");

      var name = TrimTo(nameRaw, 100);
      var description = TrimTo(nameRaw, 150);

      // Cache por (A|B|C|name)
      var key = $"{companyId.Value}|{buildingId.Value}|{floorId.Value}|{name}";
      if (cache != null && cache.TryGetValue(key, out var cached))
        return cached;

      // Buscar existente
      object existing;
      using (var check = new SqlCommand(@"
      SELECT TOP 1 officeSysId
      FROM dbo.officeses
      WHERE companySysId = @company
        AND buildingSysId = @building
        AND floorSysId = @floor
        AND [name] = @name;", cn, tx))
      {
        check.Parameters.Add("@company", SqlDbType.UniqueIdentifier).Value = companyId.Value;
        check.Parameters.Add("@building", SqlDbType.UniqueIdentifier).Value = buildingId.Value;
        check.Parameters.Add("@floor", SqlDbType.UniqueIdentifier).Value = floorId.Value;
        check.Parameters.Add("@name", SqlDbType.VarChar, 100).Value = name;
        existing = check.ExecuteScalar();
      }

      if (existing != null && existing != DBNull.Value)
      {
        var idFound = (Guid)existing;
        cache?.TryAdd(key, idFound);
        return idFound;
      }

      // Insertar nuevo (businessUnit/dept/tag = Guid.Empty; IsEnable = 1)
      var idNew = Guid.NewGuid();
      using (var insert = new SqlCommand(@"
      INSERT INTO dbo.officeses
        (officeSysId, companySysId, buildingSysId, businessUnitSysId, floorSysId, deptSysId, tagSysId,
         [name], [description], entryUser, updateUser, IsEnable)
      VALUES
        (@id, @company, @building, @bu, @floor, @dept, @tag,
         @name, @desc, @entryUser, @updateUser, @isEnable);", cn, tx))
      {
        insert.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = idNew;
        insert.Parameters.Add("@company", SqlDbType.UniqueIdentifier).Value = companyId.Value;
        insert.Parameters.Add("@building", SqlDbType.UniqueIdentifier).Value = buildingId.Value;
        insert.Parameters.Add("@floor", SqlDbType.UniqueIdentifier).Value = floorId.Value;

        insert.Parameters.Add("@bu", SqlDbType.UniqueIdentifier).Value = Guid.Empty;
        insert.Parameters.Add("@dept", SqlDbType.UniqueIdentifier).Value = Guid.Empty;
        insert.Parameters.Add("@tag", SqlDbType.UniqueIdentifier).Value = Guid.Empty;

        insert.Parameters.Add("@name", SqlDbType.VarChar, 100).Value = name;
        insert.Parameters.Add("@desc", SqlDbType.VarChar, 150).Value = description;

        insert.Parameters.Add("@entryUser", SqlDbType.UniqueIdentifier).Value = Guid.Empty;
        insert.Parameters.Add("@updateUser", SqlDbType.UniqueIdentifier).Value = Guid.Empty;
        insert.Parameters.Add("@isEnable", SqlDbType.Bit).Value = true;

        insert.ExecuteNonQuery();
      }

      cache?.TryAdd(key, idNew);
      return idNew;
    }

    private Guid? GetOrCreateUbicacionSecundaria(
  string raw,
  SqlConnection cn,
  SqlTransaction tx,
  Dictionary<string, Guid> cache // clave: nombre
)
    {
      var nameRaw = (raw ?? "").Trim();
      if (string.IsNullOrWhiteSpace(nameRaw)) return null;

      // Si viene GUID directo
      if (Guid.TryParse(nameRaw, out var g)) return g;

      var name = TrimTo(nameRaw, 100);
      var description = TrimTo(nameRaw, 100);

      // Cache por nombre
      if (cache != null && cache.TryGetValue(name, out var cached))
        return cached;

      // Buscar existente por NOMBRE
      object existing;
      using (var check = new SqlCommand(@"
      SELECT TOP 1 ID_UBICACION_SECUNDARIA
      FROM dbo.UBICACION_SECUNDARIA
      WHERE NOMBRE = @name;", cn, tx))
      {
        check.Parameters.Add("@name", SqlDbType.VarChar, 100).Value = name;
        existing = check.ExecuteScalar();
      }

      if (existing != null && existing != DBNull.Value)
      {
        var idFound = (Guid)existing;
        cache?.TryAdd(name, idFound);
        return idFound;
      }

      // Insertar nuevo
      var idNew = Guid.NewGuid();
      using (var insert = new SqlCommand(@"
      INSERT INTO dbo.UBICACION_SECUNDARIA
      (ID_UBICACION_SECUNDARIA, NOMBRE, DESCRIPCION)
      VALUES
      (@id, @name, @desc);", cn, tx))
      {
        insert.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = idNew;
        insert.Parameters.Add("@name", SqlDbType.VarChar, 100).Value = name;
        insert.Parameters.Add("@desc", SqlDbType.VarChar, 100).Value = description;
        insert.ExecuteNonQuery();
      }

      cache?.TryAdd(name, idNew);
      return idNew;
    }



    // ==== ACCIÓN FINAL ====
    [HttpPost]
    public IActionResult ImportarSeleccion([FromBody] ImportRequest req)
    {
      if (req?.Items == null || req.Items.Count == 0)
      {
        return Json(new
        {
          ok = false,
          inserted = 0,
          skippedDuplicates = 0,
          errors = new[] { new { row = 0, excelRow = 0, message = "No se recibieron filas." } }
        });
      }

      string connectionString =
        System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

      // Duplicados dentro del lote (por NUMERO_ACTIVO)
      var batchSeen = new HashSet<int>();
      var firstSeenExcelRow = new Dictionary<int, int>(); // numActivo -> fila Excel primera aparición

      // Duplicados dentro del lote (por NUMERO_ETIQUETA, ignorando may/min)
      var batchSeenEtiqueta = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
      var firstSeenEtiquetaRow = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

      using (var cn = new SqlConnection(connectionString))
      {
        cn.Open();
        using (var tx = cn.BeginTransaction())
        {
          int inserted = 0;
          int skipped = 0;
          var errors = new List<object>();

          // Chequeo existencia en BD por NUMERO_ACTIVO
          var checkCmd = new SqlCommand(
            "SELECT COUNT(1) FROM dbo.ActivosSignusID WHERE NUMERO_ACTIVO = @num", cn, tx);
          checkCmd.Parameters.Add("@num", SqlDbType.Int);

          // Chequeo existencia en BD por NUMERO_ETIQUETA
          var checkTagCmd = new SqlCommand(
            "SELECT COUNT(1) FROM dbo.ActivosSignusID WHERE NUMERO_ETIQUETA = @tag", cn, tx);
          checkTagCmd.Parameters.Add("@tag", SqlDbType.VarChar, 100);

          // Insert
          var sql = @"
INSERT INTO dbo.ActivosSignusID
(
    ID_ACTIVO,
    NUMERO_ACTIVO,
    NUMERO_ETIQUETA,
    DESCRIPCION_CORTA,
    DESCRIPCION_LARGA,
    CATEGORIA,
    ESTADO,
    EMPRESA,
    MARCA,
    MODELO,
    NUMERO_SERIE,
    COSTO,
    NUMERO_FACTURA,
    FECHA_COMPRA,
    FECHA_CAPITALIZACION,
    VALOR_RESIDUAL,
    DOCUMENTO,
    FOTOS,
    NUMERO_PARTE_FABRICANTE,
    DEPRECIADO,
    DESCRIPCION_DEPRECIADO,
    ANOS_VIDA_UTIL,
    CUENTA_CONTABLE_DEPRESIACION,
    CENTRO_COSTOS,
    DESCRIPCION_ESTADO_ULTIMO_INVENTARIO,
    TAG_EPC,
    EMPLEADO,
    UBICACION_A,
    UBICACION_B,
    UBICACION_C,
    UBICACION_D,
    UBICACION_SECUNDARIA,
    FECHA_GARANTIA,
    COLOR,
    TAMANIO_MEDIDA,
    OBSERVACIONES,
    ESTADO_ACTIVO
)
VALUES
(
    @ID_ACTIVO,
    @NUMERO_ACTIVO,
    @NUMERO_ETIQUETA,
    @DESCRIPCION_CORTA,
    @DESCRIPCION_LARGA,
    @CATEGORIA,
    @ESTADO,
    @EMPRESA,
    @MARCA,
    @MODELO,
    @NUMERO_SERIE,
    @COSTO,
    @NUMERO_FACTURA,
    @FECHA_COMPRA,
    @FECHA_CAPITALIZACION,
    @VALOR_RESIDUAL,
    @DOCUMENTO,
    @FOTOS,
    @NUMERO_PARTE_FABRICANTE,
    @DEPRECIADO,
    @DESCRIPCION_DEPRECIADO,
    @ANOS_VIDA_UTIL,
    @CUENTA_CONTABLE_DEPRESIACION,
    @CENTRO_COSTOS,
    @DESCRIPCION_ESTADO_ULTIMO_INVENTARIO,
    @TAG_EPC,
    @EMPLEADO,
    @UBICACION_A,
    @UBICACION_B,
    @UBICACION_C,
    @UBICACION_D,
    @UBICACION_SECUNDARIA,
    @FECHA_GARANTIA,
    @COLOR,
    @TAMANIO_MEDIDA,
    @OBSERVACIONES,
    @ESTADO_ACTIVO
);";

          var cmd = new SqlCommand(sql, cn, tx);

          // Parámetros
          cmd.Parameters.Add("@ID_ACTIVO", SqlDbType.UniqueIdentifier);
          cmd.Parameters.Add("@NUMERO_ACTIVO", SqlDbType.Int);
          cmd.Parameters.Add("@NUMERO_ETIQUETA", SqlDbType.VarChar, 100);
          cmd.Parameters.Add("@DESCRIPCION_CORTA", SqlDbType.VarChar, 100);
          cmd.Parameters.Add("@DESCRIPCION_LARGA", SqlDbType.VarChar, 200);
          cmd.Parameters.Add("@CATEGORIA", SqlDbType.UniqueIdentifier);
          cmd.Parameters.Add("@ESTADO", SqlDbType.UniqueIdentifier);
          cmd.Parameters.Add("@EMPRESA", SqlDbType.UniqueIdentifier);
          cmd.Parameters.Add("@MARCA", SqlDbType.UniqueIdentifier);
          cmd.Parameters.Add("@MODELO", SqlDbType.UniqueIdentifier);
          cmd.Parameters.Add("@NUMERO_SERIE", SqlDbType.VarChar, 100);
          cmd.Parameters.Add("@COSTO", SqlDbType.Float);
          cmd.Parameters.Add("@NUMERO_FACTURA", SqlDbType.VarChar, 100);
          cmd.Parameters.Add("@FECHA_COMPRA", SqlDbType.DateTime);
          cmd.Parameters.Add("@FECHA_CAPITALIZACION", SqlDbType.DateTime);
          cmd.Parameters.Add("@VALOR_RESIDUAL", SqlDbType.Float);
          cmd.Parameters.Add("@DOCUMENTO", SqlDbType.UniqueIdentifier);
          cmd.Parameters.Add("@FOTOS", SqlDbType.UniqueIdentifier);
          cmd.Parameters.Add("@NUMERO_PARTE_FABRICANTE", SqlDbType.VarChar, 100);
          cmd.Parameters.Add("@DEPRECIADO", SqlDbType.VarChar, 100);
          cmd.Parameters.Add("@DESCRIPCION_DEPRECIADO", SqlDbType.VarChar, 100);
          cmd.Parameters.Add("@ANOS_VIDA_UTIL", SqlDbType.Int);
          cmd.Parameters.Add("@CUENTA_CONTABLE_DEPRESIACION", SqlDbType.UniqueIdentifier);
          cmd.Parameters.Add("@CENTRO_COSTOS", SqlDbType.UniqueIdentifier);
          cmd.Parameters.Add("@DESCRIPCION_ESTADO_ULTIMO_INVENTARIO", SqlDbType.VarChar, 100);
          cmd.Parameters.Add("@TAG_EPC", SqlDbType.VarChar, 100);
          cmd.Parameters.Add("@EMPLEADO", SqlDbType.UniqueIdentifier);
          cmd.Parameters.Add("@UBICACION_A", SqlDbType.UniqueIdentifier);
          cmd.Parameters.Add("@UBICACION_B", SqlDbType.UniqueIdentifier);
          cmd.Parameters.Add("@UBICACION_C", SqlDbType.UniqueIdentifier);
          cmd.Parameters.Add("@UBICACION_D", SqlDbType.UniqueIdentifier);
          cmd.Parameters.Add("@UBICACION_SECUNDARIA", SqlDbType.UniqueIdentifier);
          cmd.Parameters.Add("@FECHA_GARANTIA", SqlDbType.DateTime);
          cmd.Parameters.Add("@COLOR", SqlDbType.VarChar, 100);
          cmd.Parameters.Add("@TAMANIO_MEDIDA", SqlDbType.VarChar, 100);
          cmd.Parameters.Add("@OBSERVACIONES", SqlDbType.VarChar, 400);
          cmd.Parameters.Add("@ESTADO_ACTIVO", SqlDbType.Bit);

          var cacheCuentaContable = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
          var cacheCentroCostos = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
          var cacheUbicA = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
          var cacheUbicB = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
          var cacheUbicC = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
          var cacheUbicD = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
          var cacheUbicSec = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);

          for (int i = 0; i < req.Items.Count; i++)
          {
            var row = req.Items[i];
            var excelRow = 3 + i; // (1 encabezado, 2 guía)

            try
            {
              // === Títulos visibles del payload ===
              var numActivoStr = GetVal(row, "Número de Activo");
              var numEtiqueta = GetVal(row, "Número de Etiqueta");
              var descCorta = GetVal(row, "Descripción Corta");
              var descLarga = GetVal(row, "Descripción Larga");
              var categoriaStr = GetVal(row, "Categoría");
              var categoriaStrDesc = GetVal(row, "Descripción de Categoría");
              var estadoStr = GetVal(row, "Estado");
              var estadoStrDesc = GetVal(row, "Descripción de Estado");
              var empresaStr = GetVal(row, "Empresa");
              var marcaStr = GetVal(row, "Marca");
              var modeloStr = GetVal(row, "Modelo");
              var numSerie = GetVal(row, "Número Serie");
              var costoStr = GetVal(row, "Costo");
              var numFactura = GetVal(row, "Número Factura");
              var fechaCompraStr = GetVal(row, "Fecha Compra");
              var fechaCapStr = GetVal(row, "Fecha Capitalización");
              var valorResidualStr = GetVal(row, "Valor Residual");
              var numParte = GetVal(row, "Número de Parte del Fabricante");
              var depreciado = GetVal(row, "Depreciado");
              var descDepreciado = GetVal(row, "Descripción Depreciado");
              var anosVidaStr = GetVal(row, "Años de Vida Útil");
              var cuentaContableStr = GetVal(row, "Cuenta Contable Depreciación");
              var centroCostosStr = GetVal(row, "Centro Costos");
              var descEstadoInv = GetVal(row, "Descripción del Estado del Último Inventario");
              var tagEpc = GetVal(row, "Tag EPC");
              var empleadoStr = GetVal(row, "Empleado");
              var empleadoStrApellidos = GetVal(row, "Apellidos del Empleado");
              var ubicA = GetVal(row, "Ubicación A");
              var ubicB = GetVal(row, "Ubicación B");
              var ubicC = GetVal(row, "Ubicación C");
              var ubicD = GetVal(row, "Ubicación D");
              var ubicSec = GetVal(row, "Ubicación Secundaria");
              var fechaGarantiaStr = GetVal(row, "Fecha Garantía");
              var color = GetVal(row, "Color");

              // Lista de colores permitidos
              var coloresPermitidos = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
              {
                  "Negro", "Blanco", "Gris", "Rojo", "Azul", "Verde",
                  "Amarillo", "Anaranjado", "Beige", "Plateado", "Dorado", "Café"
              };

              // Si el color no es válido o está vacío, usar "Negro"
              if (string.IsNullOrWhiteSpace(color) || !coloresPermitidos.Contains(color))
              {
                color = "Negro";
              }
              var tamanioMedida = GetVal(row, "Tamaño/Medida");
              var observaciones = GetVal(row, "Observaciones");
              var estadoActivoStr = GetVal(row, "Estado del Activo");

              // === Conversiones básicas ===
              int? numActivo = ToInt(numActivoStr);
              if (numActivo == null)
                throw new Exception("Número de Activo inválido o vacío.");

              // --- Duplicados por NUMERO_ACTIVO (lote) ---
              if (!batchSeen.Add(numActivo.Value))
              {
                skipped++;
                var prevExcelRow = firstSeenExcelRow.TryGetValue(numActivo.Value, out var prev) ? prev : -1;
                errors.Add(new { row = i + 1, excelRow, message = $"Duplicado en el archivo: NUMERO_ACTIVO {numActivo} ya se usó en la fila {prevExcelRow}." });
                continue;
              }
              else
              {
                firstSeenExcelRow[numActivo.Value] = excelRow;
              }

              // --- Duplicado en BD por NUMERO_ACTIVO ---
              checkCmd.Parameters["@num"].Value = numActivo.Value;
              var exists = (int)checkCmd.ExecuteScalar() > 0;
              if (exists)
              {
                skipped++;
                errors.Add(new { row = i + 1, excelRow, message = $"Duplicado en BD: NUMERO_ACTIVO {numActivo} ya existe." });
                continue;
              }

              // --- Duplicados por NUMERO_ETIQUETA (si trae valor) ---
              var etiquetaKey = (numEtiqueta ?? string.Empty).Trim();
              if (!string.IsNullOrEmpty(etiquetaKey))
              {
                // Lote
                if (!batchSeenEtiqueta.Add(etiquetaKey))
                {
                  skipped++;
                  var prevRow = firstSeenEtiquetaRow.TryGetValue(etiquetaKey, out var prev) ? prev : -1;
                  errors.Add(new { row = i + 1, excelRow, message = $"Duplicado en el archivo: NUMERO_ETIQUETA '{etiquetaKey}' ya se usó en la fila {prev}." });
                  continue;
                }
                else
                {
                  firstSeenEtiquetaRow[etiquetaKey] = excelRow;
                }

                // BD
                checkTagCmd.Parameters["@tag"].Value = etiquetaKey;
                var tagExists = (int)checkTagCmd.ExecuteScalar() > 0;
                if (tagExists)
                {
                  skipped++;
                  errors.Add(new { row = i + 1, excelRow, message = $"Duplicado en BD: NUMERO_ETIQUETA '{etiquetaKey}' ya existe." });
                  continue;
                }
              }

              // ---- resto de conversiones / resoluciones (igual que ya tenías) ----
              double? costo = ToDouble(costoStr);
              double? valResidual = ToDouble(valorResidualStr);
              int? anosVida = ToInt(anosVidaStr);
              bool estadoActivo = ToBool(estadoActivoStr) ?? false;

              DateTime? fCompra = ParseDate_ddMMyyyy(fechaCompraStr);
              DateTime? fCap = ParseDate_ddMMyyyy(fechaCapStr);
              DateTime? fGarantia = ParseDate_ddMMyyyy(fechaGarantiaStr);

              Guid? categoria = ToGuid(categoriaStr) ?? GetOrCreateAssetCategory(categoriaStr, categoriaStrDesc, cn, tx);
              Guid? estadoG = ToGuid(estadoStr) ?? GetOrCreateAssetStatus(estadoStr, estadoStrDesc, cn, tx);
              Guid? empresa = ToGuid(empresaStr) ?? GetOrCreateEmpresa(empresaStr, cn, tx);
              Guid? marca = ToGuid(marcaStr) ?? GetOrCreateMarca(marcaStr, cn, tx);
              Guid? modelo = ToGuid(modeloStr) ?? GetOrCreateModelo(modeloStr, marca, cn, tx);

              Guid? empleado = null;
              if (!string.IsNullOrWhiteSpace(empleadoStr) && string.IsNullOrWhiteSpace(empleadoStrApellidos)
                  && Guid.TryParse(empleadoStr.Trim(), out var empGuid))
                empleado = empGuid;
              else
                empleado = GetOrCreateEmpleadoByNames(empleadoStr, empleadoStrApellidos, cn, tx, null, null);

              Guid? cuentaContable = ToGuid(cuentaContableStr)
                ?? GetOrCreateCuentaContableDepreciacion(cuentaContableStr, cn, tx, cacheCuentaContable);

              Guid? centroCostos = ToGuid(centroCostosStr)
                ?? GetOrCreateCentroCostos(centroCostosStr, cn, tx, cacheCentroCostos);

              Guid? ubA = ToGuid(ubicA) ?? GetOrCreateUbicacionA_Company(ubicA, cn, tx, cacheUbicA);

              Guid? ubB = ToGuid(ubicB);
              if (!ubB.HasValue)
              {
                if (!string.IsNullOrWhiteSpace(ubicB) && !ubA.HasValue)
                  throw new Exception($"Se indicó una Ubicación B ('{ubicB}'), pero la Ubicación A no está definida.");
                ubB = GetOrCreateUbicacionB_Building(ubicB, ubA, cn, tx, cacheUbicB);
              }

              Guid? ubC = ToGuid(ubicC);
              if (!ubC.HasValue)
              {
                if (!string.IsNullOrWhiteSpace(ubicC) && (!ubA.HasValue || !ubB.HasValue))
                  throw new Exception($"Se indicó una Ubicación C ('{ubicC}'), pero falta definir Ubicación {(!ubA.HasValue ? "A" : "B")}.");
                ubC = GetOrCreateUbicacionC_Floor(ubicC, ubA, ubB, cn, tx, cacheUbicC);
              }

              Guid? ubD = ToGuid(ubicD);
              if (!ubD.HasValue)
              {
                if (!string.IsNullOrWhiteSpace(ubicD))
                {
                  if (!ubA.HasValue) throw new Exception($"Se indicó una Ubicación D ('{ubicD}'), pero falta definir Ubicación A.");
                  if (!ubB.HasValue) throw new Exception($"Se indicó una Ubicación D ('{ubicD}'), pero falta definir Ubicación B.");
                  if (!ubC.HasValue) throw new Exception($"Se indicó una Ubicación D ('{ubicD}'), pero falta definir Ubicación C.");
                }
                ubD = GetOrCreateUbicacionD_Office(ubicD, ubA, ubB, ubC, cn, tx, cacheUbicD);
              }

              Guid? ubSec = ToGuid(ubicSec) ?? GetOrCreateUbicacionSecundaria(ubicSec, cn, tx, cacheUbicSec);

              Guid? documento = null;
              Guid? fotos = null;

              // Parámetros
              cmd.Parameters["@ID_ACTIVO"].Value = Guid.NewGuid();
              cmd.Parameters["@NUMERO_ACTIVO"].Value = (object)numActivo ?? DBNull.Value;
              // guardar NULL cuando venga vacío/espacios
              cmd.Parameters["@NUMERO_ETIQUETA"].Value = string.IsNullOrWhiteSpace(numEtiqueta) ? (object)DBNull.Value : numEtiqueta;

              cmd.Parameters["@DESCRIPCION_CORTA"].Value = (object)descCorta ?? DBNull.Value;
              cmd.Parameters["@DESCRIPCION_LARGA"].Value = (object)descLarga ?? DBNull.Value;

              cmd.Parameters["@CATEGORIA"].Value = (object)categoria ?? DBNull.Value;
              cmd.Parameters["@ESTADO"].Value = (object)estadoG ?? DBNull.Value;
              cmd.Parameters["@EMPRESA"].Value = (object)empresa ?? DBNull.Value;
              cmd.Parameters["@MARCA"].Value = (object)marca ?? DBNull.Value;
              cmd.Parameters["@MODELO"].Value = (object)modelo ?? DBNull.Value;

              cmd.Parameters["@NUMERO_SERIE"].Value = (object)numSerie ?? DBNull.Value;
              cmd.Parameters["@COSTO"].Value = (object)costo ?? DBNull.Value;
              cmd.Parameters["@NUMERO_FACTURA"].Value = (object)numFactura ?? DBNull.Value;

              cmd.Parameters["@FECHA_COMPRA"].Value = (object)fCompra ?? DBNull.Value;
              cmd.Parameters["@FECHA_CAPITALIZACION"].Value = (object)fCap ?? DBNull.Value;

              cmd.Parameters["@VALOR_RESIDUAL"].Value = (object)valResidual ?? DBNull.Value;
              cmd.Parameters["@DOCUMENTO"].Value = (object)documento ?? DBNull.Value;
              cmd.Parameters["@FOTOS"].Value = (object)fotos ?? DBNull.Value;

              cmd.Parameters["@NUMERO_PARTE_FABRICANTE"].Value = (object)numParte ?? DBNull.Value;
              cmd.Parameters["@DEPRECIADO"].Value = (object)depreciado ?? DBNull.Value;
              cmd.Parameters["@DESCRIPCION_DEPRECIADO"].Value = (object)descDepreciado ?? DBNull.Value;

              cmd.Parameters["@ANOS_VIDA_UTIL"].Value = (object)anosVida ?? DBNull.Value;
              cmd.Parameters["@CUENTA_CONTABLE_DEPRESIACION"].Value = (object)cuentaContable ?? DBNull.Value;
              cmd.Parameters["@CENTRO_COSTOS"].Value = (object)centroCostos ?? DBNull.Value;

              cmd.Parameters["@DESCRIPCION_ESTADO_ULTIMO_INVENTARIO"].Value = (object)descEstadoInv ?? DBNull.Value;
              cmd.Parameters["@TAG_EPC"].Value = (object)tagEpc ?? DBNull.Value;

              cmd.Parameters["@EMPLEADO"].Value = (object)empleado ?? DBNull.Value;
              cmd.Parameters["@UBICACION_A"].Value = (object)ubA ?? DBNull.Value;
              cmd.Parameters["@UBICACION_B"].Value = (object)ubB ?? DBNull.Value;
              cmd.Parameters["@UBICACION_C"].Value = (object)ubC ?? DBNull.Value;
              cmd.Parameters["@UBICACION_D"].Value = (object)ubD ?? DBNull.Value;
              cmd.Parameters["@UBICACION_SECUNDARIA"].Value = (object)ubSec ?? DBNull.Value;

              cmd.Parameters["@FECHA_GARANTIA"].Value = (object)fGarantia ?? DBNull.Value;
              cmd.Parameters["@COLOR"].Value = color ?? (object)DBNull.Value;
              cmd.Parameters["@TAMANIO_MEDIDA"].Value = (object)tamanioMedida ?? DBNull.Value;
              cmd.Parameters["@OBSERVACIONES"].Value = (object)observaciones ?? DBNull.Value;

              cmd.Parameters["@ESTADO_ACTIVO"].Value = estadoActivo;

              cmd.ExecuteNonQuery();
              inserted++;
            }
            catch (Exception ex)
            {
              errors.Add(new { row = i + 1, excelRow, message = ex.Message });
            }
          }

          tx.Commit();

          string status;
          bool ok;

          int realErrorCount = errors.Count(e =>
          {
            var msg = (string)e.GetType().GetProperty("message").GetValue(e, null);
            if (string.IsNullOrWhiteSpace(msg)) return true;
            var low = msg.ToLowerInvariant();
            // No cuentan como "error real" los avisos de duplicado
            return !(low.Contains("duplicado en bd") || low.Contains("duplicado en el archivo"));
          });

          if (inserted > 0 && realErrorCount == 0 && skipped == 0)
          {
            status = "success";
            ok = true;
          }
          else if (inserted > 0)
          {
            status = "partial";
            ok = true;
          }
          else if (skipped > 0 && realErrorCount == 0)
          {
            status = "duplicate";
            ok = false;
          }
          else
          {
            status = "error";
            ok = false;
          }

          return Json(new
          {
            ok,
            status,                   // <- NUEVO
            inserted,
            skippedDuplicates = skipped,
            errors
          });
        }
      }
    }


    //*************************************************************************************************************
    //***************************************************Estados***************************************************
    //*************************************************************************************************************
    public IActionResult Estados(string search, int page = 1, int pageSize = 20, string sortColumn = "name", string sortDirection = "asc", string hasAssets = "")
    {
      // Depuración
      Console.WriteLine($"Search Query: {search}");
      // Crear y llenar una lista de EstadosActivos con datos de prueba
      var estadosActivos = new List<EstadosActivos>
    {
        new EstadosActivos { assetStatusSysId = new Guid(), name = "Activo", description = "El activo se encuentra en uso.", assignatedAssets = 1 },
        //new EstadosActivos { assetStatusSysId = 2, name = "Mantenimiento", description = "El activo está en mantenimiento.", assignatedAssets = 2 },
        //new EstadosActivos { assetStatusSysId = 3, name = "Inactivo", description = "El activo no está en uso.", assignatedAssets = 0 },
        //new EstadosActivos { assetStatusSysId = 4, name = "Retirado", description = "El activo ha sido retirado del inventario.", assignatedAssets = 5 },
        //new EstadosActivos { assetStatusSysId = 5, name = "Nuevo", description = "Activo fijo nuevo sin asignar o ubicar dentro de la empresa", assignatedAssets = 1 },
        //new EstadosActivos { assetStatusSysId = 6, name = "En uso", description = "Activo fijo que esta en buen estado y en uso dentro o fuera de la organización.", assignatedAssets = 2 },
        //new EstadosActivos { assetStatusSysId = 7, name = "Sin uso", description = "Activo fijo que está en buen estado pero no se encuentra en uso, almacenado en una bodega o estante.", assignatedAssets = 3 },
        //new EstadosActivos { assetStatusSysId = 8, name = "Dañado", description = "Activo fijo, en uso o no, que se encuentra en mal estado y debe ser reparado o reacondicionado.", assignatedAssets = 0 },
        //new EstadosActivos { assetStatusSysId = 9, name = "Destruido", description = "Activo fijo que fue dado de baja por la organización y se procedió con su destrucción.", assignatedAssets = 5 },
        //new EstadosActivos { assetStatusSysId = 10, name = "Donado", description = "Activo fijo que está en buen o mal estado y que fue donado a otra organización.", assignatedAssets = 5 },
        //new EstadosActivos { assetStatusSysId = 11, name = "Disponible", description = "Activo disponible para asignar o uso.", assignatedAssets = 0 },
        //new EstadosActivos { assetStatusSysId = 12, name = "Asignado", description = "Activo que ya ha sido asignado a un empleado o proyecto.", assignatedAssets = 1 },
        //new EstadosActivos { assetStatusSysId = 13, name = "Obsoleto", description = "Activo que ha quedado obsoleto y no se usa.", assignatedAssets = 0 },
        //new EstadosActivos { assetStatusSysId = 14, name = "Reparación", description = "Activo que está en reparación.", assignatedAssets = 2 },
        //new EstadosActivos { assetStatusSysId = 15, name = "Sustituido", description = "Activo que ha sido reemplazado por otro.", assignatedAssets = 3 },
        //new EstadosActivos { assetStatusSysId = 16, name = "Pendiente", description = "Activo que está pendiente de asignación o reparación.", assignatedAssets = 0 },
        //new EstadosActivos { assetStatusSysId = 17, name = "Descontinuado", description = "Activo que ya no se encuentra en producción o venta.", assignatedAssets = 0 },
        //new EstadosActivos { assetStatusSysId = 18, name = "En inventario", description = "Activo que está en inventario pero no asignado.", assignatedAssets = 1 },
        //new EstadosActivos { assetStatusSysId = 19, name = "Devolución", description = "Activo que ha sido devuelto después de su uso o préstamo.", assignatedAssets = 1 },
        //new EstadosActivos { assetStatusSysId = 20, name = "Préstamo", description = "Activo que ha sido prestado a otra persona o departamento.", assignatedAssets = 1 },
        //new EstadosActivos { assetStatusSysId = 21, name = "Archivado", description = "Activo que ha sido archivado y no se utiliza.", assignatedAssets = 0 },
        //new EstadosActivos { assetStatusSysId = 22, name = "Obsoleto (Temporal)", description = "Activo obsoleto pero aún en uso temporalmente.", assignatedAssets = 1 },
        //new EstadosActivos { assetStatusSysId = 23, name = "Reemplazo", description = "Activo utilizado como reemplazo temporal mientras se realiza mantenimiento.", assignatedAssets = 2 },
        //new EstadosActivos { assetStatusSysId = 24, name = "Ajustado", description = "Activo que ha sido ajustado o modificado para mejorar su funcionamiento.", assignatedAssets = 1 },
        //new EstadosActivos { assetStatusSysId = 25, name = "Evaluación", description = "Activo en proceso de evaluación para decidir su destino.", assignatedAssets = 0 },
        //new EstadosActivos { assetStatusSysId = 26, name = "Desmontado", description = "Activo que ha sido desmontado para su reparación o desecho.", assignatedAssets = 1 },
        //new EstadosActivos { assetStatusSysId = 27, name = "Restaurado", description = "Activo que ha sido restaurado a su estado funcional.", assignatedAssets = 1 },
        //new EstadosActivos { assetStatusSysId = 28, name = "Distribuido", description = "Activo distribuido a otro sitio o ubicación.", assignatedAssets = 2 },
        //new EstadosActivos { assetStatusSysId = 29, name = "Verificado", description = "Activo que ha sido verificado en cuanto a su estado y funcionamiento.", assignatedAssets = 1 },
        //new EstadosActivos { assetStatusSysId = 30, name = "Alquilado", description = "Activo alquilado a otra persona o empresa.", assignatedAssets = 0 },
        //new EstadosActivos { assetStatusSysId = 31, name = "Reciclado", description = "Activo que ha sido reciclado para su reutilización.", assignatedAssets = 1 },
        //new EstadosActivos { assetStatusSysId = 32, name = "Traslado", description = "Activo en proceso de traslado a otra ubicación.", assignatedAssets = 3 },
        //new EstadosActivos { assetStatusSysId = 33, name = "Asignado a proyecto", description = "Activo asignado a un proyecto específico.", assignatedAssets = 2 },
        //new EstadosActivos { assetStatusSysId = 34, name = "Prueba", description = "Activo en prueba para evaluar su funcionamiento.", assignatedAssets = 1 },
        //new EstadosActivos { assetStatusSysId = 35, name = "Suministrado", description = "Activo suministrado por otro proveedor o institución.", assignatedAssets = 2 },
        //new EstadosActivos { assetStatusSysId = 36, name = "Exceso", description = "Activo que es excesivo y no se requiere más.", assignatedAssets = 0 },
        //new EstadosActivos { assetStatusSysId = 37, name = "Bajo rendimiento", description = "Activo que está en funcionamiento pero tiene un bajo rendimiento.", assignatedAssets = 1 },
        //new EstadosActivos { assetStatusSysId = 38, name = "Mejorado", description = "Activo que ha sido mejorado o modificado para mayor eficiencia.", assignatedAssets = 2 },
        //new EstadosActivos { assetStatusSysId = 39, name = "Corrupto", description = "Activo que está dañado o corrupto y no puede ser utilizado.", assignatedAssets = 0 },
        //new EstadosActivos { assetStatusSysId = 40, name = "En tránsito", description = "Activo que está en tránsito hacia su ubicación final.", assignatedAssets = 1 },
        //new EstadosActivos { assetStatusSysId = 41, name = "Instalado", description = "Activo que ha sido instalado y está en funcionamiento.", assignatedAssets = 2 },
        //new EstadosActivos { assetStatusSysId = 42, name = "Desmontaje", description = "Activo en proceso de desmontaje para su reparación o reciclaje.", assignatedAssets = 1 },
        //new EstadosActivos { assetStatusSysId = 43, name = "Retiro programado", description = "Activo que está programado para ser retirado o desechado.", assignatedAssets = 1 },
        //new EstadosActivos { assetStatusSysId = 44, name = "Desactivado", description = "Activo que ha sido desactivado pero no retirado.", assignatedAssets = 2 },
        //new EstadosActivos { assetStatusSysId = 45, name = "Restablecido", description = "Activo que ha sido restablecido a su estado anterior después de un fallo.", assignatedAssets = 3 },
        //new EstadosActivos { assetStatusSysId = 46, name = "Precaución", description = "Activo que está en uso pero requiere precaución en su manejo.", assignatedAssets = 1 },
        //new EstadosActivos { assetStatusSysId = 47, name = "Observación", description = "Activo que está en observación para determinar su condición.", assignatedAssets = 0 },
        //new EstadosActivos { assetStatusSysId = 48, name = "Evaluado", description = "Activo que ha sido evaluado y se ha determinado su estado y valor.", assignatedAssets = 1 },
        //new EstadosActivos { assetStatusSysId = 49, name = "En inspección", description = "Activo que está siendo inspeccionado para verificar su funcionamiento.", assignatedAssets = 2 },
        //new EstadosActivos { assetStatusSysId = 50, name = "Autorizado", description = "Activo que ha sido autorizado para su uso o distribución.", assignatedAssets = 3 },
        //new EstadosActivos { assetStatusSysId = 51, name = "Certificado", description = "Activo que ha sido certificado para su uso o calidad.", assignatedAssets = 1 },
        //new EstadosActivos { assetStatusSysId = 52, name = "Operacional", description = "Activo que está en pleno funcionamiento y disponible.", assignatedAssets = 2 },
        //new EstadosActivos { assetStatusSysId = 53, name = "Por revisar", description = "Activo que está pendiente de revisión para determinar su estado.", assignatedAssets = 0 },
        //new EstadosActivos { assetStatusSysId = 54, name = "De baja", description = "Activo que ha sido dado de baja y ya no es funcional.", assignatedAssets = 0 },
        //new EstadosActivos { assetStatusSysId = 55, name = "En proceso", description = "Activo que está en proceso de asignación o disposición.", assignatedAssets = 1 },
        //new EstadosActivos { assetStatusSysId = 56, name = "Contratado", description = "Activo relacionado con un contrato de prestación de servicios.", assignatedAssets = 2 },
        //new EstadosActivos { assetStatusSysId = 57, name = "Baja programada", description = "Activo cuyo retiro está programado para una fecha futura.", assignatedAssets = 0 },
        //new EstadosActivos { assetStatusSysId = 58, name = "Vendido", description = "Activo que ha sido vendido y ya no forma parte de la empresa.", assignatedAssets = 0 },
        //new EstadosActivos { assetStatusSysId = 59, name = "Rentado", description = "Activo rentado por la empresa para uso temporal.", assignatedAssets = 1 },
        //new EstadosActivos { assetStatusSysId = 60, name = "Zobsoletos", description = "ZVarios activos que han quedado obsoletos de forma masiva.", assignatedAssets = 0 },
      };

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

      return View(model);
    }

    private static List<EstadosActivos> _statesList = new List<EstadosActivos>();
    private static int _nextStatusId = 1; // Contador global para los IDs

    //[HttpPost]
    //public IActionResult AgregarEstado(EstadosActivos model)
    //{


    //  // Validar el modelo
    //  if (ModelState.IsValid)
    //  {
    //    //model.assetStatusSysId = _nextStatusId++; // Asigna un ID único al rol
    //    _statesList.Add(model);

    //    // Devolver respuesta en formato JSON
    //    return Json(new { success = true, message = "Estado agregado exitosamente", states = _statesList });
    //  }

    //  return Json(new { success = false, message = "Datos inválidos" });
    //}

    private static List<EstadosActivos> _statesListEdit = new List<EstadosActivos>();
    private static int _nextStatusIdEdit = 1; // Contador global para los IDs

    [HttpPost]
    public IActionResult EditarEstado(EstadosActivos model)
    {


      // Validar el modelo
      if (ModelState.IsValid)
      {
        //model.assetStatusSysId = _nextStatusIdEdit++; // Asigna un ID único al rol
        _statesListEdit.Add(model);

        // Devolver respuesta en formato JSON
        return Json(new { success = true, message = "Estado editado exitosamente", statesE = _statesListEdit });
      }

      return Json(new { success = false, message = "Datos inválidos" });
    }
    //*************************************************************************************************************
    //***************************************************FIN Estados***********************************************
    //*************************************************************************************************************


    //*************************************************************************************************************
    //***************************************************Modelos***************************************************
    //*************************************************************************************************************
    //    public List<ModelosActivos> modelosActivos = new List<ModelosActivos>
    //        {
    //            new ModelosActivos { modeloID = Guid.NewGuid(), marcaID = Guid.NewGuid(), marca = "Apple", name = "Modelo 1", description = "Descripción del modelo 1", assignatedAssets = 10 },

    //        };

    //    public IActionResult Modelos(string search, int page = 1, int pageSize = 20, string sortColumn = "name", string sortDirection = "asc", string hasAssets = "")
    //    {
    //      // Filtrar por búsqueda si hay un término proporcionado
    //      if (!string.IsNullOrEmpty(search))
    //      {
    //        modelosActivos = modelosActivos
    //            .Where(e => e.name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
    //                        e.description.Contains(search, StringComparison.OrdinalIgnoreCase) ||
    //                        e.marca.Contains(search, StringComparison.OrdinalIgnoreCase))
    //            .ToList();
    //      }

    //      // Aplicar filtro
    //      if (hasAssets == "withAssets")
    //      {
    //        modelosActivos = modelosActivos.Where(e => e.assignatedAssets > 0).ToList();
    //      }
    //      else if (hasAssets == "withoutAssets")
    //      {
    //        modelosActivos = modelosActivos.Where(e => e.assignatedAssets == 0).ToList();
    //      }


    //      // Ordenar dinámicamente 
    //      modelosActivos = sortColumn switch
    //      {
    //        "description" => sortDirection == "asc"
    //            ? modelosActivos.OrderBy(e => e.description).ToList()
    //            : modelosActivos.OrderByDescending(e => e.description).ToList(),
    //        "assignatedAssets" => sortDirection == "asc"
    //            ? modelosActivos.OrderBy(e => e.assignatedAssets).ToList()
    //            : modelosActivos.OrderByDescending(e => e.assignatedAssets).ToList(),
    //        "marca" => sortDirection == "asc"
    //            ? modelosActivos.OrderBy(e => e.marca).ToList()
    //            : modelosActivos.OrderByDescending(e => e.marca).ToList(),
    //        _ => sortDirection == "asc"
    //            ? modelosActivos.OrderBy(e => e.name).ToList()
    //            : modelosActivos.OrderByDescending(e => e.name).ToList(),
    //      };


    //      // Calcular la paginación
    //      var totalItems = modelosActivos.Count;
    //      var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
    //      var itemsOnPage = modelosActivos.Skip((page - 1) * pageSize).Take(pageSize).ToList();

    //      // Crear el modelo de paginación
    //      var model = new ModelosViewModel
    //      {
    //        Modelos = itemsOnPage,
    //        CurrentPage = page,
    //        TotalPages = totalPages
    //      };

    //      // Pasar el término de búsqueda, columna y dirección actuales a la vista
    //      ViewBag.SearchQuery = search;
    //      ViewBag.SortColumn = sortColumn;
    //      ViewBag.SortDirection = sortDirection;
    //      ViewBag.Filter = hasAssets; // Pasar el filtro actual a la vista

    //      return View(model);
    //    }
    //    [HttpGet]
    //    public IActionResult EliminarRegistro(int modeloID)
    //    {
    //      Console.WriteLine("ID DEL MODELO: " + modeloID);  // Verifica que este valor es correcto

    //      // Buscar el objeto correspondiente al modelo utilizando el modeloID
    //      var marca = modelosActivos.FirstOrDefault(m => m.modeloID == modeloID);

    //      if (marca == null)
    //      {
    //        return Json(new { success = false, message = "El modelo con ID " + modeloID + " no existe y no se puede eliminar." });
    //      }

    //      // Verificar si el modelo tiene activos asignados
    //      if (marca.assignatedAssets > 0)
    //      {
    //        return Json(new { success = false, message = "El modelo tiene activos asignados y no se puede eliminar." });
    //      }

    //      // Verificar si el marcaID es nulo
    //      if (marca.marcaID != null)
    //      {
    //        return Json(new { success = false, message = "El modelo no se puede eliminar porque tiene un 'marcaID' asociado." });
    //      }

    //      // Si no tiene activos asignados ni un marcaID, eliminarlo
    //      modelosActivos.Remove(marca);  // Eliminar el objeto de la lista
    //      return Json(new { success = true, message = "Modelo eliminado exitosamente." });
    //    }

    //    [HttpPost]
    //    public IActionResult EliminarRegistroBatch(string modeloIDs)
    //    {
    //      if (string.IsNullOrEmpty(modeloIDs))
    //      {
    //        return Json(new { success = false, message = "No se seleccionaron modelos para eliminar." });
    //      }

    //      // Convertir la cadena de IDs en una lista
    //      var modeloIDList = modeloIDs.Split(',').Select(int.Parse).ToList();

    //      foreach (var modeloID in modeloIDList)
    //      {
    //        var marca = modelosActivos.FirstOrDefault(m => m.modeloID == modeloID);

    //        if (marca != null)
    //        {
    //          // Verificar si tiene un 'marcaID' asociado o activos asignados
    //          if (marca.marcaID == null && marca.assignatedAssets == 0)
    //          {
    //            modelosActivos.Remove(marca); // Eliminar el objeto de la lista
    //          }
    //          else
    //          {
    //            return Json(new { success = false, message = $"El modelo {modeloID} no se puede eliminar porque tiene un 'marcaID' o activos asignados." });
    //          }
    //        }
    //      }

    //      return Json(new { success = true, message = "Modelos eliminados exitosamente." });
    //    }


    //    private static List<MarcaActivos> _marcaList = new List<MarcaActivos>();
    //    private static int _nextMarcaId = 1; // Contador global para los IDs

    //    [HttpPost]
    //    public IActionResult AgregarMarca(MarcaActivos model)
    //    {


    //      // Validar el modelo
    //      if (ModelState.IsValid)
    //      {
    //        model.idMarca = _nextMarcaId++; // Asigna un ID único al rol
    //        _marcaList.Add(model);

    //        // Devolver respuesta en formato JSON
    //        return Json(new { success = true, message = "Marca agregado exitosamente", marca = _marcaList });
    //      }

    //      return Json(new { success = false, message = "Datos inválidos" });
    //    }

    //    private static List<ModelosActivos> _modeloList = new List<ModelosActivos>();
    //    private static int _nextModeloId = 1; // Contador global para los IDs

    //    [HttpPost]
    //    public IActionResult AgregarModelo(ModelosActivos model)
    //    {


    //      // Validar el modelo
    //      if (ModelState.IsValid)
    //      {
    //        model.modeloID = _nextModeloId++; // Asigna un ID único al rol
    //        _modeloList.Add(model);

    //        // Devolver respuesta en formato JSON
    //        return Json(new { success = true, message = "Marca agregado exitosamente", modelo = _modeloList });
    //      }

    //      return Json(new { success = false, message = "Datos inválidos" });
    //    }

    //    //
    //    private static List<ModelosActivos> _modeloListEdit = new List<ModelosActivos>();
    //    private static int _nextModeloIdEdit = 1; // Contador global para los IDs

    //    [HttpPost]
    //    public IActionResult EditarModelo(ModelosActivos model)
    //    {


    //      // Validar el modelo
    //      if (ModelState.IsValid)
    //      {
    //        model.modeloID = _nextModeloIdEdit++; // Asigna un ID único al rol
    //        _modeloListEdit.Add(model);

    //        // Devolver respuesta en formato JSON
    //        return Json(new { success = true, message = "Marca editado exitosamente", modeloEdit = _modeloListEdit });
    //      }

    //      return Json(new { success = false, message = "Datos inválidos" });
    //    }

  }
}

public class PagedList<T>
{
  public List<T> Items { get; set; } = new List<T>();
  public int CurrentPage { get; set; }
  public int TotalPages { get; set; }
  public int TotalItems { get; set; }
  public int PageSize { get; set; }
}
