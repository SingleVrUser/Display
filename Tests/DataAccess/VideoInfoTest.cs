using DataAccess.Context;
using DataAccess.Dao.Impl;
using DataAccess.Dao.Interface;
using DataAccess.Models.Entity;

namespace Tests.DataAccess;

[TestClass]
public class VideoInfoTest
{
    private readonly IVideoInfoDao _videoInfoDao = new VideoInfoDao();

    /// <summary>
    /// 导入视频信息
    /// </summary>
    [TestMethod]
    public void AddVideoInfo()
    {
        // _videoInfoDao.AddAndSaveChanges(1, new VideoInfo
        // {
        //     TrueName = "abp-123",
        //     Actor = "123",
        //     Interest = new VideoInterest {IsLike = true}
        // });

        _videoInfoDao.InitData();
        _videoInfoDao.AddAndSaveChanges(new VideoInfo
        {
            TrueName = "ACB-123",
            Actor = "123",
            Interest = new VideoInterest {IsLookAfter = true},
            ActorInfos = [new ActorInfo
            {
                Name = "貂蝉",
                BwhInfo = new Bwh {Bust = 135, Waist = 150, Hips = 150}
            }],
            
        });
        
        
    }
}