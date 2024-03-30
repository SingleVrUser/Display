using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.ApplicationModel.DataTransfer;
using Display.Models.Api.OneOneFive.File;
using Display.Models.Dto.OneOneFive;
using Display.Models.Spider;
using Display.Models.Vo.OneOneFive;
using Display.Views.Windows;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Sort115HomeModel = Display.Models.Vo.OneOneFive.Sort115HomeModel;

namespace Display.Views.Pages.Sort115;

public sealed partial class MainPage
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
            if (existsInfo != null) continue;

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

    public SpiderSourceName SpiderSourceName;

    public bool IsSelected;

    public FetchingSourceOptions(SpiderSourceName SpiderSourceName, bool isSelected = false)
    {
        this.SpiderSourceName = SpiderSourceName;
        name = SpiderSourceName.ToString();
        IsSelected = isSelected;
    }
}
