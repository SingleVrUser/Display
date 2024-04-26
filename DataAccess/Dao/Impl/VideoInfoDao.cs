using DataAccess.Context;
using DataAccess.Dao.Interface;
using DataAccess.Models.Entity;
using DataAccess.Models.Vo;
using static System.String;

namespace DataAccess.Dao.Impl;

public class VideoInfoDao: BaseDao<VideoInfo>, IVideoInfoDao
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


    public VideoInfo? GetById(long id)
    {
        return _videoInfoContext.VideoInfo.Find(id);
    }

    public VideoInfo? GetOneByName(string name)
    {
        return _videoInfoContext.VideoInfo.FirstOrDefault(x => x.Name.Equals(name));
    }

    public bool IsExistsName(string name)
    {
        return _videoInfoContext.VideoInfo.Any(x => x.Name.Equals(name));
    }

    public void AddByVideoInfoVo(VideoInfoVo vo)
    {
        // 查看是否有同name的视频信息
        var isExistsName = IsExistsName(vo.Name);
        // 数据库有该数据则退出
        if (isExistsName) return;

        var videoInfo = new VideoInfo(vo.Name)
        {
            Title = vo.Title,
            SourceUrl = vo.SourceUrl,
        };


        // 导演
        if (vo.DirectorName != null)
            videoInfo.Director = _videoInfoContext.DirectorInfo.FirstOrDefault(i=>string.Equals(i.Name,vo.DirectorName))
                            ?? new DirectorInfo(vo.DirectorName);
        
        // 厂商
        if (vo.ProducerName != null)
            videoInfo.Producer = _videoInfoContext.ProducerInfo.FirstOrDefault(i => string.Equals(i.Name,vo.ProducerName))
                            ?? new ProducerInfo(vo.ProducerName);
        
        // 发行者
        if (vo.PublisherName != null)
            videoInfo.Publisher = _videoInfoContext.PublisherInfo.FirstOrDefault(i=> string.Equals(i.Name,vo.PublisherName))
                                 ?? new PublisherInfo(vo.PublisherName);
        
        // 系列
        if (vo.SeriesName != null)
            videoInfo.Series = _videoInfoContext.SeriesInfo.FirstOrDefault(i=> string.Equals(i.Name,vo.SeriesName))
                              ?? new SeriesInfo(vo.SeriesName);
        
        // 演员信息
        if (vo.ActorNameList is { Count: > 0 })
        {
            videoInfo.ActorInfoList = [];
            foreach (var actorInfo in
                     from actorName in vo.ActorNameList
                        let actorInfo = _videoInfoContext.ActorInfo.FirstOrDefault(i=>string.Equals(i.Name, actorName))
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
        
        // 文件信息
        // 通过FileSpiderResult查询Named对应的FileId
        videoInfo.FileInfoList = (from fileInfo in _videoInfoContext.FileInfo
            join spiderResult in _videoInfoContext.FileSpiderResult
                on fileInfo.Id equals spiderResult.FileId
            where spiderResult.TrueName == vo.Name
            select fileInfo).ToList();
        
        AddAndSaveChanges(videoInfo);
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