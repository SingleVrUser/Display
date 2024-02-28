using Display.Setting.Models;

namespace Display.Setting.Interfaces;

internal interface ISpiderSetting
{
    public bool IsAutoSpiderInVideoDisplay { get; set; }

    public SpiderItem[] Spiders { get; set; }
}