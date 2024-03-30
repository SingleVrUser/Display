using Display.Models.Dto.OneOneFive;
using Newtonsoft.Json;

namespace Display.Models.Api.OneOneFive;

public class QRCodeStatusResult
{
    [JsonProperty("state")]
    public int State { get; set; }
    
    [JsonProperty("code")]
    public int Code { get; set; }
    
    [JsonProperty("message")]
    public string Message { get; set; }
    
    [JsonProperty("data")]
    public StatusInfo Data { get; set; }
}