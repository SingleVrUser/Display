using Newtonsoft.Json;

namespace Display.Models.Api.OneOneFive.Search;

public class CurrUser
{
    [JsonProperty("state")]
    public bool State { get; set; }
    
    [JsonProperty("user_id")]
    public int UserId { get; set; }
    
    [JsonProperty("error_code")]
    public int ErrorCode { get; set; }
    
    [JsonProperty("error_msg")]
    public string ErrorMsg { get; set; }
    
    [JsonProperty("last_login")]
    public int LastLogin { get; set; }
    
    [JsonProperty("ssoent")]
    public string Ssoent { get; set; }
    
    [JsonProperty("user_name")]
    public int UserName { get; set; }
}