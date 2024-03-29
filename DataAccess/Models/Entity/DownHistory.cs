using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models.Entity;

[PrimaryKey("FilePickcode", "Ua")]
[Table("DownHistory")]
public class DownHistory
{
    [Key]
    [Column("file_pickcode")]
    [StringLength(500)]
    public string FilePickcode { get; set; } = null!;

    [Column("file_name")]
    [StringLength(500)]
    public string? FileName { get; set; }

    [Column("true_url")]
    [StringLength(500)]
    public string? TrueUrl { get; set; }

    [Key]
    [Column("ua")]
    [StringLength(500)]
    public string Ua { get; set; } = null!;

    [Column("add_time")]
    public int? AddTime { get; set; }
}
