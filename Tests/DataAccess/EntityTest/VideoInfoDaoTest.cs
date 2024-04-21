using System.Diagnostics;
using DataAccess;
using DataAccess.Dao.Impl;

namespace Tests.DataAccess.EntityTest;

[TestClass]
public class VideoInfoDaoTest
{
    [TestMethod]
    public void GetOneTest()
    {
        Context.SetSavePath("D:\\库\\Documents\\winui");
        var videoInfoDao = new VideoInfoDao();


        var oneByTrueName = videoInfoDao.GetOneByTrueName("AOZ-322");
        Debug.WriteLine(oneByTrueName);
    }
}