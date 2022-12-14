using Data;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.Control
{
    public sealed partial class Browser : UserControl
    {
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        public Browser()
        {
            this.InitializeComponent();

            webview.Source = new Uri("https://115.com/?cid=0&offset=0&mode=wangpan");

            webview.CoreWebView2Initialized += Webview_CoreWebView2Initialized;

        }

        private void Webview_CoreWebView2Initialized(WebView2 sender, CoreWebView2InitializedEventArgs args)
        {
            //webview.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;
            //var item2 = webview.ExecuteScriptAsync($"window.open('https://baidu.com','_blank');");

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

        private void AddCookie(string key, string value)
        {
            var cookie = webview.CoreWebView2.CookieManager.CreateCookie(key, value, ".115.com", "/");
            webview.CoreWebView2.CookieManager.AddOrUpdateCookie(cookie);
        }

        private void webview_NavigationStarting(WebView2 sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs args)
        {
            NavigationProgressBar.Visibility = Visibility.Visible;
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


        private void webview_NavigationCompleted(WebView2 sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs args)
        {
            NavigationProgressBar.Visibility = Visibility.Collapsed;

            HiddenWaterMark();

            HiddenAdvertising();
        }

        /// <summary>
        /// file_count,folder_count,hasHiddenFile,size可能获取不到（选中数超过两个），所以只使用name和id
        /// </summary>
        /// <returns></returns>
        public async Task<List<SelectedItem>> GetSelectedItems()
        {
            if (webview == null)
            {
                return null;
            }

            List<SelectedItem> selectedItemList = new();

            //选择文件夹和文件
            string inputElementsIdAndValueAsJsonString = await webview.ExecuteScriptAsync(
                "Array.from(" +
                        "document.getElementById('js_center_main_box').getElementsByTagName('iframe')[0].contentDocument.getElementsByClassName('list-contents')[0].getElementsByTagName('li')" +
                    ").filter(" +
                        "li => li.getAttribute('class') == 'selected'" +
                    ").map(" +
                        "li_selected => {  " +
                            "return {'id': li_selected.getAttribute('cate_id'), " +
                                "'name': li_selected.getAttribute('title') || li_selected.getAttribute('cate_id'), " +
                                "'file_count': li_selected.getAttribute('category_file_count'), " +
                                "'folder_count': li_selected.getAttribute('cate_folder_count')," +
                                "'hasHiddenFile': li_selected.getAttribute('hdf')," +
                                " 'size': li_selected.getAttribute('cate_size')," +
                                " 'file_type': li_selected.getAttribute('file_type')," +
                                " 'file_id': li_selected.getAttribute('file_id')," +
                                " 'pick_code': li_selected.getAttribute('pick_code')}  });");

            inputElementsIdAndValueAsJsonString = inputElementsIdAndValueAsJsonString.Replace("null", "0");

            if (inputElementsIdAndValueAsJsonString != "[]" && inputElementsIdAndValueAsJsonString != "0")
            {
                selectedItemList = JsonConvert.DeserializeObject<List<SelectedItem>>(inputElementsIdAndValueAsJsonString);
            }

            return selectedItemList;
        }

    }
}
