using System.Runtime.CompilerServices;
using Display.Setting.Interfaces;

namespace Display.Setting;

/**
 * 抽象类
 */
internal abstract class SettingBase(ISettingProvider provider)
{
    protected T GetValue<T>(T defaultValue = default, [CallerMemberName] string propertyName = null)
        => provider.GetValue(defaultValue, propertyName);

    protected void SetValue<T>(T value, [CallerMemberName] string propertyName = null)
        => provider.SetValue(value, propertyName);

}