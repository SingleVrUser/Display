// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Display.Data;
using Display.Models;
using Display.Views;
using Display.WindowView;
using MediaPlayerElement_Test.Models;
using Microsoft.Graphics.Canvas;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Media.Streaming.Adaptive;
using Windows.Storage;
using Windows.System.Display;
using Windows.Web.Http;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.Controls;

public sealed partial class CustomMediaPlayerElement : UserControl
{
    public int IsLike;
    public int LookLater;

    public enum PlayType { success, fail }
    public event EventHandler<RoutedEventArgs> FullWindow;

    private WebApi _webApi;
    private List<MediaPlayItem> _allMediaPlayItems;
    private MediaPlayWindow _window;

    private DisplayRequest _appDisplayRequest = null;
    private readonly DispatcherQueue _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
    private HttpClient _httpClient;
    private int _qualityIndex = 1;
    private uint _playIndex = 0;

    public CustomMediaPlayerElement()
    {
        this.InitializeComponent();
    }

    public void InitLoad(List<MediaPlayItem> playItems, MediaPlayWindow window)
    {
        _allMediaPlayItems = playItems;
        _window = window;

        //m3u8UrlList
        _webApi ??= WebApi.GlobalWebApi;

        _httpClient = WebApi.CreateWindowWebHttpClient();

        SetMediaPlayer();


    }

    public void DisposeMediaPlayer()
    {
        if (MediaControl.MediaPlayer.Source is MediaPlaybackList mediaPlaybackList)
        {
            MediaControl.MediaPlayer.Pause();
            foreach (var mediaPlayItem in mediaPlaybackList.Items)
            {
                mediaPlayItem.Source.Dispose();
            }
        }
        else
        {
            MediaControl.MediaPlayer.Dispose();
        }

        //// 取消保持屏幕常亮
        //if (_appDisplayRequest != null)
        //{
        //    // Deactivate the display request and set the var to null.
        //    _appDisplayRequest.RequestRelease();
        //    _appDisplayRequest = null;
        //}

        _httpClient.Dispose();
    }

    private void SetMediaPlayer()
    {
        var mediaPlaybackList = new MediaPlaybackList();

        foreach (var playItem in _allMediaPlayItems)
        {
            var binder = new MediaBinder
            {
                Token = playItem.PickCode
            };
            binder.Binding += Binder_Binding;

            var mediaPlaybackItem = new MediaPlaybackItem(MediaSource.CreateFromMediaBinder(binder));

            var props = mediaPlaybackItem.GetDisplayProperties();
            props.Type = MediaPlaybackType.Video;
            props.VideoProperties.Title = playItem.Title;
            props.VideoProperties.Subtitle = playItem.Description;

            mediaPlaybackItem.ApplyDisplayProperties(props);

            mediaPlaybackList.Items.Add(mediaPlaybackItem);
        }

        if (_allMediaPlayItems.Count > 1)
        {
            mediaTransportControls.IsNextTrackButtonVisible = true;
            mediaTransportControls.IsPreviousTrackButtonVisible = true;
            mediaPlaybackList.MaxPlayedItemsToKeepOpen = 2;
        }

        mediaPlaybackList.StartingItem = mediaPlaybackList.Items[(int)_playIndex];
        mediaPlaybackList.CurrentItemChanged += MediaPlaybackList_CurrentItemChanged;
        var media = new MediaPlayer
        {
            Source = mediaPlaybackList,
        };

        MediaControl.SetMediaPlayer(media);

        //var mediaPlayerWithStreamSource = await MediaPlayerWithStreamSource.CreateMediaPlayer(url, subInfo: SubInfo);
        //var media = mediaPlayerWithStreamSource.MediaPlayer;

        ////先暂停后重新设置源，避免新的源设置失败之前的还在播放
        //if (MediaControl.MediaPlayer.Source != null && MediaControl.MediaPlayer.CurrentState == MediaPlayerState.Playing) MediaControl.MediaPlayer.Pause();

        //MediaControl.SetMediaPlayer(media);

        // 播放时修改Slider精度
        MediaControl.MediaPlayer.MediaOpened += MediaPlayer_MediaOpened;

        // 播放时保持屏幕常量，暂停播放则恢复
        MediaControl.MediaPlayer.PlaybackSession.PlaybackStateChanged += MediaPlayerElement_CurrentStateChanged;

        //_currentMediaPlayerWithStreamSource?.Dispose();
        //_currentMediaPlayerWithStreamSource = mediaPlayerWithStreamSource;
    }



    private bool _isChangedCurrentItem;

    private void MediaPlaybackList_CurrentItemChanged(MediaPlaybackList sender, CurrentMediaPlaybackItemChangedEventArgs args)
    {
        Debug.WriteLine("切换播放项");

        _isChangedCurrentItem = true;

        var index = sender.CurrentItemIndex;
        if (index > _allMediaPlayItems.Count - 1) return;

        _playIndex = index;

        var playItem = _allMediaPlayItems[(int)index];

        DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, ()=>
        {
            // 修改标题
            _window.ChangedWindowTitle($"播放- {playItem.Title}");
            mediaTransportControls.SetTitle(playItem.Title);

            _window.ChangedVideoListViewIndex((int)index);

            // 设置喜欢/稍后观看按钮
            SetButton(playItem);

            // 设置画质列表
            SetQualityList(playItem);
        });

        _isChangedCurrentItem = false;
    }

    private async void SetQualityList(MediaPlayItem playItem)
    {
        var list = await playItem.GetQualities();
        mediaTransportControls.SetQualityListSource(list,_qualityIndex);
    }

    private async void SetButton(MediaPlayItem playItem)
    {
        //设置喜欢、稍后观看
        bool isLike;
        bool isLookLater;

        var videoInfo = playItem.GetVideoInfo();

        // 先判断是否为成功，后判断是否为失败
        if(videoInfo == null)
        {
            var failInfo = await playItem.GetFailInfo();

            if (failInfo == null)
            {
                isLike = false;
                isLookLater = false;
            }
            else
            {
                IsLike = failInfo.is_like;
                LookLater = Convert.ToInt32(failInfo.look_later);

                isLike = IsLike == 1;
                isLookLater = LookLater != 0;
            }

            mediaTransportControls.SetLike_LookLater(isLike, isLookLater);

            mediaTransportControls.TrySetScreenButton();

        }
        else
        {
            IsLike = videoInfo.is_like;
            LookLater = Convert.ToInt32(videoInfo.look_later);

            isLike = this.IsLike == 1;
            isLookLater = this.LookLater != 0;

            mediaTransportControls.SetLike_LookLater(isLike, isLookLater);

            mediaTransportControls.DisableScreenButton();
        }

    }
    
    private async void Binder_Binding(MediaBinder sender, MediaBindingEventArgs args)
    {
        var deferral = args.GetDeferral();

        var content = sender.Token;
        var firstOrDefault = _allMediaPlayItems.FirstOrDefault(x => x.PickCode == content);
        if (firstOrDefault == null) return;

        var videoUrl = await firstOrDefault.GetUrl(_qualityIndex);

        if (string.IsNullOrEmpty(videoUrl)) return;

        //SetAdaptiveMediaSource
        if (videoUrl.Contains(".m3u8"))
        {
            var result = await AdaptiveMediaSource.CreateFromUriAsync(new Uri(videoUrl), _httpClient);

            if (result.Status == AdaptiveMediaSourceCreationStatus.Success && result.MediaSource != null)
            {
                args.SetAdaptiveMediaSource(result.MediaSource);

            }
        }
        //SetStream
        else
        {
            var httpClient = WebApi.CreateWindowWebHttpClient();
            var stream = await HttpRandomAccessStream.CreateAsync(httpClient, new Uri(videoUrl));

            if (stream.CanRead)
            {
                args.SetStream(stream,"video/mp4");
            }
        }

        deferral.Complete();
    }

    private void MediaPlayerElement_CurrentStateChanged(MediaPlaybackSession sender, object args)
    {
        var playbackSession = sender;
        if (playbackSession != null && playbackSession.NaturalVideoHeight != 0)
        {
            if (playbackSession.PlaybackState == MediaPlaybackState.Playing)
            {
                if (_appDisplayRequest is null)
                {
                    _dispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, () =>
                    {
                        _appDisplayRequest = new DisplayRequest();
                        _appDisplayRequest.RequestActive();
                    });
                }
            }
            else // PlaybackState is Buffering, None, Opening, or Paused.
            {
                if (_appDisplayRequest != null)
                {
                    // Deactivate the display request and set the var to null.
                    _appDisplayRequest.RequestRelease();
                    _appDisplayRequest = null;
                }
            }
        }
    }

    private void MediaPlayer_MediaOpened(MediaPlayer sender, object args)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            var transportControlsTemplateRoot = (FrameworkElement)VisualTreeHelper.GetChild(MediaControl.TransportControls, 0);
            var sliderControl = (Slider)transportControlsTemplateRoot.FindName("ProgressSlider");
            if (sliderControl != null && sender.PlaybackSession.NaturalDuration.TotalSeconds > 1000)
            {
                // 十秒一步
                sliderControl.StepFrequency = 1000 / sender.PlaybackSession.NaturalDuration.TotalSeconds;
                sliderControl.SmallChange = 1000 / sender.PlaybackSession.NaturalDuration.TotalSeconds;
            }
        });
    }

    private void QualityChanged(object sender, SelectionChangedEventArgs e)
    {
        //设置currentItem时不计入改变
        if (_isChangedCurrentItem) return;


        if (sender is not ListView listView) return;
        if (listView.ItemsSource is not List<Quality> list) return;
        if (e.AddedItems.FirstOrDefault() is not Quality quality) return;

        _qualityIndex = list.IndexOf(quality);

        //记录当前的时间
        var time = MediaControl.MediaPlayer.Position;

        // 先销毁
        if (MediaControl.MediaPlayer.Source is MediaPlaybackList mediaPlaybackList)
        {
            MediaControl.MediaPlayer.Pause();
            foreach (var mediaPlayItem in mediaPlaybackList.Items)
            {
                mediaPlayItem.Source.Dispose();
            }
        }

        // 后重新设置
        SetMediaPlayer();

        //恢复之前的时间
        MediaControl.MediaPlayer.Position = time;
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

        var playItem = _allMediaPlayItems[(int)_playIndex];

        var pickCode = playItem.PickCode;
        var trueName = playItem.TrueName;

        if (string.IsNullOrEmpty(pickCode)) return;

        IsLike = button.IsChecked == true ? 1 : 0;

        var videoInfo = playItem.GetVideoInfo();

        if (videoInfo != null)
        {
            DataAccess.UpdateSingleDataFromVideoInfo(trueName, "is_like", IsLike.ToString());

            if (IsLike == 1) ShowTeachingTip("已添加进喜欢");
        }
        else
        {
            Debug.WriteLine($"数据库不存在该数据:{trueName}");

            var failInfo = await playItem.GetFailInfo();

            if (failInfo == null)
            {
                var capPath = await ScreenShotAsync(pickCode);
                DataAccess.AddOrReplaceFailList_islike_looklater(new()
                {
                    pc = pickCode,
                    is_like = IsLike,
                    image_path = capPath
                });

                if (IsLike == 1) ShowTeachingTip("已添加进喜欢");
            }
            else
            {
                DataAccess.UpdateSingleFailInfo(pickCode, "is_like", IsLike.ToString());

                //需要截图
                if (failInfo.image_path == Const.NoPicturePath || !File.Exists(failInfo.image_path))
                {
                    var capPath = await ScreenShotAsync(pickCode);
                    DataAccess.UpdateSingleFailInfo(pickCode, "image_path", capPath);

                    if (IsLike == 1) ShowTeachingTip("已添加进喜欢，并截取当前画面作为封面");
                }
                else
                {
                    if (IsLike == 1) ShowTeachingTip("已添加进喜欢");
                }
            }
        }
    }

    private async void LookLaterButtonClick(object sender, RoutedEventArgs e)
    {
        if (sender is not AppBarToggleButton button) return;

        var playItem = _allMediaPlayItems[(int)_playIndex];
        var pickCode = playItem.PickCode;
        var trueName = playItem.TrueName;

        if (string.IsNullOrEmpty(pickCode)) return;


        LookLater = button.IsChecked == true ? Convert.ToInt32(DateTimeOffset.Now.ToUnixTimeSeconds()) : 0;

        var videoInfo = playItem.GetVideoInfo();

        if (videoInfo != null)
        {
            DataAccess.UpdateSingleDataFromVideoInfo(trueName, "look_later", LookLater.ToString());

            if (LookLater != 0) ShowTeachingTip("已添加进稍后观看");
        }else
        {
            var failInfo = await playItem.GetFailInfo();

            if (failInfo == null)
            {
                var capPath = await ScreenShotAsync(pickCode);
                DataAccess.AddOrReplaceFailList_islike_looklater(new()
                {
                    pc = pickCode,
                    look_later = LookLater,
                    image_path = capPath
                });

                if (LookLater != 0) ShowTeachingTip("已添加进稍后观看");
            }
            else
            {
                DataAccess.UpdateSingleFailInfo(pickCode, "look_later", LookLater.ToString());

                //需要添加截图
                if (failInfo.image_path == Const.NoPicturePath || !File.Exists(failInfo.image_path))
                {
                    var capPath = await ScreenShotAsync(pickCode);
                    DataAccess.UpdateSingleFailInfo(pickCode, "image_path", capPath);

                    if (LookLater != 0) ShowTeachingTip("已添加进稍后观看，并截取当前画面作为封面");
                }
                else
                {
                    if (LookLater != 0) ShowTeachingTip("已添加进稍后观看");
                }
            }
        }
    }
    private async void ScreenShotButtonClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button) return;

        var playItem = _allMediaPlayItems[(int)_playIndex];
        var pickCode = playItem.PickCode;

        var failInfo = await DataAccess.LoadSingleFailInfo(pickCode);

        var capPath = await ScreenShotAsync(pickCode);

        if (failInfo == null)
        {
            DataAccess.AddOrReplaceFailList_islike_looklater(new()
            {
                pc = pickCode,
                image_path = capPath
            });
        }
        else
        {
            DataAccess.UpdateSingleFailInfo(pickCode, "image_path", capPath);
        }

        ShowTeachingTip("已截取当前画面作为封面");
    }

    public async Task<string> ScreenShotAsync(string PickCode)
    {
        string savePath = Path.Combine(AppSettings.ImageSavePath, "Screen");
        if (!Directory.Exists(savePath)) Directory.CreateDirectory(savePath);
        StorageFolder storageFolder = await StorageFolder.GetFolderFromPathAsync(savePath);
        StorageFile file = await storageFolder.CreateFileAsync($"{PickCode}.png", CreationCollisionOption.ReplaceExisting);
        var fileStream = await file.OpenAsync(FileAccessMode.ReadWrite);

        var canvasRenderTarget = new CanvasRenderTarget(
                CanvasDevice.GetSharedDevice(),
                MediaControl.MediaPlayer.PlaybackSession.NaturalVideoWidth,
                MediaControl.MediaPlayer.PlaybackSession.NaturalVideoHeight,
                96);
        MediaControl.MediaPlayer.CopyFrameToVideoSurface(canvasRenderTarget);

        var targetFileStream = fileStream.AsStreamForWrite();
        await using (targetFileStream)
        {
            var stream = targetFileStream.AsRandomAccessStream();
            await canvasRenderTarget.SaveAsync(stream, CanvasBitmapFileFormat.Png);
        }

        return file.Path;
    }

    private async void PlayerChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.FirstOrDefault() is not Player player) return;

        await _webApi.PlayVideoWithPlayer(_allMediaPlayItems, player.PlayMethod, XamlRoot);
    }

    private void ShowTeachingTip(string subTitle)
    {
        BasePage.ShowTeachingTip(PlayerTeachingTip,subTitle);
    }

    private void OnOnApplyTemplateCompleted(object sender, EventArgs e)
    {
        mediaTransportControls.InitQuality(Resources["QualityDataTemplate"] as DataTemplate);

        mediaTransportControls.InitPlayer(Resources["PlayerDataTemplate"] as DataTemplate);

        if (_allMediaPlayItems.Count > 1)
        {
            mediaTransportControls.SetRightButton();
        }
    }

    public event EventHandler<RoutedEventArgs> RightButtonClick;
    private void MediaTransportControls_OnRightButtonClick(object sender, RoutedEventArgs e)
    {
        RightButtonClick?.Invoke(sender,e);
    }
}
