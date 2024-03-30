using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models.Entity;

[PrimaryKey("Id", "Name")]
[Table("Actor_Names")]
public class ActorName
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Key]
    [StringLength(20)]
    public string Name { get; set; } = null!;
}
