using HtmlAgilityPack;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Display.Models.Data;
using static Display.Models.Spider.SpiderInfos;
using Display.Helper.Network;
using Display.Providers.Spider;
using System.Collections.Concurrent;

namespace Display.Managers;

public class SpiderManager
{
    private static BaseSpider[] _spiders;
    public static BaseSpider[] Spiders
    {
        get
        {
            return _spiders ??= [
                new JavBus(),
                new JavDb()
            ];
        }
    }

    private static SpiderManager _spiderManager;
    public static SpiderManager Instance
    {
        get
        {
            _spiderManager ??= new SpiderManager();

            return _spiderManager;
        }
    }


    /// <summary>
    /// 获取当前cid的单条VideoInfo信息，多个搜刮源按顺序执行，直到搜索到为止
    /// </summary>
    /// <param name="cid"></param>
    /// <param name="token"></param>
    /// <returns>单条VideoInfo</returns>
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

    /// <summary>
    /// 获取当前cid的VideoInfo列表信息，所有搜刮源同步执行，所有搜刮源都搜一遍
    /// </summary>
    /// <param name="cid"></param>
    /// <param name="token"></param>
    /// <returns>多条VideoInfo</returns>
    public async Task<List<VideoInfo>> DispatchSpiderInfosByCidInOrder(string cid, CancellationToken token = default)
    {
        cid = cid.ToUpper();

        ConcurrentQueue<VideoInfo> videoInfoQueue = [];
        var tasks = Spiders.Where(spider => spider.IsSearch(cid))
            .Select(spider =>
            {
                return Task.Run(async () =>
                {
                    var info = await spider.GetInfoByCid(cid, token);
                    if(info != null) videoInfoQueue.Enqueue(info);
                }, token);
            }).ToArray();

        // 等待所有任务结束
        await Task.WhenAll(tasks);

        return videoInfoQueue.ToList();
    }

    public async Task<VideoInfo> DispatchSpecificSpiderInfoByCid(string cid, SpiderSourceName spiderName, CancellationToken token)
    {
        var spider = Spiders.FirstOrDefault(i => spiderName.Equals(i.Name));
        if (spider == null) return null;
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
            if (!title.Contains(spider.Keywords)) continue;

            // 当遇到需要登入才能访问的内容时，使用特定的client
            if (title.Contains("登入") && spider is JavDb)
            {
                tuple = await RequestHelper.RequestHtml(GetInfoFromNetwork.ClientWithJavDbCookie, detailUrl, token);
                if (tuple == null) return null;

                strResult = tuple.Item2;
                htmlDoc.LoadHtml(strResult);
            }

            info = await spider.GetInfoByHtmlDoc(cid.ToUpper(), detailUrl, htmlDoc);
            break;
        }

        return info;
    }
}

