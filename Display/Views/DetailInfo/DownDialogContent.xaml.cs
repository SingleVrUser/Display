﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Display.Models.Data;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.Views.DetailInfo
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DownDialogContent : Page, INotifyPropertyChanged
    {
        //public List<Data.Datum> videoInfoList = new();

        public DownDialogContent()
        {
            this.InitializeComponent();
        }

        public DownDialogContent(List<Datum> videoInfoList)
        {
            this.InitializeComponent();

            CreateCheckBox(videoInfoList);
        }

        private void CreateCheckBox(List<Datum> videoInfoList)
        {

            foreach (var videoInfo in videoInfoList)
            {
                CheckBox NewCheckBox = new();
                NewCheckBox.Content = videoInfo.Name;
                NewCheckBox.Name = videoInfo.PickCode;
                ContentStackPanel.Children.Add(NewCheckBox);
            }
        }

        private string prompt_115 = "调用 115浏览器 下载，请确保已安装此应用";
        private string downUrl_115 = "https://pc.115.com/browser.html";
        private string prompt_aria2 = "调用 Aria2 下载，请确保已安装此应用并完成相关设置";
        private string downUrl_aria2 = "https://aria2.github.io/";
        private string prompt_bitcomet = "调用 比特彗星 下载，请确保已安装此应用并完成相关设置";
        private string downUrl_bitcomet = "https://www.bitcomet.com/en/downloads";

        string downMethod = AppSettings.DefaultDownMethod;
        public string DownMethod
        {
            get => downMethod;
            set
            {
                downMethod = value;
                AppSettings.DefaultDownMethod = downMethod;
                OnPropertyChanged();


            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Select_115(object sender, RoutedEventArgs e)
        {
            DownMethod = "115";
            ContentTextBlock.Text = prompt_115;
            DownHyperLinkButton.NavigateUri = new Uri(downUrl_115);
        }

        private void Select_bitcomet(object sender, RoutedEventArgs e)
        {
            DownMethod = "比特彗星";
            ContentTextBlock.Text = prompt_bitcomet;
            DownHyperLinkButton.NavigateUri = new Uri(downUrl_bitcomet);
        }

        private void Select_aria2(object sender, RoutedEventArgs e)
        {
            DownMethod = "aria2";
            ContentTextBlock.Text = prompt_aria2;
            DownHyperLinkButton.NavigateUri = new Uri(downUrl_aria2);
        }


        private void ContentStackPanel_Loaded(object sender, RoutedEventArgs e)
        {
            ContentTextBlock.Text = prompt_115;
            DownHyperLinkButton.NavigateUri = new Uri(downUrl_115);
        }

        private void downComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string colorName = e.AddedItems[0].ToString();
        }

    }
}
