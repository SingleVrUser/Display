using System;
using System.Linq;
using Display.CustomWindows;
using Display.Models.Data;
using Display.Views.More.DatumList;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.Views.DetailInfo
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FileInfoInCidSmoke : Page
    {
        private string trueName;

        public FileInfoInCidSmoke(string trueName)
        {
            InitializeComponent();

            this.trueName = trueName;

            Loaded += PageLoad;
        }

        private async void PageLoad(object sender, RoutedEventArgs e)
        {
            var videoInfos = await DataAccess.Get.GetDatumByTrueName(trueName, null);

            InfosListView.ItemsSource = videoInfos;
        }

        public static string GetFolderString(long folderCid)
        {
            //从数据库中获取根目录信息
            var folderToRootList = DataAccess.Get.GetRootByCid(folderCid);

            return string.Join(" > ", folderToRootList.Select(x=>x.Name));
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
                new[] { fileId });

            if (!deleteResult) return;

            // 从数据库中删除
            DataAccess.Delete.DeleteDataInFilesInfoAndFileToInfo(info.PickCode);
        }
    }
}
