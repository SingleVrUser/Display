using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models.Entity;

/// <summary>
/// 视频信息
/// </summary>
[Table("video")]
public class VideoInfo(string name) : BaseEntity
{
    [StringLength(10)]
    public string Name { get; init; } = name;

    [StringLength(100)]
    public string? Title { get; set; }

    [StringLength(15)]
    public string? ReleaseTime { get; set; }

    [StringLength(15)]
    public string? LengthTime { get; set; }
    
    [StringLength(100)]
    public string? ImageUrl { get; set; }

    [StringLength(500)]
    public string? SampleImages { get; set; }

    /// <summary>
    /// 下载到本地的相对路径
    /// </summary>
    [StringLength(200)]
    public string? ImagePath { get; set; }

    /// <summary>
    /// 信息地址
    /// </summary>
    [StringLength(200)]
    public string? SourceUrl { get; set; }

    /// <summary>
    /// 导演
    /// </summary>
    public DirectorInfo? Director { get; set; }

    /// <summary>
    /// 厂商
    /// </summary>
    public ProducerInfo? Producer { get; set; }

    /// <summary>
    /// 发布者
    /// </summary>
    public PublisherInfo? Publisher { get; set; }
    
    /// <summary>
    /// 系列
    /// </summary>
    public SeriesInfo? Series { get; set; }

    /// <summary>
    /// 兴趣
    /// </summary>
    public VideoInterest? Interest { get; set; }
    
    /// <summary>
    /// 文件列表
    /// </summary>
    public List<FileInfo>? FileInfoList { get; set; }
    
    /// <summary>
    /// 演员列表
    /// </summary>
    public List<ActorInfo>? ActorInfoList { get; set; }
    
    /// <summary>
    /// 标签列表
    /// </summary>
    public List<CategoryInfo>? CategoryList { get; set; }
    
    // 其他信息
    // 该选项的优先级比厂商的优先级高
    [NotMapped]
    public bool? IsWm { get; set; }
}
