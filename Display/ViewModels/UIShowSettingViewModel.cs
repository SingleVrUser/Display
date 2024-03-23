using Display.ViewModels.Sub;

namespace Display.ViewModels;

internal class UIShowSettingViewModel(
    NavigationItemViewModel navigationItemViewModel,
    MoreNavigationItemViewModel moreNavigationItemViewModel)
{
    public readonly NavigationItemViewModel NavigationItemViewModel = navigationItemViewModel;

    public readonly MoreNavigationItemViewModel MoreNavigationItemViewModel = moreNavigationItemViewModel;
}