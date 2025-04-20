using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using CommunityToolkit.WinUI.UI.Controls;
using Display.Controls.UserController;
using Display.Models.Vo.OneOneFive;
using Display.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.Views.Pages.More.DatumList;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class ThumbnailPage : Page
{
    private readonly ThumbnailViewModel _viewModel;

    public ThumbnailPage()
    {
        InitializeComponent();

        this.NavigationCacheMode = NavigationCacheMode.Disabled;

        _viewModel = App.GetService<ThumbnailViewModel>();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is not List<DetailFileInfo> filesInfos) return;

        _viewModel.SetData(filesInfos);
        ThumbnailsCvs.Source = _viewModel.ThumbnailList;
        _viewModel.ThumbnailList.CollectionChanged += ThumbnailList_CollectionChanged;

        ImageViewer.SelectionChanged += ImageViewer_SelectionChanged;

        _viewModel.StartAsync(() =>
        {
            ImageViewer.SelectedIndex = 0;
        });
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);
        _viewModel.ClearDataCommand.Execute(null);
        _viewModel.ThumbnailList.CollectionChanged -= ThumbnailList_CollectionChanged;
        ImageViewer.SelectionChanged -= ImageViewer_SelectionChanged;
    }

    private void ThumbnailList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action != NotifyCollectionChangedAction.Remove) return;

        if (_viewModel.ThumbnailList.Count == 0)
        {
            TitleTextBlock.Visibility = Visibility.Collapsed;
            ThumbnailInfoGrid.Visibility = Visibility.Collapsed;
        }
        else
        {
            //if (e.OldItems[0] is GroupThumbnailCollection removeCollection)
            //{
            //    var _viewModel.ThumbnailList.IndexOf(removeCollection);
            //}
            int count = 0;
            for(int i = 0; i <  e.OldStartingIndex && i < _viewModel.ThumbnailList.Count - 1; i ++)
            {
                var item = _viewModel.ThumbnailList[i];
                count += item.Count;
            }

            ImageViewer.SelectedIndex = Math.Max(0, count);
        }
    }

    private void ImageViewer_SelectionChanged(object sender, int e)
    {
        var _ = ImageViewer.ChangedImage(e);
        _viewModel.SetCurrentItem(ThumbnailsCvs.View.CurrentItem);
    }

    private void TopGrid_PointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        VisualStateManager.GoToState(this, "ShowTopPanel", true);
        if (InfoGrid.Visibility == Visibility.Collapsed)
        {
            MoreButton.Visibility = Visibility.Visible;
        }
    }

    private void TopGrid_PointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        VisualStateManager.GoToState(this, "HiddenTopPanel", true);
        if (InfoGrid.Visibility == Visibility.Collapsed)
        {
            MoreButton.Visibility = Visibility.Collapsed;
        }
    }

    private void GoBackButtonClick(object sender, RoutedEventArgs e)
    {
        if (Frame.CanGoBack) Frame.GoBack();
    }

    private void OpenInfoButtonClick(object sender, RoutedEventArgs e)
    {
        VisualStateManager.GoToState(this,
            InfoGrid.Visibility == Visibility.Collapsed ? "ShowInfoGrid" : "HiddenInfoGrid", true);
    }

    private async void DeleteItemClicked(object sender, RoutedEventArgs e)
    {

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

        if (result == ContentDialogResult.Primary)
        {
            _viewModel.DeleteCommand.Execute(null);
        }
    }

    private void LocationImageClicked(object sender, RoutedEventArgs e)
    {
        adaptiveGridView.ScrollIntoView(ImageViewer.CurrentItemSource, ScrollIntoViewAlignment.Leading);
    }
}