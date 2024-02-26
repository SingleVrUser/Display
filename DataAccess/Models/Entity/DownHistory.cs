using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Models.Entity;

[PrimaryKey("FilePickcode", "Ua")]
[Table("DownHistory")]
public partial class DownHistory
{
    [Key]
    [Column("file_pickcode")]
    public string FilePickcode { get; set; } = null!;

    [Column("file_name")]
    public string? FileName { get; set; }

    [Column("true_url")]
    public string? TrueUrl { get; set; }

    [Key]
    [Column("ua")]
    public string Ua { get; set; } = null!;

    [Column("add_time")]
    public int? AddTime { get; set; }
}
