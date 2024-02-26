using DataAccess.Dao.Interface;
using DataAccess.Models.Entity;
using DataAccess.Vo;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Dao.Impl;

public class FilesInfoDao : DaoImpl<FilesInfo>, IFilesInfoDao
{
    public List<FilesInfo> GetListByTrueName(string name)
    {
        //return DbSet.SelectMany(fileInfo =>
        //        Context.FileToInfos.Where(fileToInfo =>
        //            fileInfo.Pc.Equals(fileToInfo.FilePickCode)
        //            && EF.Functions.Like(fileToInfo.Truename, name)).DefaultIfEmpty(),
        //    (info, middle) => info).ToList();

        return (from fileInfo in DbSet
                join fileToInfo in Context.FileToInfos
                    on fileInfo.Pc equals fileToInfo.FilePickCode
                where EF.Functions.Like(fileToInfo.Truename, name)
                select fileInfo).ToList();
    }

    public void GetVideoInfoInFailStatusList(VideoInfoInFailStatusQueryVo queryVo)
    {
        var query = DbSet.SelectMany(
            i => Context.FileToInfos
                .Where(fileToInfo => fileToInfo.FilePickCode.Equals(i.Pc)
                                     && EF.Functions.Like(fileToInfo.Truename, queryVo.Name))
                .DefaultIfEmpty(),
            (info, middle) => info
        ).Skip(queryVo.Position)
        .Take(queryVo.Take);

        if (queryVo.IsRandom)
        {
            query = query.OrderBy(x => Guid.NewGuid());
        }
        else
        {
            //query.OrderBy(x=>x.)
        }

    }
}