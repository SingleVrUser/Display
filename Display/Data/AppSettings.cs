using System.IO;
using System.Text.Json;
using Windows.Storage;
using DefaultValue = Display.Data.Const.DefaultSettings;

namespace Display.Data;

public class AppSettings
{
    /// <summary>
    /// 图片宽度
    /// </summary>
    public static double ImageWidth
    {
        get => Helper.Settings.GetValue(nameof(ImageWidth), DefaultValue.Ui.ImageSize.Width);
        set => Helper.Settings.SetValue(nameof(ImageWidth), value);
    }

    /// <summary>
    /// 图片高度
    /// </summary>
    public static double ImageHeight
    {
        get => Helper.Settings.GetValue(nameof(ImageHeight), DefaultValue.Ui.ImageSize.Height);
        set => Helper.Settings.SetValue(nameof(ImageHeight), value);
    }


    /// <summary>
    /// 是否动态调整图片大小
    /// </summary>
    public static bool IsAutoAdjustImageSize
    {
        get => Helper.Settings.GetValue(nameof(IsAutoAdjustImageSize), DefaultValue.Ui.IsAutoAdjustImageSize);
        set => Helper.Settings.SetValue(nameof(IsAutoAdjustImageSize), value);
    }

    /// <summary>
    /// 是否已经升级了数据库
    /// </summary>
    public static bool IsUpdatedDataAccessFrom014
    {
        get => Helper.Settings.GetValue(nameof(IsUpdatedDataAccessFrom014), DefaultValue.App.IsUpdatedDataAccessFrom014);
        set => Helper.Settings.SetValue(nameof(IsUpdatedDataAccessFrom014), value);
    }

    /// <summary>
    /// 是否自动播放视频
    /// </summary>
    public static bool IsAutoPlayInVideoDisplay
    {
        get => Helper.Settings.GetValue(nameof(IsAutoPlayInVideoDisplay), DefaultValue.Player.VideoDisplay.IsAutoPlay);
        set => Helper.Settings.SetValue(nameof(IsAutoPlayInVideoDisplay), value);
    }

    /// <summary>
    /// 自动播放的位置
    /// </summary>
    public static double AutoPlayPositionPercentage
    {
        get => Helper.Settings.GetValue(nameof(AutoPlayPositionPercentage), DefaultValue.Player.VideoDisplay.AutoPlayPositionPercentage);
        set => Helper.Settings.SetValue(nameof(AutoPlayPositionPercentage), value);
    }


    /// <summary>
    /// 视频最大播放数量
    /// </summary>
    public static double MaxVideoPlayCount
    {
        get => Helper.Settings.GetValue(nameof(MaxVideoPlayCount), DefaultValue.Player.VideoDisplay.MaxVideoPlayCount);
        set => Helper.Settings.SetValue(nameof(MaxVideoPlayCount), value);
    }

    /// <summary>
    /// 是否检查更新
    /// </summary>
    public static bool IsCheckUpdate
    {
        get => Helper.Settings.GetValue(nameof(IsCheckUpdate), DefaultValue.App.IsCheckUpdate);
        set => Helper.Settings.SetValue(nameof(IsCheckUpdate), value);
    }

    /// <summary>
    /// 忽略升级的版本号
    /// </summary>
    public static string IgnoreUpdateAppVersion
    {
        get => Helper.Settings.GetValue(nameof(IgnoreUpdateAppVersion), DefaultValue.App.IgnoreUpdateAppVersion);
        set => Helper.Settings.SetValue(nameof(IgnoreUpdateAppVersion), value);
    }

    /// <summary>
    /// 是否左侧导航是否展开
    /// </summary>
    public static bool IsNavigationViewPaneOpen
    {
        get => Helper.Settings.GetValue(nameof(IsNavigationViewPaneOpen), DefaultValue.Ui.MainWindow.IsNavigationViewPaneOpen);
        set => Helper.Settings.SetValue(nameof(IsNavigationViewPaneOpen), value);
    }

    /// <summary>
    /// 115导入数据库 进程界面 的 任务完成后通知
    /// </summary>
    public static bool IsToastAfterImportDataAccess
    {
        get => Helper.Settings.GetValue(nameof(IsToastAfterImportDataAccess), DefaultValue.Handle.IsToastAfterImportDataAccess);
        set => Helper.Settings.SetValue(nameof(IsToastAfterImportDataAccess), value);
    }

    /// <summary>
    /// 115导入数据库 进程界面 的 任务完成后 开始搜刮任务
    /// </summary>
    /// 
    public static bool IsSpiderAfterImportDataAccess
    {
        get => Helper.Settings.GetValue(nameof(IsSpiderAfterImportDataAccess), DefaultValue.Handle.IsSpiderAfterImportDataAccess);
        set => Helper.Settings.SetValue(nameof(IsSpiderAfterImportDataAccess), value);
    }

    /// <summary>
    /// 115的Cookie
    /// </summary>
    public static string _115_Cookie
    {
        get => Helper.Settings.GetValue(nameof(_115_Cookie), DefaultValue.Network.Cookie._115);
        set => Helper.Settings.SetValue(nameof(_115_Cookie), value);
    }

    public static string LibreDmmBaseUrl
    {
        get => Helper.Settings.GetValue(nameof(LibreDmmBaseUrl), DefaultValue.Network.BaseUrl.LibreDmm);
        set => Helper.Settings.SetValue(nameof(LibreDmmBaseUrl), value);
    }

    /// <summary>
    /// JavBus网址
    /// </summary>
    /// 
    public static string JavBusBaseUrl
    {
        get => Helper.Settings.GetValue(nameof(JavBusBaseUrl), DefaultValue.Network.BaseUrl.JavBus);
        set => Helper.Settings.SetValue(nameof(JavBusBaseUrl), value);
    }

    /// <summary>
    /// AvMoo网址
    /// </summary>
    /// 
    public static string AvMooBaseUrl
    {
        get => Helper.Settings.GetValue(nameof(AvMooBaseUrl), DefaultValue.Network.BaseUrl.AvMoo);
        set => Helper.Settings.SetValue(nameof(AvMooBaseUrl), value);
    }

    /// <summary>
    /// AvSox网址
    /// </summary>
    /// 
    public static string AvSoxBaseUrl
    {
        get => Helper.Settings.GetValue(nameof(AvSoxBaseUrl), DefaultValue.Network.BaseUrl.AvSox);
        set => Helper.Settings.SetValue(nameof(AvSoxBaseUrl), value);
    }

    /// <summary>
    /// Jav321网址
    /// </summary>
    /// 
    public static string Jav321BaseUrl
    {
        get => Helper.Settings.GetValue(nameof(Jav321BaseUrl), DefaultValue.Network.BaseUrl.Jav321);
        set => Helper.Settings.SetValue(nameof(Jav321BaseUrl), value);
    }

    /// <summary>
    /// JavDB网址
    /// </summary>
    public static string JavDbBaseUrl
    {
        get => Helper.Settings.GetValue(nameof(JavDbBaseUrl), DefaultValue.Network.BaseUrl.JavDb);
        set => Helper.Settings.SetValue(nameof(JavDbBaseUrl), value);
    }

    /// <summary>
    /// Fc2hub网址
    /// </summary>
    public static string Fc2HubBaseUrl
    {
        get => Helper.Settings.GetValue(nameof(Fc2HubBaseUrl), DefaultValue.Network.BaseUrl.Fc2Hub);
        set => Helper.Settings.SetValue(nameof(Fc2HubBaseUrl), value);
    }

    /// <summary>
    /// minnano-av网址
    /// </summary>
    /// 
    public static string MinnanoAvBaseUrl
    {
        get => Helper.Settings.GetValue(nameof(MinnanoAvBaseUrl), DefaultValue.Network.BaseUrl.MinnanoAv);
        set => Helper.Settings.SetValue(nameof(MinnanoAvBaseUrl), value);
    }

    public static string X1080XBaseUrl
    {
        get => Helper.Settings.GetValue(nameof(X1080XBaseUrl), DefaultValue.Network.BaseUrl.X080X);
        set => Helper.Settings.SetValue(nameof(X1080XBaseUrl), value);
    }

    /// <summary>
    /// JavDB的Cookie，查询FC信息需要
    /// </summary>
    public static string JavDbCookie
    {
        get => Helper.Settings.GetValue(nameof(JavDbCookie), DefaultValue.Network.Cookie.JavDb);
        set => Helper.Settings.SetValue(nameof(JavDbCookie), value);
    }
    
    /// <summary>
    /// x1080x的Cookie，搜索信息需要
    /// </summary>
    public static string X1080XCookie
    {
        get => Helper.Settings.GetValue(nameof(X1080XCookie), DefaultValue.Network.Cookie.X1080X);
        set => Helper.Settings.SetValue(nameof(X1080XCookie), value);
    }

    /// <summary>
    /// x1080x的UA
    /// </summary>
    public static string X1080XUa
    {
        get => Helper.Settings.GetValue(nameof(X1080XUa), DefaultValue.Network.X1080X.UserAgent);
        set => Helper.Settings.SetValue(nameof(X1080XUa), value);
    }

    /// <summary>
    /// 记录获取演员信息的进度
    /// </summary>
    public static int GetActorInfoLastIndex
    {
        get => Helper.Settings.GetValue(nameof(GetActorInfoLastIndex), DefaultValue.Network.GetActorInfoLastIndex);
        set => Helper.Settings.SetValue(nameof(GetActorInfoLastIndex), value);
    }

    /// <summary>
    /// 图片保存地址
    /// </summary>
    public static string ImageSavePath
    {
        get => Helper.Settings.GetValue(nameof(ImageSavePath), DefaultValue.App.SavePath.Image);
        set => Helper.Settings.SetValue(nameof(ImageSavePath), value);
    }

    /// <summary>
    /// 字幕保存地址
    /// </summary>
    public static string SubSavePath
    {
        get => Helper.Settings.GetValue(nameof(SubSavePath), DefaultValue.App.SavePath.Sub);
        set => Helper.Settings.SetValue(nameof(SubSavePath), value);
    }

    /// <summary>
    /// 演员信息保存地址
    /// </summary>
    public static string ActorInfoSavePath
    {
        get => Helper.Settings.GetValue(nameof(ActorInfoSavePath), DefaultValue.App.SavePath.Actor);
        set => Helper.Settings.SetValue(nameof(ActorInfoSavePath), value);
    }

    public static string X1080XAttmnSavePath
    {
        get => Helper.Settings.GetValue(nameof(X1080XAttmnSavePath), DefaultValue.App.SavePath.Attmn);
        set => Helper.Settings.SetValue(nameof(X1080XAttmnSavePath), value);
    }

    public static string DataSavePath
    {
        get => Helper.Settings.GetValue(nameof(DataSavePath), DefaultValue.App.SavePath.Data);
        set => Helper.Settings.SetValue(nameof(DataSavePath), value);
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
        get => Helper.Settings.GetValue(nameof(DataAccessSavePath), DefaultValue.App.SavePath.DataAccess);
        set => Helper.Settings.SetValue(nameof(DataAccessSavePath), value);
    }

    /// <summary>
    /// 应用的启动页面
    /// </summary>
    public static int StartPageIndex
    {
        get => Helper.Settings.GetValue(nameof(StartPageIndex), DefaultValue.Ui.MainWindow.StartPageIndex);
        set => Helper.Settings.SetValue(nameof(StartPageIndex), value);
    }

    /// <summary>
    /// 是否使用JavDB
    /// </summary>
    public static bool IsUseJavDb
    {
        get => Helper.Settings.GetValue(nameof(IsUseJavDb), DefaultValue.Network.Open.JavDb);
        set => Helper.Settings.SetValue(nameof(IsUseJavDb), value);
    }

    /// <summary>
    /// 是否使用JavBus
    /// </summary>
    public static bool IsUseJavBus
    {
        get => Helper.Settings.GetValue(nameof(IsUseJavBus), DefaultValue.Network.Open.JavBus);
        set => Helper.Settings.SetValue(nameof(IsUseJavBus), value);
    }

    /// <summary>
    /// 是否使用AvMoo
    /// </summary>
    public static bool IsUseAvMoo
    {
        get => Helper.Settings.GetValue(nameof(IsUseAvMoo), DefaultValue.Network.Open.AvMoo);
        set => Helper.Settings.SetValue(nameof(IsUseAvMoo), value);
    }

    /// <summary>
    /// 是否使用AvSox
    /// </summary>
    public static bool IsUseAvSox
    {
        get => Helper.Settings.GetValue(nameof(IsUseAvSox), DefaultValue.Network.Open.AvSox);
        set => Helper.Settings.SetValue(nameof(IsUseAvSox), value);
    }

    /// <summary>
    /// 是否使用Jav321
    /// </summary>
    public static bool IsUseJav321
    {
        get => Helper.Settings.GetValue(nameof(IsUseJav321), DefaultValue.Network.Open.Jav321);
        set => Helper.Settings.SetValue(nameof(IsUseJav321), value);
    }

    /// <summary>
    /// 是否使用Fc2Hub
    /// </summary>
    public static bool IsUseFc2Hub
    {
        get => Helper.Settings.GetValue(nameof(IsUseFc2Hub), DefaultValue.Network.Open.Fc2Hub);
        set => Helper.Settings.SetValue(nameof(IsUseFc2Hub), value);
    }

    /// <summary>
    /// 是否使用LibDmm
    /// </summary>
    public static bool IsUseLibreDmm
    {
        get => Helper.Settings.GetValue(nameof(IsUseLibreDmm), DefaultValue.Network.Open.LibreDmm);
        set => Helper.Settings.SetValue(nameof(IsUseLibreDmm), value);
    }


    /// <summary>
    /// 是否使用x1080x
    /// </summary>
    public static bool IsUseX1080X
    {
        get => Helper.Settings.GetValue(nameof(IsUseX1080X), DefaultValue.Network.Open.X1080X);
        set => Helper.Settings.SetValue(nameof(IsUseX1080X), value);
    }

    public static string VlcExePath
    {
        get => Helper.Settings.GetValue(nameof(VlcExePath), DefaultValue.Player.ExePath.Vlc);
        set => Helper.Settings.SetValue(nameof(VlcExePath), value);
    }

    public static string MpvExePath
    {
        get => Helper.Settings.GetValue(nameof(MpvExePath), DefaultValue.Player.ExePath.Mpv);
        set => Helper.Settings.SetValue(nameof(MpvExePath), value);
    }

    public static string PotPlayerExePath
    {
        get => Helper.Settings.GetValue(nameof(PotPlayerExePath), DefaultValue.Player.ExePath.PotPlayer);
        set => Helper.Settings.SetValue(nameof(PotPlayerExePath), value);
    }

    /// <summary>
    /// 播放方式
    /// </summary>
    public static int PlayerSelection
    {
        get => Helper.Settings.GetValue(nameof(PlayerSelection), DefaultValue.Player.Selection);
        set => Helper.Settings.SetValue(nameof(PlayerSelection), value);
    }



    public static int DefaultPlayQuality
    {
        get => Helper.Settings.GetValue(nameof(DefaultPlayQuality), DefaultValue.Player.DefaultQuality);
        set => Helper.Settings.SetValue(nameof(DefaultPlayQuality), value);
    }

    /// <summary>
    /// 是否搜索字幕
    /// </summary>
    public static bool IsFindSub
    {
        get => Helper.Settings.GetValue(nameof(IsFindSub), DefaultValue.Network._115.IsFindSub);
        set => Helper.Settings.SetValue(nameof(IsFindSub), value);
    }

    /// <summary>
    /// 是否记录下载请求
    /// </summary>
    public static bool IsRecordDownRequest
    {
        get => Helper.Settings.GetValue(nameof(IsRecordDownRequest), DefaultValue.Network._115.IsRecordDownRequest);
        set => Helper.Settings.SetValue(nameof(IsRecordDownRequest), value);
    }

    /// <summary>
    /// 下载链接失效时间
    /// </summary>
    public static double DownUrlOverdueTime
    {
        get => Helper.Settings.GetValue(nameof(DownUrlOverdueTime), DefaultValue.Network._115.DownUrlOverdueTime);
        set => Helper.Settings.SetValue(nameof(DownUrlOverdueTime), value);
    }

    /// <summary>
    /// 缩略图的显示来源
    /// </summary>
    public static int ThumbnailOrigin
    {
        get => Helper.Settings.GetValue(nameof(ThumbnailOrigin), DefaultValue.Ui.ThumbnailOrigin);
        set => Helper.Settings.SetValue(nameof(ThumbnailOrigin), value);
    }


    //默认下载方式
    public static string DefaultDownMethod
    {
        get => Helper.Settings.GetValue(nameof(DefaultDownMethod), DefaultValue.Network._115.DefaultDownMethod);
        set => Helper.Settings.SetValue(nameof(DefaultDownMethod), value);
    }

    public static DownApiSettings BitCometSettings
    {
        get => Helper.Settings.GetValue<DownApiSettings>(nameof(BitCometSettings));
        set => Helper.Settings.SetValue(nameof(BitCometSettings), value);
    }

    public static string BitCometSavePath
    {
        get => Helper.Settings.GetValue(nameof(BitCometSavePath), DefaultValue.App.SavePath.BitCometDown);
        set => Helper.Settings.SetValue(nameof(BitCometSavePath), value);
    }

    public static DownApiSettings Aria2Settings
    {
        get => Helper.Settings.GetValue<DownApiSettings>(nameof(Aria2Settings));
        set => Helper.Settings.SetValue(nameof(Aria2Settings), value);
    }

    public static string Aria2SavePath
    {
        get => Helper.Settings.GetValue(nameof(Aria2SavePath), DefaultValue.App.SavePath.Aria2);
        set => Helper.Settings.SetValue(nameof(Aria2SavePath), value);
    }
    
    /// <summary>
    /// 展示匹配失败的列表?
    /// </summary>
    public static bool IsShowFailListInDisplay
    {
        get => Helper.Settings.GetValue(nameof(IsShowFailListInDisplay), DefaultValue.Ui.IsShowFailListInDisplay);
        set => Helper.Settings.SetValue(nameof(IsShowFailListInDisplay), value);
    }

    public static string SavePath115Name
    {
        get => Helper.Settings.GetValue(nameof(SavePath115Name), DefaultValue.Network._115.SavePathName);
        set => Helper.Settings.SetValue(nameof(SavePath115Name), value);
    }

    public static string SavePath115Cid
    {
        get => Helper.Settings.GetValue(nameof(SavePath115Cid), DefaultValue.Network._115.SavePathCid);
        set => Helper.Settings.SetValue(nameof(SavePath115Cid), value);
    }

}

public class DownApiSettings
{
    public string UserName { get; set; }
    public string Password { get; set; }
    public string ApiUrl { get; set; }
}

