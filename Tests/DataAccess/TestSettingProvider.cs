using Display.Setting.Interfaces;

namespace Tests.DataAccess;

internal class TestSettingProvider : ISettingProvider
{
    public override T GetValue<T>(T defaultValue = default, string propertyName = null)
    {
        throw new NotImplementedException();
    }

    public override void SetValue<T>(T value, string propertyName = null)
    {
        throw new NotImplementedException();
    }
}