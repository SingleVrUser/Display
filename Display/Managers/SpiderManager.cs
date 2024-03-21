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
using Display.Helper.Data;
using System;
using System.Diagnostics;

namespace Display.Managers;

public class SpiderManager
{
    private CancellationTokenSource _cancellationTokenSource;

    /// <summary>
    /// 待处理的队列
    /// </summary>
    private readonly ConcurrentQueue<SearchItem> _taskItemQueue = [];

    /// <summary>
    /// 成功队列（存入数据库的临时空间）
    /// </summary>
    private readonly ConcurrentQueue<VideoInfo> _successNameInfos = [];

    private readonly ConcurrentQueue<string> _failureNameInfos = [];

    /// <summary>
    /// 随机
    /// </summary>
    private readonly Random _random = new();

    public static BaseSpider[] Spiders = [
        new JavBus(),
        new JavDb()
    ];

    #region 单线程

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
    /// 通过指定搜刮源搜索VideoInfo
    /// </summary>
    /// <param name="cid"></param>
    /// <param name="spiderName"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public async Task<VideoInfo> DispatchSpecificSpiderInfoByCid(string cid, SpiderSourceName spiderName, CancellationToken token)
    {
        var spider = Spiders.FirstOrDefault(i => spiderName.Equals(i.Name));
        if (spider == null) return null;
        return await spider.GetInfoByCid(cid, token);
    }

    /// <summary>
    /// 通过指定网页搜索VideoInfo
    /// </summary>
    /// <param name="cid"></param>
    /// <param name="detailUrl"></param>
    /// <param name="token"></param>
    /// <returns></returns>
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

    #endregion

    #region 多线程

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
                    if (info != null) videoInfoQueue.Enqueue(info);
                }, token);
            }).ToArray();

        // 等待所有任务结束
        await Task.WhenAll(tasks);

        return videoInfoQueue.ToList();
    }

    /// <summary>
    /// 运行启动的搜刮源，搜刮任务队列
    /// </summary>
    /// <param name="spider"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    private async Task RunTaskBySingleSpider(BaseSpider spider, CancellationToken token)
    {
        lock (Spiders) spider.IsRunning = true;

        while (true)
        {
            if (token.IsCancellationRequested) break;

            if (!_taskItemQueue.TryDequeue(out var item))
            {
                // 任务队列为空的情况，还要考虑有Spider取出了，正在处理中，可能存在处理不成功的情况
                if (_taskItemQueue.IsEmpty)
                {
                    bool isAllStop;
                    lock (Spiders)
                    {
                        // 其他的正在开始
                        isAllStop = Spiders.All(currentSpider => currentSpider.Equals(spider) || !currentSpider.IsRunning);
                    }

                    await Task.Delay(1000, token); // 必须延迟，不然Exec前的等待会一直堵塞（例:当queue里只有一个）
                    if (isAllStop) break;
                }

                continue;
            }

            // 之前搜索过了，跳过
            if (item.SpiderHashCode.Contains(spider.GetHashCode()))
            {
                // 归还
                _taskItemQueue.Enqueue(item);

                continue;
            }

            try
            {
                var second = TimeSpan.FromSeconds(_random.Next(spider.MinDelaySecond,
                    spider.MaxDelaySecond));
                Debug.WriteLine($"{spider.Name}等待 {second.TotalSeconds} s");

                await Task.Delay(second, token); // 当queue只有一个时，调用这个，会一直阻塞当前线程
                //Thread.Sleep(second); // 使用这个，后面的Execute报错，但捕获不了

                if (token.IsCancellationRequested) break;

                Debug.WriteLine($"{spider.Name}开始搜索: {item.Name}");
                var newInfo = await spider.GetInfoByCid(item.Name, token);

                if (newInfo is null)
                {
                    Debug.WriteLine("搜索结果为空");
                    continue;
                }

                item.AddInfo(newInfo);

                Debug.WriteLine(item.IsCompleted ? "成功" : "成功但不完整");
            }
            catch (TaskCanceledException)
            {
                Debug.WriteLine("任务被取消了");
            }
            // 失败了
            catch (Exception e)
            {
                Debug.WriteLine($"发生错误了:{e.Message}");
            }
            finally
            {
                // 向Item项添加当前id
                item.SpiderHashCode.Enqueue(spider.GetHashCode());

                // 所有字段搜索完成
                if (item.IsCompleted)
                {
                    DoWhenNameIsAllSearched(item);
                }
                // 没搜索完整，但全部的搜刮源都搜索过了
                // 不归还
                else if (item.SpiderHashCode.Count >= Spiders.Length)
                {
                    DoWhenNameIsAllSearched(item);
                }
                else
                {
                    // 归还到任务队列
                    _taskItemQueue.Enqueue(item);
                }
            }

            if (_taskItemQueue.Count > Spiders.Length) continue;


            // 如果剩下的都是搜索过的
            var queueArray = _taskItemQueue.ToArray();
            if (queueArray.All(
                    searchItem => searchItem.SpiderHashCode.Contains(spider.GetHashCode()))
               )
            {
                break;
            }
        }

        Debug.WriteLine($"当前搜刮源完成: {spider.Name}");
        lock (Spiders) spider.IsRunning = false;
    }

    /// <summary>
    /// 任务完成后
    /// </summary>
    /// <param name="item"></param>
    private void DoWhenNameIsAllSearched(SearchItem item)
    {
        // 搜索失败
        if (item.Info is null)
        {
            ItemTaskFailAction?.Invoke(item.Name);
            _failureNameInfos.Enqueue(item.Name);
            return;
        }

        // 搜刮成功
        ItemTaskSuccessAction?.Invoke(item.Name);
        _successNameInfos.Enqueue(item.Info);

        // TODO 当_nameInfos达到指定数量时才添加进数据库
    }

    /// <summary>
    /// 批量添加任务
    /// </summary>
    /// <param name="names"></param>
    public async void AddTask(IEnumerable<string> names)
    {
        // 添加进任务队列
        foreach (var name in names)
        {
            _taskItemQueue.Enqueue(new SearchItem(name));
        }

        _cancellationTokenSource ??= new CancellationTokenSource();

        // 只启动未启动的搜刮源
        var tasks = Spiders
            .Where(i => !i.IsRunning)
            .Select(spider => RunTaskBySingleSpider(spider, _cancellationTokenSource.Token)).ToArray();

        // 没有需要启动的搜刮源就退出
        if (tasks.Length == 0) return;

        await Task.WhenAll(tasks);

        TaskCompletedAction?.Invoke();
    }

    /// <summary>
    /// 取消任务
    /// </summary>
    public void CancelTask()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null;
    }


    public List<string> GetFailNames()
    {
        //_failureNameInfos.Enqueue("nihao");
        //_failureNameInfos.Enqueue("wohao");
        //_failureNameInfos.Enqueue("dajiahao");
        return _failureNameInfos.ToList();
    }

    public List<string> GetOnSpiderNames()
    {
        return Spiders.Where(item => item.IsOn).Select(item => item.Abbreviation).ToList();
    }

    public int GetLeftNameCount() => _taskItemQueue.Count;

    public event Action<string> ItemTaskFailAction;
    public event Action<string> ItemTaskSuccessAction;
    public event Action TaskCompletedAction;

    #endregion

    private record SearchItem(string Name)
    {
        public ConcurrentQueue<int> SpiderHashCode { get; } = [];

        public VideoInfo Info { get; private set; }

        public bool IsCompleted { get; private set; }

        public void AddInfo(VideoInfo info)
        {
            if (Info == null)
            {
                Info = info;
                IsCompleted = ClassHelper.InfoIsCompleted(info);
            }
            else
            {
                UpdateInfo(info);
            }
        }

        private void UpdateInfo(VideoInfo info)
        {
            // 新数据为完整的，直接替换
            if (ClassHelper.InfoIsCompleted(Info))
            {
                Info = info;
                IsCompleted = true;
            }
            else
            {
                var oldInfo = Info;
                IsCompleted = ClassHelper.UpdateOldInfoByNew(ref oldInfo, info);
                Info = oldInfo;
            }

        }
    }

}

