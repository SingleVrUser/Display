using Newtonsoft.Json;

namespace Display.Models.Dto.OneOneFive;

public class VerifyAccountResult
{
    [JsonProperty(propertyName: "state")]
    public bool State { get; set; }

    [JsonProperty(propertyName: "error")]
    public string Error { get; set; }

    [JsonProperty(propertyName: "errno")]
    public int Errno { get; set; }

}