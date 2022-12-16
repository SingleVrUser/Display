using Data;
using Display.WindowView;
using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.System;
using WinUIEx;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.ContentsPage.Import115DataToLocalDataAccess
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Progress : Page
    {
        WebApi webapi = new ();
        public ObservableCollection<FolderCategory> FolderCategory = new();
        public ObservableCollection<Datum> FilesItemsSource = new();
        List<string> cidList;
        public Status _status;

        CancellationTokenSource s_cts = new();
        Window currentWindow;

        public Progress()
        {
            this.InitializeComponent();

        }

        /// <summary>
        /// 从其他页面Navigate来
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var content = e.Parameter as ContentPassBetweenPage;

            cidList = content.cidList;

            currentWindow = content.window;

            if (cidList != null)
            {
                loadData();
            }

        }

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

        //protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        //{
        //    base.OnNavigatingFrom(e);
        //}

        private async void loadData()
        {
            currentWindow.Closed += CurrentWindow_Closed;

            GetFolderCategory_Expander.Visibility = Visibility.Visible;
            GetFolderCategory_Progress.status = Status.doing;

            // 1.预准备，获取所有文件的全部信息（大小和数量）
            //1-1.获取数据

            //cid为0（根目录）无法使用GetFolderCategory接口获取文件信息，故将0目录变为0目录下的目录
            List<string> cidWithoutRootList = new();
            foreach (var cid in cidList)
            {
                if(cid == "0")
                {
                    var RootFileInfo = webapi.GetFile(cid);

                    var FoldersInfoInRoot = RootFileInfo.data;

                    foreach (var info in FoldersInfoInRoot)
                    {
                        cidWithoutRootList.Add(info.cid);
                    }
                }
                else
                {
                    cidWithoutRootList.Add(cid);
                }
            }

            foreach (var cid in cidWithoutRootList)
            {
                var item = await webapi.GetFolderCategory(cid);

                //添加文件和文件夹数量
                overallCount += item.folder_count;
                overallCount += item.count;

                //更新文件夹数量
                folderCount += item.folder_count;
                FolderCategory.Add(item);
            }

            if (overallCount == 0)
            {
                GetFolderCategory_Progress.status = Status.error;
                return;
            }

            //1-2.显示进度
            overallProgress.Maximum = overallCount;
            updataProgress();
            GetFolderCategory_Progress.status = Status.success;


            // 2-1.显示进度
            GetInfos_Expander.Visibility = Visibility.Visible;
            GetInfos_Progress.status = Status.doing;
            LeftTimeTip_TextBlock.Visibility = Visibility.Visible;
            UpdateLayout();

            var startTime = DateTimeOffset.Now.ToUnixTimeSeconds();

            //进度条
            var progress = new Progress<GetFileProgessIProgress> (progressPercent =>
            {
                //正常
                if(progressPercent.status == ProgressStatus.normal)
                {
                    successCount = progressPercent.getFilesProgressInfo.AllCount;
                    updataProgress();
                    cps_TextBlock.Text = $"{progressPercent.sendCountPerMinutes} 次/分钟";
                    leftTime_Run.Text = FileMatch.ConvertInt32ToDateStr(1.5* (folderCount- progressPercent.getFilesProgressInfo.FolderCount));
                    //updateSendSpeed(progressPercent.sendCountPerSecond);

                }
                else if(progressPercent.status == ProgressStatus.done)
                {
                    //全部完成
                    if (successCount == overallCount)
                    {
                        GetInfos_Progress.status = Status.success;

                        //通知
                        tryToast("任务已完成", $"{overallCount}条数据添加进数据库 👏");
                    }
                    else
                    {
                        Fail_Expander.Visibility = Visibility.Visible;
                        GetInfos_Progress.status = Status.pause;

                        Fail_Expander.IsExpanded = true;

                        Fail_ListView.ItemsSource = progressPercent.getFilesProgressInfo.FailCid;
                        FailCount_TextBlock.Text = progressPercent.getFilesProgressInfo.FailCid.Count.ToString();

                        //通知
                        tryToast("任务已结束", $"完成情况：{successCount}/{overallCount}，问题不大 😋");
                    }

                    //剩余时间改总耗时
                    leftTimeTitle_Run.Text = "总耗时：";
                    leftTime_Run.Text = FileMatch.ConvertInt32ToDateStr(DateTimeOffset.Now.ToUnixTimeSeconds() - startTime);

                    cps_TextBlock.Visibility = Visibility.Collapsed;
                    GetFolderCategory_Expander.IsExpanded = true;
                    GetInfos_Progress.Visibility = Visibility.Collapsed;
                }
                else if(progressPercent.status == ProgressStatus.cancel)
                {
                    Debug.WriteLine("退出进程");
                }
                //出错
                else
                {
                    ErrorTeachingTip.IsOpen = true;
                    GetInfos_Progress.status = Status.error;
                }

                _status = GetInfos_Progress.status;

            });

            // 2.获取数据，获取所有文件的全部信息（大小和数量）
            await webapi.GetAllFileInfoToDataAccess(cidWithoutRootList, new GetFilesProgressInfo(), s_cts.Token, progress);

            //搜刮完成,是否自动搜刮
            if (AppSettings.ProgressOfImportDataAccess_IsStartSpiderAfterTask)
            {
                //提示将会开始搜刮
                WillStartSpiderTaskTip.IsOpen = true;

                await Task.Delay(3000);

                List<string> folderList = FolderCategory.Select(item=>item.file_name).ToList();

                //datum只用到其中的cid,所以只赋值cid (fid默认为空,不用理)
                List<Datum> datumList = new();
                cidWithoutRootList.ForEach(item => datumList.Add(new() { cid = item }));

                var page = new ContentsPage.SpiderVideoInfo.Progress(folderList, datumList);
                //创建搜刮进度窗口
                page.CreateWindow();
            }

            currentWindow.Closed -= CurrentWindow_Closed;
        }

        //文件总数（包括文件夹）
        int successCount = 0;
        int overallCount = 0;
        int folderCount = 0;

        private void tryToast(string title, string content)
        {
            if (!AppSettings.ProgressOfImportDataAccess_IsStartSpiderAfterTask) return;

            new ToastContentBuilder()
                .AddArgument("action", "viewConversation")
                .AddArgument("conversationId", 384928)

                .AddText(title)

                .AddText(content)

                .Show();
        }

        //更新进度环信息
        private void updataProgress()
        {
            int percentProgress = (successCount * 100) / overallCount;
            percent_TextBlock.Text = $"{percentProgress}%";
            countProgress_TextBlock.Text = $"{successCount}/{overallCount}";
            overallProgress.Value = successCount;
        }

        private async void OpenSavePathButton_Click(object sender, RoutedEventArgs e)
        {
            StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(AppSettings.DataAccess_SavePath);

            await Launcher.LaunchFolderAsync(folder);
        }

        private async void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if(_status == Status.doing)
            {
                ContentDialog dialog = new ContentDialog();

                // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
                dialog.XamlRoot = this.XamlRoot;
                dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
                dialog.Title = "确认";
                dialog.PrimaryButtonText = "确认返回";
                dialog.CloseButtonText = "退出";
                dialog.DefaultButton = ContentDialogButton.Close;
                dialog.Content = "当前任务正在运行，确认返回上一页面？";

                var result = await dialog.ShowAsync();
                if(result == ContentDialogResult.Primary)
                {
                    tryFrameGoBack();
                }
            }
            else
            {
                tryFrameGoBack();
            }
        }

        private void tryFrameGoBack()
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
                s_cts.Cancel();
            }
        }
    }
}
