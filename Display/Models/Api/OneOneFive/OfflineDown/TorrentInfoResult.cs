using Newtonsoft.Json;

namespace Display.Models.Api.OneOneFive.OfflineDown;

public class TorrentInfoResult
{
    [JsonProperty("state")]
    public bool State { get; set; }
    
    [JsonProperty("error_msg")]
    public string ErrorMsg { get; set; }
    
    [JsonProperty("errno")]
    public int Errno { get; set; }
    
    [JsonProperty("errtype")]
    public string ErrType { get; set; }
    
    [JsonProperty("errcode")]
    public int ErrCode { get; set; }
    
    [JsonProperty("file_size")]
    public long FileSize { get; set; }
    
    [JsonProperty("torrent_name")]
    public string TorrentName { get; set; }
    
    [JsonProperty("file_count")]
    public int FileCount { get; set; }
    
    [JsonProperty("info_hash")]
    public string InfoHash { get; set; }
    
    [JsonProperty("torrent_filelist_web")]
    public TorrentFileListWeb[] TorrentFileListWeb { get; set; }


}