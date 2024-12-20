using System.Diagnostics;
using DataAccess.Dao.Impl;
using DataAccess.Dao.Interface;
using DataAccess.Models.Entity;
using DataAccess.Models.Dto;

namespace DataAccessTest.SingleTable;

public class VideoInfoTest
{
    private readonly IVideoInfoDao _videoInfoDao = new VideoInfoDao();

    [Test]
    public void WhereTest()
    {
        var videoInfo = _videoInfoDao.GetById(1L);
        Debug.WriteLine(videoInfo);
    }

    [Test]
    public void RandomTest()
    {
        var list = _videoInfoDao.GetRandomList(0, 20);
        Debug.WriteLine(list);
    }
    
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
            }],
            CategoryList = [new CategoryInfo("标签1"), new CategoryInfo("标签2")]
        };

        var videoInfo2 = new VideoInfo("124")
        {
            SourceUrl = "124",
            ActorInfoList = [new ActorInfo("貂蝉2")
            {
                Bwh = new BwhInfo {Bust = 135, Waist = 150, Hips = 150}
            }],
            CategoryList = [new CategoryInfo("标签1"), new CategoryInfo("标签2")]
        };

        _videoInfoDao.ExecuteAdd(videoInfo);
        _videoInfoDao.ExecuteAdd(videoInfo2);


        Assert.That(videoInfo.Id, Is.EqualTo(1));
        var videoInfoDb = _videoInfoDao.GetById(videoInfo.Id);
        Assert.That(videoInfoDb, Is.Not.EqualTo(null));
    }
    
    
    /**
     * 仅添加一个视频信息
     */
    [Test]
    public void SearchVideoInfoTest()
    {
        _videoInfoDao.InitData();
        
        var videoInfoVo = new VideoInfoDto
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
        var videoInfoVo = new VideoInfoDto
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
    
    [Test]
    public void UpdateByVideoInfoVoTest()
    {
        var videoInfoVo = new VideoInfoDto
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