using Display.Helper;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Display.Data;
using Display.Models;
using Display.ContentsPage.DetailInfo;
using System.Xml.Linq;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.Views
{

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DetailInfoPage : Page
    {
        public VideoCoverDisplayClass DetailInfo;

        public static DetailInfoPage Current;

        //public VideoInfo VideoInfo = new();

        public DetailInfoPage()
        {
            this.InitializeComponent();

            Current = this;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);



            var anim = ConnectedAnimationService.GetForCurrentView().GetAnimation("ForwardConnectedAnimation");
            if (anim != null)
            {
                anim.TryStart(VideoDetailsControl.Cover_Image);

                ////有动画，动画完成后监听Cover_Grid（封面显示播放按钮）
                //anim.Completed += (_, _) => VideoDetailsControl.StartListCover_GridTapped();
            }
            else
            {
                VideoDetailsControl.CoverImageAddEnterAnimation();
            }
            //else
            //{
            //    //没有动画，直接监听Cover_Grid（封面显示播放按钮）
            //    VideoDetailsControl.StartListCover_GridTapped();
            //}


            DetailInfo = e.Parameter switch
            {
                VideoCoverDisplayClass detailinfo => detailinfo,
                VideoInfo videoinfo => new VideoCoverDisplayClass(videoinfo, 500, 300),
                _ => DetailInfo
            };
        }

        // Create connected animation back to collection page.
        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);

            //准备动画
            //限定为 返回操作
            if (e.NavigationMode == NavigationMode.Back)
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
        /// <param Name="sender"></param>
        /// <param Name="e"></param>
        private void Actor_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Grid clickButton) return;

            string actorName = null;
            if (clickButton.DataContext is string actorNameStr)
            {
                actorName = actorNameStr;
            }
            else if (clickButton.DataContext is ActorInfo actorInfo)
            {
                actorName = actorInfo.name;
            }

            if (string.IsNullOrEmpty(actorName)) return;

            Tuple<List<string>, string, bool> TypesAndName = new(new() { "actor" }, actorName, false);

            //准备动画
            ConnectedAnimation animation = ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("ForwardConnectedAnimation", clickButton);
            animation.Configuration = new BasicConnectedAnimationConfiguration();
            Frame.Navigate(typeof(ActorInfoPage), TypesAndName);
        }

        /// <summary>
        /// 标签更多页跳转
        /// </summary>
        /// <param Name="sender"></param>
        /// <param Name="e"></param>
        private void Label_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button clickButton) return;

            string LabelName = clickButton.Content as string;
            Tuple<List<string>, string, bool> TypesAndName = new(new() { "category" }, LabelName, false);

            ConnectedAnimation animation = ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("ForwardConnectedAnimation", clickButton);
            Frame.Navigate(typeof(ActorInfoPage), TypesAndName);
        }

        /// <summary>
        /// 视频播放页面跳转
        /// </summary>
        /// <param Name="sender"></param>
        /// <param Name="e"></param>
        private async void VideoPlay_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button videoPlayButton)
                return;

            string trueName = DetailInfo.truename;
            var videoInfoList = DataAccess.loadFileInfoByTruename(trueName);

            //没有该数据
            if (videoInfoList.Count == 0)
            {
                videoPlayButton.Flyout = new Flyout()
                {
                    Content = new TextBlock() { Text = "经查询，本地数据库无该文件，请导入后继续" }
                };
            }
            //一集
            else if (videoInfoList.Count == 1)
            {
                var mediaPlayItem = new MediaPlayItem(videoInfoList[0].PickCode, trueName, FilesInfo.FileType.File);
                await PlayVideoHelper.PlayVideo(new List<MediaPlayItem>() { mediaPlayItem }, this.XamlRoot, lastPage: this);
            }
            //有多集
            else
            {
                PlayVideoHelper.ShowSelectedVideoToPlayPage(videoInfoList, this.XamlRoot);
            }
        }


        //private async void VideoInfoListView_ItemClick(object sender, ItemClickEventArgs e)
        //{
        //    if (e.ClickedItem is not Datum singleVideoInfo) return;

        //    if (sender is not ListView { DataContext: string trueName }) return;

        //    var mediaPlayItem = new MediaPlayItem(singleVideoInfo.pc, trueName, FilesInfo.FileType.File);

        //    await PlayVideoHelper.PlayVideo(new List<MediaPlayItem>() { mediaPlayItem }, this.XamlRoot , lastPage: this);
        //}



        /// <summary>
        /// 点击了删除键
        /// </summary>
        /// <param Name="sender"></param>
        /// <param Name="e"></param>
        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new ContentDialog()
            {
                XamlRoot = XamlRoot,
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                Title = "确认",
                PrimaryButtonText = "删除",
                CloseButtonText = "返回",
                DefaultButton = ContentDialogButton.Close,
                Content = "该操作只删除本地数据库数据，不对115服务器进行操作，确认删除？"
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                if (sender is AppBarButton)
                {
                    //从数据库中删除
                    DataAccess.DeleteDataInVideoInfoTable(DetailInfo.truename);

                    //删除存储的文件夹
                    string savePath = Path.Combine(AppSettings.ImageSavePath, DetailInfo.truename);
                    if (Directory.Exists(savePath))
                    {
                        Directory.Delete(savePath, true);
                    }

                    DetailInfo.IsDeleted = Visibility.Visible;

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
            var videoInfoList = DataAccess.loadFileInfoByTruename(name);

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

                var mediaPlayItem = new MediaPlayItem(videoInfoList[0].PickCode, name, FilesInfo.FileType.File);
                await PlayVideoHelper.PlayVideo(new List<MediaPlayItem>() { mediaPlayItem }, this.XamlRoot, lastPage: this);
            }

            //有多集
            else
            {
                PlayVideoHelper.ShowSelectedVideoToPlayPage(videoInfoList, this.XamlRoot);
            }
        }
    }
}
