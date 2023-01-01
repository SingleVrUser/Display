// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI.UI.Controls;
using Data;
using Display.Model;
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
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.ApplicationModel.DataTransfer;
using Windows.Media.Core;

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

    WebApi webApi;

    public MainPage(List<FilesInfo> filesInfos)
    {
        this.InitializeComponent();

        FilesInfos = new();
        filesInfos.ForEach(item => FilesInfos.Add(item));

        _units = new ObservableCollection<MetadataItem>() { new MetadataItem { Label = "�����б�", Command = OpenFolderCommand, CommandParameter = "0" } };
        metadataControl.Items = _units;
        network = new();
        webApi = new();
        this.Loaded += PageLoaded;
    }

    private void PageLoaded(object sender, RoutedEventArgs e)
    {
        int MaxPlayCount;
        if (AppSettings.IsDefaultPlaySingleVideo)
        {
            MaxPlayCount = 1;
        }
        else
        {
            MaxPlayCount = 4;
        }

        var videoList = FilesInfos.Where(item => item.Type == FilesInfo.FileType.File && item.datum.iv == 1).Take(MaxPlayCount).ToList();

        if (videoList.Count > 0)
        {
            tryPlayVideos(videoList);
        }
    }

    private RelayCommand<string> _openFolderCommand;
    private RelayCommand<string> OpenFolderCommand =>
        _openFolderCommand ??= new RelayCommand<string>(OpenFolder);
    private async void OpenFolder(string cid)
    {
        var currentItem = _units.FirstOrDefault(item => item.CommandParameter.ToString() == cid);

        //�����ڣ�����
        if (currentItem.CommandParameter == null) return;

        //ɾ��ѡ��·�������·��
        var index = _units.IndexOf(currentItem);

        //�����ڣ�����
        if (index < 0) return;

        for (int i = _units.Count - 1; i > index; i--)
        {
            _units.RemoveAt(i);
        }

        //ѡ�е��ǵ�һ��
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
        CommonWindow window = new CommonWindow("����");

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

    private void ChangedFolder(FilesInfo filesInfo)
    {
        //�����ļ�
        if (filesInfo.Type == FilesInfo.FileType.File) return;

        if (FilesInfosCollection == null)
        {
            FilesInfosCollection = new(filesInfo.Cid);
        }
        else
        {
            FilesInfosCollection.SetCid(filesInfo.Cid);
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
        //���ѡ�е��ļ����ļ���
        if (!(VideoShow_ListView.SelectedItems.FirstOrDefault() is FilesInfo)) return;

        List<FilesInfo> filesInfo = new();
        foreach (var item in VideoShow_ListView.SelectedItems)
        {
            var info = (FilesInfo)item;

            if (info.Type == FilesInfo.FileType.File && info.datum.iv == 1)
                filesInfo.Add(info);
        }

        tryPlayVideos(filesInfo);
    }

    private async void tryPlayVideos(List<FilesInfo> filesInfo)
    {
        switch (filesInfo.Count)
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

        PlayingVideoInfos.Clear();
        RemoveMediaControl();
        foreach (var file in filesInfo.Take(4))
        {
            string videoUrl = null;
            //ת��ɹ���������m3u8
            if (file.datum.vdi != 0)
            {
                var m3u8Infos = await webApi.Getm3u8InfoByPickCode(file.datum.pc);

                if (m3u8Infos.Count > 0)
                {
                    //ѡ����߷ֱ��ʵĲ���
                    videoUrl = m3u8Infos[0].Url;
                }
            }

            if (videoUrl == null) continue;

            Video_UniformGrid.Children.Add(
                new MediaPlayerElement()
                {
                    Source = MediaSource.CreateFromUri(new Uri(videoUrl)),
                    Tag = file.datum.pc
                }); 

            PlayingVideoInfos.Add(file);
        }

        VideoPlay_Pivot.SelectedIndex = 1;

        var cidInfoDicts = await FindInfosFromInternet(PlayingVideoInfos);

        CidInfos.Clear();
        foreach (var info in cidInfoDicts)
        {
            CidInfos.Add(info.Value);
        }
    }

    private async Task<Dictionary<string, VideoInfo>> FindInfosFromInternet(ObservableCollection<FilesInfo> filesInfos)
    {
        Dictionary<string, VideoInfo> cidInfoDicts = new();

        //�ѹ�
        foreach (var video in filesInfos)
        {
            var name = video.Name;
            var cid = FileMatch.MatchName(name);
            if (cid == null) continue;

            //�Ѵ��ڣ�����
            if (cidInfoDicts.ContainsKey(cid) == true) continue;

            var result = DataAccess.SelectTrueName(name);

            VideoInfo cidInfo = null;
            //���ݿ�����
            if (result.Count != 0)
            {
                //ʹ�õ�һ������������Name
                cidInfo = DataAccess.LoadOneVideoInfoByCID(result[0]);
            }
            //�����в�ѯ
            else
            {
                bool isFc = FileMatch.IsFC2(cid);

                if (!isFc && AppSettings.isUseJavBus)
                    cidInfo = await network.SearchInfoFromJavBus(cid);
                if (cidInfo == null && !isFc && AppSettings.isUseJav321)
                    cidInfo = await network.SearchInfoFromJav321(cid);
                if (cidInfo == null && !isFc && AppSettings.isUseAvMoo)
                    cidInfo = await network.SearchInfoFromAvMoo(cid);
                if (cidInfo == null && AppSettings.isUseAvSox)
                    cidInfo = await network.SearchInfoFromAvSox(cid);
                if (cidInfo == null && !isFc && AppSettings.isUseLibreDmm)
                    cidInfo = await network.SearchInfoFromLibreDmm(cid);
                if (cidInfo == null && isFc && AppSettings.isUseFc2Hub)
                    cidInfo = await network.SearchInfoFromFc2Hub(cid);
                if (cidInfo == null && AppSettings.isUseJavDB)
                    cidInfo = await network.SearchInfoFromJavDB(cid);
            }

            if (cidInfo == null) continue;

            cidInfoDicts.Add(cid, cidInfo);
        }

        return cidInfoDicts;

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

    private async void DeletedFileButton_Click(object sender, RoutedEventArgs e)
    {
        await DeletedFilesFromListView(VideoPlay_ListView, PlayingVideoInfos);
    }

    private async void DeleteFiles_Click(object sender, RoutedEventArgs e)
    {
        await DeletedFilesFromListView(VideoShow_ListView, FilesInfos);
    }

    private async Task DeletedFilesFromListView(ListView listView, ObservableCollection<FilesInfo> filesInfos)
    {
        //���ѡ�е��ļ����ļ���
        if (!(listView.SelectedItems.FirstOrDefault() is FilesInfo)) return;

        List<FilesInfo> filesInfo = new();
        foreach (var item in listView.SelectedItems)
        {
            filesInfo.Add((FilesInfo)item);
        }

        if (filesInfo.Count == 0) return;

        ContentDialog dialog = new ContentDialog()
        {
            XamlRoot = this.XamlRoot,
            Title = "ȷ��",
            PrimaryButtonText = "ɾ��",
            CloseButtonText = "����",
            DefaultButton = ContentDialogButton.Close,
            Content = "�ò�����ɾ��115�����е��ļ���ȷ��ɾ����"
        };

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            System.Diagnostics.Debug.WriteLine("ɾ��");

            filesInfo.ForEach(item => filesInfos.Remove(item));

            await webApi.DeleteFiles(filesInfo.FirstOrDefault().datum.pid, filesInfo.Select(item=>item.Fid).ToList());
        }
    }

    private void RemoveFileFromListButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuFlyoutItem item) return;

        if (item.DataContext is not FilesInfo fileInfo) return;

        if (FilesInfos.Contains(fileInfo)) FilesInfos.Remove(fileInfo);
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
        if(sender is not ListView target) return;

        if(target.ItemsSource != FilesInfos) return;

        if (e.DataView.Properties.Values.FirstOrDefault() is not List<Data.FilesInfo> filesInfos) return;

        foreach(var fileInfo in filesInfos)
        {
            FilesInfos.Add(fileInfo);
        }
    }

    private void EmptyList_Click(object sender, RoutedEventArgs e)
    {
        FilesInfos.Clear();
    }

    private void VideoGrid_Drop(object sender, DragEventArgs e)
    {
        if (sender is not Grid) return;

        if (e.DataView.Properties.Values.FirstOrDefault() is not List<Data.FilesInfo> filesInfos) return;

        List<FilesInfo> filesInfo = new();
        foreach (var info in filesInfos)
        {
            if (info.Type == FilesInfo.FileType.File && info.datum.iv == 1)
                filesInfo.Add(info);
        }

        tryPlayVideos(filesInfo);

    }

    private void Link_DragOver(object sender, DragEventArgs e)
    {
        e.AcceptedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Link;
    }

    private void EnlargeButton_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        if(sender is not Button button) return;
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
}
