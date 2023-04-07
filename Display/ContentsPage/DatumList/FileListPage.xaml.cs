// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI.UI.Controls;
using Display.Helper;
using Display.Models;
using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Display.Data;
using Display.Views;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.ContentsPage.DatumList;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class FileListPage : INotifyPropertyChanged
{
    ObservableCollection<MetadataItem> _units;

    IncrementalLoadDatumCollection _filesInfos;
    IncrementalLoadDatumCollection filesInfos
    {
        get => _filesInfos;
        set
        {
            if (_filesInfos == value) return;
            _filesInfos = value;

            OnPropertyChanged();
        }
    }

    WebApi webApi;


    /// <summary>
    /// 中转站文件
    /// </summary>
    ObservableCollection<TransferStationFiles> transferStationFiles;

    ///// <summary>
    ///// 回收站撤销文件
    ///// </summary>
    //List<FilesInfo> RecyFiles;

    public FileListPage()
    {
        this.InitializeComponent();

        _units = new ObservableCollection<MetadataItem>() { new MetadataItem { Label = "根目录", Command = OpenFolderCommand, CommandParameter = "0" } };
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        // Raise the PropertyChanged event, passing the Name of the property whose value has changed.
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Grid加载时调用
    /// </summary>
    /// <param Name="sender"></param>
    /// <param Name="e"></param>
    private void Grid_loaded(object sender, RoutedEventArgs e)
    {
        MyProgressBar.Visibility = Visibility.Visible;

        filesInfos = new IncrementalLoadDatumCollection("0");
        BaseExample.ItemsSource = filesInfos;
        MetadataControl.Items = _units;
        filesInfos.GetFileInfoCompleted += FilesInfos_GetFileInfoCompleted;
    }

    private void FilesInfos_GetFileInfoCompleted(object sender, GetFileInfoCompletedEventArgs e)
    {
        ChangedOrderIcon(e.orderby, e.asc);
        MyProgressBar.Visibility = Visibility.Collapsed;
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

        await filesInfos.SetCid(cid);
    }

    private void OpenFolder_Tapped(object sender, Microsoft.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
    {
        if (sender is not Grid grid) return;
        if (grid.DataContext is not FilesInfo filesInfo) return;


        ChangedFolder(filesInfo);
    }

    private async void ChangedFolder(FilesInfo filesInfo)
    {
        //跳过文件
        if (filesInfo.Type == FilesInfo.FileType.File) return;

        await filesInfos.SetCid(filesInfo.Cid);

        _units.Add(new MetadataItem
        {
            Label = filesInfo.Name,
            Command = OpenFolderCommand,
            CommandParameter = filesInfo.Cid,
        });
    }

    private void TextBlock_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
    {
        if (!(sender is TextBlock textBlock)) return;
        if (!(textBlock.DataContext is FilesInfo filesInfo)) return;

        ChangedFolder(filesInfo);
    }

    private void ToTopButton_Click(object sender, RoutedEventArgs e)
    {
        if (BaseExample.ItemsSource is IncrementalLoadDatumCollection Collection)
            BaseExample.ScrollIntoView(Collection.First());
    }

    private void PlayButton_Click(object sender, RoutedEventArgs e)
    {
        //检查选中的文件或文件夹
        if (BaseExample.SelectedItems.FirstOrDefault() is not FilesInfo) return;

        var filesInfo = BaseExample.SelectedItems.Cast<FilesInfo>().ToList();

        var page = new VideoDisplay.MainPage(filesInfo, BaseExample);
        page.CreateWindow();
    }

    private void OrderBy_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
    {
        if (sender is not TextBlock textBlock) return;

        if (textBlock.Inlines.FirstOrDefault() is not Run run) return;

        //string DownSortIconRun = "\uE015";

        switch (run.Text)
        {
            case "名称":
                ChangedOrder(WebApi.OrderBy.file_name, Name_Run);
                break;
            case "修改时间":
                ChangedOrder(WebApi.OrderBy.user_ptime, Time_Run);
                break;
            case "大小":
                ChangedOrder(WebApi.OrderBy.file_size, Size_Run);
                break;
        }
    }

    private async void ChangedOrder(WebApi.OrderBy orderBy, Run run)
    {
        string UpSortIconRun = "\uE014";
        int asc = run.Text == UpSortIconRun ? 0 : 1;

        await filesInfos.SetOrder(orderBy, asc);
    }

    private void ChangedOrderIcon(WebApi.OrderBy orderBy, int asc)
    {
        string UpSortIconRun = "\uE014";
        string DownSortIconRun = "\uE015";

        Run[] OrderIconRunList = new[] { Time_Run, Name_Run, Size_Run };

        Run run;

        switch (orderBy)
        {
            case WebApi.OrderBy.file_name:
                run = Name_Run;
                break;
            case WebApi.OrderBy.user_ptime:
                run = Time_Run;
                break;
            case WebApi.OrderBy.file_size:
                run = Size_Run;
                break;
            default:
                run = Time_Run;
                break;
        }

        foreach (var itemRun in OrderIconRunList)
        {
            if (itemRun == run)
            {
                itemRun.Text = asc == 1 ? UpSortIconRun : DownSortIconRun;
            }
            else if (itemRun.Text != string.Empty)
            {
                itemRun.Text = string.Empty;
            }
        }
    }



    private void Source_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
    {
        RecyStationGrid.Visibility = Visibility.Visible;

        TransferStation_Grid.Visibility = Visibility.Visible;

        List<Data.FilesInfo> filesInfos = new();

        foreach (Data.FilesInfo item in e.Items)
        {
            filesInfos.Add(item);
        }

        e.Data.Properties.Add("items", filesInfos);

    }

    /// <summary>
    /// 拖拽文件，在中转站上松开
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void TransferStationGrid_Drop(object sender, DragEventArgs e)
    {
        if (sender is not Grid) return;

        if (e.DataView.Properties.Values.FirstOrDefault() is not List<Data.FilesInfo> sourceFilesInfos) return;

        System.Diagnostics.Debug.WriteLine("拖拽文件，在中转站上松开");

        if (transferStationFiles == null)
        {
            transferStationFiles = new();
            TransferStation_ListView.ItemsSource = transferStationFiles;
        };

        transferStationFiles.Add(new(sourceFilesInfos));
    }

    private void DeletedFileMove_DragOver(object sender, DragEventArgs e)
    {
        e.AcceptedOperation = DataPackageOperation.Move;

        e.DragUIOverride.Caption = "删除";
    }

    /// <summary>
    /// 拖拽文件，在文件列表上方不松开
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void FileMove_DragOver(object sender, DragEventArgs e)
    {
        if (sender is not ListView target) return;

        if (e.DataView.Properties.Values.FirstOrDefault() is not List<Data.FilesInfo> sourceFilesInfos) return;
        int index = GetInsertIndex(target, e);

        // 范围之外
        if (index == -1)
        {
            e.AcceptedOperation = DataPackageOperation.None;
            e.DragUIOverride.Caption = null;

            // 文件从中转站中拖拽过来，允许拖拽文件到此处
            if (!filesInfos.Contains(sourceFilesInfos.FirstOrDefault()))
            {
                e.AcceptedOperation = DataPackageOperation.Move;
            }
        }
        else
        {
            var item = filesInfos[index];
            //目标与移动文件有重合，退出
            if (sourceFilesInfos.Contains(item))
            {
                System.Diagnostics.Debug.WriteLine("目标与移动文件有重合，退出");

                e.AcceptedOperation = DataPackageOperation.None;
                e.DragUIOverride.Caption = null;
            }
            else
            {
                e.AcceptedOperation = DataPackageOperation.Move;
                e.DragUIOverride.Caption = $"移动到 {item.Name}";
            }
        }

    }


    /// <summary>
    /// 拖拽文件，在中转站上方不松开
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void TransferMove_DragOver(object sender, DragEventArgs e)
    {
        if (sender is not Grid target) return;

        if (e.DataView.Properties.Values.FirstOrDefault() is not List<Data.FilesInfo> sourceFilesInfos) return;

        if (transferStationFiles == null)
        {
            e.AcceptedOperation = DataPackageOperation.Move;
        }
        else
        {
            // 如果移动的文件已经在中转站了，没必要再移动了
            var sameFile = transferStationFiles.Where(item =>
            {
                if (sourceFilesInfos.Count == item.TransferFiles.Count)
                {
                    foreach (var file in item.TransferFiles)
                    {
                        if (!sourceFilesInfos.Contains(file)) return false;
                    }

                    return true;
                }

                return false;
            }).FirstOrDefault();

            if (sameFile == null)
            {
                e.AcceptedOperation = DataPackageOperation.Move;
            }
        }



    }

    private void EmptyTranferStationButton_Click(object sender, RoutedEventArgs e)
    {
        if (transferStationFiles == null) return;

        transferStationFiles.Clear();

        TransferStation_Grid.Visibility = Visibility.Collapsed;
    }

    private async void RecyStationGrid_Drop(object sender, DragEventArgs e)
    {
        if (sender is not Grid) return;

        if (e.DataView.Properties.Values.FirstOrDefault() is not List<Data.FilesInfo> sourceFilesInfos) return;

        //115删除
        ContentDialog dialog = new ContentDialog()
        {
            XamlRoot = this.XamlRoot,
            Title = "确认",
            PrimaryButtonText = "删除",
            CloseButtonText = "返回",
            DefaultButton = ContentDialogButton.Close,
            Content = "该操作将删除115网盘中的文件，确认删除？"
        };

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            sourceFilesInfos.ForEach(item => filesInfos.Remove(item));

            webApi ??= WebApi.GlobalWebApi;

            await webApi.DeleteFiles(sourceFilesInfos.FirstOrDefault()?.datum.pid, sourceFilesInfos.Select(item =>
            {
                //文件
                if (item.Type == FilesInfo.FileType.File)
                    return item.Fid;
                //文件夹
                else
                {
                    return item.Cid;
                }
            }).ToList());
        }

        VisualStateManager.GoToState(this, "NoDelete", true);
    }


    private void RecyStationGrid_DragEnter(object sender, DragEventArgs e)
    {
        VisualStateManager.GoToState(this, "ReadyDelete", true);
    }

    private void RecyStationGrid_DragLeave(object sender, DragEventArgs e)
    {
        VisualStateManager.GoToState(this, "NoDelete", true);
    }


    /// <summary>
    /// 拖拽文件，在文件列表上松开
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void BaseExample_Drop(object sender, DragEventArgs e)
    {
        if (sender is not ListView target) return;

        if (e.DataView.Properties.Values.FirstOrDefault() is not List<Data.FilesInfo> sourceFilesInfos) return;

        System.Diagnostics.Debug.WriteLine("拖拽文件，在文件列表上松开");

        int index = GetInsertIndex(target, e);

        //在范围之外
        if (index == -1)
        {
            // 文件从中转站中拖拽过来，允许拖拽文件到此处
            if (!filesInfos.Contains(sourceFilesInfos.FirstOrDefault()))
            {
                await Move115Files(filesInfos.cid, sourceFilesInfos);

                sourceFilesInfos.ForEach(item => filesInfos.Add(item));

            }
            else
            {
                System.Diagnostics.Debug.WriteLine("在范围之外，退出");
                return;
            }

        }
        else
        {
            var item = filesInfos[index];
            //目标与移动文件有重合，退出
            if (sourceFilesInfos.Contains(item))
            {
                System.Diagnostics.Debug.WriteLine("目标与移动文件有重合，退出");
                return;
            }

            await Move115Files(item.Cid, sourceFilesInfos);

        }

        //从中转站拖入的，中转站为空时隐藏
        if (TransferStation_Grid.Visibility == Visibility.Visible && (transferStationFiles == null || transferStationFiles.Count == 0))
        {
            TransferStation_Grid.Visibility = Visibility.Collapsed;
        }
    }

    /// <summary>
    /// 移动115文件
    /// </summary>
    /// <returns></returns>
    private async Task Move115Files(string pid, List<FilesInfo> files)
    {
        webApi ??= WebApi.GlobalWebApi;

        await webApi.MoveFiles(pid, files.Select(item => item.Type == FilesInfo.FileType.Folder ? item.Cid : item.Fid).ToList());

        //删除列表文件
        foreach (var item in files)
        {
            //文件列表中的
            if (filesInfos.Contains(item))
            {
                filesInfos.Remove(item);
            }
        }

        // 从中转站列表中删除已经移动的文件
        if (transferStationFiles != null)
        {
            var transferListReadlyRemove = transferStationFiles.Where(item =>
            {
                //if (item.TransferFiles.Count != files.Count)
                //    return false;

                foreach (var file in item.TransferFiles)
                {
                    if (!files.Contains(file))
                        return false;
                }

                return true;
            }).ToList();

            foreach (var file in transferListReadlyRemove)
            {
                transferStationFiles.Remove(file);

                System.Diagnostics.Debug.WriteLine("从中转站列表中删除已经移动的文件");
            }

        }

    }

    private int GetInsertIndex(ListView target, DragEventArgs e)
    {
        // Find the insertion index:
        Windows.Foundation.Point pos = e.GetPosition(target.ItemsPanelRoot);

        // If the target ListView has items in it, use the height of the first item
        //      to find the insertion index.
        int index = -1;
        if (target.Items.Count != 0)
        {
            // Get a reference to the first item in the ListView
            ListViewItem sampleItem = (ListViewItem)target.ContainerFromIndex(0);

            // Adjust itemHeight for margins
            double itemHeight = sampleItem.ActualHeight + sampleItem.Margin.Top + sampleItem.Margin.Bottom;

            // Find index based on dividing number of items by height of each item
            int tmp = (int)(pos.Y / itemHeight);

            if (tmp <= target.Items.Count - 1)
            {
                index = tmp;
            }
        }

        return index;
    }


    private void FilesTransferStation_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
    {
        List<Data.FilesInfo> filesInfos = new();

        foreach (TransferStationFiles item in e.Items)
        {
            filesInfos.AddRange(item.TransferFiles);
        }

        e.Data.Properties.Add("items", filesInfos);
    }


    private void Source_DragItemsCompleted(ListViewBase sender, DragItemsCompletedEventArgs args)
    {
        RecyStationGrid.Visibility = Visibility.Collapsed;

        if (transferStationFiles == null || transferStationFiles.Count == 0)
        {
            TransferStation_Grid.Visibility = Visibility.Collapsed;
        }
    }

    private async void ImportDataButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button) return;

        List<string> cids = new();
        List<string> nameList = new();
        foreach (FilesInfo item in BaseExample.SelectedItems)
        {
            if (item.Type == FilesInfo.FileType.Folder)
            {
                cids.Add(item.Cid);
                nameList.Add(item.Name);
            }
        }

        if (cids.Count == 0)
        {
            SelectedNull_TeachingTip.IsOpen = true;
            return;
        }

        //确认对话框
        var receiveResult = await ShowContentDialog(nameList);
        if (receiveResult == ContentDialogResult.Primary)
        {
            var page = new Import115DataToLocalDataAccess.Progress(cids);
            page.CreateWindow();
        }

    }

    /// <summary>
    /// 显示确认提示框
    /// </summary>
    /// <param Name="selectedItemList"></param>
    /// <returns></returns>
    private async Task<ContentDialogResult> ShowContentDialog(List<string> NameList)
    {
        StackPanel readyStackPanel = new StackPanel();

        readyStackPanel.Children.Add(new TextBlock() { Text = "选中文件夹：" });
        int index = 0;

        foreach (var name in NameList)
        {
            index++;
            TextBlock textBlock = new TextBlock()
            {
                Text = $"  {index}.{name}",
                IsTextSelectionEnabled = true,
                Margin = new Thickness(0, 2, 0, 0)
            };

            readyStackPanel.Children.Add(textBlock);
        }

        readyStackPanel.Children.Add(
            new TextBlock()
            {
                Text = "未进入隐藏系统的情况下，隐藏的文件将被跳过，请知晓",
                TextWrapping = TextWrapping.Wrap,
                Foreground = new SolidColorBrush(Colors.LightGray),
                MaxWidth = 300,
                Margin = new Thickness(0, 8, 0, 0)
            });

        ContentDialog dialog = new ContentDialog();

        dialog.XamlRoot = this.XamlRoot;
        dialog.Title = "确认后继续";
        dialog.PrimaryButtonText = "继续";
        dialog.CloseButtonText = "取消";
        dialog.DefaultButton = ContentDialogButton.Primary;
        dialog.Content = readyStackPanel;

        var result = await dialog.ShowAsync();

        return result;
    }

    private async void deleData_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new ContentDialog
        {
            XamlRoot = this.XamlRoot,
            Title = "确认后继续",
            PrimaryButtonText = "继续",
            CloseButtonText = "取消",
            DefaultButton = ContentDialogButton.Close
        };

        RichTextBlock textHighlightingRichTextBlock = new();

        Paragraph paragraph = new();
        paragraph.Inlines.Add(new Run() { Text = "该操作将" });
        paragraph.Inlines.Add(new Run() { Text = "删除", Foreground = new SolidColorBrush(Colors.OrangeRed), FontWeight = FontWeights.Bold, FontSize = 15 });
        paragraph.Inlines.Add(new Run() { Text = "之前导入的" });
        paragraph.Inlines.Add(new Run() { Text = "所有", Foreground = new SolidColorBrush(Colors.OrangeRed) });
        paragraph.Inlines.Add(new Run() { Text = "115数据" });

        textHighlightingRichTextBlock.Blocks.Add(paragraph);

        dialog.Content = textHighlightingRichTextBlock;

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            DataAccess.DeleteFilesInfoTable();
        }
    }

    private async void DownButton_Click(object sender, RoutedEventArgs e)
    {
        await DownFiles(Data.WebApi.downType.bc);
    }

    private async void Aria2Down_Click(object sender, RoutedEventArgs e)
    {
        await DownFiles(Data.WebApi.downType.bc);
    }

    private async Task DownFiles(Data.WebApi.downType downtype)
    {
        if (BaseExample.SelectedItems is null)
        {
            ShowTeachingTip("当前未选中要下载的文件或文件夹");
            return;
        }

        if (BaseExample.SelectedItems.FirstOrDefault() is not FilesInfo) return;

        webApi ??= WebApi.GlobalWebApi;

        List<Datum> videoinfos = new();

        foreach (var item in BaseExample.SelectedItems)
        {
            if (item is not FilesInfo fileinfo) continue;

            Datum datum = new();
            datum.cid = fileinfo.Cid;
            datum.n = fileinfo.Name;
            datum.pc = fileinfo.datum.pc;
            datum.fid = fileinfo.Fid;
            videoinfos.Add(datum);
        }

        //BitComet只需要cid,n,pc三个值
        bool isSuccess = await webApi.RequestDown(videoinfos, downtype);

        if (!isSuccess)
            ShowTeachingTip("请求下载失败");
    }

    private void ShowTeachingTip(string message)
    {
        BasePage.ShowTeachingTip(lightDismissTeachingTip:LightDismissTeachingTip, message);
    }
    
    private async void PlayVideoButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuFlyoutItem item) return;

        if (item.DataContext is not FilesInfo info) return;

        await PlayVideoHelper.PlayVideo(info.datum.pc, this.XamlRoot, trueName: info.Name, lastPage: this);
    }

    private void Sort115Button_Click(object sender, RoutedEventArgs e)
    {
        //检查选中的文件或文件夹
        if (BaseExample.SelectedItems.FirstOrDefault() is not FilesInfo) return;

        //获取需要整理的文件
        var folders = BaseExample.SelectedItems.Cast<FilesInfo>().ToList();

        var page = new Sort115.MainPage(folders);
        page.CreateWindow();
    }
}

class TransferStationFiles
{
    public string Name { get; set; }
    public List<FilesInfo> TransferFiles { get; set; }

    public TransferStationFiles(List<FilesInfo> transferFiles)
    {
        if (transferFiles.Count == 1)
        {
            this.Name = $"{transferFiles.FirstOrDefault().Name}";
        }
        else if (transferFiles.Count > 1)
        {
            this.Name = $"{transferFiles.FirstOrDefault().Name} 等{transferFiles.Count}个文件";
        }

        this.TransferFiles = transferFiles;

    }

}
