using System.Collections.Generic;
using DataAccess.Dao.Interface;
using DataAccess.Models.Entity;
using Display.Controls.UserController;
using Display.Helper.Network;
using Display.Models.Dto.Media;
using Display.Models.Vo;
using Display.Models.Vo.OneOneFive;
using Display.Providers;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using VideoCoverVo = Display.Models.Vo.Video.VideoCoverVo;

namespace Display.Views.Pages;

public sealed partial class VideoViewPage : Page
{
    private readonly IFileInfoDao _filesInfoDao = App.GetService<IFileInfoDao>();
    
    //为返回动画做准备（需启动缓存）
    private VideoCoverVo _storeditem;
    public VideoViewPage()
    {
        InitializeComponent();

        //启动缓存（为了返回无需过长等待，也为了返回动画）
        NavigationCacheMode = NavigationCacheMode.Enabled;
    }

    private async void SingleVideoPlay_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Grid videoPlayGrid) return;

        MediaPlayItem mediaPlayItem;
        switch (videoPlayGrid.DataContext)
        {
            case FileInfo datum:
                mediaPlayItem = new MediaPlayItem(datum);
                break;
            default:
                return;
        }

        if (string.IsNullOrEmpty(mediaPlayItem.PickCode)) return;

        await PlayVideoHelper.PlayVideo(new List<MediaPlayItem> { mediaPlayItem }, XamlRoot, playType: CustomMediaPlayerElement.PlayType.Fail);
    }

    /// <summary>
    /// 选项选中后跳转至详情页
    /// </summary>
    private void OnClicked(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { DataContext: VideoCoverVo item }) return;

        //准备动画
        //videoControl.PrepareAnimation(item);
        _storeditem = item;
        Frame.Navigate(typeof(DetailInfoPage), item, new SuppressNavigationTransitionInfo());
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

        var fileInfoList = _filesInfoDao.GetFileInfoListByVideoInfoId(videoInfo.Id);

        //没有
        if (fileInfoList.Count == 0)
        {
            videoPlayButton.Flyout = new Flyout
            {
                Content = new TextBlock { Text = "经查询，本地数据库无该文件，请导入后继续" }
            };
        }
        else if (fileInfoList.Count == 1)
        {
            _storeditem = videoInfo;

            var mediaPlayItem = new MediaPlayItem(new DetailFileInfo(fileInfoList[0]));
            await PlayVideoHelper.PlayVideo(new List<MediaPlayItem> { mediaPlayItem }, XamlRoot, lastPage: this);
            ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Arrow);
        }
        //有多集
        else
        {
            _storeditem = videoInfo;

            PlayVideoHelper.ShowSelectedVideoToPlayPage(fileInfoList, XamlRoot);

        }
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        var animation = ConnectedAnimationService.GetForCurrentView().GetAnimation("BackConnectedAnimation");
        if (animation == null) return;

        //开始动画
        if (_storeditem != null)
        {
            //开始动画
            VideoControl.StartAnimation(animation, _storeditem);
        }
        else
        {
            animation.TryStart(VideoControl.PageShow_Grid);
        }
    }

}