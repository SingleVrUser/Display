using System;

namespace Display.Models.Dto.OneOneFive;

public class CookieFormat
{
    public string domain { get; set; } = ".115.com";
    public long expirationDate { get; set; } = DateTimeOffset.Now.AddMonths(1).ToUnixTimeSeconds();
    public bool hostOnly { get; set; } = false;
    public bool httpOnly { get; set; } = true;
    public string name { get; set; }
    public string path { get; set; } = "/";
    public string sameSite { get; set; } = null;
    public bool secure { get; set; } = false;
    public bool session { get; set; } = false;
    public string storeId { get; set; } = null;
    public string value { get; set; }
}