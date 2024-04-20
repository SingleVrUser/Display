using DataAccess.Context;
using DataAccess.Dao.Impl;
using SQLitePCL;

namespace Tests.DataAccess;

[TestClass]
public class FileTest
{
    [TestMethod]
    public void GetFileInfo()
    {
        var filesInfoDao = new FilesInfoDao();
        filesInfoDao.InitTest();

        filesInfoDao.AddTest();

        // var postContext = new PostContext();
        // postContext.Database.EnsureDeleted();
        // postContext.Database.EnsureCreated();
    }
}