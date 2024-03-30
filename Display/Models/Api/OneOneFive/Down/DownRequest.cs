using Newtonsoft.Json;

namespace Display.Models.Api.OneOneFive.Down;

public class DownRequest
{
    [JsonProperty("n")]
    public string Name;
    
    [JsonProperty("pc")]
    public string PickCode;
    
    [JsonProperty("is_dir")]
    public bool IsDir;
}