using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Data;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Linq;
using OpenCvSharp.Flann;
using System.ComponentModel;

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
        private enum navigationAnimationType { image, gridView};
        private navigationAnimationType _navigationType;
        private VideoCoverDisplayClass _storeditem;
        private GridView _stroedgridview;
        private Image _storedimage;

        public HomePage()
        {
            this.InitializeComponent();

            //启动缓存
            NavigationCacheMode = Microsoft.UI.Xaml.Navigation.NavigationCacheMode.Enabled;

            loadCover();
        }

        private void loadCover()
        {
            //随机获取20个视频，每次启动自动获取一遍
            foreach (var item in DataAccess.getNameAndIamgeRandom())
            {
                Items.Add(new VideoCoverDisplayClass(item,500,300));
            }

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

        private void Image_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            _storedimage = (Image)sender;

            VideoCoverDisplayClass coverInfo = _storedimage.DataContext as VideoCoverDisplayClass;

            _navigationType = navigationAnimationType.image;

            ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("ForwardConnectedAnimation", _storedimage);

            Frame.Navigate(typeof(DetailInfoPage), coverInfo, new SuppressNavigationTransitionInfo());
        }

        private void Image_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            Grid grid = sender as Grid;
            grid.BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.LightGray);
            ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Hand);

        }

        private void Image_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            Grid grid = sender as Grid;
            grid.BorderBrush = null;
            ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Arrow);
        }

        private void videoInfoListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            videoInfoListView.ScrollIntoView((sender as ListView).SelectedItem);
        }

        private void UpdateRandomCover_Click(object sender, RoutedEventArgs e)
        {
            RefreshHyperlinkButton.IsEnabled = false;
            Items.Clear();

            //随机获取20个视频
            foreach (var item in DataAccess.getNameAndIamgeRandom())
            {
                Items.Add(new VideoCoverDisplayClass(item,500,300));
            }

            RefreshHyperlinkButton.IsEnabled = true;
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
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
            tryUpdateCoverShow();

        }

        private void tryUpdateCoverShow()
        {
            //稍后观看
            tryUpdateVideoCoverDisplayClass(DataAccess.getNameAndImageFromLookLater(),lookLaterList);

            //最近视频
            tryUpdateVideoCoverDisplayClass(DataAccess.getNameAndIamgeRecent(), recentCoverList);

            //喜欢视频
            tryUpdateVideoCoverDisplayClass(DataAccess.getNameAndImageFromLike(), LoveCoverList);

        }

        private void tryUpdateVideoCoverDisplayClass(List<VideoInfo> videoInfos, ObservableCollection<VideoCoverDisplayClass> videoList)
        {
            //添加
            var addList = new List<VideoInfo>();
            foreach (var item in videoInfos)
            {
                bool isAdd = true;
                foreach (var showItem in videoList)
                {
                    if (showItem.truename == item.truename)
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
                    if (showItem.truename == item.truename)
                    {
                        isDel = false;
                    }
                }

                if (isDel)
                {
                    delList.Add(showItem.truename);
                }
            }

            foreach (var trueName in delList)
            {
                var delItem = videoList.FirstOrDefault(x=>x.truename == trueName);
                videoList.Remove(delItem);
            }
            foreach (var item in addList)
            {
                videoList.Add(new VideoCoverDisplayClass(item, 500, 300));
            }
        }

        private void MoreLikeVideoClick(object sender, RoutedEventArgs e)
        {
            Tuple<List<string>, string> TypesAndName = new(new() { "is_like" }, "1");

            Frame.Navigate(typeof(ActorInfoPage), TypesAndName);
        }

        private void MoreLookLaterVideoClick(object sender, RoutedEventArgs e)
        {
            Tuple<List<string>, string> TypesAndName = new(new() { "look_later" }, "1");

            Frame.Navigate(typeof(ActorInfoPage), TypesAndName);
        }
    }

}
