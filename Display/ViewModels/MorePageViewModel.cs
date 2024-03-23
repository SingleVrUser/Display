using System.Linq;
using Display.Models.Data;
using Display.Models.Settings;

namespace Display.ViewModels;

internal class MorePageViewModel
{
    public readonly MoreMenuItem[] MoreMenuItems = AppSettings.MoreMenuItemsArray.Where(i => i.IsVisible).ToArray();
}