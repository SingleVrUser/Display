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
using Display.Models;
using SpiderInfo = Display.Models.SpiderInfo;
using Org.BouncyCastle.Asn1.X509;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.ContentsPage.Sort115;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainPage : Page
{
    //private List<FetchingSourceOptions> fetchingCommonSourceList;

    public MainPage(List<FilesInfo> files)
    {
        InitializeComponent();

        ViewModel.SetFiles(files);

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
        var window = new CommonWindow("归档整理")
        {
            Content = this
        };
        window.Activate();
    }


    private void FolderVideoDragOver(object sender, DragEventArgs e)
    {
        if (!e.Data.Properties.TryGetValue("items", out var value))
        {
            if (!e.Data.Properties.TryGetValue("folder", out value) || value is not FilesInfo) return;

            e.AcceptedOperation = DataPackageOperation.Link;
        }
        else if (value is List<FilesInfo> files)
        {
            if (files.All(x => x.Type == FilesInfo.FileType.File))
            {
                e.AcceptedOperation = DataPackageOperation.Link;
            }

        }
    }

    private void FolderVideoListView_OnDrop(object sender, DragEventArgs e)
    {
        if (!e.Data.Properties.TryGetValue("items", out var value))
        {
            if (!e.Data.Properties.TryGetValue("folder", out value) || value is not FilesInfo folder) return;

            if (ViewModel.SelectedFiles.All(model => model.Info != folder))
                ViewModel.SelectedFiles.Add(new Sort115HomeModel(folder));
        }
        else if (value is List<FilesInfo> files)
        {
            foreach (var info in files)
            {
                ViewModel.SelectedFiles.Add(new Sort115HomeModel(info));
            }
        }
    }

    private void SingleFolderSaveVideoDragOver(object sender, DragEventArgs e)
    {
        if (!e.Data.Properties.TryGetValue("items", out var value)) return;

        if (value is List<FilesInfo> {Count:0})
            e.AcceptedOperation = DataPackageOperation.Link;
    }

    private void FolderVideoDragItemStarting(object sender, DragItemsStartingEventArgs e)
    {
        //var folder = e.Items.Cast<FilesInfo>().FirstOrDefault();

        //e.Data.Properties.Add("folder", folder);
        //e.Data.Properties.Add("source", nameof(FolderVideoListView));
    }

    private async void Video18SettingsButtonClick(object sender, RoutedEventArgs e)
    {
        var result = await new ContentDialog()
        {
            XamlRoot = XamlRoot,
            Title = "设置",
            PrimaryButtonText = "保存",
            CloseButtonText = "返回",
            DefaultButton = ContentDialogButton.Primary,
            Content = new Settings18Page(),
            Resources =
            {
                // 使用更大的 MaxWidth
                ["ContentDialogMaxWidth"] = 700
            }
        }.ShowAsync();

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
