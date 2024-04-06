using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using DataAccess.Models.Entity;
using Display.Helper.Network;
using Display.Models.Dto.Media;
using Display.Models.Enums;
using Display.Models.Enums.OneOneFive;
using Display.Models.Vo;
using Display.Models.Vo.IncrementalCollection;
using Display.Models.Vo.OneOneFive;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Display.Views.Pages.SpiderVideoInfo;

public sealed partial class MainPage : INotifyPropertyChanged
{
    private IncrementalLoadFailSpiderInfoCollection _failList;
    private IncrementalLoadFailSpiderInfoCollection FailList
    {
        get => _failList;
        set => SetField(ref _failList, value);
    }

    private FilesInfo _selectedDatum;
    public FilesInfo SelectedDatum
    {
        get => _selectedDatum;
        private set => SetField(ref _selectedDatum, value);
    }


    public MainPage()
    {
        InitializeComponent();
    }

    /// <summary>
    /// 点击匹配按钮开始匹配
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void StartMatchName_ButtonClick(object sender, RoutedEventArgs e)
    {
        //从本地数据库中搜刮
        if (LocalDataItem.IsSelected)
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

        var failDatumList = FailList.Where(item => !string.IsNullOrEmpty(item.MatchName)).ToList();

        if (failDatumList.Count == 0)
        {
            ShowTeachingTip("未填写任何番号，请填写后继续");
            return;
        }

        //创建进度窗口
        var page = new Progress(failDatumList);
        page.CreateWindow();
    }

    private void ShowTeachingTip(string content)
    {
        BasePage.ShowTeachingTip(SelectNullTeachingTip, content);
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

    private Tuple<List<string>, List<FilesInfo>> GetCurrentSelectedFolder()
    {
        List<string> selectFilesNameList = [];
        List<FilesInfo> datumList = [];
        foreach (var node in Explorer.FolderTreeView.SelectedNodes)
        {
            if (node.Content is not ExplorerItem explorer) continue;

            //文件夹
            selectFilesNameList.Add(explorer.Name);

            //文件夹下的文件和文件夹
            var items = Explorer.GetFilesFromItems(explorer.Id, FileType.File);

            datumList.AddRange(items);
        }

        return new Tuple<List<string>, List<FilesInfo>>(selectFilesNameList, datumList);
    }

    /// <summary>
    /// 点击显示文件信息
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ExplorerItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is not DetailFileInfo itemInfo) return;

        FileInfoShowGrid.Visibility = Visibility.Visible;
        
        SelectedDatum = itemInfo.Datum;
    }

    /// <summary>
    /// 点击TreeView的Item显示文件信息
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void TreeView_ItemInvoked(TreeView sender, TreeViewItemInvokedEventArgs args)
    {
        if (args.InvokedItem is not TreeViewNode { Content: ExplorerItem content }) return;

        if (FileInfoShowGrid.Visibility == Visibility.Collapsed) FileInfoShowGrid.Visibility = Visibility.Visible;
        SelectedDatum = content.Datum;
    }
    
    /// <summary>
    /// 点击失败列表显示文件信息
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <exception cref="NotImplementedException"></exception>
    private void FailListView_OnItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is not FailDatum failDatum) return;

        FileInfoShowGrid.Visibility = Visibility.Visible;
        
        SelectedDatum = failDatum.Datum;
    }

    private void FailTypeComboBoxChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.FirstOrDefault() is not ComboBoxItem comboBoxItem) return;
        
        ChangedFailListType(comboBoxItem);
    }

    private void ChangedFailListType(ComboBoxItem comboBoxItem)
    {
        if (FailListView.ItemsSource == null)
        {
            FailList = [];
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
                FailList.LoadData();
                break;
            //搜刮失败
            case nameof(ShowSpiderFailComboBoxItem):
                FailList.SetShowType(FailType.SpiderFail);
                FailList.LoadData();
                break;
            //所有
            default:
                FailList.SetShowType(FailType.All);
                FailList.LoadData();
                break;
        }
    }

    private void ShowData_RadioButtons_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems[0] is not ListViewItem item) return;

        if (FailShowTypeComboBox == null) return;

        switch (item.Name)
        {
            //本地数据库
            case nameof(LocalDataItem):
                FailShowTypeComboBox.SelectionChanged -= FailTypeComboBoxChanged;
                break;
            //搜刮失败
            case nameof(MatchFailItem):
                if (FailShowTypeComboBox.SelectionBoxItem == null)
                {
                    ChangedFailListType(ShowAllFailComboBoxItem);
                }

                FailShowTypeComboBox.SelectionChanged += FailTypeComboBoxChanged;
                break;
        }
    }

    private async void Explorer_OnPlayVideoClick(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuFlyoutItem { DataContext: DetailFileInfo info }) return;

        var mediaPlayItem = new MediaPlayItem(info);
        await PlayVideoHelper.PlayVideo(new Collection<MediaPlayItem> { mediaPlayItem }, XamlRoot, lastPage: this);
    }

    private async void Explorer_OnPlayWithPlayerClick(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuFlyoutItem { DataContext: DetailFileInfo info, Tag: PlayerType playerType }) return;

        await Task.Delay(1);


        var mediaPlayItem = new MediaPlayItem(info);
        await PlayVideoHelper.PlayVideo(new List<MediaPlayItem> { mediaPlayItem }, XamlRoot, lastPage: this, playerType: playerType);
    }

    
    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

}