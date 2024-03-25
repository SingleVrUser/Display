// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Display.CustomWindows;
using Display.Helper.FileProperties.Name;
using Display.Managers;
using Display.Models.Data;
using Display.Models.Data.Enums;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WinUIEx;

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
        private List<Datum> _fileList = [];
        private readonly List<FailDatum> _failDatumList = [];

        private List<MatchVideoResult> _matchVideoResults;
        //private GetInfoFromNetwork _network;
        //private List<SpiderInfo> _spiderInfos;
        //private List<string> _failVideoNameList;

        public Window CurrentWindow;

        public Progress(List<string> selectedFilesNameList, List<Datum> fileList)
        {
            this.InitializeComponent();
            this._selectedFilesNameList = selectedFilesNameList;
            this._fileList = fileList;
            this.Loaded += PageLoaded;
        }

        public Progress(List<FailDatum> failDatumList)
        {
            this.InitializeComponent();
            this._failDatumList = failDatumList;
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

            ////显示进度环
            //ShowProgress(_matchVideoResults.Count);

            if (s_cts.IsCancellationRequested) return;
            await SpiderVideoInfo();
            if (s_cts.IsCancellationRequested) return;

            CurrentWindow.Closed -= CurrentWindow_Closed;

            TopProgressRing.IsActive = false;
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
            _fileList = _fileList.Where(item => item.Fid != null).ToList();

            //去除重复文件
            var newDictList = new Dictionary<string, Datum>();
            _fileList.ForEach(item => newDictList.TryAdd(item.PickCode, item));

            _fileList = newDictList.Values.ToList();

            //显示饼状图
            ShowFilesPieCharts(_fileList);
            if (s_cts.IsCancellationRequested) return;

            TotalProgress_TextBlock.Text = "正则匹配番号名中……";

            //挑选符合条件的视频文件
            _matchVideoResults = await Task.Run(() => FileMatch.GetVideoAndMatchFile(_fileList));

            var totalCount = _matchVideoResults.Where(item => !string.IsNullOrEmpty(item.MatchName)).ToList().Count;

            ////显示进度环
            //ShowProgress(totalCount);

            TopProgressRing.IsActive = false;
        }

        //private void ShowProgress(int totalCount)
        //{
        //    //初始化进度环
        //    ProgressRing_Grid.Visibility = Visibility.Visible;
        //    overallProgress.Maximum = totalCount;
        //    overallProgress.Value = 0;
        //    countProgress_TextBlock.Text = $"0/{totalCount}";

        //    //初始化显示信息
        //    SearchProgress_TextBlock.Visibility = Visibility.Visible;
        //    SearchResult_StackPanel.Visibility = Visibility.Collapsed;
        //    ProgressRing_StackPanel.SetValue(Grid.ColumnSpanProperty, 2);

        //}

        //private void ShowSpiderInfoList()
        //{
        //    CartesianChart.Visibility = Visibility.Visible;

        //    _spiderInfos = SpiderManager.Spiders.Select(spider => new SpiderInfo(spider)).ToList();

        //    //按IsEnable排序
        //    _spiderInfos = _spiderInfos.OrderByDescending(item => item.IsEnable).ToList();

        //    SpiderInfo_GridView.ItemsSource = _spiderInfos;
        //}

        ///// <summary>
        ///// 显示各个搜刮源搜刮数量柱状图
        ///// </summary>
        //private void ShowSpiderCartesianChart()
        //{
        //    if (_spiderInfos == null || _spiderInfos.Count == 0) return;

        //    CartesianChart.Visibility = Visibility.Visible;

        //    var spiderSourceReady = _spiderInfos.Where(item => item.IsEnable).ToList();

        //    ISeries[] series =
        //            spiderSourceReady
        //                .Select(x => new RowSeries<ObservableValue>
        //                {
        //                    Values = new[] { new ObservableValue(x.SpiderCount) },
        //                    Name = x.SpiderSource.ToString(),
        //                    Stroke = null,
        //                    MaxBarWidth = 25,
        //                    DataLabelsPaint = new SolidColorPaint(new SKColor(245, 245, 245)),
        //                    DataLabelsPosition = DataLabelsPosition.End,
        //                    DataLabelsTranslate = new LvcPoint(-1, 0),
        //                    DataLabelsFormatter = point => $"{point.Context.Series.Name} {point.PrimaryValue}"
        //                })
        //                .OrderByDescending(x => ((ObservableValue[])x.Values!)[0].Value)
        //                .ToArray();
        //    Axis[] xAxes =
        //    [
        //        new Axis { SeparatorsPaint = new SolidColorPaint(new SKColor(220, 220, 220)) }
        //    ];
        //    Axis[] yAxes =
        //    [
        //        new Axis { IsVisible = false }
        //    ];

        //    CartesianChart.Series = series;
        //    CartesianChart.XAxes = xAxes;
        //    CartesianChart.YAxes = yAxes;
        //}

        //private void UpdateSpiderCartesianChart(SpiderInfos.SpiderSourceName spiderSource)
        //{
        //    var item = CartesianChart.Series.FirstOrDefault(item => item.Name == spiderSource.ToString());

        //    ((ObservableValue[])item.Values)[0].Value += 1;
        //}

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
        private Task SpiderVideoInfo()
        {
            if (_matchVideoResults == null)
                return Task.CompletedTask;

            //_network ??= new GetInfoFromNetwork();
            //ShowSpiderInfoList();
            //ShowSpiderCartesianChart();
            //CountInfo_Grid.Visibility = Visibility.Visible;
            //TopProgressRing.IsActive = true;
            //_failVideoNameList = [];

            //var startTime = DateTimeOffset.Now.ToUnixTimeSeconds();
            //ProgressMore_TextBlock.Text = "失败数：0";
            ////视频数量
            //var videoCount = _matchVideoResults.Count(info => info.statusCode != 0);
            //AllVideoCount_Run.Text = videoCount.ToString();

            //if (videoCount == 0)
            //{
            //    TotalProgress_TextBlock.Text = "视频数为0,停止任务";
            //    return;
            //}

            ////匹配成功的视频总数量（1：匹配成功，2：已添加（多集只保留一个番号））
            //var matchSuccessVideoCount = _matchVideoResults.Where(item => item.statusCode is 1 or 2).ToList().Count;

            ////匹配到的番号总数量
            //var totalCount = _matchVideoResults.Where(item => !string.IsNullOrEmpty(item.MatchName)).ToList().Count;
            //MatchCidCount_Run.Text = totalCount.ToString();

            //if (totalCount == 0)
            //{
            //    TotalProgress_TextBlock.Text = "匹配到的番号数量为0,停止任务";
            //    return;
            //}

            ////正则匹配成功的番号占总视频数的
            //FileNameSuccessRate_Run.Text = $"{matchSuccessVideoCount * 100 / videoCount}%";

            ////统计成功的名称
            //var successCount = 0;
            //List<string> successVideoNameList = [];

            //var failCount = 0;

            //var spiderSourceProgress = new Progress<SpiderInfo>(progressPercent =>
            //{
            //    var gridViewItem = _spiderInfos.FirstOrDefault(item => item.SpiderSource == progressPercent.SpiderSource);
            //    //更新信息
            //    if (gridViewItem != null)
            //    {
            //        gridViewItem.State = progressPercent.State;
            //        gridViewItem.Message = progressPercent.Message;
            //    }

            //    //更新柱状图
            //    //请求成功 (搜刮源或数据库)
            //    if (progressPercent.RequestStates == RequestStates.success)
            //    {
            //        successCount++;
            //        CidSuccessCount_Run.Text = successCount.ToString();

            //        UpdateSpiderCartesianChart(SpiderInfos.SpiderSourceName.Local);
            //    }
            //    //请求失败(搜刮源)
            //    else if (progressPercent.RequestStates == RequestStates.fail)
            //    {
            //        failCount++;
            //        FailCount_Run.Text = failCount.ToString();
            //        ProgressMore_TextBlock.Text = $"失败数：{failCount}";
            //        UpdateSpiderCartesianChart(SpiderInfos.SpiderSourceName.Local);

            //        //番号搜刮成功率
            //        CidSuccessRate_Run.Text = $"{(totalCount - _failVideoNameList.Count) * 100 / totalCount}%";
            //    }
            //    //搜刮源尝试搜刮
            //    else if (progressPercent.State == SpiderInfos.SpiderStates.Awaiting)
            //    {
            //        UpdateSpiderCartesianChart(progressPercent.SpiderSource);
            //    }

            //    //更新整体进度
            //    var currentCount = successVideoNameList.Count + _failVideoNameList.Count;
            //    overallProgress.Value = currentCount;
            //    percentProgress_TextBlock.Text = $"{currentCount * 100 / totalCount}%";
            //    countProgress_TextBlock.Text = $"{currentCount}/{totalCount}";

            //});

            //TotalProgress_TextBlock.Text = "正在使用搜刮源搜刮";

            //await SearchAllInfoMultiTask(spiderSourceProgress);

            ////完成
            //ProgressRing_StackPanel.SetValue(Grid.ColumnSpanProperty, 1);
            //SearchResult_StackPanel.Visibility = Visibility.Visible;
            //SearchProgress_TextBlock.Visibility = Visibility.Collapsed;

            //AllCount_Run.Text = _matchVideoResults.Count.ToString();
            //VideoCount_Run.Text = videoCount.ToString();
            //FailCount_Run.Text = _failVideoNameList.Count.ToString();

            //if (!GetInfoFromNetwork.IsJavDbCookieVisible)
            //{
            //    JavDbCookieVisiable_TeachingTip.IsOpen = true;
            //}

            ////显示总耗时
            //SearchMessage_TextBlock.Text = $"⏱总耗时：{DateHelper.ConvertDoubleToLengthStr(DateTimeOffset.Now.ToUnixTimeSeconds() - startTime)}";
            //TopProgressRing.IsActive = false;

            var list = _matchVideoResults.Where(item => !string.IsNullOrEmpty(item.MatchName))
                .Select(match => match.MatchName).ToList();
            var spiderManager = App.GetService<SpiderManager>();
            spiderManager.AddTask(list);

            // 展示页面
            Tasks.MainPage.ShowSingleWindow(NavigationViewItemEnum.SpiderTask);

            TopProgressRing.IsActive = false;
            return Task.CompletedTask;
        }

        ///// <summary>
        ///// 每个搜刮源分配一个线程（数据来源与本地数据库）
        ///// </summary>
        ///// <returns></returns>
        //private async Task SearchAllInfoMultiTask(IProgress<SpiderInfo> progress)
        //{
        //    // 挑选出启动的
        //    var tasks = (
        //        from item
        //            in _spiderInfos.Where(item => item.IsEnable)
        //        where item.SpiderSource != SpiderInfos.SpiderSourceName.Local
        //        select item
        //    ).ToList();

        //    ////等待任务完成
        //    //await Task.WhenAll(tasks);


        //    var spiderManager = App.GetService<SpiderManager>();

        //    foreach (var matchResult in _matchVideoResults.Where(i=>i.statusCode is 1))
        //    {
        //        var name = matchResult.MatchName;

        //        var result = DataAccess.Get.GetOneTrueNameByName(name);

        //        // 数据库没有
        //        if (!string.IsNullOrEmpty(result)) continue;

        //        foreach (var spiderInfo in tasks)
        //        {
        //            var videoInfo = await spiderManager.DispatchSpecificSpiderInfoByCid(name, spiderInfo.SpiderSource, s_cts.Token);
        //            if (videoInfo is null) continue;

        //            // 添加搜刮信息到数据库（只有从搜刮源查找到的才添加）
        //            await DataAccess.Add.AddVideoInfo_ActorInfo_IsWmAsync(videoInfo); 
        //            break;
        //        }
        //    }

        //    // 数据库源最后完成
        //    SpiderInfo currentSpiderInfo = new(SpiderInfos.SpiderSourceName.Local, "完成", SpiderInfos.SpiderStates.Done);
        //    progress.Report(currentSpiderInfo);
        //}

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

        //private void ResetMatchCountInfo(List<MatchVideoResult> matchVideoResults)
        //{
        //    overallProgress.Maximum = matchVideoResults.Count;
        //    countProgress_TextBlock.Text = $"{overallProgress.Value}/{matchVideoResults.Count}";
        //    if (matchVideoResults.Count == 0)
        //    {
        //        percentProgress_TextBlock.Text = $"100%";
        //    }
        //}

        //#region 鼠标手势变化
        //private void FailCountTextBlock_PointerEntered(object sender, PointerRoutedEventArgs e)
        //{
        //    ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Hand);
        //}
        //private void FailCountTextBlock_PointerExited(object sender, PointerRoutedEventArgs e)
        //{

        //    ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Arrow);
        //}
        //#endregion

        ///// <summary>
        ///// 点击“失败数：xx”显示失败列表
        ///// </summary>
        ///// <param Name="sender"></param>
        ///// <param Name="e"></param>
        //private async void askLookFailResult_Tapped(object sender, TappedRoutedEventArgs e)
        //{
        //    var dialog = new ContentDialog
        //    {
        //        // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
        //        XamlRoot = XamlRoot,
        //        Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
        //        Title = "失败列表",
        //        CloseButtonText = "返回",
        //        DefaultButton = ContentDialogButton.Close
        //    };

        //    var contentGrid = new Grid();
        //    contentGrid.Children.Add(new ListView() { ItemsSource = _failVideoNameList });
        //    dialog.Content = contentGrid;

        //    await dialog.ShowAsync();
        //}

        ///// <summary>
        ///// 点击了进度中的更多
        ///// </summary>
        ///// <param Name="sender"></param>
        ///// <param Name="e"></param>
        //private void ProgressMore_Click(object sender, RoutedEventArgs e)
        //{
        //    FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        //}

        public void CreateWindow()
        {
            var window = new CommonWindow("匹配名称");
            CurrentWindow = window;
            window.SetWindowSize(950, 730);
            window.Content = this;
            window.Activate();
        }
    }
}
