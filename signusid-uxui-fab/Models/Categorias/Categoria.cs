using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AspnetCoreMvcFull.Models.Categorias
{
  public class Categoria
  {
    [Key]
    public Guid AssetCategorySysId { get; set; }

    [Required]
    [StringLength(50)]
    public string Name { get; set; }

    [Required]
    [StringLength(150)]
    public string Description { get; set; }

    [Required]
    public Guid EntryUser { get; set; }

    [Required]
    public DateTime EntryDate { get; set; }

    [Required]
    public Guid UpdateUser { get; set; }

    [Required]
    public DateTime UpdateDate { get; set; }

    [Required]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid RowGuid { get; set; }

    public int? ValorVidaUtil { get; set; }

    [StringLength(100)]
    public string VidaUtilProcomer { get; set; }

    [StringLength(60)]
    public string CompanyIdExtern { get; set; }

    public Guid? DepSysId { get; set; }

    public int Actives { get; set; }
  }
}
