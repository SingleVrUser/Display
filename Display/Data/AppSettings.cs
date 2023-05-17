using Newtonsoft.Json;
using System;
using System.IO;
using Windows.Storage;

namespace Display.Data;

public class AppSettings
{
    public static ApplicationDataContainer LocalSettings => ApplicationData.Current.LocalSettings;


    /// <summary>
    /// 展示页的图片大小
    /// </summary>
    public static Tuple<double, double> ImageSize
    {
        get
        {
            var composite = (ApplicationDataCompositeValue)LocalSettings.Values["ImageSize"];

            double width;
            double height;

            if (composite != null && composite.ContainsKey("ImageWidth") && composite["ImageWidth"] is double tmp)
                width = tmp;
            else
                width = 500;

            if (composite != null && composite.ContainsKey("ImageHeight") && composite["ImageHeight"] is double tmp2)
                height = tmp2;
            else
                height = 300;


            return new Tuple<double, double>(width, height);
        }
        set
        {
            var composite = new ApplicationDataCompositeValue();
            composite["ImageWidth"] = value.Item1;
            composite["ImageHeight"] = value.Item2;
            LocalSettings.Values["ImageSize"] = composite;
        }
    }

    /// <summary>
    /// 是否动态调整图片大小
    /// </summary>
    public static bool IsAutoAdjustImageSize
    {
        get
        {
            bool check = true;
            if (LocalSettings.Values["IsAutoAdjustImageSize"] is bool value)
            {
                check = value;
            }
            return check;
        }
        set => LocalSettings.Values["IsAutoAdjustImageSize"] = value;
    }

    /// <summary>
    /// 是否已经升级了数据库
    /// </summary>
    public static bool IsUpdatedDataAccessFrom014
    {
        get
        {
            bool isUpdated = false;
            if (LocalSettings.Values["IsUpdatedDataAccessFrom014"] is bool value)
            {
                isUpdated = value;
            }
            return isUpdated;
        }
        set => LocalSettings.Values["IsUpdatedDataAccessFrom014"] = value;
    }

    /// <summary>
    /// 视频默认播放单视频
    /// </summary>
    public static bool IsDefaultPlaySingleVideo
    {
        get
        {
            bool isDefaultPlaySingleVideo = false;
            if (LocalSettings.Values["IsDefaultPlaySingleVideo"] is bool value)
            {
                isDefaultPlaySingleVideo = value;
            }
            return isDefaultPlaySingleVideo;
        }
        set => LocalSettings.Values["IsDefaultPlaySingleVideo"] = value;
    }


    /// <summary>
    /// 是否自动播放视频
    /// </summary>
    public static bool IsAutoPlayInVideoDisplay
    {
        get
        {
            var isAutoPlayVideo = false;
            if (LocalSettings.Values["IsAutoPlayInVideoDisplay"] is bool value)
            {
                isAutoPlayVideo = value;
            }
            return isAutoPlayVideo;
        }
        set => LocalSettings.Values["IsAutoPlayInVideoDisplay"] = value;
    }

    /// <summary>
    /// 自动播放的位置
    /// </summary>
    public static double AutoPlayPositionPercentage
    {
        get
        {
            var positionPercentage = 33.0;
            if (LocalSettings.Values["AutoPlayPositionPercentage"] is double value)
            {
                positionPercentage = value;
            }
            return positionPercentage;
        }
        set => LocalSettings.Values["AutoPlayPositionPercentage"] = value;
    }


    /// <summary>
    /// 视频最大播放数量
    /// </summary>
    public static double MaxVideoPlayCount
    {
        get
        {
            var count = 1.0;
            if (LocalSettings.Values["MaxVideoPlayCount"] is double value)
            {
                count = value;
            }
            return count;
        }
        set => LocalSettings.Values["MaxVideoPlayCount"] = value;
    }



    /// <summary>
    /// 是否检查更新
    /// </summary>
    public static bool IsCheckUpdate
    {
        get
        {
            bool check = true;
            if (LocalSettings.Values["IsCheckUpdate"] is bool value)
            {
                check = value;
            }
            return check;
        }
        set => LocalSettings.Values["IsCheckUpdate"] = value;
    }

    /// <summary>
    /// 忽略升级的版本号
    /// </summary>
    public static string IgnoreUpdateAppVersion
    {
        get => (string)LocalSettings.Values["IgnoreUpdateAppVersion"];
        set => LocalSettings.Values["IgnoreUpdateAppVersion"] = value;
    }

    /// <summary>
    /// 是否左侧导航是否展开
    /// </summary>
    public static bool IsNavigationViewPaneOpen
    {
        get
        {
            bool _value = false;
            if (LocalSettings.Values["IsNavigationViewPaneOpen"] is bool value)
            {
                _value = value;
            }
            return _value;
        }
        set => LocalSettings.Values["IsNavigationViewPaneOpen"] = value;
    }

    /// <summary>
    /// 115导入数据库 进程界面 的 任务完成后通知
    /// </summary>
    public static bool ProgressOfImportDataAccess_IsToastAfterTask
    {
        get
        {
            bool isToast = false;

            if (LocalSettings.Values["ProgressOfImportDataAccess_IsToastAfterTask"] is bool value)
            {
                isToast = value;
            }

            return isToast;
        }
        set
        {
            LocalSettings.Values["ProgressOfImportDataAccess_IsToastAfterTask"] = value;
        }
    }

    /// <summary>
    /// 115导入数据库 进程界面 的 任务完成后 开始搜刮任务
    /// </summary>
    /// 
    public static bool ProgressOfImportDataAccess_IsStartSpiderAfterTask
    {
        get
        {
            bool isStart = true;

            if (LocalSettings.Values["ProgressOfImportDataAccess_IsStartSpiderAfterTask"] is bool value)
            {
                isStart = value;
            }

            return isStart;
        }
        set
        {
            LocalSettings.Values["ProgressOfImportDataAccess_IsStartSpiderAfterTask"] = value;
        }
    }

    /// <summary>
    /// 115的Cookie
    /// </summary>
    public static string _115_Cookie
    {
        get
        {
            return LocalSettings.Values["Cookie"] as string;
        }
        set
        {
            LocalSettings.Values["Cookie"] = value;
        }
    }

    #region 搜刮源设置

    /// <summary>
    /// LibreDmm网址
    /// </summary>
    /// 
    private static string _libreDmm_BaseUrl = "https://www.Libredmm.com/";

    public static string LibreDmm_BaseUrl
    {
        get
        {
            var BaseUrl = LocalSettings.Values["LibreDmm_BaseUrl"] as string;
            if (string.IsNullOrEmpty(BaseUrl))
            {
                BaseUrl = _libreDmm_BaseUrl;
            }
            return BaseUrl;
        }
        set
        {
            LocalSettings.Values["LibreDmm_BaseUrl"] = value;
        }
    }

    /// <summary>
    /// JavBus网址
    /// </summary>
    /// 
    private static string _javBus_BaseUrl = "https://www.Javbus.com/";
    public static string JavBus_BaseUrl
    {
        get
        {
            var localJavBusBaseUrl = LocalSettings.Values["JavBus_BaseUrl"] as string;
            if (string.IsNullOrEmpty(localJavBusBaseUrl))
            {
                localJavBusBaseUrl = _javBus_BaseUrl;
            }
            return localJavBusBaseUrl;
        }
        set
        {
            LocalSettings.Values["JavBus_BaseUrl"] = value;
        }
    }

    /// <summary>
    /// AvMoo网址
    /// </summary>
    /// 
    private static string _avmoo_BaseUrl = "https://Avmoo.click/";
    public static string AvMoo_BaseUrl
    {
        get
        {
            var url = LocalSettings.Values["AvMoo_BaseUrl"] as string;
            if (string.IsNullOrEmpty(url))
            {
                url = _avmoo_BaseUrl;
            }
            return url;
        }
        set
        {
            LocalSettings.Values["AvMoo_BaseUrl"] = value;
        }
    }

    /// <summary>
    /// AvSox网址
    /// </summary>
    /// 
    private static string _avsox_BaseUrl = "https://Avsox.click/";
    public static string AvSox_BaseUrl
    {
        get
        {
            var url = LocalSettings.Values["AvSox_BaseUrl"] as string;
            if (string.IsNullOrEmpty(url))
            {
                url = _avsox_BaseUrl;
            }
            return url;
        }
        set
        {
            LocalSettings.Values["AvSox_BaseUrl"] = value;
        }
    }

    /// <summary>
    /// Jav321网址
    /// </summary>
    /// 
    private static string _jav321_BaseUrl = "https://www.Jav321.com/";
    public static string Jav321_BaseUrl
    {
        get
        {
            var url = LocalSettings.Values["Jav321_BaseUrl"] as string;
            if (string.IsNullOrEmpty(url))
            {
                url = _jav321_BaseUrl;
            }
            return url;
        }
        set
        {
            LocalSettings.Values["Jav321_BaseUrl"] = value;
        }
    }

    /// <summary>
    /// JavDB网址
    /// </summary>
    private static string _javDB_BaseUrl = "https://Javdb.com/";
    public static string JavDB_BaseUrl
    {
        get
        {
            var localJavDBBaseUrl = LocalSettings.Values["JavDB_BaseUrl"] as string;
            if (string.IsNullOrEmpty(localJavDBBaseUrl))
            {
                localJavDBBaseUrl = _javDB_BaseUrl;
            }
            return localJavDBBaseUrl;
        }
        set
        {
            LocalSettings.Values["JavDB_BaseUrl"] = value;
        }
    }

    /// <summary>
    /// Fc2hub网址
    /// </summary>
    private static string _fc2hub_BaseUrl = "https://fc2hub.com/";
    public static string Fc2hub_BaseUrl
    {
        get
        {
            var url = LocalSettings.Values["Fc2hub_BaseUrl"] as string;
            if (string.IsNullOrEmpty(url))
            {
                url = _fc2hub_BaseUrl;
            }
            return url;
        }
        set
        {
            LocalSettings.Values["Fc2hub_BaseUrl"] = value;
        }
    }

    /// <summary>
    /// JavDB的Cookie，查询FC信息需要
    /// </summary>
    public static string javdb_Cookie
    {
        get
        {
            return LocalSettings.Values["javDB_Cookie"] as string;
        }
        set
        {
            LocalSettings.Values["javDB_Cookie"] = value;
        }
    }
    
    /// <summary>
    /// x1080x的Cookie，搜索信息需要
    /// </summary>
    public static string X1080XCookie
    {
        get => LocalSettings.Values["X1080XCookie"] as string;
        set => LocalSettings.Values["X1080XCookie"] = value;
    }


    /// <summary>
    /// JavDB的Cookie，查询FC信息需要
    /// </summary>
    public static string X1080XUa
    {
        get => LocalSettings.Values["X1080XUa"] as string;
        set => LocalSettings.Values["X1080XUa"] = value;
    }


    #endregion


    #region 获取演员信息


    private static int? _getActorInfoLastIndex;

    public static int GetActorInfoLastIndex
    {
        get
        {
            if (_getActorInfoLastIndex != null)
            {
                return (int)_getActorInfoLastIndex;
            }
            else if (LocalSettings.Values["GetActorInfoLastIndex"] is int index)
            {
                _getActorInfoLastIndex = index;
                return index;
            }
            else
            {
                return -1;
            }
        }
        set
        {
            LocalSettings.Values["GetActorInfoLastIndex"] = value;
            _getActorInfoLastIndex = value;
        }
    }

    /// <summary>
    /// minnano-av网址
    /// </summary>
    /// 
    private const string _minnanoAv_BaseUrl = "http://www.minnano-av.com/";

    public static string MinnanoAvBaseUrl
    {
        get
        {
            var BaseUrl = LocalSettings.Values["MinnanoAv_BaseUrl"] as string;
            if (string.IsNullOrEmpty(BaseUrl))
            {
                BaseUrl = _minnanoAv_BaseUrl;
            }
            return BaseUrl;
        }
        set
        {
            LocalSettings.Values["MinnanoAv_BaseUrl"] = value;
        }
    }


    public static string X1080XBaseUrl
    {
        get
        {
            var baseUrl = "https://x222x.me/";
            if (LocalSettings.Values["MinnanoAv_BaseUrl"] is string url)
            {
                baseUrl = url;
            }

            return baseUrl;
        }
        set => LocalSettings.Values["MinnanoAv_BaseUrl"] = value;
    }



    #endregion


    /// <summary>
    /// 图片保存地址
    /// </summary>
    public static string ImageSavePath
    {
        get
        {
            string savePath = LocalSettings.Values["ImageSave_Path"] as string;
            if (string.IsNullOrEmpty(savePath))
            {
                savePath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "Image");
                FileMatch.CreateDirectoryIfNotExists(savePath);
            }
            return savePath;
        }
        set
        {
            LocalSettings.Values["ImageSave_Path"] = value;
        }
    }

    /// <summary>
    /// 字幕保存地址
    /// </summary>
    public static string SubSavePath
    {
        get
        {
            string savePath = LocalSettings.Values["SubSave_Path"] as string;
            if (string.IsNullOrEmpty(savePath))
            {
                savePath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "Sub");
                FileMatch.CreateDirectoryIfNotExists(savePath);
            }
            return savePath;
        }
        set
        {
            LocalSettings.Values["SubSave_Path"] = value;
        }
    }

    /// <summary>
    /// 图片保存地址
    /// </summary>
    public static string ActorInfoSavePath
    {
        get
        {
            string savePath = LocalSettings.Values["ActorInfo_SavePath"] as string;
            if (string.IsNullOrEmpty(savePath))
            {
                savePath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "Actor");
                FileMatch.CreateDirectoryIfNotExists(savePath);
            }
            return savePath;
        }
        set
        {
            LocalSettings.Values["ActorInfo_SavePath"] = value;
        }
    }

    /// <summary>
    /// 演员头像仓库文件保存地址
    /// </summary>
    public static string ActorFileTreeSavePath
    {
        get
        {
            string savePath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "Data");
            if (!Directory.Exists(savePath))
            {
                FileMatch.CreateDirectoryIfNotExists(savePath);
            }
            string filePath = Path.Combine(savePath, "Filetree.json");

            return filePath;
        }
    }

    /// <summary>
    /// 数据文件存储地址
    /// </summary>
    public static string DataAccessSavePath
    {
        get
        {
            string savePath = LocalSettings.Values["DataAccess_SavePath"] as string;
            if (string.IsNullOrEmpty(savePath))
            {
                savePath = ApplicationData.Current.LocalFolder.Path;
            }
            return savePath;
        }
        set
        {
            LocalSettings.Values["DataAccess_SavePath"] = value;
        }
    }

    /// <summary>
    /// 应用的启动页面
    /// </summary>
    public static int StartPageIndex
    {
        get
        {
            return Convert.ToInt32(LocalSettings.Values["StartPageIndex"]);
        }
        set
        {
            LocalSettings.Values["StartPageIndex"] = value;
        }
    }

    /// <summary>
    /// 是否使用JavDB
    /// </summary>
    public static bool IsUseJavDb
    {
        get
        {
            bool useJavDB = false;

            if (LocalSettings.Values["isUseJavDB"] is bool value)
            {
                useJavDB = value;
            }

            return useJavDB;
        }
        set
        {
            LocalSettings.Values["isUseJavDB"] = value;
        }
    }

    /// <summary>
    /// 是否使用JavBus
    /// </summary>
    public static bool IsUseJavBus
    {
        get
        {
            bool useJavBus = true;

            if (LocalSettings.Values["isUseJavBus"] is bool value)
            {
                useJavBus = value;
            }
            return useJavBus;
        }
        set
        {
            LocalSettings.Values["isUseJavBus"] = value;
        }
    }

    /// <summary>
    /// 是否使用AvMoo
    /// </summary>
    public static bool IsUseAvMoo
    {
        get
        {
            bool useJavBus = true;

            if (LocalSettings.Values["isUseAvMoo"] is bool value)
            {
                useJavBus = value;
            }
            return useJavBus;
        }
        set
        {
            LocalSettings.Values["isUseAvMoo"] = value;
        }
    }

    /// <summary>
    /// 是否使用AvSox
    /// </summary>
    public static bool IsUseAvSox
    {
        get
        {
            bool useJavBus = true;

            if (LocalSettings.Values["isUseAvSox"] is bool value)
            {
                useJavBus = value;
            }
            return useJavBus;
        }
        set
        {
            LocalSettings.Values["isUseAvSox"] = value;
        }
    }

    /// <summary>
    /// 是否使用Jav321
    /// </summary>
    public static bool IsUseJav321
    {
        get
        {
            bool useJavBus = true;

            if (LocalSettings.Values["isUseJav321"] is bool value)
            {
                useJavBus = value;
            }
            return useJavBus;
        }
        set
        {
            LocalSettings.Values["isUseJav321"] = value;
        }
    }

    /// <summary>
    /// 是否使用Fc2Hub
    /// </summary>
    public static bool IsUseFc2Hub
    {
        get
        {
            bool isUse = true;

            if (LocalSettings.Values["isUseFc2Hub"] is bool value)
            {
                isUse = value;
            }
            return isUse;
        }
        set
        {
            LocalSettings.Values["isUseFc2Hub"] = value;
        }
    }

    /// <summary>
    /// 是否使用LibDmm
    /// </summary>
    public static bool IsUseLibreDmm
    {
        get
        {
            bool isUse = true;

            if (LocalSettings.Values["isUseLibreDmm"] is bool value)
            {
                isUse = value;
            }
            return isUse;
        }
        set
        {
            LocalSettings.Values["isUseLibreDmm"] = value;
        }
    }


    /// <summary>
    /// 是否使用x1080x
    /// </summary>
    public static bool IsUseX1080X
    {
        get
        {
            var isUse = false;

            if (LocalSettings.Values["IsUseX1080X"] is bool value)
            {
                isUse = value;
            }
            return isUse;
        }
        set => LocalSettings.Values["IsUseX1080X"] = value;
    }

    public static string VlcExePath
    {
        get
        {
            var savePath = LocalSettings.Values["VlcExePath"];
            if (savePath == null)
            {
                savePath = "vlc";
                LocalSettings.Values["VlcExePath"] = savePath;
            }
            return savePath.ToString();
        }
        set
        {
            LocalSettings.Values["VlcExePath"] = value;
        }
    }

    public static string MpvExePath
    {
        get
        {
            var savePath = LocalSettings.Values["MpvExePath"];
            if (savePath == null)
            {
                savePath = "mpv";
                LocalSettings.Values["MpvExePath"] = savePath;
            }
            return savePath.ToString();
        }
        set
        {
            LocalSettings.Values["MpvExePath"] = value;
        }
    }

    public static string PotPlayerExePath
    {
        get => LocalSettings.Values["PotPlayerExePath"] as string;
        set => LocalSettings.Values["PotPlayerExePath"] = value;
    }

    /// <summary>
    /// 播放方式
    /// </summary>
    public static int PlayerSelection
    {
        get
        {
            int playerSelection = 0;

            if (LocalSettings.Values["PlayerSelection"] is int value)
            {
                playerSelection = value;
            }
            return playerSelection;
        }
        set
        {
            LocalSettings.Values["PlayerSelection"] = value;
        }
    }


    public enum PlayQuality
    {
        M3U8 = 0,
        Origin = 1
    }

    public static int DefaultPlayQuality
    {
        get
        {
            var quality = PlayQuality.M3U8;

            if (LocalSettings.Values["DefaultPlayQuality"] is int value)
            {
                quality = (PlayQuality)value;
            }
            return (int)quality;
        }
        set => LocalSettings.Values["DefaultPlayQuality"] = value;
    }

    /// <summary>
    /// 是否搜索字幕
    /// </summary>
    public static bool IsFindSub
    {
        get
        {
            var isUse = true;

            if (LocalSettings.Values["IsFindSub"] is bool value)
            {
                isUse = value;
            }
            return isUse;
        }
        set => LocalSettings.Values["IsFindSub"] = value;
    }

    /// <summary>
    /// 是否记录下载请求
    /// </summary>
    public static bool IsRecordDownRequest
    {
        get
        {
            bool isUse = true;

            if (LocalSettings.Values["IsRecordDownRequest"] is bool value)
            {
                isUse = value;
            }
            return isUse;
        }
        set
        {
            LocalSettings.Values["IsRecordDownRequest"] = value;
        }
    }

    /// <summary>
    /// 下载链接失效时间
    /// </summary>
    public static double DownUrlOverdueTime
    {
        get
        {
            double OverdueTime = 86400.0;

            if (LocalSettings.Values["DownUrlOverdueTime"] is double value)
            {
                OverdueTime = value;
            }
            return OverdueTime;
        }
        set
        {
            LocalSettings.Values["DownUrlOverdueTime"] = value;
        }
    }



    public enum Origin { Local = 0, Web = 1 }
    /// <summary>
    /// 缩略图的显示来源
    /// </summary>
    public static int ThumbnailOrigin
    {
        get
        {
            var thumbnailOrigin = LocalSettings.Values["ThumbnailOrigin"];
            if (thumbnailOrigin == null)
            {
                return (int)Origin.Web;
            }
            else
            {
                return (int)thumbnailOrigin;
            }
        }
        set => LocalSettings.Values["ThumbnailOrigin"] = value;
    }


    //默认下载方式
    public static string DefaultDownMethod
    {
        get
        {
            var DefaultDownMethod = LocalSettings.Values["DefaultDownMethod"];

            if (DefaultDownMethod == null)
            {
                return "115";
            }
            else
            {
                return DefaultDownMethod.ToString();
            }
        }
        set
        {
            LocalSettings.Values["DefaultDownMethod"] = value;
        }
    }

    public static DownApiSettings BitCometSettings
    {
        get
        {
            var BitCometSettingsStr = LocalSettings.Values["BitCometSettings"];

            if (BitCometSettingsStr == null)
            {
                return null;
            }
            else
            {
                DownApiSettings bitCometSettings = null;
                try
                {
                    bitCometSettings = JsonConvert.DeserializeObject<DownApiSettings>(BitCometSettingsStr.ToString());
                }
                catch
                {
                    //转换失败
                }

                return bitCometSettings;
            }
        }
        set
        {
            LocalSettings.Values["BitCometSettings"] = JsonConvert.SerializeObject(value);
        }
    }

    public static string BitCometSavePath
    {
        get
        {
            var BitCometSavePath = LocalSettings.Values["BitCometSavePath"];

            if (BitCometSavePath == null)
            {
                return null;
            }
            else
            {
                return BitCometSavePath.ToString();
            }
        }
        set
        {
            LocalSettings.Values["BitCometSavePath"] = value;
        }

    }

    public static DownApiSettings Aria2Settings
    {
        get
        {
            var Aria2SettingsStr = LocalSettings.Values["Aria2Settings"];

            if (Aria2SettingsStr == null)
            {
                return null;
            }
            else
            {
                DownApiSettings Aria2Settings = null;
                try
                {
                    Aria2Settings = JsonConvert.DeserializeObject<DownApiSettings>(Aria2SettingsStr.ToString());
                }
                catch
                {
                    //转换失败
                }

                return Aria2Settings;
            }
        }
        set
        {
            LocalSettings.Values["Aria2Settings"] = JsonConvert.SerializeObject(value);
        }
    }

    public static string Aria2SavePath
    {
        get
        {
            var aria2SavePath = LocalSettings.Values["Aria2SavePath"];

            return aria2SavePath?.ToString();
        }
        set => LocalSettings.Values["Aria2SavePath"] = value;
    }


    //展示页设置
    private static bool? _isShowFailListInDisplay;
    /// <summary>
    /// 展示匹配失败的列表?
    /// </summary>
    public static bool IsShowFailListInDisplay
    {
        get
        {
            if (_isShowFailListInDisplay != null)
            {
                return (bool)_isShowFailListInDisplay;
            }

            bool isShow = false;

            if (LocalSettings.Values["IsShowFailListInDisplay"] is bool value)
            {
                _isShowFailListInDisplay = value;
                isShow = value;
            }

            return isShow;
        }
        set
        {
            if (_isShowFailListInDisplay == value) return;

            _isShowFailListInDisplay = value;
            LocalSettings.Values["IsShowFailListInDisplay"] = value;
        }
    }
    /// <summary>
    /// 增量加载展示页内容
    /// </summary>
    public static bool IsIncrementalShowInDisplay
    {
        get
        {
            bool isEnable = true;

            if (LocalSettings.Values["IsIncrementalShowInDisplay"] is bool value)
            {
                isEnable = value;
            }

            return isEnable;
        }
        set
        {
            LocalSettings.Values["IsIncrementalShowInDisplay"] = value;
        }
    }


    public static string SavePath115Name
    {
        get => LocalSettings.Values["SavePath115Name"]?.ToString();
        set => LocalSettings.Values["SavePath115Name"] = value;
    }

    public static string SavePath115Cid
    {
        get => LocalSettings.Values["SavePath115Cid"]?.ToString();
        set => LocalSettings.Values["SavePath115Cid"] = value;
    }
}


public class DownApiSettings
{
    public string UserName { get; set; }
    public string Password { get; set; }
    public string ApiUrl { get; set; }
}