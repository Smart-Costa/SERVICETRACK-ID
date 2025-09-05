// Models/Common/PagedResult.cs
using System;
using System.Collections.Generic;

namespace AspnetCoreMvcFull.Models.Common
{
  public class PagedResult<T>
  {
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages => Math.Max(1, (int)Math.Ceiling((double)TotalItems / Math.Max(1, PageSize)));
    public string? Query { get; set; }   // <-- para conservar el término de búsqueda
  }

}
