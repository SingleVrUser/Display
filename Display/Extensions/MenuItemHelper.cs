using System.Collections.ObjectModel;
using System.Linq;
using Display.Models.Dto.Settings;
using Display.Models.Enums;
using LiveChartsCore.Collections;

namespace Display.Extensions;

internal static class MenuItemHelper
{
    public static ObservableCollection<MenuItem> ToObservableCollection(this NavigationViewItemEnum[] pageEnumArray, MenuItem[] allArray)
    {
        return new ObservableCollection<MenuItem>(pageEnumArray.Select(pageEnum =>
            allArray.FirstOrDefault(item => item.PageEnum == pageEnum)));
    }
}