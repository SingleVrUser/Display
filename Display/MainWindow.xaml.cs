
using Display.Data;
using Display.Helper;
using Display.Views;
using Display.WindowView;
using Microsoft.Extensions.Configuration;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mime;
using Windows.System;
using Display.ContentsPage;
using Display.ContentsPage.SearchLink;
using WinUIEx;
using Display.Models;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow
    {
        private AppWindow _appwindow;

        private bool _isPanalOpen;

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
            _appwindow = App.getAppWindow(this);

            _appwindow.SetIcon(Path.Combine(Windows.ApplicationModel.Package.Current.InstalledLocation.Path, "Assets/pokeball.ico"));

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
                var titleBar = _appwindow.TitleBar;
                titleBar.ExtendsContentIntoTitleBar = true;
                titleBar.PreferredHeightOption = TitleBarHeightOption.Tall;
                titleBar.ButtonBackgroundColor = Colors.Transparent;
                titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
                AppTitleBar.Loaded += AppTitleBar_Loaded;
                AppTitleBar.SizeChanged += AppTitleBar_SizeChanged;
                CustomAutoSuggestBox.OpenAutoSuggestionBoxCompleted += OpenAutoSuggestionBoxCompleted; ;
                CustomAutoSuggestBox.CloseAutoSuggestionBoxCompleted += CloseAutoSuggestionBoxCompleted;
            }
            else
            {
                NavView.AlwaysShowHeader = true;
                NavView.HeaderTemplate = RootGrid.Resources["HeaderTemplate"] as DataTemplate; ;
                ExtendsContentIntoTitleBar = true;
                CustomAutoSuggestBox.Visibility = Visibility.Collapsed;
                SetTitleBar(AppTitleBar);
            }

            this.Activated += Window_Activated;

            //Mica
            Backdrop = new MicaSystemBackdrop();
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
            SetDragRegionForCustomTitleBar(_appwindow);

        }

        private void AppTitleBar_Loaded(object sender, RoutedEventArgs e)
        {
            // 设置拖拽区域
            SetDragRegionForCustomTitleBar(_appwindow);
        }
        private void CloseAutoSuggestionBoxCompleted(object sender, object e)
        {
            SetDragRegionForCustomTitleBar(_appwindow);
        }

        private void OpenAutoSuggestionBoxCompleted(object sender, object e)
        {
            SetDragRegionForCustomTitleBar(_appwindow);
        }

        /// <summary>
        /// 设置标题栏的拖拽范围
        /// </summary>
        /// <param name="appWindow"></param>
        private void SetDragRegionForCustomTitleBar(AppWindow appWindow)
        {
            // 多显示器，scaleAdjustment可能不同
            double scaleAdjustment = WindowHelper.GetScaleAdjustment(this);

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
        /// 定义访问的页面
        /// </summary>
        private readonly List<(string Tag, Type Page)> _pages = new()
        {
            ("home",typeof(HomePage)),
            ("videoView",typeof (VideoViewPage)),
            ("actorsView",typeof(ActorsPage)),
            ("setting",typeof(SettingsPage)),
            ("more",typeof(MorePage)),
        };

        /// <summary>
        /// NavView加载
        /// </summary>
        /// <param Name="sender"></param>
        /// <param Name="e"></param>
        private void NavView_Loaded(object sender, RoutedEventArgs e)
        {
            //数据文件存在
            if (File.Exists(DataAccess.dbpath))
            {
                switch (AppSettings.StartPageIndex)
                {
                    case 0:
                        NavView.SelectedItem = NavView.MenuItems[0];
                        break;
                    case 1:
                        NavView.SelectedItem = NavView.MenuItems[2];
                        break;
                    case 2:
                        NavView.SelectedItem = NavView.MenuItems[3];
                        break;
                    case 3:
                        NavView.SelectedItem = NavView.MenuItems[4];
                        break;
                    case 4:
                        NavView.SelectedItem = NavView.SettingsItem;
                        break;
                }
            }

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

            var releaseCheck = await AppInfo.GetLatestReleaseCheck();

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
                Content = new ContentsPage.UpdateAppPage(releaseCheck),
                XamlRoot = ((Page)ContentFrame.Content).XamlRoot
            };

            var result = await dialog.ShowAsync();

            switch (result)
            {
                //下载
                case ContentDialogResult.Primary:
                    var installUrl = AppInfo.IsWindows11() ? $"ms-appinstaller:?source={releaseCheck.AppAsset.browser_download_url}" : $"{releaseCheck.AppAsset.browser_download_url}";

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
        /// <param Name="sender"></param>
        /// <param Name="args"></param>
        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
            {
                NavView.Header = "设置";
                NavView.SelectedItem = (NavigationViewItem)NavView.SettingsItem;
                NavView_Navigate("settings", args.RecommendedNavigationTransitionInfo);
            }
            else
            {
                NavView.Header = ((NavigationViewItem)NavView.SelectedItem)?.Content?.ToString();
                NavView.SelectedItem = args.SelectedItemContainer;
                var navItemTag = args.SelectedItemContainer.Tag.ToString();
                NavView_Navigate(navItemTag, args.RecommendedNavigationTransitionInfo);
            }

        }

        /// <summary>
        /// 页面跳转
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        private void NavView_Navigate(string navItemTag, NavigationTransitionInfo transitionInfo)
        {
            Type _page = null;
            if (navItemTag == "settings")
            {
                _page = typeof(SettingsPage);
            }
            else
            {
                var item = _pages.FirstOrDefault(p => p.Tag == navItemTag);
                _page = item.Page;
            }
            var preNavPageType = ContentFrame.CurrentSourcePageType;

            //当前页面不跳转
            if (!(_page is null) && !Type.Equals(preNavPageType, _page))
            {
                ContentFrame.Navigate(_page, null, transitionInfo);
            }
        }

        /// <summary>
        /// 跳转页面失败
        /// </summary>
        /// <param Name="sender"></param>
        /// <param Name="e"></param>
        private void ContentFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        private void NavigationViewControl_DisplayModeChanged(NavigationView sender, NavigationViewDisplayModeChangedEventArgs args)
        {
            AppTitleBar.Margin = new Thickness()
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
        /// <param Name="sender"></param>
        /// <param Name="args"></param>
        private void NavView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            TryGoBack();
        }

        private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
        {
            // 可以返回就激活返回键
            NavView.IsBackEnabled = ContentFrame.CanGoBack;
        }
        private void GoBack_KeyboardAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            TryGoBack();
        }

        private void GoForward_KeyboardAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            TryGoForward();
        }

        /// <summary>
        /// 搜索框中的选项被选中
        /// </summary>
        /// <param Name="sender"></param>
        /// <param Name="args"></param>
        private async void SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            if (args.SelectedItem is string content && content.Contains("未找到"))
            {
                // 搜索资源
                //输入框的内容
                var searchContent = sender.Text;
                if (string.IsNullOrEmpty(searchContent)) return;

                var contentPage = new SearchLinkPage(searchContent);

                ContentDialog dialog = new()
                {
                    XamlRoot = RootGrid.XamlRoot,
                    Title = "搜索结果",
                    PrimaryButtonText = "下载选中",
                    CloseButtonText = "返回",
                    DefaultButton = ContentDialogButton.Primary,
                    Content = contentPage
                };

                var result = await dialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    if (string.IsNullOrEmpty(AppSettings.SavePath115Cid) ||
                        string.IsNullOrEmpty(AppSettings.SavePath115Name))
                    {
                        ShowTeachingTip("未设置保存路径");
                    }
                    else
                    {
                        var isDownSuccess = await contentPage.OfflineDownSelectedLink();

                        ShowTeachingTip(isDownSuccess ? "已下载到网盘中" : "下载失败");
                    }
                }

                return;
            }
            
            if (args.SelectedItem is not VideoInfo nowItem) return;

            //选中的是失败项
            if (nowItem.series == "fail")
            {

                var mediaPlayItem = new MediaPlayItem(nowItem.busurl, nowItem.truename,FilesInfo.FileType.File);
                await PlayVideoHelper.PlayVideo(new List<MediaPlayItem>() { mediaPlayItem }, ((Page)ContentFrame.Content).XamlRoot);
            }
            //正常点击
            else
            {
                //加载应用记录的图片默认大小
                var imageSize = AppSettings.ImageSize;
                var newItem = new VideoCoverDisplayClass(nowItem, imageSize.Item1, imageSize.Item2);
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

            ContentFrame.Navigate(typeof(ActorInfoPage), new Tuple<List<string>, string, bool>(types, sender.Text, true), new SuppressNavigationTransitionInfo());
        }

        /// <summary>
        /// 点击了全屏或退出全屏按键
        /// </summary>
        /// <param Name="sender"></param>
        /// <param Name="e"></param>
        private void fullScreenWindowButton_Click(object sender, RoutedEventArgs e)
        {
            if (_appwindow.Presenter.Kind != AppWindowPresenterKind.FullScreen)
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
            if (_appwindow.Presenter.Kind == AppWindowPresenterKind.FullScreen) return;

            _isPanalOpen = NavView.IsPaneOpen;
            _appwindow.SetPresenter(AppWindowPresenterKind.FullScreen);
            NavView.PaneDisplayMode = NavigationViewPaneDisplayMode.LeftMinimal;

            //监听ESC退出
            RootGrid.KeyDown += RootGrid_KeyDown;
        }

        /// <summary>
        /// 退出全屏
        /// </summary>
        private void CancelFullScreen()
        {
            if (_appwindow.Presenter.Kind != AppWindowPresenterKind.FullScreen) return;

            NavView.PaneDisplayMode = NavigationViewPaneDisplayMode.Auto;
            NavView.IsPaneOpen = _isPanalOpen;
            _appwindow.SetPresenter(AppWindowPresenterKind.Default);

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

        private async void CloudDownButtonClick(object sender, TappedRoutedEventArgs e)
        {
            CreateCloudDownContentDialog();
        }

        private async void CreateCloudDownContentDialog(string defaultLink = "")
        {
            var downPage = new ContentsPage.OfflineDown.OfflineDownPage(defaultLink);

            var contentDialog = new ContentDialog
            {
                XamlRoot = this.RootGrid.XamlRoot,
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

            downPage.FailLoaded += DownPage_FailLoaded; ;

            downPage.RequestCompleted += DownPage_RequestCompleted; ;

            var result = await contentDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                downPage.CreateOfflineDownRequest();
            }
        }

        private async void DownPage_RequestCompleted(object sender, ContentsPage.OfflineDown.RequestCompletedEventArgs e)
        {
            // 查看是否有失败项
            var info = e?.Info;
            if (info is not { state: true })
            {
                ShowTeachingTip("出现未知错误");
                return;
            }

            //单链接或多链接
            var failList = !string.IsNullOrEmpty(info.url) ? new List<AddTaskUrlInfo> { info } : info.result?.Where(x => !string.IsNullOrEmpty(x.error_msg)).ToList();

            if (failList == null || failList.Count == 0)
            {
                ShowTeachingTip("添加任务成功");
                return;
            }

            var failPage = new ContentsPage.OfflineDown.FailListPage(failList);
            var contentDialog = new ContentDialog
            {
                XamlRoot = RootGrid.XamlRoot,
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
            var defaultLink = string.Join("\n", failList.Select(x => x.url));
            CreateCloudDownContentDialog(defaultLink);
        }

        private void DownPage_FailLoaded(object sender, ContentsPage.OfflineDown.FailLoadedEventArgs e)
        {
            ShowTeachingTip(e.Message);
        }

        private void ShowTeachingTip(string subtitle, string content = null)
        {
            BasePage.ShowTeachingTip(LightDismissTeachingTip, subtitle, content);
        }

        private async void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            TextBox textBox = new();
            ContentDialog dialog = new()
            {
                XamlRoot = this.RootGrid.XamlRoot,
                PrimaryButtonText = "上传",
                CloseButtonText = "返回",
                Title = "输入需要上传的文件路径",
                Content = textBox,
                DefaultButton = ContentDialogButton.Primary
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                var filePath = textBox.Text;

                if (!File.Exists(filePath)) return;

                //var upload115 = Upload115.SingleUpload115;

                //var uploadInfo = await WebApi.GlobalWebApi.GetUploadInfo();

                //if(uploadInfo == null) return;

                //await upload115.UploadTo115(filePath, "0", uploadInfo.user_id.ToString(), uploadInfo.userkey);

            }
        }
    }
}
