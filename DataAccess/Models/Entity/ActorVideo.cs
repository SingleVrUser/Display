using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models.Entity;
    
/// <summary>
/// 演员和视频的中间表
/// 演员id - 视频id，多对多
/// </summary>
public class ActorVideo
{
    [Column("actor_id")]
    public long ActorInfoId { get; set; }

    [Column("video_id")]
    public long VideoInfoId { get; set; }
}
