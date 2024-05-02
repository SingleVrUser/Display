using DataAccess.Dao.Impl;
using DataAccess.Dao.Interface;
using DataAccess.Models.Entity;

namespace DataAccessTest.SingleTable;

public class VideoInfoTest
{
    private readonly IVideoInfoDao _videoInfoDao = new VideoInfoDao();

    /// <summary>
    /// 导入视频信息
    /// </summary>
    [Test]
    public void AddVideoInfo()
    {
        _videoInfoDao.InitData();
        
        var videoInfo = new VideoInfo("123")
        {
            SourceUrl = "123",
            ActorInfoList = [new ActorInfo("貂蝉")
            {
                Bwh = new BwhInfo {Bust = 135, Waist = 150, Hips = 150}
            }]
        };

        _videoInfoDao.ExecuteAdd(videoInfo);
        
        Assert.That(videoInfo.Id, Is.EqualTo(1));
        var videoInfoDb = _videoInfoDao.GetById(videoInfo.Id);
        Assert.That(videoInfoDb, Is.Not.EqualTo(null));
    }
}