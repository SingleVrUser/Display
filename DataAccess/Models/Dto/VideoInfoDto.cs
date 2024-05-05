namespace DataAccess.Models.Dto;

public class VideoInfoDto
{
    /// <summary>
    /// 名称
    /// </summary>
    public required string Name { get; init; }
    
    /// <summary>
    /// 标题
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// 发布时间
    /// </summary>
    public string? ReleaseTime { get; set; }

    /// <summary>
    /// 时长
    /// </summary>
    public string? LengthTime { get; set; }
    
    public required string SourceUrl { get; init; }

    /// <summary>
    /// 导演名称
    /// </summary>
    public string? DirectorName { get; set; }

    /// <summary>
    /// 厂商名称
    /// </summary>
    public string? ProducerName { get; set; }

    /// <summary>
    /// 发行者名称
    /// </summary>
    public string? PublisherName { get; set; }
    
    /// <summary>
    /// 系列名称
    /// </summary>
    public string? SeriesName { get; set; }

    /// <summary>
    /// 封面图片
    /// </summary>
    public string? ImageUrl { get; set; }
    
    /// <summary>
    /// 下载好的图片
    /// </summary>
    public string? ImagePath { get; set; }
    
    public bool IsWm { get; set; }
    
    /// <summary>
    /// 演员名称列表
    /// </summary>
    public List<string>? ActorNameList { get; set; }

    /// <summary>
    /// 标签列表
    /// </summary>
    public List<string>? CategoryList { get; set; }
    
    /// <summary>
    /// 预览图列表
    /// </summary>
    public List<string>? SampleImageList { get; set; }
    
    
}