using Display.Setting.Interfaces;
using Display.Setting.Models;
using static Display.Models.Data.Constant.DefaultSettings;

namespace Display.Setting.Impl;

internal class DownSetting:SettingBase, IDownSetting
{
    public DownSetting(ISettingProvider provider) : base(provider)
    {
    }

    public string DefaultDownMethod
    {
        get => GetValue(Network._115.DefaultDownMethod);
        set => SetValue(value);
    }

    public DownApiItem[] DownApiList
    {
        get => throw new System.NotImplementedException();
        set => throw new System.NotImplementedException();
    }
}