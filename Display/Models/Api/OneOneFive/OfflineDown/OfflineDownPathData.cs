using Newtonsoft.Json;

namespace Display.Models.Api.OneOneFive.OfflineDown;

public class OfflineDownPathData
{
    [JsonProperty("id")]
    public string Id { get; set; }
    
    [JsonProperty("user_id")]
    public int UserId { get; set; }
    
    [JsonProperty("file_id")]
    public long FileId { get; set; }
    
    [JsonProperty("update_time")]
    public string UpdateTime { get; set; }
    
    [JsonProperty("is_selected")]
    public string IsSelected { get; set; }
    
    [JsonProperty("file_name")]
    public string FileName { get; set; }
}