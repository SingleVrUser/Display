﻿using System;
using Display.Constants;
using Display.Models.Enums;
using Display.Models.Settings;
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
        this.InitializeComponent();

    }

    private void GridView_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is not MoreMenuItem item) return;

        if (!PageTypeAndEnum.PageTypeAndEnumDict.TryGetValue(item.PageEnum, out var pageType)) return;

        var page = (Page)Activator.CreateInstance(pageType);

        if (item.PageEnum == NavigationViewItemEnum.BrowserPage)
        {
            var window = new CommonWindow();
            window.Content = new BrowserPage(window, isShowButton: true);
            window.Activate();
        }
        else
        {
            CommonWindow.CreateAndShowWindow(page);
        }
    }
}