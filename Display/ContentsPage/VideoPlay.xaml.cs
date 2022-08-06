using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.ContentsPage
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class VideoPlay : Page
    {
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        //public string sourceUrl { get; set; } = "https://115.com/?cid=0&offset=0&mode=wangpan";

        public VideoPlay()
        {
            this.InitializeComponent();

            //webview.Source = new Uri("https://115.com/?cid=0&offset=0&mode=wangpan");

            webview.CoreWebView2Initialized += Webview_CoreWebView2Initialized;

            //webview.WebMessageReceived += Webview_WebMessageReceived;

        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            string url = "https://115.com/?cid=2356184327894596620&offset=0&mode=wangpan";

            // Store the item to be used in binding to UI
            var pickCode = e.Parameter as string;
            if (pickCode != null)
            {
                url = $"https://v.anxia.com/?pickcode={pickCode}&share_id=0";
            }

            webview.Source = new Uri(url);
        }

        private void Webview_CoreWebView2Initialized(WebView2 sender, CoreWebView2InitializedEventArgs args)
        {
            //webview.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;
            //var item2 = webview.ExecuteScriptAsync($"window.open('https://baidu.com','_blank');");

            webview.CoreWebView2.ContainsFullScreenElementChanged += CoreWebView2_ContainsFullScreenElementChanged;
            var cookie = (string)localSettings.Values["Cookie"];
            //cookie不为空且可用
            if (!string.IsNullOrEmpty(cookie))
            {
                webview.CoreWebView2.CookieManager.DeleteAllCookies();

                var cookiesList = cookie.Split(';');
                foreach (var cookies in cookiesList)
                {
                    cookie = cookies;
                    var item = cookies.Split('=');
                    string key = item[0].Trim();
                    string value = item[1].Trim();
                    AddCookie(key, value);
                }
            }

        }

        private void CoreWebView2_ContainsFullScreenElementChanged(Microsoft.Web.WebView2.Core.CoreWebView2 sender, object args)
        {
            // WebView 中的内容处于全屏状体
            if (sender.ContainsFullScreenElement)
            {
                // 将 app 设置为全屏模式
                //applicationView.TryEnterFullScreenMode();
            }
            else
            {
                // 将 app 退出全屏模式
                //applicationView.ExitFullScreenMode();
            }
        }

        private void AddCookie(string key, string value)
        {
            var cookie = webview.CoreWebView2.CookieManager.CreateCookie(key, value, ".115.com", "/");
            webview.CoreWebView2.CookieManager.AddOrUpdateCookie(cookie);
        }

        private void webview_NavigationStarting(WebView2 sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs args)
        {
            NavigationProgressBar.Visibility = Visibility.Visible;
        }

        private void webview_NavigationCompleted(WebView2 sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs args)
        {
            NavigationProgressBar.Visibility = Visibility.Collapsed;
        }
    }

}
