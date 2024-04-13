using DataAccess.Models.Entity;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Dao.Interface;

public interface IVideoInfoDao
{
    Video? GetOneByTrueName(string name);
    
    List<Video>? GetInfoListByTrueName(string name);

    string? GetTrueNameByLikeName(string name);

    /// <summary>
    /// 通过name删除
    /// </summary>
    /// <param name="name"></param>
    void ExecuteRemoveByName(string name);


    void UpdateAllImagePathList(string srcPath, string dstPath);
    Task<Video[]> GetLookLaterListAsync(int limit);

    Task<Video[]> GetLikeListAsync(int limit);
    Task<Video[]> GetRecentListAsync(int limit);

    void ExecuteUpdateByTrueName(string trueName, Action<Video> updateAction);
}