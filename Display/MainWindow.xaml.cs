using Display.Constants;
using Display.Controls.UserController;
using Display.Helper.FileProperties.Name;
using Display.Helper.Network;
using Display.Helper.UI;
using Display.Providers;
using Display.ViewModels;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.System;
using Display.Models.Dto.Media;
using Display.Models.Dto.OneOneFive;
using Display.Models.Dto.Settings;
using Display.Models.Entities.OneOneFive;
using Display.Models.Enums;
using Display.Models.Vo;
using Display.Views.Pages;
using Display.Views.Pages.More.DatumList;
using Display.Views.Pages.OfflineDown;
using Display.Views.Pages.SearchLink;
using Display.Views.Pages.Settings;
using Display.Views.Pages.Settings.Account;
using Display.Views.Windows;
using MainPage = Display.Views.Pages.Tasks.MainPage;


namespace Display;

public sealed partial class MainWindow
{
    private readonly MainWindowViewModel _viewModel = App.GetService<MainWindowViewModel>();

    private AppWindow _appWindow;

    private bool _isPanelOpen;

    public MainWindow()
    {
        InitializeComponent();

        SetStyle();
    }

    /// <summary>
    /// 调整样式
    /// </summary>
    private void SetStyle()
    {
        _appWindow = App.GetAppWindow(this);

        _appWindow.SetIcon(Path.Combine(Windows.ApplicationModel.Package.Current.InstalledLocation.Path, "Assets/pokeball.ico"));

        Title = "Display";

        AppTitleBar.Height = NavView.CompactPaneLength - 2;

        NavView.Resources["NavigationViewContentMargin"] = new Thickness()
        {
            Left = 0,
            Top = NavView.CompactPaneLength + 2,
            Right = 0,
            Bottom = 0
        };

        // 支持自定义标题栏
        if (AppWindowTitleBar.IsCustomizationSupported())
        {
            NavView.AlwaysShowHeader = false;
            var titleBar = _appWindow.TitleBar;
            titleBar.ExtendsContentIntoTitleBar = true;
            titleBar.PreferredHeightOption = TitleBarHeightOption.Tall;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            AppTitleBar.Loaded += AppTitleBar_Loaded;
            AppTitleBar.SizeChanged += AppTitleBar_SizeChanged;
            CustomAutoSuggestBox.OpenAutoSuggestionBoxCompleted += OpenAutoSuggestionBoxCompleted;
            CustomAutoSuggestBox.CloseAutoSuggestionBoxCompleted += CloseAutoSuggestionBoxCompleted;
        }
        else
        {
            NavView.AlwaysShowHeader = true;
            NavView.HeaderTemplate = RootGrid.Resources["HeaderTemplate"] as DataTemplate;
            ExtendsContentIntoTitleBar = true;
            CustomAutoSuggestBox.Visibility = Visibility.Collapsed;
            SetTitleBar(AppTitleBar);
        }

        this.Activated += Window_Activated;

        //Mica
        //Backdrop = new MicaSystemBackdrop();

        this.SystemBackdrop = new MicaBackdrop();
    }
    private void Window_Activated(object sender, WindowActivatedEventArgs args)
    {
        AppTitleBar.Opacity = args.WindowActivationState switch
        {
            WindowActivationState.Deactivated => 0.5,
            _ => 1
        };
    }

    private void AppTitleBar_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        SetDragRegionForCustomTitleBar(_appWindow);
    }

    private void AppTitleBar_Loaded(object sender, RoutedEventArgs e)
    {
        // 设置拖拽区域
        SetDragRegionForCustomTitleBar(_appWindow);
    }
    private void CloseAutoSuggestionBoxCompleted(object sender, object e)
    {
        SetDragRegionForCustomTitleBar(_appWindow);
    }

    private void OpenAutoSuggestionBoxCompleted(object sender, object e)
    {
        SetDragRegionForCustomTitleBar(_appWindow);
    }

    /// <summary>
    /// 设置标题栏的拖拽范围
    /// </summary>
    /// <param name="appWindow"></param>
    private void SetDragRegionForCustomTitleBar(AppWindow appWindow)
    {
        // 多显示器，scaleAdjustment可能不同
        var scaleAdjustment = WindowHelper.GetScaleAdjustment(this);

        // 拖拽区域数组
        List<Windows.Graphics.RectInt32> dragRectsList = new();

        Windows.Graphics.RectInt32 dragRectL;
        dragRectL.X = (int)((NavView.CompactPaneLength + IconColumn.ActualWidth) * scaleAdjustment);
        dragRectL.Y = 0;
        dragRectL.Height = (int)(AppTitleBar.ActualHeight * scaleAdjustment);
        dragRectL.Width = (int)((TitleColumn.ActualWidth) * scaleAdjustment);
        dragRectsList.Add(dragRectL);

        Windows.Graphics.RectInt32 dragRectR;
        dragRectR.X = (int)((IconColumn.ActualWidth
                             + TitleColumn.ActualWidth
                             + SearchColumn.ActualWidth) * scaleAdjustment);
        dragRectR.Y = 0;
        dragRectR.Height = (int)(AppTitleBar.ActualHeight * scaleAdjustment);
        dragRectR.Width = (int)(RightDragColumn.ActualWidth * scaleAdjustment);
        dragRectsList.Add(dragRectR);

        Windows.Graphics.RectInt32[] dragRects = dragRectsList.ToArray();

        appWindow.TitleBar.SetDragRectangles(dragRects);

    }

    /// <summary>
    /// NavView加载
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void NavView_Loaded(object sender, RoutedEventArgs e)
    {
        var selectItem = _viewModel.NavigationItemViewModel.GetMenuItem(AppSettings.StartPageEnum, NavView.SettingsItem);
        if (selectItem != null) NavView.SelectedItem = selectItem;

        //在这里检查应用更新
        TryCheckAppUpdate();
    }

    /// <summary>
    /// 检查应用更新
    /// </summary>
    private async void TryCheckAppUpdate()
    {
        if (!AppSettings.IsCheckUpdate)
            return;

        var releaseCheck = await AppUpdateHelper.GetLatestReleaseCheck();

        if (releaseCheck == null) return;

        //可以升级且最新版本不是忽略的版本
        if (!releaseCheck.CanUpdate || releaseCheck.LatestVersion == AppSettings.IgnoreUpdateAppVersion) return;

        var dialog = new ContentDialog
        {
            // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
            Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
            Title = "有新版本可更新",
            PrimaryButtonText = "更新",
            SecondaryButtonText = "忽略该版本",
            CloseButtonText = "取消",
            DefaultButton = ContentDialogButton.Primary,
            Content = new UpdateAppPage(releaseCheck),
            XamlRoot = ((Page)ContentFrame.Content).XamlRoot
        };

        var result = await dialog.ShowAsync();

        switch (result)
        {
            //下载
            case ContentDialogResult.Primary:
                var installUrl = AppUpdateHelper.IsWindows11() ? $"ms-appinstaller:?source={releaseCheck.AppAsset.BrowserDownloadUrl}" : $"{releaseCheck.AppAsset.BrowserDownloadUrl}";

                await Launcher.LaunchUriAsync(new Uri(installUrl));
                break;
            //忽略该版本
            case ContentDialogResult.Secondary:
                AppSettings.IgnoreUpdateAppVersion = releaseCheck.LatestVersion;
                break;
            case ContentDialogResult.None:
            default:
                return;
        }
    }

    /// <summary>
    /// NavigationView的选择改变
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem == null) return;

        if (_navigating) return;

        if (args.IsSettingsSelected)
        {
            NavView_Navigate(NavigationViewItemEnum.SettingPage, args.RecommendedNavigationTransitionInfo);
        }
        else if (args.SelectedItem is MenuItem item)
        {
            NavView_Navigate(item.PageEnum, args.RecommendedNavigationTransitionInfo);
        }
    }

    /// <summary>
    /// 页面跳转
    /// </summary>
    /// <param name="pageEnum"></param>
    /// <param name="transitionInfo"></param>
    private void NavView_Navigate(NavigationViewItemEnum pageEnum, NavigationTransitionInfo transitionInfo)
    {
        if (!PageTypeAndEnum.PageTypeAndEnumDict.TryGetValue(pageEnum, out var pageType)) return;

        ContentFrame.Navigate(pageType, null, transitionInfo);

    }
    private bool _navigating;
    private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
    {
        _navigating = true;
        // 可以返回就激活返回键
        NavView.IsBackEnabled = ContentFrame.CanGoBack;

        _navigating = false;
    }

    /// <summary>
    /// 跳转页面失败
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ContentFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
    {
        throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
    }

    private void NavigationViewControl_DisplayModeChanged(NavigationView sender, NavigationViewDisplayModeChangedEventArgs args)
    {
        AppTitleBar.Margin = new Thickness
        {
            Left = sender.CompactPaneLength * (sender.DisplayMode == NavigationViewDisplayMode.Minimal ? 2 : 1),
            Top = AppTitleBar.Margin.Top,
            Right = AppTitleBar.Margin.Right,
            Bottom = AppTitleBar.Margin.Bottom
        };
    }

    private void TryGoBack()
    {
        if (ContentFrame.CanGoBack)
        {
            ContentFrame.GoBack();
        }
    }

    private void TryGoForward()
    {
        if (ContentFrame.CanGoForward)
        {
            ContentFrame.GoForward();
        }
    }

    /// <summary>
    /// 请求返回
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void NavView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
    {
        TryGoBack();
    }

    private void GoBack_KeyboardAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        TryGoBack();
    }

    private void GoForward_KeyboardAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        TryGoForward();
    }

    private async void CustomAutoSuggestBox_OnSuggestionItemTapped(object sender, string searchContent)
    {
        if (sender is not Grid grid) return;

        if (grid.DataContext is string content && content.Contains("点击搜索资源"))
        {
            //输入框的内容
            if (string.IsNullOrEmpty(searchContent)) return;

            var tupleResult = await SearchLinkPage.ShowInContentDialog(searchContent, RootGrid.XamlRoot);

            // 用户取消操作
            if (tupleResult == null) return;

            var (isSucceed, msg) = tupleResult;

            if (isSucceed)
            {
                ShowTeachingTip(msg, "打开所在目录", (_, _) =>
                {
                    // 打开所在目录
                    CommonWindow.CreateAndShowWindow(new FileListPage(AppSettings.SavePath115Cid));
                });
            }
            else
            {
                ShowTeachingTip(msg);
            }

            return;
        }

        if (grid.DataContext is not VideoInfo nowItem) return;

        //选中的是失败项
        if (nowItem is FailVideoInfo failVideoInfo)
        {
            var mediaPlayItem = new MediaPlayItem(failVideoInfo);
            await PlayVideoHelper.PlayVideo([mediaPlayItem], ((Page)ContentFrame.Content).XamlRoot);
        }
        //正常点击
        else
        {
            //加载应用记录的图片默认大小
            var newItem = new VideoCoverDisplayClass(nowItem, AppSettings.ImageWidth, AppSettings.ImageHeight);
            ContentFrame.Navigate(typeof(DetailInfoPage), newItem, new SuppressNavigationTransitionInfo());
        }

    }

    /// <summary>
    /// 提交搜索选项
    /// </summary>
    /// <param Name="sender"></param>
    /// <param Name="args"></param>
    private void CustomAutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        var types = sender.DataContext as List<string>;

        ContentFrame.Navigate(typeof(VideoCoverPage), new Tuple<List<string>, string, bool>(types, sender.Text, true), new SuppressNavigationTransitionInfo());
    }

    /// <summary>
    /// 点击了全屏或退出全屏按键
    /// </summary>
    /// <param Name="sender"></param>
    /// <param Name="e"></param>
    private void fullScreenWindowButton_Click(object sender, RoutedEventArgs e)
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

    /// <summary>
    /// 监听ESC按键（退出全屏）
    /// </summary>
    /// <param Name="sender"></param>
    /// <param Name="e"></param>
    private void RootGrid_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Escape)
        {
            CancelFullScreen();
        }
    }

    /// <summary>
    /// 进入全屏
    /// </summary>
    private void EnterFullScreen()
    {
        if (_appWindow.Presenter.Kind == AppWindowPresenterKind.FullScreen) return;

        _isPanelOpen = NavView.IsPaneOpen;
        _appWindow.SetPresenter(AppWindowPresenterKind.FullScreen);
        NavView.PaneDisplayMode = NavigationViewPaneDisplayMode.LeftMinimal;

        //监听ESC退出
        RootGrid.KeyDown += RootGrid_KeyDown;
    }

    /// <summary>
    /// 退出全屏
    /// </summary>
    private void CancelFullScreen()
    {
        if (_appWindow.Presenter.Kind != AppWindowPresenterKind.FullScreen) return;

        NavView.PaneDisplayMode = NavigationViewPaneDisplayMode.Auto;
        NavView.IsPaneOpen = _isPanelOpen;
        _appWindow.SetPresenter(AppWindowPresenterKind.Default);

        //取消监听
        RootGrid.KeyDown -= RootGrid_KeyDown;
    }

    private void FullScreenButton_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        ((HyperlinkButton)sender).Opacity = 1;
    }

    private void FullScreenButton_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        ((HyperlinkButton)sender).Opacity = 0.2;
    }

    private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
    {
        FileMatch.LaunchFolder(AppSettings.DataAccessSavePath);
    }

    private async void CreateCloudDownContentDialog(string defaultLink = "")
    {
        var downPage = new OfflineDownPage(defaultLink);

        var contentDialog = new ContentDialog
        {
            XamlRoot = RootGrid.XamlRoot,
            Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
            Title = "添加链接任务",
            PrimaryButtonText = "开始下载",
            CloseButtonText = "取消",
            DefaultButton = ContentDialogButton.Primary,
            Content = downPage,
            Resources =
            {
                // 使用更大的 MaxWidth
                ["ContentDialogMaxWidth"] = 2000
            }
        };

        downPage.FailLoaded += DownPage_FailLoaded;

        downPage.RequestCompleted += DownPage_RequestCompleted;

        var result = await contentDialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            downPage.CreateOfflineDownRequest();
        }
    }

    private async void DownPage_RequestCompleted(object sender, RequestCompletedEventArgs e)
    {
        var info = e?.Info;

        // 未知错误
        if (info is null)
        {
            ShowTeachingTip("出现未知错误");
            return;
        }

        // 成功
        if (info.State)
        {
            ShowTeachingTip("添加任务成功");
            return;
        }

        // 需要验证账户
        if (info.ErrCode == Account.AccountAnomalyCode)
        {
            var window = WebApi.CreateWindowToVerifyAccount();

            if (window.Content is not VerifyAccountPage page) return;

            page.VerifyAccountCompleted += (_, _) =>
            {
                // 重新显示输入窗口
                CreateCloudDownContentDialog();
            };

            window.Activate();

            return;
        }

        var failList = !string.IsNullOrEmpty(info.ErrorMsg)
            ? [info] // 单链接
            : info.Result?.Where(x => !string.IsNullOrEmpty(x.ErrorMsg)).ToList();  // 多链接

        if (failList == null || failList.Count == 0)
        {
            ShowTeachingTip("添加任务成功");
            return;
        }

        var failPage = new FailListPage(failList);
        var contentDialog = new ContentDialog
        {
            XamlRoot = RootGrid.XamlRoot,
            Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
            Title = "下载任务失败列表",
            PrimaryButtonText = "重试",
            CloseButtonText = "返回",
            DefaultButton = ContentDialogButton.Close,
            Content = failPage,
            Resources =
            {
                // 使用更大的 MaxWidth
                ["ContentDialogMaxWidth"] = 2000
            }
        };

        var result = await contentDialog.ShowAsync();
        if (result != ContentDialogResult.Primary) return;

        // 跳转失败的重试
        var defaultLink = string.Join("\n", failList.Select(x => x.Url));
        CreateCloudDownContentDialog(defaultLink);
    }

    private void DownPage_FailLoaded(object sender, FailLoadedEventArgs e)
    {
        ShowTeachingTip(e.Message);
    }

    private void ShowTeachingTip(string subtitle, string actionContent = null)
    {
        BasePage.ShowTeachingTip(LightDismissTeachingTip, subtitle, actionContent);
    }

    private void ShowTeachingTip(string subtitle,
        string actionContent, TypedEventHandler<TeachingTip, object> actionButtonClick)
    {
        BasePage.ShowTeachingTip(LightDismissTeachingTip, subtitle, actionContent, actionButtonClick);
    }

    private void CtrlF_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        if (args.Element is not CustomAutoSuggestBox suggestionBox) return;

        suggestionBox.Focus(FocusState.Programmatic);
    }

    private void UIElement_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        if (sender is not NavigationViewItem item) return;

        if (item.Tag is not NavigationViewItemEnum pageEnum) return;

        switch (pageEnum)
        {
            case NavigationViewItemEnum.DownPage:
                CreateCloudDownContentDialog();
                break;
            case NavigationViewItemEnum.TaskPage:
                DispatcherQueue.TryEnqueue(() =>
                    MainPage.ShowSingleWindow());
                break;
        }
    }


}