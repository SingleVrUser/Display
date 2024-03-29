namespace Display.Models.Dto.OneOneFive;

public class TokenInfo
{
    public int state { get; set; }
    public string error { get; set; }
    public int errno { get; set; }
    public string message { get; set; }
    public int code { get; set; }
    public TokenInfoData data { get; set; }
}