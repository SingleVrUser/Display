using System;
using System.Linq;
using Display.Models.Dto.OneOneFive;
using Display.Providers;
using Display.Views.Pages.More.DatumList;
using Display.Views.Windows;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Display.Views.Pages.DetailInfo;

public sealed partial class FileInfoInCidSmoke
{
    private readonly string _trueName;

    public FileInfoInCidSmoke(string trueName)
    {
        InitializeComponent();

        this._trueName = trueName;

        Loaded += PageLoad;
    }

    private async void PageLoad(object sender, RoutedEventArgs e)
    {
        var videoInfos = await DataAccess.Get.GetDatumByTrueName(_trueName, null);

        InfosListView.ItemsSource = videoInfos;
    }

    public static string GetFolderString(long folderCid)
    {
        //从数据库中获取根目录信息
        var folderToRootList = DataAccess.Get.GetRootByCid(folderCid);

        return string.Join(" > ", folderToRootList.Select(x => x.Name));
    }

    private void OpenCurrentFolderItem_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuFlyoutItem { DataContext: Datum info }) return;

        CommonWindow.CreateAndShowWindow(new FileListPage(info.Cid));
    }

    private async void DeleteItem_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuFlyoutItem { DataContext: Datum info }) return;

        if (info.Fid == null) return;

        var fileId = (long)info.Fid;

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

        if (result != ContentDialogResult.Primary) return;

        // 从115中删除 
        var deleteResult = await WebApi.GlobalWebApi.DeleteFiles(info.Cid,
            [fileId]);

        if (!deleteResult) return;

        // 从数据库中删除
        DataAccess.Delete.DeleteDataInFilesInfoAndFileToInfo(info.PickCode);
    }
}