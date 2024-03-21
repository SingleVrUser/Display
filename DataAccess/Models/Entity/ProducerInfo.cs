using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models.Entity;

[Table("ProducerInfo")]
public class ProducerInfo
{
    [Key]
    public string Name { get; set; } = null!;

    [Column("is_wm", TypeName = "is_wm")]
    public int? IsWm { get; set; }
}
