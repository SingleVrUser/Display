using Data;
using Data.Helper;
using Data.Spider;
using HtmlAgilityPack;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.Media.Protection.PlayReady;

namespace Display.ContentsPage.DetailInfo
{
    public sealed partial class FindInfoAgainSmoke : Page
    {
        private string cidName { get; set; }

        private ObservableCollection<VideoInfo> VideoInfos = new();

        private VideoInfo VideoInfo;

        public FindInfoAgainSmoke(string cidName)
        {
            this.InitializeComponent();

            VideoInfo = new();

            this.cidName = cidName;
        }

        private GetInfoFromNetwork _searchNetwork;

        private GetInfoFromNetwork SearchNetwork
        {
            get
            {
                if (_searchNetwork == null)
                    _searchNetwork = new GetInfoFromNetwork();
                return _searchNetwork;
            }
            set => _searchNetwork = value;
        }

        async void FindInfos()
        {
            ReCheckProgressRing.Visibility = Visibility.Visible;

            List<VideoInfo> infos = await Manager.Current.DispatchSpiderInfosByCIDInOrder(cidName);

            if (infos.Count > 0)
            {
                VideoInfos.Clear();
                infos.ForEach(info => VideoInfos.Add(info));
            }

            ReCheckProgressRing.Visibility = Visibility.Collapsed;

        }

        private void RefreshButtonClicked(object sender, RoutedEventArgs e)
        {
            FindInfos();
        }

        /// <summary>
        /// 只有选中了才能按确认键
        /// </summary>
        /// <param Name="sender"></param>
        /// <param Name="e"></param>
        private void NewInfo_ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(sender is ListView listView))
                return;

            if(listView.SelectedIndex== -1)
                Confirm_Button.IsEnabled = false;
            else
            {
                Confirm_Button.IsEnabled = true;
            }
        }

        public event RoutedEventHandler ConfirmClick;
        //public event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>
        /// 点击确认键替换该番号信息
        /// </summary>
        /// <param Name="sender"></param>
        /// <param Name="e"></param>
        private void Confirm_Button_Click(object sender, RoutedEventArgs e)
        {
            if (!(NewInfo_ListView.SelectedItem is VideoInfo videoInfo)) return;
            if (!(sender is Button button)) return;

            //修改一下
            button.DataContext= videoInfo;

            ConfirmClick?.Invoke(button, e);
        }

        private void SpecificSearchSourceGrid_loaded(object sender, RoutedEventArgs e)
        {
            FindInfos();

            SpecificSearchSourceGrid.Loaded -= SpecificSearchSourceGrid_loaded;
        }

        private async void SearchInfoBySpecificUrlButton_Click(object sender, RoutedEventArgs e)
        {
            string url = SepcificUrl_TextBlock.Text;

            ReCheckProgressRing.Visibility = Visibility.Visible;
            ConfirmSpecificUrlButton.IsEnabled = false;

            var info =  await Data.Spider.Manager.Current.DispatchSpiderInfoByDetailUrl(cidName, url);

            ReCheckProgressRing.Visibility = Visibility.Collapsed;

            if (info == null) return;

            //更新ResultInfo数据
            foreach (var item in info.GetType().GetProperties())
            {
                var name = item.Name;
                var value = item.GetValue(info);

                var newItem = VideoInfo.GetType().GetProperty(name);
                newItem.SetValue(VideoInfo, value);
            }

            ConfirmSpecificUrlButton.IsEnabled = true;

        }

        private void ConfirmSpecificUrlButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button button)) return;
            if (VideoInfo == null) return;

            //修改一下
            button.DataContext = VideoInfo;

            ConfirmClick?.Invoke(button, e);
        }
    }
}
