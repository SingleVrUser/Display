using Display.ViewModels;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.Views.Settings;

public sealed partial class MainPage : Page
{
    private SettingViewModel _viewModel;

    public MainPage()
    {
        _viewModel = App.GetService<SettingViewModel>();

        InitializeComponent();
    }

    private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        SettingFrame.Navigate(_viewModel.CurrentLink.NavPageType, NotificationQueue);
    }
}