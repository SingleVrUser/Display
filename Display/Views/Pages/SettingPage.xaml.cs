using Display.Constants;
using Display.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace Display.Views.Pages;

public sealed partial class SettingPage
{
    private SettingViewModel _viewModel;

    public SettingPage()
    {
        _viewModel = App.GetService<SettingViewModel>();

        InitializeComponent();
    }

    private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        SettingFrame.Navigate(_viewModel.CurrentLink.PageType, NotificationQueue);
    }
}