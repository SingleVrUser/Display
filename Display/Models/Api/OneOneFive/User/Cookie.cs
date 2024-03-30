using Newtonsoft.Json;

namespace Display.Models.Api.OneOneFive.User;

public class Cookie
{
    [JsonProperty("UID")]
    public string Uid { get; set; }
    
    [JsonProperty("CID")]
    public string Cid { get; set; }
    
    [JsonProperty("SEID")]
    public string Seid { get; set; }
}