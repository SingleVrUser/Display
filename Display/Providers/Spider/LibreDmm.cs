using Display.Helper.Network;
using Display.Models.Spider;
using HtmlAgilityPack;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DataAccess.Models.Dto;

namespace Display.Providers.Spider;

public class LibreDmm : BaseSpider
{
    public override SpiderSourceName Name => SpiderSourceName.Libredmm;
    public override string Abbreviation => "libre";
    public override string Keywords => "LibreFanza";

    public override bool IsOn
    {
        get => AppSettings.IsUseLibreDmm;
        set => AppSettings.IsUseLibreDmm = value;
    }

    public override string BaseUrl
    {
        get => AppSettings.LibreDmmBaseUrl;
        set => AppSettings.LibreDmmBaseUrl = value;
    }
    public override async Task<VideoInfoDto> GetInfoByCid(string cid, CancellationToken token)
    {
        var url = NetworkHelper.UrlCombine(BaseUrl, $"movies/{cid}");

        var result = await RequestHelper.RequestHtml(Common.Client, url, token);
        if (result == null) return null;

        var detailUrl = result.Item1;
        var htmlString = result.Item2;

        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(htmlString);

        return await GetInfoByHtmlDoc(cid, detailUrl, htmlDoc);
    }

    public override async Task<VideoInfoDto> GetInfoByHtmlDoc(string cid, string detailUrl, HtmlDocument htmlDoc)
    {
        //搜索封面
        string imageUrl = null;
        var imageUrlNode = htmlDoc.DocumentNode.SelectSingleNode("//img[@class='img-fluid']");
        if (imageUrlNode != null)
        {
            imageUrl = imageUrlNode.Attributes["src"].Value;
        }

        var videoInfo = new VideoInfoDto
        {
            SourceUrl = detailUrl,
            Name = cid,
            //dmm肯定没有步兵
            IsWm = false
        };

        var titleNode = htmlDoc.DocumentNode.SelectSingleNode("//h1");
        if (titleNode != null)
            videoInfo.Title = titleNode.InnerText.Trim();

        var keyNodes = htmlDoc.DocumentNode.SelectNodes("//div[@class='col-md-4']/dl/dt");
        if (keyNodes == null) return null;
        var valueNodes = htmlDoc.DocumentNode.SelectNodes("//div[@class='col-md-4']/dl/dd");
        //其他信息
        for (var i = 0; i < keyNodes.Count; i++)
        {
            var key = keyNodes[i].InnerText;
            switch (key)
            {
                case "Release Date":
                    videoInfo.ReleaseTime = valueNodes[i].InnerText.Trim();
                    break;
                case "Directors":
                    videoInfo.DirectorName = valueNodes[i].InnerText.Trim();
                    break;
                case "Genres":
                    var genresNodes = valueNodes[i].SelectNodes("ul/li");
                    videoInfo.CategoryList = genresNodes.Select(x => x.InnerText.Trim()).ToList();
                    break;
                case "Labels":
                    videoInfo.SeriesName = valueNodes[i].InnerText.Trim();
                    break;
                case "Makers":
                    videoInfo.ProducerName = valueNodes[i].InnerText.Trim();
                    break;
                case "Volume":
                    videoInfo.LengthTime = valueNodes[i].InnerText.Trim().Replace(" minutes", "分钟");
                    break;
            }
        }

        //演员
        var actressesNodes = htmlDoc.DocumentNode.SelectNodes("//div[@class='card actress']");

        if (actressesNodes != null)
        {
            videoInfo.ActorNameList = actressesNodes.Select(x => x.InnerText.Trim()).ToList();
        }

        //下载封面
        if (string.IsNullOrEmpty(imageUrl)) return videoInfo;

        var savePath = AppSettings.ImageSavePath;
        var filePath = Path.Combine(savePath, cid);
        videoInfo.ImageUrl = imageUrl;
        videoInfo.ImagePath = await DbNetworkHelper.DownloadFile(imageUrl, filePath, cid);

        return videoInfo;
    }
}
