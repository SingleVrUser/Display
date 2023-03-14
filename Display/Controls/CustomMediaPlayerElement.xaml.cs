// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Data;
using Microsoft.Graphics.Canvas;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.UserDataTasks;
using Windows.Foundation;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.Streams;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.Controls;

public sealed partial class CustomMediaPlayerElement : UserControl
{

    public static readonly DependencyProperty PickCodeProperty =
        DependencyProperty.Register(nameof(PickCode), typeof(string), typeof(CustomMediaPlayerElement), null);

    public static readonly DependencyProperty playTypeProperty =
        DependencyProperty.Register(nameof(playType), typeof(PlayType), typeof(CustomMediaPlayerElement), null);

    public static readonly DependencyProperty TrueNameProperty =
        DependencyProperty.Register(nameof(TrueName), typeof(string), typeof(CustomMediaPlayerElement), null);

    public static readonly DependencyProperty SubInfoProperty =
        DependencyProperty.Register(nameof(SubInfo), typeof(SubInfo), typeof(CustomMediaPlayerElement), null);



    public int IsLike;
    public int LookLater;

    public enum PlayType { success, fail }

    private WebApi webApi;


    public string TrueName
    {
        get => (string)GetValue(TrueNameProperty);
        set => SetValue(TrueNameProperty, value);
    }

    public string PickCode
    {
        get => (string)GetValue(PickCodeProperty);
        set => SetValue(PickCodeProperty, value);
    }

    public PlayType playType
    {
        get => (PlayType)GetValue(playTypeProperty);
        set => SetValue(playTypeProperty, value);
    }

    public SubInfo SubInfo
    {
        get => (SubInfo)GetValue(SubInfoProperty);
        set => SetValue(SubInfoProperty, value);
    }

    Dictionary<string, string> subDicts;

    public event EventHandler<RoutedEventArgs> FullWindow;

    public CustomMediaPlayerElement()
    {
        this.InitializeComponent();

        this.Loaded += CustomMediaPlayerElement_Loaded;
    }

    private async void CustomMediaPlayerElement_Loaded(object sender, RoutedEventArgs e)
    {
        if (PickCode == null) return;


        //设置画质

        //m3u8UrlList
        webApi ??= WebApi.GlobalWebApi;

        //先原画
        List<Quality> QualityItemsSource = new() { new("原画", pickCode: PickCode) };
        var m3u8InfoList = await webApi.Getm3u8InfoByPickCode(PickCode);
        string url = null;
        //有m3u8
        if (m3u8InfoList != null && m3u8InfoList.Count > 0)
        {
            //后m3u8
            m3u8InfoList.ForEach(item => QualityItemsSource.Add(new(item.Name, item.Url)));

            url = m3u8InfoList.FirstOrDefault()?.Url;
        }
        //没有m3u8，获取下载链接播放
        else
        {
            var downUrlList = webApi.GetDownUrl(PickCode,GetInfoFromNetwork.MediaElementUserAgent);

            if (downUrlList.Count>0)
            {
                url = downUrlList.FirstOrDefault().Value;
            }
        }

        mediaTransportControls.SetQuality(QualityItemsSource, this.Resources["QualityDataTemplate"] as DataTemplate);

        //设置播放器
        List<Player> plsyerSoucre = new() { new(WebApi.playMethod.vlc, pickCode: PickCode),
                                            new(WebApi.playMethod.mpv, pickCode: PickCode),
                                            new(WebApi.playMethod.pot, pickCode: PickCode)};
        mediaTransportControls.SetPlayer(plsyerSoucre, this.Resources["PlayerDataTemplate"] as DataTemplate);
        
        //设置喜欢、稍后观看
        bool isLike;
        bool isLookLater;
        if (playType == PlayType.fail)
        {
            var failInfo = await DataAccess.LoadSingleFailInfo(PickCode);
            if (failInfo != null)
            {

                isLike = this.IsLike == 1;
                isLookLater = this.LookLater != 0;
            }
            else
            {
                isLike = false;
                isLookLater = false;
            }

            mediaTransportControls.SetLike_LookLater(isLike, isLookLater);

            mediaTransportControls.SetScreenButton();
        }
        else
        {
            var videoInfo = DataAccess.LoadOneVideoInfoByCID(TrueName);

            //有该数据才可用Like和LookLater
            if (videoInfo != null)
            {
                this.IsLike = videoInfo.is_like;
                this.LookLater = Convert.ToInt32(videoInfo.look_later);

                isLike = this.IsLike == 1;
                isLookLater = this.LookLater != 0;

                mediaTransportControls.SetLike_LookLater(isLike, isLookLater);
            }
        }


        await SetMediaPlayer(url);

        this.Loaded -= CustomMediaPlayerElement_Loaded;
    }

    public void DisposeMediaPlayer()
    {
        //MediaControl.MediaPlayer.Pause();
        //MediaControl.Source = null;
        MediaControl.MediaPlayer.Dispose();

    }

    private async Task SetMediaPlayer(string url)
    {
        if (string.IsNullOrEmpty(url)) return;

        //添加播放链接
        var ms = MediaSource.CreateFromUri(new Uri(url));

        var media = new MediaPlayer();

        //添加字幕文件
        if (SubInfo != null)
        {
            //ms.ExternalTimedTextSources.Clear();

            //下载字幕
            var subPath = await webApi.TryDownSubFile(SubInfo.name, SubInfo.pickcode);

            if (!string.IsNullOrEmpty(subPath))
            {
                var storageFile = await StorageFile.GetFileFromPathAsync(subPath);
                IRandomAccessStream stream = await storageFile.OpenReadAsync();

                TimedTextSource txtsrc = TimedTextSource.CreateFromStream(stream, SubInfo.name);
                ms.ExternalTimedTextSources.Add(txtsrc);
            }

            var playbackItem = new MediaPlaybackItem(ms);


            if (ms.ExternalTimedTextSources.Count > 0)
            {
                Debug.WriteLine("发现字幕文件");

                playbackItem.TimedMetadataTracksChanged += (item, args) =>
                {
                    playbackItem.TimedMetadataTracks.SetPresentationMode(0, TimedMetadataTrackPresentationMode.PlatformPresented);
                    
                    Debug.WriteLine("默认选中第一个字幕");

                };

            }
            else
            {
                Debug.WriteLine("没有字幕文件");
            }

            media.Source = playbackItem;

        }
        else
        {
            media.Source = ms;
        }

        //先暂停后重新设置源，避免新的源设置失败之前的还在播放
        if (MediaControl.MediaPlayer.Source != null && MediaControl.MediaPlayer.CurrentState == MediaPlayerState.Playing) MediaControl.MediaPlayer.Pause();
        MediaControl.SetMediaPlayer(media);
    }


    private async void QualityChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.FirstOrDefault() is not Quality quality) return;

        string url = null;
        //m3u8播放
        if (quality.Url != null)
        {
            url = quality.Url;
        }
        //原画播放
        else if (quality.Url == null && quality.PickCode != null)
        {
            var downUrlList = webApi.GetDownUrl(PickCode, GetInfoFromNetwork.MediaElementUserAgent);

            if (downUrlList.Count == 0) return;
            url = downUrlList.FirstOrDefault().Value;

            //url = "https://d99fecf2-385d-4ce3-a85c-c7ec1c7e8627.mock.pstmn.io/video";

            //避免重复获取
            quality.Url = url;
        }

        //播放
        if (url != null)
        {
            //记录当前的时间
            var time = MediaControl.MediaPlayer.Position;

            await SetMediaPlayer(url);

            //恢复之前的时间
            MediaControl.MediaPlayer.Position = time;
        }
    }
    
    private void mediaControls_FullWindow(object sender, RoutedEventArgs e)
    {
        FullWindow?.Invoke(sender, e);
    }

    public event DoubleTappedEventHandler MediaDoubleTapped;
    private void MediaControl_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        MediaDoubleTapped?.Invoke(sender, e);
    }

    private async void LikeButtonClick(object sender, RoutedEventArgs e)
    {
        if (sender is not AppBarToggleButton button) return;

        IsLike = button.IsChecked == true ? 1 : 0;

        if (playType == PlayType.fail)
        {
            var failInfo = await DataAccess.LoadSingleFailInfo(PickCode);

            if (failInfo == null)
            {
                var capPath = await ScreenshotAsync(PickCode);
                DataAccess.AddOrReplaceFailList_islike_looklater(new()
                {
                    pc = PickCode,
                    is_like = IsLike,
                    image_path = capPath
                });

                if (IsLike == 1) ShowTeachingTip("已添加进喜欢");
            }
            else
            {
                DataAccess.UpdateSingleFailInfo(PickCode, "is_like", IsLike.ToString());

                //需要截图
                if (failInfo.image_path == Const.NoPictruePath || !File.Exists(failInfo.image_path))
                {
                    var capPath = await ScreenshotAsync(PickCode);
                    DataAccess.UpdateSingleFailInfo(PickCode, "image_path", capPath);

                    if (IsLike == 1) ShowTeachingTip("已添加进喜欢，并截取当前画面作为封面");
                }
                else
                {
                    if (IsLike == 1) ShowTeachingTip("已添加进喜欢");
                }
            }
        }
        else
        {
            var videoInfo = DataAccess.LoadOneVideoInfoByCID(TrueName);

            if(videoInfo != null)
            {
                DataAccess.UpdateSingleDataFromVideoInfo(TrueName, "is_like", IsLike.ToString());

                if (IsLike == 1) ShowTeachingTip("已添加进喜欢");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"数据库不存在该数据:{TrueName}");
            }

        }

        

    }

    private async void LookLaterButtonClick(object sender, RoutedEventArgs e)
    {
        if (sender is not AppBarToggleButton button) return;

        var failInfo = await DataAccess.LoadSingleFailInfo(PickCode);

        LookLater = button.IsChecked == true ? Convert.ToInt32(DateTimeOffset.Now.ToUnixTimeSeconds()) : 0;

        if (playType == PlayType.fail)
        {
            if (failInfo == null)
            {
                var capPath = await ScreenshotAsync(PickCode);
                DataAccess.AddOrReplaceFailList_islike_looklater(new()
                {
                    pc = PickCode,
                    look_later = LookLater,
                    image_path = capPath
                });

                if (LookLater != 0) ShowTeachingTip("已添加进稍后观看");
            }
            else
            {
                DataAccess.UpdateSingleFailInfo(PickCode, "look_later", LookLater.ToString());

                //需要添加截图
                if (failInfo.image_path == Data.Const.NoPictruePath || !File.Exists(failInfo.image_path))
                {
                    var capPath = await ScreenshotAsync(PickCode);
                    DataAccess.UpdateSingleFailInfo(PickCode, "image_path", capPath);

                    if (LookLater != 0) ShowTeachingTip("已添加进稍后观看，并截取当前画面作为封面");
                }
                else
                {
                    if (LookLater != 0) ShowTeachingTip("已添加进稍后观看");
                }
            }
        }
        else
        {
            var videoInfo = DataAccess.LoadOneVideoInfoByCID(TrueName);

            if (videoInfo != null)
            {
                DataAccess.UpdateSingleDataFromVideoInfo(TrueName, "look_later", LookLater.ToString());

                if (LookLater != 0) ShowTeachingTip("已添加进稍后观看");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"数据库不存在该数据:{TrueName}");
            }
        }

        
    }
    private async void ScreenshotButtonClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button) return;

        var failInfo = await DataAccess.LoadSingleFailInfo(PickCode);

        var capPath = await ScreenshotAsync(PickCode);

        if (failInfo == null)
        {
            DataAccess.AddOrReplaceFailList_islike_looklater(new()
            {
                pc = PickCode,
                image_path = capPath
            });
        }
        else
        {
            DataAccess.UpdateSingleFailInfo(PickCode, "image_path", capPath);
        }

        ShowTeachingTip("已截取当前画面作为封面");
    }

    public async Task<string> ScreenshotAsync(string PickCode)
    {
        string savePath = Path.Combine(AppSettings.Image_SavePath, "Screen");
        if (!Directory.Exists(savePath)) Directory.CreateDirectory(savePath);
        StorageFolder storageFolder = await StorageFolder.GetFolderFromPathAsync(savePath);
        StorageFile file = await storageFolder.CreateFileAsync($"{PickCode}.png", CreationCollisionOption.ReplaceExisting);
        var fileStream = await file.OpenAsync(FileAccessMode.ReadWrite);

        var rendertarget = new CanvasRenderTarget(
                CanvasDevice.GetSharedDevice(),
                MediaControl.MediaPlayer.PlaybackSession.NaturalVideoWidth,
                MediaControl.MediaPlayer.PlaybackSession.NaturalVideoHeight,
                96);
        MediaControl.MediaPlayer.CopyFrameToVideoSurface(rendertarget);

        var targetFileStream = fileStream.AsStreamForWrite();
        using (targetFileStream)
        {
            var stream = targetFileStream.AsRandomAccessStream();
            await rendertarget.SaveAsync(stream, CanvasBitmapFileFormat.Png);
        }

        return file.Path;
    }

    private async void PlayerChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.FirstOrDefault() is not Player player) return;

        webApi ??= WebApi.GlobalWebApi;

        await webApi.PlayVideoWithOriginUrl(player.PickCode,player.PlayMethod,this.XamlRoot);
    }

    private async void ShowTeachingTip(string subTitle)
    {
        if (Player_TeachingTip.IsOpen) Player_TeachingTip.IsOpen = false;

        Player_TeachingTip.Subtitle = subTitle;
        Player_TeachingTip.IsOpen = true;

        await Task.Delay(1000);

        Player_TeachingTip.IsOpen = false;
    }
}
