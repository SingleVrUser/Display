using System.Drawing;
using Display.Models.Settings;
using Windows.Storage;
using Display.Models.Enums;

namespace Display.Constants;
public static class DefaultSettings
{
    public static class Ui
    {
        public static class ImageSize
        {
            public const double Width = 500;
            public const double Height = 300;
        }

        public const bool IsAutoAdjustImageSize = true;


        public const bool IsShowFailListInDisplay = false;

        //Local = 0, Web = 1
        public const ThumbnailOriginType ThumbnailOrigin = ThumbnailOriginType.Web;

        public static class MainWindow
        {
            public const bool IsNavigationViewPaneOpen = false;
            public const NavigationViewItemEnum StartPageEnum = NavigationViewItemEnum.SettingPage;
            public const int StartPageIndex = 0;
        }
    }

    public static class Network
    {
        public const int GetActorInfoLastIndex = -1;

        public static class X1080X
        {
            public const string UserAgent = "";
        }

        public static class _115
        {
            private const string DownAppVersion = "8.3.0";
            internal const string UploadAppVersion = "30.5.1";

            public const string DownUserAgent = $"Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/54.0.2840.59 Safari/537.36 115Browser/{DownAppVersion}";
            public const string UploadUserAgent = $"Mozilla/5.0 115disk/{UploadAppVersion}";

            public const bool IsFindSub = true;
            public const bool IsRecordDownRequest = true;
            public const double DownUrlOverdueTime = 86400.0;

            public const string DefaultDownMethod = "115";
            public const string SavePathShowName = "根目录";
            public const long SavePathCid = 0;
        }

        public static class BaseUrl
        {
            public const string LibreDmm = "https://www.Libredmm.com/";
            public const string JavBus = "https://www.Javbus.com/";
            public const string AvMoo = "https://avmoo.online/";
            public const string AvSox = "https://Avsox.click/";
            public const string JavDb = "https://Javdb.com/";
            public const string Fc2Hub = "https://fc2hub.com/";
            public const string MinnanoAv = "http://www.minnano-av.com/";
            public const string X080X = "https://ai1080.art/";
        }

        public static class Cookie
        {
            public const string JavDb = "";
            public const string _115 = "";
            public const string X1080X = "";
        }

        public static class Open
        {
            public const bool JavBus = true;
            public const bool AvMoo = true;
            public const bool AvSox = true;
            public const bool Fc2Hub = true;
            public const bool LibreDmm = true;
            public const bool JavDb = false;
            public const bool X1080X = false;
        }
    }

    public static class Handle
    {
        public const bool IsToastAfterImportDataAccess = false;
        public const bool IsSpiderAfterImportDataAccess = true;
        public const bool IsCloseWindowAfterImportDataAccess = true;
    }

    public static class App
    {
        public const bool IsUpdatedDataAccessFrom014 = false;
        public const bool IsCheckUpdate = true;
        public const string IgnoreUpdateAppVersion = "";

        public static class SavePath
        {
            private static readonly string BaseSavePath = ApplicationData.Current.LocalFolder.Path;
            public static readonly string Image = System.IO.Path.Combine(BaseSavePath, "Image");
            public static readonly string Sub = System.IO.Path.Combine(BaseSavePath, "Sub");
            public static readonly string Actor = System.IO.Path.Combine(BaseSavePath, "Actor");
            public static readonly string Attmn = System.IO.Path.Combine(BaseSavePath, "Attmn");
            public static readonly string Data = System.IO.Path.Combine(BaseSavePath, "Data");
            public static readonly string DataAccess = BaseSavePath;

            public const string BitCometDown = "";
            public const string Aria2 = "";
        }
    }

    public static class Player
    {
        public const PlayerType Selection = PlayerType.WebView;

        public static class VideoDisplay
        {
            public const bool IsAutoPlay = false;
            public const bool IsSpiderVideoInfo = false;
            public const double AutoPlayPositionPercentage = 33.0;
            public const double MaxVideoPlayCount = 3.0;
        }

        public static class ExePath
        {
            public const string Vlc = "vlc";
            public const string Mpv = "mpv";
            public const string PotPlayer = "";
        }

        //M3U8 = 0, Origin = 1
        public const PlayQuality DefaultQuality = PlayQuality.M3U8;

        //是否画质优先
        public const bool IsPlayBestQualityFirst = true;
    }

    public static class NavigationViewItem
    {
        public static readonly MenuItem[] DefaultMenuItems =
        [
            new MenuItem("主页", "\xE10F", NavigationViewItemEnum.HomePage),
            new MenuItem("展示", "\xE8BA",NavigationViewItemEnum.VideoViewPage),
            new MenuItem("演员", "\xE77B", NavigationViewItemEnum.ActorPage),
            new MenuItem("其他", "\xE10C", NavigationViewItemEnum.MorePage)
        ];

        public static readonly MenuItem[] DefaultFootMenuItems =
        [
            new MenuItem("下载", "\xE118", NavigationViewItemEnum.DownPage)
            {
                CanSelected = false
            },
            new MenuItem("任务", "\xE174", NavigationViewItemEnum.TaskPage)
            {
                CanSelected = false
            }
        ];

        public static readonly MoreMenuItem[] DefaultMoreMenuItems = [
            new MoreMenuItem("文件列表",  "115中的文件列表", "/Assets/Svg/file_alt_icon.svg", NavigationViewItemEnum.FilePage),
            new MoreMenuItem("搜刮信息", "搜刮本地数据库中视频对应的信息", "/Assets/Svg/find_internet_magnifier_search_security_icon.svg", NavigationViewItemEnum.SpiderPage),
            new MoreMenuItem("演员头像", "从gfriends仓库中获取演员头像", "/Assets/Svg/face_male_man_portrait_icon.svg", NavigationViewItemEnum.ActorCoverPage),
            new MoreMenuItem("缩略图", "获取视频缩略图", "/Assets/Svg/image_icon.svg", NavigationViewItemEnum.ThumbnailPage),
            new MoreMenuItem("浏览器", "115网页版，并附加下载选项", "/Assets/Svg/explorer_internet_logo_logos_icon.svg", NavigationViewItemEnum.BrowserPage),
            new MoreMenuItem("计算Sha1", "计算本地文件的Sha1", "/Assets/Svg/accounting_banking_business_calculate_calculator_icon.svg", NavigationViewItemEnum.CalculateSha1Page)
            {
                Label = "测试中"
            }
        ];
    }

}


