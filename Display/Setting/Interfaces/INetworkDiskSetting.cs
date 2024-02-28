using Display.Setting.Models;
namespace Display.Setting.Interfaces;

internal interface INetworkDiskSetting
{
    public NetworkDiskItem[] NetworkDisks { get; set; }
}