using Display.Models.Data;
using HtmlAgilityPack;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using static Display.Models.Spider.SpiderInfos;

namespace Display.Helper.Network.Spider;

public abstract class InfoSpider
{
    public abstract SpiderSourceName Name { get; }
    public abstract bool IsOn { get; set; }
    public abstract string Abbreviation { get; }

    public virtual bool IsCookieEnable => false;

    /**
     * 标题包含的关键词，据此判断使用什么解析HtmlDoc
     */
    public abstract string Keywords { get; }
    public abstract string BaseUrl { get; set; }
    public virtual Tuple<int, int> DelayRanges => new(1, 3);
    public virtual HttpClient Client => Common.Client;
    public virtual bool IgnoreFc2 => true;
    public virtual bool OnlyFc2 => false;
    public virtual string Cookie { get; set; }

    public abstract Task<VideoInfo> GetInfoByCid(string cid, CancellationToken token);
    public abstract Task<VideoInfo> GetInfoByHtmlDoc(string cid, string detailUrl, HtmlDocument htmlDoc);
    
    public bool IsSearch(string cid)
    {
        if (!IsOn) return false;

        var isFc2 = cid.Contains("FC2");

        //“是Fc2且忽略FC2” 或者 “是Fc2且只有Fc2”
        // 满足以上条件，跳过该搜刮源
        return (!isFc2 || !IgnoreFc2) && (isFc2 || !OnlyFc2);
    }
}