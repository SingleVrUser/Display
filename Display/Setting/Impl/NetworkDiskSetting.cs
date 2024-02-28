using Display.Setting.Interfaces;
using Display.Setting.Models;

namespace Display.Setting.Impl;

internal class NetworkDiskSetting : SettingBase, INetworkDiskSetting
{
    public NetworkDiskSetting(ISettingProvider provider) : base(provider)
    {
    }

    public NetworkDiskItem[] NetworkDisks
    {
        get => throw new System.NotImplementedException();
        set => throw new System.NotImplementedException();
    }
}