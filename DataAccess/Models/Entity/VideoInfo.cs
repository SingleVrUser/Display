using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models.Entity;


[Table("video")]
public class VideoInfo : BaseEntity
{
    [StringLength(10)]
    public required string TrueName { get; init; }

    [StringLength(100)]
    public string? Title { get; set; }

    [StringLength(15)]
    public string? ReleaseTime { get; set; }

    [StringLength(15)]
    public string? LengthTime { get; set; }

    [StringLength(20)]
    public string? Director { get; set; }

    [StringLength(20)]
    public string? Producer { get; set; }

    [StringLength(20)]
    public string? Publisher { get; set; }

    [StringLength(100)]
    public string? Series { get; set; }

    [StringLength(100)]
    public string? Category { get; set; }

    [StringLength(100)]
    public string? Actor { get; set; }

    [StringLength(100)]
    public string? ImageUrl { get; set; }

    [StringLength(200)]
    public string? SampleImageList { get; set; }

    [StringLength(200)]
    public string ImagePath { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Url { get; set; }

    // 其他信息
    [NotMapped]
    public int IsWm { get; set; }
    
    public VideoInterest? Interest { get; set; }
    
    public List<FileInfo> FileInfos { get; set; }
    
    public List<ActorInfo> ActorInfos { get; set; }
    
    
    
}
