using Microsoft.UI.Xaml;
using System.IO;
using Windows.ApplicationModel;
using CommunityToolkit.WinUI.UI;
using Microsoft.UI.Xaml.Controls;
using WinUIEx;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.WindowView
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CommonWindow
    {
        private const string DefaultTitle = "Display";

        public CommonWindow(string title = DefaultTitle)
        {
            this.InitializeComponent();
            this.Title = title;

            var appWindow = App.getAppWindow(this);
            appWindow.SetIcon(Path.Combine(Package.Current.InstalledLocation.Path, "Assets/pokeball.ico"));

            //Mica
            Backdrop = new MicaSystemBackdrop();
        }

        public static void CreateAndShowWindow(Page page,string title= DefaultTitle)
        {
            var window = new CommonWindow(title)
            {
                Content = page
            };
            window.Activate();
        }
    }
}
