namespace Display.Models.Dto.OneOneFive;

/// <summary>
/// 用户信息相关
/// </summary>
public class UserData
{
    public int device { get; set; }
    public int rank { get; set; }
    public int liang { get; set; }

    /// <summary>
    /// 每个mark对应一个会员类型
    /// </summary>
    public int mark { get; set; }

    public int mark1 { get; set; }
    public int vip { get; set; }

    /// <summary>
    /// 会员到期时间
    /// </summary>
    public long expire { get; set; }
    public int global { get; set; }
    public int forever { get; set; }
    public bool is_privilege { get; set; }
    public Privilege privilege { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    public string user_name { get; set; }

    /// <summary>
    /// 头像图片地址
    /// </summary>
    public string face { get; set; }
    public int user_id { get; set; }

    public static string getVipName(int mark)
    {
        string vip_name = "";
        switch (mark)
        {
            case 1:
                vip_name = "月费VIP";
                break;
            case 11:
                vip_name = "体验VIP";
                break;
            case 127:
                vip_name = "黄金VIP";
                break;
            case 255:
                vip_name = "铂金VIP";
                break;
            case 1023:
                vip_name = "年费VIP";
                break;
            case 10239:
                vip_name = "年费VIP";
                break;
            case 1048575:
                vip_name = "长期VIP";
                break;
            case 15:
                vip_name = "青铜会员";
                break;
            case 7:
                vip_name = "体验VIP";
                break;
            case 3:
                vip_name = "体验VIP";
                break;
        }

        return vip_name;
    }
}