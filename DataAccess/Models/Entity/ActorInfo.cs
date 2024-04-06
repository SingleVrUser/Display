using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models.Entity;

[Table("ActorInfo")]
public class ActorInfo 
{
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [Column("name")]
    [StringLength(20)]
    public string Name { get; init; } = null!;

    [Column("is_woman")]
    public int? IsWoman { get; set; } = 1;

    [Column("birthday")]
    [StringLength(20)]
    public string? Birthday { get; set; }

    [Column("bwh")]
    [StringLength(10)]
    public string? Bwh { get; set; }
    
    [Column("height")]
    public int? Height { get; set; }

    [Column("works_count")]
    public int? WorksCount { get; set; }

    [Column("work_time")]
    [StringLength(5)]
    public string? WorkTime { get; set; }

    [Column("prifile_path")]
    [StringLength(500)]
    public string? ProfilePath { get; set; }

    [Column("blog_url")]
    [StringLength(500)]
    public string? BlogUrl { get; set; }

    [Column("is_like")]
    public int IsLike { get; init; }

    [Column("addtime")]
    [StringLength(20)]
    public long? AddTime { get; init; } = DateTimeOffset.Now.ToUnixTimeSeconds();

    [Column("info_url")]
    [StringLength(100)]
    public string? InfoUrl { get; set; }
    
    // 其他信息
    [NotMapped]
    public Bwh BwhInfo { get; set; }
    
    [NotMapped]
    public int VideoCount { get; init; }
    
    [NotMapped]
    public List<string> OtherNameList { get; set; }
    
    [NotMapped]
    public string Initials => string.Empty + Name.FirstOrDefault();
}
