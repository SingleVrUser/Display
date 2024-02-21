using System;
using System.IO;
using Windows.ApplicationModel;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.CustomWindows
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SingleFrameWindow
    {
        public SingleFrameWindow()
        {
            this.InitializeComponent();

            this.Title = "Display";
            var appwindow = App.GetAppWindow(this);
            appwindow.SetIcon(Path.Combine(Package.Current.InstalledLocation.Path, "Assets/pokeball.ico"));
        }

        public void NavigationToPage(Type page)
        {
            //该Page含有WebView，关闭窗口时必须先关闭WebView，不然会报错
            if (page.Name == "Import115DataToLocalDataAccess")
            {
                return;
            }
            ContentFrame.Navigate(page);
        }

        private void NavigationToPageWithWebView(Type page, Microsoft.UI.Xaml.Window window)
        {
            ContentFrame.Navigate(page, window);
        }

        public static void CreateWindow(Type page)
        {
            var window = new SingleFrameWindow();
            window.NavigationToPageWithWebView(page, window);
            window.Activate();
        }

    }
}
