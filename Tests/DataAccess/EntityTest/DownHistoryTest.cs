using DataAccess.Dao.Impl;
using DataAccess.Models.Entity;

namespace Tests.DataAccess.EntityTest;

[TestClass]
public class DownHistoryTest
{
    private readonly DownHistoryDao _dao = new();

    private readonly List<DownHistory> _infos =
    [
        new DownHistory
        {
            FilePickCode = "150",
            FileName = "151",
            Ua = "152"
        },
        new DownHistory
        {
            FilePickCode = "160",
            FileName = "161",
            Ua = "162"
        }
    ];
        
    [TestMethod]
    public void GetTest()
    {
        var info = _dao.List();
        Assert.IsTrue(info.Count > 0);
    }

    [TestMethod]
    public void CurdTest()
    {
        _dao.Delete();
        var info = _infos.FirstOrDefault();
        if (info == null) return;

        // 增
        _infos.ForEach(_dao.Add);

        // 查
        // 查询全部
        var checkInfos = _dao.List();
        // 大小断言
        Assert.AreEqual(_infos.Count, checkInfos.Count);
        for (var i = 0; i < checkInfos.Count; i++)
        {
            // 内容断言
            Assert.AreEqual(_infos[i], checkInfos[i]);
        }

        // Key查询
        var databaseVideoInfo = _dao.Find(info.FilePickCode, info.Ua);
        Assert.AreEqual(info, databaseVideoInfo);

        // 条件查询
        databaseVideoInfo = _dao.FirstOrDefault(i => i.FilePickCode == info.FilePickCode);
        Assert.AreEqual(info, databaseVideoInfo);

        // 改
        info.TrueUrl = "15222222";
        _dao.UpdateSingle(info);

        databaseVideoInfo = _dao.Find(info.FilePickCode, info.Ua);
        Assert.AreEqual(info, databaseVideoInfo);

        // 删
        for (var i = _infos.Count - 1; i >= 0; i--)
        {
            var removeInfo = _infos[i];
            _dao.ExecuteRemoveSingle(removeInfo);

            _infos.RemoveAt(i);
            Assert.AreEqual(_infos.Count, _dao.Count());
        }
        
    }
}