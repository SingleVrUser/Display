using Display.Helper.Network;
using HtmlAgilityPack;
using System.Threading;
using System.Threading.Tasks;
using DataAccess.Models.Entity;
using Display.Models.Spider;

namespace Display.Providers.Spider;

public class AvMoo : BaseSpider
{
    public override SpiderSourceName Name => SpiderSourceName.Avmoo;

    public override string Abbreviation => "Avmoo";

    public override string Keywords => "AVMOO";

    public override bool IgnoreFc2 => false;

    public override bool IsOn
    {
        get => AppSettings.IsUseAvMoo;
        set => AppSettings.IsUseAvMoo = value;
    }

    public override string BaseUrl
    {
        get => AppSettings.AvMooBaseUrl;
        set => AppSettings.AvMooBaseUrl = value;
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
        var url = NetworkHelper.UrlCombine(BaseUrl, $"cn/search/{cid}");

        // 访问
        var result = await RequestHelper.RequestHtml(Common.Client, url, token);
        if (result == null) return null;

        var htmlString = result.Item2;

        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(htmlString);

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
        var splitCid = Common.SplitCid(cid);
        if (splitCid == null) return null;

        var leftCid = splitCid.Item1;
        var rightCid = splitCid.Item2;

        foreach (var movieList in searchResultNodes)
        {
            var upperTitle = movieList.SelectSingleNode(".//div[@class='photo-info']/span/date").InnerText.ToUpper();

            if (!Common.IsSearchResultMatch(leftCid, rightCid, upperTitle)) continue;

            var detailUrl = movieList.SelectSingleNode(".//a[@class='movie-box']").Attributes["href"].Value;

            //只有“//”没有“http(s)://”
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

        info.IsWm = 0;

        return info;
    }

}
