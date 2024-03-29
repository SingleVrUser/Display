using Newtonsoft.Json;

namespace Display.Models.Dto.OneOneFive;

public class Aria2Request
{
    public string jsonrpc { get; set; }
    public string method { get; set; }
    public string id { get; set; }

    [JsonProperty(propertyName: "params")]
    public string[] _params { get; set; }
}