using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AspnetCoreMvcFull.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AspnetCoreMvcFull.Controllers;


public class CategoryController : Controller
{
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
  public Category[] filter_categories_list(Category[] categories_list, string categories_search_input, string category_actives_state)
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
  public List<List<Category>> create_categoriespages_from_categories_list(Category[] categories_list, int categories_per_page)
  {

    //Lista de paginas de categorias divididas segun la cantidad seleccionada en la vista
    List<List<Category>> Categories_Pages = new List<List<Category>>();

    //LOOP PARA DIVIDIR LA LISTA DE CATEGORIAS EN PAGINAS DE LA CANTIDAD SELECCIONADA
    for (int i = 0; i < categories_list.Length; i = i + categories_per_page)
    {
      //PAGINA CORRESPONDIENTE A ITERACION
      List<Category> categories_page = new List<Category>();

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
  public Category[] order_categorieslist_by(Category[] categories_list, string order_by)
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
    Category[] categories_list_from_JSON = Get_dummy_categories_data_from_JSON();

    //Se llama al metodo para filtrar las categorias segun Nombre
    Category[] filtered_categories_list =
    filter_categories_list(categories_list_from_JSON, categories_search_input, category_actives_state);


    //Se orderna el array de categorias despues de ser filtrado
    Category[] filtered_categories_list_ordered = order_categorieslist_by(filtered_categories_list, order_by);



    //Se llama al metodo que crea la paginacion de la lista de categorias segun los parametros designados
    List<List<Category>> Categories_Pages = create_categoriespages_from_categories_list(filtered_categories_list_ordered, categories_per_page);

    //Definir la variable que va a contener las categorias de la pagina a mostrar
    Category[] selected_categories_page = [];

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


    //RETORNAR A LA VISTA CON EL ARRAY DE CATEGORIAS FILTRADOS Y ORDERNADOS DE LA PAGINA SELECCIONADA
    return View(selected_categories_page);

  }



  [HttpPost]
  public IActionResult DeleteCategory(string category_name = "")
  {
    if (String.IsNullOrEmpty(category_name))
    {
      Console.WriteLine("No se recibió una categoría");
    }
    else
    {
      Console.WriteLine("Eliminar Categoria: " + category_name);
    }

    return RedirectToAction("ListCategories");
  }

  [HttpPost]
  public IActionResult DeleteMultipleCategories(string categories_names_string = "")
  {

    if (String.IsNullOrEmpty(categories_names_string))
    {
      Console.WriteLine("No se recibió ninguna categoría");
    }
    else
    {

      Console.WriteLine("Eliminar Categorias: ");
      string[] categories_names = categories_names_string.Split('$');

      foreach (string category_name in categories_names)
      {
        Console.WriteLine(category_name);
      }
    }

    return RedirectToAction("ListCategories");
  }



  [HttpPost]
  public IActionResult AddCategory(string add_category_name = "", string add_category_description = "")
  {
    if (String.IsNullOrEmpty(add_category_name) || String.IsNullOrEmpty(add_category_description))
    {
      Console.WriteLine("No se recibieron datos validos para registrar una categoría");
    }
    else
    {
      Console.WriteLine("Registrar Categoria: " + add_category_name);
      Console.WriteLine("Descripcion Categoria: " + add_category_description);
    }

    return RedirectToAction("ListCategories");
  }

  [HttpPost]
  public IActionResult EditCategory(string category_to_edit, string edit_category_name = "", string edit_category_description = "")
  {
    if (String.IsNullOrEmpty(category_to_edit) || String.IsNullOrEmpty(edit_category_name) || String.IsNullOrEmpty(edit_category_description))
    {
      Console.WriteLine("No se recibieron datos validos para editar una categoría");
    }
    else
    {
      Console.WriteLine("Editar Categoria: " + category_to_edit);
      Console.WriteLine("Nombre nuevo: " + edit_category_name);
      Console.WriteLine("Descripcion nueva: " + edit_category_description);
    }

    return RedirectToAction("ListCategories");
  }



}
