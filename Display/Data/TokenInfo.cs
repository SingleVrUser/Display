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
using System.Text;
using System.Text.RegularExpressions;
using Windows.ApplicationModel;
using Display.Helper;
using static Display.Data.FilesInfo;
using static SkiaSharp.HarfBuzz.SKShaper;

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

    public class WebFileInfo
    {
        public Datum[] data { get; set; }
        public WebPath[] path { get; set; }

        public int count { get; set; }
        public string data_source { get; set; }
        public int sys_count { get; set; }
        public int offset { get; set; }
        public int o { get; set; }
        public int limit { get; set; }
        public int page_size { get; set; }
        public int aid { get; set; }
        public long cid { get; set; }
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
        public int aid { get; set; }
        public long cid { get; set; }
        public long pid { get; set; }
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

    public class DeleteFilesResult
    {
        public bool state { get; set; }
        public string error { get; set; }
        public string errno { get; set; }
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
                var path = !string.IsNullOrEmpty(value) ? value : Const.FileType.NoPicturePath;
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
            this.trueName = failDatum.Name;
            this.ImagePath = "ms-appx:///Assets/Fail.jpg";
            this.Series = "fail";
            this.ReleaseTime = failDatum.Time;
            this.ImageUrl = failDatum.PickCode;
            this.LookLater = failDatum.Size;
            this.busUrl = failDatum.PickCode;
            this.Category = failDatum.Ico;
        }

        private string _trueName;

        [JsonProperty(propertyName: "truename")]
        public string trueName
        {
            get => _trueName;
            set
            {
                _trueName = value;
                OnPropertyChanged();
            }
        }

        [JsonProperty(propertyName: "title")]
        public string Title { get; set; }

        private string _releaseTime;

        [JsonProperty(propertyName: "releasetime")]
        public string ReleaseTime
        {
            get => _releaseTime;
            set
            {
                _releaseTime = value;
                OnPropertyChanged();
            }
        }

        private string _lengthTime;

        [JsonProperty(propertyName: "lengthtime")]
        public string Lengthtime
        {
            get => _lengthTime;
            set
            {
                _lengthTime = value;
                OnPropertyChanged();
            }
        }

        private string _director;

        [JsonProperty(propertyName: "director")]
        public string Director
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
        public string Producer
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
        public string Publisher
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
        public string Series
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
        public string Category
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
        public string Actor
        {
            get => _actor;
            set
            {
                _actor = value;
                OnPropertyChanged();
            }

        }

        private string _imageUrl;

        [JsonProperty(propertyName: "imageurl")]
        public string ImageUrl
        {
            get => _imageUrl;
            set
            {
                if (_imageUrl == value) return;
                _imageUrl = value;
                OnPropertyChanged();
            }
        }


        [JsonProperty(propertyName: "sampleImageList")]
        public string SampleImageList { get; set; }

        private string _imagePath;

        [JsonProperty(propertyName: "imagepath")]
        public string ImagePath
        {
            get => _imagePath;
            set
            {
                if (_imagePath == value) return;

                string path = value;
                _imagePath = !string.IsNullOrEmpty(path) ? path : Const.FileType.NoPicturePath;
                OnPropertyChanged();
            }
        }


        [JsonProperty(propertyName: "busurl")]
        public string busUrl { get; set; }

        private long _lookLater = 0;

        [JsonProperty(propertyName: "look_later")]
        public long LookLater
        {
            get => _lookLater;
            set
            {
                _lookLater = value;
                OnPropertyChanged();
            }
        }

        private double _score = -1;

        [JsonProperty(propertyName: "score")]
        public double Score
        {
            get => _score;
            set
            {
                _score = value;
                OnPropertyChanged();
            }
        }


        private int _isLike = 0;

        [JsonProperty(propertyName: "is_like")]
        public int IsLike
        {
            get => _isLike;
            set
            {
                _isLike = value;
                OnPropertyChanged();
            }
        }

        [JsonProperty(propertyName: "addtime")]
        public long AddTime { get; set; } = DateTimeOffset.Now.ToUnixTimeSeconds();
        public int IsWm { get; set; } = -1;


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
            Title = videoinfo.Title;

            //是否显示右上角的标签
            string category = videoinfo.Category;
            Visibility isShowLabel = Visibility.Collapsed;

            string ShowLabel = string.Empty;
            if (!string.IsNullOrEmpty(category))
            {
                if (category.Contains("VR") || (!string.IsNullOrEmpty(videoinfo.Series) && videoinfo.Series.Contains("VR")))
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

            if (!string.IsNullOrEmpty(ReleaseTime))
            {
                if (videoinfo.ReleaseTime.Contains("/"))
                {
                    this.realeaseYear = videoinfo.ReleaseTime.Split('/')[0];
                }
                else
                {
                    this.realeaseYear = videoinfo.ReleaseTime.Split('-')[0];
                }
            }

            this.isShowLabel = isShowLabel;
            this.ShowLabel = ShowLabel;
            this.Score = videoinfo.Score;

            //图片大小
            this.imageHeight = imgheight;
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

        private double _imageHeight;
        public double imageHeight
        {
            get
            {
                return _imageHeight;
            }
            set
            {
                _imageHeight = value;
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
        public readonly long Size;
        public readonly string Sha1;
        public readonly string PickCode;
        public readonly FileType Type;
        public readonly int Time;
        public readonly bool IsVideo;
        public readonly bool NoId;
        public string Ico{
            get
            {
                var nameArray = Name.Split('.');
                return nameArray.Length == 1 ? string.Empty : nameArray[^1];
            }
        }

        public FilesInfo(FileUploadResult result)
        {
            Name = result.Name;
            Id = result.Id;
            Cid = result.Cid;
            PickCode = result.PickCode;
            Size = result.FileSize;
            Sha1 = result.Sha1;
            Type = result.Type;
            NoId = Id ==null;
            Time = (int)DateTimeOffset.Now.ToUnixTimeSeconds();

            IsVideo = result.IsVideo;
        }

        public void UpdateName(string name)
        {
            Name = name;
            // 清空_nameWithoutExtension，再次获取时会重新计算
            _nameWithoutExtension = null;

            if(Type == FileType.File) IconPath = GetPathFromIcon(Ico);
        }

        public FilesInfo(Datum data)
        {
            Datum = data;
            Name = data.Name;
            Time = data.TimeEdit;
            
            //文件夹
            if (data.Fid == null && data.Pid!=null)
            {
                Type = FileType.Folder;
                Id = data.Cid;
                Cid = (long)data.Pid;
                PickCode = data.PickCode;
                IconPath = Const.FileType.FolderSvgPath;
            }
            else if(data.Fid != null)
            {
                Type = FileType.File;
                Id = data.Fid;
                Cid = data.Cid;
                PickCode = data.PickCode;
                Size = data.Size;
                Sha1 = data.Sha;

                //视频文件
                if (data.Iv == 1)
                {
                    IsVideo = true;
                    var videoQuality = GetVideoQualityFromVdi(data.Vdi);

                    IconPath = videoQuality != null ? "ms-appx:///Assets/115/file_type/video_quality/"+videoQuality+".svg"
                                                    : Const.FileType.VideoSvgPath;

                }
                else if (!string.IsNullOrEmpty(data.Ico))
                {
                    var tmpIcoPath = GetPathFromIcon(data.Ico);
                    if (tmpIcoPath != null)
                    {
                        IconPath = tmpIcoPath;
                    }
                }

                IconPath ??= Const.FileType.UnknownSvgPath;

            }

            NoId = Id == null;
        }

        public static string GetVideoQualityFromVdi(int vdi)
        {
            string videoQuality = null;
            switch (vdi)
            {
                case 1:
                    videoQuality = "sd";
                    break;
                case 2:
                    videoQuality = "hd";
                    break;
                case 3:
                    videoQuality = "fhd";
                    break;
                case 4:
                    videoQuality = "1080p";
                    break;
                case 5:
                    videoQuality = "4k";
                    break;
                case 100:
                    videoQuality = "origin";
                    break;
            }
            return videoQuality;
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

                var ico = Ico;
                if (Type == FileType.File && !string.IsNullOrEmpty(ico))
                {
                    _nameWithoutExtension = Regex.Replace(Name, $".{ico}$", "", RegexOptions.IgnoreCase);
                }
                else
                {
                    _nameWithoutExtension = Name;
                }

                return _nameWithoutExtension;
            }
        }
        
        private string _iconPath;
        public string IconPath
        {
            get => _iconPath;
            set
            {
                _iconPath = value;
                OnPropertyChanged();
            }
        }

        public static string GetPathFromIcon(string ico)
        {
            string iconPath = null;

            var isMatch = false;
            foreach (var (key, values) in Const.FileType.FileTypeDictionary)
            {
                if (isMatch)
                    break;

                foreach (var (key2, values2) in values)
                {
                    if (isMatch)
                        break;

                    foreach (var key3 in values2)
                    {
                        if (key3 != ico) continue;

                        var tmpStringBuilder = new StringBuilder(Const.FileType.FileTypeBasePath).Append(key).Append('/').Append(key2).Append(".svg");
                        if (File.Exists(Path.Combine(Package.Current.InstalledLocation.Path, tmpStringBuilder.ToString())))
                        {
                            iconPath = tmpStringBuilder.Insert(0,Const.FileType.MsUri).ToString();
                        }
                        else
                        {
                            var newStringBuilder = new StringBuilder(Const.FileType.FileTypeFullBasePath).Append(key).Append('/').Append(key).Append(".svg");
                            iconPath = newStringBuilder.ToString();
                        }
                        isMatch = true;
                        break;
                    }
                }
            }

            return string.IsNullOrEmpty(iconPath) ? Const.FileType.UnknownSvgPath: iconPath;
        }

        public static string GetTypeFromIcon(string ico)
        {
            foreach (var (key, values) in Const.FileType.FileTypeDictionary)
            {
                foreach (var (_, values2) in values)
                {
                    foreach (var value3 in values2)
                    {
                        if (value3 != ico) continue;

                        return key;
                    }
                }
            }

            return "unknown";
        }

        public static string GetFileIconFromType(FileType fileType)
        {
            var iconUrl  = Const.FileType.UnknownSvgPath;

            switch (fileType)
            {
                case FileType.Folder:
                    iconUrl = Const.FileType.FolderSvgPath;
                    break;
                case FileType.File:
                    iconUrl = Const.FileType.UnknownSvgPath;
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
                Time = DateHelper.ConvertInt32ToDateTime(fileCategory.utime),
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
                    _prifilePhotoPath = Const.FileType.NoPicturePath;

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
            name = videoinfo.trueName;

            var tmpList = videoinfo.SampleImageList.Split(',').ToList();
            if (tmpList.Count > 1)
            {
                thumbnailDownUrlList = tmpList;
            }

            if (videoinfo.Category.Contains("VR") || videoinfo.Series.Contains("VR"))
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

    public class FailInfo : INotifyPropertyChanged
    {
        [JsonProperty(propertyName: "pc")]
        public string PickCode { get; set; }

        [JsonProperty(propertyName: "is_like")]
        public int IsLike { get; set; } = 0;

        private double _score = 0;

        [JsonProperty(propertyName: "score")]
        public double Score
        {
            get => _score;
            set
            {
                _score = value;
                OnPropertyChanged();
            }
        }

        [JsonProperty(propertyName: "look_later")]
        public long LookLater { get; set; } = 0;

        [JsonProperty(propertyName: "image_path")]
        public string ImagePath { get; set; } = Const.FileType.NoPicturePath;

        [JsonProperty(propertyName: nameof(Datum))]
        public Datum Datum { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
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
