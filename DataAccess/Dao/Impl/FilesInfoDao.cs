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
                where EF.Functions.Like(fileToInfo.TrueName, name)
                select fileInfo).ToList();
    }

    // TODO 未完成
    public void GetVideoInfoInFailStatusList(VideoInfoInFailStatusQueryDto queryDto)
    {
        var query = DbSet.SelectMany(
            i => Context.FileToInfos
                .Where(middle => middle.FilePickCode.Equals(i.PickCode)
                                     && EF.Functions.Like(middle.TrueName, queryDto.Name))
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

    public async Task<List<FilesInfo>> GetAllFilesListByFolderIdAsync(long folderId)
    {
        return await GetAllFileListTraverseAsync(folderId, []);
    }

    public List<FilesInfo> GetPartFolderListByPid(long pid, int? limit = null)
    {
        var queryable = DbSet.Where(i => i.ParentId == pid);
        
        if (limit != null) queryable = queryable.Take(limit.Value);

        return queryable.AsNoTracking().ToList();
    }

    public List<FilesInfo> GetPartFileListByPid(long folderId, int? limit = null)
    {
        // i.FileId != default无法过滤
        // var queryable = DbSet.Where(i => (i.FileId != default && i.CurrentId == folderId ) || i.ParentId == folderId); 
        var queryable = DbSet.Where(i => (i.FileId > 0 && i.CurrentId == folderId ) || i.ParentId == folderId);  
        
        if (limit != null) queryable = queryable.Take(limit.Value);


        var filesInfos = queryable.AsNoTracking().ToList();
        return filesInfos;
    }

    public async Task ExecuteRemoveAllByFolderIdAsync(long folderId)
    {
        await RemoveAllByFolderIdAsync(folderId);
        await Context.Instance.SaveChangesAsync();
    }

    public bool IsFolderExistsById(long id)
    {
        return DbSet.FirstOrDefault(i => i.FileId <= 0 && i.CurrentId == id) != null;
    }

    public async Task RemoveAllByFolderIdAsync(long folderId)
    {
        //加上文件夹本身
        var folderInfo = DbSet.AsNoTracking().FirstOrDefault(i => i.CurrentId == folderId);
        if (folderInfo == null) return;

        List<FilesInfo> removeList = [folderInfo];

        //文件夹下所有的文件
        var filesInFolder = await GetAllFilesListByFolderIdAsync(folderId);

        removeList.AddRange(filesInFolder);

        DbSet.RemoveRange(removeList);

        // 删除中间表
        foreach (var filesInfo in removeList)
        {
            Context.FileToInfos.RemoveRange(
                Context.FileToInfos.Where(i=>i.FilePickCode == filesInfo.PickCode));
        }
    }

    public void RemoveByPickCode(string pickCode)
    {
        // DbSet.Where(i => i.PickCode == pickCode).ExecuteDelete();
        //
        // 删除中间表
        Context.FileToInfos.RemoveRange(Context.FileToInfos.Where(i => i.FilePickCode == pickCode));
        
        SaveChanges();
    }

    public void ExecuteRemoveByTrueName(string trueName)
    {
        DbSet.RemoveRange(DbSet.Where(i => i.Name == trueName));
        
        // 删除中间表
        Context.FileToInfos.RemoveRange(Context.FileToInfos.Where(i => i.TrueName == trueName));
        
        SaveChanges();
    }


    public FilesInfo? GetOneByPickCode(string pickCode)
    {
        return DbSet.FirstOrDefault(i => i.PickCode == pickCode);
    }

    public FilesInfo? GetUpperLevelFolderInfoByFolderId(long id)
    {
        return DbSet.FirstOrDefault(i => i.FileId <= 0 && i.CurrentId == id);

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
            var upperFileInfo = GetUpperLevelFolderInfoByFolderId(pid.Value);
            
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

    private async Task<List<FilesInfo>> GetAllFileListTraverseAsync(long id, List<FilesInfo> allFileList)
    {
        var list = await DbSet.Where(i => (i.FileId > 0 && i.CurrentId == id) || i.ParentId == id)
            .AsNoTracking().ToListAsync();
        
        foreach (var item in list)
        {
            // 文件夹，再向下查询
            if (item.FileId == default)
            {
                allFileList = await GetAllFileListTraverseAsync(item.CurrentId, allFileList);
            }
        }
        
        allFileList.AddRange(list);
        return allFileList;
    }

}