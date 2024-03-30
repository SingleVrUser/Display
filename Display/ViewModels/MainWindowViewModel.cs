using Display.ViewModels.Sub;

namespace Display.ViewModels;

internal class MainWindowViewModel(
    NavigationItemViewModel navigationItemViewModel)
{
    public readonly NavigationItemViewModel NavigationItemViewModel = navigationItemViewModel;


}