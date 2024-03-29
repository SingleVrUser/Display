using Display.Views.Pages.More.DatumList;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.Views.More.DatumList
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private void RootFrame_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is not Frame frame) return;

            frame.Navigate(typeof(FileListPage));
        }
    }
}
