using Display.Setting.Interfaces;
using static Display.Constants.DefaultSettings;

namespace Display.Setting.Impl;

internal class ImportSetting : SettingBase, IImportSetting
{
    public ImportSetting(ISettingProvider provider) : base(provider)
    {
    }

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