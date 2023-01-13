// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Data;
using Microsoft.Graphics.Canvas;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.Streams;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.Control;

public sealed partial class CustomMediaPlayerElement : UserControl
{
    public static readonly DependencyProperty PickCodeProperty =
        DependencyProperty.Register("PickCode", typeof(string), typeof(CustomMediaPlayerElement), null);

    public static readonly DependencyProperty playTypeProperty =
        DependencyProperty.Register("playType", typeof(PlayType), typeof(CustomMediaPlayerElement), null);

    public enum PlayType { success, fail }

    public string PickCode
    {
        get { return (string)GetValue(PickCodeProperty); }
        set { SetValue(PickCodeProperty, value); }
    }

    public PlayType playType
    {
        get { return (PlayType)GetValue(playTypeProperty); }
        set { SetValue(playTypeProperty, value); }
    }

    Dictionary<string, string> subDicts;

    private WebApi webApi;

    public event EventHandler<RoutedEventArgs> FullWindow;

    public CustomMediaPlayerElement()
    {
        this.InitializeComponent();

        this.Loaded += CustomMediaPlayerElement_Loaded;
    }

    private async void CustomMediaPlayerElement_Loaded(object sender, RoutedEventArgs e)
    {
        if (PickCode == null) return;

        //m3u8UrlList
        if (webApi == null) webApi = new();


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

        if (AppSettings.IsFindSub)
        {
            subDicts = DataAccess.FindSubFile(PickCode);
        }

        if(playType == PlayType.fail)
        {
            bool isLike;
            bool isLookLater;
            var failInfo = await DataAccess.LoadSingleFailInfo(PickCode);
            if (failInfo != null)
            {
                isLike = failInfo.is_like == 1 ? true : false;
                isLookLater = failInfo.look_later == 0 ? false : true;
            }
            else
            {
                isLike = false;
                isLookLater = false;
            }

            mediaTransportControls.SetFailPlayType(isLike, isLookLater);
        }

        await SetMediaPlayer(url, subDicts);
    }

    public void StopMediaPlayer()
    {
        MediaControl.MediaPlayer.Pause();
        MediaControl.Source = null;
    }

    private async Task SetMediaPlayer(string url, Dictionary<string,string> subDicts = null)
    {
        if (string.IsNullOrEmpty(url)) return;

        //添加播放链接
        MediaSource ms = MediaSource.CreateFromUri(new Uri(url));

        var media = new MediaPlayer();

        //添加字幕文件
        if (subDicts != null && subDicts.Count!=0)
        {
            ms.ExternalTimedTextSources.Clear();

            foreach (var item in subDicts)
            {
                string subPickCode = item.Key;
                string subName = item.Value;

                //下载字幕
                string subPath = await webApi.TryDownSubFile(subName, subPickCode);

                if (!string.IsNullOrEmpty(subPath))
                {
                    StorageFile srtfile = await StorageFile.GetFileFromPathAsync(subPath);
                    IRandomAccessStream stream = await srtfile.OpenReadAsync();

                    TimedTextSource txtsrc = TimedTextSource.CreateFromStream(stream, subName);
                    ms.ExternalTimedTextSources.Add(txtsrc);
                }
            }

            var playbackItem = new MediaPlaybackItem(ms);

            if (playbackItem.TimedMetadataTracks.Count > 0)
            {
                //选择默认字幕
                media.BufferingStarted += (sender, e) =>
                {
                    playbackItem.TimedMetadataTracks.SetPresentationMode(0, TimedMetadataTrackPresentationMode.PlatformPresented);
                };
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

    private async void mediaControls_QualityChanged(object sender, SelectionChangedEventArgs e)
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

            await SetMediaPlayer(url, subDicts);

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

        var failInfo = await DataAccess.LoadSingleFailInfo(PickCode);

        int isLike = button.IsChecked == true ? 1 : 0;

        if (failInfo == null)
        {
            var capPath = await ScreenshotAsync(PickCode);
            DataAccess.AddOrReplaceFailList_islike_looklater(new()
            {
                pc = PickCode,
                is_like = isLike,
                image_path = capPath
            }) ;
        }
        else
        {
            DataAccess.UpdateSingleFailInfo(PickCode, "is_like", isLike.ToString());

            if (string.IsNullOrEmpty(failInfo.image_path) || !File.Exists(failInfo.image_path))
            {
                var capPath = await ScreenshotAsync(PickCode);
                DataAccess.UpdateSingleFailInfo(PickCode, "image_path", capPath);
            }
        }

    }

    private async void LookLaterButtonClick(object sender, RoutedEventArgs e)
    {
        if (sender is not AppBarToggleButton button) return;

        var failInfo = await DataAccess.LoadSingleFailInfo(PickCode);

        int look_later = button.IsChecked == true ? Convert.ToInt32(DateTimeOffset.Now.ToUnixTimeSeconds()) : 0;

        if (failInfo == null)
        {
            var capPath = await ScreenshotAsync(PickCode);
            DataAccess.AddOrReplaceFailList_islike_looklater(new()
            {
                pc = PickCode,
                look_later = look_later,
                image_path = capPath
            });
        }
        else
        {
            DataAccess.UpdateSingleFailInfo(PickCode, "look_later", look_later.ToString());

            if (string.IsNullOrEmpty(failInfo.image_path) || !File.Exists(failInfo.image_path))
            {
                var capPath = await ScreenshotAsync(PickCode);
                DataAccess.UpdateSingleFailInfo(PickCode, "image_path", capPath);
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

}
