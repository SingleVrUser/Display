using Display.Models.Data;
using Display.Models.Settings;
using System.Linq;

namespace Display.ViewModels;

internal class MorePageViewModel
{
    public readonly MoreMenuItem[] MoreMenuItems = AppSettings.MoreMenuItemsArray.Where(i => i.IsVisible).ToArray();
}