using Display.Helper;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Display.Data;
using SpiderInfo = Display.Models.SpiderInfo;

namespace Display.Spider;

public class Jav321
{
    public const int Id = (int)SpiderInfo.SpiderSourceName.Jav321;

    public const string Abbreviation = "Jav321";

    public const string Keywords = "bittorrent Download dmm";

    public static Tuple<int, int> DelayRanges = new(1, 2);

    public const bool IgnoreFc2 = true;

    public static bool IsOn => AppSettings.IsUseJav321;

    private static string baseUrl => AppSettings.Jav321BaseUrl;

    public static async Task<VideoInfo> SearchInfoFromCID(string CID)
    {
        var searchUrl = GetInfoFromNetwork.UrlCombine(baseUrl, "search");

        CID = CID.ToUpper();

        var postValues = new Dictionary<string, string>
        {
            { "sn", CID}
        };

        var result = await RequestHelper.PostHtml(Common.Client, searchUrl, postValues);
        if (result == null) return null;

        var detailUrl = result.Item1;
        var htmlString = result.Item2;

        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(htmlString);

        return await GetVideoInfoFromHtmlDoc(CID, detailUrl, htmlDoc);
    }

    public static async Task<VideoInfo> GetVideoInfoFromHtmlDoc(string cid, string detailUrl, HtmlDocument htmlDoc)
    {
        //标题
        var titleNode = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='panel-heading']/h3");
        if (titleNode == null) return null;

        var videoInfo = new VideoInfo
        {
            busurl = detailUrl,
            title = titleNode.FirstChild.InnerText
        };

        //图片地址
        var imageNodes = htmlDoc.DocumentNode.SelectNodes("/html/body/div[@class='row'][2]/div[@class='col-md-3']/div//img[@class='img-responsive']");
        string ImageUrl = string.Empty;
        var sampleUrlList = new List<string>();

        //没有样图
        if (imageNodes == null)
        {
            var imageNode = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='panel-body'][1]/div[@class='row'][1]/div[@class='col-md-3'][1]/img[@class='img-responsive']");
            if (imageNode == null) return null;

            var imgSrc = imageNode.GetAttributeValue("src", string.Empty);

            ImageUrl = imgSrc;
        }
        //有样图
        else
        {
            //第一张为封面
            //其余为缩略图
            for (var i = 0; i < imageNodes.Count; i++)
            {
                var imageNode = imageNodes[i];
                string imageUrl = imageNode.GetAttributeValue("src", string.Empty);

                //if (imageUrl.EndsWith("webp")) continue;

                if (i == 0)
                {
                    ImageUrl = imageUrl;
                }
                else
                {
                    sampleUrlList.Add(imageUrl);
                }
            }
        }

        //CID
        videoInfo.truename = cid;

        //其他信息
        var InfoNode = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='col-md-9']");

        //解析
        Dictionary<string, string> infos = new Dictionary<string, string>();
        string key = null;
        List<string> values = new List<string>();
        foreach (var info in InfoNode.ChildNodes)
        {
            string name = info.Name;

            string innerText = info.InnerText.Trim();

            //key(b)
            if (name == "b")
            {
                //前一key已检索完成
                if (values.Count != 0)
                {
                    infos[key] = string.Join(",", values);

                    //Debug.WriteLine($"添加{infos[key]}");
                    values.Clear();
                }

                key = innerText;

            }
            //value（可跳转的a）
            else if (name == "a")
            {
                values.Add(innerText);
            }
            //value（不可跳转的#text）
            else if (name == "#text" && innerText.Contains(":"))
            {
                string value = innerText.Replace(":", string.Empty);

                //每个演员名之间是用“&nbsp;”分割
                if (value.Contains("&nbsp;"))
                    value = value.Replace(" &nbsp; ", ",").Replace(" &nbsp;", "");

                if (!string.IsNullOrEmpty(value))
                    values.Add(value);
            }
        }
        //最后的添加
        infos[key] = string.Join(",", values);
        values.Clear();

        //添加进VideoInfo
        foreach (var info in infos)
        {
            switch (info.Key)
            {
                case "出演者":
                    videoInfo.actor = info.Value.Trim();
                    break;
                case "メーカー":
                    videoInfo.producer = info.Value.Trim();
                    break;
                case "シリーズ":
                    videoInfo.series = info.Value.Trim();
                    break;
                case "ジャンル":
                    videoInfo.category = info.Value.Trim();
                    break;
                case "配信開始日":
                    videoInfo.releasetime = info.Value.Trim();
                    break;
                case "収録時間":
                    videoInfo.lengthtime = info.Value.Trim().Replace(" minutes", "分钟");
                    break;
            }
        }

        if (sampleUrlList != null)
        {
            videoInfo.sampleImageList = string.Join(",", sampleUrlList);
        }

        ////下载封面
        if (!string.IsNullOrEmpty(ImageUrl))
        {
            string SavePath = AppSettings.ImageSavePath;
            string filePath = Path.Combine(SavePath, cid);
            videoInfo.imageurl = ImageUrl;
            videoInfo.imagepath = await GetInfoFromNetwork.downloadFile(ImageUrl, filePath, cid);
        }
        //（接受无封面）

        return videoInfo;
    }
}
