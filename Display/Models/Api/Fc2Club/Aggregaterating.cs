using Newtonsoft.Json;

namespace Display.Models.Api.Fc2Club;

public class Aggregaterating
{
    [JsonProperty("type")]
    public string Type { get; set; }
    
    [JsonProperty("bestRating")]
    public int BestRating { get; set; }
    
    [JsonProperty("worstRating")]
    public int WorstRating { get; set; }
    
    [JsonProperty("ratingCount")]
    public int RatingCount { get; set; }
    
    [JsonProperty("ratingValue")]
    public int RatingValue { get; set; }
}