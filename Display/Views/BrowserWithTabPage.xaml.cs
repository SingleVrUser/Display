using Display.Control;
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
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class BrowserWithTabPage : Page
    {
        string url = "https://115.com";

        public BrowserWithTabPage()
        {
            this.InitializeComponent();
        }


        private void TabView_AddButtonClick(TabView sender, object args)
        {

        }

        private void TabView_TabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
        {

        }

        private void TabView_Loaded(object sender, RoutedEventArgs e)
        {
            (sender as TabView).TabItems.Add(CreateNewTab(url));
        }

        private TabViewItem CreateNewTab(string url)
        {
            TabViewItem newItem = new TabViewItem();

            newItem.Header = $"{url}";
            newItem.IconSource = new Microsoft.UI.Xaml.Controls.SymbolIconSource() { Symbol = Symbol.Document };


            //var control = new Browser(url = "https://115.com");
            // The content of the tab is often a frame that contains a page, though it could be any UIElement.
            Frame frame = new Frame();

            frame.Navigate(typeof(BrowserPage));

            newItem.Content = frame;



            return newItem;
        }

    }
}
