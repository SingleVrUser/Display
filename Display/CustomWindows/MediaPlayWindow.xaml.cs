// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Display.Helper.UI;
using Display.Models.Media;
using Display.Views.More.DatumList;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.ApplicationModel.DataTransfer;
using Display.Models.Dto.OneOneFive;
using static Display.Controls.UserController.CustomMediaPlayerElement;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.CustomWindows;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MediaPlayWindow
{
    private readonly AppWindow _appWindow;

    private readonly Page _lastPage;

    public MediaPlayWindow(IList<MediaPlayItem> playItems, Page lastPage)
    {
        _lastPage = lastPage;
        InitializeComponent();

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);

        _appWindow = App.GetAppWindow(this);

        MediaControl.InitLoad(playItems, this);

        if (playItems.Count > 1)
        {
            InitMoreGrid(playItems);
        }

        Closed += MediaPlayWindow_Closed;
    }

    private void InitMoreGrid(IList<MediaPlayItem> playItems)
    {
        MoreGrid.Visibility = Visibility.Visible;

        VideoListView.ItemsSource = playItems;
    }

    public void ChangedVideoListViewIndex(int index)
    {
        var maxIndex = VideoListView.Items.Count - 1;

        if (index > maxIndex)
        {
            index = maxIndex;
        }

        VideoListView.SelectedIndex = index;
    }

    /// <summary>
    /// 修改标题
    /// </summary>
    /// <param name="title"></param>
    public void ChangedWindowTitle(string title)
    {
        AppTitleTextBlock.Text = title;
    }

    /// <summary>
    /// 窗口关闭时应保存各信息状态（收藏，稍后观看）
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void MediaPlayWindow_Closed(object sender, WindowEventArgs args)
    {
        Debug.WriteLine("aTimer Stop");
        _aTimer?.Stop();

        if (MediaControl.IsLoaded && VideoPlayGrid.Children.Contains(MediaControl))
        {
            Debug.WriteLine("取消监听 PointerExited");
            MediaControl.PointerEntered -= MediaControl_OnPointerEntered;
            MediaControl.PointerExited -= MediaControl_OnPointerExited;

            MediaControl.DisposeMediaPlayer();
        }

        Debug.WriteLine("remove MediaControl");
        VideoPlayGrid.Children.Clear();
    }

    public static MediaPlayWindow CreateNewWindow(IList<MediaPlayItem> playItems, PlayType playType, Page lastPage)
    {
        MediaPlayWindow newWindow = new(playItems, lastPage);

        newWindow.Activate();

        return newWindow;
    }

    #region 全屏设置

    private void mediaControls_FullWindow(object sender, RoutedEventArgs e)
    {
        ChangedFullWindow();
    }

    /// <summary>
    /// 进入全屏
    /// </summary>
    private void EnterFullScreen()
    {
        if (_appWindow.Presenter.Kind == AppWindowPresenterKind.FullScreen) return;

        ExtendsContentIntoTitleBar = false;
        TitleBarRowDefinition.Height = new GridLength(0);

        _appWindow.SetPresenter(AppWindowPresenterKind.FullScreen);

        //监听ESC退出
        RootGrid.KeyDown += RootGrid_KeyDown;

        VisualStateManager.GoToState(MyUserControl, "FullWindow", true);

        VisualStateManager.GoToState(MediaControl.MediaTransportControls, "FullWindowState", true);

    }

    /// <summary>
    /// 退出全屏
    /// </summary>
    private void CancelFullScreen()
    {
        if (_appWindow.Presenter.Kind != AppWindowPresenterKind.FullScreen) return;

        this.ExtendsContentIntoTitleBar = true;
        TitleBarRowDefinition.Height = new GridLength(28);

        _appWindow.SetPresenter(AppWindowPresenterKind.Default);

        //取消监听
        RootGrid.KeyDown -= RootGrid_KeyDown;

        VisualStateManager.GoToState(MyUserControl, "NoFullWindow", true);

        VisualStateManager.GoToState(MediaControl.MediaTransportControls, "NoFullWindowState", true);
    }

    private void RootGrid_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Escape)
        {
            CancelFullScreen();
        }
    }
    #endregion

    private void ChangedFullWindow()
    {
        if (_appWindow.Presenter.Kind != AppWindowPresenterKind.FullScreen)
        {
            EnterFullScreen();
        }
        else
        {
            CancelFullScreen();
        }
    }

    private void mediaControl_MediaDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        ChangedFullWindow();
    }

    private System.Timers.Timer _aTimer;
    private void MediaControl_OnPointerEntered(object sender, PointerRoutedEventArgs e)
    {
        _aTimer?.Stop();

        // Create a timer with a one second interval.
        _aTimer = new System.Timers.Timer(500);
        _aTimer.Enabled = true;
        _aTimer.Elapsed += timer_Tick; ;
    }

    private void MediaControl_OnPointerExited(object sender, PointerRoutedEventArgs e)
    {
        _aTimer?.Stop();

        RootGrid.Cursor = null;
    }


    //鼠标状态计数器
    private int _iCount;

    private void timer_Tick(object sender, System.Timers.ElapsedEventArgs e)
    {
        //鼠标状态计数器>=0的情况下鼠标可见，<0不可见，并不是直接受api函数影响而改变
        var i = CursorHelper.GetIdleTick();

        //Debug.WriteLine($"限制时间(ms)：{i}");

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

    private void MediaControl_OnRightButtonClick(object sender, RoutedEventArgs e)
    {
        MoreGrid.Visibility = MoreGrid.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
    }


    private void MediaControl_OnDrop(object sender, DragEventArgs e)
    {
        Debug.WriteLine("拖动");

        if (e.DataView.Properties.Values.FirstOrDefault() is not List<FilesInfo> addInfos) return;

        var addMediaPlayItems = new List<MediaPlayItem>();
        addInfos.ForEach(info => { addMediaPlayItems.Add(new MediaPlayItem(info)); });

        var allMediaPlayItems = MediaControl.ReLoad(addMediaPlayItems);

        if (allMediaPlayItems.Count > 1)
        {
            InitMoreGrid(allMediaPlayItems);
        }
    }

    private void MediaControl_OnDragOver(object sender, DragEventArgs e)
    {
        e.AcceptedOperation = DataPackageOperation.Link;
        e.DragUIOverride.Caption = "播放";
    }

    private void MediaControl_OnDeleteFileClick(MediaPlayItem item)
    {
        if (_lastPage is FileListPage fileListPage && _lastPage.IsLoaded && item.Fid != null)
        {
            fileListPage.RemoveFileById((long)item.Fid);
        }
    }
}