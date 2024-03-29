namespace Display.Models.Dto.OneOneFive;

public class TokenInfoData
{
    public int user_id { get; set; }
    public string user_name { get; set; }
    public string email { get; set; }
    public string mobile { get; set; }
    public string country { get; set; }
    public long is_vip { get; set; }
    public int mark { get; set; }
    public string alert { get; set; }
    public int is_chang_passwd { get; set; }
    public int is_first_login { get; set; }
    public int bind_mobile { get; set; }
    public Face face { get; set; }
    public Cookie cookie { get; set; }
    public string from { get; set; }
    public object is_trusted { get; set; }
}