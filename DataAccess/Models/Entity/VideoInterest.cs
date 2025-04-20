using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models.Entity;

/// <summary>
/// 对当前视频的个性化标注（是否喜欢、是否稍后观看、评分）
/// </summary>
public class VideoInterest : BaseEntity
{
    [Column("video_id")]
    public long VideoInfoId { get; set; }
    
    public bool IsLike { get; set; }

    public bool IsLookAfter { get; set; }

    public double? Score { get; set; }
}