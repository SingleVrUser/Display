using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models.Entity;

/// <summary>
/// 发布者
/// </summary>
[Table("publisher")]
public class PublisherInfo(string name) : BaseInfoEntity(name)
{
}