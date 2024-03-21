using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.System;
using Display.CustomWindows;
using Display.Helper.Date;
using Display.Models.Data;
using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinUIEx;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.Views.More.Import115DataToLocalDataAccess
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Progress : Page
    {
        private List<FilesInfo> _fileInfos;
        private WebApi webapi = WebApi.GlobalWebApi;
        public ObservableCollection<FileCategory> FileCategoryCollection = new();
        public Status _status;

        private CancellationTokenSource s_cts = new();
        private Window currentWindow;

        public Progress()
        {
            InitializeComponent();
        }

        public Progress(List<FilesInfo> fileInfos)
        {
            InitializeComponent();

            _fileInfos = fileInfos;
        }

        public void CreateWindow()
        {
            var window = new CommonWindow("导入数据");

            currentWindow = window;

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

            await s_cts.CancelAsync();

            window.Closed -= CurrentWindow_Closed;
            window.Close();
        }

        private async void LoadData()
        {
            currentWindow.Closed += CurrentWindow_Closed;

            GetFolderCategory_Progress.status = Status.Doing;

            // 1.预准备，获取所有文件的全部信息（大小和数量）
            //1-1.获取数据
            List<FilesInfo> filesWithoutRootList = new();

            // 剔除根目录 并 获取进度
            foreach (var info in _fileInfos)
            {
                // 文件
                if (info.Type == FilesInfo.FileType.File)
                {
                    filesWithoutRootList.Add(info);
                    _overallCount ++;

                    FileCategoryCollection.Add(new FileCategory(info.Datum));
                }
                // 文件夹
                else
                {
                    if(info.Id == null) continue;

                    var cid = (long)info.Id;

                    //cid为0（根目录）无法使用GetFolderCategory接口获取文件信息，故将0目录变为0目录下的目录
                    if (cid == 0)
                    {
                        var rootFileInfo = await webapi.GetFileAsync(cid, loadAll: true);

                        var foldersInfoInRoot = rootFileInfo.data;

                        filesWithoutRootList.AddRange(foldersInfoInRoot.Select(datum => new FilesInfo(datum)));
                    }
                    else
                    {
                        filesWithoutRootList.Add(info);
                    }
                }
            }

            foreach (var folderInfo in filesWithoutRootList.Where(i => i.Type == FilesInfo.FileType.Folder && i.Id != null))
            {
                var item = await webapi.GetFolderCategory((long)folderInfo.Id!);

                //添加文件和文件夹数量
                _overallCount += item.folder_count;
                _overallCount += item.count;

                //当前文件夹下更新文件夹数量
                _folderCount += item.folder_count;

                //自身为文件夹，也添加进去
                _folderCount++;

                FileCategoryCollection.Add(item);
            }

            //1-2.显示进度
            overallProgress.Maximum = _overallCount;
            UpdateProgress();
            GetFolderCategory_Progress.status = Status.Success;

            // 2-1.显示进度
            GetInfos_Expander.Visibility = Visibility.Visible;
            GetInfos_Progress.status = Status.Doing;
            LeftTimeTip_TextBlock.Visibility = Visibility.Visible;
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
                        cps_TextBlock.Text = $"{progressPercent.sendCountPerMinutes} 次/分钟";
                        leftTime_Run.Text = DateHelper.ConvertDoubleToLengthStr(1.5 * (_folderCount - progressPercent.getFilesProgressInfo.FolderCount));
                        //updateSendSpeed(progressPercent.sendCountPerSecond);
                        break;
                    case ProgressStatus.done:
                    {
                        //全部完成
                        if (_successCount == _overallCount)
                        {
                            GetInfos_Progress.status = Status.Success;

                            //通知
                            TryToast("任务已完成", $"{_overallCount}条数据添加进数据库 👏");
                        }
                        else
                        {
                            Fail_Expander.Visibility = Visibility.Visible;
                            GetInfos_Progress.status = Status.Pause;

                            Fail_Expander.IsExpanded = true;

                            Fail_ListView.ItemsSource = progressPercent.getFilesProgressInfo?.FailCid;
                            FailCount_TextBlock.Text = progressPercent.getFilesProgressInfo?.FailCid.Count.ToString();

                            //通知
                            TryToast("任务已结束", $"完成情况：{_successCount}/{_overallCount}，问题不大 😋");
                        }

                        //剩余时间改总耗时
                        leftTimeTitle_Run.Text = "总耗时：";
                        leftTime_Run.Text = DateHelper.ConvertDoubleToLengthStr(DateTimeOffset.Now.ToUnixTimeSeconds() - startTime);

                        cps_TextBlock.Visibility = Visibility.Collapsed;
                        GetFolderCategory_Expander.IsExpanded = true;
                        GetInfos_Progress.Visibility = Visibility.Collapsed;
                        break;
                    }
                    case ProgressStatus.cancel:
                        Debug.WriteLine("退出进程");
                        break;
                    //出错
                    default:
                        ErrorTeachingTip.IsOpen = true;
                        GetInfos_Progress.status = Status.Error;
                        break;
                }

                _status = GetInfos_Progress.status;
            });

            // 2.获取数据，获取所有文件的全部信息（大小和数量）
            await webapi.GetAllFileInfoToDataAccess(filesWithoutRootList, s_cts.Token, progress);

            if (s_cts.Token.IsCancellationRequested) return;

            //搜刮完成,是否自动搜刮
            if (AppSettings.IsSpiderAfterImportDataAccess && _overallCount != 0)
            {
                //提示将会开始搜刮
                WillStartSpiderTaskTip.IsOpen = true;

                await Task.Delay(3000, s_cts.Token);

                if (s_cts.Token.IsCancellationRequested) return;

                WillStartSpiderTaskTip.IsOpen = false;

                var fileNameList = FileCategoryCollection.Select(item => item.file_name).ToList();
                var page = new SpiderVideoInfo.Progress(fileNameList, filesWithoutRootList.Select(x=>x.Datum).ToList());
                //创建搜刮进度窗口
                page.CreateWindow();
            }

            currentWindow.Closed -= CurrentWindow_Closed;
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
                percentProgress = (_successCount * 100) / _overallCount;

            percent_TextBlock.Text = $"{percentProgress}%";
            countProgress_TextBlock.Text = $"{_successCount}/{_overallCount}";
            overallProgress.Value = _successCount;
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

            if (_status == Status.Doing)
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
            s_cts.Cancel();
        }

        private void GetFolderCategory_Expander_Loaded(object sender, RoutedEventArgs e)
        {
            if (_fileInfos != null)
            {
                LoadData();
            }
        }
    }
}
