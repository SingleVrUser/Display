using Data;
using Display.Controls;
using Display.Helper;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class VideoViewPage : Page
    {
        public static VideoViewPage Current;

        //为返回动画做准备（需启动缓存）
        public VideoCoverDisplayClass _storeditem;
        public VideoViewPage()
        {
            this.InitializeComponent();

            Current = this;

            //启动缓存（为了返回无需过长等待，也为了返回动画）
            NavigationCacheMode = Microsoft.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
        }

        private async void SingleVideoPlay_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Grid VideoPlayGrid) return;

            string pickCode;
            if (VideoPlayGrid.DataContext is Datum datum)
            {
                pickCode = datum.pc;
            }
            else if (VideoPlayGrid.DataContext is FailInfo failInfo)
            {
                pickCode = failInfo.pc;
            }
            else
            {
                return;
            }

            if (string.IsNullOrEmpty(pickCode)) return;

            await PlayeVideoHelper.PlayeVideo(pickCode, this.XamlRoot, playType: CustomMediaPlayerElement.PlayType.fail);
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
        /// <param Name="sender"></param>
        /// <param Name="e"></param>
        private async void VideoPlay_Click(object sender, RoutedEventArgs e)
        {
            var VideoPlayButton = (Button)sender;

            if (VideoPlayButton.DataContext is not VideoCoverDisplayClass videoInfo) return;

            List<Datum> videoInfoList = DataAccess.loadFileInfoByTruename(videoInfo.truename);


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
                _storeditem = videoInfo;

                await PlayeVideoHelper.PlayeVideo(videoInfoList[0].pc, this.XamlRoot, trueName: videoInfo.truename, lastPage: this);
                ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Arrow);
            }

            //有多集
            else
            {
                _storeditem = videoInfo;

                List<Datum> multisetList = new();
                foreach (var videoinfo in videoInfoList)
                {
                    multisetList.Add(videoinfo);
                }

                multisetList = multisetList.OrderBy(item => item.n).ToList();

                ContentsPage.DetailInfo.SelectSingleVideoToPlay newPage = new(multisetList, videoInfo.truename);
                newPage.ContentListView.ItemClick += ContentListView_ItemClick; ;

                VideoPlayButton.Flyout = new Flyout()
                {
                    Content = newPage
                };
            }
        }

        private async void ContentListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var SingleVideoInfo = e.ClickedItem as Data.Datum;

            if (sender is not ListView listView) return;
            if (listView.DataContext is not string trueName) return;

            await PlayeVideoHelper.PlayeVideo(SingleVideoInfo.pc, this.XamlRoot, trueName:trueName, lastPage: this);
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
                    animation.TryStart(videoControl.PageShow_Grid);
                }
            }
        }

    }

}
