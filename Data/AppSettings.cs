using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace Data;

public class AppSettings
{
    public static ApplicationDataContainer localSettings { get { return ApplicationData.Current.LocalSettings; } }

    /// <summary>
    /// 115导入数据库 进程界面 的 任务完成后通知
    /// </summary>
    public static bool ProgressOfImportDataAccess_IsToastAfterTask;

    /// <summary>
    /// 115的Cookie
    /// </summary>
    public static string _115_Cookie
    {
        get
        {
            return localSettings.Values["Cookie"] as string;
        }
        set
        {
            localSettings.Values["Cookie"] = value;
        }
    }

    /// <summary>
    /// LibreDmm网址
    /// </summary>
    /// 
    private static string _libreDmm_BaseUrl = "https://www.libredmm.com/";
    public static string LibreDmm_BaseUrl
    {
        get
        {
            var BaseUrl = localSettings.Values["LibreDmm_BaseUrl"] as string;
            if (string.IsNullOrEmpty(BaseUrl))
            {
                BaseUrl = _libreDmm_BaseUrl;
            }
            return BaseUrl;
        }
        set
        {
            localSettings.Values["LibreDmm_BaseUrl"] = value;
        }
    }
    
    /// <summary>
    /// JavBus网址
    /// </summary>
    /// 
    private static string _javBus_BaseUrl = "https://www.javbus.com/";
    public static string JavBus_BaseUrl
    {
        get
        {
            var localJavBusBaseUrl = localSettings.Values["JavBus_BaseUrl"] as string;
            if (string.IsNullOrEmpty(localJavBusBaseUrl))
            {
                localJavBusBaseUrl = _javBus_BaseUrl;
            }
            return localJavBusBaseUrl;
        }
        set
        {
            localSettings.Values["JavBus_BaseUrl"] = value;
        }
    }

    /// <summary>
    /// JavDB网址
    /// </summary>
    private static string _javDB_BaseUrl = "https://javdb.com/";
    public static string JavDB_BaseUrl
    {
        get
        {
            var localJavDBBaseUrl = localSettings.Values["JavDB_BaseUrl"] as string;
            if (string.IsNullOrEmpty(localJavDBBaseUrl))
            {
                localJavDBBaseUrl = _javDB_BaseUrl;
            }
            return localJavDBBaseUrl;
        }
        set
        {
            localSettings.Values["JavDB_BaseUrl"] = value;
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
            var url = localSettings.Values["Fc2hub_BaseUrl"] as string;
            if (string.IsNullOrEmpty(url))
            {
                url = _fc2hub_BaseUrl;
            }
            return url;
        }
        set
        {
            localSettings.Values["Fc2hub_BaseUrl"] = value;
        }
    }

    /// <summary>
    /// JavDB的Cookie，查询FC信息需要
    /// </summary>
    public static string javdb_Cookie
    {
        get
        {
            return localSettings.Values["javDB_Cookie"] as string;
        }
        set
        {
            localSettings.Values["javDB_Cookie"] = value;
        }
    }

    /// <summary>
    /// 图片保存地址
    /// </summary>
    public static string Image_SavePath
    {
        get
        {
            string savePath = localSettings.Values["ImageSave_Path"] as string;
            if (string.IsNullOrEmpty(savePath))
            {
                savePath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "Image");
                FileMatch.CreateDirectoryIfNotExists(savePath);
            }
            return savePath;
        }
        set
        {
            localSettings.Values["ImageSave_Path"] = value;
        }
    }

    /// <summary>
    /// 字幕保存地址
    /// </summary>
    public static string Sub_SavePath
    {
        get
        {
            string savePath = localSettings.Values["SubSave_Path"] as string;
            if (string.IsNullOrEmpty(savePath))
            {
                savePath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "Sub");
                FileMatch.CreateDirectoryIfNotExists(savePath);
            }
            return savePath;
        }
        set
        {
            localSettings.Values["SubSave_Path"] = value;
        }
    }

    /// <summary>
    /// 图片保存地址
    /// </summary>
    public static string ActorInfo_SavePath
    {
        get
        {
            string savePath = localSettings.Values["ActorInfo_SavePath"] as string;
            if (string.IsNullOrEmpty(savePath))
            {
                savePath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "Actor");
                FileMatch.CreateDirectoryIfNotExists(savePath);
            }
            return savePath;
        }
        set
        {
            localSettings.Values["ActorInfo_SavePath"] = value;
        }
    }

    /// <summary>
    /// 演员头像仓库文件保存地址
    /// </summary>
    public static string ActorFileTree_SavePath
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
    public static string DataAccess_SavePath
    {
        get
        {
            string savePath = localSettings.Values["DataAccess_SavePath"] as string;
            if (string.IsNullOrEmpty(savePath))
            {
                savePath = ApplicationData.Current.LocalFolder.Path;
            }
            return savePath;
        }
        set
        {
            localSettings.Values["DataAccess_SavePath"] = value;
        }
    }

    /// <summary>
    /// 应用的启动页面
    /// </summary>
    public static int StartPageIndex
    {
        get
        {
            return Convert.ToInt32(localSettings.Values["StartPageIndex"]);
        }
        set
        {
            localSettings.Values["StartPageIndex"] = value;
        }
    }

    /// <summary>
    /// 是否使用JavDB
    /// </summary>
    public static bool isUseJavDB
    {
        get
        {
            bool useJavDB = false;

            if (localSettings.Values["isUseJavDB"] is bool value)
            {
                useJavDB = value;
            }

            return useJavDB;
        }
        set
        {
            localSettings.Values["isUseJavDB"] = value;
        }
    }

    /// <summary>
    /// 是否使用JavBus
    /// </summary>
    public static bool isUseJavBus
    {
        get
        {
            bool useJavBus = true;

            if (localSettings.Values["isUseJavBus"] is bool value)
            {
                useJavBus = value;
            }
            return useJavBus;
        }
        set
        {
            localSettings.Values["isUseJavBus"] = value;
        }
    }

    /// <summary>
    /// 是否使用Fc2Hub
    /// </summary>
    public static bool isUseFc2Hub
    {
        get
        {
            bool isUse = true;

            if (localSettings.Values["isUseFc2Hub"] is bool value)
            {
                isUse = value;
            }
            return isUse;
        }
        set
        {
            localSettings.Values["isUseFc2Hub"] = value;
        }
    }

    /// <summary>
    /// 是否使用LibDmm
    /// </summary>
    public static bool isUseLibreDmm
    {
        get
        {
            bool isUse = true;

            if (localSettings.Values["isUseLibreDmm"] is bool value)
            {
                isUse = value;
            }
            return isUse;
        }
        set
        {
            localSettings.Values["isUseLibreDmm"] = value;
        }
    }

    public static string VlcExePath
    {
        get
        {
            var savePath = localSettings.Values["VlcExePath"];
            if (savePath == null)
            {
                savePath = "vlc";
                localSettings.Values["VlcExePath"] = savePath;
            }
            return savePath.ToString();
        }
        set
        {
            localSettings.Values["VlcExePath"] = value;
        }
    }

    public static string MpvExePath
    {
        get
        {
            var savePath = localSettings.Values["MpvExePath"];
            if (savePath == null)
            {
                savePath = "mpv";
                localSettings.Values["MpvExePath"] = savePath;
            }
            return savePath.ToString();
        }
        set
        {
            localSettings.Values["MpvExePath"] = value;
        }
    }

    public static string PotPlayerExePath
    {
        get
        {
            return localSettings.Values["PotPlayerExePath"] as string;
        }
        set
        {
            localSettings.Values["PotPlayerExePath"] = value;
        }
    }

    /// <summary>
    /// 播放方式
    /// </summary>
    public static int PlayerSelection
    {
        get
        {
            int playerSelection = 0;

            if (localSettings.Values["PlayerSelection"] is int value)
            {
                playerSelection = value;
            }
            return playerSelection;
        }
        set
        {
            localSettings.Values["PlayerSelection"] = value;
        }
    }


    /// <summary>
    /// 是否搜索字幕
    /// </summary>
    public static bool IsFindSub
    {
        get
        {
            bool isUse = true;

            if (localSettings.Values["IsFindSub"] is bool value)
            {
                isUse = value;
            }
            return isUse;
        }
        set
        {
            localSettings.Values["IsFindSub"] = value;
        }
    }



    public enum Origin { Local = 0, Web = 1 }
    /// <summary>
    /// 缩略图的显示来源
    /// </summary>
    //private static Origin _thumbnialOrigin = Origin.Local;
    public static int ThumbnailOrigin
    {
        get
        {
            var thumbnialOrigin = localSettings.Values["thumbnialOrigin"];
            if (thumbnialOrigin == null)
            {
                return (int)Origin.Web;
            }
            else
            {
                return (int)thumbnialOrigin;
            }
        }
        set
        {
            localSettings.Values["thumbnialOrigin"] = value;
        }
    }


    //默认下载方式
    public static string DefaultDownMethod
    {
        get
        {
            var DefaultDownMethod = localSettings.Values["DefaultDownMethod"];

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
            localSettings.Values["DefaultDownMethod"] = value;
        }
    }

    public static DownApiSettings BitCometSettings
    {
        get
        {
            var BitCometSettingsStr = localSettings.Values["BitCometSettings"];

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
            localSettings.Values["BitCometSettings"] = JsonConvert.SerializeObject(value);
        }
    }

    public static string BitCometSavePath
    {
        get
        {
            var BitCometSavePath = localSettings.Values["BitCometSavePath"];

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
            localSettings.Values["BitCometSavePath"] = value;
        }

    }

    public static DownApiSettings Aria2Settings
    {
        get
        {
            var Aria2SettingsStr = localSettings.Values["Aria2Settings"];

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
            localSettings.Values["Aria2Settings"] = JsonConvert.SerializeObject(value);
        }
    }

    public static string Aria2SavePath
    {
        get
        {
            var Aria2SavePath = localSettings.Values["Aria2SavePath"];

            if (Aria2SavePath == null)
            {
                return null;
            }
            else
            {
                return Aria2SavePath.ToString();
            }
        }
        set
        {
            localSettings.Values["Aria2SavePath"] = value;
        }

    }


    //展示页设置

    /// <summary>
    /// 展示匹配失败的列表?
    /// </summary>
    public static bool IsShowFailListInDisplay
    {
        get
        {
            bool isShow = false;

            if (localSettings.Values["IsShowFailListInDisplay"] is bool value)
            {
                isShow = value;
            }

            return isShow;
        }
        set
        {
            localSettings.Values["IsShowFailListInDisplay"] = value;
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

            if (localSettings.Values["IsIncrementalShowInDisplay"] is bool value)
            {
                isEnable = value;
            }

            return isEnable;
        }
        set
        {
            localSettings.Values["IsIncrementalShowInDisplay"] = value;
        }
    }

}



public class DownApiSettings
{
    public string UserName { get; set; }
    public string Password { get; set; }
    public string ApiUrl { get; set; }
}