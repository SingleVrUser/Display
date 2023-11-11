using Display.Helper;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Display.Data;
using SpiderInfo = Display.Models.SpiderInfo;
using System.Threading;

namespace Display.Spider;

public class JavBus
{
    public const int Id = (int)SpiderInfo.SpiderSourceName.Javbus;

    public const string Abbreviation = "bus";

    public const string Keywords = "JavBus";

    public static Tuple<int, int> DelayRanges = new(1, 3);


    public const bool IgnoreFc2 = true;

    public static bool IsOn => AppSettings.IsUseJavBus;

    private static string baseUrl => AppSettings.JavBusBaseUrl;

    public static async Task<VideoInfo> SearchInfoFromCID(string cid, CancellationToken token)
    {
        cid = cid.ToUpper();

        var spliteCid = cid.Split("-");
        if (spliteCid.Count() != 2) return null;

        string searchCID;

        switch (spliteCid[0])
        {
            case "MIUM" or "MAAN":
                searchCID = $"300{cid}";
                break;
            case "JAC":
                searchCID = $"390{cid}";
                break;
            case "DSVR":
                searchCID = $"3{cid}";
                break;
            default:
                searchCID = cid;
                break;
        }

        var tmp_url = GetInfoFromNetwork.UrlCombine(baseUrl, searchCID);

        var result = await RequestHelper.RequestHtml(Common.Client, tmp_url, token);
        if (result == null) return null;
            
        var detail_url = result.Item1;
        var htmlString = result.Item2;

        HtmlDocument htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(htmlString);

        return await GetVideoInfoFromHtmlDoc(cid, detail_url, htmlDoc);
    }

    public static async Task<VideoInfo> GetVideoInfoFromHtmlDoc(string CID, string detail_url, HtmlDocument htmlDoc)
    {
        //搜索封面
        var ImageUrlNode = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='col-md-9 screencap']//a//img");
        if (ImageUrlNode == null) return null;

        Uri Uri = new(detail_url);
        string JavBusUrl = $"{Uri.Scheme}://{Uri.Host}";

        var ImageUrl = ImageUrlNode.Attributes["src"].Value;
        if (!ImageUrl.Contains("http"))
        {
            ImageUrl = GetInfoFromNetwork.UrlCombine(JavBusUrl, ImageUrl);
        }

        VideoInfo videoInfo = new VideoInfo();
        videoInfo.busUrl = detail_url;
        videoInfo.trueName = CID;

        //标题
        var title = ImageUrlNode.Attributes["title"].Value;
        videoInfo.Title = title;

        //是否步兵
        var activeNavbarNode = htmlDoc.DocumentNode.SelectSingleNode("//div[@id='navbar']/ul[@class='nav navbar-nav']/li[@class='active']/a");
        if (activeNavbarNode != null)
        {
            switch (activeNavbarNode.InnerText)
            {
                case "有碼":
                    videoInfo.IsWm = 0;
                    break;
                case "無碼":
                    videoInfo.IsWm = 1;
                    break;
            }
        }
        var AttributeNodes = htmlDoc.DocumentNode.SelectNodes("//div[@class='col-md-3 info']//p");
        //信息
        for (var i = 0; i < AttributeNodes.Count; i++)
        {
            var AttributeNode = AttributeNodes[i];

            var header = AttributeNode.FirstChild.InnerText.Trim();

            if (header == "發行日期:")
            {
                videoInfo.ReleaseTime = AttributeNode.LastChild.InnerText.Trim();
            }
            else if (header == "長度:")
            {
                videoInfo.Lengthtime = AttributeNode.LastChild.InnerText.Trim().Replace("分鐘", "分钟");
            }
            else if (header == "導演:")
            {
                videoInfo.Director = AttributeNode.LastChild.InnerText.Trim();
            }
            else if (header == "製作商:")
            {
                videoInfo.Producer = AttributeNode.SelectSingleNode(".//a").InnerText.Trim();
            }
            else if (header == "發行商:")
            {
                videoInfo.Publisher = AttributeNode.SelectSingleNode(".//a").InnerText.Trim();
            }
            else if (header == "系列:")
            {
                videoInfo.Series = AttributeNode.SelectSingleNode(".//a").InnerText.Trim();
            }
            else if (header == "類別:")
            {
                var categoryNodes = AttributeNodes[i + 1].SelectNodes(".//span/label");
                List<string> categoryList = new List<string>();
                foreach (var node in categoryNodes)
                {
                    categoryList.Add(node.InnerText);
                }
                videoInfo.Category = string.Join(",", categoryList);
            }
            else if (header == "演員")
            {
                if (i >= AttributeNodes.Count - 1) continue;
                var actorNodes = AttributeNodes[i + 1].SelectNodes(".//span/a");
                List<string> actorList = new();
                foreach (var node in actorNodes)
                {
                    actorList.Add(node.InnerText);
                }
                videoInfo.Actor = string.Join(",", actorList);
            }
        }

        //下载封面
        if (!string.IsNullOrEmpty(ImageUrl))
        {
            string SavePath = AppSettings.ImageSavePath;

            string filePath = Path.Combine(SavePath, CID);
            videoInfo.ImageUrl = ImageUrl;
            videoInfo.ImagePath = await GetInfoFromNetwork.DownloadFile(ImageUrl, filePath, CID);
        }

        var sampleBox_Nodes = htmlDoc.DocumentNode.SelectNodes("//a[@class='sample-box']");
        List<string> sampleUrlList = new();
        if (sampleBox_Nodes != null)
        {
            foreach (var node in sampleBox_Nodes)
            {
                string sampleImageUrl = node.Attributes["href"].Value;
                if (!sampleImageUrl.Contains("http"))
                {
                    sampleImageUrl = GetInfoFromNetwork.UrlCombine(JavBusUrl, sampleImageUrl);
                }
                sampleUrlList.Add(sampleImageUrl);
            }
            videoInfo.SampleImageList = string.Join(",", sampleUrlList);
        }

        return videoInfo;
    }

}
