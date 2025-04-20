using DataAccess.Dao.Interface;
using DataAccess.Models.Entity;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Dao.Impl;

public class FileToInfoDao : DaoImpl<FileToInfo>, IFileToInfoDao
{
    public void UpdateIsSuccessByTrueName(string trueName, int isSuccess)
    {
        var fileToInfos = DbSet.Where(i => i.TrueName == trueName).ToList();
        foreach (var fileToInfo in fileToInfos)
        {
            fileToInfo.IsSuccess = isSuccess;
        }
        SaveChanges();
    }

    public void ExecuteInitIfNoExists(FileToInfo info)
    {
        var fileToInfo = DbSet.AsNoTracking().FirstOrDefault(i => i.FilePickCode == info.FilePickCode);
        if (fileToInfo != null) return;

        DbSet.Add(info);
        SaveChanges();
    }

}