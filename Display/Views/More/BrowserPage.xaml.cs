using Display.Providers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using Display.Models.Dto.OneOneFive;

namespace Display.Views.More;

public sealed partial class BrowserPage
{
    public BrowserPage(Window window, string url = "https://115.com/?cid=0&offset=0&mode=wangpan", bool isShowButton = false)
    {
        InitializeComponent();

        Browser.WebView.Source = new Uri(url);

        window.Closed += (_, _) => Browser.WebView.Close();

        //是否显示下载按钮
        if (!isShowButton) return;

        DownButton.Click += DownButton_Click;
        DownButton.PointerEntered += DownButton_PointerEntered;
        DownButton.PointerExited += DownButton_PointerExited;
        Aria2DownItem.Click += Aria2Down_Click;

        // 加载完成后显示
        Browser.WebView.NavigationCompleted += WebViewNavigationCompleted;
    }

    private void WebViewNavigationCompleted(WebView2 sender, CoreWebView2NavigationCompletedEventArgs args)
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

    private WebApi _webApi;
    private async void DownButton_Click(object sender, RoutedEventArgs e)
    {
        await DownFiles(WebApi.DownType.Bc);

    }

    private async System.Threading.Tasks.Task DownFiles(WebApi.DownType downtype)
    {
        var selectedItemList = await Browser.GetSelectedItems();

        if (selectedItemList.Count == 0)
        {
            ShowTeachingTip("当前未选中要下载的文件或文件夹");
        }
        else
        {
            _webApi ??= WebApi.GlobalWebApi;

            List<Datum> videoInfoList = [];
            videoInfoList.AddRange(selectedItemList.Select(item => new Datum() { Cid = item.id, Name = item.name, PickCode = item.pick_code, Fid = item.file_id }));

            //BitComet只需要cid,n,pc三个值
            var isSuccess = await _webApi.RequestDown(videoInfoList, downtype);

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
}