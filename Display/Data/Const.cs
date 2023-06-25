using Windows.Storage;

namespace Display.Data;

public static class Const
{
    public static class Common
    {
        public const string NoPicturePath = "ms-appx:///Assets/NoPicture.jpg";

        // “请验证账号”
        public const int AccountAnomalyCode = 911;
        public const string AccountAnomalyTip = "\\u8bf7\\u9a8c\\u8bc1\\u8d26\\u53f7";
    }

    public static class HttpHeaders
    {
        public const string Authorization = "Authorization";
        public const string CacheControl = "Cache-Control";
        public const string ContentDisposition = "Content-Disposition";
        public const string ContentEncoding = "Content-Encoding";
        public const string ContentLength = "Content-Length";
        public const string ContentMd5 = "Content-MD5";
        public const string ContentType = "Content-Type";
        public const string Date = "Date";
        public const string Expires = "Expires";
        public const string ETag = "ETag";
        public const string LastModified = "Last-Modified";
        public const string Range = "Range";
        public const string CopySource = "x-oss-copy-source";
        public const string CopySourceRange = "x-oss-copy-source-range";
        public const string Location = "Location";
        public const string ServerSideEncryption = "x-oss-server-side-encryption";
        public const string SecurityToken = "x-oss-security-token";
        public const string NextAppendPosition = "x-oss-next-append-position";
        public const string HashCrc64Ecma = "x-oss-hash-crc64ecma";
        public const string ObjectType = "x-oss-object-type";
        public const string RequestId = "x-oss-request-id";
        public const string ServerElapsedTime = "x-oss-server-time";
        public const string Callback = "x-oss-callback";
        public const string CallbackVar = "x-oss-callback-var";
        public const string BucketRegion = "x-oss-bucket-region";
        public const string QuotaDeltaSize = "x-oss-quota-delta-size";
        public const string QosDelayTime = "x-oss-qos-delay-time";
        public const string VersionId = "x-oss-version-id";
    }

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
            public const int ThumbnailOrigin = 1;


            public static class MainWindow
            {
                public const bool IsNavigationViewPaneOpen = false;
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

                public const string DownAppVersion = "8.3.0";
                public const string DownUserAgent = $"Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/54.0.2840.59 Safari/537.36 115Browser/{DownAppVersion}";

                public const string UploadAppVersion = "30.5.1";
                public const string UploadUserAgent = $"Mozilla/5.0 115disk/{UploadAppVersion}";


                public const bool IsFindSub = true;
                public const bool IsRecordDownRequest = true;
                public const double DownUrlOverdueTime = 86400.0;

                public const string DefaultDownMethod = "115";

                public const string SavePathName = "";
                public const string SavePathCid = "";
            }

            public static class BaseUrl
            {
                public const string LibreDmm = "https://www.Libredmm.com/";
                public const string JavBus = "https://www.Javbus.com/";
                public const string AvMoo = "https://Avmoo.click/";
                public const string AvSox = "https://Avsox.click/";
                public const string Jav321 = "https://www.Jav321.com/";
                public const string JavDb = "https://Javdb.com/";
                public const string Fc2Hub = "https://fc2hub.com/";
                public const string MinnanoAv = "http://www.minnano-av.com/";
                public const string X080X = "https://x555x.me/";
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
                public const bool Jav321 = true;
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
        }

        public static class App
        {
            public const bool IsUpdatedDataAccessFrom014 = false;
            public const bool IsCheckUpdate = true;
            public const string IgnoreUpdateAppVersion = "";

            public static class SavePath
            {
                public static readonly string Image = System.IO.Path.Combine(ApplicationData.Current.LocalFolder.Path, "Image");
                public static readonly string Sub = System.IO.Path.Combine(ApplicationData.Current.LocalFolder.Path, "Sub");
                public static readonly string Actor = System.IO.Path.Combine(ApplicationData.Current.LocalFolder.Path, "Actor");
                public static readonly string Attmn = System.IO.Path.Combine(ApplicationData.Current.LocalFolder.Path, "Attmn");
                public static readonly string Data = System.IO.Path.Combine(ApplicationData.Current.LocalFolder.Path, "Data");
                public static readonly string DataAccess = ApplicationData.Current.LocalFolder.Path;

                public const string BitCometDown = "";
                public const string Aria2 = "";
            }
        }

        public static class Player
        {
            public const int Selection = 0;

            public static class VideoDisplay
            {
                public const bool IsAutoPlay = false;
                public const double AutoPlayPositionPercentage = 33.0;
                public const double MaxVideoPlayCount = 3.0;
            }

            public static class ExePath
            {
                public const string Vlc = "";
                public const string Mpv = "";
                public const string PotPlayer = "";
            }

            //M3U8 = 0, Origin = 1
            public const int DefaultQuality = 0;

        }

    }

    public enum PlayQuality
    {
        M3U8 = 0,
        Origin = 1
    }
    public enum Origin { Local = 0, Web = 1 }

    public class DownApiSettings
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string ApiUrl { get; set; }
    }

}
