using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.IO;
using Display.CustomWindows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WinRT.Interop;
using Display.Helper.UI;
using Display.Models.Data;
using Display.Helper.Notifications;
using Display.ViewModels;
using ImageViewModel = Display.ViewModels.ImageViewModel;
using Display.Interfaces;
using Display.Services;
using System.Text;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        public static Window AppMainWindow;

        private static NotificationManager _notificationManager;
        
        private static readonly IHost Host = Microsoft.Extensions.Hosting.Host
            .CreateDefaultBuilder()
            .ConfigureServices((_, services) =>
            {
                services
                    // Services
                    //.AddSingleton<IThumbnailService, ThumbnailService>()
                    // Views and ViewModels
                    .AddSingleton<IThumbnailGeneratorService, ThumbnailGeneratorService>()
                    .AddTransient<ImageViewModel>()
                    .AddTransient<ThumbnailViewModel>();
            })
            .Build();

        public App()
        {
            InitializeComponent();

            _notificationManager = new NotificationManager();

            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
        }

        public static T GetService<T>() where T : class
        {
            return Host.Services.GetService(typeof(T)) as T ??
                   throw new ArgumentException(
                       $"Service {typeof(T)} not found. Did you forget to register the service?");
        }

        private void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            //删除获取演员信息的进度通知
            NotificationManager.RemoveGetActorInfoProgessToast();

            _notificationManager.Unregister();
        }

        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            //运行前检查
            var isNormal = CheckErrorBeforeActivateMainWindow();
            if (!isNormal) return;

            // 数据文件是否存在
            // 不存在就不需要更新
            if (!File.Exists(DataAccess.DbPath))
            {
                AppSettings.IsUpdatedDataAccessFrom014 = true;
            }

            //初始化数据库
            await DataAccess.InitializeDatabase();

            // 存在数据库且没有升级过数据库
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

            _notificationManager.Init();

            AppMainWindow.Activate();
        }


        private static bool CheckErrorBeforeActivateMainWindow()
        {
            var isNormal = true;

            var dataAccessSavePath = AppSettings.DataAccessSavePath;

            //首次启动
            if (string.IsNullOrEmpty(dataAccessSavePath)) return true;

            //数据存放目录不存在
            if (!Directory.Exists(dataAccessSavePath))
            {
                CommonWindow window1 = new()
                {
                    Content = new TextBlock { Text = $"数据文件存放目录不存在，请检查：{dataAccessSavePath}", IsTextSelectionEnabled = true, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center },
                    Title = "出错"
                };
                window1.Activate();

                isNormal = false;
            }
            //数据文件不存在
            else if (!File.Exists(DataAccess.DbPath))
            {
                DataAccess.TryCreateDbFile(dataAccessSavePath);
            }
            // TODO 数据文件格式有误

            return isNormal;
        }

        public static AppWindow GetAppWindow(Window window)
        {
            // 获取当前窗口句柄
            var windowHandle = WindowNative.GetWindowHandle(window);
            var windowId = Win32Interop.GetWindowIdFromWindow(windowHandle);

            return AppWindow.GetFromWindowId(windowId);
        }

        public static void ToForeground()
        {
            if (AppMainWindow != null)
            {
                WindowHelper.SetForegroundWindow(AppMainWindow);
            }
        }
    }
}
