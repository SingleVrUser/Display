using Display.Setting.Interfaces;
using Display.Setting.Models;

namespace Display.Setting.Impl;

internal class NetworkDiskSetting(ISettingProvider provider) : SettingBase(provider), INetworkDiskSetting
{
    public NetworkDiskItem[] NetworkDisks
    {
        get => throw new System.NotImplementedException();
        set => throw new System.NotImplementedException();
    }
}