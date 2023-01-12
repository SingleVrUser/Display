using Data.Helper;
using HtmlAgilityPack;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media.Protection.PlayReady;
using Windows.Media.Streaming.Adaptive;
using static Data.Spider.Manager;

namespace Data.Spider;

public class LibreDmm
{
    public const int Id = (int)SpiderSourceName.libredmm;

    public const string Abbreviation = "libre";

    public const string Keywords = "LibreFanza";

    public static Tuple<int, int> DelayRanges = new(1, 2);

    public const bool IgnoreFc2 = true;

    public static bool IsTrue => AppSettings.isUseLibreDmm;

    private static string baseUrl => AppSettings.LibreDmm_BaseUrl;

    public static async Task<VideoInfo> SearchInfoFromCID(string CID)
    {
        CID = CID.ToUpper();
        string url = GetInfoFromNetwork.UrlCombine(baseUrl, $"movies/{CID}");

        Tuple<string, string> result = await RequestHelper.RequestHtml(Common.Client, url);
        if (result == null) return null;

        string detail_url = result.Item1;
        string htmlString = result.Item2;

        HtmlDocument htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(htmlString);

        return await GetVideoInfoFromHtmlDoc(CID, detail_url, htmlDoc);
    }

    public static async Task<VideoInfo> GetVideoInfoFromHtmlDoc(string CID, string detail_url, HtmlDocument htmlDoc)
    {
        //搜索封面
        string ImageUrl = null;
        var ImageUrlNode = htmlDoc.DocumentNode.SelectSingleNode("//img[@class='img-fluid']");
        if (ImageUrlNode != null)
        {
            ImageUrl = ImageUrlNode.Attributes["src"].Value;
        }

        VideoInfo videoInfo = new VideoInfo();
        videoInfo.busurl = detail_url;
        videoInfo.truename = CID;

        //dmm肯定没有步兵
        videoInfo.is_wm = 0;

        var titleNode = htmlDoc.DocumentNode.SelectSingleNode("//h1");
        if (titleNode != null)
            videoInfo.title = titleNode.InnerText.Trim();

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
                    videoInfo.releasetime = valueNodes[i].InnerText.Trim();
                    break;
                case "Directors":
                    videoInfo.director = valueNodes[i].InnerText.Trim();
                    break;
                case "Genres":
                    var generesNodes = valueNodes[i].SelectNodes("ul/li");
                    videoInfo.category = string.Join(",", generesNodes.Select(x => x.InnerText.Trim()));
                    break;
                case "Labels":
                    videoInfo.series = valueNodes[i].InnerText.Trim();
                    break;
                case "Makers":
                    videoInfo.producer = valueNodes[i].InnerText.Trim();
                    break;
                case "Volume":
                    videoInfo.lengthtime = valueNodes[i].InnerText.Trim().Replace(" minutes", "分钟");
                    break;
            }
        }

        //演员
        var actressesNodes = htmlDoc.DocumentNode.SelectNodes("//div[@class='card actress']");

        if (actressesNodes != null)
        {
            videoInfo.actor = string.Join(",", actressesNodes.Select(x => x.InnerText.Trim()));
        }

        //下载封面
        if (!string.IsNullOrEmpty(ImageUrl))
        {
            string SavePath = AppSettings.Image_SavePath;
            string filePath = Path.Combine(SavePath, CID);
            videoInfo.imageurl = ImageUrl;
            videoInfo.imagepath = await GetInfoFromNetwork.downloadFile(ImageUrl, filePath, CID);
        }

        return videoInfo;
    }
}
