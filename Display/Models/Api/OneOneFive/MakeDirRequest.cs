using Newtonsoft.Json;

namespace Display.Models.Api.OneOneFive;

public class MakeDirRequest
{
    [JsonProperty("state")]
    public bool State { get; set; }
    
    [JsonProperty("error")]
    public string Error { get; set; }
    
    [JsonProperty("errno")]
    public string Errno { get; set; }
    
    [JsonProperty("errtype")]
    public string ErrType { get; set; }
    
    [JsonProperty("aid")]
    public int Aid { get; set; }
    
    [JsonProperty("cid")]
    public long Cid { get; set; }
    
    [JsonProperty("cname")]
    public string Cname { get; set; }
    
    [JsonProperty("file_id")]
    public string FileId { get; set; }
    
    [JsonProperty("file_name")]
    public string FileName { get; set; }
}