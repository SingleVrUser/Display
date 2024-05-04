using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Windows.Foundation;
using DataAccess.Dao.Interface;
using DataAccess.Models.Entity;
using Display.Helper.Network;
using Display.Models.Dto.Media;
using Display.Models.Dto.OneOneFive;
using Display.Models.Vo;
using Display.Models.Vo.OneOneFive;
using Display.Providers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Display.Models.Vo.Video;

namespace Display.Views.Pages;

public sealed partial class DetailInfoPage
{
    public VideoDetailVo VideoDetailVo;

    private readonly IFileInfoDao _filesInfoDao = App.GetService<IFileInfoDao>();
    private readonly IVideoInfoDao _videoInfoDao = App.GetService<IVideoInfoDao>();

    public DetailInfoPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        var anim = ConnectedAnimationService.GetForCurrentView().GetAnimation("ForwardConnectedAnimation");
        if (anim != null)
        {
            anim.TryStart(VideoDetailsControl.CoverImage);

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

        switch (e.Parameter)
        {
            case VideoCoverVo detailInfo:
            {
                var videoInfoId = detailInfo.Id;
                var videoInfo = _videoInfoDao.GetById(videoInfoId);
                VideoDetailVo = new VideoDetailVo(videoInfo);
                break;
            }
            case VideoInfo videoInfo:
                VideoDetailVo = new VideoDetailVo(videoInfo);
                break;
        }
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
            if (e.SourcePageType == typeof(VideoViewPage) || e.SourcePageType == typeof(HomePage) || e.SourcePageType == typeof(VideoCoverPage))
            {
                //无Cover_Image，退出
                //例：不存在Image中Source指向的图片文件
                if (VideoDetailsControl.CoverImage.DesiredSize == new Size(0, 0)) return;

                ConnectedAnimation animation = ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("BackConnectedAnimation", VideoDetailsControl.CoverImage);
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
        if (sender is not Grid clickButton) return;

        string actorName = null;
        if (clickButton.DataContext is string actorNameStr)
        {
            actorName = actorNameStr;
        }
        else if (clickButton.DataContext is ActorInfo actorInfo)
        {
            actorName = actorInfo.Name;
        }

        if (string.IsNullOrEmpty(actorName)) return;

        Tuple<List<string>, string, bool> typesAndName = new(["actor"], actorName, false);

        //准备动画
        ConnectedAnimation animation = ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("ForwardConnectedAnimation", clickButton);
        animation.Configuration = new BasicConnectedAnimationConfiguration();
        Frame.Navigate(typeof(VideoCoverPage), typesAndName);
    }

    /// <summary>
    /// 标签更多页跳转
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Label_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button clickButton) return;

        var labelName = clickButton.Content as string;
        Tuple<List<string>, string, bool> typesAndName = new(["category"], labelName, false);

        var animation = ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("ForwardConnectedAnimation", clickButton);
        Frame.Navigate(typeof(VideoCoverPage), typesAndName);
    }

    /// <summary>
    /// 视频播放页面跳转
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void VideoPlay_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button) return;

        await PlayVideo(VideoDetailVo.Id);
    }


    /// <summary>
    /// 点击了删除键
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void Delete_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new ContentDialog
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

        if (result != ContentDialogResult.Primary) return;

        if (sender is not AppBarButton) return;
        
        //从数据库中删除
        _filesInfoDao.ExecuteRemoveById(VideoDetailVo.Id);

        //删除存储的文件夹
        var savePath = Path.Combine(AppSettings.ImageSavePath, VideoDetailVo.Name);
        if (Directory.Exists(savePath))
        {
            Directory.Delete(savePath, true);
        }

        //// TODO 删除文件后，视图发生相应改变
        //VideoDetailVo.IsDeleted = Visibility.Visible;

        if (Frame.CanGoBack)
        {
            Frame.GoBack();
        }
    }


    private async void CoverTapped(object sender, TappedRoutedEventArgs e)
    {
        if (sender is not Grid) return;

        await PlayVideo(VideoDetailVo.Id);
    }

    private async Task PlayVideo(long videoInfoId)
    {
        var fileInfoList = _filesInfoDao.GetFileInfoListByVideoInfoId(videoInfoId);

        //没有该数据
        if (fileInfoList.Count == 0)
        {
            ContentDialog dialog = new()
            {
                XamlRoot = XamlRoot,
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                Title = "播放",
                CloseButtonText = "返回",
                Content = "经查询，本地数据库无该文件，请导入后继续",
                DefaultButton = ContentDialogButton.Close
            };
            await dialog.ShowAsync();
        }
        //一集
        else if (fileInfoList.Count == 1)
        {

            var mediaPlayItem = new MediaPlayItem(new DetailFileInfo(fileInfoList[0]));
            await PlayVideoHelper.PlayVideo(new List<MediaPlayItem> { mediaPlayItem }, XamlRoot, lastPage: this);
        }

        //有多集
        else
        {
            PlayVideoHelper.ShowSelectedVideoToPlayPage(fileInfoList, XamlRoot);
        }
    }
}