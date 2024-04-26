using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models.Entity;

/// <summary>
/// 演员名称
/// 演员id - 名称，一对多
/// </summary>
public class ActorName
{
    [Key]
    public long Id { get; init; }
    
    [Column("actor_id")]
    public long ActorInfoId { get; init; }
    
    [StringLength(10)]
    public required string Name { get; init; }
}
