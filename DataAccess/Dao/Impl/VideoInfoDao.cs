using DataAccess.Context;
using DataAccess.Dao.Interface;
using DataAccess.Models.Entity;
using DataAccess.Models.Dto;
using Microsoft.EntityFrameworkCore;
using static System.String;

namespace DataAccess.Dao.Impl;

public class VideoInfoDao: BaseDao<VideoInfo>, IVideoInfoDao
{
    public void AddOrUpdateInfoAndAttachFile(VideoInfoDto dto, List<long> fileIdList)
    {
        // 在文件信息中添加该视频信息的id
        var fileInfoList = Context.FileInfo.Where(i => fileIdList.Contains(i.Id)).ToList();
        if (fileInfoList.Count == 0) return;

        using var transaction = Context.Database.BeginTransaction();

        // 添加视频信息
        var videoInfo = GetVideoInfoFromVo(dto);
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
    

    public void AddOrUpdateByVideoInfoVo(VideoInfoDto dto)
    {
        var videoInfo = GetVideoInfoFromVo(dto);
        
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

    public void ExecuteDeleteById(long id)
    {
        CurrentDbSet.Where(i => i.Id.Equals(id)).ExecuteDelete();
    }

    private VideoInfo GetVideoInfoFromVo(VideoInfoDto dto)
    {
        // 查看是否有同name的视频信息
        var videoInfo = GetOneByName(dto.Name);
        
        // 数据库没有改数据，则构建一个新的
        videoInfo ??= new VideoInfo(dto.Name)
        {
            SourceUrl = dto.SourceUrl
        };
        
        videoInfo.Title = dto.Title;
        videoInfo.SourceUrl = dto.SourceUrl;
        videoInfo.ReleaseTime = dto.ReleaseTime;
        videoInfo.LengthTime = dto.LengthTime;
        videoInfo.ImageUrl = dto.SampleImageList == null
            ? null
            : Join(",", dto.SampleImageList);
        
        // 导演
        if (dto.DirectorName != null)
            videoInfo.Director = Context.DirectorInfo.FirstOrDefault(i=>string.Equals(i.Name,dto.DirectorName))
                                 ?? new DirectorInfo(dto.DirectorName);
        
        // 厂商
        if (dto.ProducerName != null)
            videoInfo.Producer = Context.ProducerInfo.FirstOrDefault(i => string.Equals(i.Name,dto.ProducerName))
                                 ?? new ProducerInfo(dto.ProducerName);
        
        // 发行者
        if (dto.PublisherName != null)
            videoInfo.Publisher = Context.PublisherInfo.FirstOrDefault(i=> string.Equals(i.Name,dto.PublisherName))
                                  ?? new PublisherInfo(dto.PublisherName);
        
        // 系列
        if (dto.SeriesName != null)
            videoInfo.Series = Context.SeriesInfo.FirstOrDefault(i=> string.Equals(i.Name,dto.SeriesName))
                               ?? new SeriesInfo(dto.SeriesName);
        
        // 演员信息
        if (dto.ActorNameList is { Count: > 0 })
        {
            videoInfo.ActorInfoList = [];
            foreach (var actorInfo in
                     from actorName in dto.ActorNameList
                        let actorInfo = Context.ActorInfo.FirstOrDefault(i=>string.Equals(i.Name, actorName))
                     where actorInfo == null
                     select new ActorInfo(actorName))
            {
                videoInfo.ActorInfoList.Add(actorInfo);
            }
        }
        
        // 标签列表
        if (dto.SampleImageList is { Count: > 0 })
        {
            videoInfo.SampleImages = Concat(dto.SampleImageList);
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