using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models.Entity;

/// <summary>
/// 搜索历史
/// </summary>
[Table("search_histories")]
public class SearchHistory : BaseEntity
{
    [StringLength(20)] // 会被Fluent API替代
    public required string Keyword { get; init; }
}
