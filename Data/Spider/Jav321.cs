using Data.Helper;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using static Data.Model.SpiderInfo;

namespace Data.Spider;

public class Jav321
{
    public const int Id = (int)SpiderSourceName.Jav321;

    public const string Abbreviation = "Jav321";

    public const string Keywords = "bittorrent Download dmm";

    public static Tuple<int, int> DelayRanges = new(1, 2);


    public const bool IgnoreFc2 = true;

    public static bool IsTrue => AppSettings.isUseJav321;

    private static string baseUrl => AppSettings.Jav321_BaseUrl;

    public static async Task<VideoInfo> SearchInfoFromCID(string CID)
    {
        string SearchUrl = GetInfoFromNetwork.UrlCombine(baseUrl, "search");

        CID = CID.ToUpper();

        var postValues = new Dictionary<string, string>
        {
            { "sn", CID}
        };

        Tuple<string, string> result = await RequestHelper.PostHtml(Common.Client, SearchUrl, postValues);
        if (result == null) return null;

        string detail_url = result.Item1;
        string htmlString = result.Item2;

        HtmlDocument htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(htmlString);

        return await GetVideoInfoFromHtmlDoc(CID, detail_url, htmlDoc);
    }

    public static async Task<VideoInfo> GetVideoInfoFromHtmlDoc(string CID, string detail_url, HtmlDocument htmlDoc)
    {
        //标题
        var TitleNode = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='panel-heading']/h3");
        if (TitleNode == null) return null;

        VideoInfo videoInfo = new VideoInfo();
        videoInfo.busurl = detail_url;

        videoInfo.title = TitleNode.FirstChild.InnerText;

        //图片地址
        var ImageNodes = htmlDoc.DocumentNode.SelectNodes("/html/body/div[@class='row'][2]/div[@class='col-md-3']/div//img[@class='img-responsive']");
        string ImageUrl = string.Empty;
        List<string> sampleUrlList = new List<string>();

        //没有样图
        if (ImageNodes == null)
        {
            var ImageNode = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='panel-body'][1]/div[@class='row'][1]/div[@class='col-md-3'][1]/img[@class='img-responsive']");
            if (ImageNode == null) return null;

            string imgSrc = ImageNode.GetAttributeValue("src", string.Empty);

            ImageUrl = imgSrc;
        }
        //有样图
        else
        {
            //第一张为封面
            //其余为缩略图
            for (int i = 0; i < ImageNodes.Count; i++)
            {
                var imageNode = ImageNodes[i];
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
        videoInfo.truename = CID;

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
            string SavePath = AppSettings.Image_SavePath;
            string filePath = Path.Combine(SavePath, CID);
            videoInfo.imageurl = ImageUrl;
            videoInfo.imagepath = await GetInfoFromNetwork.downloadFile(ImageUrl, filePath, CID);
        }
        //（接受无封面）

        return videoInfo;
    }
}
