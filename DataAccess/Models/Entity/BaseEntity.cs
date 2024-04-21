using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Models.Entity;

// 与fluent API混用（为了time）
[PrimaryKey(nameof(Id))]
public abstract class BaseEntity
{
    public long Id { get; set; }
    
    public DateTime CreateTime { get; set; }
    
    public DateTime UpdateTime { get; set; }
}