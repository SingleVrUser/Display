using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models.Entity;

[Table("ActorInfo")]
public class ActorInfo
{
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int Id { get; init; }

    [Column("name")]
    [StringLength(20)]
    public string Name { get; init; } = null!;

    [Column("is_woman")]
    public int? IsWoman { get; init; }

    [Column("birthday")]
    [StringLength(20)]
    public string? Birthday { get; init; }

    [Column("bwh")]
    [StringLength(10)]
    public string? Bwh { get; init; }

    [Column("height")]
    public int? Height { get; init; }

    [Column("works_count")]
    public int? WorksCount { get; init; }

    [Column("work_time")]
    [StringLength(5)]
    public string? WorkTime { get; init; }

    [Column("prifile_path")]
    [StringLength(500)]
    public string? PrifilePath { get; init; }

    [Column("blog_url")]
    [StringLength(500)]
    public string? BlogUrl { get; init; }

    [Column("is_like")]
    public int? IsLike { get; init; }

    [Column("addtime")]
    [StringLength(20)]
    public int? Addtime { get; init; }

    [Column("info_url")]
    [StringLength(100)]
    public string? InfoUrl { get; init; }
}
