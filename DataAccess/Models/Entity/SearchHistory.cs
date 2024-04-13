using System.ComponentModel.DataAnnotations;

namespace DataAccess.Models.Entity;

public class SearchHistory : BaseEntity
{
    [StringLength(20)] // 会被Fluent API替代
    public required string Keyword { get; init; }
}
