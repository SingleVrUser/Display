using System;
using Microsoft.UI.Xaml.Navigation;

namespace Display.Views.Pages.More;

public sealed partial class VideoPlay
{
    public VideoPlay()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        string url;

        if (e.Parameter is string pickCode)
            url = $"https://v.anxia.com/?pickcode={pickCode}&share_id=0";
        else
            url = "https://115.com/?cid=0&offset=0&mode=wangpan";

        Browser.WebView.Source = new Uri(url);
    }
}