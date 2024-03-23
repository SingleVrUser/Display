using Display.Constants;
using Display.Setting.Interfaces;
using Display.Setting.Models;

namespace Display.Setting.Impl;

internal class DownSetting(ISettingProvider provider) : SettingBase(provider), IDownSetting
{
    public string DefaultDownMethod
    {
        get => GetValue(DefaultSettings.Network._115.DefaultDownMethod);
        set => SetValue(value);
    }

    public DownApiItem[] DownApiList
    {
        get => throw new System.NotImplementedException();
        set => throw new System.NotImplementedException();
    }
}