using Newtonsoft.Json;

namespace Display.Models.Api.OneOneFive.OfflineDown;

public class AddTaskBtResult
{
    [JsonProperty("state")]
    public bool State { get; set; }
    
    [JsonProperty("errno")]
    public int Errno { get; set; }
    
    [JsonProperty("errtype")]
    public string ErrType { get; set; }
    
    [JsonProperty("errcode")]
    public int ErrCode { get; set; }
    
    [JsonProperty("info_hash")]
    public string InfoHash { get; set; }
    
    [JsonProperty("name")]
    public string Name { get; set; }
    
    [JsonProperty("start_torrent")]
    public int StartTorrent { get; set; }
}