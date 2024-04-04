using Windows.Storage;
using Display.Models.Dto.Settings;
using Display.Models.Enums;
using Display.Models.Records;
using Display.Views.Pages;

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
        public static readonly PageEnumAndIsVisible[] DefaultMenuItems =
        [
            new PageEnumAndIsVisible(NavigationViewItemEnum.HomePage),
            new PageEnumAndIsVisible(NavigationViewItemEnum.VideoViewPage),
            new PageEnumAndIsVisible(NavigationViewItemEnum.ActorPage),
            new PageEnumAndIsVisible(NavigationViewItemEnum.MorePage)
        ];

        public static readonly PageEnumAndIsVisible[] DefaultFootMenuItems =
        [
            new PageEnumAndIsVisible(NavigationViewItemEnum.DownPage),
            new PageEnumAndIsVisible(NavigationViewItemEnum.TaskPage)
        ];

        public static readonly PageEnumAndIsVisible[] DefaultMoreMenuItems = [
            new PageEnumAndIsVisible(NavigationViewItemEnum.FilePage),
            new PageEnumAndIsVisible(NavigationViewItemEnum.SpiderPage),
            new PageEnumAndIsVisible(NavigationViewItemEnum.BrowserPage),
            new PageEnumAndIsVisible(NavigationViewItemEnum.CalculateSha1Page),
        ];
    }

}


