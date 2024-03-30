// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Display.Constants;
using Display.Helper.UI;
using Display.Models.Dto.Settings;
using Display.Models.Enums;
using Display.ViewModels;
using Display.Views.Windows;
using Microsoft.UI.Xaml.Controls;
using WinUIEx;
using Window = Microsoft.UI.Xaml.Window;


namespace Display.Views.Pages.Tasks;

public sealed partial class MainPage
{
    private readonly TaskViewModel _viewModel = App.GetService<TaskViewModel>();

#nullable enable
    private static Window? _window;
#nullable disable

    public MainPage()
    {
        InitializeComponent();

        ContentNavigationView.SelectedItem = _viewModel.CurrentLink;
    }

    /// <summary>
    /// 仅允许打开单个窗口
    /// </summary>
    public static void ShowSingleWindow(NavigationViewItemEnum pageEnum = NavigationViewItemEnum.UploadTask)
    {
        if (_window == null)
        {
            var page = new MainPage();
            page.SetPage(pageEnum);

            _window = new CommonWindow("传输任务", 842, 537)
            {
                Content = page
            };
            _window.Closed += (_, _) =>
            {
                _window = null;
            };
            _window?.Show();
        }
        else
        {
            if (_window.Content is MainPage page)
            {
                page.SetPage(pageEnum);
            }

            WindowHelper.SetForegroundWindow(_window);
        }
    }

    public void SetPage(NavigationViewItemEnum pageEnum)
    {
        _viewModel.SetCurrentLink(pageEnum);
    }

    /// <summary>
    /// 响应切换页面的请求
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void ContentNavigationView_OnSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is not MenuItem currentLink) return;

        if (PageTypeAndEnum.PageTypeAndEnumDict.TryGetValue(currentLink.PageEnum, out var pageType))
        {
            ContentFrame.Navigate(pageType);
        }

    }

}