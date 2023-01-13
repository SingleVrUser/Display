using Data.Helper;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media.Protection.PlayReady;

namespace Data.Spider;

public class JavBus
{
    public const int Id = (int)Manager.SpiderSourceName.javbus;

    public const string Abbreviation = "bus";

    public const string Keywords = "JavBus";

    public static Tuple<int, int> DelayRanges = new(1,3);


    public const bool IgnoreFc2 = true;

    public static bool IsTrue => AppSettings.isUseJavBus; 

    private static string baseUrl => AppSettings.JavBus_BaseUrl;

    public static async Task<VideoInfo> SearchInfoFromCID(string CID)
    {
        CID = CID.ToUpper();

        var spliteCid = CID.Split("-");
        if (spliteCid.Count() != 2) return null;

        string searchCID;

        switch (spliteCid[0])
        {
            case "MIUM" or "MAAN":
                searchCID = $"300{CID}";
                break;
            case "JAC":
                searchCID = $"390{CID}";
                break;
            case "DSVR":
                searchCID = $"3{CID}";
                break;
            default:
                searchCID = CID;
                break;
        }

        string tmp_url = GetInfoFromNetwork.UrlCombine(baseUrl, searchCID);

        Tuple<string, string> result = await RequestHelper.RequestHtml(Common.Client, tmp_url);
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
        videoInfo.busurl = detail_url;
        videoInfo.truename = CID;

        //标题
        var title = ImageUrlNode.Attributes["title"].Value;
        videoInfo.title = title;

        //是否步兵
        var activeNavbarNode = htmlDoc.DocumentNode.SelectSingleNode("//div[@id='navbar']/ul[@class='nav navbar-nav']/li[@class='active']/a");
        if (activeNavbarNode != null)
        {
            switch (activeNavbarNode.InnerText)
            {
                case "有碼":
                    videoInfo.is_wm = 0;
                    break;
                case "無碼":
                    videoInfo.is_wm = 1;
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
                videoInfo.releasetime = AttributeNode.LastChild.InnerText.Trim();
            }
            else if (header == "長度:")
            {
                videoInfo.lengthtime = AttributeNode.LastChild.InnerText.Trim().Replace("分鐘", "分钟");
            }
            else if (header == "導演:")
            {
                videoInfo.director = AttributeNode.LastChild.InnerText.Trim();
            }
            else if (header == "製作商:")
            {
                videoInfo.producer = AttributeNode.SelectSingleNode(".//a").InnerText.Trim();
            }
            else if (header == "發行商:")
            {
                videoInfo.publisher = AttributeNode.SelectSingleNode(".//a").InnerText.Trim();
            }
            else if (header == "系列:")
            {
                videoInfo.series = AttributeNode.SelectSingleNode(".//a").InnerText.Trim();
            }
            else if (header == "類別:")
            {
                var categoryNodes = AttributeNodes[i + 1].SelectNodes(".//span/label");
                List<string> categoryList = new List<string>();
                foreach (var node in categoryNodes)
                {
                    categoryList.Add(node.InnerText);
                }
                videoInfo.category = string.Join(",", categoryList);
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
                videoInfo.actor = string.Join(",", actorList);
            }
        }

        //下载封面
        if (!string.IsNullOrEmpty(ImageUrl))
        {
            string SavePath = AppSettings.Image_SavePath;

            string filePath = Path.Combine(SavePath, CID);
            videoInfo.imageurl = ImageUrl;
            videoInfo.imagepath = await GetInfoFromNetwork.downloadFile(ImageUrl, filePath, CID);
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
            videoInfo.sampleImageList = string.Join(",", sampleUrlList);
        }

        return videoInfo;
    }
}
