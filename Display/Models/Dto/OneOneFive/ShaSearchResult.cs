namespace Display.Models.Dto.OneOneFive;

public class ShaSearchResult
{
    public string sha1 { get; set; }
    public string _ { get; set; }
    public Curr_User curr_user { get; set; }
    public int user_id { get; set; }
    public ShaSearchResultData data { get; set; }
    public bool state { get; set; }
    public string error { get; set; }
}