using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Display.Models.Data;
using static Display.Models.Spider.SpiderInfos;
using Display.Helper.Network;

namespace Display.Providers.Spider;

public class JavDb : BaseSpider
{
    public override string Abbreviation => "db";
    public override string Keywords => "JavDB";
    public override SpiderSourceName Name => SpiderSourceName.Javdb;
    public override bool IsOn
    {
        get => AppSettings.IsUseJavDb;
        set => AppSettings.IsUseJavDb = value;
    }

    public override bool IsCookieEnable => true;

    public override string BaseUrl
    {
        get => AppSettings.JavDbBaseUrl;
        set => AppSettings.JavDbBaseUrl = value;
    }

    public override Tuple<int, int> DelayRanges => new(3, 6);
    public override bool IgnoreFc2 => false;
    public static readonly char ManSymbol = '♂';
    public static readonly char WomanSymbol = '♀';

    public override string Cookie
    {
        get => AppSettings.JavDbCookie;
        set
        {
            AppSettings.JavDbCookie = value;
            _client = CreateClient(value);
        }
    }

    private static HttpClient _client;

    public override HttpClient Client =>
        _client ??= CreateClient(Cookie);


    private static HttpClient CreateClient(string cookie)
    {
        return GetInfoFromNetwork.CreateClient(
            new Dictionary<string, string>
            {
                { "cookie", cookie },
                { "user-agent", GetInfoFromNetwork.DownUserAgent }
            });
    }

    public override async Task<VideoInfo> GetInfoByCid(string cid, CancellationToken token)
    {
        cid = cid.ToUpper();
        var isUseCookie = cid.Contains("FC");

        if (isUseCookie && string.IsNullOrEmpty(AppSettings.JavDbCookie)) return null;

        var detailUrl = await GetDetailUrlFromCid(cid, token);

        //搜索无果，退出
        if (detailUrl == null) return null;

        Tuple<string, string> tuple;
        //访问fc内容需要cookie
        if (isUseCookie)
            tuple = await RequestHelper.RequestHtml(Client, detailUrl, token);
        else
            tuple = await RequestHelper.RequestHtml(Common.Client, detailUrl, token);

        if (tuple == null) return null;

        var htmlString = tuple.Item2;
        if (string.IsNullOrEmpty(htmlString)) return null;

        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(htmlString);

        return await GetInfoByHtmlDoc(cid, detailUrl, htmlDoc);
    }

    public override async Task<VideoInfo> GetInfoByHtmlDoc(string cid, string detailUrl, HtmlDocument htmlDoc)
    {
        var videoMetaPanelNode = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='video-meta-panel']");

        //检查是不是没有登录
        if (videoMetaPanelNode == null)
        {
            var headingNode = htmlDoc.DocumentNode.SelectSingleNode("//p[@class='panel-heading']");
            //没有登录
            if (headingNode != null && headingNode.InnerText.Contains("登入"))
            {
                var tuple = await RequestHelper.RequestHtml(Client, detailUrl, default);
                var htmlString = tuple.Item2;

                htmlDoc.LoadHtml(htmlString);

                //再次检查信息 meta_panel
                videoMetaPanelNode = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='video-meta-panel']");

                //依旧不能用就退出
                if (videoMetaPanelNode == null) return null;
            }
            else
                return null;
        }

        var videoInfo = new VideoInfo
        {
            busUrl = detailUrl
        };

        Uri uri = new(detailUrl);
        var javDbUrl = $"{uri.Scheme}://{uri.Host}";

        var imageUrl = videoMetaPanelNode.SelectSingleNode(".//img[@class='video-cover']").Attributes["src"].Value;

        if (!imageUrl.Contains("http"))
        {
            imageUrl = GetInfoFromNetwork.UrlCombine(javDbUrl, imageUrl);
        }

        var attributeNodes = videoMetaPanelNode.SelectNodes(".//div[contains(@class,'panel-block')]");

        videoInfo.trueName = cid;
        //信息
        foreach (var currentNode in attributeNodes)
        {
            var keyNode = currentNode.SelectSingleNode("strong");
            if (keyNode == null) continue;
            var key = keyNode.InnerText;

            var valueNode = currentNode.SelectSingleNode("span");

            if (key.Contains("日期"))
            {
                videoInfo.ReleaseTime = valueNode.InnerText;
            }
            else if (key.Contains("時長"))
            {
                videoInfo.Lengthtime = valueNode.InnerText.Trim().Replace(" 分鍾", "分钟");
            }
            else if (key.Contains("片商") || key.Contains("賣家"))
            {
                videoInfo.Producer = valueNode.InnerText;
            }
            else if (key.Contains("發行"))
            {
                videoInfo.Publisher = valueNode.InnerText;
            }
            else if (key.Contains("系列"))
            {
                videoInfo.Series = valueNode.InnerText;
            }
            else if (key.Contains("類別"))
            {
                var categoryNodes = valueNode.SelectNodes("a");
                List<string> categoryList = [];
                categoryList.AddRange(categoryNodes.Select(node => node.InnerText));
                videoInfo.Category = string.Join(",", categoryList);
            }
            else if (key.Contains("演員"))
            {
                var actorNodes = valueNode.SelectNodes("a");
                var genderNodes = valueNode.SelectNodes("strong");

                if (actorNodes == null) continue;
                var actorList = new List<string>();


                for (var j = 0; j < actorNodes.Count; j++)
                {
                    var actorNode = actorNodes[j];

                    //♀ or ♂
                    var genderNode = genderNodes[j];

                    actorList.Add($"{actorNode.InnerText.Trim()}{genderNode.InnerText}");
                }
                videoInfo.Actor = string.Join(",", actorList);
            }
        }

        //标题
        var titleNode = htmlDoc.DocumentNode.SelectSingleNode(".//strong[@class='current-title']");
        var title = titleNode.InnerText;
        videoInfo.Title = title.Replace(videoInfo.trueName, "").Trim();

        //下载封面
        var savePath = AppSettings.ImageSavePath;
        var filePath = Path.Combine(savePath, cid);
        videoInfo.ImageUrl = imageUrl;
        videoInfo.ImagePath = await GetInfoFromNetwork.DownloadFile(imageUrl, filePath, cid);

        //样品图片
        var previewImagesSingesNode = htmlDoc.DocumentNode.SelectSingleNode("//div[contains(@class,'preview-images')]");
        if (previewImagesSingesNode == null) return videoInfo;

        var previewImagesNodes = previewImagesSingesNode.SelectNodes(".//a[@class='tile-item']");
        List<string> sampleUrlList = [];
        if (previewImagesNodes == null) return videoInfo;

        foreach (var node in previewImagesNodes)
        {
            var sampleImageUrl = node.Attributes["href"].Value;
            if (!sampleImageUrl.Contains("http"))
            {
                sampleImageUrl = GetInfoFromNetwork.UrlCombine(javDbUrl, sampleImageUrl);
            }
            sampleUrlList.Add(sampleImageUrl);
        }
        videoInfo.SampleImageList = string.Join(",", sampleUrlList);

        return videoInfo;
    }

    private async Task<string> GetDetailUrlFromCid(string cid, CancellationToken token)
    {
        var url = GetInfoFromNetwork.UrlCombine(BaseUrl, $"search?q={cid}&f=all");

        // 访问
        var tuple = await RequestHelper.RequestHtml(Common.Client, url, token);
        if (tuple == null) return null;

        var strResult = tuple.Item2;
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(strResult);

        var result = GetDetailUrlFromSearchResult(htmlDoc, cid);

        return result;
    }

    private static string GetDetailUrlFromSearchResult(HtmlDocument htmlDoc, string cid)
    {
        var searchResultNodes = htmlDoc.DocumentNode.SelectNodes("//div[contains(@class,'movie-list')]");

        //搜索无果，退出
        if (searchResultNodes == null) return null;

        //分割通过正则匹配得到的CID
        var splitResult = Common.SplitCid(cid);
        if (splitResult == null) return null;

        var leftCid = splitResult.Item1;
        var rightCid = splitResult.Item2;

        foreach (var movieList in searchResultNodes)
        {
            var searchLeftCid = string.Empty;
            var searchRightCid = string.Empty;

            var titleSearch = movieList.SelectSingleNode(".//div[@class='video-title']/strong").InnerText;
            var title = titleSearch.ToUpper();

            var split = title.Split('-', '_');
            switch (split.Length)
            {
                case 1:
                {
                    var matchResult = Regex.Match(title, @"([A-Z]+)(\d+)");
                    if (matchResult.Success)
                    {
                        searchLeftCid = matchResult.Groups[1].Value;
                        searchRightCid = matchResult.Groups[2].Value;
                    }

                    break;
                }
                case 2:
                    searchLeftCid = split[0];
                    searchRightCid = split[1];
                    break;
                case 3 when title.Contains("HEYDOUGA"):
                    searchLeftCid = split[1];
                    searchRightCid = split[2];
                    break;
                case 3:
                    return null;
                default:
                    return null;
            }

            if (searchLeftCid != leftCid
                || (searchRightCid != rightCid
                    && (!int.TryParse(rightCid, out var currentNum)
                        || !int.TryParse(searchRightCid, out var searchNum)
                        || !currentNum.Equals(searchNum)))) continue;

            var detailUrl = movieList.SelectSingleNode(".//a").Attributes["href"].Value;
            detailUrl = GetInfoFromNetwork.UrlCombine(AppSettings.JavDbBaseUrl, detailUrl);
            return detailUrl;
        }

        return null;
    }

    public static string TrimGenderFromActorName(string actorName)
    {
        return actorName.TrimEnd(ManSymbol, WomanSymbol);
    }

    public static string RemoveGenderFromActorListString(string actorListString)
    {
        var result = actorListString;

        if (result.Contains(ManSymbol))
        {
            result = result.Replace(ManSymbol, char.MinValue);
        }
        else if (result.Contains(WomanSymbol))
        {
            result = result.Replace(WomanSymbol, char.MinValue);
        }

        return result;
    }
}
