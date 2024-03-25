using Display.Constants;
using Display.Models.Data.Enums;
using Display.Models.Settings;
using Display.Models.Settings.Options;
using System.IO;
using System.Runtime.CompilerServices;
using static Display.Constants.DefaultSettings;
using DefaultValue = Display.Constants.DefaultSettings;
using MenuItem = Display.Models.Settings.MenuItem;

namespace Display.Models.Data;

public class AppSettings
{
    private static T GetValue<T>(T defaultValue = default, [CallerMemberName] string propertyName = null)
    {
        return string.IsNullOrEmpty(propertyName) ? defaultValue : Helper.Data.Settings.GetValue(propertyName, defaultValue);
    }

    private static void SetValue<T>(T value, [CallerMemberName] string propertyName = null)
    {
        if (string.IsNullOrEmpty(propertyName)) return;

        Helper.Data.Settings.SetValue(propertyName, value);
    }

    public static bool IsPlayBestQualityFirst
    {
        get => GetValue(DefaultValue.Player.IsPlayBestQualityFirst);
        set => SetValue(value);
    }

    /// <summary>
    /// 是否自动播放视频
    /// </summary>
    public static bool IsAutoPlayInVideoDisplay
    {
        get => GetValue(DefaultSettings.Player.VideoDisplay.IsAutoPlay);
        set => SetValue(value);
    }

    /// <summary>
    /// 自动播放的位置
    /// </summary>
    public static double AutoPlayPositionPercentage
    {
        get => GetValue(DefaultSettings.Player.VideoDisplay.AutoPlayPositionPercentage);
        set => SetValue(value);
    }

    /// <summary>
    /// 视频最大播放数量
    /// </summary>
    public static double MaxVideoPlayCount
    {
        get => GetValue(DefaultSettings.Player.VideoDisplay.MaxVideoPlayCount);
        set => SetValue(value);
    }

    public static string VlcExePath
    {
        get => GetValue(DefaultSettings.Player.ExePath.Vlc);
        set => SetValue(value);
    }

    public static string MpvExePath
    {
        get => GetValue(DefaultSettings.Player.ExePath.Mpv);
        set => SetValue(value);
    }

    public static string PotPlayerExePath
    {
        get => GetValue(DefaultSettings.Player.ExePath.PotPlayer);
        set => SetValue(value);
    }

    /// <summary>
    /// 播放方式
    /// </summary>
    public static PlayerType PlayerSelection
    {
        get => GetValue(DefaultSettings.Player.Selection);
        set => SetValue(value);
    }

    public static PlayQuality DefaultPlayQuality
    {
        get => GetValue(DefaultSettings.Player.DefaultQuality);
        set => SetValue(value);
    }

    /// <summary>
    /// 是否搜索字幕
    /// </summary>
    public static bool IsFindSub
    {
        get => GetValue(Network._115.IsFindSub);
        set => SetValue(value);
    }

    /// <summary>
    /// 是否已经升级了数据库
    /// </summary>
    public static bool IsUpdatedDataAccessFrom014
    {
        get => GetValue(DefaultSettings.App.IsUpdatedDataAccessFrom014);
        set => SetValue(value);
    }

    /// <summary>
    /// 忽略升级的版本号
    /// </summary>
    public static string IgnoreUpdateAppVersion
    {
        get => GetValue(DefaultSettings.App.IgnoreUpdateAppVersion);
        set => SetValue(value);
    }

    public static bool IsAutoSpiderInVideoDisplay
    {
        get => GetValue(DefaultSettings.Player.VideoDisplay.IsSpiderVideoInfo);
        set => SetValue(value);
    }

    public static string LibreDmmBaseUrl
    {
        get => GetValue(Network.BaseUrl.LibreDmm);
        set => SetValue(value);
    }

    /// <summary>
    /// 是否使用Fc2Hub
    /// </summary>
    public static bool IsUseFc2Hub
    {
        get => GetValue(Network.Open.Fc2Hub);
        set => SetValue(value);
    }

    /// <summary>
    /// 是否使用LibDmm
    /// </summary>
    public static bool IsUseLibreDmm
    {
        get => GetValue(Network.Open.LibreDmm);
        set => SetValue(value);
    }

    /// <summary>
    /// 是否使用x1080x
    /// </summary>
    public static bool IsUseX1080X
    {
        get => GetValue(Network.Open.X1080X);
        set => SetValue(value);
    }

    /// <summary>
    /// JavBus网址
    /// </summary>
    /// 
    public static string JavBusBaseUrl
    {
        get => GetValue(Network.BaseUrl.JavBus);
        set => SetValue(value);
    }

    /// <summary>
    /// AvMoo网址
    /// </summary>
    /// 
    public static string AvMooBaseUrl
    {
        get => GetValue(Network.BaseUrl.AvMoo);
        set => SetValue(value);
    }

    /// <summary>
    /// AvSox网址
    /// </summary>
    /// 
    public static string AvSoxBaseUrl
    {
        get => GetValue(Network.BaseUrl.AvSox);
        set => SetValue(value);
    }

    /// <summary>
    /// JavDB网址
    /// </summary>
    public static string JavDbBaseUrl
    {
        get => GetValue(Network.BaseUrl.JavDb);
        set => SetValue(value);
    }

    /// <summary>
    /// Fc2hub网址
    /// </summary>
    public static string Fc2HubBaseUrl
    {
        get => GetValue(Network.BaseUrl.Fc2Hub);
        set => SetValue(value);
    }

    /// <summary>
    /// minnano-av网址
    /// </summary>
    /// 
    public static string MinnanoAvBaseUrl
    {
        get => GetValue(Network.BaseUrl.MinnanoAv);
        set => SetValue(value);
    }

    public static string X1080XBaseUrl
    {
        get => GetValue(Network.BaseUrl.X080X);
        set => SetValue(value);
    }

    /// <summary>
    /// JavDB的Cookie，查询FC信息需要
    /// </summary>
    public static string JavDbCookie
    {
        get => GetValue(Network.Cookie.JavDb);
        set => SetValue(value);
    }

    /// <summary>
    /// x1080x的Cookie，搜索信息需要
    /// </summary>
    public static string X1080XCookie
    {
        get => GetValue(Network.Cookie.X1080X);
        set => SetValue(value);
    }

    /// <summary>
    /// x1080x的UA
    /// </summary>
    public static string X1080XUa
    {
        get => GetValue(Network.X1080X.UserAgent);
        set => SetValue(value);
    }

    /// <summary>
    /// 是否使用JavDB
    /// </summary>
    public static bool IsUseJavDb
    {
        get => GetValue(Network.Open.JavDb);
        set => SetValue(value);
    }

    /// <summary>
    /// 是否使用JavBus
    /// </summary>
    public static bool IsUseJavBus
    {
        get => GetValue(Network.Open.JavBus);
        set => SetValue(value);
    }

    /// <summary>
    /// 是否使用AvMoo
    /// </summary>
    public static bool IsUseAvMoo
    {
        get => GetValue(Network.Open.AvMoo);
        set => SetValue(value);
    }

    /// <summary>
    /// 是否使用AvSox
    /// </summary>
    public static bool IsUseAvSox
    {
        get => GetValue(Network.Open.AvSox);
        set => SetValue(value);
    }

    /// <summary>
    /// 是否左侧导航是否展开
    /// </summary>
    public static bool IsNavigationViewPaneOpen
    {
        get => GetValue(Ui.MainWindow.IsNavigationViewPaneOpen);
        set => SetValue(value);
    }

    /// <summary>
    /// 是否检查更新
    /// </summary>
    public static bool IsCheckUpdate
    {
        get => GetValue(DefaultSettings.App.IsCheckUpdate);
        set => SetValue(value);
    }

    /// <summary>
    /// 应用的启动页面
    /// </summary>
    public static int StartPageIndex
    {
        get => GetValue(Ui.MainWindow.StartPageIndex);
        set => SetValue(value);
    }

    public static NavigationViewItemEnum StartPageEnum
    {
        get => GetValue(Ui.MainWindow.StartPageEnum);
        set => SetValue(value);
    }

    /// <summary>
    /// 115导入数据库 进程界面 的 任务完成后通知
    /// </summary>
    public static bool IsToastAfterImportDataAccess
    {
        get => GetValue(Handle.IsToastAfterImportDataAccess);
        set => SetValue(value);
    }

    /// <summary>
    /// 115导入数据库 进程界面 的 任务完成后 开始搜刮任务
    /// </summary>
    /// 
    public static bool IsSpiderAfterImportDataAccess
    {
        get => GetValue(Handle.IsSpiderAfterImportDataAccess);
        set => SetValue(value);
    }

    /// <summary>
    /// 115导入数据库 进程界面 的 任务完成后 关闭窗口
    /// </summary>
    /// 
    public static bool IsCloseWindowAfterImportDataAccess
    {
        get => GetValue(Handle.IsCloseWindowAfterImportDataAccess);
        set => SetValue(value);
    }

    /// <summary>
    /// 115的Cookie
    /// </summary>
    public static string _115_Cookie
    {
        get => GetValue(Network.Cookie._115);
        set => SetValue(value);
    }

    /// <summary>
    /// 是否记录下载请求
    /// </summary>
    public static bool IsRecordDownRequest
    {
        get => GetValue(Network._115.IsRecordDownRequest);
        set => SetValue(value);
    }

    /// <summary>
    /// 下载链接失效时间
    /// </summary>
    public static double DownUrlOverdueTime
    {
        get => GetValue(Network._115.DownUrlOverdueTime);
        set => SetValue(value);
    }

    public static string SavePath115Name
    {
        get => GetValue(Network._115.SavePathShowName);
        set => SetValue(value);
    }

    public static long SavePath115Cid
    {
        get => GetValue(Network._115.SavePathCid);
        set => SetValue(value);
    }

    /// <summary>
    /// 记录获取演员信息的进度
    /// </summary>
    public static int GetActorInfoLastIndex
    {
        get => GetValue(Network.GetActorInfoLastIndex);
        set => SetValue(value);
    }

    /// <summary>
    /// 图片保存地址
    /// </summary>
    public static string ImageSavePath
    {
        get => GetValue(DefaultSettings.App.SavePath.Image);
        set => SetValue(value);
    }

    /// <summary>
    /// 字幕保存地址
    /// </summary>
    public static string SubSavePath
    {
        get => GetValue(DefaultSettings.App.SavePath.Sub);
        set => SetValue(value);
    }

    /// <summary>
    /// 演员信息保存地址
    /// </summary>
    public static string ActorInfoSavePath
    {
        get => GetValue(DefaultSettings.App.SavePath.Actor);
        set => SetValue(value);
    }

    public static string X1080XAttmnSavePath
    {
        get => GetValue(DefaultSettings.App.SavePath.Attmn);
        set => SetValue(value);
    }

    public static string DataSavePath
    {
        get => GetValue(DefaultSettings.App.SavePath.Data);
        set => SetValue(value);
    }

    /// <summary>
    /// 演员头像仓库文件保存地址
    /// </summary>
    public static string ActorFileTreeSavePath => Path.Combine(DataSavePath, "Filetree.json");

    /// <summary>
    /// 数据文件存储地址
    /// </summary>
    public static string DataAccessSavePath
    {
        get => GetValue(DefaultSettings.App.SavePath.DataAccess);
        set => SetValue(value);
    }

    public static string DefaultDownMethod
    {
        get => GetValue(Network._115.DefaultDownMethod);
        set => SetValue(value);
    }

    public static DownApiSettings BitCometSettings
    {
        get => GetValue<DownApiSettings>();
        set => SetValue(value);
    }

    public static string BitCometSavePath
    {
        get => GetValue(DefaultSettings.App.SavePath.BitCometDown);
        set => SetValue(value);
    }

    public static DownApiSettings Aria2Settings
    {
        get => GetValue<DownApiSettings>();
        set => SetValue(value);
    }

    public static string Aria2SavePath
    {
        get => GetValue(DefaultSettings.App.SavePath.Aria2);
        set => SetValue(value);
    }

    /// <summary>
    /// 展示匹配失败的列表?
    /// </summary>
    public static bool IsShowFailListInDisplay
    {
        get => GetValue(Ui.IsShowFailListInDisplay);
        set => SetValue(value);
    }

    /// <summary>
    /// 图片宽度
    /// </summary>
    public static double ImageWidth
    {
        get => GetValue(Ui.ImageSize.Width);
        set => SetValue(value);
    }

    /// <summary>
    /// 图片高度
    /// </summary>
    public static double ImageHeight
    {
        get => GetValue(Ui.ImageSize.Height);
        set => SetValue(value);
    }

    /// <summary>
    /// 是否动态调整图片大小
    /// </summary>
    public static bool IsAutoAdjustImageSize
    {
        get => GetValue(Ui.IsAutoAdjustImageSize);
        set => SetValue(value);
    }

    /// <summary>
    /// 缩略图的显示来源
    /// </summary>
    public static ThumbnailOriginType ThumbnailOriginType
    {
        get => GetValue(Ui.ThumbnailOrigin);
        set => SetValue(value);
    }


    public static MenuItem[] MenuItemsArray
    {
        get => GetValue(NavigationViewItem.DefaultMenuItems);
        set => SetValue(value);
    }

    public static MenuItem[] FootMenuItemsArray
    {
        get => GetValue(NavigationViewItem.DefaultFootMenuItems);
        set => SetValue(value);
    }

    public static MoreMenuItem[] MoreMenuItemsArray
    {
        get => GetValue(NavigationViewItem.DefaultMoreMenuItems);
        set => SetValue(value);
    }
}