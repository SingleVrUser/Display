using Display.Setting.Interfaces;
using DefaultValue = Display.Models.Data.Constant.DefaultSettings;

namespace Display.Setting.Impl;

internal class UpdateSetting: SettingBase,IUpdateSetting
{
    public UpdateSetting(ISettingProvider provider) : base(provider)
    {
    }

    /// <summary>
    /// 是否已经升级了数据库(从0.1.16.14)
    /// </summary>
    public bool IsUpdatedDataAccessFrom014
    {
        get => GetValue(DefaultValue.App.IsUpdatedDataAccessFrom014);
        set => SetValue(value);
    }

    /// <summary>
    /// 忽略升级的版本号
    /// </summary>
    public string IgnoreUpdateAppVersion
    {
        get => GetValue(DefaultValue.App.IgnoreUpdateAppVersion);
        set => SetValue(value);
    }

}