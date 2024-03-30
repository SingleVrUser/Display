using Newtonsoft.Json;

namespace Display.Models.Api.OneOneFive.OfflineDown;

public class OfflineSpaceInfo
{
    [JsonProperty("state")]
    public bool State { get; set; }
    
    [JsonProperty("data")]
    public long Data { get; set; }
    
    [JsonProperty("size")]
    public string Size { get; set; }
    
    [JsonProperty("url")]
    public string Url { get; set; }
    
    [JsonProperty("bt_url")]
    public string BtUrl { get; set; }
    
    [JsonProperty("limit")]
    public long Limit { get; set; }
    
    [JsonProperty("sign")]
    public string Sign { get; set; }
    
    [JsonProperty("time")]
    public int Time { get; set; }
}