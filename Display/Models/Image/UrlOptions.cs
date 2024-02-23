using System.Collections.Generic;

namespace Display.Models.Image;

public class UrlOptions
{
    public string Url { get; set; }

    public Dictionary<string, string> Headers { get; set; }

    public bool IsM3U8 => Url.Contains("m3u8");
}