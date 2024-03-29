// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using ByteSizeLib;
using Display.Models.Data;
using Display.Models.Media;
using Display.Services;
using Display.Views;
using Microsoft.Graphics.Canvas;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using SharpCompress;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Display.CustomWindows;
using Display.Constants;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.Controls;

public sealed partial class CustomMediaPlayerElement
{
    public int IsLike;
    public int LookLater;

    public enum PlayType { Success, Fail }
    public event EventHandler<RoutedEventArgs> FullWindow;

    private WebApi _webApi;
    private ObservableCollection<MediaPlayItem> _allMediaPlayItems;
    private MediaPlaybackList _mediaPlaybackList;
    private MediaPlayWindow _window;

    private DisplayRequest _appDisplayRequest;
    private readonly DispatcherQueue _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
    private HttpClient _httpClient;
    private int _qualityIndex = 1;
    private uint _playIndex = 0;

    private List<AdaptiveMediaSourceCreationResult> _adaptiveMediaSourceList;
    private List<HttpRandomAccessStream> _httpRandomAccessStreamList;

    public CustomMediaPlayerElement()
    {
        this.InitializeComponent();
    }

    public void InitLoad(IList<MediaPlayItem> playItems, MediaPlayWindow window)
    {
        _allMediaPlayItems = new ObservableCollection<MediaPlayItem>(playItems);
        _window = window;

        //m3u8UrlList
        _webApi ??= WebApi.GlobalWebApi;
        _httpClient = WebApi.SingleVideoWindowWebHttpClient;

        _adaptiveMediaSourceList = new List<AdaptiveMediaSourceCreationResult>();
        _httpRandomAccessStreamList = new List<HttpRandomAccessStream>();
        _mediaPlaybackList = new MediaPlaybackList();
        SetMediaPlayer();
    }

    public IList<MediaPlayItem> ReLoad(IList<MediaPlayItem> playItems)
    {
        playItems.ForEach(item => _allMediaPlayItems.Add(item));

        SetMediaPlayItems(playItems);

        return _allMediaPlayItems;
    }


    public void DisposeMediaPlayer()
    {
        MediaControl.MediaPlayer.MediaOpened -= MediaPlayer_MediaOpened;
        MediaControl.MediaPlayer.PlaybackSession.PlaybackStateChanged -= MediaPlayerElement_CurrentStateChanged;

        if (MediaControl.MediaPlayer.Source is MediaPlaybackList mediaPlaybackList)
        {
            mediaPlaybackList.CurrentItemChanged -= MediaPlaybackList_CurrentItemChanged;

            DisposeMediaPlayer(mediaPlaybackList);

            // TODO 会报错
            //MediaControl.MediaPlayer.Dispose();
        }
        else
        {
            MediaControl.MediaPlayer.Dispose();
        }
    }

    private void DisposeMediaPlayer(MediaPlaybackList mediaPlaybackList)
    {
        MediaControl.MediaPlayer.Pause();

        MediaControl.MediaPlayer.Source = null;
        foreach (var mediaPlayItem in mediaPlaybackList.Items)
        {
            mediaPlayItem.Source.Dispose();
        }
        mediaPlaybackList.Items.Clear();

        foreach (var source in _adaptiveMediaSourceList)
        {
            source.MediaSource.Dispose();
        }
        _adaptiveMediaSourceList.Clear();


    }

    private bool _isHandlerCurrentItemChanged;
    private void SetMediaPlayer()
    {
        SetMediaPlayItems(_allMediaPlayItems);

        _mediaPlaybackList.StartingItem = _mediaPlaybackList.Items[(int)_playIndex];

        if (!_isHandlerCurrentItemChanged)
        {
            _mediaPlaybackList.CurrentItemChanged += MediaPlaybackList_CurrentItemChanged;
            _isHandlerCurrentItemChanged = true;
        }

        var mediaPlayer = new MediaPlayer
        {
            Source = _mediaPlaybackList,
        };

        MediaControl.SetMediaPlayer(mediaPlayer);

        // 播放时修改Slider精度
        MediaControl.MediaPlayer.MediaOpened += MediaPlayer_MediaOpened;

        // 播放时保持屏幕常亮，暂停播放则恢复
        MediaControl.MediaPlayer.PlaybackSession.PlaybackStateChanged += MediaPlayerElement_CurrentStateChanged;
    }

    private void SetMediaPlayItems(IEnumerable<MediaPlayItem> items)
    {
        foreach (var playItem in items)
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

            playItem.MediaPlaybackItem = mediaPlaybackItem;

            mediaPlaybackItem.ApplyDisplayProperties(props);

            _mediaPlaybackList.Items.Add(mediaPlaybackItem);
        }

        if (_allMediaPlayItems.Count <= 1) return;

        mediaTransportControls.IsNextTrackButtonVisible = true;
        mediaTransportControls.IsPreviousTrackButtonVisible = true;
        _mediaPlaybackList.AutoRepeatEnabled = true;
    }

    private bool _isChangedCurrentItem;
    private bool _isBindingCurrentItem;

    private void MediaPlaybackList_CurrentItemChanged(MediaPlaybackList sender, CurrentMediaPlaybackItemChangedEventArgs args)
    {
        if (args.NewItem == null) return;

        Debug.WriteLine("切换播放项");

        _isChangedCurrentItem = true;

        var index = sender.CurrentItemIndex;
        if (index > _allMediaPlayItems.Count - 1) return;

        _playIndex = index;

        var playItem = _allMediaPlayItems[(int)index];
        
        //修改
        DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal,  ()=>
        {
            // 修改标题
            _window.ChangedWindowTitle($"播放- {playItem.Title}");
            mediaTransportControls.SetTitle(playItem.Title);

            _window.ChangedVideoListViewIndex((int)index);

            // 设置喜欢/稍后观看按钮
            SetButton(playItem);

            Debug.WriteLine("设置画质列表");

            // 设置画质列表
            SetQualityList(playItem);
        });


        _isChangedCurrentItem = false;
    }

    private static async Task TryLoadSub(MediaPlayItem playItem)
    {
        var mediaPlaybackItem = playItem.MediaPlaybackItem;
        if(mediaPlaybackItem == null) return;

        // 之前已添加，退出
        if (mediaPlaybackItem.Source.ExternalTimedMetadataTracks.Count != 0) return;

        var subPath = await playItem.GetOneSubFilePath();

        // 不存在字幕文件，退出
        if (string.IsNullOrEmpty(subPath) || !File.Exists(subPath)) return;

        var timedTextSource = TimedTextSource.CreateFromUri(new Uri(subPath));
        timedTextSource.Resolved += (_, eventArgs) => { eventArgs.Tracks[0].Label = "Sub"; };

        Debug.WriteLine("添加字幕");
        mediaPlaybackItem.Source.ExternalTimedTextSources.Add(timedTextSource);

        mediaPlaybackItem.TimedMetadataTracksChanged += (sender, args) =>
        {
            Debug.WriteLine($"当前字幕数量为: {mediaPlaybackItem.TimedMetadataTracks.Count}");

            if (sender.TimedMetadataTracks.Count == 0) return;

            sender.TimedMetadataTracks.SetPresentationMode(0,
                TimedMetadataTrackPresentationMode.PlatformPresented);
            Debug.WriteLine("默认加载第一个字幕");
        };
    }


    private async void SetQualityList(MediaPlayItem playItem)
    {
        var list = await playItem.GetQualities();
        mediaTransportControls.SetQualityListSource(list,_qualityIndex);
    }

    private void SetButton(MediaPlayItem playItem)
    {
        //设置喜欢、稍后观看
        bool isLike;
        bool isLookLater;

        var videoInfo = playItem.GetVideoInfo();

        // 先判断是否为成功，后判断是否为失败
        if(videoInfo == null)
        {
            var failInfo = playItem.GetFailInfo();

            if (failInfo == null)
            {
                isLike = false;
                isLookLater = false;
            }
            else
            {
                IsLike = failInfo.IsLike;
                LookLater = Convert.ToInt32(failInfo.LookLater);

                isLike = IsLike == 1;
                isLookLater = LookLater != 0;
            }

            mediaTransportControls.SetLike_LookLater(isLike, isLookLater);

            mediaTransportControls.TrySetScreenButton();

        }
        else
        {
            IsLike = videoInfo.IsLike;
            LookLater = Convert.ToInt32(videoInfo.LookLater);

            isLike = IsLike == 1;
            isLookLater = this.LookLater != 0;

            mediaTransportControls.SetLike_LookLater(isLike, isLookLater);

            mediaTransportControls.DisableScreenButton();
        }
    }
    
    private async void Binder_Binding(MediaBinder sender, MediaBindingEventArgs args)
    {
        _isBindingCurrentItem = true;
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

            _adaptiveMediaSourceList.Add(result);

            if (result.Status == AdaptiveMediaSourceCreationStatus.Success && result.MediaSource != null)
            {
                args.SetAdaptiveMediaSource(result.MediaSource);
            }
        }
        //SetStream
        else
        {
            var stream = await HttpRandomAccessStream.CreateAsync(_httpClient, new Uri(videoUrl));

            _httpRandomAccessStreamList.Add(stream);

            if (stream.CanRead)
            {
                args.SetStream(stream,"video/mp4");
            }
        }

        await TryLoadSub(firstOrDefault);

        deferral.Complete();

        _isBindingCurrentItem = false;
    }

    private void MediaPlayerElement_CurrentStateChanged(MediaPlaybackSession sender, object args)
    {
        /*
         * 会出现以下异常
         * Exception thrown: 'System.Runtime.InteropServices.COMException' in System.Private.CoreLib.dll
         * Exception thrown: 'System.Runtime.InteropServices.COMException' in WinRT.Runtime.dll
         */

        if (sender == null || sender.NaturalVideoHeight == 0) return;

        if (sender.PlaybackState == MediaPlaybackState.Playing)
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
            if (_appDisplayRequest == null) return;

            // Deactivate the display request and set the var to null.
            _appDisplayRequest.RequestRelease();
            _appDisplayRequest = null;
        }

    }

    private void MediaPlayer_MediaOpened(MediaPlayer sender, object args)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            Debug.WriteLine("正在设置进度条");
            var transportControlsTemplateRoot = (FrameworkElement)VisualTreeHelper.GetChild(MediaControl.TransportControls, 0);
            var sliderControl = (Slider)transportControlsTemplateRoot?.FindName("ProgressSlider");
            if (sliderControl == null || !(sender.PlaybackSession.NaturalDuration.TotalSeconds > 1000)) return;

            // 十秒一步
            sliderControl.StepFrequency = 1000 / sender.PlaybackSession.NaturalDuration.TotalSeconds;
            sliderControl.SmallChange = 1000 / sender.PlaybackSession.NaturalDuration.TotalSeconds;


            Debug.WriteLine("成功设置进度条");
        });
    }
    

    private void QualityChanged(object sender, SelectionChangedEventArgs e)
    {
        //设置currentItem以及Binder_Binding时不计入改变
        if (_isChangedCurrentItem || _isBindingCurrentItem) return;
        
        if (sender is not ListView { ItemsSource: List<Quality> list }) return;
        if (e.AddedItems.FirstOrDefault() is not Quality quality) return;

        _qualityIndex = list.IndexOf(quality);

        //记录当前的时间
        var time = MediaControl.MediaPlayer.Position;

        Debug.WriteLine("销毁先前的设置");
        // 先销毁
        if (MediaControl.MediaPlayer.Source is MediaPlaybackList mediaPlaybackList)
        {
            DisposeMediaPlayer(mediaPlaybackList);
        }

        Debug.WriteLine("重新设置播放源");
        // 后重新设置
        SetMediaPlayer();

        Debug.WriteLine("恢复之前的时间");
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
            DataAccess.Update.UpdateSingleDataFromVideoInfo(trueName, "is_like", IsLike.ToString());

            if (IsLike == 1) ShowTeachingTip("已添加进喜欢");
        }
        else
        {
            Debug.WriteLine($"数据库不存在该数据:{trueName}");

            var failInfo = playItem.GetFailInfo();

            if (failInfo == null)
            {
                var capPath = await ScreenShotAsync(pickCode);
                DataAccess.Add.AddOrReplaceFailList_IsLike_LookLater(new()
                {
                    PickCode = pickCode,
                    IsLike = IsLike,
                    ImagePath = capPath
                });

                if (IsLike == 1) ShowTeachingTip("已添加进喜欢");
            }
            else
            {
                DataAccess.Update.UpdateSingleFailInfo(pickCode, "is_like", IsLike.ToString());

                //需要截图
                if (failInfo.ImagePath == Constants.FileType.NoPicturePath || !File.Exists(failInfo.ImagePath))
                {
                    var capPath = await ScreenShotAsync(pickCode);
                    DataAccess.Update.UpdateSingleFailInfo(pickCode, "image_path", capPath);

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
            DataAccess.Update.UpdateSingleDataFromVideoInfo(trueName, "look_later", LookLater.ToString());

            if (LookLater != 0) ShowTeachingTip("已添加进稍后观看");
        }else
        {
            var failInfo = playItem.GetFailInfo();

            if (failInfo == null)
            {
                var capPath = await ScreenShotAsync(pickCode);
                DataAccess.Add.AddOrReplaceFailList_IsLike_LookLater(new()
                {
                    PickCode = pickCode,
                    LookLater = LookLater,
                    ImagePath = capPath
                });

                if (LookLater != 0) ShowTeachingTip("已添加进稍后观看");
            }
            else
            {
                DataAccess.Update.UpdateSingleFailInfo(pickCode, "look_later", LookLater.ToString());

                //需要添加截图
                if (failInfo.ImagePath == FileType.NoPicturePath || !File.Exists(failInfo.ImagePath))
                {
                    var capPath = await ScreenShotAsync(pickCode);
                    DataAccess.Update.UpdateSingleFailInfo(pickCode, "image_path", capPath);

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

        var failInfo = DataAccess.Get.GetSingleFailInfoByPickCode(pickCode);

        var capPath = await ScreenShotAsync(pickCode);

        if (failInfo == null)
        {
            DataAccess.Add.AddOrReplaceFailList_IsLike_LookLater(new FailInfo
            {
                PickCode = pickCode,
                ImagePath = capPath
            });
        }
        else
        {
            DataAccess.Update.UpdateSingleFailInfo(pickCode, "image_path", capPath);
        }

        ShowTeachingTip("已截取当前画面作为封面");
    }

    private async Task<string> ScreenShotAsync(string pickCode)
    {
        var savePath = Path.Combine(AppSettings.ImageSavePath, "Screen");
        if (!Directory.Exists(savePath)) Directory.CreateDirectory(savePath);
        var storageFolder = await StorageFolder.GetFolderFromPathAsync(savePath);
        var file = await storageFolder.CreateFileAsync($"{pickCode}.png", CreationCollisionOption.ReplaceExisting);

        var canvasRenderTarget = new CanvasRenderTarget(
                CanvasDevice.GetSharedDevice(),
                MediaControl.MediaPlayer.PlaybackSession.NaturalVideoWidth,
                MediaControl.MediaPlayer.PlaybackSession.NaturalVideoHeight,
                96);
        MediaControl.MediaPlayer.CopyFrameToVideoSurface(canvasRenderTarget);

        await using var targetFileStream = await file.OpenStreamForWriteAsync();
        var stream = targetFileStream.AsRandomAccessStream();
        await canvasRenderTarget.SaveAsync(stream, CanvasBitmapFileFormat.Png);

        return file.Path;
    }

    private async void PlayerChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.FirstOrDefault() is not Player player) return;

        await _webApi.PlayVideoWithPlayer(_allMediaPlayItems, player.PlayerType, XamlRoot);
    }

    private void ShowTeachingTip(string subTitle)
    {
        BasePage.ShowTeachingTip(PlayerTeachingTip,subTitle);
    }

    private void OnOnApplyTemplateCompleted(object sender, EventArgs e)
    {
        if(!Resources.TryGetValue("QualityDataTemplate", out var qualityDataTemplateValue) || qualityDataTemplateValue is not DataTemplate qualityDataTemplate) return;
        if(!Resources.TryGetValue("PlayerDataTemplate", out var playerDataTemplateValue) || playerDataTemplateValue is not DataTemplate playerDataTemplate) return;

        mediaTransportControls.InitQuality(qualityDataTemplate);
        mediaTransportControls.InitPlayer(playerDataTemplate);

        if (_allMediaPlayItems.Count > 1) mediaTransportControls.SetRightButton();
    }

    public event EventHandler<RoutedEventArgs> RightButtonClick;
    private void MediaTransportControls_OnRightButtonClick(object sender, RoutedEventArgs e)
    {
        RightButtonClick?.Invoke(sender,e);
    }

    private TimeSpan _changedPositionTimeSpan = TimeSpan.MinValue;
    private double _changedVolume = double.NaN;

    private System.Timers.Timer _pTimer;
    private System.Timers.Timer _vTimer;
    private void InitVolumeContent()
    {
        if (_vTimer == null)
        {
            _vTimer = new System.Timers.Timer(600);
            _vTimer.Elapsed += VTimerElapsed;
        }
        else
        {
            _vTimer.Stop();
        }

        _vTimer.Start();

        VisualStateManager.GoToState(this, "AdditionalContentShowState", true);
    }

    private void InitPositionContent()
    {
        if (_pTimer == null)
        {
            _pTimer = new System.Timers.Timer(800);
            _pTimer.Elapsed += PTimerElapsed;
        }
        else
        {
            _pTimer.Stop();
        }

        _pTimer.Start();

        VisualStateManager.GoToState(this, "AdditionalContentShowState", true);
    }

    private void VTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
        if (_changedVolume is double.NaN) return;

        _vTimer.Stop();

        DispatcherQueue.TryEnqueue(() =>
        {
            MediaControl.MediaPlayer.Volume = _changedVolume;

            _changedVolume = double.NaN;

            VisualStateManager.GoToState(this, "AdditionalContentHiddenState", true);
        });
    }
    private void PTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
        if (_changedPositionTimeSpan == TimeSpan.MinValue) return;

        _pTimer.Stop();

        DispatcherQueue.TryEnqueue(() =>
        {
            MediaControl.MediaPlayer.Position = _changedPositionTimeSpan;

            _changedPositionTimeSpan = TimeSpan.MinValue;

            VisualStateManager.GoToState(this, "AdditionalContentHiddenState", true);
        });
    }


    private void ChangedVolumeText(double step)
    {
        if (_changedVolume is double.NaN)
        {
            _changedVolume = MediaControl.MediaPlayer.Volume;
        }

        var tmpVolume = _changedVolume + step;
        if (tmpVolume > 1)
        {
            _changedVolume = 1;
        }
        else if (tmpVolume < 0)
        {
            _changedVolume = 0;
        }
        else
        {
            _changedVolume = tmpVolume;
        }

        AdditionalTextBlock.Text = $"音量：{_changedVolume:P0}";
    }


    private const string TimeFormat = @"hh\:mm\:ss";
    private void ChangedPositionText(int second)
    {
        var naturalDurationTimeSpan = MediaControl.MediaPlayer.NaturalDuration;
        var naturalDurationString = naturalDurationTimeSpan.ToString(TimeFormat);

        var currentPositionTimeSpan = MediaControl.MediaPlayer.Position;
        var currentPositionString = MediaControl.MediaPlayer.Position.ToString(TimeFormat);

        Debug.WriteLine($"总时长：{naturalDurationString}");

        Debug.WriteLine($"当前位置：{currentPositionString}");

        if (_changedPositionTimeSpan == TimeSpan.MinValue)
        {
            _changedPositionTimeSpan = currentPositionTimeSpan;
        }

        var tmpTimeSpan = _changedPositionTimeSpan.Add(TimeSpan.FromSeconds(second));
        if (tmpTimeSpan > naturalDurationTimeSpan)
        {
            _changedPositionTimeSpan = naturalDurationTimeSpan;
        }
        else if (tmpTimeSpan < TimeSpan.Zero)
        {
            _changedPositionTimeSpan = TimeSpan.Zero;
        }
        else
        {
            _changedPositionTimeSpan = tmpTimeSpan;
        }

        var changedPositionTimeSpanString = _changedPositionTimeSpan.ToString(TimeFormat);

        Debug.WriteLine($"修改后的位置：{changedPositionTimeSpanString}");

        AdditionalTextBlock.Text = $"{changedPositionTimeSpanString} / {naturalDurationString}";
    }

    private void KeyboardAcceleratorUp_OnInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        InitVolumeContent();

        ChangedVolumeText(0.05);
    }

    private void KeyboardAcceleratorDown_OnInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        InitVolumeContent();

        ChangedVolumeText(-0.05);
    }

    private void KeyboardAcceleratorLeft_OnInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        InitPositionContent();

        ChangedPositionText(-10);
    }

    private void KeyboardAcceleratorShiftLeft_OnInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        InitPositionContent();

        ChangedPositionText(-20);
    }

    private void KeyboardAcceleratorRight_OnInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        InitPositionContent();

        ChangedPositionText(10);
    }

    private void KeyboardAcceleratorShiftRight_OnInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        InitPositionContent();

        ChangedPositionText(20);
    }

    private void KeyboardAcceleratorMute_OnInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        if (MediaControl.MediaPlayer == null) return;

        MediaControl.MediaPlayer.IsMuted = !MediaControl.MediaPlayer.IsMuted;
    }

    private async void ShowInfoItemClick(object sender, RoutedEventArgs e)
    {
        var playItem = _allMediaPlayItems[(int)_playIndex];

        var infos = new Dictionary<string, string>
        {
            {"文件名",playItem.FileName},
            {"pid", playItem.PickCode},
            {"分辨率",$"{MediaControl.MediaPlayer.PlaybackSession.NaturalVideoWidth} x {MediaControl.MediaPlayer.PlaybackSession.NaturalVideoHeight}"},
            {"时长",
                MediaControl.MediaPlayer.NaturalDuration.ToString(TimeFormat)},
        };

        if (playItem.Size != null)
        {
            infos.Add("大小", ByteSize.FromBytes((long)playItem.Size).ToString("#.#"));
        }

        if (playItem.IsRequestM3U8)
        {
            var m3U8Infos = await playItem.GetM3U8Infos();
            if (m3U8Infos is { Count: > 0 })
            {
                foreach (var info in m3U8Infos)
                {
                    infos.Add($"m3u8链接({info.Name})", info.Url);
                }
            }
        }

        if (playItem.IsRequestOriginal)
        {
            infos.Add("下载链接",await playItem.GetOriginalUrl());
        }

        infos.Add("userAgent", GetInfoFromNetwork.DownUserAgent);

        await InfoPage.ShowInContentDialog(XamlRoot, infos, "媒体信息");
    }
    
    private async void DeleteFileFrom115Button_Click(object sender, RoutedEventArgs e)
    {
        var playItem = _allMediaPlayItems[(int)_playIndex];

        if (playItem.Fid == null) return;

        var result = await new ContentDialog
        {
            XamlRoot = XamlRoot,
            Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
            Title = "确认",
            PrimaryButtonText = "删除",
            CloseButtonText = "返回",
            DefaultButton = ContentDialogButton.Close,
            Content = "该操作将删除115网盘中的文件，确认删除？"
        }.ShowAsync();

        if (result != ContentDialogResult.Primary) return;

        var isSuccess = await _webApi.DeleteFiles(playItem.Cid, new[] { (long)playItem.Fid });
        if (!isSuccess)
        {
            ShowTeachingTip("删除115文件失败");
            return;
        }

        DeleteFileClick?.Invoke(playItem);
    }

    public event Action<MediaPlayItem> DeleteFileClick;
}
