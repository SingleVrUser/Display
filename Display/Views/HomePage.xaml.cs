using Display.Data;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HomePage : Page
    {
        public ObservableCollection<VideoCoverDisplayClass> Items = new();
        public ObservableCollection<VideoCoverDisplayClass> NewAddItems = new();
        private ObservableCollection<VideoCoverDisplayClass> lookLaterList = new();
        private ObservableCollection<VideoCoverDisplayClass> recentCoverList = new();
        private ObservableCollection<VideoCoverDisplayClass> LoveCoverList = new();

        //过渡动画用
        private enum navigationAnimationType { image, gridView };
        private navigationAnimationType _navigationType;
        private VideoCoverDisplayClass _storeditem;
        private GridView _stroedgridview;
        private Image _storedimage;

        public HomePage()
        {
            this.InitializeComponent();

            //启动缓存
            NavigationCacheMode = NavigationCacheMode.Required;

            LoadCover();

        }

        //private void HomePage_Loaded(object sender, RoutedEventArgs e)
        //{

        //    this.Loaded -= HomePage_Loaded;
        //}

        private async void LoadCover()
        {
            //随机获取20个视频，每次启动自动获取一遍
            var imageList = await DataAccess.Get.GetNameAndImageRandom();
            if (imageList == null) return;

            foreach (var info in imageList.Select(item => new VideoCoverDisplayClass(item, 500, 300)))
            {
                Items.Add(info);
            }

            //ImagePipsPager.DataContext = randomIamgeFlipView;
            Binding binding = new Binding() { Path = new PropertyPath("SelectedIndex"), Mode = BindingMode.TwoWay };
            ImagePipsPager.SetBinding(PipsPager.SelectedPageIndexProperty, binding);
        }

        private void MultipleCoverShow_ItemClick(object sender, ItemClickEventArgs e)
        {
            VideoCoverDisplayClass coverInfo = (VideoCoverDisplayClass)e.ClickedItem;

            _storeditem = coverInfo;
            _stroedgridview = (GridView)sender;
            _navigationType = navigationAnimationType.gridView;

            //准备动画
            _stroedgridview.PrepareConnectedAnimation("ForwardConnectedAnimation", _storeditem, "showImage");

            Frame.Navigate(typeof(DetailInfoPage), coverInfo, new SuppressNavigationTransitionInfo());
        }

        private void Image_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (!(sender is Image image)) return;

            _storedimage = image;

            VideoCoverDisplayClass coverInfo = _storedimage.DataContext as VideoCoverDisplayClass;

            _navigationType = navigationAnimationType.image;

            var animation = ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("ForwardConnectedAnimation", _storedimage);
            animation.Configuration = new BasicConnectedAnimationConfiguration();
            Frame.Navigate(typeof(DetailInfoPage), coverInfo, new SuppressNavigationTransitionInfo());
        }

        private void Image_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            //Grid grid = sender as Grid;
            //grid.BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.LightGray);
            ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Hand);

        }

        private void Image_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            //Grid grid = sender as Grid;
            //grid.BorderBrush = null;
            ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Arrow);
        }

        private void videoInfoListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is not ListView listView) return;

            if (videoInfoListView.ItemsSource is not ObservableCollection<VideoCoverDisplayClass> items) return;

            if (listView.SelectedItem is not VideoInfo videoInfo) return;

            if (items.Contains(videoInfo))
            {
                videoInfoListView.ScrollIntoView(videoInfo);
            }
        }

        private async void UpdateRandomCover_Click(object sender, RoutedEventArgs e)
        {
            RefreshHyperlinkButton.IsEnabled = false;

            ImagePipsPager.ClearValue(PipsPager.SelectedPageIndexProperty);
            videoInfoListView.SelectionChanged -= videoInfoListView_SelectionChanged;

            Items.Clear();

            //随机获取20个视频
            foreach (var item in await DataAccess.Get.GetNameAndImageRandom())
            {
                Items.Add(new VideoCoverDisplayClass(item, 500, 300));
            }

            videoInfoListView.SelectionChanged += videoInfoListView_SelectionChanged;

            Binding binding = new Binding() { Path = new PropertyPath("SelectedIndex"), Mode = BindingMode.TwoWay };
            ImagePipsPager.SetBinding(PipsPager.SelectedPageIndexProperty, binding);



            RefreshHyperlinkButton.IsEnabled = true;

        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // 过渡动画
            ConnectedAnimation animation = ConnectedAnimationService.GetForCurrentView().GetAnimation("BackConnectedAnimation");
            if (animation != null)
            {
                if (_navigationType == navigationAnimationType.image)
                {
                    if (_storedimage != null)
                    {
                        animation.TryStart(_storedimage);
                    }
                }
                else if (_navigationType == navigationAnimationType.gridView)
                {
                    //开始动画
                    if (_storeditem != null)
                    {
                        //开始动画
                        await _stroedgridview.TryStartConnectedAnimationAsync(animation, _storeditem, "showImage");
                    }
                }
            }

            //对上一级页面的更改做出响应，删除或添加喜欢
            TryUpdateCoverShow();
        }

        private void TryUpdateVideoCoverDisplayClass(VideoInfo[] videoInfos, ObservableCollection<VideoCoverDisplayClass> videoList)
        {
            if (videoInfos == null) return;

            //添加
            var addList = new List<VideoInfo>();
            foreach (var item in videoInfos)
            {
                bool isAdd = true;
                foreach (var showItem in videoList)
                {
                    if (showItem.trueName == item.trueName)
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
                bool isDel = true;
                foreach (var item in videoInfos)
                {
                    if (showItem.trueName == item.trueName)
                    {
                        isDel = false;
                    }
                }

                if (isDel)
                {
                    delList.Add(showItem.trueName);
                }
            }

            foreach (var trueName in delList)
            {
                var delItem = videoList.FirstOrDefault(x => x.trueName == trueName);
                videoList.Remove(delItem);
            }
            foreach (var item in addList)
            {
                videoList.Add(new VideoCoverDisplayClass(item, 500, 300));
            }
        }

        private void MoreLikeVideoClick(object sender, RoutedEventArgs e)
        {
            Tuple<List<string>, string, bool> TypesAndName = new(new() { "is_like" }, "1", false);

            Frame.Navigate(typeof(ActorInfoPage), TypesAndName);
        }

        private void MoreLookLaterVideoClick(object sender, RoutedEventArgs e)
        {
            Tuple<List<string>, string, bool> TypesAndName = new(new() { "look_later" }, "1", false);

            Frame.Navigate(typeof(ActorInfoPage), TypesAndName);
        }

        private async void RefreshNewestVideoButtonClick(object sender, RoutedEventArgs e)
        {
            recentCoverList.Clear();

            foreach (var videoInfo in await DataAccess.Get.GetNameAndImageRecent())
            {
                recentCoverList.Add(new VideoCoverDisplayClass(videoInfo, 500, 300));
            }
            
        }

        private async void RefreshLookLaterVideoButtonClick(object sender, RoutedEventArgs e)
        {
            lookLaterList.Clear();

            foreach (var videoInfo in await DataAccess.Get.GetNameAndImageFromLookLater())
            {
                lookLaterList.Add(new VideoCoverDisplayClass(videoInfo, 500, 300));
            }
        }

        private async void RefreshLikeVideoButtonClick(object sender, RoutedEventArgs e)
        {
            LoveCoverList.Clear();

            foreach (var videoInfo in await DataAccess.Get.GetNameAndImageFromLike())
            {
                LoveCoverList.Add(new VideoCoverDisplayClass(videoInfo, 500, 300));
            }
        }

        private async void TryUpdateCoverShow()
        {
            //最近视频
            TryUpdateVideoCoverDisplayClass(await DataAccess.Get.GetNameAndImageRecent(), recentCoverList);
            //稍后观看
            TryUpdateVideoCoverDisplayClass(await DataAccess.Get.GetNameAndImageFromLookLater(), lookLaterList);
            //喜欢视频
            TryUpdateVideoCoverDisplayClass(await DataAccess.Get.GetNameAndImageFromLike(), LoveCoverList);
        }
    }

}
