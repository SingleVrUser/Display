using System;

namespace Display.Models.Upload;

public class OssToken
{
    public string StatusCode { get; set; }
    public string AccessKeySecret { get; set; }
    public string SecurityToken { get; set; }
    public DateTime Expiration { get; set; }
    public string AccessKeyId { get; set; }
}