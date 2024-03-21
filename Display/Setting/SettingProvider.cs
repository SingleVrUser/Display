using Display.Helper.Data;
using Display.Setting.Interfaces;

namespace Display.Setting;

internal class SettingProvider : ISettingProvider
{
    public override T GetValue<T>(T defaultValue = default, string propertyName = null)
    => Settings.GetValue(propertyName, defaultValue);

    public override void SetValue<T>(T value, string propertyName = null)
    => Settings.SetValue(propertyName, value);
}