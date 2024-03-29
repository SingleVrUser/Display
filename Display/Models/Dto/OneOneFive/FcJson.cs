namespace Display.Models.Dto.OneOneFive;

public class FcJson
{
    public string context { get; set; }
    public string type { get; set; }
    public string name { get; set; }
    public string description { get; set; }
    public string image { get; set; }
    public string[] identifier { get; set; }
    public string datePublished { get; set; }
    public string duration { get; set; }
    public object[] actor { get; set; }
    public string[] genre { get; set; }
    public string[] sameAs { get; set; }
    public string director { get; set; }
    public Aggregaterating aggregateRating { get; set; }
}