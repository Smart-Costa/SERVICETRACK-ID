using Microsoft.AspNetCore.Mvc;
using System.Collections;
using System.Configuration;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace AspnetCoreMvcFull.Controllers
{

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
  }

  public class EmployeeList
  {
    public List<Employee> Employees { get; set; } = new List<Employee>();
  }




  public class EmployeesController : Controller
  {

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
      //FILTRADO DE EMPLEADOS
      //Filtrar segun el dropdown ACTIVO / INACTIVO
      if (!String.IsNullOrEmpty(employee_state) && employee_state == "activo")
      {
        employee_list = employee_list
          .Where(empleado =>
          {
            return empleado.State == "Activo";

          })
          .ToArray();
      }
      else if (!String.IsNullOrEmpty(employee_state) && employee_state == "inactivo")
      {
        employee_list = employee_list
          .Where(empleado =>
          {
            return empleado.State == "Inactivo";

          })
          .ToArray();
      }

      //Filtrar segun los parametros de texto de busqueda
      // NOMBRE, APELLIDO, CORREO
      if (!string.IsNullOrEmpty(employee_search_input))
      {
        employee_list = employee_list
          .Where(empleado =>
          {
            return
            empleado.Name.ToLower().Contains(employee_search_input.ToLower()) ||
            empleado.LastName.ToLower().Contains(employee_search_input.ToLower()) ||
            empleado.Email.ToLower().Contains(employee_search_input.ToLower());

          })
          .ToArray();
      }

      if (!string.IsNullOrEmpty(employee_ID_search_input))
      {
        employee_list = employee_list
          .Where(empleado =>
          {
            return
            empleado.Id.ToString().Contains(employee_ID_search_input);

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
    public IActionResult BusinessEmployeeList(string employee_state = "activo/inactivo", string employee_search_input = "",
     string employee_ID_search_input = "", string order_by = "name_ascending", int employees_per_page = 5, int page_number = 1)
    {


      //Se llama al metodo para obtener los datos del JSON
      Employee[] employee_list_from_JSON = get_dummy_employee_data_from_JSON();

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


      //RETORNAR A LA VISTA CON EL ARRAY DE EMPLEADOS FILTRADO
      return View(selected_employee_page);
    }








    [HttpPost]
    public IActionResult BusinessEmployeeRegister(string? txtAddName, string? txtAddLastname,
    string? txtAddCorreoEmpleado, string? txtAddIdEmpleado, string? txtAddIdentificacionE, string? add_employee_state)
    {

      //Validacion de campos
      // Si los campos son nulos o vacios se retorna a la vista de empleados imprimiendo un mensaje de error en consola
      if (String.IsNullOrEmpty(txtAddName) || String.IsNullOrEmpty(txtAddCorreoEmpleado) || String.IsNullOrEmpty(txtAddIdEmpleado)
      || String.IsNullOrEmpty(txtAddLastname) || String.IsNullOrEmpty(txtAddIdentificacionE) || String.IsNullOrEmpty(add_employee_state))
      {
        Console.WriteLine("Error: Campos incompletos");
        return RedirectToAction("BusinessEmployeeList");
      }


      //Si los campos son correctos se procede con la data que llegó al controller



      /// LOGICA DE BACKEND AQUI ///

      //Mostrar los datos que llegaron en consola

      Console.WriteLine("Nombre del Empleado: " + txtAddName + "\n");
      Console.WriteLine("Apellidos del Empleado: " + txtAddLastname + "\n");
      Console.WriteLine("Correo del Empleado: " + txtAddCorreoEmpleado + "\n");
      //txtAddIdEmpleado == Identificaion corporativa
      Console.WriteLine("Identificacion Corporativa del Empleado: " + txtAddIdEmpleado + "\n");
      //txtAddIdentificacionE == Cedula del empleado
      Console.WriteLine("Cedula del Empleado: " + txtAddIdentificacionE + "\n");
      Console.WriteLine("Estado del Empleado: " + add_employee_state + "\n");

      /// LOGICA DE BACKEND AQUI ///




      //Despues de hacer la logica se vuelve a la vista de Empleados
      return RedirectToAction("BusinessEmployeeList");
    }








    [HttpPost]
    public IActionResult BusinessEmployeeEdit(string? txtEditName, string? txtEditLastName,
    string? txtEditEmailA, string? txtEditIdentificacion, string? txtEditIDEmpleado, string? edit_employee_state)
    {

      //Validacion de campos
      // Si los campos son nulos o vacios se retorna a la vista de empleados imprimiendo un mensaje de error en consola
      if (String.IsNullOrEmpty(txtEditName) || String.IsNullOrEmpty(txtEditEmailA) || String.IsNullOrEmpty(txtEditIDEmpleado)
      || String.IsNullOrEmpty(txtEditLastName) || String.IsNullOrEmpty(txtEditIdentificacion) || String.IsNullOrEmpty(edit_employee_state))
      {
        Console.WriteLine("Error: Campos incompletos");
        return RedirectToAction("BusinessEmployeeList");
      }


      //Si los campos son correctos se procede con la data que llegó al controller



      /// LOGICA DE BACKEND AQUI ///

      //Mostrar los datos que llegaron en consola

      Console.WriteLine("Nombre del Empleado: " + txtEditName + "\n");
      Console.WriteLine("Apellidos del Empleado: " + txtEditLastName + "\n");
      Console.WriteLine("Correo del Empleado: " + txtEditEmailA + "\n");
      //txtAddIdEmpleado == Identificaion corporativa
      Console.WriteLine("Identificacion Corporativa del Empleado: " + txtEditIDEmpleado + "\n");
      //txtAddIdentificacionE == Cedula del empleado
      Console.WriteLine("Cedula del Empleado: " + txtEditIdentificacion + "\n");
      Console.WriteLine("Estado del Empleado: " + edit_employee_state + "\n");

      /// LOGICA DE BACKEND AQUI ///




      //Despues de hacer la logica se vuelve a la vista de Empleados
      return RedirectToAction("BusinessEmployeeList");
    }

  }
}
