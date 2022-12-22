using Data;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Display.ContentsPage.DetailInfo
{
    public sealed partial class FindInfoAgainSmoke : Page
    {
        private string cidName { get; set; }

        public ObservableCollection<VideoInfo> VideoInfos = new();

        public FindInfoAgainSmoke(string cidName)
        {
            this.InitializeComponent();

            this.cidName = cidName;

            this.Loaded += PageLoaded;
        }

        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            FindInfos();

            this.Loaded -= PageLoaded;
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
            List<VideoInfo> infos = new();

            ReCheckProgressRing.Visibility = Visibility.Visible;

            bool isFc2 = FileMatch.IsFC2(cidName);

            if (!isFc2 && JavBus_CheckBox.IsChecked == true)
            {
                var info = await SearchNetwork.SearchInfoFromJavBus(cidName);
                if(info!=null)
                    infos.Add(info);
            }
            if (!isFc2 && Jav321_CheckBox.IsChecked == true)
            {
                var info = await SearchNetwork.SearchInfoFromJav321(cidName);
                if(info!=null)
                    infos.Add(info);
            }
            if (!isFc2 && AvMoo_CheckBox.IsChecked == true)
            {
                var info = await SearchNetwork.SearchInfoFromAvMoo(cidName);
                if(info!=null)
                    infos.Add(info);
            }
            if (AvSox_CheckBox.IsChecked == true)
            {
                var info = await SearchNetwork.SearchInfoFromAvSox(cidName);
                if(info!=null)
                    infos.Add(info);
            }
            if (!isFc2 && LibreDmm_CheckBox.IsChecked == true)
            {
                var info = await SearchNetwork.SearchInfoFromLibreDmm(cidName);
                if(info!=null)
                    infos.Add(info);
            }
            if (isFc2 && Fc2Hub_CheckBox.IsChecked == true)
            {
                var info = await SearchNetwork.SearchInfoFromFc2Hub(cidName);
                if (info != null)
                    infos.Add(info);
            }
            if (JavDB_CheckBox.IsChecked == true)
            {
                var info = await SearchNetwork.SearchInfoFromJavDB(cidName);
                if (info != null)
                    infos.Add(info);
            }

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
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// 点击确认键替换该番号信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Confirm_Button_Click(object sender, RoutedEventArgs e)
        {
            if (!(NewInfo_ListView.SelectedItem is VideoInfo videoInfo)) return;
            if (!(sender is Button button)) return;

            //修改一下
            button.DataContext= videoInfo;

            ConfirmClick?.Invoke(button, e);
        }
    }
}
