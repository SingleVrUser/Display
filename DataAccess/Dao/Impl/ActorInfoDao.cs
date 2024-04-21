using DataAccess.Context;
using DataAccess.Dao.Interface;

namespace DataAccess.Dao.Impl;

public class ActorInfoDao : IActorInfoDao
{
    private readonly ActorContext _actorContext = new();

//     public List<ActorInfo> GetPartListByVideoName(string videoName)
//     {
//         // video_count有误
//         // var actorInfoQueryable = from actorInfo in DbSet
//         //     join actorVideo in Context.ActorVideos
//         //         on actorInfo.Id equals actorVideo.ActorId
//         //     where actorVideo.VideoName == trueName
//         //     join bwh in Context.Bwhs
//         //         on actorInfo.Bwh equals bwh.BwhContent into bwhEnumerable //  into bwhEnumerable是为了实现左联
//         //         from bwh in bwhEnumerable.DefaultIfEmpty()
//         //     group new
//         //     {
//         //         actorInfo,
//         //         bwh
//         //     } by actorInfo.Id
//         //     into groupedActor
//         //     select new ActorInfo
//         //     {
//         //         Id = groupedActor.FirstOrDefault().actorInfo.Id,
//         //         Name = groupedActor.FirstOrDefault().actorInfo.Name,
//         //         IsWoman = groupedActor.FirstOrDefault().actorInfo.IsWoman,
//         //         Birthday = groupedActor.FirstOrDefault().actorInfo.Birthday,
//         //         Bwh = groupedActor.FirstOrDefault().actorInfo.Bwh,
//         //         Height = groupedActor.FirstOrDefault().actorInfo.Height,
//         //         WorksCount = groupedActor.FirstOrDefault().actorInfo.WorksCount,
//         //         WorkTime = groupedActor.FirstOrDefault().actorInfo.WorkTime,
//         //         ProfilePath = groupedActor.FirstOrDefault().actorInfo.ProfilePath,
//         //         BlogUrl = groupedActor.FirstOrDefault().actorInfo.BlogUrl,
//         //         IsLike = groupedActor.FirstOrDefault().actorInfo.IsLike,
//         //         AddTime = groupedActor.FirstOrDefault().actorInfo.AddTime,
//         //         InfoUrl = groupedActor.FirstOrDefault().actorInfo.InfoUrl,
//         //         
//         //         BwhInfo = groupedActor.FirstOrDefault().bwh ?? new Bwh(),
//         //         video_count = groupedActor.Count()
//         //     };
//
//         var actorInfoQueryable = from actorInfo in DbSet
//             join actorVideo in Context.ActorVideos
//                 on actorInfo.Id equals actorVideo.ActorId
//             where actorVideo.VideoName == videoName
//
//             join bwh in Context.Bwhs
//                 on actorInfo.Bwh equals bwh.BwhContent into bwhEnumerable //  into bwhEnumerable是为了实现左联
//                 from bwh in bwhEnumerable.DefaultIfEmpty()
//             select new ActorInfo
//             {
//                 Id = actorInfo.Id,
//                 Name = actorInfo.Name,
//                 IsWoman = actorInfo.IsWoman,
//                 Birthday = actorInfo.Birthday,
//                 Bwh = actorInfo.Bwh,
//                 Height = actorInfo.Height,
//                 WorksCount = actorInfo.WorksCount,
//                 WorkTime = actorInfo.WorkTime,
//                 ProfilePath = actorInfo.ProfilePath,
//                 BlogUrl = actorInfo.BlogUrl,
//                 IsLike = actorInfo.IsLike,
//                 InfoUrl = actorInfo.InfoUrl,
//                 BwhInfo = bwh ?? new Bwh()
//             };
//             
//         return actorInfoQueryable.AsNoTracking().ToList();
//     }
//
//     public List<ActorInfo> GetList(int index, int limit)
//     {
//         return (from info in DbSet select info).Skip(index).Take(limit).ToList();
//     }
//
//     public List<ActorInfo> GetPageList(int index, int limit, Dictionary<string, bool>? orderByList = null, List<string>? filterList = null)
//     {
//         var orderStr = string.Empty;
//         if (orderByList != null)
//         {
//             List<string> orderStrList = [];
//             foreach (var item in orderByList)
//             {
//                 var dscStr = item.Value ? " DESC" : string.Empty;
//
//                 orderStrList.Add($"{item.Key}{dscStr}");
//             }
//
//             orderStr = $" ORDER BY {string.Join(",", orderStrList)}";
//         }
//
//         var filterStr = string.Empty;
//         if (filterList != null)
//         {
//             filterStr = $" WHERE {string.Join(",", filterList)}";
//         }
//
//         var actorInfos = Context.Database.SqlQuery<ActorInfo>(
//             $"SELECT ActorInfo.*,bwh.bust,bwh.waist,bwh.hips,COUNT(id) as video_count FROM ActorInfo LEFT JOIN Actor_Video ON Actor_Video.actor_id = ActorInfo.id LEFT JOIN bwh ON ActorInfo.bwh = bwh.bwh{filterStr} GROUP BY id{orderStr} LIMIT {limit} offset {index}"
//         );
//
//         return actorInfos.ToList();
//     }
//
//     public void UpdateAllProfilePathList(string srcPath, string dstPath)
//     {
//         var videoInfos = DbSet.Where(i => i.ProfilePath != null && i.ProfilePath.Contains(srcPath)).ToList();
//         videoInfos.ForEach(i=>i.ProfilePath= i.ProfilePath!.Replace(srcPath, dstPath));
//         SaveChanges();
//     }
//
//     public ActorInfo? GetOne()
//     {
//         return DbSet.FirstOrDefault();
//     }
//
//     public void ExecuteUpdateById(long id, Action<ActorInfo> updateAction)
//     {
//         var info = DbSet.FirstOrDefault(i => i.Id == id);
//         if (info == null) return;
//
//         updateAction.Invoke(info);
//         SaveChanges();
//     }
//
//
//
//     public ActorInfo? GetPartInfoByActorName(string showName)
//     {
//         var query = from actorInfo in DbSet
//             join actorName in Context.ActorNames
//                 on actorInfo.Id equals actorName.Id 
//             where actorName.Name == showName
//             join bwh in Context.Bwhs
//                 on actorInfo.Bwh equals bwh.BwhContent into bwhEnumerable //  into bwhEnumerable是为了实现左联
//                 from bwh in bwhEnumerable.DefaultIfEmpty()
//             select new ActorInfo
//             {
//                 Id = actorInfo.Id,
//                 Name = actorInfo.Name,
//                 IsWoman = actorInfo.IsWoman,
//                 Birthday = actorInfo.Birthday,
//                 Bwh = actorInfo.Bwh,
//                 Height = actorInfo.Height,
//                 WorksCount = actorInfo.WorksCount,
//                 WorkTime = actorInfo.WorkTime,
//                 ProfilePath = actorInfo.ProfilePath,
//                 BlogUrl = actorInfo.BlogUrl,
//                 IsLike = actorInfo.IsLike,
//                 InfoUrl = actorInfo.InfoUrl,
//                 BwhInfo = bwh ?? new Bwh()
//             };
//         return query.FirstOrDefault();
//     }
}