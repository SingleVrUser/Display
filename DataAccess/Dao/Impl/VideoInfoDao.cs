using DataAccess.Dao.Interface;
using DataAccess.Models.Entity;
using DataAccess.Models.Dto;
using Microsoft.EntityFrameworkCore;
using static System.String;

namespace DataAccess.Dao.Impl;

public class VideoInfoDao : BaseDao<VideoInfo>, IVideoInfoDao
{
    private static readonly double Random = new Random().NextDouble();
    
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
        Context.SaveChanges();

        // 更新file_info
        // videoInfo保存后，id才有值
        fileInfoList.ForEach(i=>i.VideoId=videoInfo.Id);
        
        Context.UpdateRange(fileInfoList);
        Context.SaveChanges();
        transaction.Commit();
    }


    public void AddOrUpdateByVideoInfoVo(VideoInfoDto dto)
    {
        var videoInfo = GetVideoInfoFromVo(dto);

        // 不存在则添加，存在则更新
        if (videoInfo.Id.Equals(default))
            CurrentDbSet.Add(videoInfo);
        else
            CurrentDbSet.Update(videoInfo);
        Context.SaveChanges();
    }

    public VideoInfo? GetById(long id)
    {
        return CurrentDbSet.Find(id);
    }

    public VideoInfo? GetOneByName(string name)
    {
        return CurrentDbSet.FirstOrDefault(x => x.Name.Equals(name));
    }

    public bool IsExistsName(string name)
    {
        return Context.VideoInfo.Any(x => x.Name.Equals(name));
    }

    public void ExecuteDeleteById(long id)
    {
        CurrentDbSet.Where(i => i.Id.Equals(id)).ExecuteDelete();
    }

    public List<VideoInfo> ListWithActor(int offset, int limit)
    {
        return CurrentDbSet.Include(i => i.ActorInfoList)
            .Skip(offset)
            .Take(limit)
            .ToList();
    }

    public List<VideoInfo> GetRandomList(int offset, int limit)
    {
        return CurrentDbSet.Include(i => i.ActorInfoList)
            .Include(i => i.CategoryList)
            .OrderBy(_ => Random)
            .Skip(offset)
            .Take(limit)
            .ToList();
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
        videoInfo.ImagePath = dto.ImagePath;
        videoInfo.ImageUrl = dto.SampleImageList == null
            ? null
            : Join(",", dto.SampleImageList);

        // 导演
        if (dto.DirectorName != null)
            videoInfo.Director = Context.DirectorInfo.FirstOrDefault(i => string.Equals(i.Name, dto.DirectorName))
                                 ?? new DirectorInfo(dto.DirectorName);

        // 厂商
        if (dto.ProducerName != null)
            videoInfo.Producer = Context.ProducerInfo.FirstOrDefault(i => string.Equals(i.Name, dto.ProducerName))
                                 ?? new ProducerInfo(dto.ProducerName);

        // 发行者
        if (dto.PublisherName != null)
            videoInfo.Publisher = Context.PublisherInfo.FirstOrDefault(i => string.Equals(i.Name, dto.PublisherName))
                                  ?? new PublisherInfo(dto.PublisherName);

        // 系列
        if (dto.SeriesName != null)
            videoInfo.Series = Context.SeriesInfo.FirstOrDefault(i => string.Equals(i.Name, dto.SeriesName))
                               ?? new SeriesInfo(dto.SeriesName);

        // 演员信息
        if (dto.ActorNameList is { Count: > 0 })
        {
            videoInfo.ActorInfoList ??= [];
            foreach (var actorName in from actorName in dto.ActorNameList
                     let actorInfo = Context.ActorInfo.FirstOrDefault(i => i.Name.Equals(actorName))
                     where actorInfo == null
                     select actorName)
            {
                videoInfo.ActorInfoList.Add(new ActorInfo(actorName));
            }
        }

        // 类别列表
        if (dto.CategoryList is { Count: > 0 })
        {
            videoInfo.CategoryList = [];
            foreach (var categoryName in dto.CategoryList)
            {
                var category = Context.Category.FirstOrDefault(i=>i.Name.Equals(categoryName));
                videoInfo.CategoryList.Add(category ?? new CategoryInfo(categoryName));
            }
        }

        // 标签列表
        if (dto.SampleImageList is { Count: > 0 })
        {
            videoInfo.SampleImages = Join(",", dto.SampleImageList);
        }


        // 文件信息，搜刮的时候添加

        return videoInfo;
    }

    public async Task<VideoInfo[]> GetLookLaterListAsync(int limit)
    {
        return await CurrentDbSet.Where(i => i.Interest != null && i.Interest.IsLookAfter)
            .OrderByDescending(i => i.CreateTime)
            .Take(limit).ToArrayAsync();
    }

    public async Task<VideoInfo[]> GetLikeListAsync(int limit)
    {
        return await CurrentDbSet.Where(i => i.Interest != null && i.Interest.IsLike).Take(limit).ToArrayAsync();
    }

    public async Task<VideoInfo[]> GetRecentListAsync(int limit)
    {
        return await CurrentDbSet.OrderByDescending(i => i.CreateTime).Take(limit).ToArrayAsync();
    }

    public void ExecuteUpdateByTrueName(string trueName, Action<VideoInfo> updateAction)
    {
        var info = CurrentDbSet.FirstOrDefault(i => i.Name.Equals(trueName));
        if (info == null) return;

        updateAction.Invoke(info);
        Context.SaveChanges();
    }

    public VideoInfo? getOneByFileId(long fileInfoId)
    {
        return CurrentDbSet.FirstOrDefault(videoInfo =>
            videoInfo.FileInfoList != null && videoInfo.FileInfoList.FirstOrDefault(fileInfo =>
                fileInfo.Id.Equals(fileInfoId)) != null);
    }

    public VideoInfo? GetForDetailById(long id)
    {
        return CurrentDbSet
            .Include(i => i.Director)
            .Include(i => i.Producer)
            .Include(i => i.Series)
            .Include(i => i.CategoryList)
            .Include(i => i.Interest)
            .Include(i => i.ActorInfoList)
            .FirstOrDefault(i => i.Id.Equals(id));
    }
}