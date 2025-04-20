using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DataAccess.Models.Entity;
using Display.Helper.Network;
using Display.Models.Dto.OneOneFive;
using Display.Models.Entities.OneOneFive;
using Display.Models.Spider;
using Display.Models.Vo;
using HttpClient = System.Net.Http.HttpClient;

namespace Display.Providers.Spider;

public class JavBus : BaseSpider
{
    public override SpiderSourceName Name => SpiderSourceName.Javbus;
    public override string Abbreviation => "bus";
    public override string Keywords => "JavBus";

    public override bool IsOn
    {
        get => AppSettings.IsUseJavBus;
        set => AppSettings.IsUseJavBus = value;
    }

    public override string BaseUrl
    {
        get => AppSettings.JavBusBaseUrl;
        set => AppSettings.JavBusBaseUrl = value;
    }

    private static HttpClient _client;
    public override HttpClient Client =>
        _client ??= NetworkHelper.CreateClient(
            new Dictionary<string, string>
            {
                { "accept-language", "zh-CN,zh;q=0.8,zh-TW;q=0.7,zh-HK;q=0.5,en-US;q=0.3,en;q=0.2" },
                { "user-agent", DbNetworkHelper.DownUserAgent }
            });

    public override async Task<VideoInfo> GetInfoByCid(string cid, CancellationToken token)
    {
        var splitCid = cid.Split("-");
        if (splitCid.Length != 2) return null;

        var searchCid = splitCid[0] switch
        {
            "MIUM" or "MAAN" => $"300{cid}",
            "JAC" => $"390{cid}",
            "DSVR" => $"3{cid}",
            _ => cid
        };

        var tmpUrl = NetworkHelper.UrlCombine(BaseUrl, searchCid);

        var result = await RequestHelper.RequestHtml(Client, tmpUrl, token);
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
        var imageUrlNode = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='col-md-9 screencap']//a//img");
        if (imageUrlNode == null) return null;

        Uri uri = new(detailUrl);
        var javBusUrl = $"{uri.Scheme}://{uri.Host}";

        var imageUrl = imageUrlNode.Attributes["src"].Value;
        if (!imageUrl.Contains("http"))
        {
            imageUrl = NetworkHelper.UrlCombine(javBusUrl, imageUrl);
        }

        var videoInfo = new VideoInfo
        {
            Url = detailUrl,
            TrueName = cid
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

            switch (header)
            {
                case "發行日期:":
                    videoInfo.ReleaseTime = attributeNode.LastChild.InnerText.Trim();
                    break;
                case "長度:":
                    videoInfo.LengthTime = attributeNode.LastChild.InnerText.Trim().Replace("分鐘", "分钟");
                    break;
                case "導演:":
                    videoInfo.Director = attributeNode.LastChild.InnerText.Trim();
                    break;
                case "製作商:":
                    videoInfo.Producer = attributeNode.SelectSingleNode(".//a").InnerText.Trim();
                    break;
                case "發行商:":
                    videoInfo.Publisher = attributeNode.SelectSingleNode(".//a").InnerText.Trim();
                    break;
                case "系列:":
                    videoInfo.Series = attributeNode.SelectSingleNode(".//a").InnerText.Trim();
                    break;
                case "類別:":
                    {
                        var categoryNodes = attributeNodes[i + 1].SelectNodes(".//span/label");
                        var categoryList = categoryNodes.Select(node => node.InnerText).ToList();
                        videoInfo.Category = string.Join(",", categoryList);
                        break;
                    }
                case "演員" when i >= attributeNodes.Count - 1:
                    continue;
                case "演員":
                    {
                        var actorNodes = attributeNodes[i + 1].SelectNodes(".//span/a");
                        var actorList = actorNodes.Select(node => node.InnerText).ToList();
                        videoInfo.Actor = string.Join(",", actorList);
                        break;
                    }
            }
        }

        //下载封面
        if (!string.IsNullOrEmpty(imageUrl))
        {
            var savePath = AppSettings.ImageSavePath;

            var filePath = Path.Combine(savePath, cid);
            videoInfo.ImageUrl = imageUrl;
            videoInfo.ImagePath = await DbNetworkHelper.DownloadFile(imageUrl, filePath, cid);
        }

        var sampleBoxNodes = htmlDoc.DocumentNode.SelectNodes("//a[@class='sample-box']");
        List<string> sampleUrlList = [];
        if (sampleBoxNodes == null) return videoInfo;

        foreach (var node in sampleBoxNodes)
        {
            var sampleImageUrl = node.Attributes["href"].Value;
            if (!sampleImageUrl.Contains("http"))
            {
                sampleImageUrl = NetworkHelper.UrlCombine(javBusUrl, sampleImageUrl);
            }
            sampleUrlList.Add(sampleImageUrl);
        }
        videoInfo.SampleImageList = string.Join(",", sampleUrlList);

        return videoInfo;
    }

}
