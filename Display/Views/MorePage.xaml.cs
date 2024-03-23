using Microsoft.UI.Xaml.Controls;
using Display.Constants;
using Display.CustomWindows;
using Display.Views.More;
using Display.Models.Settings;
using System;
using Display.Models.Data.Enums;
using Display.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.Views;



public sealed partial class MorePage : Page
{
    private MorePageViewModel _viewModel = App.GetService<MorePageViewModel>();
    
    public MorePage()
    {
        this.InitializeComponent();

    }


    private void GridView_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is not MoreMenuItem item) return;

        if (!PageTypeAndEnum.PageTypeAndEnumDict.TryGetValue(item.PageEnum, out var pageType)) return;

        var page = (Page)Activator.CreateInstance(pageType);

        if (item.PageEnum == NavigationViewItemEnum.BrowserPage)
        {
            var window = new CommonWindow();
            window.Content = new BrowserPage(window, isShowButton: true);
            window.Activate();
        }
        else
        {
            CommonWindow.CreateAndShowWindow(page);
        }
    }
}