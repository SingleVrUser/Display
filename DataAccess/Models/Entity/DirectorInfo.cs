using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models.Entity;

/// <summary>
/// 导演
/// </summary>
/// 
[Table("director")]
public class DirectorInfo(string name) : BaseInfoEntity(name)
{
}