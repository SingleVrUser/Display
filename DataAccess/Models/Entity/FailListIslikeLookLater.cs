using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models.Entity;

[Table("FailList_islike_looklater")]
public class FailListIslikeLookLater
{
    [Key]
    [Column("pc")]
    [StringLength(20)]
    public string Pc { get; set; } = null!;

    [Column("is_like")]
    public int? IsLike { get; set; }

    [Column("score")]
    public int? Score { get; set; }

    [Column("look_later")]
    public int? LookLater { get; set; }

    [Column("image_path")]
    [StringLength(500)]
    public string? ImagePath { get; set; }
}
