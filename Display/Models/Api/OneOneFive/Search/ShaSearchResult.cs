using Display.Models.Dto.OneOneFive;
using Newtonsoft.Json;

namespace Display.Models.Api.OneOneFive.Search;

public class ShaSearchResult
{
    [JsonProperty("sha1")]
    public string Sha1 { get; set; }
    
    [JsonProperty("_")]
    public string Other { get; set; }
    
    [JsonProperty("curr_user")]
    public CurrUser CurrUser { get; set; }
    
    [JsonProperty("user_id")]
    public int UserId { get; set; }
    
    [JsonProperty("data")]
    public ShaSearchResultData Data { get; set; }
    
    [JsonProperty("state")]
    public bool State { get; set; }
    
    [JsonProperty("error")]
    public string Error { get; set; }
}