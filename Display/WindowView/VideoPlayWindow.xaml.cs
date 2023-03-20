using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using Windows.Storage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class VideoPlayWindow : Window
    {
        public string sourceUrl { get; set; }
        private AppWindow appwindow;
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        //全屏前记录当前状态
        private AppWindowPresenterKind _markPresenterKindBeforeFullScreen;

        public VideoPlayWindow()
        {
            this.InitializeComponent();

            ExtendsContentIntoTitleBar = true;

            this.SetTitleBar(AppTitleBar);

            webview.CoreWebView2Initialized += Webview_CoreWebView2Initialized;

            appwindow = App.getAppWindow(this);
        }

        private void VideoPlayWindow_Closed(object sender, WindowEventArgs args)
        {
            //BUG，窗口含有WebView2，关闭窗口后会奔溃
            if (webview.CoreWebView2 != null)
            {
                webview.CoreWebView2.Stop();
            }
            webview.Close();
        }

        private void Webview_CoreWebView2Initialized(WebView2 sender, CoreWebView2InitializedEventArgs args)
        {
            this.Closed += VideoPlayWindow_Closed;
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

        //WebView 进入后全屏 window也进入全屏
        private void CoreWebView2_ContainsFullScreenElementChanged(Microsoft.Web.WebView2.Core.CoreWebView2 sender, object args)
        {
            if (sender.ContainsFullScreenElement)
            {
                _markPresenterKindBeforeFullScreen = appwindow.Presenter.Kind;
                appwindow.SetPresenter(AppWindowPresenterKind.FullScreen);

                this.ExtendsContentIntoTitleBar = false;
                TitleBarRowDefinition.Height = new(0);
            }
            else
            {
                appwindow.SetPresenter(_markPresenterKindBeforeFullScreen);

                this.ExtendsContentIntoTitleBar = true;
                TitleBarRowDefinition.Height = new(28);
            }
        }

        private void AddCookie(string key, string value)
        {
            var cookie = webview.CoreWebView2.CookieManager.CreateCookie(key, value, ".115.com", "/");
            webview.CoreWebView2.CookieManager.AddOrUpdateCookie(cookie);
        }

        private Visibility isSourceUrlNull()
        {
            return sourceUrl == null ? Visibility.Visible : Visibility.Collapsed;
        }

        public static VideoPlayWindow createNewWindow(string sourUrl)
        {
            VideoPlayWindow newWindow = new VideoPlayWindow();
            newWindow.sourceUrl = sourUrl;
            newWindow.Activate();

            return newWindow;
        }

        private void webview_NavigationStarting(WebView2 sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs args)
        {
            NavigationProgressBar.Visibility = Visibility.Visible;
        }

        private void webview_NavigationCompleted(WebView2 sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs args)
        {
            NavigationProgressBar.Visibility = Visibility.Collapsed;

            HiddenWaterMark();
            HiddenAdvertising();
        }


        private async void HiddenWaterMark()
        {
            await webview.ExecuteScriptAsync(
                @"var tag = document.getElementsByTagName('div');for(var i=0;i<tag.length;i++){if(tag[i].className.indexOf('fp-') != -1){tag[i].remove();console.log(""删除水印"")}};");
        }

        private async void HiddenAdvertising()
        {
            await webview.ExecuteScriptAsync(
                "document.getElementById('mini-dialog').remove()");

            await webview.ExecuteScriptAsync(
                "document.getElementById('js_common_mini-dialog').remove()");

            await webview.ExecuteScriptAsync(
                "document.getElementById('js_common_act-enter').remove()");
        }
    }
}
