using Display.Helper;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Display.Data;
using SpiderInfo = Display.Models.SpiderInfo;

namespace Display.Spider;

public class Manager
{
    private static Dictionary<int, Func<string, Task<VideoInfo>>> spiderByCIDHandlers;
    private static Dictionary<string, Func<string, string, HtmlDocument, Task<VideoInfo>>> spiderByUrlHandlers;

    private static Manager _manager;
    public static Manager Current
    {
        get
        {
            _manager ??= new Manager();

            return _manager;
        }
    }

    public Manager()
    {
        spiderByCIDHandlers = new Dictionary<int, Func<string, Task<VideoInfo>>>
        {
            { JavBus.Id, JavBus.SearchInfoFromCID },
            { Jav321.Id, Jav321.SearchInfoFromCid },
            { AvMoo.Id, AvMoo.SearchInfoFromCID },
            { AvSox.Id, AvSox.SearchInfoFromCID },
            { LibreDmm.Id, LibreDmm.SearchInfoFromCID },
            { Fc2hub.Id, Fc2hub.SearchInfoFromCID },
            { JavDB.Id, JavDB.SearchInfoFromCID },
        };

        spiderByUrlHandlers = new()
        {
            { JavBus.Keywords, JavBus.GetVideoInfoFromHtmlDoc },
            { Jav321.Keywords, Jav321.GetVideoInfoFromHtmlDoc },
            { AvMoo.Keywords, AvMoo.GetVideoInfoFromHtmlDoc },
            { AvSox.Keywords, AvSox.GetVideoInfoFromHtmlDoc },
            { LibreDmm.Keywords, LibreDmm.GetVideoInfoFromHtmlDoc },
            { Fc2hub.Keywords, Fc2hub.GetVideoInfoFromHtmlDoc },
            { JavDB.Keywords, JavDB.GetVideoInfoFromHtmlDoc },
        };

    }
    public async Task<VideoInfo> DispatchSpiderInfoByCIDInOrder(string CID)
    {
        CID = CID.ToUpper();

        VideoInfo videoInfo = null;


        foreach (int id in Enum.GetValues(typeof(SpiderInfo.SpiderSourceName)))
        {
            SpiderSource spiderSource = new((SpiderInfo.SpiderSourceName)id);

            //判断搜刮源是否可以搜刮该番号
            if (!AnalysisIfCIDCanSpider(CID, spiderSource)) continue;

            videoInfo = await DispatchSpecificSpiderInfoByCID(CID, id);

            if (videoInfo != null) break;
        }

        return videoInfo;
    }

    public async Task<List<VideoInfo>> DispatchSpiderInfosByCIDInOrder(string CID)
    {
        CID = CID.ToUpper();

        List<VideoInfo> videoInfos = new();

        foreach (int id in Enum.GetValues(typeof(SpiderInfo.SpiderSourceName)))
        {
            SpiderSource spiderSource = new((SpiderInfo.SpiderSourceName)id);

            //判断搜刮源是否可以搜刮该番号
            if (!AnalysisIfCIDCanSpider(CID, spiderSource)) continue;

            var videoInfo = await DispatchSpecificSpiderInfoByCID(CID, id);

            if (videoInfo == null) continue;

            videoInfos.Add(videoInfo);
        }



        return videoInfos;
    }

    private bool AnalysisIfCIDCanSpider(string CID, SpiderSource spiderSource)
    {
        if (!spiderSource.IsTrue) return false;

        var isFc2 = CID.Contains("FC2");

        //“是Fc2且忽略FC2” 或者 “是Fc2且只有Fc2”
        // 满足以上条件，跳过该搜刮源
        return (!isFc2 || !spiderSource.IgnoreFc2) && (isFc2 || !spiderSource.OnlyFc2);
    }

    public async Task<VideoInfo> DispatchSpecificSpiderInfoByCID(string CID, int spiderId)
    {
        if (!spiderByCIDHandlers.TryGetValue(spiderId, out Func<string, Task<VideoInfo>> func)) return null;

        return await func(CID.ToUpper());
    }


    public async Task<VideoInfo> DispatchSpiderInfoByDetailUrl(string cid, string detailUrl)
    {
        //先访问detail_url，获取到标题
        //当访问JavDB且内容为FC2时，由于使用的是CommonClient，所以会提示需要登入
        var tuple = await RequestHelper.RequestHtml(GetInfoFromNetwork.CommonClient, detailUrl);
        if (tuple == null) return null;

        string strResult = tuple.Item2;
        HtmlDocument htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(strResult);
        var titleNode = htmlDoc.DocumentNode.SelectSingleNode("/html/head/title");
        if (titleNode == null) return null;

        var title = titleNode.InnerText;

        VideoInfo info = null;

        //通过标题判断需要用哪种方式解析
        foreach (var handler in spiderByUrlHandlers)
        {
            var keywords = handler.Key;
            var fun = handler.Value;

            if (!title.Contains(keywords)) continue;

            // 当遇到需要登入才能访问的内容时，使用特定的client
            if (keywords == JavDB.Keywords && title.Contains("登入"))
            {
                tuple = await RequestHelper.RequestHtml(GetInfoFromNetwork.ClientWithJavDBCookie, detailUrl);
                if (tuple == null) return null;

                strResult = tuple.Item2;
                htmlDoc.LoadHtml(strResult);
            }

            info = await fun(cid.ToUpper(), detailUrl, htmlDoc);
            break;
        }

        return info;
    }




    public class SpiderSource
    {
        public string Name { get; private set; }

        public Tuple<int, int> DelayRanges { get; private set; }

        public bool IsTrue { get; private set; }

        public bool OnlyFc2 { get; private set; } = false;

        public bool IgnoreFc2 { get; private set; } = true;

        public SpiderSource(SpiderInfo.SpiderSourceName source)
        {
            string name = string.Empty;

            switch (source)
            {
                case SpiderInfo.SpiderSourceName.Javbus:
                    name = JavBus.Abbreviation;
                    DelayRanges = JavBus.DelayRanges;
                    IsTrue = JavBus.IsOn;
                    break;
                case SpiderInfo.SpiderSourceName.Jav321:
                    name = Jav321.Abbreviation;
                    DelayRanges = Jav321.DelayRanges;
                    IsTrue = Jav321.IsOn;
                    break;
                case SpiderInfo.SpiderSourceName.Avmoo:
                    name = AvMoo.Abbreviation;
                    DelayRanges = AvMoo.DelayRanges;
                    IsTrue = AvMoo.IsOn;
                    break;
                case SpiderInfo.SpiderSourceName.Avsox:
                    name = AvSox.Abbreviation;
                    DelayRanges = AvSox.DelayRanges;
                    IsTrue = AvSox.IsOn;
                    break;
                case SpiderInfo.SpiderSourceName.Libredmm:
                    name = LibreDmm.Abbreviation;
                    DelayRanges = LibreDmm.DelayRanges;
                    IsTrue = LibreDmm.IsOn;
                    break;
                case SpiderInfo.SpiderSourceName.Fc2club:
                    name = Fc2hub.Abbreviation;
                    DelayRanges = Fc2hub.DelayRanges;
                    IsTrue = Fc2hub.IsOn;
                    OnlyFc2 = Fc2hub.OnlyFc2;
                    IgnoreFc2 = Fc2hub.IgnoreFc2;
                    break;
                case SpiderInfo.SpiderSourceName.Javdb:
                    name = JavDB.Abbreviation;
                    DelayRanges = JavDB.DelayRanges;
                    IsTrue = JavDB.IsOn;
                    IgnoreFc2 = JavDB.IgnoreFc2;
                    break;
            }

            this.Name = name;
        }
    }

}

