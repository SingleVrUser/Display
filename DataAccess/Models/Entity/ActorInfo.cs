using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models.Entity;

/// <summary>
/// 演员信息
/// </summary>
[Table("actor")]
public class ActorInfo(string name) : BaseEntity
{
    [StringLength(20)]
    public string Name { get; init; } = name;

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

    [StringLength(100)]
    public string? InfoUrl { get; set; }
    
    
    // 其他信息
    
    // [NotMapped]
    // public int VideoCount { get; init; }
    //
    // [NotMapped]
    // public List<string>? OtherNameList { get; set; }
    //
    // [NotMapped]
    // public bool IsLike { get; init; }
    
    [NotMapped]
    public string Initials => string.Empty + Name.FirstOrDefault();
    
    /// <summary>
    /// 身材信息
    /// </summary>
    public BwhInfo? Bwh { get; set; }
    
    public List<ActorName> NameList { get; set; }
    
    /// <summary>
    /// 个性化标注
    /// </summary>
    public ActorInterest Interest { get; set; }
    
    // 其他信息
    public List<VideoInfo> VideoInfos { get; set; }
}
