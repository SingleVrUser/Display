using DataAccess.Dao.Impl;

namespace DataAccessTest.Step;

public class UpdateActorInfoTest
{
    private readonly ActorInfoDao _actorInfoDao = new();
    
    [Test]
    public void UpdateTest()
    {
        _actorInfoDao.Update();
    }
}