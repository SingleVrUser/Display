using Display.Helper.Network;
using Display.Models.Spider;
using Display.Providers.Spider;
using HtmlAgilityPack;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DataAccess.Dao.Interface;
using DataAccess.Models.Entity;
using Display.Models.Vo.Spider;
using Display.Providers;
using DataAccess.Dao.Impl;
using FFmpeg.AutoGen.Abstractions;

namespace Display.Managers;

public class SpiderManager
{
    private CancellationTokenSource _cancellationTokenSource;

    /// <summary>
    /// 待处理的队列
    /// </summary>
    private readonly ConcurrentQueue<SpiderItem> _taskItemQueue = [];

    /// <summary>
    /// 成功队列（存入数据库的临时空间）
    /// </summary>
    private readonly ConcurrentQueue<VideoInfo> _successNameInfos = [];

    private readonly ConcurrentQueue<string> _failureNameInfos = [];

    private readonly IFileToInfoDao _fileToInfoDao = App.GetService<IFileToInfoDao>();
    private readonly IVideoInfoDao _videoInfoDao = App.GetService<IVideoInfoDao>();
    

    /// <summary>
    /// 随机
    /// </summary>
    private readonly Random _random = new();

    private static BaseSpider[] _spiders;

    public static BaseSpider[] Spiders
    {
        get
        {
            if (_spiders != null) return _spiders;

            _spiders =
            [
                new JavBus(),
                new JavDb(),
                new AvMoo(),
                new AvSox(),
                new Fc2Hub(),
                new LibreDmm()
            ];

            return _spiders;
        }
    }

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
        var tuple = await RequestHelper.RequestHtml(NetworkHelper.CommonClient, detailUrl, token);
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
                tuple = await RequestHelper.RequestHtml(DbNetworkHelper.ClientWithJavDbCookie, detailUrl, token);
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
                    bool isContinue;
                    lock (Spiders)
                    {
                        // 其他运行中的搜刮源
                        isContinue = Spiders.Any(curSpider => curSpider.HandleItem != null &&
                            !curSpider.HandleItem.DoneSpiderNameArray.Contains(spider.Name));
                    }

                    await Task.Delay(100, token); // 必须延迟，不然Exec前的等待会一直堵塞（例:当queue里只有一个）
                    if (!isContinue) break;
                }

                continue;
            }

            // 之前搜索过了，跳过
            if (item.DoneSpiderNameArray.Contains(spider.Name))
            {
                // 能运行到这，说明数量很少了（队列先进先出）

                // 归还
                _taskItemQueue.Enqueue(item);

                await Task.Delay(100, token); // 必须延迟，不然会强占资源（一直取出、归还）

                // 如果剩下的待处理的包括正在处理的队列都是该搜刮源搜索过的，则退出该搜刮源的搜索
                if (IsLeftNamesSpiderDone(spider)) break;

                continue;
            }

            spider.HandleItem = item;

            // 当前搜刮源可以搜索该资源
            if (spider.IsSearch(item.Name))
            {
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
                        Debug.WriteLine($"{spider.Name}搜索: {item.Name} - 搜索结果为空");
                    }
                    else
                    {
                        item.AddInfo(newInfo);
                        Debug.WriteLine(item.IsCompleted ? "成功" : "成功但不完整");
                    }

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
            }

            // 向Item项添加当前id
            item.DoneSpiderNameArray.Enqueue(spider.Name);

            // 所有字段搜索完成
            var isThisNameDone = item.IsCompleted;

            // 没搜索完整，但全部的正在启动的搜刮源都搜索过了
            if (!isThisNameDone)
            {
                var onSpiderCount = Spiders.Count(curSpider => curSpider.IsOn);

                isThisNameDone = item.DoneSpiderNameArray.Count >= onSpiderCount;
            }

            spider.HandleItem = null;

            if (isThisNameDone)
            {
                DoWhenNameIsAllSearched(item);
                continue;
            }

            // 归还到任务队列
            _taskItemQueue.Enqueue(item);

            // 如果剩下的待处理的包括正在处理的队列都是该搜刮源搜索过的，则退出该搜刮源的搜索
            if (IsLeftNamesSpiderDone(spider)) break;
        }

        Debug.WriteLine($"当前搜刮源完成: {spider.Name}");
        lock (Spiders) spider.IsRunning = false;
    }

    private bool IsLeftNamesSpiderDone(BaseSpider curSpider)
    {
        var spiders = Spiders.Where(spider => spider.IsOn).ToArray();
        var queueArray = _taskItemQueue.ToArray();

        return queueArray.All(item => item.DoneSpiderNameArray.Contains(curSpider.Name)) &&
               spiders.All(spider => spider.HandleItem == null ||
                                     (spider.HandleItem != null && spider.HandleItem.DoneSpiderNameArray.Contains(curSpider.Name)));
    }

    /// <summary>
    /// 任务完成后
    /// </summary>
    /// <param name="item"></param>
    private async void DoWhenNameIsAllSearched(SpiderItem item)
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
        await DataAccessLocal.Add.AddVideoInfo_ActorInfo_IsWmAsync(item.Info);

        _fileToInfoDao.UpdateIsSuccessByTrueName(item.Name, 1);
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
        var tasks = Spiders
            .Where(spider => spider.IsSearch(cid))
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
    /// 批量添加任务
    /// </summary>
    /// <param name="names"></param>
    public async void AddTask(IEnumerable<string> names)
    {
        // 添加进任务队列
        foreach (var name in names)
        {
            var upperName = name.ToUpper();

            Debug.WriteLine(upperName);
            // 先从数据库中搜索
            var singleVideoInfoByTrueName = _videoInfoDao.GetOneByTrueName(upperName);
            if (singleVideoInfoByTrueName != null)
            {
                //替换数据库的数据
                _fileToInfoDao.UpdateIsSuccessByTrueName(upperName, 1);
                continue;
            }
            _taskItemQueue.Enqueue(new SpiderItem(upperName));
        }

        //记录当前任务队列的个数
        LeftNameCount = _taskItemQueue.Count;

        _cancellationTokenSource ??= new CancellationTokenSource();

        // 只启动未启动的搜刮源
        var tasks = Spiders
            .Where(i => i.IsOn && !i.IsRunning)
            .Select(spider => RunTaskBySingleSpider(spider, _cancellationTokenSource.Token)).ToArray();

        // 没有需要启动的搜刮源就退出
        if (tasks.Length == 0) return;

        await Task.WhenAll(tasks);

        Debug.WriteLine("所有搜刮源完成搜刮");

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

    public string[] GetFailNames()
    {
        return _failureNameInfos.ToArray();
    }

    public string[] SpiderNames => Spiders.Where(item => item.IsOn).Select(item => item.Abbreviation).ToArray();

    public int LeftNameCount { get; set; }

    public event Action<string> ItemTaskFailAction;
    public event Action<string> ItemTaskSuccessAction;
    public event Action TaskCompletedAction;

    #endregion
}

