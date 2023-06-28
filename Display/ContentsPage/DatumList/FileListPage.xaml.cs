// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using ByteSizeLib;
using Display.Data;
using Display.Helper;
using Display.Models;
using Display.Views;
using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Microsoft.UI.Dispatching;
using Windows.Storage;
using Display.ViewModels;
using Display.Services.Upload;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.ContentsPage.DatumList;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class FileListPage : INotifyPropertyChanged
{
    private ObservableCollection<ExplorerItem> _units;
    private ExplorerItem CurrentExplorerItem => _units.LastOrDefault();

    private IncrementalLoadDatumCollection _filesInfos;
    private IncrementalLoadDatumCollection FilesInfos
    {
        get => _filesInfos;
        set
        {
            if (_filesInfos == value) return;
            _filesInfos = value;

            OnPropertyChanged();
        }
    }

    private WebApi _webApi;

    private WebApi WebApi => _webApi ??= WebApi.GlobalWebApi;

    /// <summary>
    /// 中转站文件
    /// </summary>
    private ObservableCollection<TransferStationFiles> _transferStationFiles;

    public FileListPage(long cid = 0)
    {
        this.InitializeComponent();

        List<ExplorerItem> unit;
        if (cid == 0)
        {
            unit = new List<ExplorerItem> { new() { Name = "根目录", Id = 0 } };

        }
        else
        {
            var folderToRootList = DataAccess.GetRootByCid(cid);
            unit = folderToRootList.Select(x => new ExplorerItem { Name = x.Name, Id = x.Cid }).ToList();
        }

        InitData(unit);
    }


    private void InitData(List<ExplorerItem> units)
    {
        MyProgressBar.Visibility = Visibility.Visible;

        _units = new ObservableCollection<ExplorerItem>();
        units.ForEach(_units.Add);

        MetadataControl.ItemsSource = _units;

        FilesInfos = new IncrementalLoadDatumCollection(CurrentExplorerItem?.Id ?? 0);
        BaseExample.ItemsSource = FilesInfos;

        FilesInfos.GetFileInfoCompleted += FilesInfos_GetFileInfoCompleted;
    }



    public event PropertyChangedEventHandler PropertyChanged;

    public void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        // Raise the PropertyChanged event, passing the Name of the property whose value has changed.
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void FilesInfos_GetFileInfoCompleted(object sender, GetFileInfoCompletedEventArgs e)
    {
        ChangedOrderIcon(e.orderby, e.asc);
        MyProgressBar.Visibility = Visibility.Collapsed;
    }

    private async Task OpenFolder(ExplorerItem currentItem)
    {
        //不存在，返回
        if (currentItem?.Id == null) return;

        //删除选中路径后面的路径
        var index = _units.IndexOf(currentItem);

        //不存在，返回
        if (index < 0) return;

        for (int i = _units.Count - 1; i > index; i--)
        {
            _units.RemoveAt(i);
        }

        await FilesInfos.SetCid(currentItem.Id);

        // 切换目录时，全选checkBox不是选中状态
        MultipleSelectedCheckBox.IsChecked = false;
    }

    private void OpenFile_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        if (sender is not Grid { DataContext: FilesInfo info }) return;

        // 不是文件夹或视频就跳过
        if (info.Type == FilesInfo.FileType.Folder) return;

        if (!info.IsVideo)
        {
            ShowTeachingTip("不支持打开非视频文件");

            return;
        }

        // 只有文件能双击，文件夹Click后就跳转到新页面了
        var mediaPlayItem = new MediaPlayItem(info.PickCode, info.Name, info.Type);

        DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal,
           () => _ = PlayVideoHelper.PlayVideo(new List<MediaPlayItem> { mediaPlayItem }, XamlRoot, lastPage: this));

    }

    private async void ChangedFolder(FilesInfo filesInfo)
    {
        //跳过文件
        if (filesInfo.Type == FilesInfo.FileType.File) return;

        if (filesInfo.Id == null) return;

        var id = (long)filesInfo.Id;

        await FilesInfos.SetCid(id);

        _units.Add(new ExplorerItem
        {
            Name = filesInfo.Name,
            Id = id,
        });

        // 切换目录时，全选checkBox不是选中状态
        MultipleSelectedCheckBox.IsChecked = false;
    }

    private void TextBlock_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
    {
        if (sender is not TextBlock { DataContext: FilesInfo filesInfo }) return;

        ChangedFolder(filesInfo);
    }

    private void ToTopButton_Click(object sender, RoutedEventArgs e)
    {
        if (BaseExample.ItemsSource is IncrementalLoadDatumCollection collection)
            BaseExample.ScrollIntoView(collection.First());
    }

    private void PlayButton_Click(object sender, RoutedEventArgs e)
    {
        if (BaseExample.SelectedItems is null || BaseExample.SelectedItems.Count == 0)
        {
            ShowTeachingTip("当前未选中文件");
            return;
        }

        ////检查选中的文件或文件夹
        //if (BaseExample.SelectedItems.FirstOrDefault() is not FilesInfo) return;

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
                ChangedOrder(WebApi.OrderBy.FileName, Name_Run);
                break;
            case "修改时间":
                ChangedOrder(WebApi.OrderBy.UserPtime, Time_Run);
                break;
            case "大小":
                ChangedOrder(WebApi.OrderBy.FileSize, Size_Run);
                break;
        }
    }

    private async void ChangedOrder(WebApi.OrderBy orderBy, Run run)
    {
        string UpSortIconRun = "\uE014";
        int asc = run.Text == UpSortIconRun ? 0 : 1;

        await FilesInfos.SetOrder(orderBy, asc);
    }

    private void ChangedOrderIcon(WebApi.OrderBy orderBy, int asc)
    {
        string UpSortIconRun = "\uE014";
        string DownSortIconRun = "\uE015";

        Run[] OrderIconRunList = new[] { Time_Run, Name_Run, Size_Run };

        Run run;

        switch (orderBy)
        {
            case WebApi.OrderBy.FileName:
                run = Name_Run;
                break;
            case WebApi.OrderBy.UserPtime:
                run = Time_Run;
                break;
            case WebApi.OrderBy.FileSize:
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
        // 排除缺失Id的文件，比如秒传后临时添加的文件
        var infos = e.Items.Cast<FilesInfo>().Where(x => !x.NoId).ToList();
        if (infos.Count == 0) return;

        // 添加数据
        e.Data.Properties.Add("items", infos);

        // 显示回收站
        RecycleStationGrid.Visibility = Visibility.Visible;

        // 显示中转站
        TransferStationGrid.Visibility = Visibility.Visible;


    }

    /// <summary>
    /// 拖拽文件，在中转站上松开
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void TransferStationGrid_Drop(object sender, DragEventArgs e)
    {
        Debug.WriteLine("拖拽文件，在中转站上松开");
        if (sender is not Grid) return;

        if (e.DataView.Properties.Values.FirstOrDefault() is not List<FilesInfo> sourceFilesInfos) return;

        if (_transferStationFiles == null)
        {
            _transferStationFiles = new ObservableCollection<TransferStationFiles>();
            TransferStationListView.ItemsSource = _transferStationFiles;
        };

        _transferStationFiles.Add(new TransferStationFiles(sourceFilesInfos));
    }

    /// <summary>
    /// 拖拽文件，在回收站上不松开
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void DeletedFileMove_DragOver(object sender, DragEventArgs e)
    {
        e.AcceptedOperation = DataPackageOperation.Link;

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

        // 应用内的文件拖动
        if (e.DataView.Properties.Values.FirstOrDefault() is List<FilesInfo> sourceFilesInfos)
        {
            HandleCaption(e, target, sourceFilesInfos);
        }
        //从外部拖入文件信息
        else if (e.DataView.Contains(StandardDataFormats.StorageItems))
        {
            e.AcceptedOperation = DataPackageOperation.Link;
            e.DragUIOverride.Caption = "松开后开始上传";
        }
    }

    private void HandleCaption(DragEventArgs e, ListView target, List<FilesInfo> sourceFilesInfos)
    {
        var index = GetInsertIndexInListView(target, e);
        // 范围之外
        if (index == -1)
        {
            // 文件从中转站中拖拽过来，允许拖拽文件到此处
            if (!FilesInfos.Contains(sourceFilesInfos.FirstOrDefault()))
            {
                e.AcceptedOperation = DataPackageOperation.Move;
            }

            //否则禁止操作
            else
            {
                e.AcceptedOperation = DataPackageOperation.None;
                e.DragUIOverride.Caption = null;
            }
        }
        else
        {
            var item = FilesInfos[index];
            //目标与移动文件有重合，退出
            if (sourceFilesInfos.Contains(item))
            {
                Debug.WriteLine("目标与移动文件有重合，禁止操作");

                e.AcceptedOperation = DataPackageOperation.None;
                e.DragUIOverride.Caption = null;
            }
            else if (item.Type == FilesInfo.FileType.File)
            {
                Debug.WriteLine("目标为文件，禁止操作");

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
        if (sender is not Grid) return;

        if (e.DataView.Properties.Values.FirstOrDefault() is not List<Data.FilesInfo> sourceFilesInfos) return;

        if (_transferStationFiles == null)
        {
            e.AcceptedOperation = DataPackageOperation.Link;
        }
        else
        {
            // 如果移动的文件已经在中转站了，没必要再移动了
            var sameFile = _transferStationFiles.Where(item =>
            {
                foreach (var file in item.TransferFiles)
                {
                    if (!sourceFilesInfos.Contains(file)) return false;
                }

                return true;

            }).FirstOrDefault();

            if (sameFile == null)
            {
                e.AcceptedOperation = DataPackageOperation.Link;
            }
        }

    }

    /// <summary>
    /// 点击清空中转站
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void EmptyTransferStationButton_Click(object sender, RoutedEventArgs e)
    {
        if (_transferStationFiles == null) return;

        _transferStationFiles.Clear();

        TransferStationGrid.Visibility = Visibility.Collapsed;
    }

    /// <summary>
    /// 拖拽文件，在回收站上松开
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void RecycleStationGrid_Drop(object sender, DragEventArgs e)
    {
        if (sender is not Grid) return;

        if (e.DataView.Properties.Values.FirstOrDefault() is not List<FilesInfo> sourceFilesInfos) return;

        //115删除
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

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            await Delete115Files(sourceFilesInfos);
        }

        VisualStateManager.GoToState(this, "NoDelete", true);
    }

    /// <summary>
    /// 删除文件
    /// </summary>
    /// <param name="sourceFilesInfos"></param>
    /// <returns></returns>
    private async Task Delete115Files(List<FilesInfo> sourceFilesInfos)
    {
        // 删除文件列表中的文件
        TryRemoveFilesInExplorer(sourceFilesInfos);

        // 删除中转站中的文件
        TryRemoveTransferFiles(sourceFilesInfos);

        var cid = sourceFilesInfos.First().Cid;

        // 从115中删除
        await WebApi.DeleteFiles((long)cid,
            sourceFilesInfos.Where(item=>item.Id!=null).Select(item => (long)item.Id).ToArray());

    }

    private void RecycleStationGrid_DragEnter(object sender, DragEventArgs e)
    {
        VisualStateManager.GoToState(this, "ReadyDelete", true);
    }

    private void RecycleStationGrid_DragLeave(object sender, DragEventArgs e)
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

        Debug.WriteLine("拖拽文件，在文件列表上松开");
        if (e.DataView.Properties.Values.FirstOrDefault() is List<FilesInfo> sourceFilesInfos)
        {
            await HandleFileDrop(e, target, sourceFilesInfos);
        }
        else if (e.DataView.Contains(StandardDataFormats.StorageItems))
        {
            var items = await e.DataView.GetStorageItemsAsync();

            var storageFiles = items.Where(i => i.IsOfType(StorageItemTypes.File) && i is StorageFile).Select(x => x as StorageFile).ToArray();

            if (storageFiles.Length == 0) return;

            // 标记当前文件夹，避免切换文件夹后才上传成功导致的错误文件（UI）添加，影响显示
            var currentFolderCid = CurrentExplorerItem.Id;

            foreach (var storageFile in storageFiles)
            {
                UploadViewModel.Instance.AddUploadTask(storageFile.Path, currentFolderCid, result =>
                {
                    if (!result.Success) return;

                    if (currentFolderCid != CurrentExplorerItem.Id) return;

                    // 上传成功后更新UI
                    FilesInfos.Insert(0, new FilesInfo(result));
                });
            }

            //添加任务后显示传输任务窗口
            TaskPage.ShowSingleWindow();
        }
    }

    private async Task HandleFileDrop(DragEventArgs e, ListView target, List<FilesInfo> sourceFilesInfos)
    {
        var index = GetInsertIndexInListView(target, e);

        //在范围之外
        if (index == -1)
        {
            // 文件从中转站中拖拽过来，允许拖拽文件到此处
            if (!FilesInfos.Contains(sourceFilesInfos.FirstOrDefault()))
            {
                await Move115Files(FilesInfos.Cid, sourceFilesInfos);

                // 在文件列表中添加
                sourceFilesInfos.ForEach(FilesInfos.Add);
            }
            else
            {
                Debug.WriteLine("在范围之外，退出");
                return;
            }
        }
        else
        {
            var item = FilesInfos[index];

            //目标与移动文件有重合，退出
            if (sourceFilesInfos.Contains(item))
            {
                Debug.WriteLine("目标与移动文件有重合，退出");
                return;
            }

            await Move115Files(item.Cid, sourceFilesInfos);
        }
    }

    /// <summary>
    /// 删除文件列表中的文件
    /// </summary>
    /// <param name="files"></param>
    private void TryRemoveFilesInExplorer(List<FilesInfo> files)
    {
        foreach (var item in files.Where(FilesInfos.Contains))
        {
            FilesInfos.Remove(item);
        }
    }


    private void TryRemoveTransferFiles(ICollection<FilesInfo> files)
    {
        if (_transferStationFiles is not { Count: > 0 }) return;

        var transferListReadyRemove = _transferStationFiles.Where(item => item.TransferFiles.All(files.Contains)).ToList();

        foreach (var file in transferListReadyRemove)
        {
            _transferStationFiles.Remove(file);
        }
    }

    /// <summary>
    /// 移动115文件
    /// </summary>
    /// <returns></returns>
    private async Task Move115Files(long cid, List<FilesInfo> files)
    {
        await WebApi.MoveFiles(cid, files.Select(item => item.Id).ToArray());
            
        // 从中转站列表中删除已经移动的文件
        TryRemoveTransferFiles(files);
    }


    private int GetInsertIndexInListView(ListView target, DragEventArgs e)
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
        // 显示回收站
        RecycleStationGrid.Visibility = Visibility.Visible;

        // 添加数据
        var infos = e.Items.Cast<TransferStationFiles>().ToList();
        e.Data.Properties.Add("items", TransferStationFilesToFilesInfo(infos));
    }

    private List<FilesInfo> TransferStationFilesToFilesInfo(List<TransferStationFiles> srcList)
    {
        List<FilesInfo> infos = new();
        foreach (TransferStationFiles item in srcList)
        {
            infos.AddRange(item.TransferFiles);
        }

        return infos;
    }


    private void Source_DragItemsCompleted(ListViewBase sender, DragItemsCompletedEventArgs args)
    {
        // 隐藏回收站
        RecycleStationGrid.Visibility = Visibility.Collapsed;

        // 中转站为空时隐藏
        if (_transferStationFiles == null || _transferStationFiles.Count == 0)
        {
            TransferStationGrid.Visibility = Visibility.Collapsed;
        }

        // 移动成功
        if (args.DropResult != DataPackageOperation.Move) return;

        // 删除文件列表
        TryRemoveFilesInExplorer(args.Items.Cast<FilesInfo>().ToList());
    }

    private async void ImportDataButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button) return;

        List<FilesInfo> files = new();
        List<string> folderList = new();
        foreach (FilesInfo item in BaseExample.SelectedItems)
        {
            files.Add(item);

            if (item.Type == FilesInfo.FileType.Folder)
            {
                folderList.Add(item.Name);
            }
        }

        if (files.Count == 0)
        {
            ShowTeachingTip("当前未选中文件");
            return;
        }

        //确认对话框
        var isContinue = true;
        if (folderList.Count > 0)
        {
            var receiveResult = await ShowContentDialog(folderList);
            if (receiveResult != ContentDialogResult.Primary) isContinue = false;
        }

        if (!isContinue) return;

        var page = new Import115DataToLocalDataAccess.Progress(files);
        page.CreateWindow();

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

        ContentDialog dialog = new ContentDialog
        {
            XamlRoot = XamlRoot,
            Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
            Title = "确认后继续",
            PrimaryButtonText = "继续",
            CloseButtonText = "取消",
            DefaultButton = ContentDialogButton.Primary,
            Content = readyStackPanel
        };

        var result = await dialog.ShowAsync();

        return result;
    }

    private async void deleData_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new ContentDialog
        {
            XamlRoot = XamlRoot,
            Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
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
            DataAccess.DeleteTable(DataAccess.TableName.FilesInfo); ;
        }
    }

    private async void DownButton_Click(object sender, RoutedEventArgs e)
    {
        await DownFiles(WebApi.DownType.Bc);
    }

    private async void Aria2Down_Click(object sender, RoutedEventArgs e)
    {
        await DownFiles(WebApi.DownType.Aria2);
    }

    private async void Browser115Down_Click(object sender, RoutedEventArgs e)
    {
        await DownFiles(WebApi.DownType._115);
    }

    private async Task DownFiles(WebApi.DownType downType)
    {
        if (BaseExample.SelectedItems is null || BaseExample.SelectedItems.Count == 0)
        {
            ShowTeachingTip("当前未选中文件");
            return;
        }

        if (BaseExample.SelectedItems.FirstOrDefault() is not FilesInfo) return;

        List<Datum> videoInfos = new();

        foreach (var item in BaseExample.SelectedItems)
        {
            if (item is not FilesInfo fileInfo) continue;

            Datum datum = new()
            {
                Cid = fileInfo.Cid,
                Name = fileInfo.Name,
                PickCode = fileInfo.PickCode,
                Fid = fileInfo.Id
            };
            videoInfos.Add(datum);
        }

        //BitComet只需要cid,n,pc三个值
        var isSuccess = await WebApi.RequestDown(videoInfos, downType);

        if (!isSuccess)
            ShowTeachingTip("请求下载失败");
    }

    private void ShowTeachingTip(string message)
    {
        BasePage.ShowTeachingTip(lightDismissTeachingTip: LightDismissTeachingTip, message);
    }

    private async void PlayVideoButton_Click(object sender, RoutedEventArgs e)
    {
        // 多个播放
        if (sender is not MenuFlyoutItem menuFlyoutItem) return;
        if (menuFlyoutItem.DataContext is null)
        {
            if (!BaseExample.SelectedItems.Any())
            {
                ShowTeachingTip("当前未选中文件");
                return;
            }

            //获取需要播放的文件
            var files = BaseExample.SelectedItems.Cast<FilesInfo>().ToList();

            // 挑选视频文件，并转换为mediaPlayItem
            var mediaPlayItems = files
                .Where(x => x.Type == FilesInfo.FileType.Folder || x.IsVideo)
                .Select(x => new MediaPlayItem(x.PickCode, x.Name, x.Type, x.Cid)).ToList();

            await PlayVideoHelper.PlayVideo(mediaPlayItems, this.XamlRoot, lastPage: this);

            return;
        }

        // 单个播放
        if (menuFlyoutItem.DataContext is not FilesInfo info) return;

        if (info.Type == FilesInfo.FileType.Folder || !info.IsVideo) return;

        var mediaPlayItem = new MediaPlayItem(info.PickCode, info.Name, info.Type, info.Cid);
        await PlayVideoHelper.PlayVideo(new List<MediaPlayItem> { mediaPlayItem }, this.XamlRoot, lastPage: this);
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

    private async void PlayWithPlayerButtonClick(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuFlyoutItem menuFlyoutItem) return;
        if (menuFlyoutItem is not { Tag: string aTag }) return;
        if (!int.TryParse(aTag, out var playerSelection)) return;

        // 多个播放
        if (menuFlyoutItem.DataContext is null)
        {
            if (BaseExample.SelectedItems is null || BaseExample.SelectedItems.Count == 0)
            {
                ShowTeachingTip("当前未选中文件");
                return;
            }

            //获取需要播放的文件
            var files = BaseExample.SelectedItems.Cast<FilesInfo>().ToList();

            var mediaPlayItems = files
                .Where(x => x.Type == FilesInfo.FileType.Folder || x.IsVideo)
                .Select(x => new MediaPlayItem(x.PickCode, x.Name, x.Type, x.Cid)).ToList();
            await PlayVideoHelper.PlayVideo(mediaPlayItems, this.XamlRoot, lastPage: this, playerSelection: playerSelection);

            return;
        }

        // 单个播放
        if (menuFlyoutItem is not { DataContext: FilesInfo info }) return;

        if (info.Type == FilesInfo.FileType.Folder || !info.IsVideo) return;

        var mediaPlayItem = new MediaPlayItem(info.PickCode, info.Name, info.Type, info.Cid);

        await PlayVideoHelper.PlayVideo(new List<MediaPlayItem> { mediaPlayItem }, XamlRoot, lastPage: this, playerSelection: playerSelection);
    }

    private async void MoveToNewFolderItemClick(object sender, RoutedEventArgs e)
    {
        List<FilesInfo> fileInfos;
        // 选中文件，对当前文件操作
        if (BaseExample.SelectedItems.Count == 0)
        {
            if (sender is not MenuFlyoutItem { DataContext: FilesInfo info }) return;

            fileInfos = new List<FilesInfo>
            {
                info
            };


        }
        else
        {
            //获取需要整理的文件
            fileInfos = BaseExample.SelectedItems.Cast<FilesInfo>().ToList();

        }

        if (fileInfos.Count == 0) return;

        var currentFolderId = CurrentExplorerItem.Id;

        // 移除前缀
        var infos = fileInfos.Select(x =>
                            Regex.Replace(x.NameWithoutExtension, "\\w+.(com|cn|xyz|la|me|net|app|cc)@", string.Empty, RegexOptions.IgnoreCase))
                            .ToArray();

        var sameName = MatchHelper.GetSameFirstStringFromList(infos);

        // 移除后缀
        sameName = Regex.Replace(sameName, "[-_.]part$", string.Empty, RegexOptions.IgnoreCase);

        TextBox inputTextBox = new()
        {
            Text = sameName,
            PlaceholderText = "输入新文件夹的名称"
        };

        ContentDialog contentDialog = new()
        {
            XamlRoot = XamlRoot,
            Content = inputTextBox,
            Title = "新文件夹",
            PrimaryButtonText = "确定",
            CloseButtonText = "取消",
            DefaultButton = ContentDialogButton.Primary
        };

        var result = await contentDialog.ShowAsync();

        if (result != ContentDialogResult.Primary) return;

        // 新建文件夹
        var makeDirResult = await WebApi.RequestMakeDir(currentFolderId, inputTextBox.Text);
        if (makeDirResult == null) return;

        // 移动文件到新文件夹
        Debug.WriteLine($"移动文件数量：{fileInfos.Count}");
        await Move115Files(makeDirResult.cid, fileInfos);

        // 更新UI
        // 新建文件夹
        FilesInfos.Insert(0, new FilesInfo(new Datum() { Cid = makeDirResult.cid, Name = makeDirResult.cname }));

        // 删除文件
        TryRemoveFilesInExplorer(fileInfos);
    }

    private async void RenameItemClick(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuFlyoutItem { DataContext: FilesInfo info }) return;

        TextBox inputTextBox = new()
        {
            Text = info.NameWithoutExtension,
            PlaceholderText = info.NameWithoutExtension,
            SelectionStart = info.NameWithoutExtension.Length
        };

        ContentDialog contentDialog = new()
        {
            XamlRoot = this.XamlRoot,
            Content = inputTextBox,
            Title = "重命名",
            PrimaryButtonText = "确定",
            CloseButtonText = "取消",
            DefaultButton = ContentDialogButton.Primary
        };

        var result = await contentDialog.ShowAsync();

        if (result != ContentDialogResult.Primary) return;

        // 添加后缀，模仿官方
        var inputText = inputTextBox.Text;
        var newName = string.IsNullOrEmpty(info.Datum.Ico) ? inputText : $"{inputText}.{info.Datum.Ico}";

        //没变
        if (newName == info.Name) return;

        var renameRequest = await WebApi.RenameFile(info.Id, newName);
        if (renameRequest == null)
        {
            ShowTeachingTip("重命名失败");
            return;
        }

        var firstData = renameRequest.data.FirstOrDefault();

        if (firstData.Key != null)
        {
            info.Name = firstData.Value;
        }
    }

    private async void FolderBreadcrumbBar_ItemClicked(BreadcrumbBar sender, BreadcrumbBarItemClickedEventArgs args)
    {
        if (args.Item is not ExplorerItem item) return;

        await OpenFolder(item);
    }

    private void MetadataItem_OnDragOver(object sender, DragEventArgs e)
    {
        if (sender is not TextBlock { DataContext: ExplorerItem item }) return;

        // 拖拽中的文件（可能多个）
        if (e.DataView.Properties.Values.FirstOrDefault() is not List<FilesInfo> sourceFilesInfos) return;

        // 目标cid
        var cid = item.Id;

        var canMoveList = GetFilesInfosExceptInCid(cid, sourceFilesInfos);

        if (canMoveList.Any())
        {
            e.AcceptedOperation = DataPackageOperation.Move;
            e.DragUIOverride.Caption = $"移动到 {item.Name}";
        }
        else
        {
            e.AcceptedOperation = DataPackageOperation.None;
            e.DragUIOverride.Caption = null;
        }
    }

    private static List<FilesInfo> GetFilesInfosExceptInCid(long cid, List<FilesInfo> infos)
    {
        return infos.Where(x => x.Cid != cid).ToList();
    }

    private async void MetadataItem_OnDrop(object sender, DragEventArgs e)
    {
        if (sender is not TextBlock { DataContext: ExplorerItem item }) return;

        // 拖拽中的文件（可能多个）
        if (e.DataView.Properties.Values.FirstOrDefault() is not List<FilesInfo> sourceFilesInfos) return;

        var canMoveList = GetFilesInfosExceptInCid(item.Id, sourceFilesInfos);

        await Move115Files(item.Id, canMoveList);

        // 移动到当前文件夹的时候，文件列表需要添加
        var index = _units.IndexOf(item);
        if (index == _units.Count - 1)
        {
            sourceFilesInfos.ForEach(x =>
                FilesInfos.Insert(0, x));
        }
    }

    private void TransferStationListView_OnDragItemsCompleted(ListViewBase sender, DragItemsCompletedEventArgs args)
    {
        // 隐藏回收站
        RecycleStationGrid.Visibility = Visibility.Collapsed;

        if (args.DropResult is not DataPackageOperation.Move) return;

        // 移动成功，删除列表文件（如果存在）
        var infos = args.Items.Cast<TransferStationFiles>().ToList();
        TryRemoveFilesInExplorer(TransferStationFilesToFilesInfo(infos));

        // 如果中转站为空，就隐藏（目前数量为一个，移动后将为0）
        if (_transferStationFiles.Count == 1)
        {
            TransferStationGrid.Visibility = Visibility.Collapsed;
        }
    }

    private void SelectedMultipleFilesCheckBox_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is not CheckBox checkBox) return;

        if (checkBox.IsChecked is true)
        {
            BaseExample.SelectAll();
        }
        else
        {
            BaseExample.SelectedItems.Clear();
        }
    }

    private async void GoBack_KeyboardAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        if (_units.Count <= 1) return;

        var currentItem = _units[^2];
        await OpenFolder(currentItem);
    }

    private async void ShowInfoClick(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuFlyoutItem { DataContext: FilesInfo info }) return;

        string title;

        var infos = new Dictionary<string, string>
        {
            {"名称",info.Name }
        };

        if (info.Type == FilesInfo.FileType.Folder)
        {
            infos.Add("id", info.Id.ToString());
            infos.Add("pickCode", info.PickCode);
            infos.Add("所在目录id", info.Cid.ToString());

            title = "文件夹属性";
        }
        else
        {
            infos.Add("id", info.Id.ToString());
            infos.Add("pickCode", info.PickCode);
            infos.Add("大小", ByteSize.FromBytes(info.Datum.Size).ToString("#.#"));
            infos.Add("sha1", info.Datum.Sha);
            infos.Add("所在目录id", info.Cid.ToString());

            title = "文件属性";
        }

        await InfoPage.ShowInContentDialog(XamlRoot, infos, title);
    }

    private async void ShowFolderInfoClick(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuFlyoutItem { DataContext: ExplorerItem info }) return;

        var infos = new Dictionary<string, string>
        {
            {"名称",info.Name },
            {"cid",info.Id.ToString() },
        };

        await InfoPage.ShowInContentDialog(XamlRoot, infos, "属性");
    }

    private async void RefreshFolderClick(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuFlyoutItem { DataContext: ExplorerItem info }) return;

        await OpenFolder(info);
    }


    private async void Refresh_KeyboardAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        await OpenFolder(CurrentExplorerItem);
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
            this.Name = $"{transferFiles.FirstOrDefault()?.Name}";
        }
        else if (transferFiles.Count > 1)
        {
            this.Name = $"{transferFiles.FirstOrDefault()?.Name} 等{transferFiles.Count}个文件";
        }

        this.TransferFiles = transferFiles;

    }

}
