using Newtonsoft.Json;

namespace Display.Models.Dto.OneOneFive;

public class StatusInfo
{
    [JsonProperty("msg")]
    public string Msg { get; set; }
    
    [JsonProperty("status")]
    public int Status { get; set; }
    
    [JsonProperty("version")]
    public string Version { get; set; }
}