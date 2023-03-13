// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Display.Views;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
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
    private string PickCode;
    private PlayType PlayType;
    private string TrueName;

    private Page lastPage;

    public MediaPlayWindow(string pickCode, PlayType playType, string truename, Page lastPage)
    {
        this.InitializeComponent();

        this.ExtendsContentIntoTitleBar = true;
        this.SetTitleBar(AppTitleBar);


        this.PickCode = pickCode;
        this.PlayType = playType;
        this.TrueName = truename;
        this.lastPage = lastPage;

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

        //上一页为详情页，生效喜欢或稍后观看的修改
        int newestIsLike = mediaControl.IsLike;
        int newestLookLater = mediaControl.LookLater;
        if (lastPage is DetailInfoPage detailInfoPage)
        {
            if (detailInfoPage.DetailInfo.is_like != newestIsLike)
                detailInfoPage.DetailInfo.is_like = newestIsLike;

            if (detailInfoPage.DetailInfo.look_later != newestLookLater)
                detailInfoPage.DetailInfo.look_later = newestLookLater;
        }
        else if(lastPage is VideoViewPage videoViewPage)
        {
            var storageItem = videoViewPage._storeditem;

            if (videoViewPage._storeditem != null && videoViewPage._storeditem.truename == TrueName)
            {
                if (storageItem.is_like != newestIsLike)
                    storageItem.is_like = newestIsLike;

                if (storageItem.look_later != newestLookLater)
                    storageItem.look_later = newestLookLater;
            }
        }else if(lastPage is ActorInfoPage actorInfoPage)
        {
            if (actorInfoPage._storeditem.is_like != newestIsLike)
                actorInfoPage._storeditem.is_like = newestIsLike;

            if (actorInfoPage._storeditem.look_later != newestLookLater)
                actorInfoPage._storeditem.look_later = newestLookLater;
        }
    }

    public static MediaPlayWindow CreateNewWindow(string pickCode, PlayType playType,string trueName,Page lastPage)
    {
        MediaPlayWindow newWindow = new(pickCode, playType, trueName,lastPage);
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

        appwindow.SetPresenter(Microsoft.UI.Windowing.AppWindowPresenterKind.Default);

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
}
