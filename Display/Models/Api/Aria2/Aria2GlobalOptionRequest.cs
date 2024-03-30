using Display.Models.Dto.OneOneFive;
using Newtonsoft.Json;

namespace Display.Models.Api.Aria2;

public class Aria2GlobalOptionRequest
{
    [JsonProperty("id")]
    public string Id { get; set; }
    
    [JsonProperty("jsonrpc")]
    public string Jsonrpc { get; set; }
    
    [JsonProperty("result")]
    public Aria2GlobalOptionRequestResult Result { get; set; }
}