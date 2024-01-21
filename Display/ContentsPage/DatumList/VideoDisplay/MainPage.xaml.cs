// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI.UI.Controls;
using Display.Data;
using Display.Helper;
using Display.Models;
using Display.Models.IncrementalCollection;
using Display.Views;
using Display.WindowView;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Media.Playback;
using SharpCompress;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.ContentsPage.DatumList.VideoDisplay;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainPage : Page,IDisposable
{

    private readonly ObservableCollection<FilesInfo> _playingVideoInfos = new();

    private readonly ObservableCollection<CidInfo> _cidInfos = new();

    private readonly ObservableCollection<FilesInfo> _filesInfos;

    private IncrementalLoadDatumCollection _filesInfosCollection;

    private readonly ObservableCollection<MetadataItem> _units;

    private readonly ListView _lastFilesListView;

    private readonly WebApi _webApi;

    private bool _isDisposing;

    /// <summary>
    /// 可播放的最大数量
    /// </summary>
    private int MaxCanPlayCount => (int)AppSettings.MaxVideoPlayCount;

    public MainPage(List<FilesInfo> filesInfos, ListView lastFilesListView)
    {
        InitializeComponent();

        _lastFilesListView = lastFilesListView;

        _filesInfos = new ObservableCollection<FilesInfo>();
        filesInfos.ForEach(_filesInfos.Add);

        _units = new ObservableCollection<MetadataItem> { new() { Label = "播放列表", Command = OpenFolderCommand, CommandParameter = (long)0 } };

        _webApi = WebApi.GlobalWebApi;

        TryPlayVideoFromSelectedFiles(_filesInfos.ToList());
    }
    
    private async void TryPlayVideoFromSelectedFiles(List<FilesInfo> filesInfos)
    {
        var maxPlayCount = MaxCanPlayCount;

        var videoList = filesInfos.Where(item => item.Type == FilesInfo.FileType.File && item.IsVideo)
            .Take(maxPlayCount).ToList();
        var videoCount = videoList.Count;

        if (maxPlayCount == 1)
        {
            TryPlayVideos(videoList);
        }
        else
        {
            if (videoCount <= maxPlayCount)
            {
                var leftCount = maxPlayCount - videoCount;

                //是否有文件夹
                var folderList = filesInfos.Where(item => item.Type == FilesInfo.FileType.Folder).Take(leftCount)
                    .ToList();
                if (folderList.Count > 0)
                {
                    foreach (var folder in folderList)
                    {
                        var filesInfo = await _webApi.GetFileAsync(folder.Cid, 40);

                        // 挑选视频数量
                        var videoInFolder = filesInfo.data
                            .Where(item => item.Fid!=null && item.Iv == 1).Take(leftCount).ToList();
                        var videoInFolderCount = videoInFolder.Count;

                        if (videoInFolderCount <= 0) continue;

                        foreach (var video in videoInFolder)
                        {
                            videoList.Add(new FilesInfo(video));
                            leftCount--;

                            if (leftCount == 0) break;
                        }

                        if (leftCount == 0) break;
                    }
                }
            }

            TryPlayVideos(videoList);
        }
    }

    private RelayCommand<long> _openFolderCommand;

    private RelayCommand<long> OpenFolderCommand =>
        _openFolderCommand ??= new RelayCommand<long>(OpenFolder);

    private async void OpenFolder(long cid)
    {
        var currentItem = _units.FirstOrDefault(item => (long)item.CommandParameter == cid);

        //不存在，返回
        if (currentItem.CommandParameter == null) return;

        //删除选中路径后面的路径
        var index = _units.IndexOf(currentItem);

        //不存在，返回
        if (index < 0) return;

        for (var i = _units.Count - 1; i > index; i--)
        {
            _units.RemoveAt(i);
        }

        //选中的是第一项
        if (index == 0)
        {
            VideoShow_ListView.ItemsSource = _filesInfos;
            return;
        }

        await _filesInfosCollection.SetCid(cid);
        VideoShow_ListView.ItemsSource = _filesInfosCollection;

    }

    public void CreateWindow()
    {
        var window = new CommonWindow("播放");

        window.Closed += Window_Closed;
        window.Content = this;
        window.Activate();
    }

    private void Window_Closed(object sender, WindowEventArgs args)
    {
        _isDisposing = true;
        Dispose();
    }

    private void RemoveAllMediaControl()
    {
        foreach (var child in Video_UniformGrid.Children)
        {
            if (child is not MediaPlayerElement { Tag: MediaPlayerWithStreamSource oldMediaPlayerWithStreamSource }) continue;

            oldMediaPlayerWithStreamSource.Dispose();
        }

        Video_UniformGrid.Children.Clear();
    }
    
    private void OpenFolder_Tapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        if (sender is not Grid { DataContext: FilesInfo filesInfo }) return;

        ChangedFolder(filesInfo);
    }


    private async void ChangedFolder(FilesInfo filesInfo)
    {
        //跳过文件
        if (filesInfo.Type == FilesInfo.FileType.File || filesInfo.Id==null) return;

        var folderId = (long)filesInfo.Id;

        if (_filesInfosCollection == null)
        {
            _filesInfosCollection = new IncrementalLoadDatumCollection(folderId);
        }
        else
        {
            await _filesInfosCollection.SetCid(folderId);
        }

        if (VideoShow_ListView.ItemsSource != _filesInfosCollection)
        {
            VideoShow_ListView.ItemsSource = _filesInfosCollection;
        }

        _units.Add(new MetadataItem
        {
            Label = filesInfo.Name,
            Command = OpenFolderCommand,
            CommandParameter = folderId
        });
    }

    private void PlayVideoButton_Click(object sender, RoutedEventArgs e)
    {
        //检查选中的文件或文件夹
        if (VideoShow_ListView.SelectedItems.FirstOrDefault() is not FilesInfo) return;

        var filesInfo = VideoShow_ListView.SelectedItems.Cast<FilesInfo>().ToList();

        TryPlayVideoFromSelectedFiles(filesInfo);
    }

    private void ChangedVideo_UniformGrid(int videoCount)
    {
        switch (videoCount)
        {
            case 0:
                return;
            case 1:
                Video_UniformGrid.Rows = 1;
                Video_UniformGrid.Columns = 1;
                break;
            case 2:
                Video_UniformGrid.Rows = 2;
                Video_UniformGrid.Columns = 1;
                break;
            case > 2:
                Video_UniformGrid.Rows = 2;
                Video_UniformGrid.Columns = 2;
                break;
        }
    }

    private async Task<string> GetVideoUrl(FilesInfo file)
    {
        string videoUrl = null;
        var pickCode = file.PickCode;

        //转码成功，可以用m3u8
        if (file.Datum.Vdi != 0)
        {
            var m3U8Infos = await _webApi.GetM3U8InfoByPickCode(pickCode);

            if (m3U8Infos.Count > 0)
            {
                //选择对应分辨率的播放
                var selectedIndex = AppSettings.IsPlayBestQualityFirst ? 0 : m3U8Infos.Count - 1;

                videoUrl = m3U8Infos[selectedIndex].Url;

            }
        }

        if (!string.IsNullOrEmpty(videoUrl)) return videoUrl;
        
        // 视频未转码，m3u8链接为0，尝试获取直链
        var downUrlList = await _webApi.GetDownUrl(pickCode, GetInfoFromNetwork.DownUserAgent);

        if (downUrlList.Count > 0)
        {
            videoUrl = downUrlList.FirstOrDefault().Value;
        }
        
        return videoUrl;

    }

    private MenuFlyout BuildMenuFlyout(FilesInfo file)
    {
        MenuFlyout menuFlyout = new();

        var menuFlyoutItemDeletedFromList = new MenuFlyoutItem()
        {
            Text = "从播放列表中移除",
            Icon = new FontIcon { FontFamily = new FontFamily("Segoe Fluent Icons"), Glyph = "\uE108" }
        };
        menuFlyoutItemDeletedFromList.Click += RemoveFileFromListButton_Click;
        menuFlyoutItemDeletedFromList.DataContext = file;

        var menuFlyoutItemDeletedFrom115 = new MenuFlyoutItem()
        {
            Text = "从115中删除",
            Icon = new FontIcon { FontFamily = new FontFamily("Segoe Fluent Icons"), Glyph = "\uE107" }
        };
        menuFlyoutItemDeletedFrom115.Click += RemoveFileFrom115Button_Click;
        menuFlyoutItemDeletedFrom115.DataContext = file;

        var menuFlyoutItemLoadVideoAgain = new MenuFlyoutItem()
        {
            Text = "重新加载",
            Icon = new FontIcon { FontFamily = new FontFamily("Segoe Fluent Icons"), Glyph = "\uF83E" }
        };
        menuFlyoutItemLoadVideoAgain.Click += LoadVideoAgainButton_Click;
        menuFlyoutItemLoadVideoAgain.DataContext = file;

        menuFlyout.Items.Add(menuFlyoutItemDeletedFromList);
        menuFlyout.Items.Add(menuFlyoutItemDeletedFrom115);
        menuFlyout.Items.Add(menuFlyoutItemLoadVideoAgain);

        return menuFlyout;
    }
    
    private async Task AddMediaElement(FilesInfo file, string videoUrl = null, int addIndex = -1)
    {
        videoUrl ??= await GetVideoUrl(file);
        if (videoUrl == null) return;

        var menuFlyout = BuildMenuFlyout(file);

        var mediaPlayerWithStreamSource =
            await MediaPlayerWithStreamSource.CreateMediaPlayer(videoUrl, filesInfo: file);

        var mediaPlayer = mediaPlayerWithStreamSource.MediaPlayer;
        var mediaPlayerElement = new MediaPlayerElement
        {
            ContextFlyout = menuFlyout,
            Tag = mediaPlayerWithStreamSource
        };

        mediaPlayerElement.SetMediaPlayer(mediaPlayer);

        // 是否自动播放
        if (AppSettings.IsAutoPlayInVideoDisplay)
        {
            mediaPlayerElement.AutoPlay = true;
        }

        mediaPlayerElement.MediaPlayer.MediaOpened += (sender, _) =>
        {
            if(_isDisposing) return;

            ChangMediaPlayerPositionWhenMediaOpened(sender);

            DispatcherQueue.TryEnqueue(() =>
            {
                var transportControlsTemplateRoot = (FrameworkElement)VisualTreeHelper.GetChild(mediaPlayerElement.TransportControls, 0);
                var sliderControl = (Slider)transportControlsTemplateRoot?.FindName("ProgressSlider");
                if (sliderControl == null || !(sender.PlaybackSession.NaturalDuration.TotalSeconds > 1000)) return;

                // 十秒一步
                sliderControl.StepFrequency = 1000 / sender.PlaybackSession.NaturalDuration.TotalSeconds;
                sliderControl.SmallChange = 1000 / sender.PlaybackSession.NaturalDuration.TotalSeconds;
            });


        };

        if (addIndex == -1)
        {
            Video_UniformGrid.Children.Add(mediaPlayerElement);
        }
        else
        {
            Video_UniformGrid.Children.Insert(addIndex, mediaPlayerElement);
        }

    }

    private static void ChangMediaPlayerPositionWhenMediaOpened(MediaPlayer sender)
    {
        // 是否需要改变起始位置
        if (AppSettings.AutoPlayPositionPercentage == 0.0) return;

        sender.Position = sender.NaturalDuration * AppSettings.AutoPlayPositionPercentage / 100;
    }


    private async void TryPlayVideos(IReadOnlyCollection<FilesInfo> filesInfo)
    {
        ChangedVideo_UniformGrid(filesInfo.Count);

        _playingVideoInfos.Clear();

        RemoveAllMediaControl();

        foreach (var file in filesInfo.Take(MaxCanPlayCount))
        {
            // 记录正在播放的视频
            _playingVideoInfos.Add(file);

            // 添加MediaElement
            await AddMediaElement(file);

        }

        VideoPlay_Pivot.SelectedIndex = 1;

        // 搜索影片信息
        _cidInfos.Clear();
        
        await FindAndShowInfosFromInternet(_playingVideoInfos.ToArray());
    }

    private async Task FindAndShowInfosFromInternet(IEnumerable<FilesInfo> filesInfos)
    {
        VideoPlay_ListView.IsEnabled = false;

        const string noPicturePath = Const.FileType.NoPicturePath;

        //搜刮
        foreach (var video in filesInfos)
        {
            if (_isDisposing) return;

            var name = video.Name;
            var trueName = FileMatch.MatchName(name);
            if (trueName == null)
            {
                _cidInfos.Add(new CidInfo(name,noPicturePath));
                continue;
            }

            Debug.WriteLine("正在添加信息："+trueName);

            // cidInfo已经存在
            var cidInfo = _cidInfos.FirstOrDefault(item => item.VideoInfo.trueName.ToUpper().Equals(trueName.ToUpper()));
            if (cidInfo is not null)
            {
                _cidInfos.Add(cidInfo);
                continue;
            }

            var result = DataAccess.Get.GetOneTrueNameByName(trueName);
            //数据库中有
            if (!string.IsNullOrEmpty(result))
            {
                //使用第一个符合条件的Name
                var videoInfo = DataAccess.Get.GetSingleVideoInfoByTrueName(result);

                _cidInfos.Add(new CidInfo(videoInfo));
            }
            //网络中查询
            else if(AppSettings.IsAutoSpiderInVideoDisplay)
            {
                Debug.WriteLine("从网络中搜索" + trueName);
                var info = new CidInfo(trueName, noPicturePath);

                _cidInfos.Add(info);

                var spiderManager = Spider.Manager.Current;


                FindCidInfo_ProgressRing.Visibility = Visibility.Visible;

                // 直接使用await spiderManager.DispatchSpiderInfoByCidInOrder会阻塞UI线程
                var videoInfo = await Task.Run(async () =>
                    await spiderManager.DispatchSpiderInfoByCidInOrder(trueName, info.CancellationTokenSource.Token));

                FindCidInfo_ProgressRing.Visibility = Visibility.Collapsed;

                if (videoInfo == null || info.CancellationTokenSource.Token.IsCancellationRequested) continue;
                
                info.UpdateInfo(videoInfo);

            }
        }


        VideoPlay_ListView.IsEnabled = true;
    }

    private void DoubleVideoPlayButton_Click(object sender, RoutedEventArgs e)
    {
        foreach (var item in Video_UniformGrid.Children)
        {
            if (item is not MediaPlayerElement videoControl) continue;
            videoControl.MediaPlayer.Play();
        }
    }

    private void DoubleVideoPauseButton_Click(object sender, RoutedEventArgs e)
    {
        foreach (var item in Video_UniformGrid.Children)
        {
            if (item is not MediaPlayerElement videoControl) continue;
            videoControl.MediaPlayer.Pause();
        }
    }

    private void IsMuteButton_Checked(object sender, RoutedEventArgs e)
    {
        foreach (var item in Video_UniformGrid.Children)
        {
            if (item is not MediaPlayerElement videoControl) continue;
            videoControl.MediaPlayer.IsMuted = true;

        }
    }

    private void IsMuteButton_Unchecked(object sender, RoutedEventArgs e)
    {
        foreach (var item in Video_UniformGrid.Children)
        {
            if (item is not MediaPlayerElement videoControl) continue;

            videoControl.MediaPlayer.IsMuted = false;
        }
    }

    private void Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
        foreach (var item in Video_UniformGrid.Children)
        {
            if (item is not MediaPlayerElement videoControl) continue;

            videoControl.MediaPlayer.Position = videoControl.MediaPlayer.NaturalDuration * e.NewValue / 100;
        }
    }

    /// <summary>
    /// 从115中删除文件
    /// </summary>
    /// <param name="fileInfo"></param>
    /// <returns></returns>
    private async Task DeletedFileFrom115Async(FilesInfo fileInfo)
    {
        // 首先，从播放列表中删除
        var fid = DeletedFileFromListAsync(fileInfo);
        if (fid == null) return;

        // 播放下一集（如果存在）
        await TryRemoveCurrentVideoAndPlayNextVideo(fileInfo);

        // 然后，删除115文件
        var result = await _webApi.DeleteFiles(fileInfo.Cid, new[] { (long)fid });
        if (!result)
        {
            ShowTeachingTip("删除115文件失败");
            return;
        }

        // 删除资源管理器的文件，如果存在（有可能已经关掉了）
        if (_lastFilesListView.IsLoaded && _lastFilesListView.ItemsSource is IncrementalLoadDatumCollection filesInfos &&
            filesInfos.Contains(fileInfo))
        {
            filesInfos.Remove(fileInfo);
        }

    }

    /// <summary>
    /// 从播放列表中删除
    /// </summary>
    /// <param name="fileInfo"></param>
    /// <returns></returns>
    private long? DeletedFileFromListAsync(FilesInfo fileInfo)
    {
        var isFile = fileInfo.Type == FilesInfo.FileType.File;

        // 文件 fid
        // 文件夹 cid
        var fid = fileInfo.Id;

        //移除播放列表
        if (_filesInfos.Contains(fileInfo)) _filesInfos.Remove(fileInfo);

        // 文件
        if (isFile)
        {
            //移除正在播放的视频列表
            if (_playingVideoInfos.Contains(fileInfo)) _playingVideoInfos.Remove(fileInfo);

        }
        // 文件夹
        else
        {
            var playList = _playingVideoInfos.Where(info => info.Id == fid).ToList();

            playList.ForEach(info =>
            {
                //移除正在播放的视频列表
                _playingVideoInfos.Remove(info);
            });
        }

        RemoveCidInfo(fileInfo);

        return fid;
    }


    private void RemoveCidInfo(FilesInfo fileInfo)
    {
        //移除cid信息（预览图/信息）
        var removeCid = _cidInfos.FirstOrDefault(item =>
            item.VideoInfo.trueName == fileInfo.Name || item.VideoInfo.trueName.ToUpper() == FileMatch.MatchName(fileInfo.Name)?.ToUpper());

        if (removeCid == null) return;

        if (removeCid.CancellationTokenSource.Token.CanBeCanceled)
        {
            removeCid.CancellationTokenSource.Cancel();
        }

        _cidInfos.Remove(removeCid);
    }

    private void RemovePlayingVideo(MediaPlayerElement mediaPlayerElement)
    {
        if (mediaPlayerElement.Tag is not MediaPlayerWithStreamSource oldMediaPlayerWithStreamSource) return;

        try
        {
            if (Video_UniformGrid.Children.Contains(mediaPlayerElement))
            {
                Video_UniformGrid.Children.Remove(mediaPlayerElement);
            }

            oldMediaPlayerWithStreamSource.Dispose();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"删除正在播放的视频文件时出现问题{ex.Message}");
        }
    }

    private async Task<ContentDialogResult> TipDeletedFiles()
    {
        var dialog = new ContentDialog
        {
            XamlRoot = XamlRoot,
            Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
            Title = "确认",
            PrimaryButtonText = "删除",
            CloseButtonText = "返回",
            DefaultButton = ContentDialogButton.Close,
            Content = "该操作将删除115网盘中的文件，确认删除？"
        };

        return await dialog.ShowAsync();
    }

    private void InfoGridVisibilityButton_Click(object sender, RoutedEventArgs e)
    {
        if (RootSplitView.IsPaneOpen)
        {
            RootSplitView.IsPaneOpen = false;
            ((HyperlinkButton)sender).Content = "\uf743";
        }
        else
        {
            RootSplitView.IsPaneOpen = true;
            ((HyperlinkButton)sender).Content = "\uf745";
        }
    }

    private void Target_DragOver(object sender, DragEventArgs e)
    {
        e.AcceptedOperation = DataPackageOperation.Copy;
    }

    private void Target_DragEnter(object sender, DragEventArgs e)
    {
        // We don't want to show the Move icon
        e.DragUIOverride.IsGlyphVisible = false;
    }

    private void Target_Drop(object sender, DragEventArgs e)
    {
        if (sender is not ListView target) return;

        if (target.ItemsSource != _filesInfos) return;

        if (e.DataView.Properties.Values.FirstOrDefault() is not List<FilesInfo> filesInfos) return;

        foreach (var fileInfo in filesInfos)
        {
            _filesInfos.Add(fileInfo);
        }
    }

    private void EmptyList_Click(object sender, RoutedEventArgs e)
    {
        _filesInfos.Clear();
    }

    private void VideoUniformGrid_Drop(object sender, DragEventArgs e)
    {
        if (sender is not Grid) return;

        if (e.DataView.Properties.Values.FirstOrDefault() is not List<FilesInfo> filesInfos) return;

        _filesInfos.Clear();
        foreach (var info in filesInfos)
        {
            //if (info.Type == FilesInfo.FileType.File && info.IsVideo)

            _filesInfos.Add(info);
        }

        TryPlayVideoFromSelectedFiles(_filesInfos.ToList());

    }

    private void Link_DragOver(object sender, DragEventArgs e)
    {
        e.AcceptedOperation = DataPackageOperation.Link;
        e.DragUIOverride.Caption = "播放";
    }

    private void EnlargeButton_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        if (sender is not Button { DataContext: CidInfo cidInfo }) return;

        ProtectedCursor = CursorHelper.GetZoomCursor();

        SmokeGrid.Visibility = Visibility.Visible;

        EnlargeImage.Source = new BitmapImage(new Uri(cidInfo.VideoInfo.ImagePath));

    }

    private void EnlargeButton_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        ProtectedCursor = null;

        SmokeGrid.Visibility = Visibility.Collapsed;
    }


    private void CidInfoUserControlPointerEntered(object sender, PointerRoutedEventArgs e)
    {
        if (e.Pointer.PointerDeviceType is PointerDeviceType.Mouse or PointerDeviceType.Pen)
        {
            VisualStateManager.GoToState(sender as Control, "EnlargeButtonShown", true);
        }
    }

    private void CidInfoUserControlPointerExited(object sender, PointerRoutedEventArgs e)
    {
        if (e.Pointer.PointerDeviceType is PointerDeviceType.Mouse or PointerDeviceType.Pen)
        {
            VisualStateManager.GoToState(sender as Control, "EnlargeButtonHidden", true);
        }
    }


    /// <summary>
    /// 从播放列表中移除
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void RemoveFileFromListButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuFlyoutItem { DataContext: FilesInfo fileInfo }) return;

        // 从播放列表中删除
        DeletedFileFromListAsync(fileInfo);

        // 移除视频后，如果播放列表中有其余的视频则插入到播放中
        await TryRemoveCurrentVideoAndPlayNextVideo(fileInfo);

    }

    private MediaPlayerElement GetFirstElementPlayPickCode(string pc)
    {
        var media = Video_UniformGrid.Children.FirstOrDefault(item =>
            ((item as MediaPlayerElement)?.Tag as MediaPlayerWithStreamSource)?.FilesInfo.PickCode == pc);
        if (media == null || media is not MediaPlayerElement mediaPlayerElement) return null;

        return mediaPlayerElement;
    }

    /// <summary>
    /// 自动播放下一视频
    /// </summary>
    /// <returns></returns>
    private async Task TryRemoveCurrentVideoAndPlayNextVideo(FilesInfo removeFileInfo)
    {
        // 即将删除的视频所属控件
        var mediaPlayerElement = GetFirstElementPlayPickCode(removeFileInfo.PickCode);

        if (mediaPlayerElement == null) return;
        
        // 挑选播放列表中不是正在播放的视频
        // 删除的一般只有一个，只取一个
        var videoInfo = _filesInfos.FirstOrDefault(item => item.IsVideo && !_playingVideoInfos.Contains(item));

        if (videoInfo != null)
        {
            // 添加到正在播放的视频的信息中
            _playingVideoInfos.Add(videoInfo);

            Debug.WriteLine("开始移除并播放下一集");
            await RemoveVideoAndPlayNextVideo(mediaPlayerElement, videoInfo);

            Debug.WriteLine("开始搜刮信息");

            // 搜索信息
            await FindAndShowInfosFromInternet(new[] { videoInfo });
        }
        // 没有下一集，则修改布局
        else
        {
            //移除正在播放的视频
            RemovePlayingVideo(mediaPlayerElement);

            // 修改布局
            ChangedVideo_UniformGrid(Video_UniformGrid.Children.Count);
        }
        
    }

    /// <summary>
    /// 删除后重新添加
    /// </summary>
    /// <param name="mediaPlayerElement"></param>
    /// <param name="videoInfo"></param>
    /// <returns></returns>
    private async Task RemoveVideoAndPlayNextVideo(MediaPlayerElement mediaPlayerElement, FilesInfo videoInfo)
    {
        RemovePlayingVideo(mediaPlayerElement);

        await AddMediaElement(videoInfo);
    }


    /// <summary>
    /// 从115中删除
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void RemoveFileFrom115Button_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuFlyoutItem { DataContext: FilesInfo fileInfo }) return;

        var result = await TipDeletedFiles();

        if (result == ContentDialogResult.Primary)
        {
            await DeletedFileFrom115Async(fileInfo);
        }
    }

    /// <summary>
    /// 重新加载视频
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void LoadVideoAgainButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuFlyoutItem { DataContext: FilesInfo fileInfo }) return;

        //获取正在播放该视频的控件
        var oldMediaElement = GetFirstElementPlayPickCode(fileInfo.PickCode);
        if (oldMediaElement == null) return;

        if (oldMediaElement.Tag is not MediaPlayerWithStreamSource oldMediaPlayerWithStreamSource) return;

        var videoUrl = oldMediaPlayerWithStreamSource.Url;

        // 删除旧的
        var index = Video_UniformGrid.Children.IndexOf(oldMediaElement);
        Video_UniformGrid.Children.RemoveAt(index);
        oldMediaPlayerWithStreamSource.Dispose();

        // 添加新的
        await AddMediaElement(fileInfo, videoUrl, index);

    }

    private System.Timers.Timer aTimer;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Video_UniformGrid_OnPointerEntered(object sender, PointerRoutedEventArgs e)
    {
        aTimer?.Stop();

        // Create a timer with a one second interval.
        aTimer = new System.Timers.Timer(500);
        aTimer.Enabled = true;
        aTimer.Elapsed += timer_Tick; ;
    }

    //鼠标状态计数器
    private int _iCount = 0;

    private void timer_Tick(object sender, System.Timers.ElapsedEventArgs e)
    {
        //鼠标状态计数器>=0的情况下鼠标可见，<0不可见，并不是直接受api函数影响而改变
        var i = CursorHelper.GetIdleTick();

        if (i > 4000)
        {
            while (_iCount >= 0)
            {
                _iCount = CursorHelper.ShowCursor(false);
                TryUpdateUi(false);
            }

        }
        else
        {
            while (_iCount < 0)
            {
                _iCount = CursorHelper.ShowCursor(true);
                TryUpdateUi();
            }
        }
    }

    private void TryUpdateUi(bool isOpenSplitPane = true)
    {
        var cursor = isOpenSplitPane ? null : CursorHelper.GetHiddenCursor();
        var visibility = isOpenSplitPane ? Visibility.Visible: Visibility.Collapsed;

        if (DispatcherQueue.HasThreadAccess)
        {
            ProtectedCursor = cursor;
            SplitViewOpenButton.Visibility = visibility;
        }
        else
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                ProtectedCursor = cursor;
                SplitViewOpenButton.Visibility = visibility;
            });
        }
    }

    private void Video_UniformGrid_OnPointerExited(object sender, PointerRoutedEventArgs e)
    {
        aTimer?.Stop();

        ProtectedCursor = null;
    }

    private void LocationFileClick(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuFlyoutItem { DataContext: FilesInfo info }) return;

        // 接着，删除资源管理器的文件，如果存在（有可能已经关掉了）
        if (_lastFilesListView.IsLoaded && _lastFilesListView.ItemsSource is IncrementalLoadDatumCollection filesInfos &&
            filesInfos.Contains(info))
        {
            _lastFilesListView.ScrollIntoView(info, ScrollIntoViewAlignment.Leading);
        }
    }

    private void ShowTeachingTip(string subtitle, string content=null)
    {
        BasePage.ShowTeachingTip(LightDismissTeachingTip,subtitle,content);
    }

    public void Dispose()
    {
        Video_UniformGrid.PointerExited -= Video_UniformGrid_OnPointerExited;
        aTimer?.Stop();
        aTimer?.Dispose();
        RemoveAllMediaControl();

        _playingVideoInfos.Clear();
        _cidInfos.Clear();
        _filesInfos?.Clear();
        _filesInfosCollection?.Clear();
        _units?.Clear();

    }

    private void DeleteFiles_Click(object sender, RoutedEventArgs e)
    {
        // 移除选中的文件
        if (VideoShow_ListView.SelectedItems.FirstOrDefault() is not FilesInfo) return;
        
        VideoShow_ListView.SelectedItems.Cast<FilesInfo>().ForEach(info =>
        {
            _filesInfos.Remove(info);
        });

    }
}

public class CidInfo
{
    public VideoInfo VideoInfo;

    public CidInfo(string name, string imagePath)
    {
        VideoInfo = new VideoInfo { trueName = name , ImagePath = imagePath};
    }

    public CidInfo(VideoInfo info)
    {
        VideoInfo = info;
    }

    public void UpdateInfo(VideoInfo info)
    {
        VideoInfo.trueName = info.trueName;
        VideoInfo.ImagePath = info.ImagePath;
        VideoInfo.ReleaseTime = info.ReleaseTime;
        VideoInfo.Actor = info.Actor;
    }

    public CancellationTokenSource CancellationTokenSource { get; set; } = new();
}
