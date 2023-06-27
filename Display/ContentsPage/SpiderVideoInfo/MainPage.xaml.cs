
using Display.Helper;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Display.Data;
using HarfBuzzSharp;
using System.Threading.Tasks;
using Display.Models;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.ContentsPage.SpiderVideoInfo;


/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainPage : Page, INotifyPropertyChanged
{
    private IncrementalLoadFailSpiderInfoCollection _failList;
    private IncrementalLoadFailSpiderInfoCollection FailList
    {
        get => _failList;
        set
        {
            if (_failList == value)
                return;

            _failList = value;

            OnPropertyChanged();
        }
    }

    private Datum _selectedDatum;
    public Datum SelectedDatum
    {
        get => _selectedDatum;
        set
        {
            if (_selectedDatum == value)
                return;
            _selectedDatum = value;

            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    public void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public MainPage()
    {
        this.InitializeComponent();
    }

    /// <summary>
    /// 展开Expander初始化检查图片路径和网络
    /// </summary>
    /// <param Name="sender"></param>
    /// <param Name="args"></param>
    private void Expander_Expanding(Expander sender, ExpanderExpandingEventArgs args)
    {
        if (!(sender is Expander expander)) return;

        if ((expander.Content as ConditionalCheck) == null)
        {
            expander.Content = new ContentsPage.SpiderVideoInfo.ConditionalCheck(this);
        }

        expander.SetValue(Grid.ColumnProperty, 0);
        expander.SetValue(Grid.ColumnSpanProperty, 2);
    }


    /// <summary>
    /// 关闭Expander
    /// </summary>
    /// <param Name="sender"></param>
    /// <param Name="args"></param>
    private void Expander_Collapsed(Expander sender, ExpanderCollapsedEventArgs args)
    {
        if (!(sender is Expander expander)) return;

        expander.SetValue(Grid.ColumnProperty, 1);
        expander.SetValue(Grid.ColumnSpanProperty, 1);
    }

    /// <summary>
    /// 点击匹配按钮开始匹配
    /// </summary>
    /// <param Name="sender"></param>
    /// <param Name="e"></param>
    private void StartMatchName_ButtonClick(object sender, RoutedEventArgs e)
    {
        ////从本地数据库中搜刮
        //if (localData_RadioButton.IsChecked == true)
        //{
        //    SpiderFromLocalData();
        //}
        ////从失败列表中搜刮（带匹配名称）
        //else
        //{
        //    SpiderFromFailList();
        //}
    }

    /// <summary>
    /// 从失败列表中搜刮（带匹配名称）
    /// </summary>
    private void SpiderFromFailList()
    {
        if (FailList == null || FailList.Count == 0)
        {
            ShowTeachingTip("当前没有需要搜刮的内容");
            return;
        }

        List<FailDatum> failDatums = FailList.Where(item => !string.IsNullOrEmpty(item.MatchName)).ToList();

        if (failDatums.Count == 0)
        {
            ShowTeachingTip("未填写任何番号，请填写后继续");
            return;
        }

        //创建进度窗口
        var page = new Progress(failDatums);
        page.CreateWindow();
    }

    private void ShowTeachingTip(string content, FrameworkElement target = null)
    {
        //SelectNull_TeachingtTip.Target = target != null ? target : StartMatchNameButton;
        //SelectNull_TeachingtTip.Subtitle = content;
        //SelectNull_TeachingtTip.IsOpen = true;
    }

    /// <summary>
    /// 从本地数据库中搜刮
    /// </summary>
    private void SpiderFromLocalData()
    {
        ////检查是否有选中文件
        //if (Explorer.FolderTreeView.SelectedNodes.Count == 0)
        //{
        //    ShowTeachingTip("没有选择文件夹，请选择后继续");
        //    return;
        //}

        //获取需要搜刮的文件夹
        var tuple = GetCurrentSelectedFolder();
        var selectedFilesNameList = tuple.Item1;
        var folderList = tuple.Item2;

        //创建进度窗口
        var page = new Progress(selectedFilesNameList, folderList);
        page.CreateWindow();
    }

    private Tuple<List<string>, List<Datum>> GetCurrentSelectedFolder()
    {
        List<string> selectFilesNameList = new();
        List<Datum> datumList = new();
        //foreach (var node in Explorer.FolderTreeView.SelectedNodes)
        //{
        //    if (node.Content is not ExplorerItem explorer) continue;

        //    //文件夹
        //    selectFilesNameList.Add(explorer.Name);

        //    //文件夹下的文件和文件夹
        //    var items = Explorer.GetFilesFromItems(explorer.Id, FilesInfo.FileType.File);

        //    datumList.AddRange(items);
        //}

        return new Tuple<List<string>, List<Datum>>(selectFilesNameList, datumList);
    }

    /// <summary>
    /// 点击显示文件信息
    /// </summary>
    /// <param Name="sender"></param>
    /// <param Name="e"></param>
    private void ExplorerItemClick(object sender, ItemClickEventArgs e)
    {
        var itemInfo = e.ClickedItem as FilesInfo;

        //if (FileInfoShow_Grid.Visibility == Visibility.Collapsed) FileInfoShow_Grid.Visibility = Visibility.Visible;
        //SelectedDatum = itemInfo.Datum;
    }

    /// <summary>
    /// 点击TreeView的Item显示文件信息
    /// </summary>
    /// <param Name="sender"></param>
    /// <param Name="args"></param>
    private void TreeView_ItemInvoked(TreeView sender, TreeViewItemInvokedEventArgs args)
    {
        var content = ((args.InvokedItem as TreeViewNode).Content as ExplorerItem);

        //if (FileInfoShow_Grid.Visibility == Visibility.Collapsed) FileInfoShow_Grid.Visibility = Visibility.Visible;
        //SelectedDatum = content.datum;
    }

    /// <summary>
    /// 点击失败列表显示文件信息
    /// </summary>
    /// <param Name="sender"></param>
    /// <param Name="e"></param>
    private void FailListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count != 1)
            return;

        //if (FileInfoShow_Grid.Visibility == Visibility.Collapsed) FileInfoShow_Grid.Visibility = Visibility.Visible;

        //SelectedDatum = (e.AddedItems[0] as FailDatum).Datum;
    }

    /// <summary>
    /// 点击视频按钮播放
    /// </summary>
    /// <param Name="sender"></param>
    /// <param Name="e"></param>
    private async void VideoPlayButton_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedDatum == null) return;

        ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Wait);

        var mediaPlayItem = new MediaPlayItem(SelectedDatum.pc, SelectedDatum.n, FilesInfo.FileType.File);
        await PlayVideoHelper.PlayVideo(new List<MediaPlayItem>() { mediaPlayItem }, this.XamlRoot, lastPage: this);
        ProtectedCursor = null;
    }

    private void FailTypeComboBoxChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems[0] is ComboBoxItem comboBoxItem)
        {
            ChangedFailListType(comboBoxItem);
        }
    }

    private async void ChangedFailListType(ComboBoxItem comboBoxItem)
    {
        //if (FailListView.ItemsSource == null)
        //{
        //    FailList = new();
        //}
        //else if (FailList.Count != 0)
        //{
        //    FailList.Clear();
        //}

        //switch (comboBoxItem.Name)
        //{
        //    //正则匹配失败
        //    case nameof(ShowMatcFail_ComboBoxItem):
        //        FailList.SetShowType(FailType.MatchFail);
        //        await FailList.LoadData();
        //        break;
        //    //搜刮失败
        //    case nameof(ShowSpiderFail_ComboBoxItem):
        //        FailList.SetShowType(FailType.SpiderFail);
        //        await FailList.LoadData();
        //        break;
        //    //所有
        //    default:
        //        FailList.SetShowType(FailType.All);
        //        await FailList.LoadData();
        //        break;
        //}
    }

    private void ShowData_RadioButtons_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        //if (e.AddedItems[0] is RadioButton radioButton)
        //{
        //    switch (radioButton.Name)
        //    {
        //        //本地数据库
        //        case nameof(localData_RadioButton):
        //            FailShowTypeComboBox.SelectionChanged -= FailTypeComboBoxChanged;
        //            break;
        //        //搜刮失败
        //        case nameof(matchFail_RadioButton):
        //            if (FailShowTypeComboBox.SelectionBoxItem == null)
        //            {
        //                ChangedFailListType(ShowAllFail_ComboBoxItem);
        //            }

        //            FailShowTypeComboBox.SelectionChanged += FailTypeComboBoxChanged;
        //            break;
        //    }
        //}
    }

    private async void PlayWithPlayerButtonClick(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuFlyoutItem { Tag: string aTag }) return;
        if (!int.TryParse(aTag, out var playerSelection)) return;
        if (SelectedDatum == null) return;

        await Task.Delay(1);


        var mediaPlayItem = new MediaPlayItem(SelectedDatum.pc, SelectedDatum.n, FilesInfo.FileType.File);
        await PlayVideoHelper.PlayVideo(new List<MediaPlayItem>() { mediaPlayItem }, this.XamlRoot, lastPage: this, playerSelection: playerSelection);
    }

    private async void Explorer_OnPlayVideoClick(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuFlyoutItem { DataContext: FilesInfo info }) return;

        var mediaPlayItem = new MediaPlayItem(info.PickCode, info.Name, FilesInfo.FileType.File);
        await PlayVideoHelper.PlayVideo(new List<MediaPlayItem>() { mediaPlayItem }, this.XamlRoot, lastPage: this);
    }

    private async void Explorer_OnPlayWithPlayerClick(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuFlyoutItem { DataContext: FilesInfo info, Tag: string aTag }) return;

        if (!int.TryParse(aTag, out var playerSelection)) return;

        await Task.Delay(1);


        var mediaPlayItem = new MediaPlayItem(info.PickCode, info.Name, FilesInfo.FileType.File);
        await PlayVideoHelper.PlayVideo(new List<MediaPlayItem>() { mediaPlayItem }, this.XamlRoot, lastPage: this, playerSelection: playerSelection);
    }
}

public class SplideInfoProgress
{
    public VideoInfo videoInfo { get; set; }
    public MatchVideoResult matchResult { get; set; }

    public int index { get; set; } = 0;
}

public enum FileFormat { Video, Subtitles, Torrent, Image, Audio, Archive }

public class FileStatistics
{
    public FileStatistics(FileFormat name)
    {
        type = name;
        size = 0;
        count = 0;
        data = new();
    }

    public FileFormat type { get; set; }
    public long size { get; set; }
    public int count { get; set; }
    public List<Data> data { get; set; }

    public class Data
    {
        public string name { get; set; }
        public int count { get; set; } = 0;
        public long size { get; set; } = 0;
    }
}
