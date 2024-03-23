using Display.Setting.Interfaces;
using DefaultValue = Display.Constants.DefaultSettings;

namespace Display.Setting.Impl;

internal class StartSetting(ISettingProvider provider) : SettingBase(provider), IStartSetting
{
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