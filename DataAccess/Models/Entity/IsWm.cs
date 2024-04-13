using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Models.Entity;

public class IsWm
{
    [Key]
    public long Id { get; init; }
    
    [StringLength(10)]
    public required string TrueName { get; set; }

    public bool IsVideoWm { get; set; }
}
