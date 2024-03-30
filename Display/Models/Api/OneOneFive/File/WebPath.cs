using Newtonsoft.Json;

namespace Display.Models.Api.OneOneFive.File;

public class WebPath
{
    [JsonProperty("name")]
    public string Name { get; set; }
    
    [JsonProperty("aid")]
    public int Aid { get; set; }
    
    [JsonProperty("cid")]
    public long Cid { get; set; }
    
    [JsonProperty("pid")]
    public long Pid { get; set; }
    
    [JsonProperty("isp")]
    public object Isp { get; set; }
    
    [JsonProperty("p_cid")]
    public string PCid { get; set; }
}