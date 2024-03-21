using DataAccess.Models.Entity;

namespace DataAccess.Dao.Interface;

internal interface IVideoInfoDao : IDao<VideoInfo>
{
    List<VideoInfo>? GetInfoListByTrueName(string name);

    string? GetTrueNameByLikeName(string name);
}