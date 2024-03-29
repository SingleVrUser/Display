using Display.Managers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using Display.Models.Dto.OneOneFive;

namespace Display.Views.DetailInfo
{
    public sealed partial class FindInfoAgainSmoke
    {
        private readonly SpiderManager _spiderManager = App.GetService<SpiderManager>();

        private string CidName { get; }

        private readonly ObservableCollection<VideoInfo> _searchResultList = [];

        private readonly VideoInfo _videoInfo;

        public FindInfoAgainSmoke(string cidName)
        {
            InitializeComponent();

            _videoInfo = new VideoInfo();

            CidName = cidName;

            InitView();
        }

        private void InitView()
        {
            SpiderCheckBoxGrid.ItemsSource = SpiderManager.Spiders;
        }


        async void FindInfos()
        {
            ReCheckProgressRing.Visibility = Visibility.Visible;

            var infos = await _spiderManager.DispatchSpiderInfosByCidInOrder(CidName);

            if (infos.Count > 0)
            {
                _searchResultList.Clear();
                infos.ForEach(_searchResultList.Add);
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
            if (NewInfoListView.SelectedItem is not VideoInfo videoInfo) return;
            if (sender is not Button button) return;

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
            var url = SpecificUrl_TextBlock.Text;

            ReCheckProgressRing.Visibility = Visibility.Visible;
            ConfirmSpecificUrlButton.IsEnabled = false;

            var info = await _spiderManager.DispatchSpiderInfoByDetailUrl(CidName, url, default);

            ReCheckProgressRing.Visibility = Visibility.Collapsed;

            if (info == null) return;

            //更新ResultInfo数据
            foreach (var item in info.GetType().GetProperties())
            {
                var name = item.Name;
                var value = item.GetValue(info);

                var newItem = _videoInfo.GetType().GetProperty(name);
                newItem?.SetValue(_videoInfo, value);
            }

            ConfirmSpecificUrlButton.IsEnabled = true;

        }

        private void ConfirmSpecificUrlButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button) return;
            if (_videoInfo == null) return;

            //修改一下
            button.DataContext = _videoInfo;

            ConfirmClick?.Invoke(button, e);
        }
    }
}
