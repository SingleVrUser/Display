using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models.Entity;
    
public class ActorVideo
{
    [Column("actor_id")]
    public long ActorInfoId { get; set; }

    [Column("video_id")]
    public long VideoInfoId { get; set; }
}
