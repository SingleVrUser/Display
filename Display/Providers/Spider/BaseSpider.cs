using Display.Models.Spider;
using HtmlAgilityPack;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DataAccess.Models.Entity;
using Display.Models.Entities.OneOneFive;
using Display.Models.Vo.Spider;

namespace Display.Providers.Spider;

public abstract class BaseSpider
{
    public bool IsRunning { get; set; }

    public SpiderItem HandleItem { get; set; }

    public abstract SpiderSourceName Name { get; }
    public abstract string Abbreviation { get; }

    /**
     * 标题包含的关键词，据此判断使用什么解析HtmlDoc
     */
    public abstract string Keywords { get; }

    public abstract bool IsOn { get; set; }
    public abstract string BaseUrl { get; set; }

    public virtual bool IsCookieEnable => false;

    public virtual int MinDelaySecond { get; set; } = 2;
    public virtual int MaxDelaySecond { get; set; } = 5;

    public virtual Tuple<int, int> DelayRanges => new(1, 3);
    public virtual HttpClient Client => Common.Client;
    public virtual bool IgnoreFc2 => true;
    public virtual bool OnlyFc2 => false;
    public virtual string Cookie { get; set; }

    public abstract Task<VideoInfo> GetInfoByCid(string cid, CancellationToken token);

    public abstract Task<VideoInfo> GetInfoByHtmlDoc(string cid, string detailUrl, HtmlDocument htmlDoc);

    public CancellationTokenSource CancellationTokenSource { get; set; }

    public bool IsSearch(string cid)
    {
        if (!IsOn) return false;

        var isFc2 = cid.Contains("FC2");

        //“是Fc2且忽略FC2” 或者 “是Fc2且只有Fc2”
        // 1. 是Fc2
        //   忽略Fc2   False
        //   其他      True

        // 2. 不是Fc2
        //   只搜刮Fc2    False
        //   其他         True

        // 满足以上条件，则使用该搜刮源

        return (isFc2 && !IgnoreFc2) || (!isFc2 && !OnlyFc2);
    }
}