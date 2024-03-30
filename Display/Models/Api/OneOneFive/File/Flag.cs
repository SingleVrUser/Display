using Newtonsoft.Json;

namespace Display.Models.Api.OneOneFive.File;

public class Flag
{
    [JsonProperty("id")]
    public string Id { get; set; }
    
    [JsonProperty("name")]
    public string Name { get; set; }
    
    [JsonProperty("sort")]
    public string Sort { get; set; }
    
    [JsonProperty("color")]
    public string Color { get; set; }
    
    [JsonProperty("update_time")]
    public int UpdateTime { get; set; }
    
    [JsonProperty("create_time")]
    public int CreateTime { get; set; }
}