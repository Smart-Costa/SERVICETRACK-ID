using System.Collections.Generic;

namespace AspnetCoreMvcFull.Models.Empresa
{
  public class EmpresaIndexVm
  {
    public AspnetCoreMvcFull.Models.Empresa.Empresa FormData { get; set; } = new();
    public List<AspnetCoreMvcFull.Models.Empresa.Empresa> Items { get; set; } = new();
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public string? Query { get; set; }
  }
}
