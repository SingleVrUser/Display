using Display.Providers;
using System.Linq;
using Display.Constants;
using Display.Models.Dto.Settings;

namespace Display.ViewModels;

internal class MorePageViewModel
{
    public readonly MoreMenuItem[] MoreMenuItems = AppSettings.MoreMenuItemEnumArray
        .Select(pageEnum =>
        {
            var firstOrDefault = MenuItems.MoreMenuItems.FirstOrDefault(item => item.PageEnum == pageEnum.PageEnum);
            if (firstOrDefault != null && !pageEnum.IsVisible)
                firstOrDefault.IsVisible = false;
            
            return firstOrDefault;
        })
        .Where(i=>i is { IsVisible: true }).ToArray();
    
}