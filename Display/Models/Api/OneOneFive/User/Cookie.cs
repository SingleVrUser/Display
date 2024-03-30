using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Display.Models.Api.OneOneFive.User;

public class Cookie
{
    [JsonProperty("UID")]
    public string UID { get; set; }
    
    [JsonProperty("CID")]
    public string CID { get; set; }
    
    [JsonProperty("SEID")]
    public string SEID { get; set; }
}