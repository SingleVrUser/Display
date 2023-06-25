using Display.Helper;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Display.Data;
using SpiderInfo = Display.Models.SpiderInfo;

namespace Display.Spider;

public class Fc2hub
{
    public const int Id = (int)SpiderInfo.SpiderSourceName.Fc2club;

    public const string Abbreviation = "fc";

    public const string Keywords = "Fc2hub.com";

    public static Tuple<int, int> DelayRanges = new(1, 2);

    public const bool OnlyFc2 = true;

    public const bool IgnoreFc2 = false;

    public static bool IsOn => AppSettings.IsUseFc2Hub;

    private static string baseUrl => AppSettings.Fc2HubBaseUrl;

    public static async Task<VideoInfo> SearchInfoFromCID(string CID)
    {
        string url = GetInfoFromNetwork.UrlCombine(baseUrl, $"search?kw={CID.Replace("FC2-", "")}");

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
        VideoInfo videoInfo = new VideoInfo();
        videoInfo.busurl = detail_url;
        //默认是步兵
        videoInfo.is_wm = 1;

        var jsons = htmlDoc.DocumentNode.SelectNodes("//script[@type='application/ld+json']");

        if (jsons == null || jsons.Count == 0) return null;

        var jsonString = jsons.Last().InnerText;

        var json = JsonConvert.DeserializeObject<FcJson>(jsonString);

        if (json.name == null || json.image == null) return null;

        videoInfo.title = json.name;
        videoInfo.truename = CID;
        videoInfo.releasetime = json.datePublished.Replace("/", "-");
        //PTxHxMxS转x分钟
        videoInfo.lengthtime = Data.FileMatch.ConvertPtTimeToTotalMinute(json.duration);
        videoInfo.director = json.director;
        videoInfo.producer = "fc2";

        if (json.genre != null)
            videoInfo.category = string.Join(",", json.genre);

        if (json.actor != null)
            videoInfo.actor = string.Join(",", json.actor);


        string ImageUrl = string.Empty;
        if (json.image != null)
        {
            ImageUrl = json.image;
        }
        else
        {
            var imageNode = htmlDoc.DocumentNode.SelectSingleNode("//meta[@property='og:image']");

            if (imageNode != null)
            {
                ImageUrl = imageNode.GetAttributeValue("content", string.Empty);
            }
        }

        ////下载封面
        if (!string.IsNullOrEmpty(ImageUrl))
        {
            string SavePath = AppSettings.ImageSavePath;
            string filePath = Path.Combine(SavePath, CID);
            videoInfo.imageurl = ImageUrl;
            videoInfo.imagepath = await GetInfoFromNetwork.downloadFile(ImageUrl, filePath, CID);
        }

        return videoInfo;
    }
}
