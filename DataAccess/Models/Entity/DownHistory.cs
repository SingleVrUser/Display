using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models.Entity;

[Table("tmp_down_histories")]
public class DownHistory : BaseEntity
{
    [StringLength(18)]
    public string FilePickCode { get; set; } = null!;

    [StringLength(500)]
    public required string FileName { get; init; }

    [StringLength(500)]
    public string? TrueUrl { get; set; }

    [StringLength(200)]
    public string Ua { get; set; } = null!;
}
