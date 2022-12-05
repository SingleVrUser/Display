using Data;
using Display.Control;
using LiveChartsCore.Drawing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using static Data.WebApi;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DetailInfoPage : Page
    {
        private VideoCoverDisplayClass DetailInfo;

        //public VideoInfo VideoInfo = new();

        public DetailInfoPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is VideoCoverDisplayClass detailinfo)
            {
                DetailInfo = detailinfo;
            }
            else if(e.Parameter is VideoInfo videoinfo)
            {
                DetailInfo = new(videoinfo);
            }

            var anim = ConnectedAnimationService.GetForCurrentView().GetAnimation("ForwardConnectedAnimation");
            if (anim != null)
            {
                anim.TryStart(VideoDetailsControl.Cover_Image);
            }

            anim.Completed += VideoDetailsControl.ForwardConnectedAnimationCompleted;

        }

        // Create connected animation back to collection page.
        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);

            //准备动画
            //限定为 返回操作
            if(e.NavigationMode == NavigationMode.Back)
            {
                //指定页面
                if (e.SourcePageType == typeof(VideoViewPage) || e.SourcePageType == typeof(HomePage) || e.SourcePageType == typeof(ActorInfoPage))
                {
                    //无Cover_Image，退出
                    //例：不存在Image中Source指向的图片文件
                    if (VideoDetailsControl.Cover_Image.DesiredSize == new Size(0, 0)) return;

                    ConnectedAnimation animation = ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("BackConnectedAnimation", VideoDetailsControl.Cover_Image);
                    //返回动画应迅速
                    animation.Configuration = new DirectConnectedAnimationConfiguration();
                }
            }
        }

        /// <summary>
        /// 演员更多页跳转
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Actor_Click(object sender, RoutedEventArgs e)
        {
            var clickButton = (sender as Grid);
            string actorName = clickButton.DataContext as string;
            string[] TypeAndName = { "actor", actorName };

            //准备动画
            ConnectedAnimation animation = ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("ForwardConnectedAnimation", clickButton);
            animation.Configuration = new BasicConnectedAnimationConfiguration();
            Frame.Navigate(typeof(ActorInfoPage), TypeAndName);
        }

        /// <summary>
        /// 标签更多页跳转
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Label_Click(object sender, RoutedEventArgs e)
        {
            var clickButton = sender as HyperlinkButton;
            string LabelName = clickButton.Content as string;
            string[] TypeAndName = { "category", LabelName };

            ConnectedAnimation animation = ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("ForwardConnectedAnimation", clickButton);
            animation.Configuration = new BasicConnectedAnimationConfiguration();
            Frame.Navigate(typeof(ActorInfoPage), TypeAndName);
        }

        /// <summary>
        /// 视频播放页面跳转
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void VideoPlay_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button VideoPlayButton))
                return;

            string name = DetailInfo.truename;
            var videoInfoList = DataAccess.loadVideoInfoByTruename(name);

            //没有该数据
            if(videoInfoList.Count == 0)
            {
                VideoPlayButton.Flyout = new Flyout()
                {
                    Content = new TextBlock() { Text = "经查询，本地数据库无该文件，请导入后继续" }
                };
            }
            //一集
            else if (videoInfoList.Count == 1)
            {
                PlayeVideo(videoInfoList[0].pc);
            }

            //有多集
            else
            {
                List<Datum> multisetList = new();
                foreach (var videoinfo in videoInfoList)
                {
                    multisetList.Add(videoinfo);
                }

                multisetList = multisetList.OrderBy(item => item.n).ToList();

                ContentsPage.SelectSingleVideoToPlay newPage = new ContentsPage.SelectSingleVideoToPlay(multisetList);
                newPage.ContentListView.ItemClick += ContentListView_ItemClick;

                VideoPlayButton.Flyout = new Flyout()
                {
                    Content = newPage
                };
            }
        }

        private void ContentListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var SingleVideoInfo = e.ClickedItem as Data.Datum;
            PlayeVideo(SingleVideoInfo.pc);
            //VideoPlayWindow.createNewWindow();

        }

        public static void PlayeVideo(string pickCode)
        {
            switch (AppSettings.PlayerSelection)
            {
                //浏览器播放
                case 0:
                    VideoPlayWindow.createNewWindow(FileMatch.getVideoPlayUrl(pickCode));
                    break;
                //PotPlayer播放
                case 1:
                    WebApi webapi = new();
                    webapi.PlayVideoWithOriginUrl(pickCode, playMethod.pot);
                    break;
                //mpv播放
                case 2:
                    webapi = new();
                    webapi.PlayVideoWithOriginUrl(pickCode,playMethod.mpv);
                    break;
                //vlc播放
                case 3:
                    webapi = new();
                    webapi.PlayVideoWithOriginUrl(pickCode, playMethod.vlc);
                    break;

            }
        }
        /// <summary>
        /// 点击了删除键
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new ContentDialog()
            {
                XamlRoot = this.XamlRoot,
                Title = "确认",
                PrimaryButtonText = "删除",
                CloseButtonText = "返回",
                DefaultButton = ContentDialogButton.Close,
                Content = "该操作只删除本地数据库数据，不对115服务器进行操作，确认删除？"
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                if (sender is AppBarButton appBarButton)
                {
                    //从数据库中删除
                    DataAccess.DeleteDataInVideoInfoTable(DetailInfo.truename);

                    //删除存储的文件夹
                    string savePath = Path.Combine(AppSettings.Image_SavePath, DetailInfo.truename);
                    if (Directory.Exists(savePath))
                    {
                        Directory.Delete(savePath, true);
                    }

                    DetailInfo.isDeleted = Visibility.Visible;

                    if (Frame.CanGoBack)
                    {
                        Frame.GoBack();
                    }
                }
            }
        }

        private async void CoverTapped(object sender, TappedRoutedEventArgs e)
        {
            if (!(sender is Grid grid))
                return;

            string name = DetailInfo.truename;
            var videoInfoList = DataAccess.loadVideoInfoByTruename(name);



            //没有该数据
            if (videoInfoList.Count == 0)
            {
                ContentDialog dialog = new();
                dialog.XamlRoot = this.XamlRoot;
                dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
                dialog.Title = "播放";
                dialog.CloseButtonText = "返回";
                dialog.Content = "经查询，本地数据库无该文件，请导入后继续";
                dialog.DefaultButton = ContentDialogButton.Close;
                await dialog.ShowAsync();
            }
            //一集
            else if (videoInfoList.Count == 1)
            {
                PlayeVideo(videoInfoList[0].pc);
            }

            //有多集
            else
            {
                List<Datum> multisetList = new();
                foreach (var videoinfo in videoInfoList)
                {
                    multisetList.Add(videoinfo);
                }

                multisetList = multisetList.OrderBy(item => item.n).ToList();

                ContentsPage.SelectSingleVideoToPlay newPage = new ContentsPage.SelectSingleVideoToPlay(multisetList);
                newPage.ContentListView.ItemClick += ContentListView_ItemClick;

                ContentDialog dialog = new();
                dialog.XamlRoot = this.XamlRoot;
                dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
                dialog.Title = "播放";
                dialog.CloseButtonText = "返回";
                dialog.Content = newPage;
                dialog.DefaultButton = ContentDialogButton.Close;
                await dialog.ShowAsync();
            }
        }
    }
}
