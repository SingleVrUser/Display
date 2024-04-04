using Display.Providers;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Display.Constants;
using Display.Extensions;
using Display.Models.Dto.Settings;
using Display.Models.Records;

namespace Display.ViewModels.Sub;

internal class MoreNavigationItemViewModel
{
    public readonly ObservableCollection<MoreMenuItem> MoreMenuItems;

    public MoreNavigationItemViewModel()
    {
        MoreMenuItems = new ObservableCollection<MoreMenuItem>(AppSettings.MoreMenuItemEnumArray
            .Select(enumAndVisible =>
            {
                var firstOrDefault = MenuItems.MoreMenuItems.FirstOrDefault(item => item.PageEnum == enumAndVisible.PageEnum);
                if (firstOrDefault != null && !enumAndVisible.IsVisible)
                    firstOrDefault.IsVisible = false;
                return firstOrDefault;
            })
            .Where(i=>i != null)
        );
    }

    public void SaveSetting()
    {
        AppSettings.MoreMenuItemEnumArray = [.. MoreMenuItems.Select(i=>new PageEnumAndIsVisible(i.PageEnum, i.IsVisible))];
    }
}