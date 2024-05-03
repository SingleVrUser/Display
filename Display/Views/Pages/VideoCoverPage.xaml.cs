using System;
using System.Collections.Generic;
using System.Linq;
using DataAccess.Models.Entity;
using Display.Controls.UserController;
using Display.Helper.Network;
using Display.Models.Dto.Media;
using Display.Models.Vo;
using Display.Providers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;

namespace Display.Views.Pages;

public sealed partial class VideoCoverPage
{
    //为返回动画做准备（需启动缓存）
    private VideoCoverVo _storedItem;


    public VideoCoverPage()
    {
        //启动缓存（为了返回无需过长等待，也为了返回动画）
        NavigationCacheMode = NavigationCacheMode.Enabled;

        InitializeComponent();
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);

        //准备动画
        //限定为 返回操作
        if (e.NavigationMode != NavigationMode.Back) return;

        //指定页面
        if (e.SourcePageType != typeof(ActorsPage)) return;

        var animation = ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("BackConnectedAnimation", VideoControl.HeaderCover);

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
                var animation = ConnectedAnimationService.GetForCurrentView().GetAnimation("BackConnectedAnimation");
                if (animation != null)
                {
                    VideoControl.StartAnimation(animation, _storedItem);
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
                        animation.TryStart(VideoControl.HeaderCover);
                    }
                    else
                    {
                        animation.TryStart(VideoControl.showNameTextBlock);
                    }
                }

                break;
            }
            case NavigationMode.Forward:
                break;
            case NavigationMode.Refresh:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void LoadShowInfo(Tuple<List<string>, string> typesAndName, bool isFuzzyQueryActor)
    {
        VideoControl.ReLoadSearchResult(typesAndName.Item1, typesAndName.Item2, isFuzzyQueryActor);
    }

    /// <summary>
    /// 选项选中后跳转至详情页
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnClicked(object sender, RoutedEventArgs e)
    {
        var item = (sender as Button)?.DataContext as VideoCoverVo;

        //准备动画
        //videoControl.PrepareAnimation(item);
        _storedItem = item;
        Frame.Navigate(typeof(DetailInfoPage), item, new EntranceNavigationTransitionInfo());
    }

    /// <summary>
    /// 视频播放页面跳转
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void VideoPlay_Click(object sender, RoutedEventArgs e)
    {
        var videoPlayButton = (Button)sender;
        if (videoPlayButton.DataContext is not VideoCoverVo videoInfo) return;

        //播放失败列表
        if (videoInfo is FailVideoCover failVideoInfo)
        {
            var mediaPlayItem = new MediaPlayItem(failVideoInfo);
            await PlayVideoHelper.PlayVideo(new List<MediaPlayItem> { mediaPlayItem }, this.XamlRoot, playType: CustomMediaPlayerElement.PlayType.Fail);
            return;
        }

        var videoInfoList = DataAccessLocal.Get.GetSingleFileInfoByTrueName(videoInfo.Name);

        _storedItem = videoInfo;

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
            await PlayVideoHelper.PlayVideo([mediaPlayItem], XamlRoot, lastPage: this);
        }

        //有多集
        else
        {
            PlayVideoHelper.ShowSelectedVideoToPlayPage(videoInfoList, XamlRoot);
        }
    }

    private async void SingleVideoPlayClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Grid { DataContext: FilesInfo datum }) return;

        var mediaPlayItem = new MediaPlayItem(datum);
        await PlayVideoHelper.PlayVideo(new List<MediaPlayItem> { mediaPlayItem }, this.XamlRoot, playType: CustomMediaPlayerElement.PlayType.Fail);
    }
}