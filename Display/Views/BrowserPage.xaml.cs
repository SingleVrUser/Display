using Data;
using Microsoft.UI.Text;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.WebUI;
using Newtonsoft.Json;

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
                if (webApi == null)
                    webApi = new();

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
