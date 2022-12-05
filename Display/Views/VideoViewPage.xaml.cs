using ByteSizeLib;
using Data;
using Display.Control;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class VideoViewPage : Page
    {
        //为返回动画做准备（需启动缓存）
        VideoCoverDisplayClass _storeditem;
        public VideoViewPage()
        {
            this.InitializeComponent();

            //启动缓存（为了返回无需过长等待，也为了返回动画）
            NavigationCacheMode = Microsoft.UI.Xaml.Navigation.NavigationCacheMode.Enabled;

            //加载数据
            LoadSqlData();

        }

        /// <summary>
        /// 从数据库中加载数据
        /// </summary>
        private void LoadSqlData()
        {
            if (videoControl.FileGrid == null)
            {
                videoControl.FileGrid = new();
            }

            List<VideoInfo> VideoInfoList = DataAccess.LoadAllVideoInfo(-1);
            videoControl.FileGrid = FileMatch.getFileGrid(VideoInfoList);
        }


        private void SingleVideoPlay_Click(object sender, RoutedEventArgs e)
        {
            var VideoPlayButton = (Button)sender;
            var datum = VideoPlayButton.DataContext as Datum;

            Views.DetailInfoPage.PlayeVideo(datum.pc);
        }

        /// <summary>
        /// 选项选中后跳转至详情页
        /// </summary>
        private void OnClicked(object sender, RoutedEventArgs e)
        {
            VideoCoverDisplayClass item = (sender as Button).DataContext as VideoCoverDisplayClass;

            //准备动画
            videoControl.PrepareAnimation(item);
            _storeditem = item;
            Frame.Navigate(typeof(DetailInfoPage), item, new SuppressNavigationTransitionInfo());
        }

        /// <summary>
        /// 视频播放页面跳转
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void VideoPlay_Click(object sender, RoutedEventArgs e)
        {
            var VideoPlayButton = (Button)sender;
            var videoInfo = VideoPlayButton.DataContext as VideoCoverDisplayClass;
            List<Datum> videoInfoList = DataAccess.loadVideoInfoByTruename(videoInfo.truename);

            //没有
            if (videoInfoList.Count == 0)
            {
                VideoPlayButton.Flyout = new Flyout()
                {
                    Content = new TextBlock() { Text = "经查询，本地数据库无该文件，请导入后继续" }
                };
            }
            else if (videoInfoList.Count == 1)
            {
                Views.DetailInfoPage.PlayeVideo(videoInfoList[0].pc);
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
                newPage.ContentListView.ItemClick += ContentListView_ItemClick; ;

                VideoPlayButton.Flyout = new Flyout()
                {
                    Content = newPage
                };
            }
        }

        private void ContentListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var SingleVideoInfo = e.ClickedItem as Data.Datum;


            Views.DetailInfoPage.PlayeVideo(SingleVideoInfo.pc);

            //VideoPlayWindow.createNewWindow(FileMatch.getVideoPlayUrl(SingleVideoInfo.pc));
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //videoControl.BasicGridView.ScrollIntoView(_storeditem);
            ConnectedAnimation animation = ConnectedAnimationService.GetForCurrentView().GetAnimation("BackConnectedAnimation");
            if (animation != null)
            {
                //开始动画
                if (_storeditem != null)
                {
                    ////开始动画
                    videoControl.StartAnimation(animation, _storeditem);
                }
                else
                {
                    animation.TryStart(videoControl.nowPageTextBox);
                }
            }
        }
    }

}
