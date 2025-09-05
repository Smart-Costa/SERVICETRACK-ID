using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;
using OfficeOpenXml;
using System.IO;
using System.Diagnostics;
using Diverscan.Activos.EL;
using AspnetCoreMvcFull.Models.Sectores;
using AspnetCoreMvcFull.Models.Oficinas;
using AspnetCoreMvcFull.Models.Categorias;
using Diverscan.Activos.DL;
using System.ComponentModel;
using System.Reflection;
using Diverscan.Activos.UIL.Admin;
using Diverscan.Activos.Utilities;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using OfficeOpenXml;
using System.Data;
using System.IO;
using LicenseContext = OfficeOpenXml.LicenseContext;

namespace AspnetCoreMvcFull.Controllers
{
  public class ReportesController : Controller
  {
    public IActionResult Index()
    {
      return View();
    }//

    public IActionResult rep_ActivosNuevos()
    {
      return View();
    }

    public IActionResult rep_ActivosLevantados()
    {
      return View();
    }

    public IActionResult rep_FaltantesPorUbicacion()
    {
      return View();
    }





    public IActionResult rep_PorEmpleados()
    {
      return View();
    }

    public IActionResult rep_EPCAsignados()
    {
      return View();
    }

    public IActionResult rep_Permisos()
    {
      return View();
    }

    public IActionResult rep_PorPolizas()
    {
      return View();
    }

    public IActionResult rep_TomasFisicasInventario()
    {
      return View();
    }
    public IActionResult Reportes()
    {
      ViewBag.NombreUbicacion = GetUltimoNombreUbicacionC() ?? "Ubicación C";
      ViewBag.NombreUbicacionA = GetUltimoNombreUbicacionA() ?? "Ubicación A";
      ViewBag.NombreUbicacionB = GetUltimoNombreUbicacionB() ?? "Ubicación B";

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
    public IActionResult rep_DestruidosDonados()
    {
      List<AssetStatus> estados = GetEstados();
      ViewBag.EstadosList = estados;

      // Si querés marcar uno como seleccionado
      // ViewBag.SelectedEstado = algunGuid.ToString();

      return View();
    }


    private DataTable GetFormatDataTableActivosRetiradosFotografia()
    {
      try
      {
        var dataTable = new DataTable("ActivosRetiradosFotografia");
        dataTable.Columns.Add("assetSysId", typeof(Guid));
        dataTable.Columns.Add("Placa", typeof(string));
        dataTable.Columns.Add("assetItemNumber", typeof(string));
        dataTable.Columns.Add("shortDescription", typeof(string));
        dataTable.Columns.Add("assetStatusSysId", typeof(Guid));
        dataTable.Columns.Add("estado", typeof(string));
        dataTable.Columns.Add("updateDate", typeof(DateTime));

        return dataTable;
      }
      catch (Exception e)
      {
        CLErrores.EscribirError(e);
        return null;
      }
    }
    public IActionResult rep_RetiradosConFotografia(int page = 1, int pageSize = 10)
    {
      // Obtener los activos retirados con fotografía
      var activos = AccessAssets.GenerarReporteActivosRetiradosFotografia();

      // Formatear tabla
      DataTable tablaCompleta = GetFormatDataTableActivosRetiradosFotografia();
      foreach (var act in activos)
      {
        DataRow row = tablaCompleta.NewRow();
        row["assetSysId"] = act.AssetSysId;
        row["Placa"] = act.AssetItemNumber;
        row["assetItemNumber"] = act.AssetItemNumber;
        row["shortDescription"] = act.ShortDescription;
        row["assetStatusSysId"] = act.AssetStatusSysId;
        row["estado"] = act.StatusDescription;
        row["updateDate"] = act.UpdateDate;
        tablaCompleta.Rows.Add(row);
      }

      // Total de registros y páginas
      int totalRecords = tablaCompleta.Rows.Count;
      int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

      // Crear tabla paginada
      DataTable tablaPaginada = tablaCompleta.Clone(); // mismo esquema
      int startIndex = (page - 1) * pageSize;
      int endIndex = Math.Min(startIndex + pageSize, totalRecords);
      for (int i = startIndex; i < endIndex; i++)
      {
        tablaPaginada.ImportRow(tablaCompleta.Rows[i]);
      }

      // ViewBags necesarios para la paginación
      ViewBag.CurrentPage = page;
      ViewBag.TotalPages = totalPages;

      // Si usás filtros en el futuro, puedes enviar más ViewBags
      // ViewBag.SelectedStatus = ...
      // ViewBag.SelectedCategory = ...

      return View(tablaPaginada);
    }


    private DataTable GetFormatDataTableDepartamentos()
    {
      try
      {
        var dataTable = new DataTable("Departamentos");
        dataTable.Columns.Add("placa", typeof(string));
        dataTable.Columns.Add("AssetItemNumber", typeof(string));

        dataTable.Columns.Add("shortDescription", typeof(string));
        dataTable.Columns.Add("Nombre", typeof(string));

        return dataTable;
      }
      catch (Exception e)
      {
        CLErrores.EscribirError(e);
        return null;
      }
    }

    public IActionResult rep_PorDepartamento(string placa, int departamento, int page = 1, int pageSize = 10)
    {
      // Obtener los activos filtrados
      var departamentos = AccessDepartment.listarActivosDepartamento(placa, departamento);

      // Crear y llenar el DataTable
      DataTable departmentsTable = GetFormatDataTableDepartamentos();
      foreach (var dep in departamentos)
      {
        DataRow row = departmentsTable.NewRow();
        row["AssetItemNumber"] = dep.AssetItemNumber;
        row["placa"] = dep.Placa;
        row["shortDescription"] = dep.ShortDescription;
        row["Nombre"] = dep.Nombre;
        departmentsTable.Rows.Add(row);
      }

      // Total de registros
      int totalRecords = departmentsTable.Rows.Count;
      int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

      // Paginación
      DataTable paginatedTable = departmentsTable.Clone();
      int startIndex = (page - 1) * pageSize;
      int endIndex = Math.Min(startIndex + pageSize, totalRecords);
      for (int i = startIndex; i < endIndex; i++)
      {
        paginatedTable.ImportRow(departmentsTable.Rows[i]);
      }

      // Pasar datos a la vista
      ViewBag.DepartamentoList = GetDepartmentsList();
      ViewBag.SelectedDepartment = departamento;
      ViewBag.SelectedPlaca = placa;
      ViewBag.CurrentPage = page;
      ViewBag.TotalPages = totalPages;


      return View(paginatedTable);
    }


    // Clase modelo para almacenar departamentos
    public class Department
    {
      public string IdLogicoB { get; set; }
      public string Nombre { get; set; }
    }

    private List<Department> GetDepartmentsList()
    {
      List<Department> departmentsList = new List<Department>();
      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

      using (SqlConnection connection = new SqlConnection(connectionString))
      {
        string query = "SELECT IdLogicoB, Nombre FROM LogicoB";
        SqlCommand command = new SqlCommand(query, connection);
        connection.Open();
        SqlDataReader reader = command.ExecuteReader();

        while (reader.Read())
        {
          departmentsList.Add(new Department
          {
            IdLogicoB = reader["IdLogicoB"].ToString(),
            Nombre = reader["Nombre"].ToString()
          });
        }
      }

      return departmentsList;
    }



    public IActionResult rep_PorUbicacion(Guid company, Guid building, Guid floor, Guid office, int page = 1, int pageSize = 10)
    {
      Asset[] activos = null;

      // Obtener los datos de activos
      activos = AccessAssets.GenerarReporteActivosUbicadosXUbicacion(company.ToString(), building.ToString(), floor.ToString(), office.ToString());

      // Crear el DataTable con el formato requerido
      DataTable activosTable = GetFormatDataTableActivosUbicacion();

      // Poblar el DataTable con los datos de los activos
      foreach (var activo in activos)
      {
        DataRow row = activosTable.NewRow();

        row["assetSysId"] = activo.AssetSysId;
        row["Status"] = activo.AssetStatusSysIdName;
        row["Category"] = activo.CategoriaNombre;
        row["assetItemNumber"] = activo.AssetItemNumber;
        row["SAPNumber"] = activo.SAPNumber;
        row["Barcode"] = activo.Barcode;
        row["shortDescription"] = activo.ShortDescription;
        row["longDescription"] = activo.LongDescription;
        row["brand"] = activo.Brand;
        row["modelNo"] = activo.ModelNo;
        row["serialNo"] = activo.SerialNo;
        row["IdBusinessName"] = activo.CompanySysId;
        row["BusinessName"] = activo.Compania;
        row["IdCountry"] = activo.BuildingSysId;
        row["Country"] = activo.Edificio;
        row["IdBuilding"] = activo.FloorSysId;
        row["Building"] = activo.Piso;
        row["IdSector"] = activo.OfficeSysId;
        row["Sector"] = activo.Oficina;
        row["BarcoreParent"] = activo.BarcodeParent;
        row["shortDescriptionParent"] = activo.ShortDescriptionParent;

        activosTable.Rows.Add(row);
      }

      // Total de registros
      int totalRecords = activosTable.Rows.Count;
      int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

      // Filtrar solo los registros de la página actual
      DataTable paginatedTable = activosTable.Clone();
      int startIndex = (page - 1) * pageSize;
      int endIndex = Math.Min(startIndex + pageSize, totalRecords);

      for (int i = startIndex; i < endIndex; i++)
      {
        paginatedTable.ImportRow(activosTable.Rows[i]);
      }

      // Cargar las listas de estado y categoría para los selects
      ViewBag.EdificioList = GetEdificios();
      ViewBag.PisoList = GetPisos();
      ViewBag.SectorList = GetSectores();
      ViewBag.OficinaList = GetOficinas();

      // Pasar los valores seleccionados a la vista
      ViewBag.SelectedCompany = company;
      ViewBag.SelectedBuilding = building;
      ViewBag.SelectedFloor = floor;
      ViewBag.SelectedOffice = office;
      ViewBag.CurrentPage = page;
      ViewBag.TotalPages = totalPages;

      return View(paginatedTable);
    }

    private DataTable GetFormatDataTableActivosUbicacion()
    {
      try
      {
        var dataTable = new DataTable("ActivosUbicacion");
        dataTable.Columns.Add("assetSysId", typeof(Guid));
        dataTable.Columns.Add("Status", typeof(string));
        dataTable.Columns.Add("Category", typeof(string));
        dataTable.Columns.Add("assetItemNumber", typeof(string));
        dataTable.Columns.Add("SAPNumber", typeof(string));
        dataTable.Columns.Add("Barcode", typeof(string));
        dataTable.Columns.Add("shortDescription", typeof(string));
        dataTable.Columns.Add("longDescription", typeof(string));
        dataTable.Columns.Add("brand", typeof(string));
        dataTable.Columns.Add("modelNo", typeof(string));
        dataTable.Columns.Add("serialNo", typeof(string));
        dataTable.Columns.Add("IdBusinessName", typeof(Guid));
        dataTable.Columns.Add("BusinessName", typeof(string));
        dataTable.Columns.Add("IdCountry", typeof(Guid));
        dataTable.Columns.Add("Country", typeof(string));
        dataTable.Columns.Add("IdBuilding", typeof(Guid));
        dataTable.Columns.Add("Building", typeof(string));
        dataTable.Columns.Add("IdSector", typeof(Guid));
        dataTable.Columns.Add("Sector", typeof(string));
        dataTable.Columns.Add("BarcoreParent", typeof(string));
        dataTable.Columns.Add("shortDescriptionParent", typeof(string));

        return dataTable;
      }
      catch (Exception)
      {

        return null;
      }
    }



    public IActionResult reporteActivoGeneral(string status = "", string category = "", int page = 1, int pageSize = 10)
    {
      Asset[] activos = AccessAssets.GenerarReporteActivosGeneral(status, category);

      // Convertir la lista de activos a DataTable
      DataTable activosTable = ConvertToDataTable(activos);

      // Total de registros y total de páginas
      int totalRecords = activosTable.Rows.Count;
      int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

      // Obtener solo los registros de la página actual
      var paginatedTable = activosTable.Clone(); // Clona la estructura
      foreach (DataRow row in activosTable.AsEnumerable().Skip((page - 1) * pageSize).Take(pageSize))
      {
        paginatedTable.ImportRow(row);
      }

      // Pasar datos a la vista
      ViewBag.StatusList = GetAssetStatusList();
      ViewBag.CategoryList = GetAssetCategoryList();
      ViewBag.SelectedStatus = status;
      ViewBag.SelectedCategory = category;

      ViewBag.CurrentPage = page;
      ViewBag.TotalPages = totalPages;

      return View(paginatedTable);
    }

    // Método para convertir una lista de objetos a DataTable
    public DataTable ConvertToDataTable(Asset[] activos)
    {
      // Crear un DataTable y añadir las columnas en el orden especificado
      DataTable dataTable = new DataTable("ActivosGenerales");
      dataTable.Columns.Add("assetSysId", typeof(Guid));
      dataTable.Columns.Add("Status", typeof(string));
      dataTable.Columns.Add("Category", typeof(string));
      dataTable.Columns.Add("assetItemNumber", typeof(string));
      dataTable.Columns.Add("SAPNumber", typeof(string));
      dataTable.Columns.Add("Barcode", typeof(string));
      dataTable.Columns.Add("shortDescription", typeof(string));
      dataTable.Columns.Add("longDescription", typeof(string));
      dataTable.Columns.Add("brand", typeof(string));
      dataTable.Columns.Add("modelNo", typeof(string));
      dataTable.Columns.Add("serialNo", typeof(string));
      dataTable.Columns.Add("cost", typeof(decimal));
      dataTable.Columns.Add("costDollars", typeof(decimal));
      dataTable.Columns.Add("LocationA", typeof(string));
      dataTable.Columns.Add("LocationB", typeof(string));
      dataTable.Columns.Add("LocationC", typeof(string));
      dataTable.Columns.Add("LocationD", typeof(string));
      dataTable.Columns.Add("EmployeeRelated", typeof(string));
      dataTable.Columns.Add("EPC", typeof(string));
      dataTable.Columns.Add("StatusDescription", typeof(string));
      dataTable.Columns.Add("Impuesto", typeof(string));
      dataTable.Columns.Add("Depreciado", typeof(string));
      dataTable.Columns.Add("PartNo", typeof(string));
      dataTable.Columns.Add("NumeroFactura", typeof(string));
      dataTable.Columns.Add("FechaCompra", typeof(DateTime));
      dataTable.Columns.Add("DiasObsoletoInterna", typeof(int));
      dataTable.Columns.Add("DiasObsoletoProcomer", typeof(int));
      dataTable.Columns.Add("duaNumber", typeof(string));
      dataTable.Columns.Add("CuentaContableActivoFijoFGAR", typeof(string));
      dataTable.Columns.Add("CuentaContableActivoFijoImpuesto", typeof(string));
      dataTable.Columns.Add("CuentaContableDepreciacion", typeof(string));
      dataTable.Columns.Add("FechaCapitalizacion", typeof(DateTime));
      dataTable.Columns.Add("InversionFTZ", typeof(string));
      dataTable.Columns.Add("CentroCostos", typeof(string));
      dataTable.Columns.Add("ClaseActivo", typeof(string));
      dataTable.Columns.Add("BarcoreParent", typeof(string));
      dataTable.Columns.Add("DescriptionParent", typeof(string));

      // Añadir las filas de datos en base a los activos
      foreach (var activo in activos)
      {
        DataRow row = dataTable.NewRow();
        row["assetSysId"] = activo.AssetSysId;
        row["Status"] = activo.AssetStatusSysIdName;
        row["Category"] = activo.CategoriaNombre;
        row["assetItemNumber"] = activo.AssetItemNumber;
        row["SAPNumber"] = activo.SAPNumber;
        row["Barcode"] = activo.Barcode;
        row["shortDescription"] = activo.ShortDescription;
        row["longDescription"] = activo.LongDescription;
        row["brand"] = activo.Brand;
        row["modelNo"] = activo.ModelNo;
        row["serialNo"] = activo.SerialNo;
        row["cost"] = activo.Cost;
        row["costDollars"] = activo.CostDollars;
        row["LocationA"] = activo.LocationA;
        row["LocationB"] = activo.LocationB;
        row["LocationC"] = activo.LocationC;
        row["LocationD"] = activo.LocationD;
        row["EmployeeRelated"] = activo.EmployeeRelated;
        row["EPC"] = activo.EPC;
        row["StatusDescription"] = activo.StatusDescription;
        row["Impuesto"] = activo.Impuesto;
        row["Depreciado"] = activo.Depreciado;
        row["PartNo"] = activo.PartNo;
        row["NumeroFactura"] = activo.NumeroFactura;
        row["FechaCompra"] = activo.DateAdquired;
        row["DiasObsoletoInterna"] = activo.valorvidaUtil;
        row["DiasObsoletoProcomer"] = activo.vidaUtilProcomer;
        row["duaNumber"] = activo.DuaNumber;
        row["CuentaContableActivoFijoFGAR"] = activo.ProcomerFixedAssetAccount;
        row["CuentaContableActivoFijoImpuesto"] = activo.TaxesFixedAssetAccount;
        row["CuentaContableDepreciacion"] = activo.AccountingAccountDepreciation;
        row["FechaCapitalizacion"] = activo.CapitalizationDate;
        row["InversionFTZ"] = activo.FTZInvestment;
        row["CentroCostos"] = activo.CostCenter;
        row["ClaseActivo"] = activo.AssetClass;
        row["BarcoreParent"] = activo.BarcodeParent;
        row["DescriptionParent"] = activo.ShortDescriptionParent;

        dataTable.Rows.Add(row);
      }

      return dataTable;
    }








    // Método para obtener los nombres de los estados
    private List<string> GetAssetStatusList()
    {
      List<string> statusList = new List<string>();
      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

      using (SqlConnection connection = new SqlConnection(connectionString))
      {
        string query = "SELECT name FROM assetStatus";
        SqlCommand command = new SqlCommand(query, connection);
        connection.Open();
        SqlDataReader reader = command.ExecuteReader();

        while (reader.Read())
        {
          statusList.Add(reader["name"].ToString());
        }
      }

      return statusList;
    }

    // Método para obtener los nombres de las categorías
    private List<string> GetAssetCategoryList()
    {
      List<string> categoryList = new List<string>();
      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

      using (SqlConnection connection = new SqlConnection(connectionString))
      {
        string query = "SELECT name FROM assetCategory";
        SqlCommand command = new SqlCommand(query, connection);
        connection.Open();
        SqlDataReader reader = command.ExecuteReader();

        while (reader.Read())
        {
          categoryList.Add(reader["name"].ToString());
        }
      }

      return categoryList;
    }

    private DataTable GetActivosPorUbicacion(Guid company = default, Guid building = default, Guid floor = default, Guid office = default)
    {
      // Establecer el valor predeterminado para cada parámetro si es Guid.Empty
      if (company == Guid.Empty) company = new Guid("00000000-0000-0000-0000-000000000000");
      if (building == Guid.Empty) building = new Guid("00000000-0000-0000-0000-000000000000");
      if (floor == Guid.Empty) floor = new Guid("00000000-0000-0000-0000-000000000000");
      if (office == Guid.Empty) office = new Guid("00000000-0000-0000-0000-000000000000");

      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

      using (SqlConnection conn = new SqlConnection(connectionString))
      {
        using (SqlCommand cmd = new SqlCommand("sp_ReporteActivosXUbicacion", conn))
        {
          cmd.CommandType = CommandType.StoredProcedure;

          // Agregar parámetros al comando
          cmd.Parameters.AddWithValue("@company", company);
          cmd.Parameters.AddWithValue("@building", building);
          cmd.Parameters.AddWithValue("@floor", floor);
          cmd.Parameters.AddWithValue("@office", office);

          // Abrir la conexión
          conn.Open();

          SqlDataAdapter adapter = new SqlDataAdapter(cmd);
          DataTable activosTable = new DataTable();
          adapter.Fill(activosTable);
          return activosTable;
        }
      }
    }


    private List<Company> GetEdificios()
    {
      List<Company> companyList = new List<Company>();
      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

      using (SqlConnection connection = new SqlConnection(connectionString))
      {
        string query = "SELECT companySysId, name FROM companies";
        SqlCommand command = new SqlCommand(query, connection);
        connection.Open();
        SqlDataReader reader = command.ExecuteReader();

        while (reader.Read())
        {
          // Asumiendo que los otros campos no son necesarios para este contexto
          companyList.Add(new Company(
              companySysId: (Guid)reader["companySysId"],
              name: reader["name"].ToString(),
              description: string.Empty,
              entryUser: Guid.Empty,
              entryDate: DateTime.MinValue,
              updateUser: Guid.Empty,
              updateDate: DateTime.MinValue,
              rowGuid: Guid.Empty
          ));
        }
      }

      return companyList;
    }


    private List<AssetStatus> GetEstados()
    {
      List<AssetStatus> estadosList = new List<AssetStatus>();
      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

      using (SqlConnection connection = new SqlConnection(connectionString))
      {
        string query = "SELECT assetStatusSysId, name, description, entryUser, entryDate, updateUser, updateDate, rowGuid FROM assetStatus";
        SqlCommand command = new SqlCommand(query, connection);
        connection.Open();
        SqlDataReader reader = command.ExecuteReader();

        while (reader.Read())
        {
          estadosList.Add(new AssetStatus
          {
            AssetStatusSysId = (Guid)reader["assetStatusSysId"],
            Name = reader["name"].ToString(),
            Description = reader["description"].ToString(),
            EntryUser = (Guid)reader["entryUser"],
            EntryDate = (DateTime)reader["entryDate"],
            UpdateUser = (Guid)reader["updateUser"],
            UpdateDate = (DateTime)reader["updateDate"],
            RowGuid = (Guid)reader["rowGuid"],
            DisplayName = $"{reader["name"]} - {reader["description"]}" // Opcional, si querés mostrarlo así
          });
        }
      }

      return estadosList;
    }

    private List<Building> GetPisos()
    {
      List<Building> buildingList = new List<Building>();
      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

      using (SqlConnection connection = new SqlConnection(connectionString))
      {
        string query = "SELECT buildingSysId, name FROM buildings";
        SqlCommand command = new SqlCommand(query, connection);
        connection.Open();
        SqlDataReader reader = command.ExecuteReader();

        while (reader.Read())
        {
          buildingList.Add(new Building(
              buildingSysId: (Guid)reader["buildingSysId"],
              name: reader["name"].ToString(),
              description: string.Empty, // O el valor que corresponda
              entryUser: Guid.Empty, // O el valor que corresponda
              entryDate: DateTime.MinValue, // O el valor que corresponda
              updateUser: Guid.Empty, // O el valor que corresponda
              updateDate: DateTime.MinValue, // O el valor que corresponda
              rowGuid: Guid.Empty, // O el valor que corresponda
              companySysId: Guid.Empty // O el valor que corresponda
          ));
        }
      }

      return buildingList;
    }


    private List<Sector> GetSectores()
    {
      List<Sector> sectorList = new List<Sector>();
      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

      using (SqlConnection connection = new SqlConnection(connectionString))
      {
        string query = "SELECT floorSysId, name FROM floors";
        SqlCommand command = new SqlCommand(query, connection);
        connection.Open();
        SqlDataReader reader = command.ExecuteReader();

        while (reader.Read())
        {
          sectorList.Add(new Sector
          {
            floorSysId = (Guid)reader["floorSysId"],
            name = reader["name"].ToString(),
            description = string.Empty, // O el valor que corresponda
            entryUser = Guid.Empty, // O el valor que corresponda
            entryDate = DateTime.MinValue, // O el valor que corresponda
            updateUser = Guid.Empty, // O el valor que corresponda
            updateDate = DateTime.MinValue, // O el valor que corresponda
            rowGuid = Guid.Empty, // O el valor que corresponda
            buildingSysId = Guid.Empty, // O el valor que corresponda
            companySysId = Guid.Empty, // O el valor que corresponda
            companyIdExtern = null, // O el valor que corresponda
            Activos = 0, // O el valor que corresponda
            Edificio = string.Empty, // O el valor que corresponda
            Piso = string.Empty // O el valor que corresponda
          });
        }
      }

      return sectorList;
    }

    private List<Oficina> GetOficinas()
    {
      List<Oficina> officeList = new List<Oficina>();
      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

      using (SqlConnection connection = new SqlConnection(connectionString))
      {
        string query = "SELECT officeSysId, name FROM officeses";
        SqlCommand command = new SqlCommand(query, connection);
        connection.Open();
        SqlDataReader reader = command.ExecuteReader();

        while (reader.Read())
        {
          officeList.Add(new Oficina(
              officeSysId: (Guid)reader["officeSysId"],
              companySysId: Guid.Empty, // O el valor que corresponda
              buildingSysId: Guid.Empty, // O el valor que corresponda
              businessUnitSysId: Guid.Empty, // O el valor que corresponda
              floorSysId: Guid.Empty, // O el valor que corresponda
              deptSysId: Guid.Empty, // O el valor que corresponda
              tagSysId: Guid.Empty, // O el valor que corresponda
              name: reader["name"].ToString(),
              description: string.Empty, // O el valor que corresponda
              entryUser: Guid.Empty, // O el valor que corresponda
              entryDate: DateTime.MinValue, // O el valor que corresponda
              updateUser: Guid.Empty, // O el valor que corresponda
              updateDate: DateTime.MinValue, // O el valor que corresponda
              rowGuid: Guid.Empty, // O el valor que corresponda
              isEnable: false // O el valor que corresponda
          ));
        }
      }

      return officeList;
    }

    private DataTable GetActivos(string status = "", string category = "")
    {
      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

      using (SqlConnection conn = new SqlConnection(connectionString))
      {
        using (SqlCommand cmd = new SqlCommand("sp_ReporteActivosGeneral", conn))
        {
          cmd.CommandType = CommandType.StoredProcedure;
          cmd.Parameters.AddWithValue("@status", string.IsNullOrEmpty(status) || status == "Todos" ? "%" : status);
          cmd.Parameters.AddWithValue("@category", string.IsNullOrEmpty(category) || category == "Todas" ? "%" : category);

          SqlDataAdapter adapter = new SqlDataAdapter(cmd);
          DataTable activosTable = new DataTable();
          adapter.Fill(activosTable);
          return activosTable;
        }
      }
    }



    public IActionResult ExportarExcel(string status = "", string category = "")
    {
      ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

      DataTable activosTable = GetActivos(status, category);

      using (var package = new ExcelPackage())
      {
        ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Activos");

        for (int col = 0; col < activosTable.Columns.Count; col++)
        {
          worksheet.Cells[1, col + 1].Value = activosTable.Columns[col].ColumnName;
        }

        for (int row = 0; row < activosTable.Rows.Count; row++)
        {
          for (int col = 0; col < activosTable.Columns.Count; col++)
          {
            worksheet.Cells[row + 2, col + 1].Value = activosTable.Rows[row][col]?.ToString();
          }
        }

        var excelBytes = package.GetAsByteArray();

        var fileName = $"Reporte_Activo_General_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
      }
    }


    public IActionResult ExportarExcelActivosPorUbicacion(Guid company = default, Guid building = default, Guid floor = default, Guid office = default)
    {
      ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

      DataTable activosTable = GetActivosPorUbicacion(company, building, floor, office);

      using (ExcelPackage package = new ExcelPackage())
      {
        ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Activos por ubicación");

        // Encabezados
        for (int col = 0; col < activosTable.Columns.Count; col++)
        {
          worksheet.Cells[1, col + 1].Value = activosTable.Columns[col].ColumnName;
        }

        // Datos
        for (int row = 0; row < activosTable.Rows.Count; row++)
        {
          for (int col = 0; col < activosTable.Columns.Count; col++)
          {
            worksheet.Cells[row + 2, col + 1].Value = activosTable.Rows[row][col]?.ToString();
          }
        }

        // Convertir a byte[]
        var excelBytes = package.GetAsByteArray();
        var fileName = $"Reporte_Activos_Ubicacion_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

        // Descargar directamente
        return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
      }
    }


    public IActionResult ExportarExcelActivosPorDepartamento(string placa, int departamento)
    {
      // Obtener los activos filtrados por departamento
      var departamentos = AccessDepartment.listarActivosDepartamento(placa, departamento);

      // Crear y llenar el DataTable
      DataTable departmentsTable = GetFormatDataTableDepartamentos();
      foreach (var dep in departamentos)
      {
        DataRow row = departmentsTable.NewRow();
        row["AssetItemNumber"] = dep.AssetItemNumber;
        row["placa"] = dep.Placa;
        row["shortDescription"] = dep.ShortDescription;
        row["Nombre"] = dep.Nombre;
        departmentsTable.Rows.Add(row);
      }

      ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

      using (ExcelPackage package = new ExcelPackage())
      {
        ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Activos por Departamento");

        // Escribir los encabezados
        for (int col = 0; col < departmentsTable.Columns.Count; col++)
        {
          worksheet.Cells[1, col + 1].Value = departmentsTable.Columns[col].ColumnName;
        }

        // Escribir los datos
        for (int row = 0; row < departmentsTable.Rows.Count; row++)
        {
          for (int col = 0; col < departmentsTable.Columns.Count; col++)
          {
            worksheet.Cells[row + 2, col + 1].Value = departmentsTable.Rows[row][col]?.ToString();
          }
        }

        // Convertir el archivo a byte[]
        var excelBytes = package.GetAsByteArray();
        var fileName = $"Reporte_Activos_Departamento_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

        // Retornar archivo como descarga
        return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
      }
    }

    public IActionResult ExportarExcelActivosRetirados()
    {
      try
      {
        // Obtener los activos retirados con fotografía
        var activos = AccessAssets.GenerarReporteActivosRetiradosFotografia();

        // Crear y llenar el DataTable
        DataTable dataTable = GetFormatDataTableActivosRetiradosFotografia();
        foreach (var act in activos)
        {
          DataRow row = dataTable.NewRow();
          row["assetSysId"] = act.AssetSysId;
          row["Placa"] = act.AssetItemNumber;
          row["assetItemNumber"] = act.AssetItemNumber;
          row["shortDescription"] = act.ShortDescription;
          row["assetStatusSysId"] = act.AssetStatusSysId;
          row["estado"] = act.StatusDescription;
          row["updateDate"] = act.UpdateDate;
          dataTable.Rows.Add(row);
        }

        ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

        using (ExcelPackage package = new ExcelPackage())
        {
          ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Activos Retirados");

          // Escribir los encabezados
          for (int col = 0; col < dataTable.Columns.Count; col++)
          {
            worksheet.Cells[1, col + 1].Value = dataTable.Columns[col].ColumnName;
          }

          // Escribir los datos
          for (int row = 0; row < dataTable.Rows.Count; row++)
          {
            for (int col = 0; col < dataTable.Columns.Count; col++)
            {
              worksheet.Cells[row + 2, col + 1].Value = dataTable.Rows[row][col]?.ToString();
            }
          }

          // Convertir el archivo a byte[]
          var excelBytes = package.GetAsByteArray();
          var fileName = $"Reporte_Activos_Retirados_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

          // Retornar archivo como descarga directa
          return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
      }
      catch (Exception ex)
      {
        CLErrores.EscribirError(ex);
        TempData["ErrorMessage"] = "Ocurrió un error al exportar el reporte.";
        return RedirectToAction("rep_RetiradosConFotografia");
      }
    }




  }
}
