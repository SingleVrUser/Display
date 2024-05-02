using DataAccess.Dao.Impl;
using DataAccess.Dao.Interface;
using FileInfo = DataAccess.Models.Entity.FileInfo;

namespace DataAccessTest.SingleTable;

public class FileTest
{
    private readonly IFileInfoDao _fileInfoDao = new FileInfoDao();
    
    /// <summary>
    /// 导入115中的文件
    /// </summary>
    [Test]
    public void AddFileInfoTest()
    {
        _fileInfoDao.InitData();
        
        var fileInfo = new FileInfo
        {
            Time = "123",
            PickCode = "123",
            Name = "nihao"
        };
        _fileInfoDao.ExecuteAdd(fileInfo);
        
        Assert.That(fileInfo.Id, Is.EqualTo(1));
        var fileInfoDb = _fileInfoDao.GetById(fileInfo.Id);
        Assert.That(fileInfoDb, Is.Not.EqualTo(null));
        
        fileInfo = new FileInfo
        {
            Time = "124",
            PickCode = "124",
            Name = "wohao"
        };
        _fileInfoDao.ExecuteAdd(fileInfo);
        Assert.That(fileInfo.Id, Is.EqualTo(2));
        fileInfoDb = _fileInfoDao.GetById(fileInfo.Id);
        Assert.That(fileInfoDb, Is.Not.EqualTo(null));
    }
}