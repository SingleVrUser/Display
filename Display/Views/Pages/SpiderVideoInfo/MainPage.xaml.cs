using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using DataAccess.Models.Entity;
using Display.Helper.Network;
using Display.Models.Dto.Media;
using Display.Models.Enums;
using Display.Models.Vo;
using Display.Models.Vo.OneOneFive;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Display.Views.Pages.SpiderVideoInfo;

public sealed partial class MainPage : INotifyPropertyChanged
{
    private FileInfo _selectedDatum;
    public FileInfo SelectedDatum
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

    private Tuple<List<string>, List<FileInfo>> GetCurrentSelectedFolder()
    {
        List<string> selectFilesNameList = [];
        List<FileInfo> datumList = [];
        foreach (var node in Explorer.FolderTreeView.SelectedNodes)
        {
            if (node.Content is not ExplorerItem explorer) continue;

            //文件夹
            selectFilesNameList.Add(explorer.Name);

            //文件夹下的文件和文件夹
            var items = Explorer.GetFilesFromItems(explorer.Id, FileType.File);

            datumList.AddRange(items);
        }

        return new Tuple<List<string>, List<FileInfo>>(selectFilesNameList, datumList);
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
    
    private void FailTypeComboBoxChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.FirstOrDefault() is not ComboBoxItem comboBoxItem) return;
        
        ChangedFailListType(comboBoxItem);
    }

    private void ChangedFailListType(ComboBoxItem comboBoxItem)
    {
        Debug.WriteLine("切换");
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