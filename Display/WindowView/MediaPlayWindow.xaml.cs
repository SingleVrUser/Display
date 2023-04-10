// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Display.Data;
using Display.Helper;
using Display.Views;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System.Diagnostics;
using static Display.Controls.CustomMediaPlayerElement;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.WindowView;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MediaPlayWindow : Window
{
    private AppWindow appwindow;
    private readonly string _pickCode;
    private readonly PlayType _playType;
    private readonly string _trueName;
    private readonly SubInfo _subInfo;

    private Page lastPage;

    public MediaPlayWindow(string pickCode, PlayType playType, string trueName, Page lastPage, SubInfo subInfo)
    {
        this.InitializeComponent();

        this.ExtendsContentIntoTitleBar = true;
        this.SetTitleBar(AppTitleBar);

        this._pickCode = pickCode;
        this._playType = playType;
        this._trueName = trueName;
        this.lastPage = lastPage;
        this._subInfo = subInfo;

        appwindow = App.getAppWindow(this);

        this.Closed += MediaPlayWindow_Closed;
    }

    private void MediaPlayWindow_Closed(object sender, WindowEventArgs args)
    {
        if (mediaControl.IsLoaded && VideoPlayGrid.Children.Contains(mediaControl))
        {
            mediaControl.DisposeMediaPlayer();
            VideoPlayGrid.Children.Remove(mediaControl);
        }

        mediaControl.PointerExited -= MediaControl_OnPointerExited;
        aTimer?.Stop();

        //上一页为详情页，生效喜欢或稍后观看的修改
        var newestIsLike = mediaControl.IsLike;
        var newestLookLater = mediaControl.LookLater;
        switch (lastPage)
        {
            case DetailInfoPage detailInfoPage:
                {
                    if (detailInfoPage.DetailInfo.is_like != newestIsLike)
                        detailInfoPage.DetailInfo.is_like = newestIsLike;

                    if (detailInfoPage.DetailInfo.look_later != newestLookLater)
                        detailInfoPage.DetailInfo.look_later = newestLookLater;
                    break;
                }
            case VideoViewPage videoViewPage:
                {
                    var storageItem = videoViewPage._storeditem;

                    if (videoViewPage._storeditem != null && videoViewPage._storeditem.truename == _trueName)
                    {
                        if (storageItem.is_like != newestIsLike)
                            storageItem.is_like = newestIsLike;

                        if (storageItem.look_later != newestLookLater)
                            storageItem.look_later = newestLookLater;
                    }

                    break;
                }
            case ActorInfoPage actorInfoPage:
                {
                    if (actorInfoPage._storeditem.is_like != newestIsLike)
                        actorInfoPage._storeditem.is_like = newestIsLike;

                    if (actorInfoPage._storeditem.look_later != newestLookLater)
                        actorInfoPage._storeditem.look_later = newestLookLater;
                    break;
                }
        }
    }

    public static MediaPlayWindow CreateNewWindow(string pickCode, PlayType playType, string trueName, Page lastPage, SubInfo subInfo)
    {
        MediaPlayWindow newWindow = new(pickCode, playType, trueName, lastPage, subInfo);
        newWindow.Activate();

        return newWindow;
    }

    private Visibility isPickCodeNull()
    {
        return _pickCode == null ? Visibility.Visible : Visibility.Collapsed;
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
            this.ExtendsContentIntoTitleBar = false;
            TitleBarRowDefinition.Height = new GridLength(0);

            appwindow.SetPresenter(AppWindowPresenterKind.FullScreen);

            //监听ESC退出
            RootGrid.KeyDown += RootGrid_KeyDown;

            VisualStateManager.GoToState(mediaControl.mediaTransportControls, "FullWindowState", true);
        }
    }

    /// <summary>
    /// 退出全屏
    /// </summary>
    private void cancelFullScreen()
    {
        if (appwindow.Presenter.Kind != AppWindowPresenterKind.FullScreen) return;


        this.ExtendsContentIntoTitleBar = true;
        TitleBarRowDefinition.Height = new GridLength(28);

        appwindow.SetPresenter(AppWindowPresenterKind.Default);

        //取消监听
        RootGrid.KeyDown -= RootGrid_KeyDown;


        VisualStateManager.GoToState(mediaControl.mediaTransportControls, "NoFullWindowState", true);
    }

    private void RootGrid_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Escape)
        {
            cancelFullScreen();
        }
    }
    #endregion

    private void mediaControl_MediaDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
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

    private System.Timers.Timer aTimer;
    private void MediaControl_OnPointerEntered(object sender, PointerRoutedEventArgs e)
    {
        aTimer?.Stop();

        // Create a timer with a one second interval.
        aTimer = new System.Timers.Timer(500);
        aTimer.Enabled = true;
        aTimer.Elapsed += timer_Tick; ;
    }

    private void MediaControl_OnPointerExited(object sender, PointerRoutedEventArgs e)
    {
        aTimer?.Stop();

        RootGrid.Cursor = null;
    }


    //鼠标状态计数器
    private int _iCount = 0;

    private void timer_Tick(object sender, System.Timers.ElapsedEventArgs e)
    {
        //鼠标状态计数器>=0的情况下鼠标可见，<0不可见，并不是直接受api函数影响而改变
        var i = CursorHelper.GetIdleTick();

        Debug.WriteLine($"限制时间(ms)：{i}");

        if (i > 4000)
        {
            while (_iCount >= 0)
            {
                Debug.WriteLine($"隐藏鼠标 ({_iCount}）");
                _iCount = CursorHelper.ShowCursor(false);

                TryUpdateUi(false);
            }
        }
        else
        {
            while (_iCount < 0)
            {
                Debug.WriteLine($"显示鼠标 ({_iCount}）");
                _iCount = CursorHelper.ShowCursor(true);

                TryUpdateUi();
            }
        }
    }

    private void TryUpdateUi(bool isOpenSplitPane = true)
    {
        var cursor = isOpenSplitPane ? null : CursorHelper.GetHiddenCursor();

        if (DispatcherQueue.HasThreadAccess)
        {
            RootGrid.Cursor = cursor;
        }
        else
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                RootGrid.Cursor = cursor;
            });
        }
    }
}
