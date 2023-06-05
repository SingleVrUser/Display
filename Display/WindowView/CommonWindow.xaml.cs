using System.IO;
using Windows.ApplicationModel;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

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

        public CommonWindow(string title = DefaultTitle,int width=0,int height=0)
        {
            this.InitializeComponent();
            this.Title = title;

            // 调整大小
            if (width != 0 && height != 0)
            {
                this.Width = width;
                this.Height = height;
            }

            var appWindow = App.getAppWindow(this);
            appWindow.SetIcon(Path.Combine(Package.Current.InstalledLocation.Path, "Assets/pokeball.ico"));

            //Mica
            //Backdrop = new MicaSystemBackdrop();

            this.SystemBackdrop = new MicaBackdrop();

        }

        public static void CreateAndShowWindow(Page page,string title= DefaultTitle, int width = 0, int height = 0)
        {
            var window = new CommonWindow(title, width, height)
            {
                Content = page
            };
            window.Activate();
        }
    }
}
