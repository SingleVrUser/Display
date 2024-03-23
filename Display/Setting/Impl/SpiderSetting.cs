using Display.Setting.Interfaces;
using Display.Setting.Models;

using DefaultValue = Display.Constants.DefaultSettings;

namespace Display.Setting.Impl;

internal class SpiderSetting(ISettingProvider provider) : SettingBase(provider), ISpiderSetting
{
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