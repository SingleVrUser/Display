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

    void UpdateCompleted(VideoInfo info);

    void UpdateAllImagePathList(string srcPath, string dstPath);
    VideoInfo[] GetLookLaterList(int limit);

    VideoInfo[] GetLikeList(int limit);
    VideoInfo[] GetRandomList(int limit);
    VideoInfo[] GetRecentList(int limit);
}