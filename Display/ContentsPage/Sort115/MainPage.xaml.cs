// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Display.ViewModels;
using Display.WindowView;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.ApplicationModel.DataTransfer;
using Display.Data;
using SpiderInfo = Display.Models.SpiderInfo;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.ContentsPage.Sort115;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainPage : Page
{
    //private List<FetchingSourceOptions> fetchingCommonSourceList;

    private readonly Sort115HomeViewModel viewModel;

    public MainPage(List<FilesInfo> folders)
    {
        this.InitializeComponent();

        viewModel = this.DataContext as Sort115HomeViewModel;
        viewModel?.SetFolders(folders);

        ////搜刮源
        //fetchingCommonSourceList = new List<FetchingSourceOptions>();
        //foreach(SpiderSourceName source in Enum.GetValues(typeof(SpiderSourceName)))
        //{
        //    // 忽略本地
        //    if(source!= SpiderSourceName.Local)
        //    {
        //        fetchingCommonSourceList.Add(new(source));
        //    }
        //}
    }

    public void CreateWindow()
    {
        CommonWindow window = new CommonWindow("归档整理");
        window.Content = this;
        window.Activate();
    }


    private void FolderVideoDragOver(object sender, DragEventArgs e)
    {
        if (!e.Data.Properties.TryGetValue("items", out var value))
        {
            if (!e.Data.Properties.TryGetValue("folder", out value) || value is not FilesInfo folder) return;

            e.AcceptedOperation = DataPackageOperation.Move;
        }
        else if (value is List<FilesInfo>)
        {
            e.AcceptedOperation = DataPackageOperation.Move;
        }
    }

    private void SingleFolderSaveVideoDrop(object sender, DragEventArgs e)
    {
        if (!e.Data.Properties.TryGetValue("items", out var value) || value is not List<FilesInfo> folders) return;

        if (folders.Count != 1) return;

        if (ViewModel.SingleFolderSaveVideo.Count > 0) ViewModel.SingleFolderSaveVideo.Clear();

        ViewModel.SingleFolderSaveVideo.Add(folders.First());
    }
    private void FolderVideoGridView_OnDrop(object sender, DragEventArgs e)
    {
        if (!e.Data.Properties.TryGetValue("items", out var value))
        {
            if (!e.Data.Properties.TryGetValue("folder", out value) || value is not FilesInfo folder) return;

            if (!ViewModel.FoldersVideo.Contains(folder))
                ViewModel.FoldersVideo.Add(folder);
        }
        else if (value is List<FilesInfo> files)
        {
            var folders = files.Where(f => f.Type == FilesInfo.FileType.Folder);

            foreach (var folder in folders)
            {
                if (!ViewModel.FoldersVideo.Contains(folder))
                    ViewModel.FoldersVideo.Add(folder);
            }
        }
    }

    private void SingleFolderSaveVideoDragOver(object sender, DragEventArgs e)
    {
        if (!e.Data.Properties.TryGetValue("items", out var value)) return;

        if (value is List<FilesInfo>)
            e.AcceptedOperation = DataPackageOperation.Link;
    }

    private void FolderVideoDragItemStarting(object sender, DragItemsStartingEventArgs e)
    {
        var folder = e.Items.Cast<FilesInfo>().FirstOrDefault();

        e.Data.Properties.Add("folder", folder);
        e.Data.Properties.Add("source", nameof(FolderVideoGridView));
    }

    private async void Video18SettingsButtonClick(object sender, RoutedEventArgs e)
    {
        var dialog = new ContentDialog()
        {
            XamlRoot = this.XamlRoot,
            Title = "设置",
            PrimaryButtonText = "保存",
            CloseButtonText = "返回",
            DefaultButton = ContentDialogButton.Primary,
            Content = new Settings18Page(),
            Height = 370,
            Width = 700
        };

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            Debug.WriteLine("点击了确定");
        }
    }
}

public class FetchingSourceOptions
{
    public string name { get; private set; }

    public SpiderInfo.SpiderSourceName SpiderSourceName;

    public bool IsSelected;

    public FetchingSourceOptions(SpiderInfo.SpiderSourceName SpiderSourceName, bool isSelected = false)
    {
        this.SpiderSourceName = SpiderSourceName;
        this.name = SpiderSourceName.ToString();
        IsSelected = isSelected;
    }
}
