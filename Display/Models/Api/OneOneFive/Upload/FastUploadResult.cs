using Newtonsoft.Json;

namespace Display.Models.Api.OneOneFive.Upload;

public class FastUploadResult
{
    
    [JsonProperty("request")]
    public string Request { get; set; }
    
    [JsonProperty("status")]
    public int Status { get; set; }
    
    [JsonProperty("statuscode")]
    public int StatusCode { get; set; }
    
    [JsonProperty("statusmsg")]
    public string StatusMsg { get; set; }
    
    [JsonProperty("pickcode")]
    public string PickCode { get; set; }
    
    [JsonProperty("target")]
    public string Target { get; set; }
    
    [JsonProperty("version")]
    public string Version { get; set; }
    
    [JsonProperty("sign_key")]
    public string SignKey { get; set; }
    
    [JsonProperty("sign_check")]
    public string SignCheck { get; set; }
    
    [JsonProperty("bucket")]
    public string Bucket { get; set; }
    
    [JsonProperty("object")]
    public string Object { get; set; }
    
    [JsonProperty("OssCallback")]
    public OssCallback OssCallback { get; set; }
}