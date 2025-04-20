using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models.Entity;

[PrimaryKey(nameof(Id), nameof(Name))]
[Table("Actor_Names")]
public class ActorName
{
    [Key]
    [Column("id")]
    public long Id { get; init; }

    [Key]
    [StringLength(20)]
    [Column("Name")]
    public string Name { get; init; } = null!;
}
