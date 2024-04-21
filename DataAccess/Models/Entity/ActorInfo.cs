using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models.Entity;

[Table("actor")]
public class ActorInfo : BaseEntity
{
    [StringLength(20)]
    public required string Name { get; init; }

    public bool IsWoman { get; set; } = true;

    [StringLength(20)]
    public string? Birthday { get; set; }

    
    public int? Height { get; set; }

    public int? WorksCount { get; set; }

    [StringLength(5)]
    public string? WorkTime { get; set; }

    [StringLength(100)]
    public string? ProfilePath { get; set; }

    [StringLength(500)]
    public string? BlogUrl { get; set; }

    public bool IsLike { get; init; }

    [StringLength(100)]
    public string? InfoUrl { get; set; }
    
    [ForeignKey(nameof(Bwh))]
    public long? BwhId { get; set; }
    
    // 其他信息
    
    [NotMapped]
    public int VideoCount { get; init; }
    
    [NotMapped]
    public List<string>? OtherNameList { get; set; }
    
    [NotMapped]
    public string Initials => string.Empty + Name.FirstOrDefault();
    
    public List<VideoInfo> VideoInfos { get; set; }
    
    public Bwh? BwhInfo { get; set; }
}
