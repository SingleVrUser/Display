using Display.Models.Api.OneOneFive.Down;
using Display.Models.Dto.OneOneFive;
using Newtonsoft.Json;

namespace Display.Models.Api.OneOneFive.Browser;

/// <summary>
/// 用115下载请求
/// </summary>
public class Browser115Request
{
    [JsonProperty("uid")]
    public long Uid { get; set; }
    
    [JsonProperty("key")]
    public string Key { get; set; }
    
    [JsonProperty("param")]
    public ParamRequest Param { get; set; }
    
    [JsonProperty("type")]
    public int Type { get; set; } = 1;
}