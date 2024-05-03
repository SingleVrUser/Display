using System.Linq.Expressions;
using DataAccess.Models.Entity;

namespace DataAccess.Dao.Interface;

public interface IActorInfoDao : IBaseDao<ActorInfo>
{
    // /// <summary>
    // /// 通过视频的Name获取出演者的部分信息列表（不包括出演视频数）
    // /// </summary>
    // /// <param name="videoName"></param>
    // /// <returns></returns>
    // List<ActorInfo> GetPartListByVideoName(string videoName);
    //
    // /// <summary>
    // /// 通过演员的名称（演员可能有多个名称）获取该演员的部分信息（不包含出演视频数）
    // /// </summary>
    // /// <param name="showName"></param>
    // /// <returns></returns>
    // ActorInfo? GetPartInfoByActorName(string showName);
    //
    // List<ActorInfo> GetList(int index, int limit);
    //
    // List<ActorInfo> GetPageList(int index, int limit, Dictionary<string, bool>? orderByList = null, List<string>? filterList = null);
    //
    // void UpdateAllProfilePathList(string srcPath, string dstPath);
    //
    // ActorInfo? GetOne();
    // void ExecuteUpdateById(long id, Action<ActorInfo> updateAction);
}