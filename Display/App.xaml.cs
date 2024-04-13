using Display.Helper.Notifications;
using Display.Helper.UI;
using Display.Interfaces;
using Display.Managers;
using Display.Providers;
using Display.Services;
using Display.ViewModels;
using Display.ViewModels.Sub;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.IO;
using DataAccess;
using DataAccess.Dao.Impl;
using DataAccess.Dao.Interface;
using Display.Views.Windows;
using WinRT.Interop;
using ImageViewModel = Display.ViewModels.ImageViewModel;


namespace Display;

public partial class App
{
    public static Window AppMainWindow;

    private static NotificationManager _notificationManager;

    private static IHost _host;

    public App()
    {
        _host = ConfigureHost();

        InitAppCenter();

        InitDataAccess();

        InitializeComponent();

        _notificationManager = new NotificationManager();

        AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
    }

    private void InitDataAccess()
    {
        Context.SetSavePath(AppSettings.DataAccessSavePath);
    }

    /**
     * 使用AppCenter收集奔溃信息，目前测试中，仅开发者可用
     */
    private void InitAppCenter()
    {
        var configuration = GetService<IConfiguration>();
        var secret = configuration["AppCenterSecret"];
        if (string.IsNullOrEmpty(secret)) return;

        AppCenter.Start(secret, typeof(Analytics), typeof(Crashes));

        //// 模拟崩溃
        //Crashes.GenerateTestCrash();
    }

    private static IHost ConfigureHost()
    {
        return Host
            .CreateDefaultBuilder()
            .ConfigureAppConfiguration(builder => builder
                .AddUserSecrets<App>())
            .ConfigureServices((_, services) =>
            {
                services
                    // Services
                    //.AddSingleton<IThumbnailService, ThumbnailService>()
                    // Views and ViewModels
                    .AddSingleton<IThumbnailGeneratorService, ThumbnailGeneratorService>()

                    .AddSingleton<IActorInfoDao, ActorInfoDao>()
                    .AddSingleton<IActorNameDao, ActorNameDao>()
                    .AddSingleton<IActorVideoDao, ActorVideoDao>()
                    .AddSingleton<IBwhDao, BwhDao>()
                    .AddSingleton<IDownHistoryDao, DownHistoryDao>()
                    .AddSingleton<IFailListIsLikeLookLaterDao, FailListIsLikeLookLaterDao>()
                    .AddSingleton<IFilesInfoDao, FilesInfoDao>()
                    .AddSingleton<IFileToInfoDao, FileToInfoDao>()
                    .AddSingleton<IIsWmDao, IsWmDao>()
                    .AddSingleton<IProducerInfoDao, ProducerInfoDao>()
                    .AddSingleton<ISearchHistoryDao, SearchHistoryDao>()
                    .AddSingleton<IVideoInfoDao, VideoInfoDao>()
                    
                    .AddSingleton<UploadViewModel>()
                    .AddSingleton<TaskViewModel>()
                    .AddSingleton<SpiderManager>()
                    .AddSingleton<NavigationItemViewModel>()
                    .AddTransient<MoreNavigationItemViewModel>()
                    .AddTransient<MorePageViewModel>()
                    .AddTransient<UIShowSettingViewModel>()
                    .AddTransient<MainWindowViewModel>()
                    .AddTransient<SpiderTaskViewModel>()
                    .AddTransient<ImageViewModel>()
                    .AddTransient<SettingViewModel>()
                    .AddTransient<ThumbnailViewModel>()
                    
                    
                    ;
                
            })
            .Build();
    }

    public static T GetService<T>() where T : class
    {
        return _host.Services.GetService(typeof(T)) as T ??
               throw new ArgumentException(
                   $"Service {typeof(T)} not found. Did you forget to register the service?");
    }

    private void CurrentDomain_ProcessExit(object sender, EventArgs e)
    {
        //删除获取演员信息的进度通知
        NotificationManager.RemoveGetActorInfoProgessToast();

        _notificationManager.Unregister();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        //运行前检查
        var isNormal = CheckErrorBeforeActivateMainWindow();
        if (!isNormal) return;
            
        CreateActivateMainWindow();
    }

    private static void CreateActivateMainWindow()
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
        // //数据文件不存在
        // else if (!File.Exists(DataAccess.DbPath))
        // {
        //     DataAccess.TryCreateDbFile(dataAccessSavePath);
        // }
        // // TODO 数据文件格式有误

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