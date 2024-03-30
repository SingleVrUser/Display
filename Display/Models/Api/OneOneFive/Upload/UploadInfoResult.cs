using System.Collections.Generic;
using Newtonsoft.Json;

namespace Display.Models.Api.OneOneFive.Upload;

public class UploadInfoResult
{
    [JsonProperty("uploadinfo")]
    public string UploadInfo { get; set; }
    
    [JsonProperty("user_id")]
    public int UserId { get; set; }
    
    [JsonProperty("app_version")]
    public int AppVersion { get; set; }
    
    [JsonProperty("app_id")]
    public int AppId { get; set; }
    
    [JsonProperty("userkey")]
    public string UserKey { get; set; }
    
    [JsonProperty("url_upload")]
    public string UrlUpload { get; set; }
    
    [JsonProperty("url_resume")]
    public string UrlResume { get; set; }
    
    [JsonProperty("url_cancel")]
    public string UrlCancel { get; set; }
    
    [JsonProperty("url_speed")]
    public string UrlSpeed { get; set; }
    
    [JsonProperty("url_speed_test")]
    public Dictionary<string, string> UrlSpeedTest { get; set; }
    
    [JsonProperty("size_limit")]
    public long SizeLimit { get; set; }
    
    [JsonProperty("size_limit_yun")]
    public int SizeLimitYun { get; set; }
    
    [JsonProperty("max_dir_level")]
    public int MaxDirLevel { get; set; }
    
    [JsonProperty("max_dir_level_yun")]
    public int MaxDirLevelYun { get; set; }
    
    [JsonProperty("max_file_num")]
    public int MaxFileNum { get; set; }
    
    [JsonProperty("max_file_num_yun")]
    public int MaxFileNumYun { get; set; }
    
    [JsonProperty("upload_allowed")]
    public bool UploadAllowed { get; set; }
    
    [JsonProperty("upload_allowed_msg")]
    public string UploadAllowedMsg { get; set; }
    
    [JsonProperty("type_limit")]
    public List<string> TypeLimit { get; set; }
    
    [JsonProperty("file_range")]
    public Dictionary<string, string> FileRange { get; set; }
    
    [JsonProperty("isp_type")]
    public int IspType { get; set; }
    
    [JsonProperty("state")]
    public bool State { get; set; }
    
    [JsonProperty("error")]
    public string Error { get; set; }
    
    [JsonProperty("errno")]
    public int Errno { get; set; }
}