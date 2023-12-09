// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Newtonsoft.Json;
using System;
using System.IO;
using Display.Data;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class VerifyAccountPage : Page
    {
        private static string RequestUrl => $"https://captchaapi.115.com/?ac=security_code&type=web&cb=Close911_{DateTimeOffset.Now.ToUnixTimeMilliseconds()}";

        public bool IsSucceeded;

        private readonly Window _currentWindow;

        //private string sign;


        public VerifyAccountPage(Window window)
        {
            this.InitializeComponent();

            _currentWindow = window;

            Browser.webview.Source = new Uri(RequestUrl);

            Browser.WebMessageReceived += Browser_WebMessageReceived;

            window.Closed += Window_Closed;
        }

        private void Window_Closed(object sender, WindowEventArgs args)
        {
            VerifyAccountCompleted?.Invoke(this, IsSucceeded);
        }

        //private string GetSign()
        //{
        //    if(sign!=null) return sign;

        //    //var result = WebApi.GlobalWebApi.GetVerifyAccountInfo();

        //    return string.Empty;
        //}

        public event EventHandler<bool> VerifyAccountCompleted;
        private async void Browser_WebMessageReceived(Microsoft.Web.WebView2.Core.CoreWebView2 sender, Microsoft.Web.WebView2.Core.CoreWebView2WebResourceResponseReceivedEventArgs args)
        {
            if (args.Response == null) return;

            if (!args.Request.Uri.Contains("webapi.115.com/user/captcha")) return;

            if (args.Response == null || args.Response.ReasonPhrase != "OK") return;

            var stream = await args.Response.GetContentAsync();

            var content = stream.AsStreamForRead();

            using TextReader tr = new StreamReader(content);
            var re = await tr.ReadToEndAsync();

            var result = JsonConvert.DeserializeObject<Models.VerifyAccountResult>(re);

            // TODO 在添加任务的异常中可用，但在播放m3u8视频的异常中无效
            if (result is not { state: true }) return;

            IsSucceeded = true;

            _currentWindow.Close();
        }


    }
}
