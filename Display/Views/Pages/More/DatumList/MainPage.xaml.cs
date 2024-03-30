using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Display.Views.Pages.More.DatumList;

public sealed partial class MainPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    private void RootFrame_OnLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is not Frame frame) return;

        frame.Navigate(typeof(FileListPage));
    }
}