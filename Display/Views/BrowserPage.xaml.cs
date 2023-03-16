using Data;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Display.Views
{
    public sealed partial class BrowserPage : Page
    {
        public BrowserPage(Window window)
        {
            this.InitializeComponent();
            //NavigationCacheMode = NavigationCacheMode.Enabled;

            Browser.webview.Source = new Uri("https://115.com/?cid=0&offset=0&mode=wangpan");

            Browser.webview.NavigationCompleted += Webview_NavigationCompleted; ;

            window.Closed += (sender, args) =>
            {
                Browser.webview.Close();
            };
        }

        private void Webview_NavigationCompleted(WebView2 sender, CoreWebView2NavigationCompletedEventArgs args)
        {
            //加载完成显示下载按钮
            DownButton.Visibility = Visibility.Visible;
        }

        private void DownButton_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            (sender as HyperlinkButton).Opacity = 1;
        }

        private void DownButton_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            (sender as HyperlinkButton).Opacity = 0.2;
        }

        WebApi webApi;
        private async void DownButton_Click(object sender, RoutedEventArgs e)
        {
            await DownFiles(Data.WebApi.downType.bc);

        }

        private async Task DownFiles(Data.WebApi.downType downtype)
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
                    datum.cid = item.id;
                    datum.n = item.name;
                    datum.pc = item.pick_code;
                    datum.fid = item.file_id;
                    videoinfos.Add(datum);
                }

                //BitComet只需要cid,n,pc三个值
                bool isSuccess = await webApi.RequestDown(videoinfos, downtype);

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

            await DownFiles(Data.WebApi.downType.aria2);
        }
    }

}
