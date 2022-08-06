using Display.ContentsPage;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.WindowView
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SingleFrameWindow : Window
    {
        public SingleFrameWindow()
        {
            this.InitializeComponent();

            this.Title = "Display";
            var appwindow = App.getAppWindow(this);
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

        public void NavigationToPageWithWebView(Type page, Window window)
        {
            ContentFrame.Navigate(page,window);
        }

    }
}
