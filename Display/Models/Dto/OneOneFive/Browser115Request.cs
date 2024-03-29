using Newtonsoft.Json;

namespace Display.Models.Dto.OneOneFive;

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