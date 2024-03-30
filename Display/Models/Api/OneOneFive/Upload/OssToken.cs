using System;
using Newtonsoft.Json;

namespace Display.Models.Api.OneOneFive.Upload;

public class OssToken
{
    public string StatusCode { get; set; }
    
    public string AccessKeySecret { get; set; }
    
    public string SecurityToken { get; set; }
    
    public DateTime Expiration { get; set; }
    
    [JsonProperty("AccessKeyId")]
    public string AccessKeyId { get; set; }
}