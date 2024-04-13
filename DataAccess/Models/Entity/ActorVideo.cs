using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace DataAccess.Models.Entity;

public class ActorVideo
{
    [Key]
    public long Id { get; init; }
    
    public long ActorId { get; init; }

    [StringLength(20)]
    public required string VideoName { get; init; }
}
