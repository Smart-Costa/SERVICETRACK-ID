using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AspnetCoreMvcFull.Models;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace AspnetCoreMvcFull.Controllers;

public class BrandController : Controller
{

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
  public Brand[] filter_brands_list(Brand[] brands_list, string brand_search_input, string brand_actives_state)
  {

    //Filtrar segun los parametros de texto de busqueda
    // NOMBRE
    if (!string.IsNullOrEmpty(brand_search_input))
    {
      brands_list = brands_list
        .Where(brand =>
        {
          return
          brand.Name.ToLower().Contains(brand_search_input.ToLower());
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
  public List<List<Brand>> create_brandspages_from_brands_list(Brand[] brands_list, int brands_per_page)
  {

    //Lista de paginas de marcas divididas segun la cantidad seleccionada en la vista
    List<List<Brand>> Brands_Pages = new List<List<Brand>>();

    //LOOP PARA DIVIDIR LA LISTA DE MARCAS EN PAGINAS DE LA CANTIDAD SELECCIONADA
    for (int i = 0; i < brands_list.Length; i = i + brands_per_page)
    {
      //PAGINA CORRESPONDIENTE A ITERACION
      List<Brand> brands_page = new List<Brand>();

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
  public Brand[] order_brandslist_by(Brand[] brands_list, string order_by)
  {

    // se realiza un switch para determinar que tipo de orden se require
    switch (order_by)
    {

      case "name_ascending":
        // Ordenar alfabéticamente ascendentemente segun Nombre, ignorando mayúsculas y minúsculas
        brands_list = brands_list.OrderBy(brand => brand.Name, StringComparer.OrdinalIgnoreCase).ToArray();
        break;

      case "name_descending":
        // Ordenar alfabéticamente descendentemente segun Nombre, ignorando mayúsculas y minúsculas
        brands_list = brands_list.OrderByDescending(brand => brand.Name, StringComparer.OrdinalIgnoreCase).ToArray();
        break;

      default:
        // Ordenar alfabéticamente segun Nombre, ignorando mayúsculas y minúsculas
        brands_list = brands_list.OrderBy(brand => brand.Name, StringComparer.OrdinalIgnoreCase).ToArray();
        break;
    }

    return brands_list;
  }




  [HttpGet]
  public IActionResult ListBrands(string brand_search_input = "", string order_by = "name_ascending",
  int brands_per_page = 10, int page_number = 1, string brand_actives_state = "")
  {

    //Se llama al metodo para obtener los datos del JSON
    Brand[] brands_list_from_JSON = Get_dummy_brands_data_from_JSON();

    //Se llama al metodo para filtrar las marcas segun Nombre
    Brand[] filtered_brands_list =
    filter_brands_list(brands_list_from_JSON, brand_search_input, brand_actives_state);


    //Se orderna el array de marcas despues de ser filtrado
    Brand[] filtered_brands_list_ordered = order_brandslist_by(filtered_brands_list, order_by);



    //Se llama al metodo que crea la paginacion de la lista de marcas segun los parametros designados
    List<List<Brand>> Brands_Pages = create_brandspages_from_brands_list(filtered_brands_list_ordered, brands_per_page);

    //Definir la variable que va a contener las marcas de la pagina a mostrar
    Brand[] selected_brands_page = [];

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


    //RETORNAR A LA VISTA CON EL ARRAY DE MARCAS FILTRADOS Y ORDERNADOS DE LA PAGINA SELECCIONADA
    return View(selected_brands_page);


  }




  [HttpPost]
  public IActionResult DeleteBrand(string brand_name = "")
  {
    if (String.IsNullOrEmpty(brand_name))
    {
      Console.WriteLine("No se recibió una marca");
    }
    else
    {
      Console.WriteLine("Eliminar Marca: " + brand_name);
    }

    return RedirectToAction("ListBrands");
  }

  [HttpPost]
  public IActionResult DeleteMultipleBrands(string brands_names_string = "")
  {

    if (String.IsNullOrEmpty(brands_names_string))
    {
      Console.WriteLine("No se recibió ninguna marca");
    }
    else
    {

      Console.WriteLine("Eliminar Marcas: ");
      string[] brands_names = brands_names_string.Split('$');

      foreach (string brand_name in brands_names)
      {
        Console.WriteLine(brand_name);
      }
    }

    return RedirectToAction("ListBrands");
  }


  [HttpPost]
  public IActionResult AddBrand(string add_brand_name = "", string add_brand_description = "")
  {
    if (String.IsNullOrEmpty(add_brand_name) || String.IsNullOrEmpty(add_brand_description))
    {
      Console.WriteLine("No se recibieron datos validos para registrar una marca.");
    }
    else
    {
      Console.WriteLine("Registrar Marca: " + add_brand_name);
      Console.WriteLine("Descripcion Marca: " + add_brand_description);
    }

    return RedirectToAction("ListBrands");
  }



  [HttpPost]
  public IActionResult EditBrand(string brand_to_edit, string edit_brand_name = "", string edit_brand_description = "")
  {
    if (String.IsNullOrEmpty(brand_to_edit) || String.IsNullOrEmpty(edit_brand_name) || String.IsNullOrEmpty(edit_brand_description))
    {
      Console.WriteLine("No se recibieron datos validos para editar una marca.");
    }
    else
    {
      Console.WriteLine("Editar Marca: " + brand_to_edit);
      Console.WriteLine("Nuevo nombre de Marca: " + edit_brand_name);
      Console.WriteLine("nueva descripcion de Marca: " + edit_brand_description);
    }

    return RedirectToAction("ListBrands");
  }




}
