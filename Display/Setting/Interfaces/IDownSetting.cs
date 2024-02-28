using Display.Setting.Models;

namespace Display.Setting.Interfaces;

internal interface IDownSetting
{
    //默认下载方式
    public string DefaultDownMethod { get; set; }

    public DownApiItem[] DownApiList { get; set; }
}