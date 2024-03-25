using Microsoft.UI.Xaml.Navigation;
using System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.Views.More
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class VideoPlay
    {
        public VideoPlay()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            string url;

            if (e.Parameter is string pickCode)
                url = $"https://v.anxia.com/?pickcode={pickCode}&share_id=0";
            else
                url = "https://115.com/?cid=0&offset=0&mode=wangpan";

            Browser.WebView.Source = new Uri(url);
        }
    }

}
