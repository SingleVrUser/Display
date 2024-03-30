// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Display.Models.Api.OneOneFive.File;
using Display.Models.Data.IncrementalCollection;
using Display.Models.Dto.OneOneFive;
using Display.Models.Vo;
using Display.Models.Vo.OneOneFive;
using Display.Providers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Display.Views.Pages.Settings;

public sealed partial class SelectedFolderPage
{
    private readonly IncrementalLoadDatumCollection _folderInfos;

    private readonly ObservableCollection<ExplorerItem> _explorerItems;

    private readonly ItemsPanelTemplate _myListViewPanelTemplate;
    private readonly ItemsPanelTemplate _myGridViewPanelTemplate;


    private readonly DataTemplate _myListViewDataTemplate;
    private readonly DataTemplate _myGridViewDataTemplate;

    public SelectedFolderPage()
    {
        InitializeComponent();

        _explorerItems = [];

        _folderInfos = new IncrementalLoadDatumCollection(0, isOnlyFolder: true);
        _folderInfos.GetFileInfoCompleted += FolderInfos_GetFileInfoCompleted;

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
        _explorerItems.Clear();

        foreach (var path in _folderInfos.WebPaths)
        {
            _explorerItems.Add(new ExplorerItem { Name = path.Name, Id = path.Cid });
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

        await _folderInfos.SetCid((long)cid);
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
        return _explorerItems.LastOrDefault();
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

            _folderInfos.Insert(0, new FilesInfo(new Datum { Cid = makeDirResult.Cid, Name = makeDirResult.Cname }));
        }

        CreateNewFolderGrid.Visibility = Visibility.Collapsed;
    }

    private void CancelCreateFolderButton_OnClick(object sender, RoutedEventArgs e)
    {
        CreateNewFolderGrid.Visibility = Visibility.Collapsed;
    }

}