using System.Collections.Generic;
using Newtonsoft.Json;

namespace Display.Models.Dto.OneOneFive;

public class RenameRequest
{
    [JsonProperty("state")]
    public bool State { get; set; }
    
    [JsonProperty("error")]
    public string Error { get; set; }
    
    [JsonProperty("errno")]
    public int Errno { get; set; }
    
    [JsonProperty("data")]
    public Dictionary<string, string> Data { get; set; }
}