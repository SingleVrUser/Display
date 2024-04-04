using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.System;
using Display.Helper.Date;
using Display.Models.Api.OneOneFive.File;
using Display.Models.Dto.OneOneFive;
using Display.Models.Enums;
using Display.Models.Enums.OneOneFive;
using Display.Models.Vo;
using Display.Models.Vo.OneOneFive;
using Display.Models.Vo.Progress;
using Display.Providers;
using Display.Views.Windows;
using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinUIEx;

namespace Display.Views.Pages.More.Import115DataToLocalDataAccess;

public sealed partial class Progress
{
    private readonly List<DetailFileInfo> _fileInfos;
    private readonly WebApi _webapi = WebApi.GlobalWebApi;
    private readonly ObservableCollection<FileCategory> _fileCategoryCollection = [];
    public Status Status { get; set; }

    private readonly CancellationTokenSource _sCts = new();
    private Window _currentWindow;

    public Progress()
    {
        InitializeComponent();
    }

    public Progress(List<DetailFileInfo> fileInfos)
    {
        InitializeComponent();

        _fileInfos = fileInfos;
    }

    public void CreateWindow()
    {
        var window = new CommonWindow("导入数据");

        _currentWindow = window;

        window.SetWindowSize(500, 666);
        window.Content = this;
        window.Activate();
    }

    private async void CurrentWindow_Closed(object sender, WindowEventArgs args)
    {
        args.Handled = true;

        if (sender is not Window window) return;

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

        window.Closed -= CurrentWindow_Closed;
        window.Close();
    }

    private async void LoadData()
    {
        _currentWindow.Closed += CurrentWindow_Closed;

        GetFolderCategoryProgress.status = Status.Doing;

        // 1.预准备，获取所有文件的全部信息（大小和数量）
        //1-1.获取数据
        List<DetailFileInfo> filesWithoutRootList = [];

        // 剔除根目录 并 获取进度
        foreach (var info in _fileInfos)
        {
            // 文件
            if (info.Type == FileType.File)
            {
                filesWithoutRootList.Add(info);
                _overallCount++;

                _fileCategoryCollection.Add(new FileCategory(info.Datum));
            }
            // 文件夹
            else
            {
                if (info.Id == null) continue;

                var cid = (long)info.Id;

                //cid为0（根目录）无法使用GetFolderCategory接口获取文件信息，故将0目录变为0目录下的目录
                if (cid == 0)
                {
                    var rootFileInfo = await _webapi.GetFileAsync(cid, loadAll: true);

                    var foldersInfoInRoot = rootFileInfo.Data;

                    filesWithoutRootList.AddRange(foldersInfoInRoot.Select(datum => new DetailFileInfo(datum)));
                }
                else
                {
                    filesWithoutRootList.Add(info);
                }
            }
        }

        foreach (var folderInfo in filesWithoutRootList.Where(i => i.Type == FileType.Folder && i.Id != null))
        {
            var item = await _webapi.GetFolderCategory((long)folderInfo.Id!);

            //添加文件和文件夹数量
            _overallCount += item.folder_count;
            _overallCount += item.count;

            //当前文件夹下更新文件夹数量
            _folderCount += item.folder_count;

            //自身为文件夹，也添加进去
            _folderCount++;

            _fileCategoryCollection.Add(item);
        }

        //1-2.显示进度
        OverallProgress.Maximum = _overallCount;
        UpdateProgress();
        GetFolderCategoryProgress.status = Status.Success;

        // 2-1.显示进度
        GetInfosExpander.Visibility = Visibility.Visible;
        GetInfosProgress.status = Status.Doing;
        LeftTimeTipTextBlock.Visibility = Visibility.Visible;
        UpdateLayout();

        var startTime = DateTimeOffset.Now.ToUnixTimeSeconds();

        //进度条
        var progress = new Progress<GetFileProgressIProgress>(progressPercent =>
        {
            switch (progressPercent.status)
            {
                //正常
                case ProgressStatus.normal:
                    _successCount = progressPercent.getFilesProgressInfo.AllCount;
                    UpdateProgress();
                    CpsTextBlock.Text = $"{progressPercent.sendCountPerMinutes} 次/分钟";
                    LeftTimeRun.Text = DateHelper.ConvertDoubleToLengthStr(1.5 * (_folderCount - progressPercent.getFilesProgressInfo.FolderCount));
                    //updateSendSpeed(progressPercent.sendCountPerSecond);
                    break;
                case ProgressStatus.done:
                {
                    //全部完成
                    if (_successCount == _overallCount)
                    {
                        GetInfosProgress.status = Status.Success;

                        //通知
                        TryToast("任务已完成", $"{_overallCount}条数据添加进数据库 👏");
                    }
                    else
                    {
                        FailExpander.Visibility = Visibility.Visible;
                        GetInfosProgress.status = Status.Pause;

                        FailExpander.IsExpanded = true;

                        FailListView.ItemsSource = progressPercent.getFilesProgressInfo?.FailCid;
                        FailCountTextBlock.Text = progressPercent.getFilesProgressInfo?.FailCid.Count.ToString();

                        //通知
                        TryToast("任务已结束", $"完成情况：{_successCount}/{_overallCount}，问题不大 😋");
                    }

                    //剩余时间改总耗时
                    LeftTimeTitleRun.Text = "总耗时：";
                    LeftTimeRun.Text = DateHelper.ConvertDoubleToLengthStr(DateTimeOffset.Now.ToUnixTimeSeconds() - startTime);

                    CpsTextBlock.Visibility = Visibility.Collapsed;
                    GetFolderCategoryExpander.IsExpanded = true;
                    GetInfosProgress.Visibility = Visibility.Collapsed;
                    break;
                }
                case ProgressStatus.cancel:
                    Debug.WriteLine("退出进程");
                    break;
                //出错
                case ProgressStatus.error:
                default:
                    ErrorTeachingTip.IsOpen = true;
                    GetInfosProgress.status = Status.Error;
                    break;
            }

            Status = GetInfosProgress.status;
        });

        // 2.获取数据，获取所有文件的全部信息（大小和数量）
        await _webapi.GetAllFileInfoToDataAccess(filesWithoutRootList, _sCts.Token, progress);

        _currentWindow.Closed -= CurrentWindow_Closed;

        //搜刮完成,是否自动搜刮
        if (AppSettings.IsSpiderAfterImportDataAccess && _overallCount != 0)
        {
            //提示将会开始搜刮
            WillStartSpiderTaskTip.IsOpen = true;

            await Task.Delay(1000, _sCts.Token);

            if (_sCts.Token.IsCancellationRequested) return;

            WillStartSpiderTaskTip.IsOpen = false;

            var fileNameList = _fileCategoryCollection.Select(item => item.file_name).ToList();
            var page = new SpiderVideoInfo.Progress(fileNameList, filesWithoutRootList.Select(x => x.Datum).ToList());
            //创建搜刮进度窗口
            page.CreateWindow();
        }

        if (AppSettings.IsCloseWindowAfterImportDataAccess)
        {
            _currentWindow.Close();
        }
    }

    //文件总数（包括文件夹）
    private int _successCount;
    private int _overallCount;
    private int _folderCount;

    private static void TryToast(string title, string content)
    {
        if (!AppSettings.IsToastAfterImportDataAccess) return;

        new ToastContentBuilder()
            .AddArgument("action", "viewConversation")
            .AddArgument("conversationId", 384928)

            .AddText(title)

            .AddText(content)

            .Show();
    }

    //更新进度环信息
    private void UpdateProgress()
    {
        int percentProgress;
        if (_overallCount == 0)
        {
            percentProgress = 100;
        }
        else
            percentProgress = _successCount * 100 / _overallCount;

        PercentTextBlock.Text = $"{percentProgress}%";
        CountProgressTextBlock.Text = $"{_successCount}/{_overallCount}";
        OverallProgress.Value = _successCount;
    }

    private async void OpenSavePathButton_Click(object sender, RoutedEventArgs e)
    {
        var folder = await StorageFolder.GetFolderFromPathAsync(AppSettings.DataAccessSavePath);

        await Launcher.LaunchFolderAsync(folder);
    }

    private async void BackButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new ContentDialog
        {
            // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
            XamlRoot = XamlRoot,
            Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
            Title = "确认",
            PrimaryButtonText = "确认返回",
            CloseButtonText = "退出",
            DefaultButton = ContentDialogButton.Close,
            Content = "当前任务正在运行，确认返回上一页面？"
        };

        if (Status == Status.Doing)
        {
            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                TryFrameGoBack();
            }
        }
        else
        {
            TryFrameGoBack();
        }
    }

    private void TryFrameGoBack()
    {
        if (!Frame.CanGoBack) return;
        Frame.GoBack();
        _sCts.Cancel();
    }

    private void GetFolderCategory_Expander_Loaded(object sender, RoutedEventArgs e)
    {
        if (_fileInfos != null)
        {
            LoadData();
        }
    }
}