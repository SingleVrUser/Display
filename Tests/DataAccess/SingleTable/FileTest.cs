using DataAccess.Dao.Impl;
using DataAccess.Dao.Interface;
using FileInfo = DataAccess.Models.Entity.FileInfo;

namespace Tests.DataAccess.SingleTable;

[TestClass]
public class FileTest
{
    private readonly IFileInfoDao _fileInfoDao = new FileInfoDao();
    
    /// <summary>
    /// 导入115中的文件
    /// </summary>
    [TestMethod]
    public void AddFileInfoTest()
    {
        _fileInfoDao.InitData();
        
        var fileInfo = new FileInfo
        {
            Time = "123",
            PickCode = "123",
            Name = "nihao"
        };
        _fileInfoDao.AddAndSaveChanges(fileInfo);
        
        Assert.AreEqual(1, fileInfo.Id);
        var fileInfoDb = _fileInfoDao.GetById(fileInfo.Id);
        Assert.AreNotEqual(null, fileInfoDb);
        
        fileInfo = new FileInfo
        {
            Time = "124",
            PickCode = "124",
            Name = "wohao"
        };
        _fileInfoDao.AddAndSaveChanges(fileInfo);
        Assert.AreEqual(2, fileInfo.Id);
        fileInfoDb = _fileInfoDao.GetById(fileInfo.Id);
        Assert.AreNotEqual(null, fileInfoDb);
    }
}