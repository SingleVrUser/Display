// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI.UI.Controls;
using Data;
using Display.Model;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Org.BouncyCastle.Asn1.Cms;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Windows.ApplicationModel.DataTransfer;
using static System.Net.Mime.MediaTypeNames;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.ContentsPage.DatumList;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class FileListPage : Page, INotifyPropertyChanged
{
    ObservableCollection<MetadataItem> _units;

    IncrementallLoadDatumCollection _filesInfos;
    IncrementallLoadDatumCollection filesInfos
    {
        get => _filesInfos;
        set
        {
            if(_filesInfos == value) return;
            _filesInfos = value;

            OnPropertyChanged();
        }
    }

    public FileListPage()
    {
        this.InitializeComponent();

        _units = new ObservableCollection<MetadataItem>() { new MetadataItem { Label = "根目录", Command = OpenFolderCommand, CommandParameter = "0" } };
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        // Raise the PropertyChanged event, passing the name of the property whose value has changed.
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Grid加载时调用
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Grid_loaded(object sender, RoutedEventArgs e)
    {
        ProgressRing.IsActive = true;

        filesInfos = new("0");
        BaseExample.ItemsSource = filesInfos;
        metadataControl.Items = _units;
        filesInfos.GetFileInfoCompleted += FilesInfos_GetFileInfoCompleted;

        ProgressRing.IsActive = false;

    }

    private void FilesInfos_GetFileInfoCompleted(object sender, GetFileInfoCompletedEventArgs e)
    {
        ChangedOrderIcon(e.orderby, e.asc);
    }

    private RelayCommand<string> _openFolderCommand;

    private RelayCommand<string> OpenFolderCommand =>
        _openFolderCommand ??= new RelayCommand<string>(OpenFolder);

    private void OpenFolder(string cid)
    {

        var currentItem = _units.FirstOrDefault(item => item.CommandParameter.ToString() == cid);

        //不存在，返回
        if (currentItem.CommandParameter == null) return;

        //删除选中路径后面的路径
        var index = _units.IndexOf(currentItem);

        //不存在，返回
        if(index <0 ) return;

        for(int i= _units.Count-1; i> index; i--)
        {
            _units.RemoveAt(i);
        }

        filesInfos.SetCid(cid);
    }

    private void OpenFolder_Tapped(object sender, Microsoft.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
    {
        if (!(sender is Grid grid)) return;
        if (!(grid.DataContext is FilesInfo filesInfo)) return;


        ChangedFolder(filesInfo);
    }

    private void ChangedFolder(FilesInfo filesInfo)
    {
        //跳过文件
        if (filesInfo.Type == FilesInfo.FileType.File) return;

        filesInfos.SetCid(filesInfo.Cid);

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
        if(BaseExample.ItemsSource is IncrementallLoadDatumCollection Collection)
            BaseExample.ScrollIntoView(Collection.First());
    }

    private void PlayButton_Click(object sender, RoutedEventArgs e)
    {
        //检查选中的文件或文件夹
        if (BaseExample.SelectedItems.FirstOrDefault() is not FilesInfo) return;

        List<FilesInfo> filesInfo = new();
        foreach(var item in BaseExample.SelectedItems)
        {
            filesInfo.Add((FilesInfo)item);
        }

        var page = new VideoDisplay.MainPage(filesInfo);
        page.CreateWindow();
    }

    private void OrderBy_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
    {
        if (sender is not TextBlock textBlock) return;

        if(textBlock.Inlines.FirstOrDefault() is not Run run) return;

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

    private async void ChangedOrder(WebApi.OrderBy orderBy,Run run)
    {
        string UpSortIconRun = "\uE014";
        int asc = run.Text == UpSortIconRun ? 0 : 1;

        await filesInfos.SetOrder(orderBy,asc);
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

    private void Source_DragOver(object sender, DragEventArgs e)
    {
        e.AcceptedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Copy;
    }


    private void Source_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
    {
        List<Data.FilesInfo> filesInfos = new();

        foreach (Data.FilesInfo item in e.Items)
        {
            filesInfos.Add(item);
        }

        e.Data.Properties.Add("items", filesInfos);

    }
}
