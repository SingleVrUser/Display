using System;
using Display.Helper.Data;
using Display.Providers;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Display.Views.Windows;

public sealed partial class VideoPlayWindow
{
    public string SourceUrl { get; set; }
    private readonly AppWindow _appwindow;

    //全屏前记录当前状态
    private AppWindowPresenterKind _markPresenterKindBeforeFullScreen;

    public VideoPlayWindow()
    {
        InitializeComponent();

        ExtendsContentIntoTitleBar = true;

        SetTitleBar(AppTitleBar);

        webview.CoreWebView2Initialized += WebView_CoreWebView2Initialized;

        _appwindow = App.GetAppWindow(this);
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

    private void WebView_CoreWebView2Initialized(WebView2 sender, CoreWebView2InitializedEventArgs args)
    {
        Closed += VideoPlayWindow_Closed;
        webview.CoreWebView2.ContainsFullScreenElementChanged += CoreWebView2_ContainsFullScreenElementChanged;

        var cookie = AppSettings._115_Cookie;
        //cookie不为空且可用
        if (string.IsNullOrEmpty(cookie)) return;

        webview.CoreWebView2.CookieManager.DeleteAllCookies();

        foreach (var (_, key, value) in CookieHelper.ProductCookieKeyValue(cookie))
        {
            AddCookie(key, value);
        }
    }

    //WebView 进入后全屏 window也进入全屏
    private void CoreWebView2_ContainsFullScreenElementChanged(Microsoft.Web.WebView2.Core.CoreWebView2 sender, object args)
    {
        if (sender.ContainsFullScreenElement)
        {
            _markPresenterKindBeforeFullScreen = _appwindow.Presenter.Kind;

            ExtendsContentIntoTitleBar = false;
            TitleBarRowDefinition.Height = new GridLength(0);

            _appwindow.SetPresenter(AppWindowPresenterKind.FullScreen);
        }
        else
        {
            _appwindow.SetPresenter(_markPresenterKindBeforeFullScreen);

            ExtendsContentIntoTitleBar = true;
            TitleBarRowDefinition.Height = new GridLength(28);
        }
    }

    private void AddCookie(string key, string value)
    {
        var cookie = webview.CoreWebView2.CookieManager.CreateCookie(key, value, ".115.com", "/");
        webview.CoreWebView2.CookieManager.AddOrUpdateCookie(cookie);
    }

    private Visibility IsSourceUrlNull()
    {
        return SourceUrl == null ? Visibility.Visible : Visibility.Collapsed;
    }

    private void WebView_NavigationStarting(WebView2 sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs args)
    {
        NavigationProgressBar.Visibility = Visibility.Visible;
    }
    
    public static VideoPlayWindow CreateNewWindow(string sourUrl)
    {
        var newWindow = new VideoPlayWindow
        {
            SourceUrl = sourUrl
        };

        newWindow.Activate();

        return newWindow;
    }

    private void WebView_NavigationCompleted(WebView2 sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs args)
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