using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.WebUI;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class BrowserPage : Page
    {
        string cookie = Data.AppSettings._115_Cookie;

        public BrowserPage(Window window)
        {
            this.InitializeComponent();
            //NavigationCacheMode = NavigationCacheMode.Enabled;

            WebViewControl.Source = new Uri("https://115.com/?cid=0&offset=0&mode=wangpan");
            WebViewControl.CoreWebView2Initialized += Webview_CoreWebView2Initialized;

            window.Closed += (sender, args) =>
            {
                WebViewControl.Close();
            };
        }

        private void Webview_CoreWebView2Initialized(WebView2 sender, CoreWebView2InitializedEventArgs args)
        {
            RefreshCookie();
        }

        private void RefreshCookie()
        {
            //cookie不为空且可用
            if (!string.IsNullOrEmpty(cookie))
            {
                WebViewControl.CoreWebView2.CookieManager.DeleteAllCookies();

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


        private void AddCookie(string key, string value)
        {
            var cookie = WebViewControl.CoreWebView2.CookieManager.CreateCookie(key, value, ".115.com", "/");
            WebViewControl.CoreWebView2.CookieManager.AddOrUpdateCookie(cookie);
        }

        private void WebViewControl_NavigationCompleted(WebView2 sender, CoreWebView2NavigationCompletedEventArgs args)
        {
            NavigationProgressBar.Visibility = Visibility.Collapsed;

            HiddenWaterMark();
        }

        private void WebViewControl_NavigationStarting(WebView2 sender, CoreWebView2NavigationStartingEventArgs args)
        {
            NavigationProgressBar.Visibility = Visibility.Visible;


        }


        private async void HiddenWaterMark()
        {
            await WebViewControl.ExecuteScriptAsync(
                @"var tag = document.getElementsByTagName('div');for(var i=0;i<tag.length;i++){if(tag[i].className.indexOf('fp-') != -1){tag[i].remove();console.log(""删除水印"")}};");
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //启动缓存时使用
            //if(cookie!= Data.AppSettings._115_Cookie)
            //{
            //    cookie = Data.AppSettings._115_Cookie;
            //    RefreshCookie();
            //    WebViewControl.Reload();
            //}
        }
    }
}
