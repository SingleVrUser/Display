using DataAccess.Dao.Interface;
using DataAccess.Models.Entity;
using DataAccess.Vo;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Dao.Impl;

public class VideoInfoDao : DaoImpl<VideoInfo>, IVideoInfoDao
{
    public List<VideoInfo> GetInfoListByTrueName(string name)
    {
        return DbSet.Where(i => i.TrueName == name).ToList();
    }

    public string? GetTrueNameByLikeName(string name)
    {
        var pattern = name.Replace('-', '_');

        return DbSet.Select(i => i.TrueName)
            .FirstOrDefault(item => EF.Functions.Like(item, pattern));
    }


}   