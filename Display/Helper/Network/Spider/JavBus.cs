using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SpiderInfo = Display.Models.Spider.SpiderInfos;
using System.Threading;
using Display.Helper.Network;
using Display.Models.Data;
using Display.Models.Spider;

namespace Display.Helper.Network.Spider;

public class JavBus
{
    public const int Id = (int)SpiderInfo.SpiderSourceName.Javbus;

    public const string Abbreviation = "bus";

    public const string Keywords = "JavBus";

    public static Tuple<int, int> DelayRanges = new(1, 3);

    public const bool IgnoreFc2 = true;

    public static bool IsOn => AppSettings.IsUseJavBus;

    private static string BaseUrl => AppSettings.JavBusBaseUrl;

    public static async Task<VideoInfo> SearchInfoFromCid(string cid, CancellationToken token)
    {
        cid = cid.ToUpper();

        var splitCid = cid.Split("-");
        if (splitCid.Length != 2) return null;

        string searchCid;

        switch (splitCid[0])
        {
            case "MIUM" or "MAAN":
                searchCid = $"300{cid}";
                break;
            case "JAC":
                searchCid = $"390{cid}";
                break;
            case "DSVR":
                searchCid = $"3{cid}";
                break;
            default:
                searchCid = cid;
                break;
        }

        var tmpUrl = GetInfoFromNetwork.UrlCombine(BaseUrl, searchCid);

        var result = await RequestHelper.RequestHtml(Common.Client, tmpUrl, token);
        if (result == null) return null;

        var detailUrl = result.Item1;
        var htmlString = result.Item2;

        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(htmlString);

        return await GetVideoInfoFromHtmlDoc(cid, detailUrl, htmlDoc);
    }

    public static async Task<VideoInfo> GetVideoInfoFromHtmlDoc(string cid, string detailUrl, HtmlDocument htmlDoc)
    {
        //搜索封面
        var imageUrlNode = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='col-md-9 screencap']//a//img");
        if (imageUrlNode == null) return null;

        Uri uri = new(detailUrl);
        var javBusUrl = $"{uri.Scheme}://{uri.Host}";

        var imageUrl = imageUrlNode.Attributes["src"].Value;
        if (!imageUrl.Contains("http"))
        {
            imageUrl = GetInfoFromNetwork.UrlCombine(javBusUrl, imageUrl);
        }

        var videoInfo = new VideoInfo
        {
            busUrl = detailUrl,
            trueName = cid
        };

        //标题
        var title = imageUrlNode.Attributes["title"].Value;
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
        var attributeNodes = htmlDoc.DocumentNode.SelectNodes("//div[@class='col-md-3 info']//p");
        //信息
        for (var i = 0; i < attributeNodes.Count; i++)
        {
            var attributeNode = attributeNodes[i];

            var header = attributeNode.FirstChild.InnerText.Trim();

            if (header == "發行日期:")
            {
                videoInfo.ReleaseTime = attributeNode.LastChild.InnerText.Trim();
            }
            else if (header == "長度:")
            {
                videoInfo.Lengthtime = attributeNode.LastChild.InnerText.Trim().Replace("分鐘", "分钟");
            }
            else if (header == "導演:")
            {
                videoInfo.Director = attributeNode.LastChild.InnerText.Trim();
            }
            else if (header == "製作商:")
            {
                videoInfo.Producer = attributeNode.SelectSingleNode(".//a").InnerText.Trim();
            }
            else if (header == "發行商:")
            {
                videoInfo.Publisher = attributeNode.SelectSingleNode(".//a").InnerText.Trim();
            }
            else if (header == "系列:")
            {
                videoInfo.Series = attributeNode.SelectSingleNode(".//a").InnerText.Trim();
            }
            else if (header == "類別:")
            {
                var categoryNodes = attributeNodes[i + 1].SelectNodes(".//span/label");
                List<string> categoryList = new List<string>();
                foreach (var node in categoryNodes)
                {
                    categoryList.Add(node.InnerText);
                }
                videoInfo.Category = string.Join(",", categoryList);
            }
            else if (header == "演員")
            {
                if (i >= attributeNodes.Count - 1) continue;
                var actorNodes = attributeNodes[i + 1].SelectNodes(".//span/a");
                List<string> actorList = new();
                foreach (var node in actorNodes)
                {
                    actorList.Add(node.InnerText);
                }
                videoInfo.Actor = string.Join(",", actorList);
            }
        }

        //下载封面
        if (!string.IsNullOrEmpty(imageUrl))
        {
            string SavePath = AppSettings.ImageSavePath;

            string filePath = Path.Combine(SavePath, cid);
            videoInfo.ImageUrl = imageUrl;
            videoInfo.ImagePath = await GetInfoFromNetwork.DownloadFile(imageUrl, filePath, cid);
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
                    sampleImageUrl = GetInfoFromNetwork.UrlCombine(javBusUrl, sampleImageUrl);
                }
                sampleUrlList.Add(sampleImageUrl);
            }
            videoInfo.SampleImageList = string.Join(",", sampleUrlList);
        }

        return videoInfo;
    }

}
