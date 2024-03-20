using CommunityToolkit.Mvvm.ComponentModel;
using Display.Models.Settings;
using Display.Views.Settings;

namespace Display.ViewModels;


internal partial class SettingViewModel : ObservableObject
{
    public NavLink[] NavLinks =
    [
        new NavLink { Label = "账户", Glyph = "\uE770", NavPageType = typeof(AccountPage)},
        new NavLink { Label = "通用", Glyph = "\uF6FA", NavPageType = typeof(CommonPage)},
        new NavLink { Label = "播放", Glyph = "\uEBD3", NavPageType = typeof(PlayPage)},
        new NavLink { Label = "搜刮", Glyph = "\uEDE4", NavPageType = typeof(SpiderPage)},
        new NavLink { Label = "搜索", Glyph = "\uEBD3", NavPageType = typeof(SearchPage)},
        new NavLink { Label = "路径", Glyph = "\uEBD3", NavPageType = typeof(PathPage)},
        new NavLink { Label = "下载", Glyph = "\uEBD3", NavPageType = typeof(DownPage)}
    ];

    public SettingViewModel()
    {
        _currentLink = NavLinks[3];
    }

    [ObservableProperty]
    private NavLink _currentLink;

}