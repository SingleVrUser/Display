using Display.Helper;
using HtmlAgilityPack;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Display.Data;
using SpiderInfo = Display.Models.SpiderInfo;
using System.Threading;

namespace Display.Spider;

public class AvMoo
{
    public const int Id = (int)SpiderInfo.SpiderSourceName.Avmoo;

    public const string Abbreviation = "Avmoo";

    public const string Keywords = "AVMOO";

    public static Tuple<int, int> DelayRanges = new(1, 2);

    public const bool IgnoreFc2 = true;

    public static bool IsOn => AppSettings.IsUseAvMoo;


    private static string baseUrl => AppSettings.AvMooBaseUrl;

    public static async Task<VideoInfo> SearchInfoFromCID(string CID, CancellationToken token)
    {
        CID = CID.ToUpper();

        var detail_url = await GetDetailUrlFromCID(CID, token);
        //搜索无果，退出
        if (detail_url == null) return null;

        Tuple<string, string> result = await RequestHelper.RequestHtml(Common.Client, detail_url, token);
        if (result == null) return null;

        string htmlString = result.Item2;

        if (string.IsNullOrEmpty(htmlString)) return null;

        HtmlDocument htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(htmlString);

        return await GetVideoInfoFromHtmlDoc(CID, detail_url, htmlDoc);

    }

    private static async Task<string> GetDetailUrlFromCID(string CID, CancellationToken token)
    {
        string url = GetInfoFromNetwork.UrlCombine(baseUrl, $"cn/search/{CID}");

        // 访问
        Tuple<string, string> result = await RequestHelper.RequestHtml(Common.Client, url, token);
        if (result == null) return null;

        string htmlString = result.Item2;

        HtmlDocument htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(htmlString);

        htmlString = GetDetailUrlFromSearchResult(htmlDoc, CID);

        return htmlString;
    }

    private static string GetDetailUrlFromSearchResult(HtmlDocument htmlDoc, string CID)
    {
        string result = null;

        //是否提示搜索失败
        var alertNode = htmlDoc.DocumentNode.SelectSingleNode("//div[contains(@class,'alert-danger')]");
        if (alertNode != null) return null;

        var SearchResultNodes = htmlDoc.DocumentNode.SelectNodes("//div[@id='waterfall']/div[@class='item']");

        //搜索无果，退出
        if (SearchResultNodes == null) return null;

        //分割通过正则匹配得到的CID
        var spliteResult = Common.SplitCid(CID);
        if (spliteResult == null) return null;

        string left_cid = spliteResult.Item1;
        string right_cid = spliteResult.Item2;

        string search_left_cid;
        string search_right_cid;
        for (var i = 0; i < SearchResultNodes.Count; i++)
        {
            var movie_list = SearchResultNodes[i];
            var title = movie_list.SelectSingleNode(".//div[@class='photo-info']/span/date").InnerText.ToUpper();

            var split_result = title.Split(new char[] { '-', '_' });
            if (split_result.Length == 1)
            {
                var match_result = Regex.Match(title, @"([A-Z]+)(\d+)");
                if (match_result == null) continue;
                search_left_cid = match_result.Groups[1].Value;
                search_right_cid = match_result.Groups[2].Value;
            }
            else if (split_result.Length == 2)
            {
                search_left_cid = split_result[0];
                search_right_cid = split_result[1];
            }
            else
                continue;

            int currentNum;
            int searchNum;

            if (search_left_cid == left_cid
                     && (search_right_cid == right_cid
                            || (Int32.TryParse(right_cid, out currentNum)
                                    && Int32.TryParse(search_right_cid, out searchNum)
                                        && currentNum.Equals(searchNum))))
            {
                var detail_url = SearchResultNodes[i].SelectSingleNode(".//a[@class='movie-box']").Attributes["href"].Value;

                //只有“//”没有“http(s)://”
                if (!detail_url.Contains("http") && detail_url.Contains("//"))
                {
                    detail_url = $"https:{detail_url}";
                }

                result = detail_url;
                break;
            }
            else
                continue;
        }

        return result;
    }


    public static async Task<VideoInfo> GetVideoInfoFromHtmlDoc(string CID, string detail_url, HtmlDocument htmlDoc)
    {
        var info = await Common.AnalysisHtmlDocInfoFromAvSoxOrAvMoo(CID, detail_url, htmlDoc);
        if (info == null) return null;

        info.IsWm = 0;

        return info;
    }
}
