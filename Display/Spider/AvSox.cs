using Display.Helper;
using HtmlAgilityPack;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Display.Data;
using SpiderInfo = Display.Models.SpiderInfo;

namespace Display.Spider;

public class AvSox
{
    public const int Id = (int)SpiderInfo.SpiderSourceName.Avsox;

    public const string Abbreviation = "Avsox";

    public const string Keywords = "AVSOX";

    public static Tuple<int, int> DelayRanges = new(1, 2);

    public const bool IgnoreFc2 = false;

    public static bool IsOn => AppSettings.IsUseAvSox;

    private static string baseUrl => AppSettings.AvSox_BaseUrl;

    public static async Task<VideoInfo> SearchInfoFromCID(string CID)
    {
        CID = CID.ToUpper();

        var detail_url = await GetDetailUrlFromCID(CID);

        //搜索无果，退出
        if (detail_url == null) return null;

        Tuple<string, string> result = await RequestHelper.RequestHtml(Common.Client, detail_url);
        if (result == null) return null;

        string htmlString = result.Item2;

        if (string.IsNullOrEmpty(htmlString)) return null;

        HtmlDocument htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(htmlString);

        return await GetVideoInfoFromHtmlDoc(CID, detail_url, htmlDoc);
    }

    private static async Task<string> GetDetailUrlFromCID(string CID)
    {
        string result;
        string url = GetInfoFromNetwork.UrlCombine(baseUrl, $"cn/search/{CID}");

        // 访问
        Tuple<string, string> tuple = await RequestHelper.RequestHtml(Common.Client, url);
        if (tuple == null) return null;

        string strResult = tuple.Item2;

        HtmlDocument htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(strResult);

        result = GetDetailUrlFromSearchResult(htmlDoc, CID);

        return result;
    }

    private static string GetDetailUrlFromSearchResult(HtmlDocument htmlDoc, string CID)
    {
        string result = null;

        //是否提示搜索失败
        var alertNode = htmlDoc.DocumentNode.SelectSingleNode("//div[contains(@class,'alert-danger')]");
        if (alertNode != null) return null;

        var searchResultNodes = htmlDoc.DocumentNode.SelectNodes("//div[@id='waterfall']/div[@class='item']");
        //搜索无果，退出
        if (searchResultNodes == null) return null;

        //分割通过正则匹配得到的CID
        var splitResult = Common.SplitCid(CID);
        if (splitResult == null) return null;

        var left_cid = splitResult.Item1;
        var right_cid = splitResult.Item2;

        string search_left_cid;
        string search_right_cid;
        for (var i = 0; i < searchResultNodes.Count; i++)
        {
            var movie_list = searchResultNodes[i];
            var title_search = movie_list.SelectSingleNode(".//div[@class='photo-info']/span/date")?.InnerText;
            if (title_search == null) continue;

            string title = title_search.ToUpper();

            var split_result = title.Split(new char[] { '-', '_' });
            //没有分隔符，尝试正则匹配（n167）
            if (split_result.Length == 1)
            {
                var match_result = Regex.Match(title, @"^([A-Z]+)(\d+)$");
                if (match_result == null) continue;
                search_left_cid = match_result.Groups[1].Value;
                search_right_cid = match_result.Groups[2].Value;
            }
            //有且只有两个分隔符（HEYZO-2934）
            else if (split_result.Length == 2)
            {
                search_left_cid = split_result[0];
                search_right_cid = split_result[1];
            }
            //有且有三个分隔符（FC2-PPV-3143749）
            else if (split_result.Length == 3)
            {
                if (CID.Contains("FC2"))
                {
                    search_left_cid = "FC2";
                    search_right_cid = split_result[2];
                }
                else
                    return null;
            }
            //超过三个，预料之外的情况
            else
                return null;

            int currentNum;
            int searchNum;

            if (search_left_cid == left_cid
                     && (search_right_cid == right_cid
                            || (Int32.TryParse(right_cid, out currentNum)
                                    && Int32.TryParse(search_right_cid, out searchNum)
                                        && currentNum.Equals(searchNum))))
            {
                var detail_url = searchResultNodes[i].SelectSingleNode(".//a[contains(@class,'movie-box')]").Attributes["href"].Value;

                //只有“//”没有“http(s)://”，补充上
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

        info.is_wm = 1;

        return info;
    }
}
