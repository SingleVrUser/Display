using Newtonsoft.Json;

namespace Display.Models.Api.OneOneFive.Search;

public class ShaSearchResultData
{
    [JsonProperty("file_id")]
    public string FileId { get; set; }
    
    [JsonProperty("file_name")]
    public string FileName { get; set; }
    
    [JsonProperty("file_size")]
    public string FileSize { get; set; }
    
    [JsonProperty("pick_code")]
    public string PickCode { get; set; }
    
    [JsonProperty("is_share")]
    public string IsShare { get; set; }
    
    [JsonProperty("category_id")]
    public string CategoryId { get; set; }
    
    [JsonProperty("area_id")]
    public string AreaId { get; set; }
    
    [JsonProperty("ico")]
    public string Ico { get; set; }
}