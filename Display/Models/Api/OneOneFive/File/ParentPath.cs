using Newtonsoft.Json;

namespace Display.Models.Api.OneOneFive.File;

public class ParentPath
{
    [JsonProperty("file_id")]
    public long FileId { get; set; }
    
    [JsonProperty("file_name")]
    public string FileName { get; set; }
}