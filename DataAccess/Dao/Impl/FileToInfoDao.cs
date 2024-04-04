using DataAccess.Dao.Interface;
using DataAccess.Models.Entity;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Dao.Impl;

public class FileToInfoDao : DaoImpl<FileToInfo>, IFileToInfoDao
{
    public void UpdateIsSuccessByTrueName(string trueName, int isSuccess)
    {
        var fileToInfos = DbSet.Where(i => i.Truename == trueName).ToList();
        foreach (var fileToInfo in fileToInfos)
        {
            fileToInfo.Issuccess = isSuccess;
        }
        SaveChanges();
    }
}