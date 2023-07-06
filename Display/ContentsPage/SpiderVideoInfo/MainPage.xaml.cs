
using Display.Data;
using Display.Helper;
using Display.Models;
using Display.Models.IncrementalCollection;
using Display.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

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
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public MainPage()
    {
        InitializeComponent();
    }

    /// <summary>
    /// 展开Expander初始化检查图片路径和网络
    /// </summary>
    /// <param Name="sender"></param>
    /// <param Name="args"></param>
    private void Expander_Expanding(Expander sender, ExpanderExpandingEventArgs args)
    {
        if (sender.Content as ConditionalCheck == null)
        {
            sender.Content = new ConditionalCheck(this);
        }

        sender.SetValue(Grid.ColumnProperty, 0);
        sender.SetValue(Grid.ColumnSpanProperty, 2);
    }

    /// <summary>
    /// 关闭Expander
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void Expander_Collapsed(Expander sender, ExpanderCollapsedEventArgs args)
    {
        sender.SetValue(Grid.ColumnProperty, 1);
        sender.SetValue(Grid.ColumnSpanProperty, 1);
    }

    /// <summary>
    /// 点击匹配按钮开始匹配
    /// </summary>
    /// <param Name="sender"></param>
    /// <param Name="e"></param>
    private void StartMatchName_ButtonClick(object sender, RoutedEventArgs e)
    {
        //从本地数据库中搜刮
        if (LocalDataRadioButton.IsChecked == true)
        {
            SpiderFromLocalData();
        }
        //从失败列表中搜刮（带匹配名称）
        else
        {
            SpiderFromFailList();
        }
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

    private void ShowTeachingTip(string content)
    {
        BasePage.ShowTeachingTip(SelectNullTeachingTip,content);
    }

    /// <summary>
    /// 从本地数据库中搜刮
    /// </summary>
    private void SpiderFromLocalData()
    {
        //检查是否有选中文件
        if (Explorer.FolderTreeView.SelectedNodes.Count == 0)
        {
            ShowTeachingTip("没有选择文件，请选择后继续");
            return;
        }

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
        foreach (var node in Explorer.FolderTreeView.SelectedNodes)
        {
            if (node.Content is not ExplorerItem explorer) continue;

            //文件夹
            selectFilesNameList.Add(explorer.Name);

            //文件夹下的文件和文件夹
            var items = Explorer.GetFilesFromItems(explorer.Id, FilesInfo.FileType.File);

            datumList.AddRange(items);
        }

        return new Tuple<List<string>, List<Datum>>(selectFilesNameList, datumList);
    }

    /// <summary>
    /// 点击显示文件信息
    /// </summary>
    /// <param Name="sender"></param>
    /// <param Name="e"></param>
    private void ExplorerItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is not FilesInfo itemInfo) return;

        if (FileInfoShow_Grid.Visibility == Visibility.Collapsed) FileInfoShow_Grid.Visibility = Visibility.Visible;
        SelectedDatum = itemInfo.Datum;
    }

    /// <summary>
    /// 点击TreeView的Item显示文件信息
    /// </summary>
    /// <param Name="sender"></param>
    /// <param Name="args"></param>
    private void TreeView_ItemInvoked(TreeView sender, TreeViewItemInvokedEventArgs args)
    {
        if (args.InvokedItem is not TreeViewNode{Content:ExplorerItem content}) return;

        if (FileInfoShow_Grid.Visibility == Visibility.Collapsed) FileInfoShow_Grid.Visibility = Visibility.Visible;
        SelectedDatum = content.datum;
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

    private void FailTypeComboBoxChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems[0] is ComboBoxItem comboBoxItem)
        {
            ChangedFailListType(comboBoxItem);
        }
    }

    private async void ChangedFailListType(ComboBoxItem comboBoxItem)
    {
        if (FailListView.ItemsSource == null)
        {
            FailList = new();
        }
        else if (FailList.Count != 0)
        {
            FailList.Clear();
        }

        switch (comboBoxItem.Name)
        {
            //正则匹配失败
            case nameof(ShowMatchFailComboBoxItem):
                FailList.SetShowType(FailType.MatchFail);
                await FailList.LoadData();
                break;
            //搜刮失败
            case nameof(ShowSpiderFailComboBoxItem):
                FailList.SetShowType(FailType.SpiderFail);
                await FailList.LoadData();
                break;
            //所有
            default:
                FailList.SetShowType(FailType.All);
                await FailList.LoadData();
                break;
        }
    }

    private void ShowData_RadioButtons_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems[0] is RadioButton radioButton)
        {
            switch (radioButton.Name)
            {
                //本地数据库
                case nameof(LocalDataRadioButton):
                    FailShowTypeComboBox.SelectionChanged -= FailTypeComboBoxChanged;
                    break;
                //搜刮失败
                case nameof(MatchFailRadioButton):
                    if (FailShowTypeComboBox.SelectionBoxItem == null)
                    {
                        ChangedFailListType(ShowAllFailComboBoxItem);
                    }

                    FailShowTypeComboBox.SelectionChanged += FailTypeComboBoxChanged;
                    break;
            }
        }
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

public class SpiderInfoProgress
{
    public VideoInfo VideoInfo { get; set; }
    public MatchVideoResult MatchResult { get; set; }

    public int Index { get; set; } = 0;
}

public enum FileFormat { Video, Subtitles, Torrent, Image, Audio, Archive }

public class FileStatistics
{
    public FileStatistics(FileFormat name)
    {
        Type = name;
        Size = 0;
        Count = 0;
        data = new();
    }

    public FileFormat Type { get; set; }
    public long Size { get; set; }
    public int Count { get; set; }
    public List<Data> data { get; set; }

    public class Data
    {
        public string Name { get; set; }
        public int Count { get; set; } = 0;
        public long Size { get; set; } = 0;
    }
}
