using DataAccess.Dao.Impl;
using DataAccess.Models.Entity;
using DataAccess.Models.Vo;

namespace DataAccessTest.Step;

public class UpdateActorInfoTest
{
    private readonly ActorInfoDao _actorInfoDao = new();
    
    [Test]
    public void UpdateTest()
    {
        _actorInfoDao.InitData();

        var actorInfo = new ActorInfo()
        {
            
        };


        _videoInfoDao.AddOrUpdateByVideoInfoVo(videoInfoVo);
    }
}