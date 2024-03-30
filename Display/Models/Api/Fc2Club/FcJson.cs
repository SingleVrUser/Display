using Newtonsoft.Json;

namespace Display.Models.Api.Fc2Club;

public class FcJson
{
    [JsonProperty("context")]
    public string Context { get; set; }
    
    [JsonProperty("type")]
    public string Type { get; set; }
    
    [JsonProperty("name")]
    public string Name { get; set; }
    
    [JsonProperty("description")]
    public string Description { get; set; }
    
    [JsonProperty("image")]
    public string Image { get; set; }
    
    [JsonProperty("identifier")]
    public string[] Identifier { get; set; }
    
    [JsonProperty("datePublished")]
    public string DatePublished { get; set; }
    
    [JsonProperty("duration")]
    public string Duration { get; set; }
    
    [JsonProperty("actor")]
    public object[] Actor { get; set; }
    
    [JsonProperty("genre")]
    public string[] Genre { get; set; }
    
    [JsonProperty("sameAs")]
    public string[] SameAs { get; set; }
    
    [JsonProperty("director")]
    public string Director { get; set; }
    
    [JsonProperty("aggregateRating")]
    public Aggregaterating AggregateRating { get; set; }
}