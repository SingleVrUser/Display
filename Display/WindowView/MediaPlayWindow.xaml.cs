// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Media;
using Windows.Storage.Streams;
using Windows.Storage;
using System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.WindowView;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MediaPlayWindow : Window
{
    private AppWindow appwindow;

    //全屏前记录当前状态
    private AppWindowPresenterKind _markPresenterKindBeforeFullScreen;

    private string PickCode;

    public MediaPlayWindow(string pickCode)
    {
        this.InitializeComponent();

        this.ExtendsContentIntoTitleBar = true;
        this.SetTitleBar(AppTitleBar);

        this.PickCode = pickCode;
        appwindow = App.getAppWindow(this);

        this.Closed += MediaPlayWindow_Closed;

    }

    private void MediaPlayWindow_Closed(object sender, WindowEventArgs args)
    {
        if (mediaControl.IsLoaded && VideoPlayGrid.Children.Contains(mediaControl))
        {
            mediaControl.StopMediaPlayer();
            VideoPlayGrid.Children.Remove(mediaControl);

        }
    }

    public static MediaPlayWindow CreateNewWindow(string pickCode)
    {
        MediaPlayWindow newWindow = new(pickCode);
        newWindow.Activate();

        return newWindow;
    }

    private Visibility isPickCodeNull()
    {
        return PickCode == null ? Visibility.Visible : Visibility.Collapsed;
    }

    #region 全屏设置

    private void mediaControls_FullWindow(object sender, RoutedEventArgs e)
    {
        if (appwindow.Presenter.Kind != AppWindowPresenterKind.FullScreen)
        {
            enterlFullScreen();
        }
        else
        {
            cancelFullScreen();
        }
    }

    /// <summary>
    /// 进入全屏
    /// </summary>
    private void enterlFullScreen()
    {
        if (appwindow.Presenter.Kind != AppWindowPresenterKind.FullScreen)
        {
            _markPresenterKindBeforeFullScreen = appwindow.Presenter.Kind;
            appwindow.SetPresenter(AppWindowPresenterKind.FullScreen);

            //监听ESC退出
            RootGrid.KeyDown += RootGrid_KeyDown;
        }
    }

    /// <summary>
    /// 退出全屏
    /// </summary>
    private void cancelFullScreen()
    {
        if (appwindow.Presenter.Kind == AppWindowPresenterKind.FullScreen)
        {
            appwindow.SetPresenter(Microsoft.UI.Windowing.AppWindowPresenterKind.Default);

            //取消监听
            RootGrid.KeyDown -= RootGrid_KeyDown;
        }
    }

    private void RootGrid_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Escape)
        {
            cancelFullScreen();
        }
    }
    #endregion
}
