using DataAccess.Models.Entity;
using Newtonsoft.Json;

namespace Display.Models.Api.OneOneFive.File;

public class WebFileInfo
{
    [JsonProperty("data")]
    public FileInfo[] Data { get; set; }
    
    [JsonProperty("path")]
    public WebPath[] Path { get; set; }
    
    [JsonProperty("count")]
    public int Count { get; set; }
    
    [JsonProperty("data_source")]
    public string DataSource { get; set; }
    
    [JsonProperty("sys_count")]
    public int SysCount { get; set; }
    
    [JsonProperty("offset")]
    public int Offset { get; set; }
    
    [JsonProperty("o")]
    public int O { get; set; }
    
    [JsonProperty("limit")]
    public int Limit { get; set; }
    
    [JsonProperty("page_size")]
    public int PageSize { get; set; }
    
    [JsonProperty("aid")]
    public int Aid { get; set; }
    
    [JsonProperty("cid")]
    public long Cid { get; set; }
    
    [JsonProperty("is_asc")]
    public int IsAsc { get; set; }
    
    [JsonProperty("order")]
    public string Order { get; set; }
    
    [JsonProperty("star")]
    public int Star { get; set; }
    
    [JsonProperty("type")]
    public int Type { get; set; }
    
    [JsonProperty("r_all")]
    public int RAll { get; set; }
    
    [JsonProperty("stdir")]
    public int StDir { get; set; }
    
    [JsonProperty("cur")]
    public int Cur { get; set; }
    
    [JsonProperty("fc_mix")]
    public int FcMix { get; set; }
    
    [JsonProperty("suffix")]
    public string Suffix { get; set; }
    
    [JsonProperty("min_size")]
    public int MinSize { get; set; }
    
    [JsonProperty("max_size")]
    public int MaxSize { get; set; }
    
    [JsonProperty("state")]
    public bool State { get; set; }
    
    [JsonProperty("error")]
    public string Error { get; set; }
    
    [JsonProperty("errNo")]
    public int ErrNo { get; set; }
}