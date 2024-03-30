using Newtonsoft.Json;

namespace Display.Models.Api.OneOneFive.User;

public class InfoData
{
    [JsonProperty("uid")]
    public string Uid { get; set; }
    
    [JsonProperty("time")]
    public int Time { get; set; }
    
    [JsonProperty("sign")]
    public string Sign { get; set; }
    
    [JsonProperty("qrcode")]
    public string Qrcode { get; set; }
}