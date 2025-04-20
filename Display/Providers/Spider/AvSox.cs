using Display.Helper.Network;
using Display.Models.Spider;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DataAccess.Models.Entity;

namespace Display.Providers.Spider;

public class AvSox : BaseSpider
{
    public override SpiderSourceName Name => SpiderSourceName.Avsox;

    public override bool IsOn
    {
        get => AppSettings.IsUseAvSox;
        set => AppSettings.IsUseAvSox = value;
    }
    public override string Abbreviation => "Avsox";
    public override string Keywords => "AVSOX";

    public override bool IgnoreFc2 => false;

    public override string BaseUrl
    {
        get => AppSettings.AvSoxBaseUrl;
        set => AppSettings.AvSoxBaseUrl = value;
    }
    public override async Task<VideoInfo> GetInfoByCid(string cid, CancellationToken token)
    {
        var detailUrl = await GetDetailUrlFromCid(cid, token);

        //搜索无果，退出
        if (detailUrl == null) return null;

        var result = await RequestHelper.RequestHtml(Common.Client, detailUrl, token);
        if (result == null) return null;

        var htmlString = result.Item2;

        if (string.IsNullOrEmpty(htmlString)) return null;

        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(htmlString);

        return await GetInfoByHtmlDoc(cid, detailUrl, htmlDoc);
    }

    private async Task<string> GetDetailUrlFromCid(string cid, CancellationToken token)
    {
        var searchKeyword = cid.Replace("FC2-", "");
        var url = NetworkHelper.UrlCombine(BaseUrl, $"cn/search/{searchKeyword}");

        // 访问
        var tuple = await RequestHelper.RequestHtml(Common.Client, url, token);
        if (tuple == null) return null;

        var strResult = tuple.Item2;

        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(strResult);

        return GetDetailUrlFromSearchResult(htmlDoc, cid);
    }

    private static string GetDetailUrlFromSearchResult(HtmlDocument htmlDoc, string cid)
    {
        //是否提示搜索失败
        var alertNode = htmlDoc.DocumentNode.SelectSingleNode("//div[contains(@class,'alert-danger')]");
        if (alertNode != null) return null;

        var searchResultNodes = htmlDoc.DocumentNode.SelectNodes("//div[@id='waterfall']/div[@class='item']");
        //搜索无果，退出
        if (searchResultNodes == null) return null;

        //分割通过正则匹配得到的CID
        var splitResult = Common.SplitCid(cid, cid.Contains("FC2"));
        if (splitResult == null) return null;

        var leftCid = splitResult.Item1;
        var rightCid = splitResult.Item2;

        foreach (var movieList in searchResultNodes)
        {
            var titleSearch = movieList.SelectSingleNode(".//div[@class='photo-info']/span/date")?.InnerText;
            if (titleSearch == null) continue;

            var title = titleSearch.ToUpper();

            var splitSearchResult = title.Split(['-', '_']);
            //没有分隔符，尝试正则匹配（n167）
            string searchLeftCid;
            string searchRightCid;

            switch (splitSearchResult.Length)
            {
                case 1:
                    {
                        var matchResult = Regex.Match(title, @"^([A-Z]+)(\d+)$");
                        if (!matchResult.Success) continue;
                        searchLeftCid = matchResult.Groups[1].Value;
                        searchRightCid = matchResult.Groups[2].Value;
                        break;
                    }
                //有且只有两个分隔符（HEYZO-2934）
                case 2:
                    searchLeftCid = splitSearchResult[0];
                    searchRightCid = splitSearchResult[1];
                    break;
                //有且有三个分隔符（FC2-PPV-3143749）
                case 3 when cid.Contains("FC2"):
                    searchLeftCid = "FC2";
                    searchRightCid = splitSearchResult[2];
                    break;
                case 3:
                    return null;
                //超过三个，预料之外的情况
                default:
                    return null;
            }

            if (searchLeftCid != leftCid
                || (searchRightCid != rightCid
                    && (!int.TryParse(rightCid, out var currentNum)
                        || !int.TryParse(searchRightCid, out var searchNum)
                        || !currentNum.Equals(searchNum)))) continue;

            var detailUrl = movieList.SelectSingleNode(".//a[contains(@class,'movie-box')]").Attributes["href"].Value;

            //只有“//”没有“http(s)://”，补充上
            if (!detailUrl.Contains("http") && detailUrl.Contains("//"))
            {
                detailUrl = $"https:{detailUrl}";
            }

            return detailUrl;
        }

        return null;

    }

    public override async Task<VideoInfo> GetInfoByHtmlDoc(string cid, string detailUrl, HtmlDocument htmlDoc)
    {
        var info = await Common.AnalysisHtmlDocInfoFromAvSoxOrAvMoo(cid, detailUrl, htmlDoc);
        if (info == null) return null;

        info.IsWm = 1;
        return info;
    }
}
