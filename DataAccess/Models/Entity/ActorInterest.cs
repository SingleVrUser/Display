using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models.Entity;

/// <summary>
/// 对当前视频的个性化标注（是否喜欢、评分）
/// </summary>
public class ActorInterest : BaseEntity
{
    public bool IsLike { get; set; }

    public double? Score { get; set; }
}