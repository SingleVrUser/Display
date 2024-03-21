using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models.Entity;

[Table("SpiderTask")]
public class SpiderTask
{
    [Key]
    public string Name { get; set; } = null!;

    [Column("bus")]
    public string? Bus { get; set; }

    public string? Jav321 { get; set; }

    public string? Avmoo { get; set; }

    public string? Avsox { get; set; }

    [Column("libre")]
    public string? Libre { get; set; }

    [Column("fc")]
    public string? Fc { get; set; }

    [Column("db")]
    public string? Db { get; set; }

    [Column("done")]
    public int? Done { get; set; }

    [Column("tadk_id")]
    public int? TadkId { get; set; }
}
