using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models.Entity;

/// <summary>
/// 身材数据
/// </summary>
[Table("bwh")]
public class BwhInfo : BaseEntity
{
    public int Bust { get; init; }

    public int Waist { get; init; }

    public int Hips { get; init; }
}
