using Display.Setting.Interfaces;
using DefaultValue = Display.Models.Data.Constant.DefaultSettings;

namespace Display.Setting.Impl;

internal class StartSetting : SettingBase, IStartSetting
{
    public StartSetting(ISettingProvider provider) : base(provider)
    {
    }

    public bool IsNavigationViewPaneOpen
    {
        get => GetValue(DefaultValue.Ui.MainWindow.IsNavigationViewPaneOpen);
        set => SetValue(value);
    }

    public bool IsCheckUpdate
    {
        get => GetValue(DefaultValue.App.IsCheckUpdate);
        set => SetValue(value);
    }

    public int StartPageIndex
    {
        get => GetValue(DefaultValue.Ui.MainWindow.StartPageIndex);
        set => SetValue(value);
    }
}