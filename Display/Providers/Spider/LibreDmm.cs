using Display.Helper.Network;
using Display.Models.Spider;
using HtmlAgilityPack;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Display.Models.Dto.OneOneFive;
using Display.Providers.Downloader;

namespace Display.Providers.Spider;

public class LibreDmm : BaseSpider
{
    public override SpiderNameAndStatus.SpiderSourceName Name => SpiderNameAndStatus.SpiderSourceName.Libredmm;
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
    public override async Task<VideoInfo> GetInfoByCid(string cid, CancellationToken token)
    {
        cid = cid.ToUpper();
        var url = NetworkHelper.UrlCombine(BaseUrl, $"movies/{cid}");

        var result = await RequestHelper.RequestHtml(Common.Client, url, token);
        if (result == null) return null;

        var detailUrl = result.Item1;
        var htmlString = result.Item2;

        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(htmlString);

        return await GetInfoByHtmlDoc(cid, detailUrl, htmlDoc);
    }

    public override async Task<VideoInfo> GetInfoByHtmlDoc(string cid, string detailUrl, HtmlDocument htmlDoc)
    {
        //搜索封面
        string imageUrl = null;
        var imageUrlNode = htmlDoc.DocumentNode.SelectSingleNode("//img[@class='img-fluid']");
        if (imageUrlNode != null)
        {
            imageUrl = imageUrlNode.Attributes["src"].Value;
        }

        var videoInfo = new VideoInfo
        {
            busUrl = detailUrl,
            trueName = cid,
            //dmm肯定没有步兵
            IsWm = 0
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
                    videoInfo.Director = valueNodes[i].InnerText.Trim();
                    break;
                case "Genres":
                    var genresNodes = valueNodes[i].SelectNodes("ul/li");
                    videoInfo.Category = string.Join(",", genresNodes.Select(x => x.InnerText.Trim()));
                    break;
                case "Labels":
                    videoInfo.Series = valueNodes[i].InnerText.Trim();
                    break;
                case "Makers":
                    videoInfo.Producer = valueNodes[i].InnerText.Trim();
                    break;
                case "Volume":
                    videoInfo.Lengthtime = valueNodes[i].InnerText.Trim().Replace(" minutes", "分钟");
                    break;
            }
        }

        //演员
        var actressesNodes = htmlDoc.DocumentNode.SelectNodes("//div[@class='card actress']");

        if (actressesNodes != null)
        {
            videoInfo.Actor = string.Join(",", actressesNodes.Select(x => x.InnerText.Trim()));
        }

        //下载封面
        if (string.IsNullOrEmpty(imageUrl)) return videoInfo;

        var savePath = AppSettings.ImageSavePath;
        var filePath = Path.Combine(savePath, cid);
        videoInfo.ImageUrl = imageUrl;
        videoInfo.ImagePath = await GetInfoFromNetwork.DownloadFile(imageUrl, filePath, cid);

        return videoInfo;
    }
}
