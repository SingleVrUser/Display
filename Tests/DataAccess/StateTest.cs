using System.Diagnostics;
using DataAccess;
using DataAccess.Dao.Impl;
using DataAccess.Models.Entity;
using Display.Providers;

namespace Tests.DataAccess;

[TestClass]
public class StateTest
{
    [TestMethod]
    public void EntityStateTest()
    {
        Context.SetSavePath("D:/库/Documents/winui");
        var actorInfoDao = new ActorInfoDao();

        var name = "成沢めい";
        var actorInfoFromDb = actorInfoDao.GetPartInfoByActorName(name);

        
        Assert.IsNotNull(actorInfoFromDb);

        var entityState = actorInfoDao.GetEntityState(actorInfoFromDb);
        Debug.WriteLine(entityState);

        var modifyData = "modify";

        actorInfoDao.Attach(actorInfoFromDb);
        entityState = actorInfoDao.GetEntityState(actorInfoFromDb);
        Debug.WriteLine(entityState);

        actorInfoFromDb.BlogUrl = modifyData;
        actorInfoDao.SaveChanges();

        var actorInfoResult = actorInfoDao.GetPartInfoByActorName(name);
        Assert.IsTrue(actorInfoResult != null && actorInfoResult.BlogUrl == modifyData);
    }
}