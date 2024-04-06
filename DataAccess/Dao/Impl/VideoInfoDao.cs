using DataAccess.Dao.Interface;
using DataAccess.Models.Entity;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Dao.Impl;

public class VideoInfoDao: DaoImpl<VideoInfo>, IVideoInfoDao
{
    public void UpdateAllImagePathList(string srcPath, string dstPath)
    {
        var videoInfos = DbSet.Where(i => i.ImagePath.Contains(srcPath)).ToList();
        videoInfos.ForEach(i=>i.ImagePath= i.ImagePath.Replace(srcPath, dstPath));
        SaveChanges();
    }

    public async Task<VideoInfo[]> GetLookLaterListAsync(int limit)
    {
        return await DbSet.AsNoTracking().Where(i => i.LookLater > 0).OrderByDescending(i => i.LookLater).Take(limit).ToArrayAsync();
    }

    public async Task<VideoInfo[]> GetLikeListAsync(int limit)
    {
        return await DbSet.AsNoTracking().Where(i => i.IsLike != 0).Take(limit).ToArrayAsync();
    }

    public async Task<VideoInfo[]> GetRecentListAsync(int limit)
    {
        return await DbSet.AsNoTracking().OrderByDescending(i=>i.AddTime).Take(limit).ToArrayAsync();
    }

    public void ExecuteUpdateByTrueName(string trueName, Action<VideoInfo> updateAction)
    {
        var info = DbSet.FirstOrDefault(i => i.TrueName == trueName);
        if (info == null) return;

        updateAction.Invoke(info);
        SaveChanges();
    }

    public VideoInfo? GetOneByTrueName(string name)
    {
        return DbSet.AsNoTracking().FirstOrDefault(i=>i.TrueName == name);
    }

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

    public void ExecuteRemoveByName(string name)
    {
        DbSet.Where(i => i.TrueName == name).ExecuteDelete();
    }

}