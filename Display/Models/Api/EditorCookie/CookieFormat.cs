using System;
using Newtonsoft.Json;

namespace Display.Models.Api.EditorCookie;

public class CookieFormat
{
    [JsonProperty("domain")]
    public string Domain { get; set; } = ".115.com";
    
    [JsonProperty("expirationDate")]
    public long ExpirationDate { get; set; } = DateTimeOffset.Now.AddMonths(1).ToUnixTimeSeconds();
    
    [JsonProperty("hostOnly")]
    public bool HostOnly { get; set; }
        
    [JsonProperty("httpOnly")]
    public bool HttpOnly { get; set; } = true;
        
    [JsonProperty("name")]
    public string Name { get; set; }
        
    [JsonProperty("path")]
    public string Path { get; set; } = "/";
        
    [JsonProperty("sameSite")]
    public string SameSite { get; set; }
        
    [JsonProperty("secure")]
    public bool Secure { get; set; }
        
    [JsonProperty("session")]
    public bool Session { get; set; }
        
    [JsonProperty("storeId")]
    public string StoreId { get; set; }
        
    [JsonProperty("value")]
    public string Value { get; set; }
}