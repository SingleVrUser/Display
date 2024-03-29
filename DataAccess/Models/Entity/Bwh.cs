using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models.Entity;

[Table("bwh")]
public class Bwh
{
    [Key]
    [Column("bwh")]
    [StringLength(100)]
    public string Bwh1 { get; set; } = null!;

    [Column("bust")]
    public int? Bust { get; set; }

    [Column("waist")]
    public int? Waist { get; set; }

    [Column("hips")]
    public int? Hips { get; set; }
}
