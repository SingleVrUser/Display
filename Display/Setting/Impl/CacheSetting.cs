using Display.Models.Data;
using Display.Setting.Interfaces;

namespace Display.Setting.Impl;

internal class CacheSetting(ISettingProvider provider) : SettingBase(provider), ICacheSetting
{
    public int GetActorInfoLastIndex
    {
        get => GetValue(Constant.DefaultSettings.Network.GetActorInfoLastIndex);
        set => SetValue(value);
    }
}