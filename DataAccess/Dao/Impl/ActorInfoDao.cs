using System.Linq.Expressions;
using DataAccess.Context;
using DataAccess.Dao.Interface;
using DataAccess.Models.Entity;

namespace DataAccess.Dao.Impl;

public class ActorInfoDao : BaseDao<ActorInfo>, IActorInfoDao
{
    
     public List<ActorInfo> GetPartListByVideoName(string videoName)
     {
         return CurrentDbSet.Where(i =>
                 i.VideoInfos.FirstOrDefault(videoInfo =>
                     videoInfo.Name.Equals(videoName)) != null
             ).ToList();
     }


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
     public ActorInfo? GetOne()
     {
         return CurrentDbSet.FirstOrDefault();
     }
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

     public ActorInfo? GetPartInfoExceptVideoInfoByActorName(string showName)
     {
         // var query = from actorInfo in DbSet
         //     join actorName in Context.ActorNames
         //         on actorInfo.Id equals actorName.Id 
         //     where actorName.Name == showName
         //     join bwh in Context.Bwhs
         //         on actorInfo.Bwh equals bwh.BwhContent into bwhEnumerable //  into bwhEnumerable是为了实现左联
         //         from bwh in bwhEnumerable.DefaultIfEmpty()
         //     select new ActorInfo
         //     {
         //         Id = actorInfo.Id,
         //         Name = actorInfo.Name,
         //         IsWoman = actorInfo.IsWoman,
         //         Birthday = actorInfo.Birthday,
         //         Bwh = actorInfo.Bwh,
         //         Height = actorInfo.Height,
         //         WorksCount = actorInfo.WorksCount,
         //         WorkTime = actorInfo.WorkTime,
         //         ProfilePath = actorInfo.ProfilePath,
         //         BlogUrl = actorInfo.BlogUrl,
         //         IsLike = actorInfo.IsLike,
         //         InfoUrl = actorInfo.InfoUrl,
         //         BwhInfo = bwh ?? new Bwh()
         //     };
         // return query.FirstOrDefault();

         return CurrentDbSet.FirstOrDefault(i => i.NameList.FirstOrDefault(a => a.Name.Equals(showName)) != null);
     }
}