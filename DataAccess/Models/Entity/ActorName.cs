using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace DataAccess.Models.Entity;

public class ActorName
{
    [Key]
    public long Id { get; init; }
    
    public long ActorId { get; init; }
    
    [StringLength(10)]
    public required string Name { get; init; }
}
