using Display.Models.Api.OneOneFive.User;
using Display.Models.Dto.OneOneFive;
using Newtonsoft.Json;

namespace Display.Models.Api.OneOneFive.Setting;

public class _115Setting
{
    [JsonProperty("state")]
    public bool State { get; set; }
    
    [JsonProperty("_goto")]
    public string Goto { get; set; }
    
    [JsonProperty("even")]
    public string Even { get; set; }
    
    [JsonProperty("data")]
    public SettingInfo Info { get; set; }
    
    [JsonProperty("flush")]
    public bool Flush { get; set; }
}