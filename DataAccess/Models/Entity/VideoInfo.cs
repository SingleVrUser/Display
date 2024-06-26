﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models.Entity;

[Table("VideoInfo")]
public class VideoInfo
{
    [Key]
    [Column("truename")]
    public string TrueName { get; set; }

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
    public string Url { get; set; }

    [Column("look_later")]
    public long LookLater { get; set; }

    [Column("score")]
    public double Score { get; set; } = -1;

    [Column("is_like")]
    public int IsLike { get; set; }

    [Column("addtime")]
    public long AddTime { get; set; } = DateTimeOffset.Now.ToUnixTimeSeconds();

    // 其他信息

    [NotMapped]
    public int IsWm { get; set; }
}
