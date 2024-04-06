using DataAccess;
using DataAccess.Dao.Impl;
using DataAccess.Models.Entity;

namespace Tests.DataAccess.EntityTest;

[TestClass]
public class VideoInfoTest
{
    private static readonly Context Context = Context.Instance;
    private static readonly List<VideoInfo> VideoInfos = [];

    [ClassInitialize]
    public static void ClassInitialize(TestContext testContext)
    {
        Context.Database.EnsureCreated();

        VideoInfos.Add(new VideoInfo
        {
            TrueName = "123",
            Actor = "123",
            Url = "123",
            Category = "123",
            ImagePath = "123",
            IsLike = 1,
            Score = 2
        });
        VideoInfos.Add(new VideoInfo
        {
            TrueName = "235",
            Actor = "123",
            Url = "123",
            Category = "123",
            ImagePath = "123",
            IsLike = 1,
            Score = 2
        });
    }

    [ClassCleanup]
    public static void ClassCleanup()
    {
        // Context.Database.EnsureDeleted();
    }

    // 增，删，改，查
    [TestMethod]
    public void CrudTest()
    {
        var dao = new VideoInfoDao();
        
        dao.Delete();
        var info = VideoInfos.FirstOrDefault();
        if (info == null) return;

        // 增
        VideoInfos.ForEach(dao.ExecuteAdd);

        // 查
        // 查询全部
        var checkInfos = dao.List();
        // 大小断言
        Assert.AreEqual(VideoInfos.Count, checkInfos.Count);
        for (var i = 0; i < checkInfos.Count; i++)
        {
            // 内容断言
            Assert.AreEqual(VideoInfos[i], checkInfos[i]);
        }

        // Key查询
        var databaseVideoInfo = dao.Find(info.TrueName);
        Assert.AreEqual(info, databaseVideoInfo);

        // 条件查询
        databaseVideoInfo = dao.FirstOrDefault(i => i.TrueName == info.TrueName);
        Assert.AreEqual(info, databaseVideoInfo);

        // 改
        info.ImagePath = "15222222";
        dao.ExecuteUpdate(info);

        databaseVideoInfo = dao.Find(info.TrueName);
        Assert.AreEqual(info, databaseVideoInfo);

        // 删
        for (var i = VideoInfos.Count - 1; i >= 0; i--)
        {
            var removeInfo = VideoInfos[i];
            dao.ExecuteRemoveSingle(removeInfo);

            VideoInfos.RemoveAt(i);
            Assert.AreEqual(VideoInfos.Count, dao.Count());
        }
    }
}