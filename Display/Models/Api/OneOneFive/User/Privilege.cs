using Newtonsoft.Json;

namespace Display.Models.Api.OneOneFive.User;

public class Privilege
{
    [JsonProperty("start")]
    public int Start { get; set; }
    
    [JsonProperty("expire")]
    public long Expire { get; set; }
    
    [JsonProperty("state")]
    public bool State { get; set; }
    
    [JsonProperty("mark")]
    public int Mark { get; set; }
}