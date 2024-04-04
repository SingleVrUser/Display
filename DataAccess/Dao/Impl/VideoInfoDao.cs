using DataAccess.Dao.Interface;
using DataAccess.Models.Entity;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Dao.Impl;

public class VideoInfoDao: DaoImpl<VideoInfo>, IVideoInfoDao
{
    public void UpdateCompleted(VideoInfo info)
    {
        // 更新表VideoInfo
        DbSet.Update(info);
        
        //更新是否步兵
        Context.IsWms.Add(new IsWm { Truename = info.TrueName, IsWm1 = info.IsWm });

        if (info.Actor == null) return;
        
        var actors = info.Actor.Split(",");
        if (actors.Length <= 0) return;
            
        //更新演员_视频中间表
        
        //先删除Actor_Videos中所有Video_name的数据
        Context.ActorVideos.RemoveRange(Context.ActorVideos.Where(i => i.VideoName == info.TrueName));
    
        //查询演员id列表
        foreach (var actorId in Context.ActorInfos.Where(i => actors.Contains(i.Name.TrimEnd('♀', '♀'))).Select(i=>i.Id))
        {
            Context.ActorVideos.Add(new ActorVideo { ActorId =  actorId, VideoName = info.TrueName});
        }
        
        SaveChanges();
    }

    public void UpdateAllImagePathList(string srcPath, string dstPath)
    {
        var videoInfos = DbSet.Where(i => i.ImagePath != null && i.ImagePath.Contains(srcPath)).ToList();
        videoInfos.ForEach(i=>i.ImagePath= i.ImagePath!.Replace(srcPath, dstPath));
        SaveChanges();
        
    }

    public VideoInfo[] GetLookLaterList(int limit)
    {
        return DbSet.Where(i => i.LookLater > 0).OrderByDescending(i => i.LookLater).Take(limit).ToArray();
    }

    public VideoInfo[] GetLikeList(int limit)
    {
        return DbSet.Where(i => i.IsLike != 0).Take(limit).ToArray();
    }

    public VideoInfo[] GetRandomList(int limit)
    {
        return DbSet.Where(i => i.IsLike != 0).OrderBy(_ => EF.Functions.Random()).Take(limit).ToArray();
    }

    public VideoInfo[] GetRecentList(int limit)
    {
        return DbSet.OrderByDescending(i=>i.AddTime).Take(limit).ToArray();
    }


    public VideoInfo? GetOneByTrueName(string name)
    {
        return DbSet.Find(name);
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