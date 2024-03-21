using Display.Models.Data;
using Display.Setting.Interfaces;
using Display.Setting.Models;

using DefaultValue = Display.Models.Data.Constant.DefaultSettings;

namespace Display.Setting.Impl;

internal class SpiderSetting : SettingBase, ISpiderSetting
{
    public SpiderSetting(ISettingProvider provider) : base(provider)
    {
    }

    public bool IsAutoSpiderInVideoDisplay
    {
        get => GetValue(DefaultValue.Player.VideoDisplay.IsSpiderVideoInfo);
        set => SetValue(value);
    }

    public SpiderItem[] Spiders
    {
        get => throw new System.NotImplementedException();
        set => throw new System.NotImplementedException();
    }
}