using Display.Providers;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Display.Models.Dto.Settings;
using Display.Models.Enums;
using Display.Models.Records;

namespace Display.ViewModels.Sub;

internal class NavigationItemViewModel
{
    public readonly ObservableCollection<MenuItem> MenuItems = new(AppSettings.MenuItemEnumArray
        .Select(enumAndVisible =>
        {
            var firstOrDefault = Constants.MenuItems.MainMenuItems.FirstOrDefault(item => item.PageEnum == enumAndVisible.PageEnum);
            if (firstOrDefault != null && !enumAndVisible.IsVisible)
                firstOrDefault.IsVisible = false;
            return firstOrDefault;
        })
        .Where(i=>i != null)
    );
    
    public readonly ObservableCollection<MenuItem> FootMenuItems = new(AppSettings.FootMenuItemEnumArray
        .Select(enumAndVisible =>
        {
            var firstOrDefault = Constants.MenuItems.FootMenuItems.FirstOrDefault(item => item.PageEnum == enumAndVisible.PageEnum);
            if (firstOrDefault != null && !enumAndVisible.IsVisible)
                firstOrDefault.IsVisible = false;
            return firstOrDefault;
        })
        .Where(i=>i != null)
    );
    
    public object GetMenuItem(NavigationViewItemEnum pageEnum, object settingItem)
    {
        if (pageEnum == NavigationViewItemEnum.SettingPage) return settingItem;

        var tmpItem = MenuItems.FirstOrDefault(item => item.PageEnum == pageEnum);

        if (tmpItem != null) return tmpItem;

        tmpItem = FootMenuItems.FirstOrDefault(item => item.PageEnum == pageEnum);

        return tmpItem;
    }


    public void SaveFootMenuItemEnumArray()
    {
        AppSettings.FootMenuItemEnumArray = [.. FootMenuItems.Select(i => new PageEnumAndIsVisible(i.PageEnum, i.IsVisible))];
    }

    public void SaveMenuItemEnumArray()
    {
        AppSettings.MenuItemEnumArray = [.. MenuItems.Select(i => new PageEnumAndIsVisible(i.PageEnum, i.IsVisible))];
    }


}