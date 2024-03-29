using Newtonsoft.Json;

namespace Display.Models.Dto.OneOneFive;

public class SearchHistory
{
    [JsonProperty(propertyName: "id")]
    public long Id { get; set; }

    [JsonProperty(propertyName: "keyword")]
    public string Keyword { get; set; }

    public SearchHistory()
    {

    }

    public SearchHistory(string keyword)
    {
        Keyword = keyword;
    }
}