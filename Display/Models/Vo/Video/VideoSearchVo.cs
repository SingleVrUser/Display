using DataAccess.Models.Dto;

namespace Display.Models.Vo.Video;

public class VideoSearchVo(VideoInfoDto videoInfoDto)
{
    public string ImageUrl { get; set; } = videoInfoDto.ImageUrl;
    public string Name { get; set; } = videoInfoDto.Name;

    public string ReleaseTime { get; set; } = videoInfoDto.ReleaseTime;

    public string ActorName { get; set; } = videoInfoDto.ActorNameList != null
        ? string.Join(",", videoInfoDto.ActorNameList)
        : string.Empty;
    
    public string CategoryName { get; set; } = videoInfoDto.CategoryList != null
        ? string.Join(",", videoInfoDto.CategoryList)
        : string.Empty;
}