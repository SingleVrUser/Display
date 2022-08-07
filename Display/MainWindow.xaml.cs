using Data;
using Display.Views;
using Microsoft.UI.Input;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.System;
using Windows.UI.Core;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private AppWindow appwindow;

        //全屏前记录当前状态
        private AppWindowPresenterKind _markPresenterKindBeforeFullScreen;

        public MainWindow()
        {
            InitializeComponent();

            SetStyle();

            appwindow = App.getAppWindow(this);
        }

        /// <summary>
        /// 调整样式
        /// </summary>
        private void SetStyle()
        {
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);

            AppTitleBar.Height = NavView.CompactPaneLength - 5;

            NavView.Resources["NavigationViewContentMargin"] = new Thickness()
            {
                Left = 0,
                Top = NavView.CompactPaneLength + 2,
                Right = 0,
                Bottom = 0
            };
        }

        /// <summary>
        /// 定义访问的页面
        /// </summary>
        private readonly List<(string Tag, Type Page)> _pages = new List<(string Tag, Type Page)>
        {
            ("home",typeof(HomePage)),
            ("browser",typeof(BrowserPage)),
            ("videoView",typeof (VideoViewPage)),
            ("actorsview",typeof(ActorsPage)),
            ("setting",typeof(SettingsPage)),
            ("more",typeof(MorePage)),
        };

        /// <summary>
        /// NavView加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NavView_Loaded(object sender, RoutedEventArgs e)
        {
            // Add handler for ContentFrame navigation.
            //ContentFrame.Navigated += On_Navigated;

            // NavView doesn't load any page by default, so load home page.
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
                

        }

        //private void CoreDispatcher_AcceleratorKeyActivated(CoreDispatcher sender, AcceleratorKeyEventArgs args)
        //{
        //    throw new NotImplementedException();
        //}

        /// <summary>
        /// ContentFrame跳转的Handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        //private void On_Navigated(object sender, NavigationEventArgs e)
        //{
        //    if (ContentFrame.SourcePageType == typeof(SettingsPage))
        //    {
        //        NavView.Header = "设置";
        //        NavView.SelectedItem = (NavigationViewItem)NavView.SettingsItem;
        //    }
        //    else if (ContentFrame.SourcePageType != null)
        //    {
        //        var item = _pages.FirstOrDefault(p => p.Page == e.SourcePageType);

        //        // MenuItems or FooterMenuItems

        //        var items = NavView.MenuItems
        //            .OfType<NavigationViewItem>()
        //            .FirstOrDefault(n => n.Tag.Equals(item.Tag));

        //        if (items == null && NavView.FooterMenuItems != null)
        //        {
        //            items = NavView.FooterMenuItems.
        //                OfType<NavigationViewItem>()
        //                .FirstOrDefault(n => n.Tag.Equals(item.Tag));
        //        }

        //        NavView.Header = ((NavigationViewItem)NavView.SelectedItem)?.Content?.ToString();
        //        NavView.SelectedItem = items;

        //    }
        //}

        /// <summary>
        /// NavigationView的选择改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
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
            if (!(_page is null) && !Type.Equals(preNavPageType, _page))
            {
                ContentFrame.Navigate(_page, null, transitionInfo);
            }
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
        /// <param name="sender"></param>
        /// <param name="args"></param>
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
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            var nowItem = args.SelectedItem as VideoInfo;

            //点击了 “未找到”
            if (nowItem != null)
            {
                var newItem = new VideoCoverDisplayClass(nowItem);
                ContentFrame.Navigate(typeof(DetailInfoPage), newItem, new SuppressNavigationTransitionInfo());
            }

        }

        /// <summary>
        /// 提交搜索选项
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void CustomAutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            string type = sender.DataContext as string;

            ContentFrame.Navigate(typeof(ActorInfoPage), new string[] { type, sender.Text }, new SuppressNavigationTransitionInfo());
        }

        private void fullScrenWindowButton_Click(object sender, RoutedEventArgs e)
        {
            if (appwindow.Presenter.Kind != AppWindowPresenterKind.FullScreen)
            {
                enterlFullScreen();
            }
            else
            {
                cancelFullScreen();
            }
            //SetTitleBar(AppTitleBar);
        }

        private void RootGrid_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Escape)
            {
                cancelFullScreen();
            }
        }

        ////记录当前NavigationView 状态
        //private NavigationViewPaneDisplayMode _displayMode;

        //进入全屏
        private void enterlFullScreen()
        {
            if (appwindow.Presenter.Kind != AppWindowPresenterKind.FullScreen)
            {
                _markPresenterKindBeforeFullScreen = appwindow.Presenter.Kind;
                appwindow.SetPresenter(AppWindowPresenterKind.FullScreen);
                //_displayMode = NavView.PaneDisplayMode;
                NavView.PaneDisplayMode = NavigationViewPaneDisplayMode.LeftMinimal;

                //监听ESC退出
                RootGrid.KeyDown += RootGrid_KeyDown;
            }
        }

        private void cancelFullScreen()
        {
            if (appwindow.Presenter.Kind == AppWindowPresenterKind.FullScreen)
            {
                NavView.PaneDisplayMode = NavigationViewPaneDisplayMode.Auto;
                appwindow.SetPresenter(Microsoft.UI.Windowing.AppWindowPresenterKind.Default);

                //取消监听
                RootGrid.KeyDown -= RootGrid_KeyDown;
            } 
        }

        private void FullScreenButton_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            (sender as HyperlinkButton).Opacity = 1;
        }

        private void FullScreenButton_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            (sender as HyperlinkButton).Opacity = 0.2;
        }

        private async void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(AppSettings.DataAccess_SavePath);
            await Launcher.LaunchFolderAsync(folder);
        }
    }
}
