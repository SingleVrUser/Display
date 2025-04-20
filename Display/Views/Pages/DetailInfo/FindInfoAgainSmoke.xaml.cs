using System.Collections.ObjectModel;
using DataAccess.Models.Entity;
using Display.Managers;
using Display.Models.Vo.Video;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Display.Views.Pages.DetailInfo;

public sealed partial class FindInfoAgainSmoke
{
    private readonly SpiderManager _spiderManager = App.GetService<SpiderManager>();

    private string CidName { get; }

    private readonly ObservableCollection<VideoSearchVo> _searchResultList = [];

    private readonly VideoSearchVo _videoSearchVo;

    public FindInfoAgainSmoke(string cidName)
    {
        InitializeComponent();

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
            infos.ForEach(i => _searchResultList.Add(new VideoSearchVo(i)));
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
        if (sender is not ListView listView)
            return;

        ConfirmButton.IsEnabled = listView.SelectedIndex != -1;
    }

    public event RoutedEventHandler ConfirmClick;
    //public event NotifyCollectionChangedEventHandler CollectionChanged;

    /// <summary>
    /// 点击确认键替换该番号信息
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
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
        var url = SpecificUrlTextBlock.Text;

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

            var newItem = _videoSearchVo.GetType().GetProperty(name);
            newItem?.SetValue(_videoSearchVo, value);
        }

        ConfirmSpecificUrlButton.IsEnabled = true;

    }

    private void ConfirmSpecificUrlButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button) return;
        if (_videoSearchVo == null) return;

        //修改一下
        button.DataContext = _videoSearchVo;

        ConfirmClick?.Invoke(button, e);
    }
}