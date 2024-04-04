using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models.Entity;

[Table("SearchHistory")]
[Index(nameof(Keyword), IsUnique = true)]
public class SearchHistory
{
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int Id { get; init; }

    [Column("keyword", TypeName = "TEXT NO")]
    public string? Keyword { get; init; }
}
