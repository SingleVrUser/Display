using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models.Entity;

[Table("SearchHistory")]
[Index("Keyword", IsUnique = true)]
public class SearchHistory
{
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int Id { get; set; }

    [Column("keyword", TypeName = "TEXT NO")]
    public string? Keyword { get; set; }
}
