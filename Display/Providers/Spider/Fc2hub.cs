using Display.Helper.Date;
using Display.Helper.Network;
using Display.Models.Spider;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DataAccess.Models.Dto;
using Display.Models.Api.Fc2Club;

namespace Display.Providers.Spider;

public class Fc2Hub : BaseSpider
{
    public override SpiderSourceName Name => SpiderSourceName.Fc2Club;

    public override string Abbreviation => "fc";
    public override string Keywords => "Fc2hub.com";

    public override bool IgnoreFc2 => false;
    public override bool OnlyFc2 => true;

    public override bool IsOn
    {
        get => AppSettings.IsUseFc2Hub;
        set => AppSettings.IsUseFc2Hub = value;
    }
    public override string BaseUrl
    {
        get => AppSettings.Fc2HubBaseUrl;
        set => AppSettings.Fc2HubBaseUrl = value;
    }
    public override async Task<VideoInfoDto> GetInfoByCid(string cid, CancellationToken token)
    {
        var url = NetworkHelper.UrlCombine(BaseUrl, $"search?kw={cid.Replace("FC2-", "")}");

        var result = await RequestHelper.RequestHtml(Common.Client, url, token);
        if (result == null) return null;

        var detailUrl = result.Item1;
        var htmlString = result.Item2;

        if (htmlString.Contains("Redirecting to"))
        {
            var match = Regex.Match(htmlString, "<a href=\"(.*?)\"");
            if(match.Success)
            {
                var newUrl = match.Groups[1].Value;
                result = await RequestHelper.RequestHtml(Common.Client, newUrl, token);
                if (result == null) return null;
                detailUrl = result.Item1;
                htmlString = result.Item2;
            }
        }

        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(htmlString);

        return await GetInfoByHtmlDoc(cid, detailUrl, htmlDoc);
    }

    public override async Task<VideoInfoDto> GetInfoByHtmlDoc(string cid, string detailUrl, HtmlDocument htmlDoc)
    {
        var videoInfo = new VideoInfoDto
        {
            Name = cid,
            SourceUrl = detailUrl,
            IsWm = true
        };

        var jsonCollection = htmlDoc.DocumentNode.SelectNodes("//script[@type='application/ld+json']");

        if (jsonCollection == null || jsonCollection.Count == 0) return null;

        var jsonString = jsonCollection.Last().InnerText;

        var json = JsonConvert.DeserializeObject<FcJson>(jsonString);

        if (json.Name == null || json.Image == null) return null;

        videoInfo.Title = json.Name;
        videoInfo.ReleaseTime = json.DatePublished.Replace("/", "-");
        //PTxHxMxS转x分钟
        videoInfo.LengthTime = DateHelper.ConvertPtTimeToTotalMinute(json.Duration);
        videoInfo.DirectorName = json.Director;
        videoInfo.ProducerName = "fc2";

        if (json.Genre != null)
            videoInfo.CategoryList = json.Genre.ToList();

        if (json.Actor != null)
            videoInfo.ActorNameList = json.Actor.ToList();


        var imageUrl = string.Empty;
        if (json.Image != null)
        {
            imageUrl = json.Image;
        }
        else
        {
            var imageNode = htmlDoc.DocumentNode.SelectSingleNode("//meta[@property='og:image']");

            if (imageNode != null)
            {
                imageUrl = imageNode.GetAttributeValue("content", string.Empty);
            }
        }

        if (string.IsNullOrEmpty(imageUrl)) return videoInfo;

        //下载封面
        var savePath = AppSettings.ImageSavePath;
        var filePath = Path.Combine(savePath, cid);
        videoInfo.ImageUrl = imageUrl;
        videoInfo.ImagePath = await DbNetworkHelper.DownloadFile(imageUrl, filePath, cid);

        return videoInfo;
    }
}
