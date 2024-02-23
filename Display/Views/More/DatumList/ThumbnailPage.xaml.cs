using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Display.ViewModels;
using Display.Models.Data;
using Microsoft.UI.Xaml.Navigation;
using System.Collections.Generic;
using System.Collections.Specialized;
using System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.Views.More.DatumList;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class ThumbnailPage : Page
{
    private readonly ThumbnailViewModel _viewModel;

    public ThumbnailPage()
    {
        this.InitializeComponent();

        _viewModel = App.GetService<ThumbnailViewModel>();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is not List<FilesInfo> filesInfos) return;

        _viewModel.SetData(filesInfos);
        ThumbnailsCvs.Source = _viewModel.ThumbnailList;
        _viewModel.ThumbnailList.CollectionChanged += ThumbnailList_CollectionChanged;

        ImageViewer.SelectionChanged += ImageViewer_SelectionChanged;

        _viewModel.StartAsync(() =>
        {
            ImageViewer.SelectedIndex = 0;
        });
    }

    private void ThumbnailList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action != NotifyCollectionChangedAction.Remove) return;

        // 全部删除时
        if (_viewModel.ThumbnailList.Count == 0)
        {
            TitleTextBlock.Visibility = Visibility.Collapsed;
            ThumbnailInfoGrid.Visibility = Visibility.Collapsed;
        }
        else
        {
            ImageViewer.SelectedIndex = 0;
        }
    }

    private void ImageViewer_SelectionChanged(object sender, int e)
    {
        ImageViewer.ChangedImage(e);
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
            _viewModel.DeleteAsyncCommand.Execute(null);
        }
    }
}