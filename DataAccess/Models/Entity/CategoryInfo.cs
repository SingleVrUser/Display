namespace DataAccess.Models.Entity;

/// <summary>
/// 标签
/// </summary>
public class CategoryInfo(string name) : BaseInfoEntity(name)
{
    // 其他信息
    public List<VideoInfo>? VideoInfos { get; set; }
}