using Display.Providers;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Display.Models.Dto.Settings;

namespace Display.ViewModels.Sub;

internal class MoreNavigationItemViewModel
{
    public readonly ObservableCollection<MoreMenuItem> MoreMenuItems = new(AppSettings.MoreMenuItemsArray);

    public MoreNavigationItemViewModel()
    {
        foreach (var moreMenuItem in MoreMenuItems)
            moreMenuItem.PropertyChanged += (_, _) =>
            {
                SaveSetting();
            };

        MoreMenuItems.CollectionChanged += MoreMenuItemsCollectionChanged;
    }

    private void MoreMenuItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action != NotifyCollectionChangedAction.Add) return;

        SaveSetting();
    }

    private void SaveSetting()
    {
        AppSettings.MoreMenuItemsArray = [.. MoreMenuItems];
    }
}