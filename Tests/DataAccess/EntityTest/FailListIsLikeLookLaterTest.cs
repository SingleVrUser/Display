using DataAccess.Dao.Impl;
using DataAccess.Models.Entity;

namespace Tests.DataAccess.EntityTest;

[TestClass]
public class FailListIsLikeLookLaterTest
{
    private readonly FailListIsLikeLookLaterDao _dao = new();

    private readonly List<FailListIsLikeLookLater> _dataList = [
        new FailListIsLikeLookLater
        {
            PickCode = "12354871",
            IsLike = 1,
            Score = 1
        },
        new FailListIsLikeLookLater
        {
            PickCode = "12354872",
            IsLike = 1,
            Score = 1
        },
    ];
    
    [TestMethod]
    public void CrudTest()
    {
        _dao.Delete();
        var info = _dataList.FirstOrDefault();
        if (info == null) return;

        // 增
        _dataList.ForEach(_dao.Add);

        // 查
        // 查询全部
        var checkInfos = _dao.List();
        // 大小断言
        Assert.AreEqual(_dataList.Count, checkInfos.Count);
        for (var i = 0; i < checkInfos.Count; i++)
        {
            // 内容断言
            Assert.AreEqual(_dataList[i], checkInfos[i]);
        }

        // Key查询
        var databaseVideoInfo = _dao.Find(info.PickCode);
        Assert.AreEqual(info, databaseVideoInfo);

        // 条件查询
        databaseVideoInfo = _dao.FirstOrDefault(i => i.PickCode == info.PickCode);
        Assert.AreEqual(info, databaseVideoInfo);

        // 改
        info.IsLike = 1;
        _dao.UpdateSingle(info);

        databaseVideoInfo = _dao.Find(info.PickCode);
        Assert.AreEqual(info, databaseVideoInfo);

        // 删
        for (var i = _dataList.Count - 1; i >= 0; i--)
        {
            var removeInfo = _dataList[i];
            _dao.ExecuteRemoveSingle(removeInfo);

            _dataList.RemoveAt(i);
            Assert.AreEqual(_dataList.Count, _dao.Count());
        }
    }
    
}