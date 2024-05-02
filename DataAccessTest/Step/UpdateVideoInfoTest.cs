using DataAccess.Dao.Impl;
using DataAccess.Models.Vo;

namespace DataAccessTest.Step;

public class UpdateVideoInfoTest
{
    private readonly VideoInfoDao _videoInfoDao = new();
    
    [Test]
    public void UpdateByVideoInfoVoTest()
    {
        var videoInfoVo = new VideoInfoVo
        {
            Name = "名字",
            Title = "修改后的标题1",
            ImageUrl = "修改后的图片地址1，修改后的图片地址2",
            SourceUrl = "修改后的来源地址",
            ReleaseTime = "修改后的发布时间",
            LengthTime = "修改后的视频时长",
            DirectorName = "修改后的导演1",
            ProducerName = "修改后的厂商2",
            PublisherName = "修改后的发布者1",
            SeriesName = "修改后的系列名称",
            ActorNameList = [
                "修改后的演员名称1",
                "修改后的演员名称2"
            ],
            CategoryList = [
                "修改后的标签1",
                "修改后的标签2",
                "修改后的标签3"
            ],
            SampleImageList = [
                "修改后的123",
                "修改后的234",
                "修改后的234"
            ]
        };

        _videoInfoDao.AddOrUpdateByVideoInfoVo(videoInfoVo);
    }
}