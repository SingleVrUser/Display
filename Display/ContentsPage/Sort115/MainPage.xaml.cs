// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Display.Data;
using Display.Models;
using Display.WindowView;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.ApplicationModel.DataTransfer;
using static Display.Data.Const;
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
        if (!e.Data.Properties.TryGetValue("items", out var value) || value is not List<FilesInfo> files) return;

        if (files.Any(x => x.Type == FilesInfo.FileType.Folder))
        {
            e.DragUIOverride.Caption = "暂不支持文件夹";
            return;
        }

        e.AcceptedOperation = DataPackageOperation.Link;
        e.DragUIOverride.Caption = "松开后添加";
    }

    private void FolderVideoListView_OnDrop(object sender, DragEventArgs e)
    {
        if (!e.Data.Properties.TryGetValue("items", out var value) || value is not List<FilesInfo> files) return;

        foreach (var info in files)
        {
            var existsInfo = ViewModel.SelectedFiles.FirstOrDefault(i => i.Info.Id == info.Id);
            if(existsInfo!=null) continue;

            ViewModel.SelectedFiles.Add(new Sort115HomeModel(info));
        }
    }

    private async void Video18SettingsButtonClick(object sender, RoutedEventArgs e)
    {
        var result = await new ContentDialog()
        {
            XamlRoot = XamlRoot,
            Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
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
