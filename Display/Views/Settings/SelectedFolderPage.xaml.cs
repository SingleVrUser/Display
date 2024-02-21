// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Display.Models.Data;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Display.Services.IncrementalCollection;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.Views.Settings
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SelectedFolderPage : Page
    {
        private readonly IncrementalLoadDatumCollection folderInfos;

        private readonly ObservableCollection<ExplorerItem> explorerItems;

        private readonly ItemsPanelTemplate _myListViewPanelTemplate;
        private readonly ItemsPanelTemplate _myGridViewPanelTemplate;


        private readonly DataTemplate _myListViewDataTemplate;
        private readonly DataTemplate _myGridViewDataTemplate;

        public SelectedFolderPage()
        {
            InitializeComponent();

            explorerItems = new ObservableCollection<ExplorerItem>();

            folderInfos = new IncrementalLoadDatumCollection(0, isOnlyFolder: true);
            folderInfos.GetFileInfoCompleted += FolderInfos_GetFileInfoCompleted;

            _myListViewPanelTemplate = Resources["ListViewPanelTemplate"] as ItemsPanelTemplate;
            _myGridViewPanelTemplate = Resources["GridViewPanelTemplate"] as ItemsPanelTemplate;

            _myListViewDataTemplate = Resources["ListViewDataTemplate"] as DataTemplate;
            _myGridViewDataTemplate = Resources["GridViewDataTemplate"] as DataTemplate;
        }


        public async Task<ContentDialogResult> ShowContentDialogResult(XamlRoot xamlRoot)
        {
            ContentDialog dialog = new()
            {
                XamlRoot = xamlRoot,
                Content = this,
                CloseButtonText = "返回",
                PrimaryButtonText = "保存到该目录",
                DefaultButton = ContentDialogButton.Primary,
                Resources =
                {
                    // 使用更大的 MaxWidth
                    ["ContentDialogMaxWidth"] = 700
                }
            };

            return await dialog.ShowAsync();
        }

        private void FolderInfos_GetFileInfoCompleted(object sender, GetFileInfoCompletedEventArgs e)
        {
            Debug.WriteLine("加载完成");
            explorerItems.Clear();

            foreach (var path in folderInfos.WebPaths)
            {
                explorerItems.Add(new ExplorerItem { Name = path.name, Id = path.cid });
            }
        }

        private void FolderBreadcrumbBar_ItemClicked(BreadcrumbBar sender, BreadcrumbBarItemClickedEventArgs args)
        {
            if (args.Item is not ExplorerItem item) return;
            var cid = item.Id;

            OpenFolder(cid);
        }

        private async void OpenFolder(long? cid)
        {
            if (cid == null) return;

            await folderInfos.SetCid((long)cid);
        }

        private void ListViewBase_OnItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is not FilesInfo info) return;

            OpenFolder(info.Id);
        }

        private void ChangedViewButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (MyListView.ItemsPanel == _myGridViewPanelTemplate)
            {
                MyListView.ItemsPanel = _myListViewPanelTemplate;
                MyListView.ItemTemplate = _myListViewDataTemplate;
            }
            else
            {
                MyListView.ItemsPanel = _myGridViewPanelTemplate;
                MyListView.ItemTemplate = _myGridViewDataTemplate;
            }
        }

        public ExplorerItem GetCurrentFolder()
        {
            return explorerItems.LastOrDefault();
        }

        private void CreateNewFolderButton_OnClick(object sender, RoutedEventArgs e)
        {
            CreateNewFolderGrid.Visibility = Visibility.Visible;

        }

        private async void CreateFolderButton_OnClick(object sender, RoutedEventArgs e)
        {
            var currentFolder = GetCurrentFolder();
            if (currentFolder != null)
            {
                var makeDirResult = await WebApi.GlobalWebApi.RequestMakeDir(currentFolder.Id, NewCreateFolderTextBox.Text);
                if (makeDirResult == null) return;

                folderInfos.Insert(0, new FilesInfo(new Datum() { Cid = makeDirResult.cid, Name = makeDirResult.cname }));
            }

            CreateNewFolderGrid.Visibility = Visibility.Collapsed;
        }

        private void CancelCreateFolderButton_OnClick(object sender, RoutedEventArgs e)
        {
            CreateNewFolderGrid.Visibility = Visibility.Collapsed;
        }

    }
}
