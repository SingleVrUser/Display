
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models.Entity;

/// <summary>
/// 演员和视频的中间表
/// 演员id - 视频id，多对多
/// </summary>
public class CategoryVideo
{
    [Column("category_id")]
    public long CategoryInfoId { get; set; }

    [Column("video_id")]
    public long VideoInfoId { get; set; }
}

