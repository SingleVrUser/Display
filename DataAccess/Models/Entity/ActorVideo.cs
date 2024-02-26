using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Models.Entity;

[PrimaryKey("ActorId", "VideoName")]
[Table("Actor_Video")]
public class ActorVideo
{
    [Key]
    [Column("actor_id")]
    public int ActorId { get; set; }

    [Key]
    [Column("video_name")]
    public string VideoName { get; set; } = null!;
}
