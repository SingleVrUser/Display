using Newtonsoft.Json;

namespace Display.Models.Api.OneOneFive.Browser;

/// <summary>
/// 浏览器选择项
/// </summary>
public class SelectedItemInBrowser
{
    [JsonProperty("id")]
    public long Id { get; set; }
    
    [JsonProperty("name")]
    public string Name { get; set; }
    
    [JsonProperty("file_count")]
    public int FileCount { get; set; }
    
    [JsonProperty("folder_count")]
    public int FolderCount { get; set; }
    
    [JsonProperty("size")]
    public string Size { get; set; }
    
    [JsonProperty("pick_code")]
    public string PickCode { get; set; }
    
    [JsonProperty("file_type")]
    public int FileType { get; set; }
    
    [JsonProperty("file_id")]
    public long FileId { get; set; }

    /// <summary>
    /// 是否有隐藏文件，有为1，无为0
    /// </summary>
    
    [JsonProperty("hasHiddenFile")]
    public int HasHiddenFile { get; set; }
}