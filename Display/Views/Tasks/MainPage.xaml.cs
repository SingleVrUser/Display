// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Display.CustomWindows;
using Display.Helper.UI;
using Display.Models.Settings;
using Display.ViewModels;
using Microsoft.UI.Xaml.Controls;
using WinUIEx;
using Window = Microsoft.UI.Xaml.Window;


namespace Display.Views.Tasks
{
    public sealed partial class MainPage : Page
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
        public static void ShowSingleWindow()
        {
            if (_window == null)
            {
                _window = new CommonWindow("传输任务", 842, 537)
                {
                    Content = new MainPage()
                };
                _window.Closed += (_, _) =>
                {
                    _window = null;
                };
                _window?.Show();
            }
            else
            {
                WindowHelper.SetForegroundWindow(_window);
            }
        }

        /// <summary>
        /// 响应切换页面的请求
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void ContentNavigationView_OnSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem is not NavLink currentLink) return;

            _viewModel.CurrentLink = currentLink;
            ContentFrame.Navigate(currentLink.NavPageType, null);
        }

    }
}
