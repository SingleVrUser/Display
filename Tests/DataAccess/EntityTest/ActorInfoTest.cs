using System.Diagnostics;
using DataAccess.Dao.Impl;
using DataAccess.Dao.Interface;
using DataAccess.Models.Entity;

namespace Tests.DataAccess.EntityTest;

[TestClass]
public class ActorInfoTest
{
    private readonly IActorInfoDao _actorInfoDao = new ActorInfoDao();
    
    [TestMethod]
    public void UploadSingleTest()
    {
        // var actorInfo = new ActorInfo
        // {
        //     IsLike = 1
        // };
        // _actorInfoDao.ExecuteUpdate(actorInfo);
        //
        // Debug.WriteLine(actorInfo);
    }
}