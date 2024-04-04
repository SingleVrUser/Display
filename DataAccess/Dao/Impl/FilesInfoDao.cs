using DataAccess.Dao.Interface;
using DataAccess.Models.Dto;
using DataAccess.Models.Entity;
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
                    on fileInfo.PickCode equals fileToInfo.FilePickCode
                where EF.Functions.Like(fileToInfo.Truename, name)
                select fileInfo).ToList();
    }

    // TODO 未完成
    public void GetVideoInfoInFailStatusList(VideoInfoInFailStatusQueryDto queryDto)
    {
        var query = DbSet.SelectMany(
            i => Context.FileToInfos
                .Where(middle => middle.FilePickCode.Equals(i.PickCode)
                                     && EF.Functions.Like(middle.Truename, queryDto.Name))
                .DefaultIfEmpty(),
            (i, _) => i
        ).Skip(queryDto.Position)
        .Take(queryDto.Take);

        if (queryDto.IsRandom)
        {
            query = query.OrderBy(_ => EF.Functions.Random());
        }
        else
        {
            //query.OrderBy(x=>x.)
        }
    }

    public List<FilesInfo> GetAllFilesListByFolderId(long folderId)
    {
        return GetAllFileListTraverse(folderId, []);
    }

    public List<FilesInfo> GetPartFolderListByPid(long pid, int? limit = null)
    {
        var queryable = DbSet.Where(i => i.ParentId == pid);
        
        if (limit != null) queryable = queryable.Take(limit.Value);

        return queryable.ToList();
    }   
    
    public List<FilesInfo> GetPartFileListById(long id)
    {
        // 文件 或者 文件夹
        return DbSet.Where(i => i.CurrentId == id || i.ParentId == id).ToList();
    }

    public void RemoveAllByFolderId(long folderId)
    {
        var allFiles = GetAllFilesListByFolderId(folderId);
        DbSet.RemoveRange(allFiles);
        
        // 删除中间表
        
        
        SaveChanges();
    }

    public void RemoveByPickCode(string pickCode)
    {
        DbSet.Where(i => i.PickCode == pickCode).ExecuteDelete();
        
        // 删除中间表
        Context.FileToInfos.RemoveRange(Context.FileToInfos.Where(i => i.FilePickCode == pickCode));
        
        SaveChanges();
    }

    public void ExecuteRemoveByTrueName(string trueName)
    {
        DbSet.RemoveRange(DbSet.Where(i => i.Name == trueName));
        
        // 删除中间表
        Context.FileToInfos.RemoveRange(Context.FileToInfos.Where(i => i.Truename == trueName));
        
        SaveChanges();
    }

    public FilesInfo? GetOneByPickCode(string pickCode)
    {
        return DbSet.FirstOrDefault(i => i.PickCode == pickCode);
    }

    public FilesInfo? GetUpperLevelFolderInfoByFolderId(long id)
    {
        return DbSet.FirstOrDefault(i => i.FileId == null && i.CurrentId == id);

    }

    public List<FilesInfo> GetFolderListToRootByFolderId(long folderCid)
    {
        const int maxDepth = 30;

        List<FilesInfo> folderToRootList = [];

        FilesInfo upperLevelFolderInfo = new() { ParentId = folderCid };
        if (upperLevelFolderInfo.ParentId == null) return [];

        var pid = upperLevelFolderInfo.ParentId;
        
        for (var i = 0; i < maxDepth; i++)
        {
            if (pid == null) break;
            var upperFileInfo = GetUpperLevelFolderInfoByFolderId((long)pid);
            
            if (upperFileInfo == null) break;
            
            upperLevelFolderInfo = upperFileInfo;
            folderToRootList.Add(upperLevelFolderInfo);

            pid = upperLevelFolderInfo.ParentId;

            if (pid != 0) continue;
                
            folderToRootList.Add(new FilesInfo { CurrentId = 0, Name = "根目录" });
            break;
        }

        folderToRootList.Reverse();

        return folderToRootList;
    }

    private List<FilesInfo> GetAllFileListTraverse(long id, List<FilesInfo> allFileList)
    {
        var list = GetPartFileListById(id);
        
        list.ForEach(item =>
        {
            // 文件夹，再向下查询
            if (item.FileId == default)
            {
                allFileList = GetAllFileListTraverse(id, allFileList);
            }
        });
        allFileList.AddRange(list);
        return allFileList;
    }

}