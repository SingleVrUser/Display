// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Data;
using Display.Control;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
        VideoInfo videoInfo = new VideoInfo();
        List<string> folderNameList = new();
        List<Datum> datumList = new();
        GetInfoFromNetwork network;
        List<string> FailVideoNameList;
        public Window currentWindow;

        public Progress(Window currentWindow, List<string> folderNameList, List<Datum> datumList)
        {
            this.InitializeComponent();

            this.currentWindow = currentWindow;
            this.folderNameList = folderNameList;
            this.datumList = datumList;

            this.Loaded += PageLoaded;
        }

        private async void PageLoaded(object sender, RoutedEventArgs e)
        {
            currentWindow.Closed += CurrentWindow_Closed;

            await ShowMatchResult();

            await SpliderVideoInfo(matchVideoResults);

            currentWindow.Closed -= CurrentWindow_Closed;
        }

        /// <summary>
        /// 显示正则匹配的结果
        /// </summary>
        /// <returns></returns>
        private async Task ShowMatchResult()
        {
            TopProgressBar.Visibility = Visibility.Visible;

            //目前datumList仅有一级目录文件
            //遍历获取文件列表中所有的文件
            datumList = await Task.Run(() => datumList = DataAccess.GetAllFilesInFolderList(datumList));

            //初始化进度环
            ProgressRing_Grid.Visibility = Visibility.Visible;
            overallProgress.Maximum = datumList.Count;
            overallProgress.Value = 0;
            countProgress_TextBlock.Text = $"0/{datumList.Count}";

            //初始化显示信息
            SearchProgress_TextBlock.Visibility = Visibility.Visible;
            SearchResult_StackPanel.Visibility = Visibility.Collapsed;
            ProgressRing_StackPanel.SetValue(Grid.ColumnSpanProperty, 2);

            ShowFilesPieCharts(datumList);

            if (s_cts.IsCancellationRequested)
            {
                return;
            }

            //挑选符合条件的视频文件
            matchVideoResults = await Task.Run(() => FileMatch.GetVideoAndMatchFile(datumList));

            if (s_cts.IsCancellationRequested)
            {
                return;
            }

            TopProgressBar.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// 显示饼形图
        /// </summary>
        /// <param name="datumList"></param>
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
        /// <param name="DataInfo"></param>
        /// <param name="TypeInfo"></param>
        /// <param name="Name"></param>
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
            if(network ==null)
                network = new();

            VideoInfo_Grid.Visibility = Visibility.Visible;
            TopProgressBar.Visibility = Visibility.Visible;
            //StartMatchName_Button.IsEnabled = false;
            FailVideoNameList = new();

            var startTime = DateTimeOffset.Now.ToUnixTimeSeconds();

            ProgressMore_TextBlock.Text = $"失败数：0";
            var progress = new Progress<SpliderInfoProgress>(progressPercent =>
            {
                //更新进度信息
                if (progressPercent.index != 0)
                {
                    overallProgress.Value = progressPercent.index;
                }

                tryUpdateVideoInfo(progressPercent.videoInfo);
                var matchResult = progressPercent.matchResult;

                //匹配失败/检索失败
                if (!matchResult.status)
                {
                    FailVideoNameList.Add(matchResult.OriginalName);

                    ProgressMore_TextBlock.Text = $"失败数：{FailVideoNameList.Count}";
                    SearchProgress_TextBlock.Text = $"{matchResult.OriginalName}";
                    SearchMessage_TextBlock.Text = $"❌{matchResult.message}";

                }
                //匹配成功/跳过非视频文件/跳过重复番号
                else
                {
                    if (matchResult.MatchName != null)
                    {
                        //匹配成功
                        if (matchResult.OriginalName != null)
                        {
                            SearchProgress_TextBlock.Text = $"{matchResult.OriginalName} => {matchResult.MatchName}";
                            SearchMessage_TextBlock.Text = $"✔{matchResult.message}";
                        }
                        //匹配中
                        else
                        {
                            SearchProgress_TextBlock.Text = $"{matchResult.MatchName}";
                            SearchMessage_TextBlock.Text = $"🐬{matchResult.message}";
                        }
                    }
                    // 其他
                    else
                    {
                        SearchProgress_TextBlock.Text = $"{matchResult.OriginalName}";
                        SearchMessage_TextBlock.Text = $"✨{matchResult.message}";
                    }
                }


                percentProgress_TextBlock.Text = $"{(int)overallProgress.Value * 100 / matchVideoResults.Count}%";
                countProgress_TextBlock.Text = $"{overallProgress.Value}/{matchVideoResults.Count}";

                //100%
                if (overallProgress.Value == overallProgress.Maximum)
                {
                    //Debug.WriteLine($"完成……");

                    ProgressRing_StackPanel.SetValue(Grid.ColumnSpanProperty, 1);
                    SearchResult_StackPanel.Visibility = Visibility.Visible;
                    SearchProgress_TextBlock.Visibility = Visibility.Collapsed;

                    AllCount_Run.Text = $"{matchVideoResults.Count}";
                    VideoCount_Run.Text = $"{matchVideoResults.Where(info => info.statusCode != 0).ToList().Count}";
                    FailCount_Run.Text = $"{FailVideoNameList.Count}";

                    //StartMatchName_Button.IsEnabled = true;
                    TopProgressBar.Visibility = Visibility.Collapsed;

                    //显示总耗时
                    SearchMessage_TextBlock.Text = $"⏱总耗时：{FileMatch.ConvertInt32ToDateStr(DateTimeOffset.Now.ToUnixTimeSeconds() - startTime)}";
                }
            });

            await SearchAllInfo(matchVideoResults, progress);
        }

        /// <summary>
        /// 搜索所有的信息
        /// </summary>
        /// <param name="matchVideoResults"></param>
        /// <param name="progress"></param>
        /// <returns></returns>
        private async Task SearchAllInfo(List<MatchVideoResult> matchVideoResults, IProgress<SpliderInfoProgress> progress)
        {
            for (int i = 0; i < matchVideoResults.Count; i++)
            {
                if (s_cts.IsCancellationRequested)
                {
                    return;
                }

                var matchResult = matchVideoResults[i];


                SpliderInfoProgress spliderInfoProgress = new();
                spliderInfoProgress.matchResult = matchResult;

                //存在匹配文件
                if (matchResult.MatchName != null)
                {
                    spliderInfoProgress.videoInfo = await SearchInfoByWeb(matchResult.MatchName, progress);

                    //检索失败
                    if (spliderInfoProgress.videoInfo == null)
                    {
                        spliderInfoProgress.matchResult.status = false;
                        spliderInfoProgress.matchResult.statusCode = -2;
                        spliderInfoProgress.matchResult.message = "检索失败";
                    }
                    //成功
                    else
                    {
                        spliderInfoProgress.matchResult.status = true;
                        spliderInfoProgress.matchResult.statusCode = 1;
                        spliderInfoProgress.matchResult.message = "检索成功";
                    }

                }

                spliderInfoProgress.index = i + 1;

                //获取到该信息，在UI上显示
                progress.Report(spliderInfoProgress);

            }
        }

        /// <summary>
        /// 更新显示的VideoInfo
        /// </summary>
        /// <param name="newInfo"></param>
        private void tryUpdateVideoInfo(VideoInfo newInfo)
        {
            if (newInfo == null) return;

            foreach (var VideoInfoItem in newInfo.GetType().GetProperties())
            {
                var key = VideoInfoItem.Name;
                var value = VideoInfoItem.GetValue(newInfo);

                var newItem = videoInfo.GetType().GetProperty(key);
                newItem.SetValue(videoInfo, value);
            }
        }

        /// <summary>
        /// 按顺序从网站中获取信息
        /// </summary>
        /// <param name="VideoName"></param>
        /// <returns></returns>
        private async Task<VideoInfo> SearchInfoByWeb(string VideoName, IProgress<SpliderInfoProgress> progress)
        {
            VideoInfo resultInfo = null;

            var result = DataAccess.SelectTrueName(VideoName.ToUpper());

            //如果数据库已存在该数据
            if (result.Count != 0)
            {
                //使用第一个符合条件的Name
                resultInfo = DataAccess.LoadOneVideoInfoByCID(result[0]);

                progress.Report(new SpliderInfoProgress()
                {
                    matchResult = new MatchVideoResult() { MatchName = VideoName, status = true, message = "数据库已存在" }
                });

                DataAccess.UpdataFileToInfo(VideoName, true);

            }
            // 数据库中不存在该数据
            // 从相关网站中搜索
            else
            {
                //Fc2类型
                if (VideoName.ToLower().Contains("fc2-"))
                {
                    if (AppSettings.isUseFc2Hub)
                    {
                        progress.Report(new SpliderInfoProgress() { matchResult = new MatchVideoResult() { MatchName = VideoName, status = true, message = "等待1~2秒" } });
                        await GetInfoFromNetwork.RandomTimeDelay(1, 2);
                        progress.Report(new SpliderInfoProgress() { matchResult = new MatchVideoResult() { MatchName = VideoName, status = true, message = "从fc2hub中搜索" } });
                        //先从fc2hub中搜索
                        resultInfo = await network.SearchInfoFromFc2Hub(VideoName);
                    }

                    //搜索无果，尝试用javdb
                    if (resultInfo == null && AppSettings.isUseJavDB && !string.IsNullOrEmpty(AppSettings.javdb_Cookie))
                    {
                        progress.Report(new SpliderInfoProgress() { matchResult = new MatchVideoResult() { MatchName = VideoName, status = true, message = "等待3~6秒" } });
                        await GetInfoFromNetwork.RandomTimeDelay(3, 6);
                        progress.Report(new SpliderInfoProgress() { matchResult = new MatchVideoResult() { MatchName = VideoName, status = true, message = "从JavDB中搜索" } });
                        resultInfo = await network.SearchInfoFromJavDB(VideoName);
                    }

                }
                else
                {
                    if (AppSettings.isUseJavBus)
                    {
                        progress.Report(new SpliderInfoProgress() { matchResult = new MatchVideoResult() { MatchName = VideoName, status = true, message = "等待1~3秒" } });
                        await GetInfoFromNetwork.RandomTimeDelay(1, 3);
                        progress.Report(new SpliderInfoProgress() { matchResult = new MatchVideoResult() { MatchName = VideoName, status = true, message = "从JavBus中搜索" } });
                        //先从javbus中搜索
                        resultInfo = await network.SearchInfoFromJavBus(VideoName);
                    }

                    //搜索无果，使用libredmm搜索
                    if (resultInfo == null && AppSettings.isUseLibreDmm)
                    {
                        progress.Report(new SpliderInfoProgress() { matchResult = new MatchVideoResult() { MatchName = VideoName, status = true, message = "等待1~2秒" } });
                        await GetInfoFromNetwork.RandomTimeDelay(1, 2);
                        progress.Report(new SpliderInfoProgress() { matchResult = new MatchVideoResult() { MatchName = VideoName, status = true, message = "从LibreDmm中搜索" } });
                        resultInfo = await network.SearchInfoFromLibreDmm(VideoName);
                    }

                    //搜索无果，使用javdb搜索
                    if (resultInfo == null && AppSettings.isUseJavDB)
                    {
                        progress.Report(new SpliderInfoProgress() { matchResult = new MatchVideoResult() { MatchName = VideoName, status = true, message = "等待3~6秒" } });
                        await GetInfoFromNetwork.RandomTimeDelay(3, 6);
                        progress.Report(new SpliderInfoProgress() { matchResult = new MatchVideoResult() { MatchName = VideoName, status = true, message = "从JavDB中搜索" } });
                        resultInfo = await network.SearchInfoFromJavDB(VideoName);
                    }
                }

                //多次搜索无果，退出
                if (resultInfo == null)
                {
                    DataAccess.UpdataFileToInfo(VideoName, false);
                    return null;
                }

                // 添加进数据库
                DataAccess.AddVideoInfo(resultInfo);

                //更新FileToInfo表
                DataAccess.UpdataFileToInfo(VideoName, true);
            }

            return resultInfo;
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
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ProgressMore_Click(object sender, RoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }

    }
}
