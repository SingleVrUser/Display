using Newtonsoft.Json;

namespace Display.Models.Api.OneOneFive.OfflineDown;

public class OfflineDownPathRequest
{
    [JsonProperty("state")]
    public bool State { get; set; }
    
    [JsonProperty("error")]
    public object Error { get; set; }
    
    [JsonProperty("errno")]
    public object Errno { get; set; }
    
    [JsonProperty("data")]
    public OfflineDownPathData[] Data { get; set; }

}