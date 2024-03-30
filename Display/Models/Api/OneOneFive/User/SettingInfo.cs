using Newtonsoft.Json;

namespace Display.Models.Api.OneOneFive.User;

public class SettingInfo
{
    [JsonProperty("asc_file")]
    public string AscFile { get; set; }
    
    [JsonProperty("order_file")]
    public string OrderFile { get; set; }
    
    [JsonProperty("view_file")]
    public string ViewFile { get; set; }
    
    [JsonProperty("language")]
    public string Language { get; set; }
    
    [JsonProperty("page_num")]
    public string PageNum { get; set; }
    
    [JsonProperty("isp_download")]
    public string IspDownload { get; set; }
    
    [JsonProperty("isp_upload")]
    public string IspUpload { get; set; }
    
    [JsonProperty("play_count")]
    public string PlayCount { get; set; }
    
    [JsonProperty("show_ad")]
    public string ShowAd { get; set; }
    
    [JsonProperty("ssl_download")]
    public string SslDownload { get; set; }
    
    [JsonProperty("ssl_upload")]
    public string SslUpload { get; set; }
    
    [JsonProperty("kc_open")]
    public string KcOpen { get; set; }
    
    [JsonProperty("user_edit_info")]
    public string UserEditInfo { get; set; }
    
    [JsonProperty("mobile")]
    public string Mobile { get; set; }
    
    [JsonProperty("mobile_set")]
    public string MobileSet { get; set; }
    
    [JsonProperty("valid_type")]
    public string ValidType { get; set; }
    
    [JsonProperty("show")]
    public string Show { get; set; }
    
    [JsonProperty("movies")]
    public int Movies { get; set; }
    
    [JsonProperty("movies_subtype")]
    public string MoviesSubtype { get; set; }
    
    [JsonProperty("theme")]
    public string Theme { get; set; }
    
    [JsonProperty("theme_wl")]
    public string ThemeWl { get; set; }
    
    [JsonProperty("tutorial")]
    public int Tutorial { get; set; }
    
    [JsonProperty("first_login")]
    public string FirstLogin { get; set; }
    
    [JsonProperty("show_hit")]
    public string ShowHit { get; set; }
    
    [JsonProperty("forever")]
    public string Forever { get; set; }
    
    [JsonProperty("natsort")]
    public int NatSort { get; set; }
    
    [JsonProperty("prav_tv_channels")]
    public int PravTvChannels { get; set; }
    
    [JsonProperty("prav_tv_channels_1")]
    public string PravTvChannels1 { get; set; }
    
    [JsonProperty("root_folder")]
    public string RootFolder { get; set; }
    
    [JsonProperty("video_progress_bar")]
    public int VideoProgressBar { get; set; }
    
    [JsonProperty("video_play_mode")]
    public int VideoPlayMode { get; set; }
    
    [JsonProperty("lang_pack")]
    public string LangPack { get; set; }
    
    [JsonProperty("play_speed")]
    public string PlaySpeed { get; set; }
    
    [JsonProperty("album_cover")]
    public int AlbumCover { get; set; }
    
    [JsonProperty("allow_radar_protocol")]
    public int AllowRadarProtocol { get; set; }
    
    [JsonProperty("allow_link_protocol")]
    public string AllowLinkProtocol { get; set; }
    
    [JsonProperty("allow_pubshare_protocol")]
    public string AllowPubShareProtocol { get; set; }
    
    [JsonProperty("allow_similarphoto_protocol")]
    public int AllowSimilarPhotoProtocol { get; set; }
    
    [JsonProperty("is_popup_showed")]
    public int IsPopupShowed { get; set; }
    
    [JsonProperty("display_week")]
    public int DisplayWeek { get; set; }
    
    [JsonProperty("allow_global_protocol")]
    public string AllowGlobalProtocol { get; set; }
    
    [JsonProperty("fc_mix")]
    public int FcMix { get; set; }
    
    [JsonProperty("is_into_transfer")]
    public string IsIntoTransfer { get; set; }
    
    [JsonProperty("skip_silence")]
    public int SkipSilence { get; set; }
    
    [JsonProperty("is_silent_hint")]
    public string IsSilentHint { get; set; }
    
    [JsonProperty("first_photo_backup")]
    public int FirstPhotoBackup { get; set; }
    
    [JsonProperty("first_play_video")]
    public string FirstPlayVideo { get; set; }
    
    [JsonProperty("first_upload")]
    public int FirstUpload { get; set; }
    
    [JsonProperty("first_download")]
    public string FirstDownload { get; set; }
    
    [JsonProperty("insert_default_file_label")]
    public string InsertDefaultFileLabel { get; set; }
    
    [JsonProperty("subtitle_color")]
    public string SubtitleColor { get; set; }
    
    [JsonProperty("subtitle_position")]
    public int SubtitlePosition { get; set; }
    
    [JsonProperty("subtitle_size_type")]
    public int SubtitleSizeType { get; set; }
    
    [JsonProperty("subtitle_web_size")]
    public int SubtitleWebSize { get; set; }
    
    [JsonProperty("last_file_type")]
    public string LastFileType { get; set; }
    
    [JsonProperty("last_file_id")]
    public string LastFileId { get; set; }
    
    [JsonProperty("default_search_path")]
    public int DefaultSearchPath { get; set; }
    
    [JsonProperty("range_options_guide")]
    public string RangeOptionsGuide { get; set; }
    
    [JsonProperty("include_music_order")]
    public string IncludeMusicOrder { get; set; }
    
    [JsonProperty("star_music_asc")]
    public int StarMusicAsc { get; set; }
    
    [JsonProperty("music_list_asc")]
    public int MusicListAsc { get; set; }
}