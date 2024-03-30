using Newtonsoft.Json;

namespace Display.Models.Api.OneOneFive.Upload;

public class OssUploadResultData
{
    [JsonProperty("pick_code")]
    public string PickCode { get; }
    
    [JsonProperty("file_size")]
    public long FileSize { get; }
    
    [JsonProperty("file_id")]
    public long FileId { get; }
    
    [JsonProperty("thumb_url")]
    public string ThumbUrl { get; }
    
    [JsonProperty("sha1")]
    public string Sha1 { get; }
    
    [JsonProperty("aid")]
    public int Aid { get; }
    
    [JsonProperty("file_name")]
    public string FileName { get; }
    
    [JsonProperty("cid")]
    public long Cid { get; }
    
    [JsonProperty("is_video")]
    public int IsVideo { get; }
}