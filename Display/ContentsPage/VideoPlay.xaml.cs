using Display.Control;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.ContentsPage
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class VideoPlay : Page
    {
        public VideoPlay()
        {
            this.InitializeComponent();

        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            string url = "https://115.com/?cid=0&offset=0&mode=wangpan";

            // Store the item to be used in binding to UI
            var pickCode = e.Parameter as string;
            if (pickCode != null)
            {
                url = $"https://v.anxia.com/?pickcode={pickCode}&share_id=0";
            }

            Browser.webview.Source = new Uri(url);
        }


    }

}
