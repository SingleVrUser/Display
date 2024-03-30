using System.Collections.Generic;
using Newtonsoft.Json;

namespace Display.Models.Api.OneOneFive.Down;

public class ParamRequest
{
    [JsonProperty("count")]
    public int Count { get; set; }
    
    [JsonProperty("ref_url")]
    public string RefUrl { get; set; }
    
    [JsonProperty("list")]
    public List<DownRequest> List { get; set; }
}