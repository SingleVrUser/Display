using Display.Setting.Interfaces;
using static Display.Constants.DefaultSettings;

namespace Display.Setting.Impl;

internal class ImportSetting(ISettingProvider provider) : SettingBase(provider), IImportSetting
{
    public bool IsToastAfterImportDataAccess
    {
        get => GetValue(Handle.IsToastAfterImportDataAccess);
        set => SetValue(value);
    }

    public bool IsSpiderAfterImportDataAccess
    {
        get => GetValue(Handle.IsSpiderAfterImportDataAccess);
        set => SetValue(value);
    }
}