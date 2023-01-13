using Data.Helper;
using HtmlAgilityPack;
using Microsoft.Windows.AppNotifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Data.Spider;

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
        spiderByCIDHandlers = new()
        {
            { JavBus.Id, JavBus.SearchInfoFromCID },
            { Jav321.Id, Jav321.SearchInfoFromCID },
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


        foreach (int id in Enum.GetValues(typeof(SpiderSourceName)))
        {
            SpiderSource spiderSource = new((SpiderSourceName)id);

            //判断搜刮源是否可以搜刮该番号
            if (!AnalysisIfCIDCanSpider(CID, spiderSource)) continue;

            videoInfo = await DispatchSpecificSpiderInfoByCID(CID, id);

            if (videoInfo != null) break;
        }

        //if (!isFc && AppSettings.isUseJavBus)
        //if (videoInfo == null && !isFc && AppSettings.isUseJav321)
        //    videoInfo = await spiderManager.DispatchSpecificSpiderInfoByCID(CID, Jav321.Id);
        //if (videoInfo == null && !isFc && AppSettings.isUseAvMoo)
        //    videoInfo = await spiderManager.DispatchSpecificSpiderInfoByCID(CID, AvMoo.Id);
        //if (videoInfo == null && AppSettings.isUseAvSox)
        //    videoInfo = await spiderManager.DispatchSpecificSpiderInfoByCID(CID, AvSox.Id);
        //if (videoInfo == null && !isFc && AppSettings.isUseLibreDmm)
        //    videoInfo = await spiderManager.DispatchSpecificSpiderInfoByCID(CID, LibreDmm.Id);
        //if (videoInfo == null && isFc && AppSettings.isUseFc2Hub)
        //    videoInfo = await spiderManager.DispatchSpecificSpiderInfoByCID(CID, Fc2hub.Id);
        //if (videoInfo == null && AppSettings.isUseJavDB)
        //    videoInfo = await spiderManager.DispatchSpecificSpiderInfoByCID(CID, JavDB.Id);

        return videoInfo;
    }

    public async Task<List<VideoInfo>> DispatchSpiderInfosByCIDInOrder(string CID)
    {
        List<VideoInfo> videoInfos = new();

        foreach (int id in Enum.GetValues(typeof(SpiderSourceName)))
        {
            SpiderSource spiderSource = new((SpiderSourceName)id);

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

        bool isFc2 = CID.Contains("FC2");

        //“是Fc2且忽略FC2” 或者 “是Fc2且只有Fc2”
        // 满足以上条件，跳过该搜刮源
        if ((isFc2 && spiderSource.IgnoreFc2) || (!isFc2 && spiderSource.OnlyFc2))
        {
            return false;
        }

        return true;
    }

    public async Task<VideoInfo> DispatchSpecificSpiderInfoByCID(string CID, int spiderId)
    {
        if (!spiderByCIDHandlers.TryGetValue(spiderId, out Func<string, Task<VideoInfo>> func)) return null;

        return await func(CID);
    }


    public async Task<VideoInfo> DispatchSpiderInfoByDetailUrl(string CID, string detail_url)
    {
        //先访问detail_url，获取到标题
        var tuple = await RequestHelper.RequestHtml(GetInfoFromNetwork.Client, detail_url);
        if (tuple == null) return null;

        string strResult = tuple.Item2;
        HtmlDocument htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(strResult);
        var TitleNode = htmlDoc.DocumentNode.SelectSingleNode("/html/head/title");
        if (TitleNode == null) return null;

        var Title = TitleNode.InnerText;

        VideoInfo info = null;

        //通过标题判断需要用哪种方式解析
        foreach (var handler in spiderByUrlHandlers)
        {
            var keywords = handler.Key;
            var fun = handler.Value;

            if (Title.Contains(keywords))
            {
                info = await fun(CID.ToUpper(), detail_url, htmlDoc);
                break;
            }
        }

        return info;
    }


    public enum SpiderSourceName { javbus, jav321, avmoo, avsox, libredmm, fc2club, javdb, local }
    public class SpiderSource
    {
        public string Name { get; private set; }

        public Tuple<int, int> DelayRanges { get; private set; }

        public bool IsTrue { get; private set; }

        public bool OnlyFc2 { get; private set; } = false;

        public bool IgnoreFc2 { get; private set; } = true;

        public SpiderSource(SpiderSourceName source)
        {
            string name = string.Empty;

            switch (source)
            {
                case SpiderSourceName.javbus:
                    name = JavBus.Abbreviation;
                    DelayRanges = JavBus.DelayRanges;
                    IsTrue = JavBus.IsTrue;
                    break;
                case SpiderSourceName.jav321:
                    name = Jav321.Abbreviation;
                    DelayRanges = Jav321.DelayRanges;
                    IsTrue = Jav321.IsTrue;
                    break;
                case SpiderSourceName.avmoo:
                    name = AvMoo.Abbreviation;
                    DelayRanges = AvMoo.DelayRanges;
                    IsTrue = AvMoo.IsTrue;
                    break;
                case SpiderSourceName.avsox:
                    name = AvSox.Abbreviation;
                    DelayRanges = AvSox.DelayRanges;
                    IsTrue = AvSox.IsTrue;
                    break;
                case SpiderSourceName.libredmm:
                    name = LibreDmm.Abbreviation;
                    DelayRanges = LibreDmm.DelayRanges;
                    IsTrue = LibreDmm.IsTrue;
                    break;
                case SpiderSourceName.fc2club:
                    name = Fc2hub.Abbreviation;
                    DelayRanges = Fc2hub.DelayRanges;
                    IsTrue = Fc2hub.IsTrue;
                    OnlyFc2 = Fc2hub.OnlyFc2;
                    IgnoreFc2 = Fc2hub.IgnoreFc2;
                    break;
                case SpiderSourceName.javdb:
                    name = JavDB.Abbreviation;
                    DelayRanges = JavDB.DelayRanges;
                    IsTrue = JavDB.IsTrue;
                    IgnoreFc2 = JavDB.IgnoreFc2;
                    break;
            }

            this.Name = name;
        }


    }


}
