using Newtonsoft.Json;

namespace Display.Models.Api.OneOneFive.File;

public class DeleteFilesResult
{
    [JsonProperty("state")]
    public bool State { get; set; }
    
    [JsonProperty("error")]
    public string Error { get; set; }
    
    [JsonProperty("errno")]
    public string Errno { get; set; }
}