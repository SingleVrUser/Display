using HtmlAgilityPack;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Display.Models.Data;
using static Display.Models.Spider.SpiderInfos;

namespace Display.Helper.Network.Spider;

public class Manager
{
    public static readonly InfoSpider[] Spiders = {
        new JavBus(),
        new JavDb()
    };

    private static Manager _manager;
    public static Manager Instance
    {
        get
        {
            _manager ??= new Manager();

            return _manager;
        }
    }

    public async Task<VideoInfo> DispatchSpiderInfoByCidInOrder(string cid, CancellationToken token)
    {
        cid = cid.ToUpper();

        VideoInfo videoInfo = null;

        foreach (var spider in Spiders)
        {
            if (token.IsCancellationRequested) break;
            if (!spider.IsSearch(cid)) continue;
            videoInfo = await spider.GetInfoByCid(cid, token);
            if (videoInfo != null) break;
        }

        return videoInfo;
    }

    public async Task<List<VideoInfo>> DispatchSpiderInfosByCidInOrder(string cid, CancellationToken token = default)
    {
        cid = cid.ToUpper();

        List<VideoInfo> videoInfos = new();

        foreach (var spider in Spiders)
        {
            if(token.IsCancellationRequested) break;

            //判断搜刮源是否可以搜刮该番号
            if (!spider.IsSearch(cid)) continue;

            var videoInfo = await spider.GetInfoByCid(cid, token);

            if (videoInfo == null) continue;

            videoInfos.Add(videoInfo);
        }

        return videoInfos;
    }

    public async Task<VideoInfo> DispatchSpecificSpiderInfoByCid(string cid, SpiderSourceName spiderName, CancellationToken token)
    {
        var spider = Spiders.FirstOrDefault(i => spiderName.Equals(i.Name));
        if(spider == null) return null;
        return await spider.GetInfoByCid(cid, token);
    }

    public async Task<VideoInfo> DispatchSpiderInfoByDetailUrl(string cid, string detailUrl, CancellationToken token)
    {
        //先访问detail_url，获取到标题
        //当访问JavDB且内容为FC2时，由于使用的是CommonClient，所以会提示需要登入
        var tuple = await RequestHelper.RequestHtml(GetInfoFromNetwork.CommonClient, detailUrl, token);
        if (tuple == null) return null;

        var strResult = tuple.Item2;
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(strResult);
        var titleNode = htmlDoc.DocumentNode.SelectSingleNode("/html/head/title");
        if (titleNode == null) return null;

        var title = titleNode.InnerText;

        VideoInfo info = null;

        //通过标题判断需要用哪种方式解析
        foreach (var spider in Spiders)
        {
            if(!title.Contains(spider.Keywords)) continue;

            // 当遇到需要登入才能访问的内容时，使用特定的client
            if (title.Contains("登入") && spider is JavDb)
            {
                tuple = await RequestHelper.RequestHtml(GetInfoFromNetwork.ClientWithJavDBCookie, detailUrl, token);
                if (tuple == null) return null;

                strResult = tuple.Item2;
                htmlDoc.LoadHtml(strResult);
            }

            info = await spider.GetInfoByHtmlDoc(cid.ToUpper(), detailUrl, htmlDoc);
            break;
        }

        return info;
    }

    //public class SpiderSource
    //{
    //    public string Name { get; }

    //    public Tuple<int, int> DelayRanges { get; }

    //    public bool IsTrue { get; }

    //    public bool OnlyFc2 { get; }

    //    public bool IgnoreFc2 { get; } = true;

    //    public SpiderSource(SpiderSourceName source)
    //    {
    //        var name = string.Empty;

    //        switch (source)
    //        {
    //            case SpiderSourceName.Javbus:
    //                name = JavBus.Abbreviation;
    //                DelayRanges = JavBus.DelayRanges;
    //                IsTrue = JavBus.IsOn;
    //                break;
    //            case SpiderSourceName.Jav321:
    //                name = Jav321.Abbreviation;
    //                DelayRanges = Jav321.DelayRanges;
    //                IsTrue = Jav321.IsOn;
    //                break;
    //            case SpiderSourceName.Avmoo:
    //                name = AvMoo.Abbreviation;
    //                DelayRanges = AvMoo.DelayRanges;
    //                IsTrue = AvMoo.IsOn;
    //                break;
    //            case SpiderSourceName.Avsox:
    //                name = AvSox.Abbreviation;
    //                DelayRanges = AvSox.DelayRanges;
    //                IsTrue = AvSox.IsOn;
    //                break;
    //            case SpiderSourceName.Libredmm:
    //                name = LibreDmm.Abbreviation;
    //                DelayRanges = LibreDmm.DelayRanges;
    //                IsTrue = LibreDmm.IsOn;
    //                break;
    //            case SpiderSourceName.Fc2club:
    //                name = Fc2hub.Abbreviation;
    //                DelayRanges = Fc2hub.DelayRanges;
    //                IsTrue = Fc2hub.IsOn;
    //                OnlyFc2 = Fc2hub.OnlyFc2;
    //                IgnoreFc2 = Fc2hub.IgnoreFc2;
    //                break;
    //            case SpiderSourceName.Javdb:
    //                name = JavDB.Abbreviation;
    //                DelayRanges = JavDB.DelayRanges;
    //                IsTrue = JavDB.IsOn;
    //                IgnoreFc2 = JavDB.IgnoreFc2;
    //                break;
    //            case SpiderSourceName.Local:
    //                break;
    //            default:
    //                throw new ArgumentOutOfRangeException(nameof(source), source, null);
    //        }

    //        Name = name;
    //    }
    //}

}

