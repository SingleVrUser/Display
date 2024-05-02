using DataAccess.Dao.Impl;
using DataAccess.Dao.Interface;
using DataAccess.Models.Vo;

namespace DataAccessTest.DataAccess.Step;

// [TestClass]
public class ImportVideoInfoTest
{
    private readonly IVideoInfoDao _videoInfoDao = new VideoInfoDao();
    
    /**
     * 仅添加一个视频信息
     */
    [Test]
    public void SearchVideoInfoTest()
    {
        _videoInfoDao.InitData();
        
        var videoInfoVo = new VideoInfoVo
        {
            Name = "名字",
            Title = "标题1",
            ImageUrl = "图片地址1，图片地址2",
            SourceUrl = "来源地址",
            ReleaseTime = "发布时间",
            LengthTime = "视频时长",
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
        
        _videoInfoDao.AddOrUpdateByVideoInfoVo(videoInfoVo);
    }

    /**
     * 添加视频信息，并绑定文件
     */
    [Test]
    public void ImportVideoAndAttachFile()
    {
        var videoInfoVo = new VideoInfoVo
        {
            Name = "名字",
            Title = "标题1",
            ImageUrl = "图片地址1，图片地址2",
            SourceUrl = "来源地址",
            ReleaseTime = "发布时间",
            LengthTime = "视频时长",
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
        
        _videoInfoDao.AddOrUpdateInfoAndAttachFile(videoInfoVo, [1, 2]);
    }

}