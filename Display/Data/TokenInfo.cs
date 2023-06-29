using ByteSizeLib;
using Display.Services.Upload;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using static Display.Data.FilesInfo;

namespace Display.Data
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

    public class FilesShowInfo
    {
        public int count { get; set; }
        public int errNo { get; set; }
        public string error { get; set; }
        public int fc_mix { get; set; }
        public int is_asc { get; set; }
        public string order { get; set; }
        public bool state { get; set; }

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
        public long Cid { get; set; }
        public Datum[] DatumList { get; set; }
    }

    public class Datum
    {
        [JsonProperty(propertyName: "fid")]
        public long? Fid { get; set; }


        [JsonProperty(propertyName: "pid")]
        public long? Pid { get; set; }


        [JsonProperty(propertyName: "cid")]
        public long Cid { get; set; }

        [JsonProperty(propertyName: "uid")]
        public long Uid { get; set; }


        [JsonProperty(propertyName: "aid")]
        public int Aid { get; set; }

        [JsonProperty(propertyName: "n")]
        public string Name { get; set; }

        [JsonProperty(propertyName: "s")]
        public long Size { get; set; }

        [JsonProperty(propertyName: "sta")]
        public int Sta { get; set; }


        [JsonProperty(propertyName: "pt")]
        public string Pt { get; set; }

        [JsonProperty(propertyName: "pc")]
        public string PickCode { get; set; }


        [JsonProperty(propertyName: "p")]
        public int P { get; set; }


        [JsonProperty(propertyName: "m")]
        public int M { get; set; }

        [JsonProperty(propertyName: "t")]
        public string Time { get; set; }


        [JsonProperty(propertyName: "te")]
        public int TimeEdit { get; set; }

        [JsonProperty(propertyName: "tp")]
        public int TimeProduce { get; set; }

        [JsonProperty(propertyName: "d")]
        public int D { get; set; }

        [JsonProperty(propertyName: "c")]
        public int C { get; set; }


        [JsonProperty(propertyName: "sh")]
        public int Sh { get; set; }

        [JsonProperty(propertyName: "e")]
        public string E { get; set; }


        [JsonProperty(propertyName: "ico")]
        public string Ico { get; set; }


        [JsonProperty(propertyName: "sha")]
        public string Sha { get; set; }


        [JsonProperty(propertyName: "fdes")]
        public string Fdes { get; set; }


        [JsonProperty(propertyName: "q")]
        public int Q { get; set; }


        [JsonProperty(propertyName: "hdf")]
        public int Hdf { get; set; }


        [JsonProperty(propertyName: "fvs")]
        public int Fvs { get; set; }


        [JsonProperty(propertyName: "fl")]
        public Fl[] Fl { get; set; }


        [JsonProperty(propertyName: "u")]
        public string U { get; set; }


        [JsonProperty(propertyName: "iv")]
        public int Iv { get; set; }


        [JsonProperty(propertyName: "current_time")]
        public int CurrentTime { get; set; }


        [JsonProperty(propertyName: "played_end")]
        public int PlayedEnd { get; set; }


        [JsonProperty(propertyName: "last_time")]
        public string LastTime { get; set; }


        [JsonProperty(propertyName: "vdi")]
        public int Vdi { get; set; }


        [JsonProperty(propertyName: "play_long")]
        public double PlayLong { get; set; }

        public override string ToString() => Name;
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
        public long cid { get; set; }
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

    public class ActorInfo : INotifyPropertyChanged
    {
        [JsonProperty(propertyName: "id")]
        public int Id { get; set; }

        [JsonProperty(propertyName: "name")]
        public string Name { set; get; }

        public List<string> OtherNames { get; set; }

        [JsonProperty(propertyName: "is_woman")]
        public int IsWoman { set; get; } = 1;

        [JsonProperty(propertyName: "birthday")]
        public string Birthday { set; get; } = string.Empty;
        
        [JsonProperty(propertyName: "bwh")]
        public string Bwh { set; get; } = string.Empty;

        [JsonProperty(propertyName: "bust")]
        public int Bust { set; get; }

        [JsonProperty(propertyName: "waist")]
        public int Waist { set; get; }

        [JsonProperty(propertyName: "hips")]
        public int Hips { set; get; }

        [JsonProperty(propertyName: "height")]
        public int Height { set; get; }

        [JsonProperty(propertyName: "works_count")]
        public int WorksCount { set; get; }

        [JsonProperty(propertyName: "work_time")]
        public string WorkTime { set; get; } = string.Empty;

        private string _profilePath { set; get; } = string.Empty;

        [JsonProperty(propertyName: "prifile_path")]
        public string ProfilePath
        {
            get => _profilePath;
            set
            {
                var path = !string.IsNullOrEmpty(value) ? value : Const.Common.NoPicturePath;
                if (_profilePath == path) return;

                _profilePath = path;

                OnPropertyChanged();
            }
        }

        [JsonProperty(propertyName: "blog_url")]
        public string BlogUrl { set; get; } = string.Empty;

        [JsonProperty(propertyName: "info_url")]
        public string InfoUrl { get; set; } = string.Empty;

        [JsonProperty(propertyName: "is_like")]
        public int IsLike { set; get; } = 0;

        [JsonProperty(propertyName: "addtime")]
        public long AddTime { set; get; } = DateTimeOffset.Now.ToUnixTimeSeconds();

        [JsonProperty(propertyName: "video_count")]
        public int VideoCount { get; set; }

        public string ImageUrl { get; set; }

        private Status _status = Status.BeforeStart;

        public Status Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged();
            }
        }

        private string _genderInfo;
        
        public string GenderInfo
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

        public string AgeInfo
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

        public string Initials => string.Empty + Name.FirstOrDefault();

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }


    /// <summary>
    /// 视频详细信息
    /// </summary>
    public class VideoInfo : INotifyPropertyChanged
    {
        public VideoInfo()
        {

        }

        //失败的文件信息（Datum）到VideoInfo
        //魔改
        public VideoInfo(Datum failDatum)
        {
            this.truename = failDatum.Name;
            this.imagepath = "ms-appx:///Assets/Fail.jpg";
            this.series = "fail";
            this.releasetime = failDatum.Time;
            this.imageurl = failDatum.PickCode;
            this.look_later = failDatum.Size;
            this.busurl = failDatum.PickCode;
            this.category = failDatum.Ico;
        }

        private string _truename;

        [JsonProperty(propertyName: "truename")]
        public string truename
        {
            get => _truename;
            set
            {
                _truename = value;
                OnPropertyChanged();
            }
        }

        [JsonProperty(propertyName: "title")]
        public string title { get; set; }

        private string _releasetime;

        [JsonProperty(propertyName: "releasetime")]
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

        [JsonProperty(propertyName: "lengthtime")]
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

        [JsonProperty(propertyName: "director")]
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

        [JsonProperty(propertyName: "producer")]
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

        [JsonProperty(propertyName: "publisher")]
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

        [JsonProperty(propertyName: "series")]
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

        [JsonProperty(propertyName: "category")]
        public string category
        {
            get => _category;
            set
            {
                _category = value;
                OnPropertyChanged();
            }
        }

        private string _actor { get; set; } = string.Empty;

        [JsonProperty(propertyName: "actor")]
        public string actor
        {
            get => _actor;
            set
            {
                _actor = value;
                OnPropertyChanged();
            }

        }

        private string _imageurl;

        [JsonProperty(propertyName: "imageurl")]
        public string imageurl
        {
            get => _imageurl;
            set
            {
                if (_imageurl == value) return;
                _imageurl = value;
                OnPropertyChanged();
            }
        }


        [JsonProperty(propertyName: "sampleImageList")]
        public string sampleImageList { get; set; }

        private string _imagepath;

        [JsonProperty(propertyName: "imagepath")]
        public string imagepath
        {
            get => _imagepath;
            set
            {
                if (_imagepath == value) return;

                string path = value;
                _imagepath = !string.IsNullOrEmpty(path) ? path : Const.Common.NoPicturePath;
                OnPropertyChanged();
            }
        }


        [JsonProperty(propertyName: "busurl")]
        public string busurl { get; set; }

        private long _look_later = 0;

        [JsonProperty(propertyName: "look_later")]
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

        [JsonProperty(propertyName: "score")]
        public double score
        {
            get => _score;
            set
            {
                _score = value;
                OnPropertyChanged();
            }
        }


        private int _is_like = 0;

        [JsonProperty(propertyName: "is_like")]
        public int is_like
        {
            get => _is_like;
            set
            {
                _is_like = value;
                OnPropertyChanged();
            }
        }

        [JsonProperty(propertyName: "addtime")]
        public long addtime { get; set; } = DateTimeOffset.Now.ToUnixTimeSeconds();
        public int is_wm { get; set; } = -1;


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

        public VideoCoverDisplayClass(VideoInfo videoinfo, double imgwidth, double imgheight)
        {
            foreach (var videoInfoItem in videoinfo.GetType().GetProperties())
            {
                var key = videoInfoItem.Name;
                var value = videoInfoItem.GetValue(videoinfo);

                var newItem = this.GetType().GetProperty(key);
                newItem.SetValue(this, value);
            }

            //标题
            title = videoinfo.title;

            //是否显示右上角的标签
            string category = videoinfo.category;
            Visibility isShowLabel = Visibility.Collapsed;

            string ShowLabel = string.Empty;
            if (!string.IsNullOrEmpty(category))
            {
                if (category.Contains("VR") || (!string.IsNullOrEmpty(videoinfo.series) && videoinfo.series.Contains("VR")))
                {
                    isShowLabel = Visibility.Visible;
                    ShowLabel = "VR";
                }
                else if (category.Contains("4K"))
                {
                    isShowLabel = Visibility.Visible;
                    ShowLabel = "4K";
                }
            }

            if (!string.IsNullOrEmpty(releasetime))
            {
                if (videoinfo.releasetime.Contains("/"))
                {
                    this.realeaseYear = videoinfo.releasetime.Split('/')[0];
                }
                else
                {
                    this.realeaseYear = videoinfo.releasetime.Split('-')[0];
                }
            }

            this.isShowLabel = isShowLabel;
            this.ShowLabel = ShowLabel;
            this.score = videoinfo.score;

            //图片大小
            this.imageheight = imgheight;
            this.ImageWidth = imgwidth;
        }

        public string realeaseYear { get; set; }
        public Visibility isShowLabel { get; set; } = Visibility.Collapsed;
        public string ShowLabel { get; set; }

        private Visibility _isDeleted = Visibility.Collapsed;
        public Visibility IsDeleted
        {
            get => _isDeleted;
            set
            {
                _isDeleted = value;
                OnPropertyChanged();
            }
        }

        private double _imageWidth;
        public double ImageWidth
        {
            get => _imageWidth;
            set
            {
                _imageWidth = value;
                OnPropertyChanged();
            }
        }

        private double _imageheight;
        public double imageheight
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
        //protected void RaisePropertyChanged([CallerMemberName] string Name = "")
        //{
        //    if (PropertyChanged != null)
        //    {
        //        PropertyChanged(this, new PropertyChangedEventArgs(Name));
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
        public string Name { get; set; }
        public long Id { get; set; }
        public bool HasUnrealizedChildren { get; set; }
        public FileType Type { get; set; }
        private ObservableCollection<ExplorerItem> m_children;
        public ObservableCollection<ExplorerItem> Children
        {
            get => m_children ??= new ObservableCollection<ExplorerItem>();
            set
            {
                m_children = value;
            }
        }

        public Datum datum;

        private string _iconPath;
        public string IconPath
        {
            get
            {
                if (_iconPath != null) return _iconPath;
                _iconPath = GetFileIconFromType(Type);

                return _iconPath;
            }
            set => _iconPath = value;
        }

        private bool m_isExpanded;
        public bool IsExpanded
        {
            get => m_isExpanded;
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
            get => m_isSelected;

            set
            {
                if (m_isSelected == value) return;
                m_isSelected = value;
                NotifyPropertyChanged(nameof(IsSelected));
            }

        }

        public event PropertyChangedEventHandler PropertyChanged;
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
    public class FilesInfo : INotifyPropertyChanged
    {
        public readonly Datum Datum;
        public readonly long? Id;
        public readonly long Cid;
        public readonly string PickCode;
        public readonly FileType Type;
        public readonly string IconPath;
        public readonly int Time;
        public readonly bool IsVideo;
        public readonly bool NoId;

        public FilesInfo(FileUploadResult result)
        {
            Name = result.Name;
            Id = result.Id;
            Cid = result.Cid;
            PickCode = result.PickCode;
            Type = result.Type;
            IsVideo = result.IsVideo;
            NoId = Id ==null;
            Time = (int)DateTimeOffset.Now.ToUnixTimeSeconds();

            IconPath = GetPathFromFileType(Name.Split(".")[^1]);

            // TODO 应该逐渐抛弃Datum
            //var isFile = Type == FileType.File;
            //Datum = new Datum
            //{
            //    fid = isFile ? result.Id : string.Empty,
            //    cid = isFile ? result.Cid.ToString() : result.Id,
            //    pid = isFile ? string.Empty : result.Cid.ToString(),
            //    n = result.Name,
            //    s = result.FileSize,
            //    pc = result.PickCode,
            //    sha = result.Sha1,
            //    u = result.ThumbUrl,
            //    aid = result.Aid,
            //    iv = result.IsVideo ? 1 : 0,
            //    te = result.Time,
            //    tp = result.Time,
            //    t = FileMatch.ConvertInt32ToDateTime(result.Time)
            //};
        }

        public FilesInfo(Datum data)
        {
            Datum = data;

            Name = data.Name;

            Time = data.TimeEdit;

            IconPath = "ms-appx:///Assets/115/file_type/other/unknown.svg";
            //文件夹
            if (data.Fid == null && data.Pid!=null)
            {
                Type = FileType.Folder;
                Id = data.Cid;
                Cid = (long)data.Pid;
                PickCode = data.PickCode;

                IconPath = "ms-appx:///Assets/115/file_type/folder/folder.svg";
            }
            else if(data.Fid != null)
            {
                Type = FileType.File;
                Id = data.Fid;
                Cid = data.Cid;
                PickCode = data.PickCode;

                //视频文件
                if (data.Iv == 1)
                {
                    IsVideo = true;
                    var videoQuality = GetVideoQualityFromVdi(data.Vdi);

                    IconPath = videoQuality != null ? $"ms-appx:///Assets/115/file_type/video_quality/{videoQuality}.svg"
                                                    : "ms-appx:///Assets/115/file_type/video/video.svg";

                }
                else if (!string.IsNullOrEmpty(data.Ico))
                {
                    var tmpIcoPath = GetPathFromFileType(data.Ico);
                    if (tmpIcoPath != null)
                    {
                        IconPath = tmpIcoPath;
                    }
                }
            }

            NoId = Id == null;
        }

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

        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        private string _nameWithoutExtension;
        public string NameWithoutExtension
        {
            get
            {
                if (_nameWithoutExtension != null) return _nameWithoutExtension;

                if (Type == FileType.File && !string.IsNullOrEmpty(Datum.Ico))
                {
                    _nameWithoutExtension = Regex.Replace(Name, $".{Datum.Ico}$", "", RegexOptions.IgnoreCase);
                }
                else
                {
                    _nameWithoutExtension = Name;
                }

                return _nameWithoutExtension;
            }
        }
        public static string GetPathFromFileType(string ico)
        {
            var iconPath = "ms-appx:///Assets/115/file_type/other/unknown.svg";
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
                            var tmpPath = $"Assets/115/file_type/{key}/{key2}.svg";

                            if (!File.Exists(Path.Combine(Package.Current.InstalledLocation.Path, tmpPath)))
                                iconPath = $"ms-appx:///Assets/115/file_type/{key}/{key}.svg";
                            else
                                iconPath = $"ms-appx:///{tmpPath}";


                            isMatch = true;
                            break;
                        }
                    }
                }
            }

            return iconPath;
        }

        public static string GetFileIconFromType(FileType fileType)
        {
            var iconUrl = "ms-appx:///Assets/115/file_type/other/unknown.svg";

            switch (fileType)
            {
                case FileType.Folder:
                    iconUrl = "ms-appx:///Assets/115/file_type/folder/folder.svg";
                    break;
                case FileType.File:
                    iconUrl = "ms-appx:///Assets/115/file_type/other/unknown.svg";
                    break;

            }

            return iconUrl;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public enum FileType { Folder, File };
    }

    /// <summary>
    /// 状态
    /// </summary>
    public enum Status { Doing, Success, Error, Pause, BeforeStart };

    /// <summary>
    /// 浏览器选择项
    /// </summary>
    public class SelectedItem
    {
        public long id { get; set; }
        public string name { get; set; }
        public int file_count { get; set; }
        public int folder_count { get; set; }
        public string size { get; set; }
        public string pick_code { get; set; }
        public int file_type { get; set; }
        public long file_id { get; set; }

        /// <summary>
        /// 是否有隐藏文件，有为1，无为0
        /// </summary>
        public int hasHiddenFile { get; set; }
    }

    public class FileCategory
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

        public int allCount => count + folder_count;

        public static Datum ConvertFolderToDatum(FileCategory fileCategory, long cid)
        {
            Datum datum = new()
            {
                Uid = 0,
                Aid = 1,
                Cid = cid,
                Name = fileCategory.file_name,
                Pid = fileCategory.paths[^1].file_id,
                PickCode = fileCategory.pick_code,
                Time = FileMatch.ConvertInt32ToDateTime(fileCategory.utime),
                TimeProduce = fileCategory.ptime,
                TimeEdit = fileCategory.utime,
            };

            return datum;
        }

        public FileCategory()
        {

        }

        public FileCategory(Datum datum)
        {
            this.file_name = datum.Name;
            this.pick_code = datum.PickCode;
            this.count = 1;
            this.file_category = 1;
            this.utime = datum.TimeEdit;
            this.ptime = datum.TimeProduce;
            this.sha1 = datum.Sha;
            this.size = ByteSize.FromBytes(datum.Size).ToString("#.#");
        }
    }

    public class ParentPath
    {
        public long file_id { get; set; }
        public string file_name { get; set; }
    }

    //115导入数据库的进度信息
    public enum ProgressStatus { normal, error, done, cancel }
    //报告文件数量
    public class GetFilesProgressInfo
    {
        public int FolderCount { get; set; } = 0;

        public int FilesCount { get; set; } = 0;

        public int AllCount => FolderCount + FilesCount;

        public List<long?> FailCid { get; set; } = new();

        public List<Datum> AddToDataAccessList = new();
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

        public string prifilePhotoPath
        {
            get
            {
                if (string.IsNullOrEmpty(_prifilePhotoPath))
                {
                    //初始化
                    _prifilePhotoPath = Const.Common.NoPicturePath;

                    //检查演员图片是否存在
                    string imagePath = Path.Combine(AppSettings.ActorInfoSavePath, name, "face.jpg");
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

        private Status _status = Status.BeforeStart;
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


        public ActorsInfo()
        {

        }

        public ActorsInfo(string name)
        {
            this.name = name;
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

        private Status _status = Status.BeforeStart;
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
        [JsonProperty(propertyName: "file_pickcode")]
        public string PickCode { get; set; }


        [JsonProperty(propertyName: "file_name")]
        public string FileName { get; set; }

        [JsonProperty(propertyName: "true_url")]
        public string TrueUrl { get; set; }

        [JsonProperty(propertyName: "ua")]
        public string Ua { get; set; }

        [JsonProperty(propertyName: "add_time")]
        public long AddTime { get; set; } = DateTimeOffset.Now.ToUnixTimeSeconds();
    }


    public class Jav321Json
    {
        public Cursor_Jav321 cursor { get; set; }
        public int code { get; set; }
        public Response_Jav321 response { get; set; }
        public string order { get; set; }
    }

    public class Cursor_Jav321
    {
        public bool hasPrev { get; set; }
        public object prev { get; set; }
        public int total { get; set; }
        public bool hasNext { get; set; }
        public string next { get; set; }
    }

    public class Response_Jav321
    {
        public int lastModified { get; set; }
        public object[] posts { get; set; }
        public Thread_Jav321 thread { get; set; }
    }

    public class Thread_Jav321
    {
        public string feed { get; set; }
        public string clean_title { get; set; }
        public int dislikes { get; set; }
        public int likes { get; set; }
        public string message { get; set; }
        public bool ratingsEnabled { get; set; }
        public bool isSpam { get; set; }
        public bool isDeleted { get; set; }
        public string category { get; set; }
        public bool adsDisabled { get; set; }
        public string author { get; set; }
        public string id { get; set; }
        public string signedLink { get; set; }
        public DateTime createdAt { get; set; }
        public bool hasStreaming { get; set; }
        public string raw_message { get; set; }
        public bool isClosed { get; set; }
        public string link { get; set; }
        public string slug { get; set; }
        public string forum { get; set; }
        public string[] identifiers { get; set; }
        public int posts { get; set; }
        public int[] moderators { get; set; }
        public bool validateAllPosts { get; set; }
        public string title { get; set; }
        public object highlightedPost { get; set; }
    }

    //字幕文件信息
    public class SubInfo
    {
        public string PickCode { get; set; }
        public string Name { get; set; }
        public string FileBelongPickCode { get; set; }

        public string TrueName { get; set; }

        public SubInfo(string pickCode, string name, string fileBelongPickCode, string trueName)
        {
            this.PickCode = pickCode;
            this.Name = name;
            this.FileBelongPickCode = fileBelongPickCode;
            this.TrueName = trueName;
        }
    }



    public enum RequestStates { none, fail, success };
    public enum SpiderStates { ready, doing, awaiting, done }

    public class SpiderInfo : INotifyPropertyChanged
    {
        public Models.SpiderInfo.SpiderSourceName SpiderSource { get; set; }

        public string Name { get; }

        public SpiderStates State { get; set; }
        public bool IsEnable { get; set; }
        public Microsoft.UI.Xaml.Media.Brush EllipseColor
        {
            get
            {
                switch (State)
                {
                    case SpiderStates.ready:
                        return new SolidColorBrush(Colors.MediumSeaGreen);
                    case SpiderStates.doing:
                        return new SolidColorBrush(Colors.MediumSeaGreen);
                    case SpiderStates.awaiting:
                        return new SolidColorBrush(Colors.SkyBlue);
                    case SpiderStates.done:
                        return new SolidColorBrush(Colors.LightGray);
                }

                return new SolidColorBrush(Colors.OrangeRed);
            }

        }

        /// <summary>
        /// 成功或失败是针对番号的搜刮的
        /// </summary>
        public RequestStates RequestStates { get; set; }

        private long _spidercount;
        public long SpiderCount
        {
            get => _spidercount;
            set
            {
                if (_spidercount == value) return;

                _spidercount = value;
            }
        }

        public Visibility EllipseVisiable
        {
            get
            {
                if (IsEnable)
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }
        }

        private string _message;
        public string Message
        {
            get => _message;
            set
            {
                if (_message == value)
                    return;
                _message = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(EllipseColor));
                OnPropertyChanged(nameof(SpiderCount));
            }
        }

        //初始化
        public SpiderInfo(Models.SpiderInfo.SpiderSourceName spiderSource, bool isEnable)
        {
            this.SpiderSource = spiderSource;
            this.IsEnable = isEnable;

            if (!IsEnable)
                this.Message = "已禁用";
        }

        public SpiderInfo(Models.SpiderInfo.SpiderSourceName spiderSource, string name)
        {
            this.SpiderSource = spiderSource;
            this.Name = name;
        }

        public SpiderInfo(Models.SpiderInfo.SpiderSourceName spiderSource)
        {
            this.SpiderSource = spiderSource;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }

    public class FailDatum
    {
        public FailDatum(Datum Datum)
        {
            this.Datum = Datum;
        }

        public Datum Datum { get; private set; }

        public string MatchName { get; set; }

        /// <summary>
        /// 是否是匹配失败
        /// </summary>
        public bool IsMatchFail { get; set; }

        /// <summary>
        /// 是否是搜刮失败
        /// </summary>
        public bool IsSpiderFail { get; set; }
    }


    public enum FailType { All, MatchFail, SpiderFail }

    public class FailInfo
    {
        [JsonProperty(propertyName: "pc")]
        public string PickCode { get; set; }

        [JsonProperty(propertyName: "is_like")]
        public int IsLike { get; set; } = 0;

        [JsonProperty(propertyName: "score")]
        public double Score { get; set; } = -1;

        [JsonProperty(propertyName: "look_later")]
        public long LookLater { get; set; } = 0;

        [JsonProperty(propertyName: "image_path")]
        public string ImagePath { get; set; } = Const.Common.NoPicturePath;

        [JsonProperty(propertyName: nameof(Datum))]
        public Datum Datum { get; set; }
    }

    public enum FailInfoShowType { like, look_later }

    public class MakeDirRequest
    {
        public bool state { get; set; }
        public string error { get; set; }
        public string errno { get; set; }
        public string errtype { get; set; }
        public int aid { get; set; }
        public long cid { get; set; }
        public string cname { get; set; }
        public string file_id { get; set; }
        public string file_name { get; set; }


    }


    public class ShaSearchResult
    {
        public string sha1 { get; set; }
        public string _ { get; set; }
        public Curr_User curr_user { get; set; }
        public int user_id { get; set; }
        public ShaSearchResultData data { get; set; }
        public bool state { get; set; }
        public string error { get; set; }
    }

    public class Curr_User
    {
        public bool state { get; set; }
        public int user_id { get; set; }
        public int error_code { get; set; }
        public string error_msg { get; set; }
        public int last_login { get; set; }
        public string ssoent { get; set; }
        public int user_name { get; set; }
    }

    public class ShaSearchResultData
    {
        public string file_id { get; set; }
        public string file_name { get; set; }
        public string file_size { get; set; }
        public string pick_code { get; set; }
        public string is_share { get; set; }
        public string category_id { get; set; }
        public string area_id { get; set; }
        public string ico { get; set; }
    }

    public class AddTaskBtResult
    {
        public bool state { get; set; }
        public int errno { get; set; }
        public string errtype { get; set; }
        public int errcode { get; set; }
        public string info_hash { get; set; }
        public string name { get; set; }
        public int start_torrent { get; set; }
    }


    public class TorrentInfoResult
    {
        public bool state { get; set; }
        public string error_msg { get; set; }
        public int errno { get; set; }
        public string errtype { get; set; }
        public int errcode { get; set; }
        public long file_size { get; set; }
        public string torrent_name { get; set; }
        public int file_count { get; set; }
        public string info_hash { get; set; }
        public Torrent_Filelist_Web[] torrent_filelist_web { get; set; }


    }

    public class Torrent_Filelist_Web
    {
        public long size { get; set; }
        public string path { get; set; }
        public int wanted { get; set; }
    }


    public class TorrentCidResult
    {
        public long cid { get; set; }
    }

    public class RenameRequest
    {
        public bool state { get; set; }
        public string error { get; set; }
        public int errno { get; set; }
        public Dictionary<string, string> data { get; set; }
    }

    public class OfflineDownPathRequest
    {
        public bool state { get; set; }
        public object error { get; set; }
        public object errno { get; set; }
        public OfflineDownPathData[] data { get; set; }

    }

    public class OfflineDownPathData
    {
        public string id { get; set; }
        public int user_id { get; set; }
        public long file_id { get; set; }
        public string update_time { get; set; }
        public string is_selected { get; set; }
        public string file_name { get; set; }
    }

    public class AddTaskUrlInfo
    {
        public bool state { get; set; }
        public int errno { get; set; }

        public int errcode { get; set; }

        public string errtype { get; set; }

        public string error_msg { get; set; }
        public string info_hash { get; set; }

        public string url { get; set; }

        public AddTaskUrlInfo[] result { get; set; }
    }

    public class UploadInfo
    {
        public string uploadinfo { get; set; }
        public int user_id { get; set; }
        public int app_version { get; set; }
        public int app_id { get; set; }
        public string userkey { get; set; }
        public string url_upload { get; set; }
        public string url_resume { get; set; }
        public string url_cancel { get; set; }
        public string url_speed { get; set; }
        public Dictionary<string, string> url_speed_test { get; set; }
        public long size_limit { get; set; }
        public int size_limit_yun { get; set; }
        public int max_dir_level { get; set; }
        public int max_dir_level_yun { get; set; }
        public int max_file_num { get; set; }
        public int max_file_num_yun { get; set; }
        public bool upload_allowed { get; set; }
        public string upload_allowed_msg { get; set; }
        public List<string> type_limit { get; set; }
        public Dictionary<string, string> file_range { get; set; }
        public int isp_type { get; set; }
        public bool state { get; set; }
        public string error { get; set; }
        public int errno { get; set; }
    }

    public class OfflineSpaceInfo
    {
        public bool state { get; set; }
        public long data { get; set; }
        public string size { get; set; }
        public string url { get; set; }
        public string bt_url { get; set; }
        public long limit { get; set; }
        public string sign { get; set; }
        public int time { get; set; }
    }
}
