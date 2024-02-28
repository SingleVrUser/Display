using Display.Models.Data;
using Display.Setting.Interfaces;

namespace Display.Setting.Impl;

internal class CacheSetting : SettingBase, ICacheSetting
{
    public CacheSetting(ISettingProvider provider) : base(provider)
    {
    }

    public int GetActorInfoLastIndex
    {
        get => GetValue(Constant.DefaultSettings.Network.GetActorInfoLastIndex);
        set => SetValue(value);
    }
}