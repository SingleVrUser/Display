
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Display.Data;
using Display.Spider;

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


        async void FindInfos()
        {
            ReCheckProgressRing.Visibility = Visibility.Visible;

            var infos = await Manager.Current.DispatchSpiderInfosByCidInOrder(cidName);

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
            if (sender is not ListView listView)
                return;

            Confirm_Button.IsEnabled = listView.SelectedIndex != -1;
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
            button.DataContext = videoInfo;

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

            var info = await Manager.Current.DispatchSpiderInfoByDetailUrl(cidName, url, default);

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
