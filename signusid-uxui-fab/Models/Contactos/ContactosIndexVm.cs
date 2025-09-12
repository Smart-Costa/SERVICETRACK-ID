using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace AspnetCoreMvcFull.Models.Empresa
{
  public class ContactosIndexVm
  {
    public AspnetCoreMvcFull.Models.Contactos.Contactos FormData { get; set; } = new();
    public List<AspnetCoreMvcFull.Models.Contactos.Contactos> Items { get; set; } = new();
    public IEnumerable<SelectListItem> EmpresasOptions { get; set; } = Enumerable.Empty<SelectListItem>();
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public string? Query { get; set; }
    public string? Todos { get; set; }
    public string? EstatusFilter { get; set; }
  }

}
