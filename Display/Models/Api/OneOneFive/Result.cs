using Display.Models.Api.OneOneFive.File;
using Display.Models.Dto.OneOneFive;
using Newtonsoft.Json;
using UserInfo = Display.Models.Api.OneOneFive.User.UserInfo;

namespace Display.Models.Api.OneOneFive;

public class Result
{
    [JsonProperty("state")]
    public int State { get; set; }
    
    [JsonProperty("error")]
    public string Error { get; set; }
    
    [JsonProperty("errno")]
    public int Errno { get; set; }
    
    [JsonProperty("message")]
    public string Message { get; set; }
    
    [JsonProperty("code")]
    public int Code { get; set; }
    
    [JsonProperty("data")]
    public UserInfo Data { get; set; }
}