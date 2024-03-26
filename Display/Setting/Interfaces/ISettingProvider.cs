namespace Display.Setting.Interfaces;

interface ISettingProvider
{
    public T GetValue<T>(T defaultValue = default, string propertyName = null);
    public void SetValue<T>(T value, string propertyName = null);
}