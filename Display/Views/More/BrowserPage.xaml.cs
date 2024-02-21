using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Display.Models.Data;

namespace Display.Views.More
{
    public sealed partial class BrowserPage : Page
    {
        public BrowserPage(Window window, string url = "https://115.com/?cid=0&offset=0&mode=wangpan", bool isShowButton = false)
        {
            this.InitializeComponent();

            Browser.webview.Source = new Uri(url);

            window.Closed += (sender, args) =>
            {
                Browser.webview.Close();
            };

            //是否显示下载按钮
            if (isShowButton)
            {
                DownButton.Click += DownButton_Click;
                DownButton.PointerEntered += DownButton_PointerEntered;
                DownButton.PointerExited += DownButton_PointerExited;
                Aria2DownItem.Click += Aria2Down_Click;

                // 加载完成后显示
                Browser.webview.NavigationCompleted += Webview_NavigationCompleted;
            }
        }

        private void Webview_NavigationCompleted(WebView2 sender, CoreWebView2NavigationCompletedEventArgs args)
        {
            //加载完成显示下载按钮
            DownButton.Visibility = Visibility.Visible;
        }

        private void DownButton_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            ((HyperlinkButton)sender).Opacity = 1;
        }

        private void DownButton_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            ((HyperlinkButton)sender).Opacity = 0.2;
        }

        WebApi webApi;
        private async void DownButton_Click(object sender, RoutedEventArgs e)
        {
            await DownFiles(WebApi.DownType.Bc);

        }

        private async System.Threading.Tasks.Task DownFiles(WebApi.DownType downtype)
        {
            var selectedItemList = await Browser.GetSelectedItems();

            if (selectedItemList.Count == 0)
            {
                //SelectedNull_TeachingTip.IsOpen = true;
                ShowTeachingTip("当前未选中要下载的文件或文件夹");
                return;
            }
            else
            {
                webApi ??= WebApi.GlobalWebApi;

                List<Datum> videoinfos = new();

                foreach (var item in selectedItemList)
                {
                    Datum datum = new();
                    datum.Cid = item.id;
                    datum.Name = item.name;
                    datum.PickCode = item.pick_code;
                    datum.Fid = item.file_id;
                    videoinfos.Add(datum);
                }

                //BitComet只需要cid,n,pc三个值
                var isSuccess = await webApi.RequestDown(videoinfos, downtype);

                if (!isSuccess)
                    ShowTeachingTip("请求下载失败");
            }
        }

        private void ShowTeachingTip(string subtitle, string content = null)
        {
            LightDismissTeachingTip.Subtitle = subtitle;
            if (content != null)
                LightDismissTeachingTip.Content = content;

            LightDismissTeachingTip.IsOpen = true;
        }

        private async void Aria2Down_Click(object sender, RoutedEventArgs e)
        {

            await DownFiles(WebApi.DownType.Aria2);
        }

        private void GoBack_KeyboardAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            if (!Browser.webview.CanGoBack) return;

            Browser.webview.GoBack();
        }
    }

}
