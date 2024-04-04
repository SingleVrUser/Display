using System;
using Display.Constants;
using Display.Models.Dto.Settings;
using Display.Models.Enums;
using Display.ViewModels;
using Display.Views.Pages.More;
using Display.Views.Windows;
using Microsoft.UI.Xaml.Controls;

namespace Display.Views.Pages;

public sealed partial class MorePage
{
    private readonly MorePageViewModel _viewModel = App.GetService<MorePageViewModel>();

    public MorePage()
    {
        InitializeComponent();

    }

    private void GridView_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is not MoreMenuItem item) return;

        if (item.PageEnum == NavigationViewItemEnum.BrowserPage)
        {
            var window = new CommonWindow();
            window.Content = new BrowserPage(window, isShowButton: true);
            window.Activate();
        }
        else
        {
            var page = (Page)Activator.CreateInstance(item.PageType);
            CommonWindow.CreateAndShowWindow(page);
        }
    }
}