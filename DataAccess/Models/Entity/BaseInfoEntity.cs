using System.ComponentModel.DataAnnotations;

namespace DataAccess.Models.Entity;

/// <summary>
/// 信息的基本类
/// </summary>
public abstract class BaseInfoEntity (string name) : BaseEntity
{
    // 名称
    [StringLength(20)]
    public string Name { get; init; } = name;

}