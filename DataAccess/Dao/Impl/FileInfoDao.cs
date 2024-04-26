using DataAccess.Context;
using DataAccess.Dao.Interface;
using DataAccess.Models.Entity;
using Microsoft.EntityFrameworkCore;
using FileInfo = DataAccess.Models.Entity.FileInfo;

namespace DataAccess.Dao.Impl;

public class FileInfoDao : BaseDao<FileInfo>, IFileInfoDao
{
    private readonly FileContext _fileContext = new();

    //
    // public List<FilesInfo> GetListByTrueName(string name)
    // {
    //     //return DbSet.SelectMany(fileInfo =>
    //     //        Context.FileToInfos.Where(fileToInfo =>
    //     //            fileInfo.Pc.Equals(fileToInfo.FilePickCode)
    //     //            && EF.Functions.Like(fileToInfo.Truename, name)).DefaultIfEmpty(),
    //     //    (info, middle) => info).ToList();
    //
    //     return (from fileInfo in _fileContext.FilesInfos
    //             join fileToInfo in _fileContext.FileToInfos
    //                 on fileInfo.PickCode equals fileToInfo.FilePickCode
    //             where EF.Functions.Like(fileToInfo.TrueName, name)
    //             select fileInfo).ToList();
    // }
    //
    // // TODO 未完成
    // public void GetVideoInfoInFailStatusList(VideoInfoInFailStatusQueryDto queryDto)
    // {
    //     var query = _fileContext.FilesInfos.SelectMany(
    //         i => _fileContext.FileToInfos
    //             .Where(middle => middle.FilePickCode.Equals(i.PickCode)
    //                                  && EF.Functions.Like(middle.TrueName, queryDto.Name))
    //             .DefaultIfEmpty(),
    //         (i, _) => i
    //     ).Skip(queryDto.Position)
    //     .Take(queryDto.Take);
    //
    //     if (queryDto.IsRandom)
    //     {
    //         query = query.OrderBy(_ => EF.Functions.Random());
    //     }
    //     else
    //     {
    //         //query.OrderBy(x=>x.)
    //     }
    // }

    public async Task<List<FileInfo>> GetAllFilesListByFolderIdAsync(long folderId)
    {
        return await GetAllFileListTraverseAsync(folderId, []);
    }

    public List<FileInfo> GetPartFolderListByPid(long pid, int? limit = null)
    {
        var queryable = _fileContext.FileInfo.Where(i => i.ParentId == pid);
        
        if (limit != null) queryable = queryable.Take(limit.Value);

        return queryable.AsNoTracking().ToList();
    }

    public List<FileInfo> GetPartFileListByPid(long folderId, int? limit = null)
    {
        // i.FileId != default无法过滤
        // var queryable = DbSet.Where(i => (i.FileId != default && i.CurrentId == folderId ) || i.ParentId == folderId); 
        var queryable = _fileContext.FileInfo.Where(i => (i.FileId > 0 && i.CurrentId == folderId ) || i.ParentId == folderId);  
        
        if (limit != null) queryable = queryable.Take(limit.Value);


        var filesInfos = queryable.AsNoTracking().ToList();
        return filesInfos;
    }
    //
    // public async Task ExecuteRemoveAllByFolderIdAsync(long folderId)
    // {
    //     await RemoveAllByFolderIdAsync(folderId);
    //     await _fileContext.SaveChangesAsync();
    // }

    public bool IsFolderExistsById(long id)
    {
        return _fileContext.FileInfo.FirstOrDefault(i => i.FileId <= 0 && i.CurrentId == id) != null;
    }
    //
    // public async Task RemoveAllByFolderIdAsync(long folderId)
    // {
    //     //加上文件夹本身
    //     var folderInfo = _fileContext.FilesInfos.AsNoTracking().FirstOrDefault(i => i.CurrentId == folderId);
    //     if (folderInfo == null) return;
    //
    //     List<FilesInfo> removeList = [folderInfo];
    //
    //     //文件夹下所有的文件
    //     var filesInFolder = await GetAllFilesListByFolderIdAsync(folderId);
    //
    //     removeList.AddRange(filesInFolder);
    //
    //     _fileContext.FilesInfos.RemoveRange(removeList);
    //
    //     // 删除中间表
    //     foreach (var filesInfo in removeList)
    //     {
    //         _fileContext.FileToInfos.RemoveRange(
    //             _fileContext.FileToInfos.Where(i=>i.FilePickCode == filesInfo.PickCode));
    //     }
    // }
    //
    // public void RemoveByPickCode(string pickCode)
    // {
    //     // DbSet.Where(i => i.PickCode == pickCode).ExecuteDelete();
    //     //
    //     // 删除中间表
    //     _fileContext.FileToInfos.RemoveRange(_fileContext.FileToInfos.Where(i => i.FilePickCode == pickCode));
    //     
    //     _fileContext.SaveChanges();
    // }

    public void ExecuteRemoveByTrueName(string trueName)
    {
        // _fileContext.Files.RemoveRange(_fileContext.Files.Where(i => i.Name == trueName));
        //
        // // 删除中间表
        // _fileContext.Files.RemoveRange(_fileContext.Files.Where(i => i.TrueName == trueName));
        //
        // _fileContext.SaveChanges();
    }


    public FileInfo? GetOneByPickCode(string pickCode)
    {
        return _fileContext.FileInfo.FirstOrDefault(i => i.PickCode == pickCode);
    }

    public FileInfo? GetUpperLevelFolderInfoByFolderId(long id)
    {
        return _fileContext.FileInfo.FirstOrDefault(i => i.FileId <= 0 && i.CurrentId == id);

    }
    //
    // public List<Files> GetFolderListToRootByFolderId(long folderCid)
    // {
    //     const int maxDepth = 30;
    //
    //     List<Files> folderToRootList = [];
    //
    //     Files upperLevelFolder = new() { ParentId = folderCid };
    //     if (upperLevelFolder.ParentId == null) return [];
    //
    //     var pid = upperLevelFolder.ParentId;
    //     
    //     for (var i = 0; i < maxDepth; i++)
    //     {
    //         if (pid == null) break;
    //         var upperFileInfo = GetUpperLevelFolderInfoByFolderId(pid.Value);
    //         
    //         if (upperFileInfo == null) break;
    //         
    //         upperLevelFolder = upperFileInfo;
    //         folderToRootList.Add(upperLevelFolder);
    //
    //         pid = upperLevelFolder.ParentId;
    //
    //         if (pid != 0) continue;
    //             
    //         folderToRootList.Add(new Files { CurrentId = 0, Name = "根目录" });
    //         break;
    //     }
    //
    //     folderToRootList.Reverse();
    //
    //     return folderToRootList;
    // }

    private async Task<List<FileInfo>> GetAllFileListTraverseAsync(long id, List<FileInfo> allFileList)
    {
        var list = await _fileContext.FileInfo.Where(i => (i.FileId > 0 && i.CurrentId == id) || i.ParentId == id)
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