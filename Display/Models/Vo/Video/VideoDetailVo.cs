using System.Collections.Generic;
using System.Linq;
using DataAccess.Models.Entity;

namespace Display.Models.Vo.Video;

public class VideoDetailVo(VideoInfo videoInfo)
{
    public long Id { get; init; } = videoInfo.Id;

    public string Title { get; set; } = videoInfo.Title;

    public string Name { get; set; } = videoInfo.Name;

    public string ReleaseTime { get; set; } = videoInfo.ReleaseTime;

    public double Score { get; set; } = videoInfo.Interest.Score ?? -1;

    public bool IsLookLater { get; set; } = videoInfo.Interest.IsLookAfter;
    
    public bool IsLike { get; set; } = videoInfo.Interest.IsLike;

    public string ImageUrl { get; set; } = videoInfo.ImageUrl;
    
    public string ImagePath { get; set; } = videoInfo.ImagePath ?? Constants.FileType.NoPicturePath;

    public string SourceUrl { get; set; } = videoInfo.SourceUrl;

    public string LengthTime { get; set; } = videoInfo.LengthTime;

    public string Director { get; set; } = videoInfo.Director?.Name ?? string.Empty;
    
    public string Series { get; set; } = videoInfo.Series?.Name ?? string.Empty;
    
    public string Publisher { get; set; } = videoInfo.Publisher?.Name ?? string.Empty;
    
    public string Producer { get; set; } = videoInfo.Producer?.Name ?? string.Empty;

    public List<ActorInfo> ActorInfoList { get; set; } = videoInfo.ActorInfoList;

    public string ActorName { get; set; } = videoInfo.ActorInfoList.Count > 0
        ? videoInfo.ActorInfoList.First().Name
        : string.Empty;
    
    public string CategoryName { get; set; } = videoInfo.CategoryList is { Count: > 0 }
        ? videoInfo.CategoryList.First().Name
        : string.Empty;

    public List<CategoryInfo> CategoryList { get; set; } = videoInfo.CategoryList ?? [];
}   