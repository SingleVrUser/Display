// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Display.CustomWindows;
using Display.Helper.Date;
using Display.Helper.FileProperties.Name;
using Display.Helper.Network.Spider;
using Display.Models.Data;
using Display.Models.Spider;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Drawing;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Microsoft.AppCenter.Utils.Synchronization;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using SkiaSharp;
using WinUIEx;
using WinUIEx.Messaging;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.Views.SpiderVideoInfo
{

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Progress : Page
    {
        private readonly CancellationTokenSource s_cts = new();
        private readonly List<string> _selectedFilesNameList;
        private List<Datum> _fileList = new();
        private readonly List<FailDatum> _failDatumList = new();

        private List<MatchVideoResult> _matchVideoResults;
        private GetInfoFromNetwork _network;
        private List<SpiderInfo> _spiderInfos;
        private List<string> _failVideoNameList;

        public Window CurrentWindow;

        public Progress(List<string> selectedFilesNameList, List<Datum> fileList)
        {
            this.InitializeComponent();
            this._selectedFilesNameList = selectedFilesNameList;
            this._fileList = fileList;
            this.Loaded += PageLoaded;
        }

        public Progress(List<FailDatum> failDatums)
        {
            this.InitializeComponent();
            this._failDatumList = failDatums;
            this.Loaded += ReSpiderPageLoaded;
        }

        private async void ReSpiderPageLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= ReSpiderPageLoaded;

            if (_failDatumList == null || _failDatumList.Count == 0) return;

            CurrentWindow.Closed += CurrentWindow_Closed;
            _matchVideoResults ??= [];

            foreach (var item in _failDatumList)
            {
                _matchVideoResults.Add(new MatchVideoResult { status = true, OriginalName = item.Datum.Name, message = "匹配成功", statusCode = 1, MatchName = item.MatchName });

                //替换数据库的数据
                DataAccess.Add.AddFileToInfo(item.Datum.PickCode, item.MatchName, isReplace: true);
            }

            //显示进度环
            ShowProgress(_matchVideoResults.Count);
            
            if (s_cts.IsCancellationRequested) return;
            await SpiderVideoInfo();
            if (s_cts.IsCancellationRequested) return;


            CurrentWindow.Closed -= CurrentWindow_Closed;

            TopProgressRing.IsActive = false;
            TotalProgress_TextBlock.Text = "完成";
        }

        private async void PageLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= PageLoaded;

            CurrentWindow.Closed += CurrentWindow_Closed;

            await ShowMatchResult();

            if (s_cts.IsCancellationRequested) return;

            await SpiderVideoInfo();

            if (s_cts.IsCancellationRequested) return;

            CurrentWindow.Closed -= CurrentWindow_Closed;

            TopProgressRing.IsActive = false;
            TotalProgress_TextBlock.Text = "完成";
        }

        /// <summary>
        /// 显示正则匹配的结果
        /// </summary>
        /// <returns></returns>
        private async Task ShowMatchResult()
        {
            if (_fileList == null) return;

            TopProgressRing.IsActive = true;

            //目前datumList仅有一级目录文件
            //遍历获取文件列表中所有的文件
            _fileList = await DataAccess.Get.GetAllFilesInFolderListAsync(_fileList);

            //除去文件夹
            _fileList = _fileList.Where(item => item.Fid!=null).ToList();

            //去除重复文件
            Dictionary<string, Datum> newDictList = new();
            _fileList.ForEach(item => newDictList.TryAdd(item.PickCode, item));

            _fileList = newDictList.Values.ToList();

            //显示饼状图
            ShowFilesPieCharts(_fileList);
            if (s_cts.IsCancellationRequested) return;

            TotalProgress_TextBlock.Text = "正则匹配番号名中……";

            //挑选符合条件的视频文件
            _matchVideoResults = await Task.Run(() => FileMatch.GetVideoAndMatchFile(_fileList));

            var totalCount = _matchVideoResults.Where(item => !string.IsNullOrEmpty(item.MatchName)).ToList().Count;

            //显示进度环
            ShowProgress(totalCount);


            TopProgressRing.IsActive = false;
        }

        private void ShowProgress(int totalCount)
        {
            //初始化进度环
            ProgressRing_Grid.Visibility = Visibility.Visible;
            overallProgress.Maximum = totalCount;
            overallProgress.Value = 0;
            countProgress_TextBlock.Text = $"0/{totalCount}";

            //初始化显示信息
            SearchProgress_TextBlock.Visibility = Visibility.Visible;
            SearchResult_StackPanel.Visibility = Visibility.Collapsed;
            ProgressRing_StackPanel.SetValue(Grid.ColumnSpanProperty, 2);

        }

        private void ShowSpiderInfoList()
        {
            CartesianChart.Visibility = Visibility.Visible;

            _spiderInfos = Manager.Spiders.Select(spider => new SpiderInfo(spider)).ToList();

            //按IsEnable排序
            _spiderInfos = _spiderInfos.OrderByDescending(item => item.IsEnable).ToList();

            SpiderInfo_GridView.ItemsSource = _spiderInfos;
        }

        /// <summary>
        /// 显示各个搜刮源搜刮数量柱状图
        /// </summary>
        private void ShowSpiderCartesianChart()
        {
            if (_spiderInfos == null || _spiderInfos.Count == 0) return;

            CartesianChart.Visibility = Visibility.Visible;

            var spiderSourceReady = _spiderInfos.Where(item => item.IsEnable).ToList();

            ISeries[] series =
                    spiderSourceReady
                        .Select(x => new RowSeries<ObservableValue>
                        {
                            Values = new[] { new ObservableValue(x.SpiderCount) },
                            Name = x.SpiderSource.ToString(),
                            Stroke = null,
                            MaxBarWidth = 25,
                            DataLabelsPaint = new SolidColorPaint(new SKColor(245, 245, 245)),
                            DataLabelsPosition = DataLabelsPosition.End,
                            DataLabelsTranslate = new LvcPoint(-1, 0),
                            DataLabelsFormatter = point => $"{point.Context.Series.Name} {point.PrimaryValue}"
                        })
                        .OrderByDescending(x => ((ObservableValue[])x.Values!)[0].Value)
                        .ToArray();
            Axis[] xAxes = {
                new() { SeparatorsPaint = new SolidColorPaint(new SKColor(220, 220, 220)) }
            };
            Axis[] yAxes = {
                new() { IsVisible = false }
            };

            CartesianChart.Series = series;
            CartesianChart.XAxes = xAxes;
            CartesianChart.YAxes = yAxes;
        }

        private void UpdateSpiderCartesianChart(SpiderInfos.SpiderSourceName spiderSource)
        {
            var item = CartesianChart.Series.FirstOrDefault(item => item.Name == spiderSource.ToString());

            ((ObservableValue[])item.Values)[0].Value += 1;
        }

        /// <summary>
        /// 显示饼形图
        /// </summary>
        /// <param Name="datumList"></param>
        private void ShowFilesPieCharts(List<Datum> datumList)
        {
            FileInfoPieChart.Visibility = Visibility.Visible;

            //1.视频文件，其中SD:x个，hd:x个，fhd:x个，1080p:x个，4k:x个，原画:x个
            var videoInfo = new FileStatistics(FileFormat.Video);
            var subInfo = new FileStatistics(FileFormat.Subtitles);
            var torrentInfo = new FileStatistics(FileFormat.Torrent);
            var imageInfo = new FileStatistics(FileFormat.Image);
            var audioInfo = new FileStatistics(FileFormat.Audio);
            //FileStatistics ArchiveInfo = new FileStatistics(FileFormat.Archive);

            foreach (var info in datumList)
            {
                //文件夹，暂不统计
                if (info.Fid == null)
                {

                }
                //视频文件
                else if (info.Iv == 1)
                {
                    var videoQuality = FilesInfo.GetVideoQualityFromVdi(info.Vdi);

                    UpdateFileStaticstics(info, videoInfo, videoQuality);
                }   
                //其他文件
                else
                {
                    switch (info.Ico)
                    {
                        //字幕
                        case "ass" or "srt" or "ssa":
                            UpdateFileStaticstics(info, subInfo, info.Ico);
                            break;
                        //种子
                        case "torrent":
                            UpdateFileStaticstics(info, torrentInfo, info.Ico);
                            break;
                        //图片
                        case "gif" or "bmp" or "tiff" or "exif" or "jpg" or "png" or "raw" or "svg":
                            UpdateFileStaticstics(info, imageInfo, info.Ico);
                            break;
                        //音频
                        case "ape" or "wav" or "midi" or "mid" or "flac" or "aac" or "m4a" or "ogg" or "amr":
                            UpdateFileStaticstics(info, audioInfo, info.Ico);
                            break;
                            //case "7z" or "cab" or "dmg" or "iso" or "rar" or "zip":
                            //    UpdateFileStaticstics(info, ArchiveInfo, info.ico);
                            //    break;
                    }
                }
            }

            CountPercentPieChart.Series = new ISeries[] {
                new PieSeries<double> { Values = new List<double> { videoInfo.Count }, Pushout = 5, Name = "视频"},
                new PieSeries<double> { Values = new List<double> { audioInfo.Count }, Pushout = 0, Name = "音频"},
                new PieSeries<double> { Values = new List<double> { subInfo.Count }, Pushout = 0, Name = "字幕"},
                new PieSeries<double> { Values = new List<double> { torrentInfo.Count }, Pushout = 0, Name = "种子"},
                new PieSeries<double> { Values = new List<double> { imageInfo.Count }, Pushout = 0, Name = "图片"}
            };

        }

        /// <summary>
        /// 分析文件信息（饼形图）
        /// </summary>
        /// <param Name="DataInfo"></param>
        /// <param Name="TypeInfo"></param>
        /// <param Name="Name"></param>
        private void UpdateFileStaticstics(Datum DataInfo, FileStatistics TypeInfo, string Name)
        {
            TypeInfo.Size += DataInfo.Size;
            TypeInfo.Count++;

            if (TypeInfo.data.Count != 0)
            {
                var tmpData = TypeInfo.data.FirstOrDefault(x => x.Name == Name);

                if (tmpData == null)
                {
                    TypeInfo.data.Add(new FileStatistics.Data() { Name = Name, Count = 1, Size = DataInfo.Size });
                }
                else
                {
                    tmpData.Size += DataInfo.Size;
                    tmpData.Count++;
                }
            }
            else
            {
                TypeInfo.data.Add(new FileStatistics.Data() { Name = Name, Count = 1, Size = DataInfo.Size });
            }
        }

        /// <summary>
        /// 开始从网络中检索视频信息
        /// </summary>
        private async Task SpiderVideoInfo()
        {
            if (_matchVideoResults == null)
                return;

            _network ??= new GetInfoFromNetwork();

            ShowSpiderInfoList();
            ShowSpiderCartesianChart();

            CountInfo_Grid.Visibility = Visibility.Visible;
            TopProgressRing.IsActive = true;

            _failVideoNameList = [];

            var startTime = DateTimeOffset.Now.ToUnixTimeSeconds();

            ProgressMore_TextBlock.Text = "失败数：0";
  
            //视频数量
            var videoCount = _matchVideoResults.Count(info => info.statusCode != 0);
            AllVideoCount_Run.Text = videoCount.ToString();

            if (videoCount == 0)
            {
                TotalProgress_TextBlock.Text = "视频数为0,停止任务";
                return;
            }

            //匹配成功的视频总数量（1：匹配成功，2：已添加（多集只保留一个番号））
            var matchSuccessVideoCount = _matchVideoResults.Where(item => item.statusCode is 1 or 2).ToList().Count;

            //匹配到的番号总数量
            var totalCount = _matchVideoResults.Where(item => !string.IsNullOrEmpty(item.MatchName)).ToList().Count;
            MatchCidCount_Run.Text = totalCount.ToString();

            if (totalCount == 0)
            {
                TotalProgress_TextBlock.Text = "匹配到的番号数量为0,停止任务";
                return;
            }

            //正则匹配成功的番号占总视频数的
            FileNameSuccessRate_Run.Text = $"{matchSuccessVideoCount * 100 / videoCount}%";

            //统计成功的名称
            var successCount = 0;
            List<string> successVideoNameList = [];

            var failCount = 0;

            var spiderSourceProgress = new Progress<SpiderInfo>(progressPercent =>
            {
                var gridViewItem = _spiderInfos.FirstOrDefault(item => item.SpiderSource == progressPercent.SpiderSource);
                //更新信息
                if (gridViewItem != null)
                {
                    gridViewItem.State = progressPercent.State;
                    gridViewItem.Message = progressPercent.Message;
                }

                //更新柱状图
                //请求成功 (搜刮源或数据库)
                if (progressPercent.RequestStates == RequestStates.success)
                {
                    successCount++;
                    CidSuccessCount_Run.Text = successCount.ToString();

                    UpdateSpiderCartesianChart(SpiderInfos.SpiderSourceName.Local);
                }
                //请求失败(搜刮源)
                else if (progressPercent.RequestStates == RequestStates.fail)
                {
                    failCount++;
                    FailCount_Run.Text = failCount.ToString();
                    ProgressMore_TextBlock.Text = $"失败数：{failCount}";
                    UpdateSpiderCartesianChart(SpiderInfos.SpiderSourceName.Local);

                    //番号搜刮成功率
                    CidSuccessRate_Run.Text = $"{(totalCount - _failVideoNameList.Count) * 100 / totalCount}%";
                }
                //搜刮源尝试搜刮
                else if (progressPercent.State == SpiderInfos.SpiderStates.Awaiting)
                {
                    UpdateSpiderCartesianChart(progressPercent.SpiderSource);
                }

                //更新整体进度
                var currentCount = successVideoNameList.Count + _failVideoNameList.Count;
                overallProgress.Value = currentCount;
                percentProgress_TextBlock.Text = $"{currentCount * 100 / totalCount}%";
                countProgress_TextBlock.Text = $"{currentCount}/{totalCount}";

            });

            TotalProgress_TextBlock.Text = "正在使用搜刮源搜刮";

            await SearchAllInfoMultiTask(spiderSourceProgress);

            //完成
            ProgressRing_StackPanel.SetValue(Grid.ColumnSpanProperty, 1);
            SearchResult_StackPanel.Visibility = Visibility.Visible;
            SearchProgress_TextBlock.Visibility = Visibility.Collapsed;

            AllCount_Run.Text = _matchVideoResults.Count.ToString();
            VideoCount_Run.Text = videoCount.ToString();
            FailCount_Run.Text = _failVideoNameList.Count.ToString();

            if (!GetInfoFromNetwork.IsJavDbCookieVisible)
            {
                JavDbCookieVisiable_TeachingTip.IsOpen = true;
            }

            //显示总耗时
            SearchMessage_TextBlock.Text = $"⏱总耗时：{DateHelper.ConvertDoubleToLengthStr(DateTimeOffset.Now.ToUnixTimeSeconds() - startTime)}";

            TopProgressRing.IsActive = false;
        }

        /// <summary>
        /// 每个搜刮源分配一个线程（数据来源与本地数据库）
        /// </summary>
        /// <returns></returns>
        private async Task SearchAllInfoMultiTask(IProgress<SpiderInfo> progress)
        {
            // 挑选出启动的
            var tasks = (
                from item
                    in _spiderInfos.Where(item => item.IsEnable)
                where item.SpiderSource != SpiderInfos.SpiderSourceName.Local
                select item
            ).ToList();

            ////等待任务完成
            //await Task.WhenAll(tasks);

            var spiderManager = Manager.Instance;
            foreach (var matchResult in _matchVideoResults.Where(i=>i.statusCode is 1))
            {
                var name = matchResult.MatchName;

                var result = DataAccess.Get.GetOneTrueNameByName(name);

                // 数据库没有
                if (!string.IsNullOrEmpty(result)) continue;

                foreach (var spiderInfo in tasks)
                {
                    var videoInfo = await spiderManager.DispatchSpecificSpiderInfoByCid(name, spiderInfo.SpiderSource, s_cts.Token);
                    if (videoInfo is null) continue;

                    // 添加搜刮信息到数据库（只有从搜刮源查找到的才添加）
                    await DataAccess.Add.AddVideoInfo_ActorInfo_IsWmAsync(videoInfo); 
                    break;
                }
            }

            // 数据库源最后完成
            SpiderInfo currentSpiderInfo = new(SpiderInfos.SpiderSourceName.Local, "完成", SpiderInfos.SpiderStates.Done);
            progress.Report(currentSpiderInfo);
        }

        //锁
        private static readonly object MyLock = new();

        // 创建SpiderTask
        private async Task StartSearchTask(SpiderInfo spiderInfo, IProgress<SpiderInfo> progress)
        {
            var spiderManager = Manager.Instance;

            while (true)
            {

                // 从队列中查找cid
                MatchVideoResult matchResult;
                lock (MyLock)
                {
                    // 查找status==0的
                    matchResult = _matchVideoResults.FirstOrDefault(result => result.statusCode is 2);
                }

                if (matchResult == null) break;

                var name = matchResult.MatchName;

                var result = DataAccess.Get.GetOneTrueNameByName(name);

                // 数据库没有
                if (string.IsNullOrEmpty(result))
                {
                    matchResult.statusCode = 3;
                    progress.Report(new SpiderInfo(spiderInfo.SpiderSource, name, SpiderInfos.SpiderStates.Doing));

                    var videoInfo = await spiderManager.DispatchSpecificSpiderInfoByCid(name, spiderInfo.SpiderSource, s_cts.Token);

                    // 没搜索到
                    if (videoInfo is null)
                    {
                        matchResult.statusCode = 2;
                    }
                    else
                    {
                        // 添加搜刮信息到数据库（只有从搜刮源查找到的才添加）
                        await DataAccess.Add.AddVideoInfo_ActorInfo_IsWmAsync(videoInfo);
                        matchResult.statusCode = 4;
                    }
                }
                else
                {
                    matchResult.statusCode = 4;
                }

                matchResult.statusCode = -1;
                progress.Report(new SpiderInfo(spiderInfo.SpiderSource, name, SpiderInfos.SpiderStates.Done));
            }


            //该搜刮源已结束


            spiderInfo.State = SpiderInfos.SpiderStates.Done;
            spiderInfo.Message = "完成";
            progress.Report(spiderInfo);

            //SpiderInfo currentSpiderInfo;
            //while (true)
            //{
            //    VideoInfo resultInfo;
            //    if (s_cts.IsCancellationRequested) return;

            //    string name;

            //    if (string.IsNullOrEmpty(name))
            //    {
            //        currentSpiderInfo = new SpiderInfo(spiderName, name)
            //        {
            //            State = SpiderInfos.SpiderStates.Doing,
            //            Message = "等待分配任务"
            //        };
            //        progress.Report(currentSpiderInfo);

            //        var isExistCurrentSpiderTask = true;

            //        //循环查询，最多100次
            //        for (var i = 0; i < 100; i++)
            //        {
            //            //确认正在进行搜刮的番号已经被搜刮源搜刮过，避免发生只有该搜刮源能搜刮到的情况
            //            var leftWaitCount = DataAccess.Get.GetWaitSpiderTaskCount(spiderName);

            //            if (leftWaitCount == 0)
            //            {
            //                //退出该搜刮任务
            //                break;
            //            }

            //            await Task.Delay(5000);

            //            lock (MyLock)
            //            {
            //                //再次查询待搜刮的name
            //                name = DataAccess.Get.GetOneSpiderTask(spiderName, null);
            //            }

            //            //查询到了，退出循环，开始任务
            //            if (!string.IsNullOrEmpty(name))
            //            {
            //                isExistCurrentSpiderTask = false;
            //                break;
            //            }

            //            //查询不到，下一循环继续查询

            //        }

            //        //退出整个任务
            //        if (isExistCurrentSpiderTask)
            //            break;
            //    }

            //    var result = DataAccess.Get.GetOneTrueNameByName(name);

            //    //如果数据库已存在该数据，直接从数据库中读取
            //    if (!string.IsNullOrEmpty(result))
            //    {
            //        currentSpiderInfo = new SpiderInfo(Models.Spider.SpiderInfos.SpiderSourceName.Local, name)
            //        {
            //            State = Models.Spider.SpiderInfos.SpiderStates.Doing,
            //            Message = name
            //        };

            //        //不汇报,两次汇报间隔时间太短,会将第二次的数据重复提交两次
            //        //progress.Report(currentSpiderInfo);

            //        //使用第一个符合条件的Name
            //        resultInfo = DataAccess.Get.GetSingleVideoInfoByTrueName(result);

            //        DataAccess.Update.UpdateFileToInfo(name, true);
            //    }
            //    //数据库没有，则开始搜刮
            //    else
            //    {
            //        currentSpiderInfo = new SpiderInfo(spiderSourceName, name)
            //        {
            //            State = Models.Spider.SpiderInfos.SpiderStates.Doing,
            //            Message = name
            //        };
            //        progress.Report(currentSpiderInfo);

            //        //FC2且cookie异常（如未登录）
            //        if (name.Contains("FC2") && spiderSourceName == SpiderInfos.SpiderSourceName.Javdb && !GetInfoFromNetwork.IsJavDbCookieVisible)
            //        {
            //            break;
            //        }

            //        //不同搜刮源用不同的方式搜刮,并等待对应的时间
            //        resultInfo = await spiderManager.DispatchSpecificSpiderInfoByCid(name, spiderSourceName, default);
            //        currentSpiderInfo.State = Models.Spider.SpiderInfos.SpiderStates.Awaiting;
            //        currentSpiderInfo.Message = $"等待 {spiderSource.DelayRanges.Item1}~{spiderSource.DelayRanges.Item2} s";
            //        progress.Report(currentSpiderInfo);
            //        await GetInfoFromNetwork.RandomTimeDelay(spiderSource.DelayRanges.Item1, spiderSource.DelayRanges.Item2);

            //        // 添加搜刮信息到数据库（只有从搜刮源查找到的才添加）
            //        if (resultInfo != null) await DataAccess.Add.AddVideoInfo_ActorInfo_IsWmAsync(resultInfo);

            //    }

            //    //检查一下是否需要标记为全部完成
            //    //搜刮成功
            //    if (resultInfo != null)
            //    {
            //        //宣告成功(数据库或搜刮源)
            //        currentSpiderInfo.RequestStates = RequestStates.success;
            //        currentSpiderInfo.Message = "搜刮完成";
            //        //System.Diagnostics.Debug.WriteLine($"<<发送:{currentSpiderInfo.Name} - {currentSpiderInfo.SpiderSource}<{spiderSource.Name}> - {currentSpiderInfo.RequestStates} - {currentSpiderInfo.Message}");
            //        progress.Report(currentSpiderInfo);

            //        lock (MyLock)
            //        {
            //            //记录为已完成且已全部完成
            //            DataAccess.Update.UpdateSpiderTask(name, spiderSource, Models.Spider.SpiderInfos.SpiderStates.Done, true);
            //        }

            //        //更新FileToInfo表
            //        DataAccess.Update.UpdateFileToInfo(name, true);
            //    }
            //    else
            //    {
            //        //检查是否还有其他搜刮源未尝试，全部源都尝试过就标记为AllDone,并宣告该番号搜刮失败
            //        bool IsAllSpiderSourceAttempt = false;
            //        lock (MyLock)
            //        {
            //            //仅记录该搜刮源为已完成
            //            DataAccess.Update.UpdateSpiderTask(name, spiderSource, Models.Spider.SpiderInfos.SpiderStates.Done);
            //            IsAllSpiderSourceAttempt = DataAccess.Get.GetIsAllSpiderSourceAttempt(name);
            //        }

            //        if (IsAllSpiderSourceAttempt)
            //        {
            //            lock (MyLock)
            //            {
            //                //标记为AllDone
            //                DataAccess.Update.UpdateSpiderTask(name, spiderSource, Models.Spider.SpiderInfos.SpiderStates.Done, true);
            //            }

            //            //宣告失败(搜刮源)
            //            currentSpiderInfo.RequestStates = RequestStates.fail;
            //            currentSpiderInfo.State = Models.Spider.SpiderInfos.SpiderStates.Ready;
            //            currentSpiderInfo.Message = "搜刮失败";
            //            progress.Report(currentSpiderInfo);
            //        }
            //    }

            //    //继续下一个搜刮
            //}

            ////该搜刮源已结束
            //currentSpiderInfo.State = SpiderInfos.SpiderStates.Done;
            //currentSpiderInfo.Message = "完成";
            //progress.Report(currentSpiderInfo);
        }

        /// <summary>
        /// 当前窗口请求关闭，显示关闭提示，如确定关闭则退出当前所有进程
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private async void CurrentWindow_Closed(object sender, WindowEventArgs args)
        {
            args.Handled = true;
            var window = (sender as Window);

            var dialog = new ContentDialog
            {
                // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
                XamlRoot = XamlRoot,
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                Title = "确认",
                PrimaryButtonText = "关闭",
                CloseButtonText = "返回",
                DefaultButton = ContentDialogButton.Primary,
                Content = "关闭窗口后将取消当前任务，是否继续关闭"
            };

            var result = await dialog.ShowAsync();
            if (result != ContentDialogResult.Primary) return;

            await s_cts.CancelAsync();

            if (window == null) return;
            window.Closed -= CurrentWindow_Closed;
            window.Close();
        }

        private void ResetMatchCountInfo(List<MatchVideoResult> matchVideoResults)
        {
            overallProgress.Maximum = matchVideoResults.Count;
            countProgress_TextBlock.Text = $"{overallProgress.Value}/{matchVideoResults.Count}";
            if (matchVideoResults.Count == 0)
            {
                percentProgress_TextBlock.Text = $"100%";
            }
        }

        #region 鼠标手势变化
        private void FailCountTextBlock_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Hand);
        }
        private void FailCountTextBlock_PointerExited(object sender, PointerRoutedEventArgs e)
        {

            ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Arrow);
        }
        #endregion

        /// <summary>
        /// 点击“失败数：xx”显示失败列表
        /// </summary>
        /// <param Name="sender"></param>
        /// <param Name="e"></param>
        private async void askLookFailResult_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ContentDialog dialog = new ContentDialog
            {
                // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
                XamlRoot = XamlRoot,
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                //dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
                Title = "失败列表",
                CloseButtonText = "返回",
                DefaultButton = ContentDialogButton.Close
            };

            var ContentGrid = new Grid();
            ContentGrid.Children.Add(new ListView() { ItemsSource = _failVideoNameList });
            dialog.Content = ContentGrid;

            await dialog.ShowAsync();
        }

        /// <summary>
        /// 点击了进度中的更多
        /// </summary>
        /// <param Name="sender"></param>
        /// <param Name="e"></param>
        private void ProgressMore_Click(object sender, RoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }

        public void CreateWindow()
        {
            CommonWindow window = new CommonWindow("搜刮进度");
            this.CurrentWindow = window;
            window.SetWindowSize(950, 730);
            window.Content = this;
            window.Activate();
        }
    }
}
