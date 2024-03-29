using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Display.Models.Dto.OneOneFive;
using Display.Providers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Display.Views.Pages.DetailInfo;

public sealed partial class DownDialogContent : INotifyPropertyChanged
{
    public DownDialogContent()
    {
        InitializeComponent();
    }

    public DownDialogContent(List<Datum> videoInfoList)
    {
        InitializeComponent();

        CreateCheckBox(videoInfoList);
    }

    private void CreateCheckBox(List<Datum> videoInfoList)
    {
        videoInfoList.ForEach(videoInfo => ContentStackPanel.Children.Add(new CheckBox
        {
            Content = videoInfo.Name,
            Name = videoInfo.PickCode
        }));
    }

    private const string Prompt115 = "调用 115浏览器 下载，请确保已安装此应用";
    private const string DownUrl115 = "https://pc.115.com/browser.html";
    private const string PromptAria2 = "调用 Aria2 下载，请确保已安装此应用并完成相关设置";
    private const string DownUrlAria2 = "https://aria2.github.io/";
    private const string PromptBitcomet = "调用 比特彗星 下载，请确保已安装此应用并完成相关设置";
    private const string DownUrlBitcomet = "https://www.bitcomet.com/en/downloads";

    private string _downMethod = AppSettings.DefaultDownMethod;
    public string DownMethod
    {
        get => _downMethod;
        private set
        {
            _downMethod = value;
            AppSettings.DefaultDownMethod = _downMethod;
            OnPropertyChanged();


        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void Select_115(object sender, RoutedEventArgs e)
    {
        DownMethod = "115";
        ContentTextBlock.Text = Prompt115;
        DownHyperLinkButton.NavigateUri = new Uri(DownUrl115);
    }

    private void Select_bitcomet(object sender, RoutedEventArgs e)
    {
        DownMethod = "比特彗星";
        ContentTextBlock.Text = PromptBitcomet;
        DownHyperLinkButton.NavigateUri = new Uri(DownUrlBitcomet);
    }

    private void Select_aria2(object sender, RoutedEventArgs e)
    {
        DownMethod = "aria2";
        ContentTextBlock.Text = PromptAria2;
        DownHyperLinkButton.NavigateUri = new Uri(DownUrlAria2);
    }


    private void ContentStackPanel_Loaded(object sender, RoutedEventArgs e)
    {
        ContentTextBlock.Text = Prompt115;
        DownHyperLinkButton.NavigateUri = new Uri(DownUrl115);
    }


}