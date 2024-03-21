using System.Collections.Generic;

namespace Display.Setting.Models;

public class SpiderItem
{
    public string Name { get; set; }

    public string BaseUrl { get; set; }

    public Dictionary<string, string> Headers { get; set; }

    public bool IsOn { get; set; }
}