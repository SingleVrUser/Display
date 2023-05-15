// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Display.WindowView;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Drawing;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Display.Data;
using Display.Spider;
using WinUIEx;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.ContentsPage.SpiderVideoInfo
{

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Progress : Page
    {
        CancellationTokenSource s_cts = new();
        List<MatchVideoResult> matchVideoResults;
        List<string> folderNameList = new();
        List<Datum> datumList = new();
        List<FailDatum> failDatumList = new();
        GetInfoFromNetwork network;
        List<SpiderInfo> SpiderInfos;
        List<string> FailVideoNameList;
        public Window currentWindow;

        public Progress(List<string> folderNameList, List<Datum> datumList)
        {
            this.InitializeComponent();
            this.folderNameList = folderNameList;
            this.datumList = datumList;
            this.Loaded += PageLoaded;
        }

        public Progress(List<FailDatum> failDatums)
        {
            this.InitializeComponent();
            this.failDatumList = failDatums;
            this.Loaded += ReSpiderPageLoaded;
        }

        private async void ReSpiderPageLoaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= ReSpiderPageLoaded;

            if (failDatumList == null || failDatumList.Count == 0) return;

            currentWindow.Closed += CurrentWindow_Closed;
            if (matchVideoResults == null) matchVideoResults = new();
            foreach (var item in failDatumList)
            {
                matchVideoResults.Add(new MatchVideoResult() { status = true, OriginalName = item.Datum.n, message = "匹配成功", statusCode = 1, MatchName = item.MatchName });

                //替换数据库的数据
                DataAccess.AddFileToInfo(item.Datum.pc, item.MatchName, isReplace: true);
            }

            //显示进度环
            ShowProgress(matchVideoResults.Count);

            if (s_cts.IsCancellationRequested) return;
            await SpliderVideoInfo(matchVideoResults);
            if (s_cts.IsCancellationRequested) return;


            currentWindow.Closed -= CurrentWindow_Closed;

            TopProgressRing.IsActive = false;
            TotalProgress_TextBlock.Text = "完成";
        }

        private async void PageLoaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= PageLoaded;

            currentWindow.Closed += CurrentWindow_Closed;

            await ShowMatchResult();

            if (s_cts.IsCancellationRequested) return;

            await SpliderVideoInfo(matchVideoResults);

            if (s_cts.IsCancellationRequested) return;

            currentWindow.Closed -= CurrentWindow_Closed;

            TopProgressRing.IsActive = false;
            TotalProgress_TextBlock.Text = "完成";
        }

        /// <summary>
        /// 显示正则匹配的结果
        /// </summary>
        /// <returns></returns>
        private async Task ShowMatchResult()
        {
            if (datumList == null) return;

            TopProgressRing.IsActive = true;

            //目前datumList仅有一级目录文件
            //遍历获取文件列表中所有的文件
            await Task.Run(() => datumList = DataAccess.GetAllFilesInFolderList(datumList));

            //除去文件夹
            datumList = datumList.Where(item => !string.IsNullOrEmpty(item.fid)).ToList();

            //去除重复文件
            Dictionary<string, Datum> newDictList = new();
            datumList.ForEach(item => newDictList.TryAdd(item.pc, item));

            datumList = newDictList.Values.ToList();

            //显示饼状图
            ShowFilesPieCharts(datumList);
            if (s_cts.IsCancellationRequested) return;

            TotalProgress_TextBlock.Text = "正则匹配番号名中……";

            //挑选符合条件的视频文件
            matchVideoResults = await Task.Run(() => FileMatch.GetVideoAndMatchFile(datumList));

            int totalCount = matchVideoResults.Where(item => !string.IsNullOrEmpty(item.MatchName)).ToList().Count;

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

            if (s_cts.IsCancellationRequested) return;
        }

        private void ShowSpiderInfoList()
        {
            CartesianChart.Visibility = Visibility.Visible;

            SpiderInfos = new List<SpiderInfo>() {
                new(Models.SpiderInfo.SpiderSourceName.Javbus, AppSettings.IsUseJavBus) ,
                new(Models.SpiderInfo.SpiderSourceName.Jav321, AppSettings.IsUseJav321),
                new(Models.SpiderInfo.SpiderSourceName.Avmoo, AppSettings.IsUseAvMoo),
                new(Models.SpiderInfo.SpiderSourceName.Avsox, AppSettings.IsUseAvSox),
                new(Models.SpiderInfo.SpiderSourceName.Libredmm, AppSettings.IsUseLibreDmm),
                new(Models.SpiderInfo.SpiderSourceName.Fc2club, AppSettings.IsUseFc2Hub),
                new(Models.SpiderInfo.SpiderSourceName.Javdb, AppSettings.IsUseJavDb),
                new(Models.SpiderInfo.SpiderSourceName.Local, true)
                };

            //按IsEnable排序
            SpiderInfos = SpiderInfos.OrderByDescending(item => item.IsEnable).ToList();

            SpiderInfo_GridView.ItemsSource = SpiderInfos;
        }

        /// <summary>
        /// 显示各个搜刮源搜刮数量柱状图
        /// </summary>
        private void ShowSpiderCartesianChart()
        {
            if (SpiderInfos == null || SpiderInfos.Count == 0) return;

            CartesianChart.Visibility = Visibility.Visible;

            var SpiderSoureReady = SpiderInfos.Where(item => item.IsEnable).ToList();

            ISeries[] Series =
                    SpiderSoureReady
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
            Axis[] XAxes = new Axis[]
            {
                new Axis { SeparatorsPaint = new SolidColorPaint(new SKColor(220, 220, 220)) }
            };
            Axis[] YAxes = new Axis[]
            {
                new Axis { IsVisible = false }
            };

            CartesianChart.Series = Series;
            CartesianChart.XAxes = XAxes;
            CartesianChart.YAxes = YAxes;
        }

        private void UpdateSpiderCartesianChart(Models.SpiderInfo.SpiderSourceName spiderSource)
        {
            var item = CartesianChart.Series.Where(item => item.Name == spiderSource.ToString()).FirstOrDefault();

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
            FileStatistics VideoInfo = new FileStatistics(FileFormat.Video);
            FileStatistics SubInfo = new FileStatistics(FileFormat.Subtitles);
            FileStatistics TorrentInfo = new FileStatistics(FileFormat.Torrent);
            FileStatistics ImageInfo = new FileStatistics(FileFormat.Image);
            FileStatistics AudioInfo = new FileStatistics(FileFormat.Audio);
            //FileStatistics ArchiveInfo = new FileStatistics(FileFormat.Archive);

            foreach (var info in datumList)
            {
                //文件夹，暂不统计
                if (info.fid == "")
                {

                }
                //视频文件
                else if (info.iv == 1)
                {
                    string video_qulity = FilesInfo.GetVideoQualityFromVdi(info.vdi);

                    UpdateFileStaticstics(info, VideoInfo, video_qulity);
                }
                //其他文件
                else
                {
                    switch (info.ico)
                    {
                        //字幕
                        case "ass" or "srt" or "ssa":
                            UpdateFileStaticstics(info, SubInfo, info.ico);
                            break;
                        //种子
                        case "torrent":
                            UpdateFileStaticstics(info, TorrentInfo, info.ico);
                            break;
                        //图片
                        case "gif" or "bmp" or "tiff" or "exif" or "jpg" or "png" or "raw" or "svg":
                            UpdateFileStaticstics(info, ImageInfo, info.ico);
                            break;
                        //音频
                        case "ape" or "wav" or "midi" or "mid" or "flac" or "aac" or "m4a" or "ogg" or "amr":
                            UpdateFileStaticstics(info, AudioInfo, info.ico);
                            break;
                            //case "7z" or "cab" or "dmg" or "iso" or "rar" or "zip":
                            //    UpdateFileStaticstics(info, ArchiveInfo, info.ico);
                            //    break;
                    }
                }
            }

            CountPercentPieChart.Series = new ISeries[] {
                new PieSeries<double> { Values = new List<double> { VideoInfo.count }, Pushout = 5, Name = "视频"},
                new PieSeries<double> { Values = new List<double> { AudioInfo.count }, Pushout = 0, Name = "音频"},
                new PieSeries<double> { Values = new List<double> { SubInfo.count }, Pushout = 0, Name = "字幕"},
                new PieSeries<double> { Values = new List<double> { TorrentInfo.count }, Pushout = 0, Name = "种子"},
                new PieSeries<double> { Values = new List<double> { ImageInfo.count }, Pushout = 0, Name = "图片"}
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
            TypeInfo.size += DataInfo.s;
            TypeInfo.count++;

            if (TypeInfo.data.Count != 0)
            {
                var tmpData = TypeInfo.data.Where(x => x.name == Name).FirstOrDefault();

                if (tmpData == null)
                {
                    TypeInfo.data.Add(new FileStatistics.Data() { name = Name, count = 1, size = DataInfo.s });
                }
                else
                {
                    tmpData.size += DataInfo.s;
                    tmpData.count++;
                }
            }
            else
            {
                TypeInfo.data.Add(new FileStatistics.Data() { name = Name, count = 1, size = DataInfo.s });
            }
        }

        /// <summary>
        /// 开始从网络中检索视频信息
        /// </summary>
        private async Task SpliderVideoInfo(List<MatchVideoResult> matchVideoResults)
        {
            if (matchVideoResults == null)
                return;

            network ??= new GetInfoFromNetwork();

            ShowSpiderInfoList();
            ShowSpiderCartesianChart();

            CountInfo_Grid.Visibility = Visibility.Visible;
            TopProgressRing.IsActive = true;

            FailVideoNameList = new();

            var startTime = DateTimeOffset.Now.ToUnixTimeSeconds();

            ProgressMore_TextBlock.Text = $"失败数：0";

            //记录到SpiderLog
            long task_id = DataAccess.InsertSpiderLog();

            //记录到SpiderTask
            foreach (var item in matchVideoResults)
            {
                if (item.MatchName == null)
                    continue;

                DataAccess.AddSpiderTask(item.MatchName, task_id);
            }

            //视频数量
            int videoCount = matchVideoResults.Where(info => info.statusCode != 0).ToList().Count;
            AllVideoCount_Run.Text = videoCount.ToString();

            if (videoCount == 0)
            {
                TotalProgress_TextBlock.Text = "视频数为0,停止任务";
                return;
            }

            //匹配成功的视频总数量（1：匹配成功，2：已添加（多集只保留一个番号））
            int matchSuccessVideoCount = matchVideoResults.Where(item => item.statusCode == 1 || item.statusCode == 2).ToList().Count;

            //匹配到的番号总数量
            int totalCount = matchVideoResults.Where(item => !string.IsNullOrEmpty(item.MatchName)).ToList().Count;
            MatchCidCount_Run.Text = totalCount.ToString();

            if (totalCount == 0)
            {
                TotalProgress_TextBlock.Text = "匹配到的番号数量为0,停止任务";
                return;
            }

            //正则匹配成功的番号占总视频数的
            FileNameSuccessRate_Run.Text = $"{matchSuccessVideoCount * 100 / videoCount}%";

            //统计成功的名称
            int successCount = 0;
            List<string> successVIdeoNameList = new();

            int failCount = 0;

            int i = 0;
            var SpiderSourceProgress = new Progress<SpiderInfo>(progressPercent =>
            {
                var GridViewItem = SpiderInfos.Where(item => item.SpiderSource == progressPercent.SpiderSource).FirstOrDefault();
                //更新信息
                GridViewItem.State = progressPercent.State;
                GridViewItem.Message = progressPercent.Message;

                //更新柱状图
                //请求成功 (搜刮源或数据库)
                if (progressPercent.RequestStates == RequestStates.success)
                {
                    successCount++;
                    CidSuccessCount_Run.Text = successCount.ToString();
                    successVIdeoNameList.Add(progressPercent.Name);

                    UpdateSpiderCartesianChart(Models.SpiderInfo.SpiderSourceName.Local);
                }
                //请求失败(搜刮源)
                else if (progressPercent.RequestStates == RequestStates.fail)
                {
                    failCount++;
                    FailVideoNameList.Add(progressPercent.Name);
                    FailCount_Run.Text = failCount.ToString();
                    ProgressMore_TextBlock.Text = $"失败数：{failCount}";
                    UpdateSpiderCartesianChart(Models.SpiderInfo.SpiderSourceName.Local);

                    //番号搜刮成功率
                    CidSuccessRate_Run.Text = $"{(totalCount - FailVideoNameList.Count) * 100 / totalCount}%";
                }
                //搜刮源尝试搜刮
                else if (progressPercent.State == SpiderStates.awaiting)
                {
                    UpdateSpiderCartesianChart(progressPercent.SpiderSource);
                }

                i++;
                //System.Diagnostics.Debug.WriteLine($">>>>>>>>>>>>>>>>>>>>>>>>>接受:{i} - {progressPercent.Name} - {progressPercent.SpiderSource} - {progressPercent.RequestStates} - {progressPercent.Message}");

                //更新整体进度
                var currentCount = successVIdeoNameList.Count + FailVideoNameList.Count;
                overallProgress.Value = currentCount;
                percentProgress_TextBlock.Text = $"{currentCount * 100 / totalCount}%";
                countProgress_TextBlock.Text = $"{currentCount}/{totalCount}";

            });

            TotalProgress_TextBlock.Text = "正在使用搜刮源搜刮";

            await SearchAllInfoMultiTask(task_id, SpiderSourceProgress);

            //完成
            ProgressRing_StackPanel.SetValue(Grid.ColumnSpanProperty, 1);
            SearchResult_StackPanel.Visibility = Visibility.Visible;
            SearchProgress_TextBlock.Visibility = Visibility.Collapsed;

            AllCount_Run.Text = matchVideoResults.Count.ToString();
            VideoCount_Run.Text = videoCount.ToString();
            FailCount_Run.Text = FailVideoNameList.Count.ToString();

            if (!GetInfoFromNetwork.IsJavDbCookieVisible)
            {
                JavDbCookieVisiable_TeachingTip.IsOpen = true;
            }

            //显示总耗时
            SearchMessage_TextBlock.Text = $"⏱总耗时：{FileMatch.ConvertDoubleToDateStr(DateTimeOffset.Now.ToUnixTimeSeconds() - startTime)}";

            TopProgressRing.IsActive = false;
        }

        /// <summary>
        /// 每个搜刮源分配一个线程（数据来源与本地数据库）
        /// </summary>
        /// <returns></returns>
        private async Task SearchAllInfoMultiTask(long task_id, IProgress<SpiderInfo> progress)
        {
            List<Task> tasks = new List<Task>();

            foreach (var item in SpiderInfos.Where(item => item.IsEnable))
            {
                if (item.SpiderSource == Models.SpiderInfo.SpiderSourceName.Local) continue;

                tasks.Add(Task.Run(() => CreadSpiderTask(item.SpiderSource, progress)));
            }

            //等待任务完成
            await Task.WhenAll(tasks);

            //数据库源最后完成
            SpiderInfo currentSpiderInfo = new(Models.SpiderInfo.SpiderSourceName.Local);
            currentSpiderInfo.State = SpiderStates.done;
            currentSpiderInfo.Message = "完成";
            progress.Report(currentSpiderInfo);

            ////所有任务已结束（后面已经删除了,所以这个没用了）
            //DataAccess.UpdateSpiderLogDone(task_id);

            //删除SpiderTask和SpiderLog表中的所有数据
            DataAccess.DeleteSpiderLogTable();
            DataAccess.DeleteSpiderTaskTable();

        }

        //锁
        private static object myLock = new object();

        /// <summary>
        /// 创建SpiderTask
        /// </summary>
        /// <param Name="spiderSourceName"></param>
        /// <returns></returns>
        private async Task CreadSpiderTask(Models.SpiderInfo.SpiderSourceName spiderSourceName, IProgress<SpiderInfo> progress)
        {
            string name = null;
            VideoInfo resultInfo;
            Manager.SpiderSource spiderSource = new(spiderSourceName);

            var spiderManager = Manager.Current;

            SpiderInfo currentSpiderInfo;
            while (true)
            {
                resultInfo = null;
                if (s_cts.IsCancellationRequested) return;

                lock (myLock)
                {
                    //查询待搜刮的name
                    name = DataAccess.GetOneSpiderTask(spiderSource);

                    if (!string.IsNullOrEmpty(name))
                    {
                        //System.Diagnostics.Debug.WriteLine($"{spiderSourceName}查询到的{Name}");

                        //记录为正在进行
                        DataAccess.UpdateSpiderTask(name, spiderSource, SpiderStates.doing);
                    }
                }

                if (string.IsNullOrEmpty(name))
                {
                    currentSpiderInfo = new(spiderSourceName, name);
                    currentSpiderInfo.State = SpiderStates.doing;
                    currentSpiderInfo.Message = "等待分配任务";
                    progress.Report(currentSpiderInfo);

                    bool isExistCurrentSpiderTask = true;

                    //循环查询，最多100次
                    for (int i = 0; i < 100; i++)
                    {
                        //确认正在进行搜刮的番号已经被搜刮源搜刮过，避免发生只有该搜刮源能搜刮到的情况
                        var leftWaitCount = DataAccess.GetWaitSpiderTaskCount(spiderSource);

                        if (leftWaitCount == 0)
                        {
                            //退出该搜刮任务
                            break;
                        }
                        else
                        {
                            await Task.Delay(5000);

                            lock (myLock)
                            {
                                //再次查询待搜刮的name
                                name = DataAccess.GetOneSpiderTask(spiderSource);
                            }

                            //查询到了，退出循环，开始任务
                            if (!string.IsNullOrEmpty(name))
                            {
                                isExistCurrentSpiderTask = false;
                                break;
                            }

                            //查询不到，下一循环继续查询
                        }

                    }

                    //退出整个任务
                    if (isExistCurrentSpiderTask)
                        break;
                }

                var result = DataAccess.SelectTrueName(name);

                //如果数据库已存在该数据，直接从数据库中读取
                if (result.Count != 0)
                {
                    currentSpiderInfo = new(Models.SpiderInfo.SpiderSourceName.Local, name);
                    currentSpiderInfo.State = SpiderStates.doing;
                    currentSpiderInfo.Message = name;

                    //不汇报,两次汇报间隔时间太短,会将第二次的数据重复提交两次
                    //progress.Report(currentSpiderInfo);

                    //使用第一个符合条件的Name
                    resultInfo = DataAccess.LoadOneVideoInfoByCID(result[0]);

                    DataAccess.UpdateFileToInfo(name, true);
                }
                //数据库没有，则开始搜刮
                else
                {
                    currentSpiderInfo = new(spiderSourceName, name);
                    currentSpiderInfo.State = SpiderStates.doing;
                    currentSpiderInfo.Message = name;
                    progress.Report(currentSpiderInfo);

                    //FC2且cookie异常（如未登录）
                    if (name.Contains("FC2") && spiderSourceName == Models.SpiderInfo.SpiderSourceName.Javdb && !GetInfoFromNetwork.IsJavDbCookieVisible)
                    {
                        break;
                    }

                    //不同搜刮源用不同的方式搜刮,并等待对应的时间
                    resultInfo = await spiderManager.DispatchSpecificSpiderInfoByCID(name, (int)spiderSourceName);
                    currentSpiderInfo.State = SpiderStates.awaiting;
                    currentSpiderInfo.Message = $"等待 {spiderSource.DelayRanges.Item1}~{spiderSource.DelayRanges.Item2} s";
                    progress.Report(currentSpiderInfo);
                    await GetInfoFromNetwork.RandomTimeDelay(spiderSource.DelayRanges.Item1, spiderSource.DelayRanges.Item2);

                    // 添加搜刮信息到数据库（只有从搜刮源查找到的才添加）
                    if (resultInfo != null)
                        DataAccess.AddVideoInfo(resultInfo);

                }

                //检查一下是否需要标记为全部完成
                //搜刮成功
                if (resultInfo != null)
                {
                    //宣告成功(数据库或搜刮源)
                    currentSpiderInfo.RequestStates = RequestStates.success;
                    currentSpiderInfo.Message = "搜刮完成";
                    //System.Diagnostics.Debug.WriteLine($"<<发送:{currentSpiderInfo.Name} - {currentSpiderInfo.SpiderSource}<{spiderSource.Name}> - {currentSpiderInfo.RequestStates} - {currentSpiderInfo.Message}");
                    progress.Report(currentSpiderInfo);

                    lock (myLock)
                    {
                        //记录为已完成且已全部完成
                        DataAccess.UpdateSpiderTask(name, spiderSource, SpiderStates.done, true);
                    }

                    //更新FileToInfo表
                    DataAccess.UpdateFileToInfo(name, true);
                }
                else
                {
                    //检查是否还有其他搜刮源未尝试，全部源都尝试过就标记为AllDone,并宣告该番号搜刮失败
                    bool IsAllSpiderSourceAttempt = false;
                    lock (myLock)
                    {
                        //仅记录该搜刮源为已完成
                        DataAccess.UpdateSpiderTask(name, spiderSource, SpiderStates.done);
                        IsAllSpiderSourceAttempt = DataAccess.IsAllSpiderSourceAttempt(name);
                    }

                    if (IsAllSpiderSourceAttempt)
                    {
                        lock (myLock)
                        {
                            //标记为AllDone
                            DataAccess.UpdateSpiderTask(name, spiderSource, SpiderStates.done, true);
                        }

                        //宣告失败(搜刮源)
                        currentSpiderInfo.RequestStates = RequestStates.fail;
                        currentSpiderInfo.State = SpiderStates.ready;
                        currentSpiderInfo.Message = "搜刮失败";
                        progress.Report(currentSpiderInfo);
                    }
                }

                //继续下一个搜刮
            }

            //该搜刮源已结束
            currentSpiderInfo = new(spiderSourceName);
            currentSpiderInfo.State = SpiderStates.done;
            currentSpiderInfo.Message = "完成";
            progress.Report(currentSpiderInfo);

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

            ContentDialog dialog = new ContentDialog();

            // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
            dialog.XamlRoot = this.XamlRoot;
            dialog.Title = "确认";
            dialog.PrimaryButtonText = "关闭";
            dialog.CloseButtonText = "返回";
            dialog.DefaultButton = ContentDialogButton.Primary;
            dialog.Content = "关闭窗口后将取消当前任务，是否继续关闭";

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                s_cts.Cancel();

                window.Closed -= CurrentWindow_Closed;
                window.Close();
            }
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
            ContentDialog dialog = new ContentDialog();

            // XamlRoot must be set in the case of a ContentDialog running in a Desktop app

            dialog.XamlRoot = this.XamlRoot;
            //dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
            dialog.Title = "失败列表";
            dialog.CloseButtonText = "返回";
            dialog.DefaultButton = ContentDialogButton.Close;

            var ContentGrid = new Grid();
            ContentGrid.Children.Add(new ListView() { ItemsSource = FailVideoNameList });
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
            this.currentWindow = window;
            window.SetWindowSize(950, 730);
            window.Content = this;
            window.Activate();
        }
    }
}
