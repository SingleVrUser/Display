namespace Display.Setting.Interfaces;

internal interface IUpdateSetting
{
    /// <summary>
    /// 是否已经升级了数据库
    /// </summary>
    public bool IsUpdatedDataAccessFrom014 { get; set; }

    /// <summary>
    /// 忽略升级的版本号
    /// </summary>
    public string IgnoreUpdateAppVersion { get; set; }
}