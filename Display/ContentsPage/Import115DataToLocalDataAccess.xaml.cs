using Data;
using Microsoft.UI;
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

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.ContentsPage
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Import115DataToLocalDataAccess : Page
    {
        private WebView2 webview;
        //private string _stroeUrl;

        public Import115DataToLocalDataAccess()
        {
            this.InitializeComponent();

            ////开启缓存
            //NavigationCacheMode = NavigationCacheMode.Enabled;

            webview = Browser.webview;

            string url;
            if(Data.StaticData.ImportDataAccess_NavigationUrl != null)
            {
                url = Data.StaticData.ImportDataAccess_NavigationUrl;
            }
            else
            {
                url = "https://115.com/?cid=0&offset=0&mode=wangpan";
            }
            webview.Source = new Uri(url);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            //if (!string.IsNullOrEmpty(_stroeUrl))
            //{

            //}

            Window window = e.Parameter as Window;
            window.Closed += (sender, args) =>
            {
                if(webview.CoreWebView2 != null)
                {
                    webview.CoreWebView2.Stop();
                }
                webview.Close();
            };
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);

            //储存跳转前的地址
            Data.StaticData.ImportDataAccess_NavigationUrl = webview.Source.AbsoluteUri;

            //跳转前先关闭WebView，不然会出错
            webview.Close();

        }

        /// <summary>
        /// 进入隐藏系统后，即使有隐藏文件也能全部显示，弃用
        /// </summary>
        /// <returns></returns>
        private async Task<bool> IsAllShow()
        {
            bool isHiddenModel = false;
            string inputElementsIdAndValueAsJsonString = await webview.ExecuteScriptAsync(
            "(()=>{const length = document.getElementById('js_center_main_box').getElementsByTagName('iframe')[0].contentDocument.getElementsByClassName('visible-model').length ; return length})();");

            if (inputElementsIdAndValueAsJsonString == "2")
            {
                isHiddenModel = true;
            }

            return isHiddenModel;
        }

        /// <summary>
        /// file_count,folder_count,hasHiddenFile,size可能获取不到（选中数超过两个），所以只使用name和id
        /// </summary>
        /// <returns></returns>
        private async Task<List<SelectedItem>> GetSelectedItems()
        {
            if(webview == null)
            {
                return null;
            }

            List<SelectedItem> selectedItemList = new();

            string inputElementsIdAndValueAsJsonString = await webview.ExecuteScriptAsync(
                "Array.from(" +
                        "document.getElementById('js_center_main_box').getElementsByTagName('iframe')[0].contentDocument.getElementsByClassName('list-contents')[0].getElementsByTagName('li')" +
                    ").filter(" +
                        "li => li.getAttribute('class') == 'selected' & li.getAttribute('file_type') == 0" +
                    ").map(" +
                        "li_selected => {  " +
                            "return {'id': li_selected.getAttribute('cate_id'), " +
                                "'name': li_selected.getAttribute('title') || li_selected.getAttribute('cate_id'), " +
                                "'file_count': li_selected.getAttribute('category_file_count'), " +
                                "'folder_count': li_selected.getAttribute('cate_folder_count')," +
                                "'hasHiddenFile': li_selected.getAttribute('hdf')," +
                                " 'size': li_selected.getAttribute('cate_size')}  });");

            inputElementsIdAndValueAsJsonString = inputElementsIdAndValueAsJsonString.Replace("null", "0");

            if (inputElementsIdAndValueAsJsonString != "[]" && inputElementsIdAndValueAsJsonString != "0")
            {
                selectedItemList = JsonConvert.DeserializeObject<List<SelectedItem>>(inputElementsIdAndValueAsJsonString);
            }


            return selectedItemList;
        }


        /// <summary>
        /// 点击了开始按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {

            //var item2 = webview.ExecuteScriptAsync($"alert('this is not safe, try an https link')");

            ////WebView可以通过chrome.webview.postMessage 发送消息至应用，应用通过WebMessageReceived接收
            //var userId = await webview.ExecuteScriptAsync(@"chrome.webview.postMessage(""message"")");


            ////直接使用performance.memory不返回结果，将值显式复制到另一个对象中才能返回
            //var result2 = await webview.ExecuteScriptAsync(
            //    "(() => { const {totalJSHeapSize, usedJSHeapSize} = performance.memory; return {totalJSHeapSize, usedJSHeapSize}; })();"

            //    );
            List<SelectedItem> selectedItemList = await GetSelectedItems();

            if (selectedItemList.Count == 0)
            {
                SelectedNull_TeachingTip.IsOpen = true;
                return;
            }

            var receiveResult = await ShowContentDialog(selectedItemList);


            if (receiveResult == ContentDialogResult.Primary)
            {
                var cidList = new List<string>();
                foreach(var item in selectedItemList)
                {
                    cidList.Add(item.id);
                }

                Frame.Navigate(typeof(ProgressOfImportDataAccess),cidList);

            }
        }

        /// <summary>
        /// 显示确认提示框
        /// </summary>
        /// <param name="selectedItemList"></param>
        /// <returns></returns>
        private async Task<ContentDialogResult> ShowContentDialog(List<SelectedItem> selectedItemList)
        {
            StackPanel readyStackPanel = new StackPanel();

            //readyStackPanel.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
            readyStackPanel.Children.Add(new TextBlock() { Text = "选中文件夹：" });
            int index = 0;

            foreach (var selectedItem in selectedItemList)
            {
                index++;
                TextBlock textBlock = new TextBlock() { Text= $"  {index}.{selectedItem.name}" ,
                    IsTextSelectionEnabled = true,
                    Margin = new Thickness(0,2,0,0)
                };

                readyStackPanel.Children.Add(textBlock);
            }

            readyStackPanel.Children.Add(
                new TextBlock()
                {
                    Text = "未进入隐藏系统的情况下，隐藏的文件将被跳过，请知晓",
                    TextWrapping = TextWrapping.Wrap,
                    Foreground = new SolidColorBrush(Colors.LightGray),
                    MaxWidth = 300,
                    Margin = new Thickness(0, 8, 0, 0)
                });

            ContentDialog dialog = new ContentDialog();

            // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
            dialog.XamlRoot = this.XamlRoot;
            //dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
            dialog.Title = "确认后继续";
            dialog.PrimaryButtonText = "继续";
            dialog.CloseButtonText = "取消";
            dialog.DefaultButton = ContentDialogButton.Primary;
            dialog.Content = readyStackPanel;

            var result = await dialog.ShowAsync();

            return result;
        }

    }
}
