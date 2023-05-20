using Display.Helper;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Display.Data;
using SpiderInfo = Display.Models.SpiderInfo;

namespace Display.Spider;

public class JavDB
{
    public const int Id = (int)SpiderInfo.SpiderSourceName.Javdb;

    public const string Abbreviation = "db";

    public const string Keywords = "JavDB";

    public static Tuple<int, int> DelayRanges = new(3, 6);

    public static readonly char manSymbol = '♂';

    public static readonly char womanSymbol = '♀';

    public const bool IgnoreFc2 = false;

    public static bool IsOn => AppSettings.IsUseJavDb;

    private static string baseUrl => AppSettings.JavDB_BaseUrl;

    private static HttpClient _clientWithJavDBCookie;
    private static HttpClient ClientWithCookie
    {
        get
        {
            if (_clientWithJavDBCookie == null)
            {
                _clientWithJavDBCookie = GetInfoFromNetwork.CreateClient(new Dictionary<string, string>() {
                        {"cookie",AppSettings.javdb_Cookie },
                        {"user-agent" ,GetInfoFromNetwork.BrowserUserAgent}
                    });
            }

            return _clientWithJavDBCookie;
        }

        set => _clientWithJavDBCookie = value;
    }

    public static async Task<VideoInfo> SearchInfoFromCID(string CID)
    {
        var isUseCookie = CID.Contains("FC");

        if (isUseCookie && string.IsNullOrEmpty(AppSettings.javdb_Cookie)) return null;

        var detailUrl = await GetDetailUrlFromCid(CID);

        //搜索无果，退出
        if (detailUrl == null) return null;

        Tuple<string, string> tuple;
        //访问fc内容需要cookie
        if (isUseCookie)
            tuple = await RequestHelper.RequestHtml(ClientWithCookie, detailUrl);
        else
            tuple = await RequestHelper.RequestHtml(Common.Client, detailUrl);

        var htmlString = tuple.Item2;
        if (string.IsNullOrEmpty(htmlString)) return null;

        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(htmlString);

        return await GetVideoInfoFromHtmlDoc(CID, detailUrl, htmlDoc);
    }

    private static async Task<string> GetDetailUrlFromCid(string CID)
    {
        var url = GetInfoFromNetwork.UrlCombine(baseUrl, $"search?q={CID}&f=all");

        // 访问
        var tuple = await RequestHelper.RequestHtml(Common.Client, url);
        if (tuple == null) return null;

        var strResult = tuple.Item2;
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(strResult);

        var result = GetDetailUrlFromSearchResult(htmlDoc, CID);

        return result;
    }

    private static string GetDetailUrlFromSearchResult(HtmlDocument htmlDoc, string CID)
    {
        string result = null;

        var searchResultNodes = htmlDoc.DocumentNode.SelectNodes("//div[contains(@class,'movie-list')]");

        //搜索无果，退出
        if (searchResultNodes == null) return null;

        //分割通过正则匹配得到的CID
        var splitResult = Common.SplitCid(CID);
        if (splitResult == null) return null;

        var leftCid = splitResult.Item1;
        var rightCid = splitResult.Item2;

        for (var i = 0; i < searchResultNodes.Count; i++)
        {
            var searchLeftCid = string.Empty;
            var searchRightCid = string.Empty;

            var movieList = searchResultNodes[i];
            var titleSearch = movieList.SelectSingleNode(".//div[@class='video-title']/strong").InnerText;
            var title = titleSearch.ToUpper();

            var split_result = title.Split(new char[] { '-', '_' });
            if (split_result.Length == 1)
            {
                var match_result = Regex.Match(title, @"([A-Z]+)(\d+)");
                if (match_result.Success)
                {
                    searchLeftCid = match_result.Groups[1].Value;
                    searchRightCid = match_result.Groups[2].Value;
                }
            }
            else if (split_result.Length == 2)
            {
                searchLeftCid = split_result[0];
                searchRightCid = split_result[1];
            }
            else if (split_result.Length == 3)
            {
                if (title.Contains("HEYDOUGA"))
                {
                    searchLeftCid = split_result[1];
                    searchRightCid = split_result[2];
                }
                else
                    return null;
            }
            else
                return null;

            if (searchLeftCid == leftCid
                && (searchRightCid == rightCid
                    || (Int32.TryParse(rightCid, out var currentNum)
                        && Int32.TryParse(searchRightCid, out var searchNum)
                        && currentNum.Equals(searchNum))))
            {
                var detailUrl = searchResultNodes[i].SelectSingleNode(".//a").Attributes["href"].Value;
                detailUrl = GetInfoFromNetwork.UrlCombine(AppSettings.JavDB_BaseUrl, detailUrl);
                result = detailUrl;
                break;
            }
            else
            {

            }
        }

        return result;
    }

    public static async Task<VideoInfo> GetVideoInfoFromHtmlDoc(string CID, string detail_url, HtmlDocument htmlDoc)
    {
        // TODO之前已经访问过一次了，查看是否需要用Cookie再访问一次（Fc2）

        //bool isUseCookie = CID.Contains("FC");

        //if (isUseCookie && string.IsNullOrEmpty(AppSettings.javdb_Cookie)) return null;

        ////访问fc内容需要cookie
        //Tuple<string, string> tuple;
        //if (isUseCookie)
        //    tuple = await RequestHelper.RequestHtml(ClientWithCookie, detail_url);
        //else
        //    tuple = await RequestHelper.RequestHtml(Common.Client, detail_url);

        //string strResult = tuple.Item2;
        //if (string.IsNullOrEmpty(strResult)) return null;

        var video_meta_panelNode = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='video-meta-panel']");

        //检查是不是没有登录
        if (video_meta_panelNode == null)
        {
            var headingNode = htmlDoc.DocumentNode.SelectSingleNode("//p[@class='panel-heading']");
            //没有登录
            if (headingNode != null && headingNode.InnerText.Contains("登入"))
            {
                var tuple = await RequestHelper.RequestHtml(ClientWithCookie, detail_url);
                string htmlString = tuple.Item2;

                htmlDoc.LoadHtml(htmlString);

                //再次检查信息 meta_panel
                video_meta_panelNode = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='video-meta-panel']");

                //依旧不能用就退出
                if (video_meta_panelNode == null) return null;
            }
            else
                return null;
        }

        VideoInfo videoInfo = new VideoInfo();

        videoInfo.busurl = detail_url;

        Uri Uri = new(detail_url);
        string JavDBUrl = $"{Uri.Scheme}://{Uri.Host}";

        var ImageUrl = video_meta_panelNode.SelectSingleNode(".//img[@class='video-cover']").Attributes["src"].Value;

        if (!ImageUrl.Contains("http"))
        {
            ImageUrl = GetInfoFromNetwork.UrlCombine(JavDBUrl, ImageUrl);
        }

        var AttributeNodes = video_meta_panelNode.SelectNodes(".//div[contains(@class,'panel-block')]");

        videoInfo.truename = CID;
        //信息
        for (var i = 0; i < AttributeNodes.Count; i++)
        {
            var keyNode = AttributeNodes[i].SelectSingleNode("strong");
            if (keyNode == null) continue;
            string key = keyNode.InnerText;

            var valueNode = AttributeNodes[i].SelectSingleNode("span");

            ////以网页的CID为准
            //if (key.Contains("番號"))
            //{
            //    videoInfo.truename = valueNode.InnerText;
            //    CID = videoInfo.truename;
            //}
            if (key.Contains("日期"))
            {
                videoInfo.releasetime = valueNode.InnerText;
            }
            else if (key.Contains("時長"))
            {
                videoInfo.lengthtime = valueNode.InnerText.Trim().Replace(" 分鍾", "分钟");
            }
            else if (key.Contains("片商") || key.Contains("賣家"))
            {
                videoInfo.producer = valueNode.InnerText;
            }
            else if (key.Contains("發行"))
            {
                videoInfo.publisher = valueNode.InnerText;
            }
            else if (key.Contains("系列"))
            {
                videoInfo.series = valueNode.InnerText;
            }
            else if (key.Contains("類別"))
            {
                var categoryNodes = valueNode.SelectNodes("a");
                List<string> categoryList = new List<string>();
                foreach (var node in categoryNodes)
                {
                    categoryList.Add(node.InnerText);
                }
                videoInfo.category = string.Join(",", categoryList);
            }
            else if (key.Contains("演員"))
            {
                var actorNodes = valueNode.SelectNodes("a");
                var genderNodes = valueNode.SelectNodes("strong");

                if (actorNodes == null) continue;
                List<string> actorList = new List<string>();


                for (int j = 0; j < actorNodes.Count; j++)
                {
                    var actorNode = actorNodes[j];

                    //♀ or ♂
                    var genderNode = genderNodes[j];


                    actorList.Add($"{actorNode.InnerText.Trim()}{genderNode.InnerText}");
                }
                videoInfo.actor = string.Join(",", actorList);
            }
        }

        //标题
        var TitleNode = htmlDoc.DocumentNode.SelectSingleNode(".//strong[@class='current-title']");
        var title = TitleNode.InnerText;
        videoInfo.title = title.Replace(videoInfo.truename, "").Trim();

        //下载封面
        string SavePath = AppSettings.ImageSavePath;
        string filePath = Path.Combine(SavePath, CID);
        videoInfo.imageurl = ImageUrl;
        videoInfo.imagepath = await GetInfoFromNetwork.downloadFile(ImageUrl, filePath, CID);

        //样品图片
        var preview_imagesSingesNode = htmlDoc.DocumentNode.SelectSingleNode("//div[contains(@class,'preview-images')]");
        if (preview_imagesSingesNode != null)
        {
            var preview_imagesNodes = preview_imagesSingesNode.SelectNodes(".//a[@class='tile-item']");
            List<string> sampleUrlList = new();
            if (preview_imagesNodes != null)
            {
                foreach (var node in preview_imagesNodes)
                {
                    var sampleImageUrl = node.Attributes["href"].Value;
                    if (!sampleImageUrl.Contains("http"))
                    {
                        sampleImageUrl = GetInfoFromNetwork.UrlCombine(JavDBUrl, sampleImageUrl);
                    }
                    sampleUrlList.Add(sampleImageUrl);
                }
                videoInfo.sampleImageList = string.Join(",", sampleUrlList);
            }
        }

        return videoInfo;
    }

    public static string TrimGenderFromActorName(string actorName)
    {
        return actorName.TrimEnd(new char[] { manSymbol, womanSymbol });
    }

    public static string RemoveGenderFromActorListString(string actorListString)
    {
        string result = actorListString;

        if (result.Contains(manSymbol))
        {
            result = result.Replace(manSymbol, char.MinValue);
        }
        else if (result.Contains(womanSymbol))
        {
            result = result.Replace(womanSymbol, char.MinValue);
        }

        return result;
    }
}
