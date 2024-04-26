using DataAccess.Models.Entity;
using DataAccess.Models.Vo;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Dao.Interface;

public interface IVideoInfoDao : IBaseDao<VideoInfo>
{
    /// <summary>
    /// 添加视频信息，并与文件信息关联
    /// </summary>
    /// <param name="fileId"></param>
    /// <param name="videoInfo"></param>
    void AddAndSaveChanges(long fileId, VideoInfo videoInfo);

    /// <summary>
    /// 通过id获取视频信息
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    VideoInfo? GetById(long id);

    /// <summary>
    /// 通过名字获取信息
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    VideoInfo? GetOneByName(string name);

    /// <summary>
    /// 通过名字判断是否存在
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    bool IsExistsName(string name);

    /// <summary>
    /// 通过VideoInfoVo添加数据
    /// </summary>
    void AddByVideoInfoVo(VideoInfoVo vo);

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