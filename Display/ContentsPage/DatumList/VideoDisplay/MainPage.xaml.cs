// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI.UI.Controls;
using CommunityToolkit.WinUI.UI.Controls.TextToolbarSymbols;
using Data;
using Data.Spider;
using Display.Models;
using Display.Views;
using Display.WindowView;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Org.BouncyCastle.Asn1.X509;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.ApplicationModel.DataTransfer;
using Windows.Media.Core;
using static QRCoder.PayloadGenerator;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.ContentsPage.DatumList.VideoDisplay;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainPage : Page
{

    ObservableCollection<FilesInfo> PlayingVideoInfos = new();

    ObservableCollection<VideoInfo> CidInfos = new();

    ObservableCollection<FilesInfo> FilesInfos;

    IncrementallLoadDatumCollection FilesInfosCollection;

    ObservableCollection<MetadataItem> _units;

    GetInfoFromNetwork network;

    ListView LastFilesListView;

    WebApi webApi;


    /// <summary>
    /// 可播放的最大数量
    /// </summary>
    private int MaxCanPlayCount = 4;

    public MainPage(List<FilesInfo> filesInfos, ListView lastFilesListView)
    {
        this.InitializeComponent();

        this.LastFilesListView = lastFilesListView;

        FilesInfos = new();
        filesInfos.ForEach(item => FilesInfos.Add(item));

        _units = new ObservableCollection<MetadataItem>() { new MetadataItem { Label = "播放列表", Command = OpenFolderCommand, CommandParameter = "0" } };
        metadataControl.Items = _units;
        network = new();
        webApi = new();
        this.Loaded += PageLoaded;
    }


    private void PageLoaded(object sender, RoutedEventArgs e)
    {
        TryPlayVideoFromSelectedFiles(FilesInfos.ToList());
    }

    private async void TryPlayVideoFromSelectedFiles(List<FilesInfo> filesInfos)
    {
        int MaxPlayCount;
        if (AppSettings.IsDefaultPlaySingleVideo)
        {
            MaxPlayCount = 1;
        }
        else
        {
            MaxPlayCount = MaxCanPlayCount;
        }

        var videoList = filesInfos.Where(item => item.Type == FilesInfo.FileType.File && item.datum.iv == 1).Take(MaxPlayCount).ToList();
        int videoCount = videoList.Count;

        if (MaxPlayCount == 1)
        {
            tryPlayVideos(videoList);
        }
        else
        {
            if (videoCount >= MaxPlayCount)
            {
                tryPlayVideos(videoList);
            }
            // count > 4
            else
            {
                int leftCount = MaxPlayCount - videoCount;

                //是否有文件夹
                var folderList = filesInfos.Where(item => item.Type == FilesInfo.FileType.Folder).Take(leftCount).ToList();
                if (folderList.Count > 0)
                {

                    foreach (var folder in folderList)
                    {
                        var FilesInfo = await webApi.GetFileAsync(folder.Cid, 40);

                        // 挑选视频数量
                        var videoInFolder = FilesInfo.data.Where(item => !string.IsNullOrEmpty(item.fid) && item.iv == 1).Take(leftCount).ToList();
                        var videoInFolderCount = videoInFolder.Count;

                        if (videoInFolderCount > 0)
                        {
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

                tryPlayVideos(videoList);

            }
        }
    }

    private RelayCommand<string> _openFolderCommand;
    private RelayCommand<string> OpenFolderCommand =>
        _openFolderCommand ??= new RelayCommand<string>(OpenFolder);
    private async void OpenFolder(string cid)
    {
        var currentItem = _units.FirstOrDefault(item => item.CommandParameter.ToString() == cid);

        //不存在，返回
        if (currentItem.CommandParameter == null) return;

        //删除选中路径后面的路径
        var index = _units.IndexOf(currentItem);

        //不存在，返回
        if (index < 0) return;

        for (int i = _units.Count - 1; i > index; i--)
        {
            _units.RemoveAt(i);
        }

        //选中的是第一项
        if (index == 0)
        {
            VideoShow_ListView.ItemsSource = FilesInfos;
            return;
        }
        else
        {
            await FilesInfosCollection.SetCid(cid);
            VideoShow_ListView.ItemsSource = FilesInfosCollection;
        }

    }

    public void CreateWindow()
    {
        CommonWindow window = new CommonWindow("播放");

        window.Closed += Window_Closed;
        window.Content = this;
        window.Activate();
    }

    private void Window_Closed(object sender, WindowEventArgs args)
    {
        RemoveMediaControl();
    }

    private void RemoveMediaControl()
    {
        foreach (var child in Video_UniformGrid.Children)
        {
            var videoControl = child as MediaPlayerElement;
            videoControl.SetMediaPlayer(null);
        }
        Video_UniformGrid.Children.Clear();
    }


    private void OpenFolder_Tapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        if (!(sender is Grid grid)) return;
        if (!(grid.DataContext is FilesInfo filesInfo)) return;


        ChangedFolder(filesInfo);
    }

    private void TextBlock_Tapped(object sender, TappedRoutedEventArgs e)
    {
        if (!(sender is TextBlock textBlock)) return;
        if (!(textBlock.DataContext is FilesInfo filesInfo)) return;

        ChangedFolder(filesInfo);
    }

    private async void ChangedFolder(FilesInfo filesInfo)
    {
        //跳过文件
        if (filesInfo.Type == FilesInfo.FileType.File) return;

        if (FilesInfosCollection == null)
        {
            FilesInfosCollection = new(filesInfo.Cid);
        }
        else
        {
            await FilesInfosCollection.SetCid(filesInfo.Cid);
        }

        if (VideoShow_ListView.ItemsSource != FilesInfosCollection)
        {
            VideoShow_ListView.ItemsSource = FilesInfosCollection;
        }


        _units.Add(new MetadataItem
        {
            Label = filesInfo.Name,
            Command = OpenFolderCommand,
            CommandParameter = filesInfo.Cid,
        });
    }

    private void PlayVideoButton_Click(object sender, RoutedEventArgs e)
    {
        //检查选中的文件或文件夹
        if (!(VideoShow_ListView.SelectedItems.FirstOrDefault() is FilesInfo)) return;

        List<FilesInfo> filesInfo = new();
        foreach (var item in VideoShow_ListView.SelectedItems)
        {
            var info = (FilesInfo)item;

            //if (info.Type == FilesInfo.FileType.File && info.datum.iv == 1)

            filesInfo.Add(info);
        }

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

    private async Task AddMediaElement(FilesInfo file)
    {
        string videoUrl = null;
        string pickCode = file.datum.pc;

        //转码成功，可以用m3u8
        if (file.datum.vdi != 0)
        {
            var m3u8Infos = await webApi.Getm3u8InfoByPickCode(pickCode);

            if (m3u8Infos.Count > 0)
            {
                //选择最高分辨率的播放
                videoUrl = m3u8Infos[0].Url;
            }
        }
        // 视频未转码，尝试获取直链
        else
        {
            var downUrlList = webApi.GetDownUrl(pickCode, GetInfoFromNetwork.MediaElementUserAgent);

            if (downUrlList.Count > 0)
            {
                videoUrl = downUrlList.FirstOrDefault().Value;
            }
        }


        if (videoUrl == null) return;

        //MenuFlyout menuFlyout = this.Resources["MediaContentFlyout"] as MenuFlyout;

        MenuFlyout menuFlyout = new();

        var MenuFlyoutItemDeletedFromList = new MenuFlyoutItem()
        {
            Text = "从播放列表中移除",
            Icon = new FontIcon() { FontFamily = new FontFamily("Segoe Fluent Icons"), Glyph = "\uE108" }
        };
        MenuFlyoutItemDeletedFromList.Click += RemoveFileFromListButton_Click;
        MenuFlyoutItemDeletedFromList.DataContext = file;

        var MenuFlyoutItemDeletedFrom115 = new MenuFlyoutItem()
        {
            Text = "从115中删除",
            Icon = new FontIcon() { FontFamily = new FontFamily("Segoe Fluent Icons"), Glyph = "\uE107" }
        };
        MenuFlyoutItemDeletedFrom115.Click += RemoveFileFrom115Button_Click;
        MenuFlyoutItemDeletedFrom115.DataContext = file;


        menuFlyout.Items.Add(MenuFlyoutItemDeletedFromList);
        menuFlyout.Items.Add(MenuFlyoutItemDeletedFrom115);

        Video_UniformGrid.Children.Add(
            new MediaPlayerElement()
            {
                Source = MediaSource.CreateFromUri(new Uri(videoUrl)),
                ContextFlyout = menuFlyout,
                Tag = file.datum.pc
            });

    }

    private async void tryPlayVideos(List<FilesInfo> filesInfo)
    {
        ChangedVideo_UniformGrid(filesInfo.Count);

        PlayingVideoInfos.Clear();
        RemoveMediaControl();

        foreach (var file in filesInfo.Take(MaxCanPlayCount))
        {
            // 添加MediaElement
            await AddMediaElement(file);

            // 记录正在播放的视频
            PlayingVideoInfos.Add(file);
        }

        VideoPlay_Pivot.SelectedIndex = 1;

        // 搜索影片信息

        CidInfos.Clear();
        await FindAndShowInfosFromInternet(PlayingVideoInfos.ToList());

    }

    private async Task FindAndShowInfosFromInternet(List<FilesInfo> filesInfos)
    {
        FindCidInfo_ProgressRing.Visibility = Visibility.Visible;

        Dictionary<string, VideoInfo> cidInfoDicts = new();

        string noPicturePath = Data.Const.NoPictruePath;

        //搜刮
        foreach (var video in filesInfos)
        {
            var name = video.Name;
            var cid = FileMatch.MatchName(name);
            if (cid == null)
            {
                cidInfoDicts.Add(name, new() { truename = name, imagepath = noPicturePath });
                continue;
            }

            //已存在，跳过
            if (cidInfoDicts.ContainsKey(name) == true) continue;

            var result = DataAccess.SelectTrueName(name);

            VideoInfo cidInfo = null;
            //数据库中有
            if (result.Count != 0)
            {
                //使用第一个符合条件的Name
                cidInfo = DataAccess.LoadOneVideoInfoByCID(result[0]);
            }
            //网络中查询
            else
            {
                var spiderManager = Data.Spider.Manager.Current;

                cidInfo = await spiderManager.DispatchSpiderInfoByCIDInOrder(cid);
            }

            if (cidInfo == null) cidInfo = new() { truename = cid, imagepath = noPicturePath };

            CidInfos.Add(cidInfo);

        }
        FindCidInfo_ProgressRing.Visibility = Visibility.Collapsed;
    }

    private void DoubleVideoPlayButton_Click(object sender, RoutedEventArgs e)
    {
        foreach (var item in Video_UniformGrid.Children)
        {
            var videoControl = item as MediaPlayerElement;
            videoControl.MediaPlayer.Play();
        }
    }

    private void DoubleVideoPauseButton_Click(object sender, RoutedEventArgs e)
    {
        foreach (var item in Video_UniformGrid.Children)
        {
            var videoControl = item as MediaPlayerElement;
            videoControl.MediaPlayer.Pause();
        }
    }

    private void IsMuteButton_Checked(object sender, RoutedEventArgs e)
    {
        foreach (var item in Video_UniformGrid.Children)
        {
            var videoControl = item as MediaPlayerElement;
            videoControl.MediaPlayer.IsMuted = true;

        }
    }

    private void IsMuteButton_Unchecked(object sender, RoutedEventArgs e)
    {
        foreach (var item in Video_UniformGrid.Children)
        {
            var videoControl = item as MediaPlayerElement;
            videoControl.MediaPlayer.IsMuted = false;
        }
    }

    private void Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine(e.NewValue);

        foreach (var item in Video_UniformGrid.Children)
        {
            var videoControl = item as MediaPlayerElement;

            videoControl.MediaPlayer.Position = videoControl.MediaPlayer.NaturalDuration * e.NewValue / 100;
        }
    }

    /// <summary>
    /// 从115中删除文件
    /// </summary>
    /// <param name="fileInfo"></param>
    /// <returns></returns>
    private async Task DeledtedFileFrom115Async(FilesInfo fileInfo)
    {
        // 首先，从播放列表中删除
        string fid = DeletedFileFromListAsync(fileInfo);

        // 然后，删除115文件
        await webApi.DeleteFiles(fileInfo.Cid, new() { fid });

        // 接着，删除资源管理器的文件，如果存在（有可能已经关掉了）
        if (LastFilesListView.IsLoaded && LastFilesListView.ItemsSource is IncrementallLoadDatumCollection filesInfos && filesInfos.Contains(fileInfo))
        {
            filesInfos.Remove(fileInfo);
        }

        // 最后，播放下一集（如果存在）
        await TryAddNextVideo();

    }

    /// <summary>
    /// 从播放列表中删除
    /// </summary>
    /// <param name="fileInfo"></param>
    /// <returns></returns>
    private string DeletedFileFromListAsync(FilesInfo fileInfo)
    {
        bool isFile = fileInfo.Type == FilesInfo.FileType.File;

        // 文件 fid
        // 文件夹 cid
        string fid = isFile ? fileInfo.Fid : fileInfo.Cid;

        //移除播放列表
        if (FilesInfos.Contains(fileInfo)) FilesInfos.Remove(fileInfo);

        // 文件
        if (isFile)
        {
            //移除正在播放视频列表
            if (PlayingVideoInfos.Contains(fileInfo)) PlayingVideoInfos.Remove(fileInfo);

            //移除正在播放的视频
            RemovePlayingVideo(fileInfo.datum.pc);
        }
        // 文件夹
        else
        {
            var playList = PlayingVideoInfos.Where(info => info.Cid == fileInfo.Cid).ToList();

            playList?.ForEach(info =>
            {
                //移除正在播放的视频
                PlayingVideoInfos.Remove(info);

                //移除正在播放的视频
                RemovePlayingVideo(info.datum.pc);
            });
        }

        RemoveCidInfo(fileInfo);

        return fid;
    }


    private void RemoveCidInfo(FilesInfo fileInfo)
    {
        //移除cid信息（预览图/信息）
        var removeCids = CidInfos.Where(item => item.truename == FileMatch.MatchName(fileInfo.Name).ToUpper()).ToList();
        foreach (var cid in removeCids)
        {
            CidInfos.Remove(cid);
        }
    }

    private void RemovePlayingVideo(String pc)
    {
        var media = Video_UniformGrid.Children.Where(item => (item as MediaPlayerElement).Tag.ToString() == pc).FirstOrDefault();
        if (media != null)
        {
            Video_UniformGrid.Children.Remove(media);
        }

    }

    private async Task<ContentDialogResult> TipDeletedFiles()
    {
        ContentDialog dialog = new ContentDialog()
        {
            XamlRoot = this.XamlRoot,
            Title = "确认",
            PrimaryButtonText = "删除",
            CloseButtonText = "返回",
            DefaultButton = ContentDialogButton.Close,
            Content = "该操作将删除115网盘中的文件，确认删除？"
        };

        return await dialog.ShowAsync();
    }

    private async Task DeletedFilesFromListView(ListView listView, ObservableCollection<FilesInfo> filesInfos)
    {
        //检查选中的文件或文件夹
        if (!(listView.SelectedItems.FirstOrDefault() is FilesInfo)) return;

        List<FilesInfo> filesInfo = new();
        foreach (var item in listView.SelectedItems)
        {
            filesInfo.Add((FilesInfo)item);
        }

        if (filesInfo.Count == 0) return;

        var result = await TipDeletedFiles();

        if (result == ContentDialogResult.Primary)
        {
            System.Diagnostics.Debug.WriteLine("删除");

            filesInfo.ForEach(item => filesInfos.Remove(item));

            await webApi.DeleteFiles(filesInfo.FirstOrDefault().datum.pid, filesInfo.Select(item => item.Fid).ToList());
        }
    }


    private void InfoGridVisiableButton_Click(object sender, RoutedEventArgs e)
    {
        switch (InfoGrid.Visibility)
        {
            case Visibility.Visible:
                InfoGrid.Visibility = Visibility.Collapsed;
                InfoGridVisiableButton.Content = "\uF743";
                break;
            case Visibility.Collapsed:
                InfoGrid.Visibility = Visibility.Visible;
                InfoGridVisiableButton.Content = "\uF745";
                break;
        }
    }

    private void Target_DragOver(object sender, DragEventArgs e)
    {
        e.AcceptedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Copy;
    }

    private void Target_DragEnter(object sender, DragEventArgs e)
    {
        // We don't want to show the Move icon
        e.DragUIOverride.IsGlyphVisible = false;
    }

    private void Target_Drop(object sender, DragEventArgs e)
    {
        if (sender is not ListView target) return;

        if (target.ItemsSource != FilesInfos) return;

        if (e.DataView.Properties.Values.FirstOrDefault() is not List<Data.FilesInfo> filesInfos) return;

        foreach (var fileInfo in filesInfos)
        {
            FilesInfos.Add(fileInfo);
        }
    }

    private void EmptyList_Click(object sender, RoutedEventArgs e)
    {
        FilesInfos.Clear();
    }

    private void VideoUniformGrid_Drop(object sender, DragEventArgs e)
    {
        if (sender is not Grid) return;

        if (e.DataView.Properties.Values.FirstOrDefault() is not List<Data.FilesInfo> filesInfos) return;

        FilesInfos.Clear();
        foreach (var info in filesInfos)
        {
            //if (info.Type == FilesInfo.FileType.File && info.datum.iv == 1)

            FilesInfos.Add(info);
        }

        TryPlayVideoFromSelectedFiles(FilesInfos.ToList());

    }

    private void Link_DragOver(object sender, DragEventArgs e)
    {
        e.AcceptedOperation = DataPackageOperation.Link;
        e.DragUIOverride.Caption = "播放";
    }

    private void EnlargeButton_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        if (sender is not Button button) return;
        if (button.DataContext is not VideoInfo videoInfo) return;
        ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Hand);

        SmokeGrid.Visibility = Visibility.Visible;

        EnlargeImage.Source = new BitmapImage(new Uri(videoInfo.imagepath));

    }

    private void EnlargeButton_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Arrow);

        SmokeGrid.Visibility = Visibility.Collapsed;
    }


    /// <summary>
    /// 从播放列表中移除
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void RemoveFileFromListButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuFlyoutItem item) return;

        if (item.DataContext is not FilesInfo fileInfo) return;

        // 从播放列表中删除
        DeletedFileFromListAsync(fileInfo);

        // 移除视频后，如果播放列表中有其余的视频则插入到播放中
        await TryAddNextVideo();

    }

    /// <summary>
    /// 自动播放下一视频
    /// </summary>
    /// <returns></returns>
    private async Task TryAddNextVideo()
    {
        //正在播放的文件的pc
        var playingPcList = Video_UniformGrid.Children.Select(element => ((MediaPlayerElement)element).Tag.ToString()).ToList();

        // 挑选播放列表中不是正在播放的视频
        // 删除的一般只有一个，只取一个
        var videoInfo = FilesInfos.FirstOrDefault(item => item.datum.iv == 1 && !playingPcList.Contains(item.datum.pc));

        if (videoInfo != null)
        {
            await AddMediaElement(videoInfo);

            // 添加到正在播放的视频的信息中
            PlayingVideoInfos.Add(videoInfo);

            // 搜索信息
            await FindAndShowInfosFromInternet(new List<FilesInfo>() { videoInfo });
        }
        // 没有下一集，则修改布局
        else
        {
            // 修改布局
            ChangedVideo_UniformGrid(Video_UniformGrid.Children.Count);
        }
    }

    /// <summary>
    /// 从115中删除
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void RemoveFileFrom115Button_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuFlyoutItem item) return;

        if (item.DataContext is not FilesInfo fileInfo) return;

        var result = await TipDeletedFiles();

        if (result == ContentDialogResult.Primary)
        {
            await DeledtedFileFrom115Async(fileInfo);
        }

    }
}
