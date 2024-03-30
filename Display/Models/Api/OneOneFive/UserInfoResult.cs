using Display.Models.Api.OneOneFive.User;
using Display.Models.Dto.OneOneFive;
using Newtonsoft.Json;

namespace Display.Models.Api.OneOneFive;

public class UserInfoResult
{
    [JsonProperty("state")]
    public bool State { get; set; }
    
    [JsonProperty("error")]
    public string Error { get; set; }
    
    [JsonProperty("data")]
    public UserData Data { get; set; }
}