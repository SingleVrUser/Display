using DataAccess.Models.Entity;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Dao.Interface;

public interface IVideoInfoDao
{
    /// <summary>
    /// 添加视频信息，并与文件信息关联
    /// </summary>
    /// <param name="fileId"></param>
    /// <param name="videoInfo"></param>
    void AddAndSaveChanges(long fileId, VideoInfo videoInfo);

    void AddAndSaveChanges(VideoInfo videoInfo);

    void InitData();

    // Video? GetOneByTrueName(string name);
    //
    // List<Video>? GetInfoListByTrueName(string name);
    //
    // string? GetTrueNameByLikeName(string name);
    //
    // /// <summary>
    // /// 通过name删除
    // /// </summary>
    // /// <param name="name"></param>
    // void ExecuteRemoveByName(string name);
    //
    //
    // void UpdateAllImagePathList(string srcPath, string dstPath);
    // Task<Video[]> GetLookLaterListAsync(int limit);
    //
    // Task<Video[]> GetLikeListAsync(int limit);
    // Task<Video[]> GetRecentListAsync(int limit);
    //
    // void ExecuteUpdateByTrueName(string trueName, Action<Video> updateAction);
}