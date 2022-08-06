using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.ContentsPage
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DownDialogContent : Page
    {
        //public List<Data.Datum> videoInfoList = new();

        public DownDialogContent()
        {
            this.InitializeComponent();
        }

        public DownDialogContent(List<Data.Datum> videoInfoList)
        {
            this.InitializeComponent();

            CreateCheckBox(videoInfoList);
        }

        private void CreateCheckBox(List<Data.Datum> videoInfoList)
        {

            foreach (var videoInfo in videoInfoList)
            {
                CheckBox NewCheckBox = new();
                NewCheckBox.Content = videoInfo.n;
                NewCheckBox.Name = videoInfo.pc;
                ContentStackPanel.Children.Add(NewCheckBox);
            }
        }

        private string prompt_115 = "调用 115浏览器 下载，请确保已安装此应用";
        private string downUrl_115 = "https://pc.115.com/browser.html";
        private string prompt_aria2 = "调用 Aria2 下载，请确保已安装此应用并完成相关设置";
        private string downUrl_aria2 = "https://aria2.github.io/";

        private void Select_115(object sender, RoutedEventArgs e)
        {
            DownMethodSelect_DropDownButton.Content = "115";
            ContentTextBlock.Text = prompt_115;
            DownHyperLinkButton.NavigateUri = new Uri(downUrl_115);
        }

        private void Select_aria2(object sender, RoutedEventArgs e)
        {
            DownMethodSelect_DropDownButton.Content = "aria2";
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
