using System;
using System.Collections.Generic;
using System.Diagnostics;
using Display.Helper.Data;
using Display.Models.Data;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using ImageViewModel = Display.ViewModels.ImageViewModel;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.Views.More.DatumList
{
    public sealed partial class ImagePage:Page
    {
        public ImageViewModel ViewModel { get; }

        public ImagePage()
        {
            InitializeComponent();

            ViewModel = App.GetService<ImageViewModel>();

            Unloaded += MainPage_Unloaded;
        }

        private void MainPage_Unloaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("查看缓存大小，如果过大则清空");
            LocalCacheHelper.ClearAllCacheIfSizeTooLarge();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {   
            base.OnNavigatedTo(e);

            if (e.Parameter is not Tuple<List<FilesInfo>, int> tuple) return;

            var (files, currentIndex) = tuple;

            await ViewModel.SetDataAndCurrentIndex(files, currentIndex);

            ImageViewer.SelectionChanged += MyViewerOnSelectionChanged;
            ImageViewer.SelectedIndex = currentIndex;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            ViewModel.ClearAllPhotosCommand.Execute(null);
        }

        private void GoBackButtonClick(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack) Frame.GoBack();
        }

        private async void MyViewerOnSelectionChanged(object sender, int newValue)
        {
            await ViewModel.PreparePhotoCommand.ExecuteAsync(newValue);
            ImageViewer.ChangedImage(newValue);
        }

        private void ClearImageClick(object sender, RoutedEventArgs e)
        {
            ViewModel.ClearAllPhotosCommand.Execute(null);
        }

        private void ClearCacheImageClick(object sender, RoutedEventArgs e)
        {
            LocalCacheHelper.ClearAllCache();
        }

        private async void OpenCacheDirClick(object sender, RoutedEventArgs e)
        {
            await LocalCacheHelper.OpenCachePath();
        }

        private async void ExportImageClick(object sender, RoutedEventArgs e)
        {
            await ViewModel.ExportCurrentImageCommand.ExecuteAsync(XamlRoot.Content);
        }

        /// <summary>
        /// 打开详情信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenInfoButtonClick(object sender, RoutedEventArgs e)
        {
            VisualStateManager.GoToState(this,
                InfoGrid.Visibility == Visibility.Collapsed ? "ShowInfoGrid" : "HiddenInfoGrid", true);
        }

        private async void MenuFlyoutItem_OnClick(object sender, RoutedEventArgs e)
        {
            await ViewModel.OpenWithOtherApplicationCommand.ExecuteAsync(null);
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
    }
}
