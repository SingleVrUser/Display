using DataAccess.Context;
using DataAccess.Dao.Interface;
using DataAccess.Models.Entity;
using DataAccess.Models.Vo;
using static System.String;

namespace DataAccess.Dao.Impl;

public class VideoInfoDao: BaseDao<VideoInfo>, IVideoInfoDao
{
    public void AddOrUpdateInfoAndAttachFile(VideoInfoVo vo, List<long> fileIdList)
    {
        // 在文件信息中添加该视频信息的id
        var fileInfoList = Context.FileInfo.Where(i => fileIdList.Contains(i.Id)).ToList();
        if (fileInfoList.Count == 0) return;

        using var transaction = Context.Database.BeginTransaction();

        // 添加视频信息
        var videoInfo = GetVideoInfoFromVo(vo);
        // 不存在添加
        if (videoInfo.Id.Equals(default))
            CurrentDbSet.Add(videoInfo);
        // 存在则更新
        else
            CurrentDbSet.Update(videoInfo);

        // 更新file_info
        fileInfoList.ForEach(i =>i.VideoId = videoInfo.Id);
        Context.UpdateRange(fileInfoList);
        Context.SaveChanges();
        transaction.Commit();
    }
    

    public void AddOrUpdateByVideoInfoVo(VideoInfoVo vo)
    {
        var videoInfo = GetVideoInfoFromVo(vo);
        
        // 不存在则添加，存在则更新
        if(videoInfo.Id.Equals(default))
            CurrentDbSet.Add(videoInfo);
        else
            CurrentDbSet.Update(videoInfo);
        Context.SaveChanges();
    }
    
    public VideoInfo? GetById(long id)
    {
        return Context.VideoInfo.Find(id);
    }

    public VideoInfo? GetOneByName(string name)
    {
        return Context.VideoInfo.FirstOrDefault(x => x.Name.Equals(name));
    }

    public bool IsExistsName(string name)
    {
        return Context.VideoInfo.Any(x => x.Name.Equals(name));
    }

    private VideoInfo GetVideoInfoFromVo(VideoInfoVo vo)
    {
        // 查看是否有同name的视频信息
        var videoInfo = GetOneByName(vo.Name);
        
        // 数据库没有改数据，则构建一个新的
        videoInfo ??= new VideoInfo(vo.Name)
        {
            SourceUrl = vo.SourceUrl
        };
        
        videoInfo.Title = vo.Title;
        videoInfo.SourceUrl = vo.SourceUrl;
        videoInfo.ReleaseTime = vo.ReleaseTime;
        videoInfo.LengthTime = vo.LengthTime;
        videoInfo.ImageUrl = vo.SampleImageList == null
            ? null
            : Join(",", vo.SampleImageList);
        
        // 导演
        if (vo.DirectorName != null)
            videoInfo.Director = Context.DirectorInfo.FirstOrDefault(i=>string.Equals(i.Name,vo.DirectorName))
                                 ?? new DirectorInfo(vo.DirectorName);
        
        // 厂商
        if (vo.ProducerName != null)
            videoInfo.Producer = Context.ProducerInfo.FirstOrDefault(i => string.Equals(i.Name,vo.ProducerName))
                                 ?? new ProducerInfo(vo.ProducerName);
        
        // 发行者
        if (vo.PublisherName != null)
            videoInfo.Publisher = Context.PublisherInfo.FirstOrDefault(i=> string.Equals(i.Name,vo.PublisherName))
                                  ?? new PublisherInfo(vo.PublisherName);
        
        // 系列
        if (vo.SeriesName != null)
            videoInfo.Series = Context.SeriesInfo.FirstOrDefault(i=> string.Equals(i.Name,vo.SeriesName))
                               ?? new SeriesInfo(vo.SeriesName);
        
        // 演员信息
        if (vo.ActorNameList is { Count: > 0 })
        {
            videoInfo.ActorInfoList = [];
            foreach (var actorInfo in
                     from actorName in vo.ActorNameList
                        let actorInfo = Context.ActorInfo.FirstOrDefault(i=>string.Equals(i.Name, actorName))
                     where actorInfo == null
                     select new ActorInfo(actorName))
            {
                videoInfo.ActorInfoList.Add(actorInfo);
            }
        }
        
        // 标签列表
        if (vo.SampleImageList is { Count: > 0 })
        {
            videoInfo.SampleImages = Concat(vo.SampleImageList);
        }
        
        // 文件信息，搜刮的时候添加

        return videoInfo;
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