using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models.Entity;

/// <summary>
/// 系列
/// </summary>
[Table("series")]
public class SeriesInfo(string name) : BaseInfoEntity(name)
{
}