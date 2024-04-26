using DataAccess.Dao.Impl;
using DataAccess.Dao.Interface;
using DataAccess.Models.Entity;

namespace Tests.DataAccess.SingleTable;

[TestClass]
public class VideoInfoTest
{
    private readonly IVideoInfoDao _videoInfoDao = new VideoInfoDao();
    private readonly IFileInfoDao _fileInfoDao = new FileInfoDao();

    /// <summary>
    /// 导入视频信息
    /// </summary>
    [TestMethod]
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

        _videoInfoDao.AddAndSaveChanges(videoInfo);
        
        Assert.AreEqual(1, videoInfo.Id);
        var videoInfoDb = _videoInfoDao.GetById(videoInfo.Id);
        Assert.AreNotEqual(null, videoInfoDb);
    }
}