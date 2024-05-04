using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DataAccess;
using DataAccess.Dao.Impl;
using DataAccess.Dao.Interface;
using DataAccess.Models.Entity;
using Display.Helper.FileProperties.Name;
using Display.Managers;
using Display.Models.Dto.OneOneFive;
using Display.Models.Enums;
using Display.Models.Vo;
using Display.Models.Vo.OneOneFive;
using Display.Views.Windows;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinUIEx;

namespace Display.Views.Pages.SpiderVideoInfo;

public sealed partial class Progress
{
    private readonly CancellationTokenSource _sCts = new();
    private List<string> SelectedFilesNameList { get; }
    private List<FileInfo> _fileList = [];

    private List<MatchVideoResult> _matchVideoResults;
    private Window _currentWindow;

    private readonly IFileInfoDao _filesInfoDao = App.GetService<IFileInfoDao>();

    public Progress(List<string> selectedFilesNameList, List<FileInfo> fileList)
    {
        InitializeComponent();
        SelectedFilesNameList = selectedFilesNameList;
        _fileList = fileList;
        Loaded += PageLoaded;
    }

    private async void PageLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= PageLoaded;

        _currentWindow.Closed += CurrentWindow_Closed;

        await ShowMatchResult();

        SpiderVideoInfo();

        _currentWindow.Closed -= CurrentWindow_Closed;

        TopProgressRing.IsActive = false;
        TotalProgressTextBlock.Text = "完成";
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

        var newList = new List<FileInfo>();
        foreach (var filesInfo in _fileList.Where(i=> i.FileId == default))
        {
            var allFilesInFolder = await _filesInfoDao.GetAllFilesListByFolderIdAsync(filesInfo.CurrentId);
            newList.AddRange(allFilesInFolder);
        }
        
        _fileList.AddRange(newList);

        //除去文件夹
        _fileList = _fileList.Where(item => item.FileId != default).ToList();

        //去除重复文件
        var newDictList = new Dictionary<string, FileInfo>();
        _fileList.ForEach(item => newDictList.TryAdd(item.PickCode, item));

        _fileList = newDictList.Values.ToList();

        //显示饼状图
        ShowFilesPieCharts(_fileList);
        if (_sCts.IsCancellationRequested) return;

        TotalProgressTextBlock.Text = "正则匹配番号名中……";

        //挑选符合条件的视频文件
        _matchVideoResults = await Task.Run(() => FileMatch.GetVideoAndMatchFile(_fileList));

        TopProgressRing.IsActive = false;
    }

    /// <summary>
    /// 显示饼形图
    /// </summary>
    /// <param name="datumList"></param>
    private void ShowFilesPieCharts(List<FileInfo> datumList)
    {
        FileInfoPieChart.Visibility = Visibility.Visible;

        //1.视频文件，其中SD:x个，hd:x个，fhd:x个，1080p:x个，4k:x个，原画:x个
        var videoInfo = new FileStatistics(FileFormatEnum.Video);
        var subInfo = new FileStatistics(FileFormatEnum.Subtitles);
        var torrentInfo = new FileStatistics(FileFormatEnum.Torrent);
        var imageInfo = new FileStatistics(FileFormatEnum.Image);
        var audioInfo = new FileStatistics(FileFormatEnum.Audio);

        foreach (var info in datumList)
        {
            //文件夹，暂不统计
            if (info.FileId == default)
            {

            }
            //视频文件
            else if (info.Iv == 1)
            {
                var videoQuality = DetailFileInfo.GetVideoQualityFromVdi(info.Vdi);

                UpdateFileStatistics(info, videoInfo, videoQuality);
            }
            //其他文件
            else
            {
                switch (info.Ico)
                {
                    //字幕
                    case "ass" or "srt" or "ssa":
                        UpdateFileStatistics(info, subInfo, info.Ico);
                        break;
                    //种子
                    case "torrent":
                        UpdateFileStatistics(info, torrentInfo, info.Ico);
                        break;
                    //图片
                    case "gif" or "bmp" or "tiff" or "exif" or "jpg" or "png" or "raw" or "svg":
                        UpdateFileStatistics(info, imageInfo, info.Ico);
                        break;
                    //音频
                    case "ape" or "wav" or "midi" or "mid" or "flac" or "aac" or "m4a" or "ogg" or "amr":
                        UpdateFileStatistics(info, audioInfo, info.Ico);
                        break;
                    //case "7z" or "cab" or "dmg" or "iso" or "rar" or "zip":
                    //    UpdateFileStaticstics(info, ArchiveInfo, info.ico);
                    //    break;
                }
            }
        }

        CountPercentPieChart.Series = new ISeries[] {
            new PieSeries<double> { Values = [videoInfo.Count], Pushout = 5, Name = "视频"},
            new PieSeries<double> { Values = [audioInfo.Count], Pushout = 0, Name = "音频"},
            new PieSeries<double> { Values = [subInfo.Count], Pushout = 0, Name = "字幕"},
            new PieSeries<double> { Values = [torrentInfo.Count], Pushout = 0, Name = "种子"},
            new PieSeries<double> { Values = [imageInfo.Count], Pushout = 0, Name = "图片"}
        };

    }

    /// <summary>
    /// 分析文件信息（饼形图）
    /// </summary>
    /// <param name="dataInfo"></param>
    /// <param name="typeInfo"></param>
    /// <param name="name"></param>
    private void UpdateFileStatistics(FileInfo dataInfo, FileStatistics typeInfo, string name)
    {
        typeInfo.Size += dataInfo.Size;
        typeInfo.Count++;

        if (typeInfo.FileInfo.Count != 0)
        {
            var tmpData = typeInfo.FileInfo.FirstOrDefault(x => x.Name == name);

            if (tmpData == null)
            {
                typeInfo.FileInfo.Add(new FileStatistics.Data { Name = name, Count = 1, Size = dataInfo.Size });
            }
            else
            {
                tmpData.Size += dataInfo.Size;
                tmpData.Count++;
            }
        }
        else
        {
            typeInfo.FileInfo.Add(new FileStatistics.Data { Name = name, Count = 1, Size = dataInfo.Size });
        }
    }

    /// <summary>
    /// 开始从网络中检索视频信息
    /// </summary>
    private void SpiderVideoInfo()
    {
        if (_matchVideoResults == null) return;

        var matchNameList = _matchVideoResults.Where(item => !string.IsNullOrEmpty(item.MatchName))
            .Select(match => match.MatchName).ToList();

        if (matchNameList.Count == 0) return;

        var spiderManager = App.GetService<SpiderManager>();
        spiderManager.AddTask(matchNameList);

        // 展示页面
        TaskPage.ShowSingleWindow(NavigationViewItemEnum.SpiderTask);

        TopProgressRing.IsActive = false;
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

        await _sCts.CancelAsync();

        if (window == null) return;
        window.Closed -= CurrentWindow_Closed;
        window.Close();
    }

    public void CreateWindow()
    {
        var window = new CommonWindow("匹配名称");
        _currentWindow = window;
        window.SetWindowSize(950, 730);
        window.Content = this;
        window.Activate();
    }
}