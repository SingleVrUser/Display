using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models.Entity;

[Table("VideoInfo")]
public class VideoInfo
{
    [Key]
    [Column("truename")]
    public string TrueName { get; set; } = null!;

    [Column("title")]
    public string? Title { get; set; }

    [Column("releasetime")]
    public string? ReleaseTime { get; set; }

    [Column("lengthtime")]
    public string? LengthTime { get; set; }

    [Column("director")]
    public string? Director { get; set; }

    [Column("producer")]
    public string? Producer { get; set; }

    [Column("publisher")]
    public string? Publisher { get; set; }

    [Column("series")]
    public string? Series { get; set; }

    [Column("category")]
    public string? Category { get; set; }

    [Column("actor")]
    public string? Actor { get; set; }

    [Column("imageurl")]
    public string? ImageUrl { get; set; }

    [Column("sampleImageList")]
    public string? SampleImageList { get; set; }

    [Column("imagepath")]
    public string? ImagePath { get; set; }

    [Column("busurl")]
    public string? Url { get; set; }

    [Column("look_later")]
    public int? LookLater { get; set; }

    [Column("score")]
    public int? Score { get; set; }

    [Column("is_like")]
    public int? IsLike { get; set; }

    [Column("addtime")]
    public int? AddTime { get; set; }
}
