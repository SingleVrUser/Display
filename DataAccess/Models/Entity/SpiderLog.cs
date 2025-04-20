using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models.Entity;

[Table("SpiderLog")]
public class SpiderLog
{
    [Key]
    [Column("task_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int TaskId { get; set; }

    [Column("time")]
    public string? Time { get; set; }

    [Column("done")]
    public string? Done { get; set; }
}
