using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models.Entity;

[Table("bwh")]
public class Bwh
{
    [Key]
    [Column("bwh")]
    [StringLength(100)]
    public string BwhContent { get; init; } = null!;

    [Column("bust")]
    public int Bust { get; init; }

    [Column("waist")]
    public int Waist { get; init; }

    [Column("hips")]
    public int Hips { get; init; }
}
