using Data;
using Display.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.Control
{
    public sealed partial class VideoDetails : UserControl
    {
        public VideoCoverDisplayClass resultinfo
        {
            get { return (VideoCoverDisplayClass)GetValue(resultinfoProperty); }
            set { SetValue(resultinfoProperty, value); }
        }

        public static readonly DependencyProperty resultinfoProperty =
            DependencyProperty.Register("resultinfo", typeof(string), typeof(VideoDetails), null);

        public VideoDetails()
        {
            this.InitializeComponent();

        }

        private void loadData()
        {
            if (resultinfo == null) return;

            //演员
            var actorList = resultinfo.actor.Split(',');
            for (var i = 0; i < actorList.Length; i++)
            {
                // 定义button
                var hyperButton = new HyperlinkButton();
                hyperButton.Content = actorList[i];
                hyperButton.Click += ActorButtonOnClick;

                // stackpanel内添加button
                ActorSatckPanel.Children.Insert(i, hyperButton);
            }

            //标签
            var categoryList = resultinfo.category.Split(",");
            for (var i = 0; i < categoryList.Length; i++)
            {
                // 定义button
                var hyperButton = new HyperlinkButton();
                hyperButton.Content = categoryList[i];
                hyperButton.Click += LabelButtonOnClick;

                // stackpanel内添加button
                CategorySatckPanel.Children.Insert(i, hyperButton);
            }
        }

        private void GridlLoaded(object sender, RoutedEventArgs e)
        {
            loadData();
        }

        // 点击了演员更多页
        public event RoutedEventHandler ActorClick;
        private void ActorButtonOnClick(object sender, RoutedEventArgs args)
        {
            ActorClick?.Invoke(sender, args);
        }

        // 点击了标签更多页
        public event RoutedEventHandler LabelClick;
        private void LabelButtonOnClick(object sender, RoutedEventArgs args)
        {
            LabelClick?.Invoke(sender, args);
        }

        //点击播放键
        public event RoutedEventHandler VideoPlayClick;
        private void VideoPlay_Click(object sender, RoutedEventArgs args)
        {
            VideoPlayClick?.Invoke(sender, args);
        }

        //点击了多集中的具体集数
        public event ItemClickEventHandler MultisetListClick;
        private void StationsList_OnItemClick(object sender, ItemClickEventArgs e)
        {
            MultisetListClick?.Invoke(sender, e);
        }

        private async void DownButton_Click(object sender, RoutedEventArgs e)
        {
            string name = resultinfo.truename;
            var videoinfoList = DataAccess.loadVideoInfoByTruename(name);

            ContentDialog dialog = new ContentDialog();

            // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
            dialog.XamlRoot = this.XamlRoot;
            dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
            dialog.Title = "下载";
            dialog.PrimaryButtonText = "下载全部";
            dialog.SecondaryButtonText = "下载选中项";
            dialog.CloseButtonText = "返回";
            dialog.DefaultButton = ContentDialogButton.Primary;

            videoinfoList = videoinfoList.OrderBy(item => item.n).ToList();

            var DownDialogContent = new ContentsPage.DownDialogContent(videoinfoList);

            dialog.Content = DownDialogContent;
            var result = await dialog.ShowAsync();

            List<Datum> downVideoInfoList = new List<Datum>();

            WebApi webapi = new();

            //下载全部
            if (result == ContentDialogResult.Primary)
            {
                webapi.RequestDown(videoinfoList);
            }

            //下载选中
            else if (result == ContentDialogResult.Secondary)
            {
                var stackPanel = DownDialogContent.Content as StackPanel;
                foreach (var item in stackPanel.Children)
                {
                    if (item is CheckBox)
                    {
                        CheckBox fileBox = item as CheckBox;
                        if (fileBox.IsChecked == true)
                        {
                            var selectVideoInfo = videoinfoList.Where(x => x.pc == fileBox.Name).First();
                            if (selectVideoInfo != null)
                            {
                                downVideoInfoList.Add(selectVideoInfo);
                            }
                        }
                    }
                }
                webapi.RequestDown(downVideoInfoList);
            }
            else
            {
                //DialogResult.Text = "User cancelled the dialog";
            }
        }

        private void updateLookLater(bool? val)
        {
            long lookLater_t = val == true ? DateTimeOffset.Now.ToUnixTimeSeconds() : 0;

            resultinfo.look_later = lookLater_t;
            DataAccess.UpdateSingleDataFromVideoInfo(resultinfo.truename, "look_later", lookLater_t.ToString());
        }

        private void updateLike(bool? val)
        {
            int is_like = val == true ? 1 : 0;

            resultinfo.is_like = is_like;
            DataAccess.UpdateSingleDataFromVideoInfo(resultinfo.truename, "is_like", is_like.ToString());
        }

    }
}
