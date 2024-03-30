using Newtonsoft.Json;

namespace Display.Models.Api.OneOneFive.OfflineDown;

public class AddTaskUrlInfo
{
    [JsonProperty("state")]
    public bool State { get; set; }
    
    [JsonProperty("errno")]
    public int Errno { get; set; }

    [JsonProperty("errcode")]
    public int ErrCode { get; set; }

    [JsonProperty("errtype")]
    public string ErrType { get; set; }

    [JsonProperty("error_msg")]
    public string ErrorMsg { get; set; }
    
    [JsonProperty("info_hash")]
    public string InfoHash { get; set; }

    [JsonProperty("url")]
    public string Url { get; set; }

    [JsonProperty("AddTaskUrlInfo")]
    public AddTaskUrlInfo[] Result { get; set; }
}