using DataAccess.Dao.Impl;
using DataAccess.Dao.Interface;
using DataAccess.Models.Vo;

namespace Tests.DataAccess.Step;

[TestClass]
public class ImportVideoInfoTest
{
    private readonly IVideoInfoDao _videoInfoDao = new VideoInfoDao();
    
    [TestMethod]
    public void SearchVideoInfoTest()
    {
        _videoInfoDao.InitData();
        
        var videoInfoVo = new VideoInfoVo
        {
            Name = "123",
            ImageUrl = "123",
            SourceUrl = "123",
            LengthTime = "135",
            DirectorName = "导演1",
            ProducerName = "厂商2",
            PublisherName = "发布者1",
            SeriesName = "asdf",
            ActorNameList = [
                "演员名称1",
                "演员名称2"
            ],
            CategoryList = [
                "标签1",
                "标签2",
                "标签3"
            ],
            SampleImageList = [
                "123",
                "234",
                "234"
            ]
        };
        
        _videoInfoDao.AddByVideoInfoVo(videoInfoVo);
    }
}