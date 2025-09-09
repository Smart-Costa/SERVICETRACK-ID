using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace AspnetCoreMvcFull.Models.Empresa
{
  public class EmpresaIndexVm
  {
    public AspnetCoreMvcFull.Models.Empresa.Empresa FormData { get; set; } = new();
    public IEnumerable<SelectListItem> EmpresasOptions { get; set; } = Enumerable.Empty<SelectListItem>();
    public List<AspnetCoreMvcFull.Models.Empresa.Empresa> Items { get; set; } = new();
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public string? Query { get; set; }
    public string? Todos { get; set; }
    public string? EstatusFilter { get; set; }
  }
  public class EmpresaListItem
  {
    public Guid IdEmpresa { get; set; }
    public string? Nombre { get; set; }
  }
}
