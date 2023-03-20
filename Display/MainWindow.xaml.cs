
using Display.Controls;
using Display.Data;
using Display.Helper;
using Display.Views;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.System;

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

            this.Title = "Display";

            AppTitleBar.Height = NavView.CompactPaneLength -2;

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
                AppTitleBar.Loaded += AppTitleBar_Loaded;
                AppTitleBar.SizeChanged += AppTitleBar_SizeChanged;
                CustomAutoSuggestBox.OpenAutoSuggestionBoxCompleted += OpenAutoSuggestionBoxCompleted; ;
                CustomAutoSuggestBox.CloseAutoSuggestionBoxCompleted += CloseAutoSuggestionBoxCompleted;
            }
            else
            {
                NavView.AlwaysShowHeader = true;
                NavView.HeaderTemplate = RootGrid.Resources["HeaderTemplate"] as DataTemplate;;
                ExtendsContentIntoTitleBar = true;
                CustomAutoSuggestBox.Visibility = Visibility.Collapsed;
                SetTitleBar(AppTitleBar);
                
            }
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
        private readonly List<(string Tag, Type Page)> _pages = new List<(string Tag, Type Page)>
        {
            ("home",typeof(HomePage)),
            ("videoView",typeof (VideoViewPage)),
            ("actorsview",typeof(ActorsPage)),
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

            var ReleaseCheck = await AppInfo.GetLatestReleaseCheck();

            if (ReleaseCheck == null) return;

            //可以升级且最新版本不是忽略的版本
            if (!ReleaseCheck.CanUpdate || ReleaseCheck.LatestVersion == AppSettings.IgnoreUpdateAppVersion) return;

            var dialog = new ContentDialog
            {
                // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                Title = "有新版本可升级",
                PrimaryButtonText = "下载",
                SecondaryButtonText = "忽略该版本",
                CloseButtonText = "取消",
                DefaultButton = ContentDialogButton.Primary,
                Content = new ContentsPage.UpdateAppPage(ReleaseCheck),
                XamlRoot = ((Page)ContentFrame.Content).XamlRoot
            };

            var result = await dialog.ShowAsync();

            switch (result)
            {
                //下载
                case ContentDialogResult.Primary:
                    await Launcher.LaunchUriAsync(new Uri(ReleaseCheck.AppAsset.browser_download_url));
                    break;
                //忽略该版本
                case ContentDialogResult.Secondary:
                    AppSettings.IgnoreUpdateAppVersion = ReleaseCheck.LatestVersion;
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
            if (args.IsSettingsSelected == true)
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
                var itme = _pages.FirstOrDefault(p => p.Tag == navItemTag);
                _page = itme.Page;
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
            var nowItem = args.SelectedItem as VideoInfo;

            //点击了某个选项
            if (nowItem != null)
            {
                //选中的是失败项
                if (nowItem.series == "fail")
                {
                    await PlayVideoHelper.PlayVideo(nowItem.busurl, ((Page)ContentFrame.Content).XamlRoot);
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
            FileMatch.LaunchFolder(AppSettings.DataAccess_SavePath);
        }

    }
}
