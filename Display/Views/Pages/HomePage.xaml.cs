using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DataAccess.Dao.Interface;
using DataAccess.Models.Entity;
using Display.Models.Vo;
using Display.Models.Vo.IncrementalCollection;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;


namespace Display.Views.Pages;

public sealed partial class HomePage
{
    private readonly IncrementalLoadSuccessInfoCollection _recentList = [];

    private readonly ObservableCollection<VideoInfoVo> _lookLaterList = [];
    private readonly ObservableCollection<VideoInfoVo> _recentCoverList = [];
    private readonly ObservableCollection<VideoInfoVo> _loveCoverList = [];

    private readonly IVideoInfoDao _videoInfoDao = App.GetService<IVideoInfoDao>();

    //过渡动画用
    private enum NavigationAnimationType { Image, GridView };
    private NavigationAnimationType _navigationType;
    private VideoInfoVo _storedItem;
    private GridView _storedGridView;
    private Image _storedImage;

    public HomePage()
    {
        InitializeComponent();

        //启动缓存
        NavigationCacheMode = NavigationCacheMode.Required;

        LoadRandomList();
    }

    private async void LoadRandomList()
    {
        _recentList.SetOrder("random", true);
        await _recentList.LoadData();
    }

    //private void LoadCover()
    //{
    //    // 随机获取20个视频，每次启动自动获取一遍
    //    var imageList = await _videoInfoDao.GetRandomListAsync(10);
    //    if (imageList.Length == 0) return;

    //    foreach (var info in imageList.Select(item => new VideoInfoVo(item, 500, 300)))
    //    {
    //        _recentList.Add(info);
    //    }

    //    //var binding = new Binding { Path = new PropertyPath("SelectedIndex"), Mode = BindingMode.TwoWay };
    //    //ImagePipsPager.SetBinding(PipsPager.SelectedPageIndexProperty, binding);
    //}

    private void MultipleCoverShow_ItemClick(object sender, ItemClickEventArgs e)
    {
        var coverInfo = (VideoInfoVo)e.ClickedItem;

        _storedItem = coverInfo;
        _storedGridView = (GridView)sender;
        _navigationType = NavigationAnimationType.GridView;

        //准备动画
        _storedGridView.PrepareConnectedAnimation("ForwardConnectedAnimation", _storedItem, "showImage");

        Frame.Navigate(typeof(DetailInfoPage), coverInfo, new SuppressNavigationTransitionInfo());
    }

    private void Image_Tapped(object sender, TappedRoutedEventArgs e)
    {
        if (sender is not Image image) return;

        _storedImage = image;

        var coverInfo = _storedImage.DataContext as VideoInfoVo;

        _navigationType = NavigationAnimationType.Image;

        var animation = ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("ForwardConnectedAnimation", _storedImage);
        animation.Configuration = new BasicConnectedAnimationConfiguration();
        Frame.Navigate(typeof(DetailInfoPage), coverInfo, new SuppressNavigationTransitionInfo());
    }

    private void Image_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Hand);

    }

    private void Image_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Arrow);
    }

    private void videoInfoListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is not ListView listView) return;

        if (VideoInfoListView.ItemsSource is not ObservableCollection<VideoInfoVo> items) return;

        if (listView.SelectedItem is not VideoInfo videoInfo) return;

        if (items.Contains(videoInfo))
        {
            VideoInfoListView.ScrollIntoView(videoInfo);
        }
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        // 过渡动画
        var animation = ConnectedAnimationService.GetForCurrentView().GetAnimation("BackConnectedAnimation");
        if (animation != null)
        {
            if (_navigationType == NavigationAnimationType.Image)
            {
                if (_storedImage != null)
                {
                    animation.TryStart(_storedImage);
                }
            }
            else if (_navigationType == NavigationAnimationType.GridView)
            {
                //开始动画
                if (_storedItem != null)
                {
                    //开始动画
                    await _storedGridView.TryStartConnectedAnimationAsync(animation, _storedItem, "showImage");
                }
            }
        }

        //对上一级页面的更改做出响应，删除或添加喜欢
        TryUpdateCoverShow();
    }

    private void TryUpdateVideoCoverDisplayClass(VideoInfo[] videoInfos, ObservableCollection<VideoInfoVo> videoList)
    {
        if (videoInfos == null) return;

        //添加
        var addList = new List<VideoInfo>();
        foreach (var item in videoInfos)
        {
            var isAdd = true;
            foreach (var showItem in videoList)
            {
                if (showItem.TrueName == item.TrueName)
                {
                    isAdd = false;
                }
            }

            if (isAdd)
            {
                addList.Add(item);
            }
        }

        //删除
        var delList = new List<string>();
        foreach (var showItem in videoList)
        {
            var isDel = true;
            foreach (var item in videoInfos)
            {
                if (showItem.TrueName == item.TrueName)
                {
                    isDel = false;
                }
            }

            if (isDel)
            {
                delList.Add(showItem.TrueName);
            }
        }

        foreach (var trueName in delList)
        {
            var delItem = videoList.FirstOrDefault(x => x.TrueName == trueName);
            videoList.Remove(delItem);
        }
        foreach (var item in addList)
        {
            videoList.Add(new VideoInfoVo(item));
        }
    }

    private void MoreLikeVideoClick(object sender, RoutedEventArgs e)
    {
        Tuple<List<string>, string, bool> typesAndName = new(["is_like"], "1", false);

        Frame.Navigate(typeof(VideoCoverPage), typesAndName);
    }

    private void MoreLookLaterVideoClick(object sender, RoutedEventArgs e)
    {
        Tuple<List<string>, string, bool> typesAndName = new(["look_later"], "1", false);

        Frame.Navigate(typeof(VideoCoverPage), typesAndName);
    }

    private async void RefreshNewestVideoButtonClick(object sender, RoutedEventArgs e)
    {
        _recentCoverList.Clear();

        foreach (var videoInfo in await _videoInfoDao.GetRecentListAsync(10))
        {
            _recentCoverList.Add(new VideoInfoVo(videoInfo));
        }

    }

    private async void RefreshLookLaterVideoButtonClick(object sender, RoutedEventArgs e)
    {
        _lookLaterList.Clear();

        foreach (var videoInfo in await _videoInfoDao.GetLookLaterListAsync(10))
        {
            _lookLaterList.Add(new VideoInfoVo(videoInfo));
        }
    }

    private async void RefreshLikeVideoButtonClick(object sender, RoutedEventArgs e)
    {
        _loveCoverList.Clear();

        foreach (var videoInfo in await _videoInfoDao.GetLikeListAsync(10))
        {
            _loveCoverList.Add(new VideoInfoVo(videoInfo));
        }
    }

    private async void TryUpdateCoverShow()
    {
        //最近视频
        TryUpdateVideoCoverDisplayClass(await _videoInfoDao.GetRecentListAsync(10), _recentCoverList);
        //稍后观看
        TryUpdateVideoCoverDisplayClass(await _videoInfoDao.GetLookLaterListAsync(10), _lookLaterList);
        //喜欢视频
        TryUpdateVideoCoverDisplayClass(await _videoInfoDao.GetLikeListAsync(10), _loveCoverList);
    }
}