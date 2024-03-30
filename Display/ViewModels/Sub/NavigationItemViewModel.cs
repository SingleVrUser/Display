using Display.Providers;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Display.Models.Dto.Settings;
using Display.Models.Enums;

namespace Display.ViewModels.Sub;

internal class NavigationItemViewModel
{
    public readonly ObservableCollection<MenuItem> MenuItems = new(AppSettings.MenuItemsArray);
    public readonly ObservableCollection<MenuItem> FootMenuItems = new(AppSettings.FootMenuItemsArray);

    public NavigationItemViewModel()
    {
        MonitorSettingChanged();

    }

    public object GetMenuItem(NavigationViewItemEnum pageEnum, object settingItem)
    {
        if (pageEnum == NavigationViewItemEnum.SettingPage) return settingItem;

        var tmpItem = MenuItems.FirstOrDefault(item => item.PageEnum == pageEnum);

        if (tmpItem != null) return tmpItem;


        tmpItem = FootMenuItems.FirstOrDefault(item => item.PageEnum == pageEnum);

        return tmpItem;
    }

    private void MonitorSettingChanged()
    {
        MenuItems.CollectionChanged += NavigationMenuItems_CollectionChanged;
        FootMenuItems.CollectionChanged += NavigationMenuItems_CollectionChanged;

        foreach (var menuItem in MenuItems)
            menuItem.PropertyChanged += (_, _) =>
            {
                AppSettings.MenuItemsArray = [.. MenuItems];
            };

        foreach (var footMenuItem in FootMenuItems)
            footMenuItem.PropertyChanged += (_, _) =>
            {
                AppSettings.FootMenuItemsArray = [.. FootMenuItems];
            };
    }

    private void NavigationMenuItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action != NotifyCollectionChangedAction.Add) return;

        AppSettings.MenuItemsArray = [.. MenuItems];
        AppSettings.FootMenuItemsArray = [.. FootMenuItems];
    }


}