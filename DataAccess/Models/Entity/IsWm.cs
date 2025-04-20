using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models.Entity;

[Table("Is_Wm")]
public class IsWm
{
    [Key]
    [Column("truename")]
    public string Truename { get; set; } = null!;

    [Column("is_wm")]
    public int? IsWm1 { get; set; }
}
