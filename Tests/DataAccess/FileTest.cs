using DataAccess.Context;
using DataAccess.Dao.Impl;
using DataAccess.Dao.Interface;
using DataAccess.Models.Entity;
using SQLitePCL;
using FileInfo = DataAccess.Models.Entity.FileInfo;

namespace Tests.DataAccess;

[TestClass]
public class FileTest
{
    private readonly IFileInfoDao _fileInfoDao = new FileInfoDao();
    
    [TestMethod]
    public void InitFileTest()
    {
        var fileInfoDao = new FileInfoDao();
        fileInfoDao.InitTest();
    }

    /// <summary>
    /// 导入115中的文件
    /// </summary>
    [TestMethod]
    public void AddFileInfoTest()
    {
        var fileInfo = new FileInfo
        {
            Time = "123",
            PickCode = "123",
            Name = "nihao"
        };
        _fileInfoDao.AddAndSaveChanges(fileInfo);
    }
}