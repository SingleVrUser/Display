using System.ComponentModel.DataAnnotations;

namespace DataAccess.Models.Entity;

/// <summary>
/// 搜索历史
/// </summary>
public class SearchHistory : BaseEntity
{
    [StringLength(20)] // 会被Fluent API替代
    public required string Keyword { get; init; }
}
