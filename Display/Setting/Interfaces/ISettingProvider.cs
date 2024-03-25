namespace Display.Setting.Interfaces;

internal abstract class ISettingProvider
{
    public abstract T GetValue<T>(T defaultValue = default, string propertyName = null);
    public abstract void SetValue<T>(T value, string propertyName = null);
}