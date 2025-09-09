using System.Collections.Generic;

namespace AspnetCoreMvcFull.Models.Empresa
{
  public class ContactosIndexVm
  {
    public AspnetCoreMvcFull.Models.Contactos.Contactos FormData { get; set; } = new();
    public List<AspnetCoreMvcFull.Models.Contactos.Contactos> Items { get; set; } = new();
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public string? Query { get; set; }
  }
}
