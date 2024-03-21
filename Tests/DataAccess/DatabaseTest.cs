using DataAccess;
using DataAccess.Dao.Impl;
using DataAccess.Models.Entity;

namespace Tests.DataAccess;

[TestClass]
public class DatabaseTest
{
    /**
     * 指定位置创建数据库
     */
    [TestMethod]
    [DataRow(null)] // 默认
    [DataRow("D:/")] // 指定
    public void CreateDatabaseTest(string savePath)
    {
        // 设置位置
        Context.SetSavePath(savePath);

        var path = Context.DbPath;
        // 如果数据库文件存在,则删除
        if (File.Exists(path)) File.Delete(path);

        var context = Context.Instance;
        var result = context.Database.EnsureCreated();
        Assert.IsTrue(result);

        context.Database.EnsureDeleted();
    }



    [TestMethod]
    public void GetDbSet()
    {
        var context = Context.Instance;
        var i = context.Set<VideoInfo>();
        Assert.AreEqual(context.VideoInfos, i);

    }
}