using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models.Entity;

[PrimaryKey(nameof(ActorId), nameof(VideoName))]
[Table("Actor_Video")]
public class ActorVideo
{
    [Key]
    [Column("actor_id")]
    public long ActorId { get; init; }

    [Key]
    [Column("video_name")]
    [StringLength(20)]
    public string VideoName { get; init; } = null!;
}
