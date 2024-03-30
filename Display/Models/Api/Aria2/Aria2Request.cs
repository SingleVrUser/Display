using Newtonsoft.Json;

namespace Display.Models.Api.Aria2;

public class Aria2Request
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("params")]
    public string[] Params { get; set; }
    
    [JsonProperty("jsonrpc")]
    public string Jsonrpc { get; set; }

    [JsonProperty("method")]
    public string Method { get; set; }
}