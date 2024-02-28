using System.Runtime.CompilerServices;
using Display.Setting.Interfaces;

namespace Display.Setting;

/**
 * 抽象类
 */
internal abstract class SettingBase
{
    protected ISettingProvider Provider;

    private SettingBase(){}

    protected SettingBase(ISettingProvider provider)
    {
        Provider = provider;
    }

    protected T GetValue<T>(T defaultValue = default, [CallerMemberName] string propertyName = null)
        => Provider.GetValue(defaultValue, propertyName);

    protected void SetValue<T>(T value, [CallerMemberName] string propertyName = null)
        => Provider.SetValue(value, propertyName);

}