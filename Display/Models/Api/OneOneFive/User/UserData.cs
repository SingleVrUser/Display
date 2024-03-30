using System;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Display.Models.Api.OneOneFive.User;

/// <summary>
/// 用户信息相关
/// </summary>
public class UserData
{
    [JsonProperty("device")]
    public int Device { get; set; }
    
    [JsonProperty("rank")]
    public int Rank { get; set; }
    
    [JsonProperty("liang")]
    public int Liang { get; set; }

    /// <summary>
    /// 每个mark对应一个会员类型
    /// </summary>
    [JsonProperty("mark")]
    public int Mark { get; set; }

    [JsonProperty("mark1")]
    public int Mark1 { get; set; }
    
    [JsonProperty("vip")]
    public int Vip { get; set; }

    /// <summary>
    /// 会员到期时间
    /// </summary>
    [JsonProperty("expire")]
    public long Expire { get; set; }
    
    [JsonProperty("global")]
    public int Global { get; set; }
    
    [JsonProperty("forever")]
    public int Forever { get; set; }
    
    [JsonProperty("is_privilege")]
    public bool IsPrivilege { get; set; }
    
    [JsonProperty("privilege")]
    public Privilege Privilege { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    /// 
    [JsonProperty("user_name")]
    public string UserName { get; set; }

    /// <summary>
    /// 头像图片地址
    /// </summary>
    [JsonProperty("face")]
    public string Face { get; set; }
    
    [JsonProperty("user_id")]
    public int UserId { get; set; }

    [JsonInclude] public string VipName => GetVipName(Mark1);
    
    private static string GetVipName(int mark)
    {
        return mark switch
        {
            1 => "月费VIP",
            3 => "体验VIP",
            7 => "体验VIP",
            11 => "体验VIP",
            15 => "青铜会员",
            127 => "黄金VIP",
            255 => "铂金VIP",
            1023 => "年费VIP",
            10239 => "年费VIP",
            1048575 => "长期VIP",
            _ => "未知"
        };
    }
}