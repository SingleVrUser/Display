
using Display.Notifications;
using Display.WindowView;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.IO;
using System.Runtime.InteropServices;
using Windows.Storage;
using Display.Data;
using Display.Helper;
using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// 主窗口.
        /// </summary>
        public static Window AppMainWindow;

        private static NotificationManager notificationManager;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();

            notificationManager = new NotificationManager();

            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

        }

        private void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            //删除获取演员信息的进度通知
            NotificationManager.RemoveGetActorInfoProgessToast();

            notificationManager.Unregister();
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param Name="args">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            //运行前检查
            var isNormal = checkErrorBeforeActivateMainWindow();
            if (!isNormal) return;

            //初始化数据库
            await DataAccess.InitializeDatabase();

            //没有升级过数据库
            if (!AppSettings.IsUpdatedDataAccessFrom014)
            {
                var startWindow = new StartWindow();
                startWindow.Activate();
            }
            else
            {
                CreateActivateMainWindow();
            }

        }

        public static void CreateActivateMainWindow()
        {
            AppMainWindow = new MainWindow();

            notificationManager.Init();

            AppMainWindow.Activate();

        }


        private bool checkErrorBeforeActivateMainWindow()
        {
            bool isNormal = true;

            var DataAccess_SavePath = ApplicationData.Current.LocalSettings.Values["DataAccess_SavePath"] as string;

            //首次启动
            if (string.IsNullOrEmpty(DataAccess_SavePath)) return true;

            //数据存放目录不存在
            if (!Directory.Exists(DataAccess_SavePath))
            {
                CommonWindow window1 = new();
                window1.Content = new TextBlock() { Text = $"数据文件存放目录不存在，请检查：{AppSettings.DataAccess_SavePath}", IsTextSelectionEnabled = true, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
                window1.Title = "出错";
                window1.Activate();

                isNormal = false;
            }
            //数据文件不存在
            else if (!File.Exists(DataAccess.dbpath))
            {
                DataAccess.tryCreateDBFile(DataAccess_SavePath);

                //CommonWindow window1 = new();
                //window1.Content = new TextBlock() { Text = $"数据文件不存在，请检查：{DataAccess.dbpath}", IsTextSelectionEnabled = true, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
                //window1.Title = "出错";
                //window1.Activate();
                //isNormal = false;
            }
            // TODO
            // 数据文件格式有误

            return isNormal;
        }

        public static AppWindow getAppWindow(Window window)
        {
            // 获取当前窗口句柄
            IntPtr _windowHandle = WindowNative.GetWindowHandle(window);
            WindowId windowId = Win32Interop.GetWindowIdFromWindow(_windowHandle);

            return AppWindow.GetFromWindowId(windowId);
        }

        public static void ToForeground()
        {
            if (AppMainWindow != null)
            {
                WindowHelper.ShowWindow(AppMainWindow);
            }
        }


    }
}
