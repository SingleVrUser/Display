using Data;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.ContentsPage
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ProgressOfImportDataAccess : Page
    {
        WebApi webapi = new ();
        public ObservableCollection<FolderCategory> FolderCategory = new();
        public ObservableCollection<Datum> FilesItemsSource = new();
        List<string> cidList;
        public Status _status;

        public ProgressOfImportDataAccess()
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

            // Store the item to be used in binding to UI
            cidList = e.Parameter as List<string>;

            if(cidList != null)
            {
                loadData();
            }
        }

        private async void loadData()
        {
            GetFolderCategory_Expander.Visibility = Visibility.Visible;
            GetFolderCategory_Progress.status = Status.doing;

            // 1.预准备，获取所有文件的全部信息（大小和数量）
            //1-1.获取数据
            foreach (var cid in cidList)
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
                return;
            }

            //1-2.显示进度
            overallProgress.Maximum = overallCount;
            updataProgress();
            GetFolderCategory_Progress.status = Status.success;

            // 2.获取数据，获取所有文件的全部信息（大小和数量）

            // 2-1.显示进度
            GetInfos_Expander.Visibility = Visibility.Visible;
            GetInfos_Progress.status = Status.doing;
            LeftTimeTip_TextBlock.Visibility = Visibility.Visible;
            UpdateLayout();

            //int totalCount = FolderCategory.Count;

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
                    //隐藏剩余时间
                    LeftTimeTip_TextBlock.Visibility = Visibility.Visible;


                    //全部完成
                    if (successCount == overallCount)
                    {
                        GetInfos_Progress.status = Status.success;

                        //通知
                        tryToast("任务已完成", $"{overallCount}条数据添加进数据库 👏");
                    }
                    else
                    {
                        GetInfos_Progress.status = Status.pause;

                        //通知
                        tryToast("任务已结束", $"完成情况：{successCount}/{overallCount}，问题不大 😋");
                    }

                    GetFolderCategory_Expander.IsExpanded = true;
                }
                //出错
                else
                {
                    ErrorTeachingTip.IsOpen = true;
                    GetInfos_Progress.status = Status.error;
                }

                _status = GetInfos_Progress.status;

            });

            await webapi.GetAllFileInfoToDataAccess(cidList, new GetFilesProgressInfo(), progress);

            //Task.Run(() =>
            //{
            //    //int count = 0;
            //    //var fileProgressInfo = new GetFilesProgressInfo() { count = 0 };
                

            //});
        }

        //文件总数（包括文件夹）
        int successCount = 0;
        int overallCount = 0;
        int folderCount = 0;


        private void tryToast(string title, string content)
        {
            if (!IsToastAfterDone_ToggleSwitch.IsOn) return;

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
            }
        }
    }
}
