using System;
using System.Linq;
using DataAccess.Dao.Interface;
using DataAccess.Models.Entity;
using Display.Providers;
using Display.Views.Pages.More.DatumList;
using Display.Views.Windows;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Display.Views.Pages.DetailInfo;

public sealed partial class FileInfoInCidSmoke
{
    private static readonly IFilesInfoDao FilesInfoDao = App.GetService<IFilesInfoDao>();
    
    private readonly string _trueName;

    public FileInfoInCidSmoke(string trueName)
    {
        InitializeComponent();

        _trueName = trueName;

        Loaded += PageLoad;
    }

    private async void PageLoad(object sender, RoutedEventArgs e)
    {
        var videoInfos = DataAccessLocal.Get.GetFilesInfoByTrueName(_trueName);

        InfosListView.ItemsSource = videoInfos;
    }

    private static string GetFolderString(long folderCid)
    {
        //从数据库中获取根目录信息
        var folderToRootList = FilesInfoDao.GetFolderListToRootByFolderId(folderCid);

        return string.Join(" > ", folderToRootList.Select(x => x.Name));
    }

    private void OpenCurrentFolderItem_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuFlyoutItem { DataContext: FilesInfo info }) return;

        CommonWindow.CreateAndShowWindow(new FileListPage(info.CurrentId));
    }

    private async void DeleteItem_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuFlyoutItem { DataContext: FilesInfo info }) return;

        if (info.FileId == null) return;

        var fileId = (long)info.FileId;

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
        var deleteResult = await WebApi.GlobalWebApi.DeleteFiles(info.CurrentId,
            [fileId]);

        if (!deleteResult) return;

        // 从数据库中删除
        FilesInfoDao.RemoveByPickCode(info.PickCode);
    }
}