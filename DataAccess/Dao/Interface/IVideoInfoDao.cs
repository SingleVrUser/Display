using DataAccess.Models.Entity;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Dao.Interface;

public interface IVideoInfoDao : IDao<VideoInfo>
{
    VideoInfo? GetOneByTrueName(string name);
    
    List<VideoInfo>? GetInfoListByTrueName(string name);

    string? GetTrueNameByLikeName(string name);

    /// <summary>
    /// 通过name删除
    /// </summary>
    /// <param name="name"></param>
    void ExecuteRemoveByName(string name);


    void UpdateAllImagePathList(string srcPath, string dstPath);
    Task<VideoInfo[]> GetLookLaterListAsync(int limit);

    Task<VideoInfo[]> GetLikeListAsync(int limit);
    Task<VideoInfo[]> GetRecentListAsync(int limit);

    void ExecuteUpdateByTrueName(string trueName, Action<VideoInfo> updateAction);
}