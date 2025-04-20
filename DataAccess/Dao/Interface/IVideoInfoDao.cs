using DataAccess.Models.Entity;
using DataAccess.Models.Dto;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Dao.Interface;

public interface IVideoInfoDao : IBaseDao<VideoInfo>
{
    /// <summary>
    /// 添加视频信息，并与文件信息关联
    /// </summary>
    /// <param name="dto"></param>
    /// <param name="fileIdList"></param>
    void AddOrUpdateInfoAndAttachFile(VideoInfoDto dto, List<long> fileIdList);

    /// <summary>
    /// 通过VideoInfoVo添加数据
    /// </summary>
    void AddOrUpdateByVideoInfoVo(VideoInfoDto dto);

    
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
    /// 通过id删除数据
    /// </summary>
    /// <param name="id"></param>
    void ExecuteDeleteById(long id);

    /// <summary>
    /// 获取首页展示的列表
    /// </summary>
    /// <param name="offset"></param>
    /// <param name="limit"></param>
    /// <returns></returns>
    List<VideoInfo> GetRandomList(int offset, int limit);

    /// <summary>
    /// 分页查询列表，含有Actor信息
    /// </summary>
    /// <param name="offset"></param>
    /// <param name="limit"></param>
    /// <returns></returns>
    List<VideoInfo> ListWithActor(int offset, int limit);

    Task<VideoInfo[]> GetLookLaterListAsync(int limit);
    
    Task<VideoInfo[]> GetLikeListAsync(int limit);
    
    Task<VideoInfo[]> GetRecentListAsync(int limit);
    
    void ExecuteUpdateByTrueName(string trueName, Action<VideoInfo> updateAction);
    VideoInfo? getOneByFileId(long fileInfoId);
    VideoInfo? GetForDetailById(long id);
}