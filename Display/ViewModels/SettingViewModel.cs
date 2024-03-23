using CommunityToolkit.Mvvm.ComponentModel;
using Display.Models.Data.Enums;
using Display.Models.Settings;
using Display.Views.Settings;

namespace Display.ViewModels;


internal partial class SettingViewModel : ObservableObject
{
    public MenuItem[] NavLinks = 
    [
        new MenuItem("账户", "\uE779", NavigationViewItemEnum.AccountSetting),
        new MenuItem("通用", "\uE770", NavigationViewItemEnum.CommonSetting),
        new MenuItem("显示", "\uF0E2", NavigationViewItemEnum.UIShowSetting),
        new MenuItem("播放", "\uE786", NavigationViewItemEnum.PlaySetting ),
        new MenuItem("搜刮", "\uEDE4", NavigationViewItemEnum.SpiderSetting),
        new MenuItem("搜索", "\uF6FA", NavigationViewItemEnum.SearchSetting),
        new MenuItem("存储", "\uE96A", NavigationViewItemEnum.StorageSetting),
        new MenuItem("下载", "\uEBD3", NavigationViewItemEnum.DownSetting)
    ];

    public SettingViewModel()
    {
        _currentLink = NavLinks[0];
    }

    [ObservableProperty]
    private MenuItem _currentLink;

}