using System.Diagnostics;
using AutoFixture;
using AutoFixture.NUnit3;
using DataAccess.Dao.Impl;
using DataAccess.Dao.Interface;
using DataAccess.Models.Entity;

namespace DataAccessTest.SingleTable;

public class ActorInfoTest
{
    private readonly IFixture _fixture = new Fixture();
    
    private readonly IActorInfoDao _actorInfoDao = new ActorInfoDao();

    [Test]
    public void Init()
    {
        _actorInfoDao.InitData();
        
    }
    
    [Test]
    public void WhereTest()
    {
        var actorInfo = _actorInfoDao.GetById(1L);
        Debug.WriteLine(actorInfo);
    }
    
    
    [Test, AutoData]
    public void AddTest()
    {
        _actorInfoDao.InitData();;
        
        var actorInfo = new ActorInfo("演员名称1")
        {
            Birthday = "生日1",
            Height = 160,
            WorksCount = 120,
            WorkTime = "135",
            ProfilePath = "头像地址",
            BlogUrl = _fixture.Create<string>(),
            InfoUrl = _fixture.Create<string>(),
            Bwh = new BwhInfo
            {
                Bust = _fixture.Create<int>(),
                Hips = _fixture.Create<int>(),
                Waist = _fixture.Create<int>()
            },
            NameList =
            [
                new ActorName(_fixture.Create<string>()),
                new ActorName(_fixture.Create<string>())
            ],
            Interest = new ActorInterest()
            {
                IsLike = true,
                Score = 2.0
            }
        };

        _actorInfoDao.ExecuteAdd(actorInfo);
    }

}