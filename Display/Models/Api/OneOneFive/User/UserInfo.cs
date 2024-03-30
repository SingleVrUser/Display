using Newtonsoft.Json;

namespace Display.Models.Api.OneOneFive.User;

public class UserInfo
{
    [JsonProperty("user_id")]
    public int UserId { get; set; }
    
    [JsonProperty("user_name")]
    public string UserName { get; set; }
    
    [JsonProperty("email")]
    public string Email { get; set; }
    
    [JsonProperty("mobile")]
    public string Mobile { get; set; }
    
    [JsonProperty("country")]
    public string Country { get; set; }
    
    [JsonProperty("is_vip")]
    public long IsVip { get; set; }
    
    [JsonProperty("mark")]
    public int Mark { get; set; }
    
    [JsonProperty("alert")]
    public string Alert { get; set; }
    
    [JsonProperty("is_chang_passwd")]
    public int IsChangPasswd { get; set; }
    
    [JsonProperty("is_first_login")]
    public int IsFirstLogin { get; set; }
    
    [JsonProperty("bind_mobile")]
    public int BindMobile { get; set; }
    
    [JsonProperty("face")]
    public Face Face { get; set; }
    
    [JsonProperty("cookie")]
    public Cookie Cookie { get; set; }
    
    [JsonProperty("from")]
    public string From { get; set; }
    
    [JsonProperty("is_trusted")]
    public object IsTrusted { get; set; }
}