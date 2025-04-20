using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Models.Entity;

/// <summary>
/// 厂商信息
/// </summary>
[Table("producer")]
public class ProducerInfo(string name) : BaseInfoEntity(name)
{
    public bool IsWm { get; set; }
}