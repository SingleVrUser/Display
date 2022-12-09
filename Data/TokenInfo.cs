using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.UI.Xaml;
using System;
using System.IO;
using System.Linq;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Windows.ApplicationModel;

namespace Data
{
    public class TokenInfo
    {
        public int state { get; set; }
        public string error { get; set; }
        public int errno { get; set; }
        public string message { get; set; }
        public int code { get; set; }
        public TokenInfoData data { get; set; }
    }

    public class TokenInfoData
    {
        public int user_id { get; set; }
        public string user_name { get; set; }
        public string email { get; set; }
        public string mobile { get; set; }
        public string country { get; set; }
        public int is_vip { get; set; }
        public int mark { get; set; }
        public string alert { get; set; }
        public int is_chang_passwd { get; set; }
        public int is_first_login { get; set; }
        public int bind_mobile { get; set; }
        public Face face { get; set; }
        public Cookie cookie { get; set; }
        public string from { get; set; }
        public object is_trusted { get; set; }
    }

    public class Face
    {
        public string face_l { get; set; }
        public string face_m { get; set; }
        public string face_s { get; set; }
    }

    //调用115下载请求
    public class Browser_115_Request
    {
        public long uid { get; set; }
        public string key { get; set; }
        public Param_Request param { get; set; }
        public int type { get; set; } = 1;
    }

    public class Param_Request
    {
        public List<Down_Request> list { get; set; }
        public int count { get; set; }
        public string ref_url { get; set; }
    }

    public class Down_Request
    {
        public string n;
        public string pc;
        public bool is_dir;
    }

    public class Cookie
    {
        public string UID { get; set; }
        public string CID { get; set; }
        public string SEID { get; set; }
    }

    public class WebFileInfo
    {
        public Datum[] data { get; set; }
        public int count { get; set; }
        public string data_source { get; set; }
        public int sys_count { get; set; }
        public int offset { get; set; }
        public int o { get; set; }
        public int limit { get; set; }
        public int page_size { get; set; }
        public string aid { get; set; }
        public string cid { get; set; }
        public int is_asc { get; set; }
        public string order { get; set; }
        public int star { get; set; }
        public int type { get; set; }
        public int r_all { get; set; }
        public int stdir { get; set; }
        public int cur { get; set; }
        public int fc_mix { get; set; }
        public string suffix { get; set; }
        public int min_size { get; set; }
        public int max_size { get; set; }
        public WebPath[] path { get; set; }
        public bool state { get; set; }
        public string error { get; set; }
        public int errNo { get; set; }
    }

    /// <summary>
    /// 存储Cid下面的文件列表
    /// </summary>
    public class StoreDatum
    {
        public string Cid { get; set; }
        public List<Datum> DatumList { get; set; }
    }

    public class Datum
    {
        public string fid { get; set; }
        public long uid { get; set; }
        public int aid { get; set; }
        public string cid { get; set; }
        public string n { get; set; }
        public long s { get; set; }
        public int sta { get; set; }
        public string pt { get; set; }
        public string pid { get; set; }
        public string pc { get; set; }
        public int p { get; set; }
        public int m { get; set; }
        public string t { get; set; }
        public int te { get; set; }
        public int tp { get; set; }
        public int d { get; set; }
        public int c { get; set; }
        public int sh { get; set; }
        public string e { get; set; }
        public string ico { get; set; }
        public string sha { get; set; }
        public string fdes { get; set; }
        public int q { get; set; }
        public int hdf { get; set; }
        public int fvs { get; set; }
        public Fl[] fl { get; set; }
        public string u { get; set; }
        public int iv { get; set; }
        public int current_time { get; set; }
        public int played_end { get; set; }
        public string last_time { get; set; }
        public int vdi { get; set; }
        public float play_long { get; set; }

        public override string ToString() => n;
    }

    public class Fl
    {
        public string id { get; set; }
        public string name { get; set; }
        public string sort { get; set; }
        public string color { get; set; }
        public int update_time { get; set; }
        public int create_time { get; set; }
    }

    public class WebPath
    {
        public string name { get; set; }
        public object aid { get; set; }
        public object cid { get; set; }
        public object pid { get; set; }
        public object isp { get; set; }
        public string p_cid { get; set; }
    }


    public class UserInfo
    {
        public bool state { get; set; }
        public UserData data { get; set; }
        public string error { get; set; }
    }

    /// <summary>
    /// 用户信息相关
    /// </summary>
    public class UserData
    {
        public int device { get; set; }
        public int rank { get; set; }
        public int liang { get; set; }

        /// <summary>
        /// 每个mark对应一个会员类型
        /// </summary>
        public int mark { get; set; }

        public int mark1 { get; set; }
        public int vip { get; set; }

        /// <summary>
        /// 会员到期时间
        /// </summary>
        public long expire { get; set; }
        public int global { get; set; }
        public int forever { get; set; }
        public bool is_privilege { get; set; }
        public Privilege privilege { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string user_name { get; set; }

        /// <summary>
        /// 头像图片地址
        /// </summary>
        public string face { get; set; }
        public int user_id { get; set; }

        public static string getVipName(int mark)
        {
            string vip_name = "";
            switch (mark)
            {
                case 1:
                    vip_name = "月费VIP";
                    break;
                case 11:
                    vip_name = "体验VIP";
                    break;
                case 127:
                    vip_name = "黄金VIP";
                    break;
                case 255:
                    vip_name = "铂金VIP";
                    break;
                case 1023:
                    vip_name = "年费VIP";
                    break;
                case 10239:
                    vip_name = "年费VIP";
                    break;
                case 1048575:
                    vip_name = "长期VIP";
                    break;
                case 15:
                    vip_name = "青铜会员";
                    break;
                case 7:
                    vip_name = "体验VIP";
                    break;
                case 3:
                    vip_name = "体验VIP";
                    break;
            }

            return vip_name;
        }
    }

    public class Privilege
    {
        public int start { get; set; }
        public long expire { get; set; }
        public bool state { get; set; }
        public int mark { get; set; }
    }

    public class QRCodeInfo
    {
        public int state { get; set; }
        public int code { get; set; }
        public string message { get; set; }
        public InfoData data { get; set; }
    }

    public class InfoData
    {
        public string uid { get; set; }
        public int time { get; set; }
        public string sign { get; set; }
        public string qrcode { get; set; }
    }

    public class QRCodeStatus
    {
        public int state { get; set; }
        public int code { get; set; }
        public string message { get; set; }
        public StatusInfo data { get; set; }
    }

    public class StatusInfo
    {
        public string msg { get; set; }
        public int status { get; set; }
        public string version { get; set; }
    }

    public class HiddenFileSwitch
    {
        public bool state { get; set; }
        public string error { get; set; }
        public int errno { get; set; }
        public string token { get; set; }
    }

    public class DisplayInfo
    {
        public string name { get; set; }
        public string size { get; set; }
        public string modifyTime { get; set; }
        public string coverImage { get; set; }
    }

    public class AccountContentInPage
    {
        public int ContentAcount { get; set; }
        //public bool IsSelected { get; set; } = false;
    }

    public class _115Setting
    {
        public bool state { get; set; }
        public string _goto { get; set; }
        public string even { get; set; }
        public SettingData data { get; set; }
        public bool flush { get; set; }
    }


    public class SettingData
    {
        public string asc_file { get; set; }
        public string order_file { get; set; }
        public string view_file { get; set; }
        public string language { get; set; }
        public string page_num { get; set; }
        public string isp_download { get; set; }
        public string isp_upload { get; set; }
        public string play_count { get; set; }
        public string show_ad { get; set; }
        public string ssl_download { get; set; }
        public string ssl_upload { get; set; }
        public string kc_open { get; set; }
        public string user_edit_info { get; set; }
        public string mobile { get; set; }
        public string mobile_set { get; set; }
        public string valid_type { get; set; }
        public string show { get; set; }
        public int movies { get; set; }
        public string movies_subtype { get; set; }
        public string theme { get; set; }
        public string theme_wl { get; set; }
        public int tutorial { get; set; }
        public string first_login { get; set; }
        public string show_hit { get; set; }
        public string forever { get; set; }
        public int natsort { get; set; }
        public int prav_tv_channels { get; set; }
        public string prav_tv_channels_1 { get; set; }
        public string root_folder { get; set; }
        public int video_progress_bar { get; set; }
        public int video_play_mode { get; set; }
        public string lang_pack { get; set; }
        public string play_speed { get; set; }
        public int album_cover { get; set; }
        public int allow_radar_protocol { get; set; }
        public string allow_link_protocol { get; set; }
        public string allow_pubshare_protocol { get; set; }
        public int allow_similarphoto_protocol { get; set; }
        public int is_popup_showed { get; set; }
        public int display_week { get; set; }
        public string allow_global_protocol { get; set; }
        public int fc_mix { get; set; }
        public string is_into_transfer { get; set; }
        public int skip_silence { get; set; }
        public string is_silent_hint { get; set; }
        public int first_photo_backup { get; set; }
        public string first_play_video { get; set; }
        public int first_upload { get; set; }
        public string first_download { get; set; }
        public string insert_default_file_label { get; set; }
        public string subtitle_color { get; set; }
        public int subtitle_position { get; set; }
        public int subtitle_size_type { get; set; }
        public int subtitle_web_size { get; set; }
        public string last_file_type { get; set; }
        public string last_file_id { get; set; }
        public int default_search_path { get; set; }
        public string range_options_guide { get; set; }
        public string include_music_order { get; set; }
        public int star_music_asc { get; set; }
        public int music_list_asc { get; set; }
    }

    public class CookieFormat
    {
        public string domain { get; set; } = ".115.com";
        public long expirationDate { get; set; } = DateTimeOffset.Now.AddMonths(1).ToUnixTimeSeconds();
        public bool hostOnly { get; set; } = false;
        public bool httpOnly { get; set; } = true;
        public string name { get; set; }
        public string path { get; set; } = "/";
        public string sameSite { get; set; } = null;
        public bool secure { get; set; } = false;
        public bool session { get; set; } = false;
        public string storeId { get; set; } = null;
        public string value { get; set; }

    }


    /// <summary>
    /// 视频详细信息
    /// </summary>
    public class VideoInfo : INotifyPropertyChanged
    {
        private string _truename;
        public string truename
        {
            get => _truename;
            set
            {
                _truename = value;
                OnPropertyChanged();
            }
        }

        public string title { get; set; }

        private string _releasetime;
        public string releasetime
        {
            get => _releasetime;
            set
            {
                _releasetime = value;
                OnPropertyChanged();
            }
        }

        private string _lengthtime;
        public string lengthtime
        {
            get => _lengthtime;
            set
            {
                _lengthtime = value;
                OnPropertyChanged();
            }
        }

        private string _director;
        public string director
        {
            get => _director;
            set
            {
                _director = value;
                OnPropertyChanged();
            }
        }

        private string _producer;
        public string producer
        {
            get => _producer;
            set
            {
                _producer = value;
                OnPropertyChanged();
            }
        }

        private string _publisher;
        public string publisher
        {
            get => _publisher;
            set
            {
                _publisher = value;
                OnPropertyChanged();
            }
        }

        private string _series;
        public string series
        {
            get => _series;
            set
            {
                _series = value;
                OnPropertyChanged();
            }
        }

        private string _category;
        public string category
        {
            get => _category;
            set
            {
                _category = value;
                OnPropertyChanged();
            }
        }

        private string _actor;
        public string actor
        {
            get => _actor;
            set
            {
                _actor = value;
                OnPropertyChanged();
            }

        }


        public string imageurl { get; set; }

        public string sampleImageList { get; set; }

        private string _imagepath;
        public string imagepath
        {
            get => _imagepath;
            set
            {
                string path = value;
                _imagepath = !string.IsNullOrEmpty(path) ? path : "ms-appx:///Assets/NoPicture.jpg";
                OnPropertyChanged();
            }
        }



        public string busurl { get; set; }

        //public long look_later { get; set; } = 0;
        private long _look_later = 0;
        public long look_later
        {
            get => _look_later;
            set
            {
                _look_later = value;
                OnPropertyChanged();
            }
        }

        private double _score = -1;
        public double score
        {
            get => _score;
            set
            {
                _score = value;
                OnPropertyChanged();
            }
        }

        //public int is_like { get; set; } = 0;
        private int _is_like = 0;
        public int is_like
        {
            get => _is_like;
            set
            {
                _is_like = value;
                OnPropertyChanged();
            }
        }

        public long addtime { get; set; } = DateTimeOffset.Now.ToUnixTimeSeconds();

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }

    /// <summary>
    /// 视频封面缩略信息
    /// </summary>
    public class VideoCoverDisplayClass : VideoInfo, INotifyPropertyChanged
    {
        public VideoCoverDisplayClass()
        {
            OnPropertyChanged();
        }

        public VideoCoverDisplayClass(VideoInfo videoinfo)
        {
            foreach (var VideoInfoItem in videoinfo.GetType().GetProperties())
            {
                var key = VideoInfoItem.Name;
                var value = VideoInfoItem.GetValue(videoinfo);

                var newItem = this.GetType().GetProperty(key);
                newItem.SetValue(this, value);
            }

            //标题
            title = videoinfo.title;

            //是否显示右上角的标签
            string category = videoinfo.category;
            Visibility isShowLabel = Visibility.Collapsed;
            string ShowLabel = "";
            if (category.IndexOf("VR") > 0 || videoinfo.series.IndexOf("VR") > 0)
            {
                isShowLabel = Visibility.Visible;
                ShowLabel = "VR";
            }
            else if (category.IndexOf("4K") > 0)
            {
                isShowLabel = Visibility.Visible;
                ShowLabel = "4K";
            }

            if (videoinfo.releasetime.Contains("/"))
            {
                this.realeaseYear = videoinfo.releasetime.Split('/')[0];
            }
            else
            {
                this.realeaseYear = videoinfo.releasetime.Split('-')[0];
            }
            this.isShowLabel = isShowLabel;
            this.ShowLabel = ShowLabel;
            this.score = videoinfo.score;
        }

        public string realeaseYear { get; set; }
        public Visibility isShowLabel { get; set; } = Visibility.Collapsed;
        public string ShowLabel { get; set; }

        //public bool isLookLater { get; set; }

        //public double score { get; set; }
        private Visibility _isDeleted = Visibility.Collapsed;
        public Visibility isDeleted
        {
            get => _isDeleted;
            set
            {
                _isDeleted = value;
                OnPropertyChanged();
            }
        }

        ////是否显示已删除
        //public Visibility isShowDeletedGrid(bool isDeleted)
        //{
        //    return isDeleted ? Visibility.Visible : Visibility.Collapsed;
        //}
        //private bool _islike;
        //public bool isLike
        //{
        //    get
        //    {
        //        return _islike;
        //    }
        //    set
        //    {
        //        _islike = value;
        //        OnPropertyChanged();
        //    }
        //}

        private int _imagewidth = 300;
        public int imagewidth
        {
            get
            {
                return _imagewidth;
            }
            set
            {
                _imagewidth = value;
                OnPropertyChanged();
            }
        }

        private int _imageheight = 200;
        public int imageheight
        {
            get
            {
                return _imageheight;
            }
            set
            {
                _imageheight = value;
                OnPropertyChanged();
            }
        }

        //public event PropertyChangedEventHandler PropertyChanged;
        //protected void RaisePropertyChanged([CallerMemberName] string name = "")
        //{
        //    if (PropertyChanged != null)
        //    {
        //        PropertyChanged(this, new PropertyChangedEventArgs(name));
        //    }
        //}

    }

    /// <summary>
    ///  封面展示（单次展示多张）
    /// </summary>
    public class CoverFlipItems
    {
        public ObservableCollection<VideoCoverDisplayClass> CoverItems;
    }

    /// <summary>
    ///  封面展示类别
    /// </summary>
    public class CoverItems
    {
        public string name;
        public string ImagePath;
    }

    /// <summary>
    /// 目录展示
    /// </summary>
    public class ExplorerItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        //public enum ExplorerItemType { Folder, File };
        public string Name { get; set; }
        public string Cid { get; set; }
        public bool HasUnrealizedChildren { get; set; }
        public FilesInfo.FileType Type { get; set; }
        private ObservableCollection<ExplorerItem> m_children;
        public ObservableCollection<ExplorerItem> Children
        {
            get
            {
                if (m_children == null)
                {
                    m_children = new ObservableCollection<ExplorerItem>();
                }
                return m_children;
            }
            set
            {
                m_children = value;
            }
        }

        public Datum datum;

        public string IconPath
        {
            get
            {
                return FilesInfo.getFileIcon(Type);
            }
            set
            {
                IconPath = value;
            }
        }

        private bool m_isExpanded;
        public bool IsExpanded
        {
            get { return m_isExpanded; }
            set
            {
                if (m_isExpanded != value)
                {
                    m_isExpanded = value;
                    NotifyPropertyChanged("IsExpanded");
                }
            }
        }

        private bool m_isSelected;
        public bool IsSelected
        {
            get { return m_isSelected; }

            set
            {
                if (m_isSelected != value)
                {
                    m_isSelected = value;
                    NotifyPropertyChanged("IsSelected");
                }
            }

        }

        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    /// <summary>
    /// 文件详情展示
    /// </summary>
    public class FilesInfo
    {
        public FilesInfo()
        {

        }
        public FilesInfo(Datum data)
        {
            this.datum= data;

            Name = data.n;
            Cid = data.cid;
            Fid = data.fid;

            IconPath = "ms-appx:///Assets/115/file_type/other/unknown.svg";
            //文件夹
            if (string.IsNullOrEmpty(data.fid))
            {
                Type = FileType.Folder;
                IconPath = "ms-appx:///Assets/115/file_type/folder/folder.svg";
            }
            else
            {
                Type = FileType.File;

                //视频文件
                if (data.iv == 1)
                {
                    string video_quality = GetVideoQualityFromVdi(data.vdi);

                    if (video_quality != null)
                    {
                        IconPath = $"ms-appx:///Assets/115/file_type/video_quality/{video_quality}.svg";
                    }
                    else
                    {
                        IconPath = "ms-appx:///Assets/115/file_type/video/video.svg";
                    }
                }
                else if (!string.IsNullOrEmpty(data.ico))
                {
                    var tmpIcoPath = getPathFromFileType(data.ico);
                    if (tmpIcoPath != null)
                    {
                        IconPath = tmpIcoPath;
                    }
                }
            }
        }

        public Datum datum;

        public static string GetVideoQualityFromVdi(int vdi)
        {
            string video_quality = null;
            switch (vdi)
            {
                case 1:
                    video_quality = "sd";
                    //IconPath = "ms-appx:///Assets/115/file_type/video_quality/sd.svg";
                    break;
                case 2:
                    video_quality = "hd";
                    //IconPath = "ms-appx:///Assets/115/file_type/video_quality/hd.svg";
                    break;
                case 3:
                    video_quality = "fhd";
                    //IconPath = "ms-appx:///Assets/115/file_type/video_quality/fhd.svg";
                    break;
                case 4:
                    video_quality = "1080p";
                    //IconPath = "ms-appx:///Assets/115/file_type/video_quality/1080p.svg";
                    break;
                case 5:
                    video_quality = "4k";
                    //IconPath = "ms-appx:///Assets/115/file_type/video_quality/4k.svg";
                    break;
                case 100:
                    video_quality = "origin";
                    //IconPath = "ms-appx:///Assets/115/file_type/video_quality/origin.svg";
                    break;
            }
            return video_quality;
        }

        //public event PropertyChangedEventHandler PropertyChanged;

        public enum FileType { Folder, File };
        public string Name { get; set; }
        public string Cid { get; set; }
        public string Fid { get; set; }
        public FileType Type { get; set; }
        public string IconPath { get; set; }

        //    private void NotifyPropertyChanged(string propertyName)
        //    {
        //        if (PropertyChanged != null)
        //        {
        //            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        //        }
        //    }

        public static string getPathFromFileType(string ico)
        {
            string IconPath = "ms-appx:///Assets/115/file_type/other/unknown.svg";
            var fileType = new Dictionary<string, Dictionary<string, List<string>>>()
            {
                { "application", new Dictionary<string, List<string>>(){
                    { "apk",new List<string>(){"apk"}  },
                    { "bat",new List<string>(){ "bat" }  },
                    { "exe",new List<string>(){ "exe" }  },
                    { "ipa",new List<string>(){ "ipa" }  },
                    { "msi",new List<string>(){"msi"}  },
                } },
                { "archive", new Dictionary<string, List<string>>(){
                    {"7z",new List<string>(){"7z"} },
                    {"cab",new List<string>(){"cab"} },
                    {"dmg",new List<string>(){"dmg"} },
                    {"iso",new List<string>(){"iso"} },
                    {"rar",new List<string>(){"rar", "tar"} },
                    {"zip",new List<string>(){"zip"} },
                } },
                { "audio", new Dictionary<string, List<string>>(){
                    {"ape",new List<string>(){"ape"} },
                    {"audio",new List<string>(){"wav","midi","mid","flac","aac","m4a","ogg","amr"}},
                    {"mp3",new List<string>(){"mp3"} },
                    {"wma",new List<string>(){"wma"} },
                } },
                { "code", new Dictionary<string, List<string>>(){
                    {"code",new List<string>(){ "c", "cpp", "asp", "js", "php", "tpl", "xml", "h", "cs", "plist", "py", "rb" } },
                    {"css",new List<string>(){ "css", "sass", "scss", "less" } },
                    {"html",new List<string>(){ "htm", "html" } },
                } },
                { "document", new Dictionary<string, List<string>>(){
                    {"ass",new List<string>(){"ass"} },
                    {"chm",new List<string>(){"chm"} },
                    {"doc",new List<string>(){ "doc", "docx", "docm", "dot", "dotx", "dotm" } },
                    {"key",new List<string>(){"key"} },
                    {"log",new List<string>(){"log"} },
                    {"numbers",new List<string>(){"numbers"} },
                    {"pages",new List<string>(){"pages"} },
                    {"pdf",new List<string>(){"pdf"} },
                    {"ppt",new List<string>(){ "ppt", "pptx", "pptm", "pps", "pot" } },
                    {"srt",new List<string>(){"srt"} },
                    {"ssa",new List<string>(){"ssa"} },
                    {"torrent",new List<string>(){"torrent"} },
                    {"txt",new List<string>(){"txt"} },
                    {"xls",new List<string>(){ "xls", "xlsx", "xlsm", "xltx", "xltm", "xlam", "xlsb" } },
                } },
                { "image", new Dictionary<string, List<string>>(){
                    {"gif",new List<string>(){"gif"} },
                    {"img",new List<string>(){ "bmp", "tiff", "exif" } },
                    {"jpg",new List<string>(){"jpg"} },
                    {"png",new List<string>(){"png"} },
                    {"raw",new List<string>(){"raw"} },
                    {"svg",new List<string>(){ "svg" } }
                } },
                { "source", new Dictionary<string, List<string>>(){
                    {"ai",new List<string>(){ "ai" } },
                    {"fla",new List<string>(){"fla"} },
                    {"psd",new List<string>(){"psd"} },
                } },
                { "video", new Dictionary<string, List<string>>(){
                    {"3gp",new List<string>(){ "3g2", "3gp", "3gp2", "3gpp" } },
                    {"flv",new List<string>(){"flv"} },
                    {"mkv",new List<string>(){"mkv"} },
                    {"mov",new List<string>(){"mov"} },
                    {"mp4",new List<string>(){ "mp4", "mpeg4" } },
                    {"rm",new List<string>(){"rm"} },
                    {"rmvb",new List<string>(){"rmvb"} },
                    {"swf",new List<string>(){"swf"} },
                    {"video",new List<string>(){ "mpe", "mpeg", "mpg", "asf", "ram", "m4v", "vob", "divx", "webm" } },
                    {"wmv",new List<string>(){"wmv"} },
                    {"mts",new List<string>(){"mts"} },
                    {"mpg",new List<string>(){"mpg"} },
                    {"dat",new List<string>(){"dat"} },
                } },
                { "folder", new Dictionary<string, List<string>>(){
                    {"folder",new List<string>(){ "folder" } }
                } },
            };

            bool isMatch = false;
            foreach (var kvp in fileType)
            {
                if (isMatch)
                    break;

                string key = kvp.Key;
                var values = kvp.Value;

                foreach (var kvp2 in values)
                {
                    if (isMatch)
                        break;

                    string key2 = kvp2.Key;
                    List<string> values2 = kvp2.Value;

                    foreach (string key3 in values2)
                    {
                        if (key3 == ico)
                        {
                            string tmpPath = $"Assets/115/file_type/{key}/{key2}.svg";
                            
                            if (!File.Exists(Path.Combine(Package.Current.InstalledLocation.Path, tmpPath)))
                                IconPath = $"ms-appx:///Assets/115/file_type/{key}/{key}.svg";
                            else
                                IconPath = $"ms-appx:///{tmpPath}";


                            isMatch = true;
                            break;
                        }
                    }
                }
            }

            return IconPath;
        }


        public static string getFileIcon(FileType fileType)
        {
            string IconUrl = "ms-appx:///Assets/115/file_type/other/unknown.svg";

            switch (fileType)
            {
                case FileType.Folder:
                    IconUrl = "ms-appx:///Assets/115/file_type/folder/folder.svg";
                    break;
                case FileType.File:
                    IconUrl = "ms-appx:///Assets/115/file_type/other/unknown.svg";
                    break;

            }

            return IconUrl;
        }

    }

    /// <summary>
    /// 状态
    /// </summary>
    public enum Status { doing, success, error, pause, beforeStart };

    /// <summary>
    /// 浏览器选择项
    /// </summary>
    public class SelectedItem
    {
        public string id { get; set; }
        public string name { get; set; }
        public int file_count { get; set; }
        public int folder_count { get; set; }
        public string size { get; set; }
        public string pick_code { get; set; }
        public int file_type { get; set; }
        public string file_id { get; set; }

        /// <summary>
        /// 是否有隐藏文件，有为1，无为0
        /// </summary>
        public int hasHiddenFile { get; set; }
    }

    public class FolderCategory
    {
        /// <summary>
        /// 文件数量（不包括文件夹）
        /// </summary>
        public int count { get; set; }
        public string size { get; set; }
        /// <summary>
        /// 文件夹数量（不包括文件）
        /// </summary>
        public int folder_count { get; set; }
        public int show_play_long { get; set; }
        public int ptime { get; set; }
        public int utime { get; set; }
        public int is_share { get; set; }
        public string file_name { get; set; }
        public string pick_code { get; set; }
        public string sha1 { get; set; }
        public int is_mark { get; set; }
        public int open_time { get; set; }
        public string desc { get; set; }
        public int file_category { get; set; }
        public ParentPath[] paths { get; set; }

        public int allCount
        {
            get
            {
                return count + folder_count;
            }
            set
            {
                allCount = value;
            }
        }


        public static Datum ConvertFolderToDatum(FolderCategory folderCategory, string cid)
        {
            Datum datum = new()
            {
                uid = 0,
                aid = 1,
                cid = cid,
                n = folderCategory.file_name,
                pid = folderCategory.paths[folderCategory.paths.Length - 1].file_id,
                pc = folderCategory.pick_code,
                t = FileMatch.ConvertInt32ToDateTime(folderCategory.utime),
                tp = folderCategory.ptime,
                te = folderCategory.utime,
            };

            return datum;
        }
    }

    public class ParentPath
    {
        public string file_id { get; set; }
        public string file_name { get; set; }
    }

    //115导入数据库的进度信息
    public enum ProgressStatus { normal, error, done, cancel }
    //报告文件数量
    public class GetFilesProgressInfo
    {
        public int FolderCount { get; set; } = 0;
        public int FilesCount { get; set; } = 0;
        public int AllCount
        {
            get
            {
                return FolderCount + FilesCount;
            }
            set
            {
                AllCount = value;
            }
        }
        public List<string> FailCid { get; set; } = new();

        public List<Datum> addToDataAccessList = new();
    }
    //报告进度和状态
    public class GetFileProgessIProgress
    {
        public ProgressStatus status { get; set; } = ProgressStatus.normal;
        public GetFilesProgressInfo getFilesProgressInfo { get; set; }
        public long sendCountPerMinutes { get; set; }
    }

    // 视频文件匹配结果
    public class MatchVideoResult
    {
        public bool status;
        public int statusCode;
        public string message;
        public string OriginalName;
        public string MatchName;
    }

    public class tsInfo
    {
        public double Second;

        public string Url;

    }

    public class m3u8Info
    {
        public m3u8Info(string name, string bandwidth, string resolution, string url)
        {
            Name = name;
            Bandwidth = bandwidth;
            Resolution = resolution;
            Url = url;
        }

        public string Name { get; set; }
        public string Bandwidth { get; set; }
        public string Resolution { get; set; }
        public string Url { get; set; }

        public double TotalSecond
        {
            get
            {
                return ts_info_list.Sum(x => x.Second);
            }
        }

        public string BaseUrl
        {
            get
            {
                var urlInfo = new Uri(Url);

                return $"{urlInfo.Scheme}://{urlInfo.Host}";
            }
        }

        public List<tsInfo> ts_info_list = new();

    }

    //真实下载链接信息
    public class DownUrlBase64EncryptInfo
    {
        public bool state { get; set; }
        public string msg { get; set; }
        public int errno { get; set; }
        public string data { get; set; }
    }

    //演员信息
    public class ActorsInfo : INotifyPropertyChanged
    {
        public string name;
        public int count;

        private string _prifilePhotoPath;
        //private string _prifilePhotoPath;
        public string prifilePhotoPath
        {
            get
            {
                if (string.IsNullOrEmpty(_prifilePhotoPath))
                {
                    //初始化
                    _prifilePhotoPath = "ms-appx:///Assets/NoPicture.jpg";

                    //检查演员图片是否存在
                    string imagePath = Path.Combine(AppSettings.ActorInfo_SavePath, name, "face.jpg");
                    if (File.Exists(imagePath))
                    {
                        _prifilePhotoPath = imagePath;
                    }
                }

                return _prifilePhotoPath;
            }
            set
            {
                _prifilePhotoPath = value;
                OnPropertyChanged();
            }
        }

        private Status _status = Status.beforeStart;
        public Status Status
        {
            get
            {
                return _status;
            }
            set
            {
                _status = value;
                OnPropertyChanged();
            }
        }

        private string _genderInfo;
        public string genderInfo
        {
            get
            {
                return _genderInfo;
            }
            set
            {
                _genderInfo = value;
                OnPropertyChanged();
            }
        }

        private string _ageInfo;
        public string ageInfo
        {
            get
            {
                return _ageInfo;
            }
            set
            {
                _ageInfo = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    //缩略图信息
    public class ThumbnailInfo : INotifyPropertyChanged
    {
        public ThumbnailInfo(VideoInfo videoinfo)
        {
            name = videoinfo.truename;

            var tmpList = videoinfo.sampleImageList.Split(',').ToList();
            if (tmpList.Count > 1)
            {
                thumbnailDownUrlList = tmpList;
            }

            if (videoinfo.category.Contains("VR") || videoinfo.series.Contains("VR"))
            {
                isVr = true;
            }
        }

        public bool isVr = false;

        public string name;
        public int count;

        private string _PhotoPath = "ms-appx:///Assets/Svg/picture-o.svg";
        //private string _prifilePhotoPath;
        public string PhotoPath
        {
            get
            {
                return _PhotoPath;
            }
            set
            {
                _PhotoPath = value;
                OnPropertyChanged();
            }
        }

        public List<string> thumbnailDownUrlList = new();

        private Status _status = Status.beforeStart;
        public Status Status
        {
            get
            {
                return _status;
            }
            set
            {
                _status = value;
                OnPropertyChanged();
            }
        }

        private string _genderInfo;
        public string genderInfo
        {
            get
            {
                return _genderInfo;
            }
            set
            {
                _genderInfo = value;
                OnPropertyChanged();
            }
        }

        private string _ageInfo;
        public string ageInfo
        {
            get
            {
                return _ageInfo;
            }
            set
            {
                _ageInfo = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    //使用OpenCv获取115在线视频缩略图
    public class VideoToThumbnail
    {
        public string name;

        //总时长
        public float play_long { get; set; } = 0;

        //总帧数
        public double frame_count { get; set; } = 0;

        //pickCode
        public List<string> pickCodeList { get; set; } = new();

    }

    public class Aria2Request
    {
        public string jsonrpc { get; set; }
        public string method { get; set; }
        public string id { get; set; }
        public string[] _params { get; set; }
    }


    public class Aria2GlobalOptionRequest
    {
        public string id { get; set; }
        public string jsonrpc { get; set; }
        public Aria2GlobalOptionRequestResult result { get; set; }
    }

    public class Aria2GlobalOptionRequestResult
    {
        public string allowoverwrite { get; set; }
        public string allowpiecelengthchange { get; set; }
        public string alwaysresume { get; set; }
        public string asyncdns { get; set; }
        public string autofilerenaming { get; set; }
        public string autosaveinterval { get; set; }
        public string btdetachseedonly { get; set; }
        public string btenablehookafterhashcheck { get; set; }
        public string btenablelpd { get; set; }
        public string btforceencryption { get; set; }
        public string bthashcheckseed { get; set; }
        public string btloadsavedmetadata { get; set; }
        public string btmaxopenfiles { get; set; }
        public string btmaxpeers { get; set; }
        public string btmetadataonly { get; set; }
        public string btmincryptolevel { get; set; }
        public string btremoveunselectedfile { get; set; }
        public string btrequestpeerspeedlimit { get; set; }
        public string btrequirecrypto { get; set; }
        public string btsavemetadata { get; set; }
        public string btseedunverified { get; set; }
        public string btstoptimeout { get; set; }
        public string bttrackerconnecttimeout { get; set; }
        public string bttrackerinterval { get; set; }
        public string bttrackertimeout { get; set; }
        public string cacertificate { get; set; }
        public string checkcertificate { get; set; }
        public string checkintegrity { get; set; }
        public string conditionalget { get; set; }
        public string confpath { get; set; }
        public string connecttimeout { get; set; }
        public string consoleloglevel { get; set; }
        public string contentdispositiondefaultutf8 { get; set; }
        public string _continue { get; set; }
        public string daemon { get; set; }
        public string deferredinput { get; set; }
        public string dhtfilepath { get; set; }
        public string dhtfilepath6 { get; set; }
        public string dhtlistenport { get; set; }
        public string dhtmessagetimeout { get; set; }
        public string dir { get; set; }
        public string disableipv6 { get; set; }
        public string diskcache { get; set; }
        public string downloadresult { get; set; }
        public string dryrun { get; set; }
        public string dscp { get; set; }
        public string enablecolor { get; set; }
        public string enabledht { get; set; }
        public string enabledht6 { get; set; }
        public string enablehttpkeepalive { get; set; }
        public string enablehttppipelining { get; set; }
        public string enablemmap { get; set; }
        public string enablepeerexchange { get; set; }
        public string enablerpc { get; set; }
        public string eventpoll { get; set; }
        public string fileallocation { get; set; }
        public string followmetalink { get; set; }
        public string followtorrent { get; set; }
        public string forcesave { get; set; }
        public string ftppasv { get; set; }
        public string ftpreuseconnection { get; set; }
        public string ftptype { get; set; }
        public string hashcheckonly { get; set; }
        public string help { get; set; }
        public string httpacceptgzip { get; set; }
        public string httpauthchallenge { get; set; }
        public string httpnocache { get; set; }
        public string humanreadable { get; set; }
        public string keepunfinisheddownloadresult { get; set; }
        public string listenport { get; set; }
        public string loglevel { get; set; }
        public string lowestspeedlimit { get; set; }
        public string maxconcurrentdownloads { get; set; }
        public string maxconnectionperserver { get; set; }
        public string maxdownloadlimit { get; set; }
        public string maxdownloadresult { get; set; }
        public string maxfilenotfound { get; set; }
        public string maxmmaplimit { get; set; }
        public string maxoveralldownloadlimit { get; set; }
        public string maxoveralluploadlimit { get; set; }
        public string maxresumefailuretries { get; set; }
        public string maxtries { get; set; }
        public string maxuploadlimit { get; set; }
        public string metalinkenableuniqueprotocol { get; set; }
        public string metalinkpreferredprotocol { get; set; }
        public string minsplitsize { get; set; }
        public string mintlsversion { get; set; }
        public string netrcpath { get; set; }
        public string noconf { get; set; }
        public string nofileallocationlimit { get; set; }
        public string nonetrc { get; set; }
        public string optimizeconcurrentdownloads { get; set; }
        public string parameterizeduri { get; set; }
        public string pausemetadata { get; set; }
        public string peeragent { get; set; }
        public string peeridprefix { get; set; }
        public string piecelength { get; set; }
        public string proxymethod { get; set; }
        public string quiet { get; set; }
        public string realtimechunkchecksum { get; set; }
        public string remotetime { get; set; }
        public string removecontrolfile { get; set; }
        public string retrywait { get; set; }
        public string reuseuri { get; set; }
        public string rlimitnofile { get; set; }
        public string rpcalloworiginall { get; set; }
        public string rpclistenall { get; set; }
        public string rpclistenport { get; set; }
        public string rpcmaxrequestsize { get; set; }
        public string rpcsaveuploadmetadata { get; set; }
        public string rpcsecure { get; set; }
        public string savenotfound { get; set; }
        public string savesession { get; set; }
        public string savesessioninterval { get; set; }
        public string seedratio { get; set; }
        public string serverstattimeout { get; set; }
        public string showconsolereadout { get; set; }
        public string showfiles { get; set; }
        public string socketrecvbuffersize { get; set; }
        public string split { get; set; }
        public string stderr { get; set; }
        public string stop { get; set; }
        public string streampieceselector { get; set; }
        public string summaryinterval { get; set; }
        public string timeout { get; set; }
        public string truncateconsolereadout { get; set; }
        public string uriselector { get; set; }
        public string usehead { get; set; }
        public string useragent { get; set; }
    }


    //FC2的格式
    public class FcJson
    {
        public string context { get; set; }
        public string type { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string image { get; set; }
        public string[] identifier { get; set; }
        public string datePublished { get; set; }
        public string duration { get; set; }
        public object[] actor { get; set; }
        public string[] genre { get; set; }
        public string[] sameAs { get; set; }
        public string director { get; set; }
        public Aggregaterating aggregateRating { get; set; }
    }

    public class Aggregaterating
    {
        public string type { get; set; }
        public int bestRating { get; set; }
        public int worstRating { get; set; }
        public int ratingCount { get; set; }
        public int ratingValue { get; set; }
    }

    //下载链接历史
    public class DownInfo
    {
        public string pickCode { get; set; }
        public string fileName { get; set; }
        public string trueUrl { get; set; }
        public string ua { get; set; }
        public long addTime { get; set; } = DateTimeOffset.Now.ToUnixTimeSeconds();
    }

    //字幕文件信息
    public class SubInfo
    {
        public string pickcode { get; set; }
        public string name { get; set; }
        public string fileBelongPickcode { get; set; }

        public SubInfo(string pickcode, string name, string fileBelongPickcode)
        {
            this.pickcode = pickcode;
            this.name = name;
            this.fileBelongPickcode = fileBelongPickcode;
        }

        public SubInfo()
        {

        }
    }
}
