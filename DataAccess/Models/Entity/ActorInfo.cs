using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DataAccess.Models.Entity;

public class ActorInfo : BaseEntity
{
    [StringLength(20)]
    public required string Name { get; init; }

    public bool IsWoman { get; set; } = true;

    [StringLength(20)]
    public string? Birthday { get; set; }

    [StringLength(10)]
    public string? Bwh { get; set; }
    
    public int? Height { get; set; }

    public int? WorksCount { get; set; }

    [StringLength(5)]
    public string? WorkTime { get; set; }

    [StringLength(100)]
    public string? ProfilePath { get; set; }

    [StringLength(500)]
    public string? BlogUrl { get; set; }

    public bool IsLike { get; init; }

    [Column("info_url")]
    [StringLength(100)]
    public string? InfoUrl { get; set; }
    
    // 其他信息
    [NotMapped]
    public Bwh? BwhInfo { get; set; }
    
    [NotMapped]
    public int VideoCount { get; init; }
    
    [NotMapped]
    public List<string>? OtherNameList { get; set; }
    
    [NotMapped]
    public string Initials => string.Empty + Name.FirstOrDefault();
}
