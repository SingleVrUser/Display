using Newtonsoft.Json;

namespace Display.Models.Api.OneOneFive.Down;

public class DownUrlBase64EncryptInfo
{
    [JsonProperty("state")]
    public bool State { get; set; }
    
    [JsonProperty("msg")]
    public string Msg { get; set; }
    
    [JsonProperty("errno")]
    public int Errno { get; set; }
    
    [JsonProperty("data")]
    public string Data { get; set; }
}