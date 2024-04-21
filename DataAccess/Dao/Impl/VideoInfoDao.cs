using DataAccess.Context;
using DataAccess.Dao.Interface;
using DataAccess.Models.Entity;

namespace DataAccess.Dao.Impl;

public class VideoInfoDao: IVideoInfoDao
{
    private readonly VideoInfoContext _videoInfoContext = new();
    
    public void AddAndSaveChanges(long fileId, VideoInfo videoInfo)
    {
        // 在文件信息中添加该视频信息的id
        var fileInfo = _videoInfoContext.FileInfo.Find(fileId);
        if (fileInfo == null) return;

        using var transaction = _videoInfoContext.Database.BeginTransaction();

        // 添加视频信息
        _videoInfoContext.Add(videoInfo);
        _videoInfoContext.SaveChanges();
        
        fileInfo.VideoId = videoInfo.Id;
        _videoInfoContext.Update(fileInfo);
        _videoInfoContext.SaveChanges();
        
        transaction.Commit();
    }

    public void AddAndSaveChanges(VideoInfo videoInfo)
    {
        _videoInfoContext.Add(videoInfo);
        _videoInfoContext.SaveChanges();
    }

    public void InitData()
    {
        _videoInfoContext.Database.EnsureDeleted();
        _videoInfoContext.Database.EnsureCreated();
    }


    //     public void UpdateAllImagePathList(string srcPath, string dstPath)
//     {
//         var videoInfos = DbSet.Where(i => i.ImagePath.Contains(srcPath)).ToList();
//         videoInfos.ForEach(i=>i.ImagePath= i.ImagePath.Replace(srcPath, dstPath));
//         SaveChanges();
//     }
//
//     public async Task<VideoInfo[]> GetLookLaterListAsync(int limit)
//     {
//         return await DbSet.AsNoTracking().Where(i => i.LookLater > 0).OrderByDescending(i => i.LookLater).Take(limit).ToArrayAsync();
//     }
//
//     public async Task<VideoInfo[]> GetLikeListAsync(int limit)
//     {
//         return await DbSet.AsNoTracking().Where(i => i.IsLike).Take(limit).ToArrayAsync();
//     }
//
//     public async Task<VideoInfo[]> GetRecentListAsync(int limit)
//     {
//         return await DbSet.AsNoTracking().OrderByDescending(i=>i.CreateTime).Take(limit).ToArrayAsync();
//     }
//
//     public void ExecuteUpdateByTrueName(string trueName, Action<VideoInfo> updateAction)
//     {
//         var info = DbSet.FirstOrDefault(i => i.TrueName == trueName);
//         if (info == null) return;
//
//         updateAction.Invoke(info);
//         SaveChanges();
//     }
//
//     public VideoInfo? GetOneByTrueName(string name)
//     {
//         return DbSet.AsNoTracking().FirstOrDefault(i=>i.TrueName == name);
//     }
//
//     public List<VideoInfo> GetInfoListByTrueName(string name)
//     {
//         return DbSet.Where(i => i.TrueName == name).ToList();
//     }
//
//     public string? GetTrueNameByLikeName(string name)
//     {
//         var pattern = name.Replace('-', '_');
//
//         return DbSet.Select(i => i.TrueName)
//             .FirstOrDefault(item => EF.Functions.Like(item, pattern));
//     }
//
//     public void ExecuteRemoveByName(string name)
//     {
//         DbSet.Where(i => i.TrueName == name).ExecuteDelete();
//     }
//

}