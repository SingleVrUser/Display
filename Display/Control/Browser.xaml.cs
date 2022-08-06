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
using Windows.Web.UI.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.Control
{
    public sealed partial class Browser : UserControl
    {

        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        public string url { get; set; }

        public Browser()
        {
            this.InitializeComponent();

            WebViewControl.CoreWebView2Initialized += Webview_CoreWebView2Initialized;

            //loadData();
        }

        public Browser(string url)
        {
            this.url = url;
        }

        private void loadData()
        {
            //base.OnNavigatedTo(e);

            //// Store the item to be used in binding to UI
            //var url = e.Parameter as string;
            WebViewControl.Source = new Uri(url);

        }

        private void Webview_CoreWebView2Initialized(WebView2 sender, CoreWebView2InitializedEventArgs args)
        {
            var cookie = (string)localSettings.Values["Cookie"];
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

        /// <summary>
        /// 播放按钮点击事件
        /// </summary>
        public event RoutedEventHandler VideoPlayClick;
        private void VideoPlay_Click(object sender, RoutedEventArgs args)
        {
            VideoPlayClick?.Invoke(sender, args);
        }

        ///// <summary>
        ///// 监听跳转事件
        ///// </summary>
        //public event TypedEventHandler<WebView2, CoreWebView2NavigationStartingEventArgs> NavigationStarting;
        //private void WebViewControl_NavigationStarting(WebView2 sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs args)
        //{
        //    NavigationStarting?.Invoke(sender, args);
        //}
    }
}
