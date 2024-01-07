using Display.Controls;
using Display.Helper;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using Display.Data;
using static Display.Controls.CustomMediaPlayerElement;
using Display.Models;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ActorInfoPage : Page
    {
        //为返回动画做准备（需启动缓存）
        public VideoCoverDisplayClass _storeditem;


        public ActorInfoPage()
        {
            //启动缓存（为了返回无需过长等待，也为了返回动画）
            NavigationCacheMode = NavigationCacheMode.Enabled;

            this.InitializeComponent();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            //准备动画
            //限定为 返回操作
            if (e.NavigationMode != NavigationMode.Back) return;

            //指定页面
            if (e.SourcePageType != typeof(ActorsPage)) return;

            var animation = ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("BackConnectedAnimation", videoControl.HeaderCover);

            //返回动画应迅速
            animation.Configuration = new DirectConnectedAnimationConfiguration();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            switch (e.NavigationMode)
            {
                //过渡动画
                case NavigationMode.Back:
                {
                    ConnectedAnimation animation = ConnectedAnimationService.GetForCurrentView().GetAnimation("BackConnectedAnimation");
                    if (animation != null)
                    {
                        videoControl.StartAnimation(animation, _storeditem);
                    }

                    break;
                }
                case NavigationMode.New:
                {
                    var item = e.Parameter;
                    if (item == null) return;
                    //需要显示的是搜索结果
                    if (item is Tuple<List<string>, string, bool> tuple)
                    {
                        Tuple<List<string>, string> typesAndName = new(tuple.Item1, tuple.Item2);

                        LoadShowInfo(typesAndName, tuple.Item3);
                    }
                    else
                    {
                        return;
                    }

                    var animation = ConnectedAnimationService.GetForCurrentView().GetAnimation("ForwardConnectedAnimation");
                    if (animation != null)
                    {
                        if (tuple.Item1.Count == 1 && tuple.Item1.FirstOrDefault() == "actor")
                        {
                            animation.TryStart(videoControl.HeaderCover);
                        }
                        else
                        {
                            animation.TryStart(videoControl.showNameTextBlock);
                        }
                    }

                    break;
                }
            }
        }

        private void LoadShowInfo(Tuple<List<string>, string> typesAndName, bool isFuzzyQueryActor)
        {
            videoControl.ReLoadSearchResult(typesAndName.Item1, typesAndName.Item2, isFuzzyQueryActor);
        }

        /// <summary>
        /// 选项选中后跳转至详情页
        /// </summary>
        private void OnClicked(object sender, RoutedEventArgs e)
        {
            var item = (sender as Button)?.DataContext as VideoCoverDisplayClass;

            //准备动画
            //videoControl.PrepareAnimation(item);
            _storeditem = item;
            Frame.Navigate(typeof(DetailInfoPage), item, new EntranceNavigationTransitionInfo());
        }

        /// <summary>
        /// 视频播放页面跳转
        /// </summary>
        /// <param Name="sender"></param>
        /// <param Name="e"></param>
        private async void VideoPlay_Click(object sender, RoutedEventArgs e)
        {
            var videoPlayButton = (Button)sender;
            if (videoPlayButton.DataContext is not VideoCoverDisplayClass videoInfo) return;

            //播放失败列表
            if (videoInfo is FailVideoInfo failVideoInfo)
            {
                var mediaPlayItem = new MediaPlayItem(failVideoInfo);
                await PlayVideoHelper.PlayVideo(new List<MediaPlayItem> { mediaPlayItem }, this.XamlRoot, playType: PlayType.Fail);
                return;
            }

            var videoInfoList = DataAccess.Get.GetSingleFileInfoByTrueName(videoInfo.trueName);

            _storeditem = videoInfo;

            //没有
            if (videoInfoList == null || videoInfoList.Count == 0)
            {
                videoPlayButton.Flyout = new Flyout()
                {
                    Content = new TextBlock { Text = "经查询，本地数据库无该文件，请导入后继续" }
                };
            }
            else if (videoInfoList.Count == 1)
            {
                var mediaPlayItem = new MediaPlayItem(videoInfoList[0]);
                await PlayVideoHelper.PlayVideo(new List<MediaPlayItem>() { mediaPlayItem }, this.XamlRoot, lastPage: this);
            }

            //有多集
            else
            {
                //List<Datum> multisetList = new();
                //foreach (var info in videoInfoList)
                //{
                //    multisetList.Add(info);
                //}

                //multisetList = multisetList.OrderBy(item => item.n).ToList();

                //ContentsPage.DetailInfo.SelectVideoToPlay newPage = new(multisetList, videoInfo.truename);
                //newPage.ContentListView.ItemClick += ContentListView_ItemClick; ;

                //videoPlayButton.Flyout = new Flyout()
                //{
                //    Content = newPage
                //};

                PlayVideoHelper.ShowSelectedVideoToPlayPage(videoInfoList, XamlRoot);
            }
        }

        private async void ContentListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is not Data.Datum singleVideoInfo) return;

            if (sender is not ListView { DataContext: string trueName }) return;
            
            var mediaPlayItem = new MediaPlayItem(singleVideoInfo);
            await PlayVideoHelper.PlayVideo(new List<MediaPlayItem>() { mediaPlayItem }, XamlRoot, lastPage: this);
        }

        private async void SingleVideoPlayClick(object sender, RoutedEventArgs e)
        {
            if (sender is not Grid { DataContext: Datum datum }) return;

            var mediaPlayItem = new MediaPlayItem(datum);
            await PlayVideoHelper.PlayVideo(new List<MediaPlayItem> { mediaPlayItem }, this.XamlRoot, playType: PlayType.Fail);
        }
    }

}
