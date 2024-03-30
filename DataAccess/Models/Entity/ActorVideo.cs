using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
    [StringLength(20)]
    public string VideoName { get; set; } = null!;
}
