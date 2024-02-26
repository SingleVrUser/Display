using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models.Entity;

[Table("ActorInfo")]
public class ActorInfo
{
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    [Column("is_woman")]
    public int? IsWoman { get; set; }

    [Column("birthday")]
    public string? Birthday { get; set; }

    [Column("bwh")]
    public string? Bwh { get; set; }

    [Column("height")]
    public int? Height { get; set; }

    [Column("works_count")]
    public int? WorksCount { get; set; }

    [Column("work_time")]
    public string? WorkTime { get; set; }

    [Column("prifile_path")]
    public string? PrifilePath { get; set; }

    [Column("blog_url")]
    public string? BlogUrl { get; set; }

    [Column("is_like")]
    public int? IsLike { get; set; }

    [Column("addtime")]
    public int? Addtime { get; set; }

    [Column("info_url")]
    public string? InfoUrl { get; set; }
}
