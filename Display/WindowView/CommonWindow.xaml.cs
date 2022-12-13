using Microsoft.UI.Xaml;
using System.IO;
using Windows.ApplicationModel;
using WinUIEx;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.WindowView
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CommonWindow : Window
    {
        public CommonWindow(string title = "Display")
        {
            this.InitializeComponent();
            this.Title = title;

            var appwindow = App.getAppWindow(this);
            appwindow.SetIcon(Path.Combine(Package.Current.InstalledLocation.Path, "Assets/pokeball.ico"));
        }


    }
}
