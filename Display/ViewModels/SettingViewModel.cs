using CommunityToolkit.Mvvm.ComponentModel;
using Display.Models.Dto.Settings;
using Display.Models.Dto.Settings.Options;
using Display.Models.Enums;
using Display.Views.Pages.Settings;

namespace Display.ViewModels;


internal partial class SettingViewModel : ObservableObject
{
    public readonly MenuItem[] NavLinks =
    [
        new MenuItem(NavigationViewItemEnum.CommonSetting,"通用", "\uE770", typeof(CommonPage)),
        new MenuItem(NavigationViewItemEnum.UIShowSetting, "显示", "\uF0E2", typeof(UIShowPage)),
        new MenuItem(NavigationViewItemEnum.PlaySetting, "播放", "\uE786", typeof(PlayPage)),
        new MenuItem(NavigationViewItemEnum.SpiderSetting, "搜刮", "\uEDE4", typeof(SpiderPage)),
        new MenuItem(NavigationViewItemEnum.SearchSetting, "搜索", "\uF6FA", typeof(SearchPage)),
        new MenuItem(NavigationViewItemEnum.StorageSetting, "存储", "\uE96A", typeof(StoragePage)),
        new MenuItem(NavigationViewItemEnum.DownSetting, "下载", "\uEBD3", typeof(DownPage)),
        new MenuItem(NavigationViewItemEnum.AccountSetting, "账户", "\uE779", typeof(AccountPage))
    ];

    public SettingViewModel()
    {
        _currentLink = NavLinks[0];
    }

    [ObservableProperty]
    private MenuItem _currentLink;

}