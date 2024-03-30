namespace Display.Models.Vo.Forum;

public class BaiduShareInfo
{
    public string ShareLink { get; set; }
    public string SharePassword { get; set; }
    public string ShareLinkWithPwd => $"{ShareLink}?pwd={SharePassword}";
}