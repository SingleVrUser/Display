using Data;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.Services.Store;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ActorsPage : Page
    {
        public ObservableCollection<ActorsInfo> actorinfo = new();

        List<ActorsInfo> actorinfoList;
        ObservableCollection<ActorsInfo> actorPartInfo = new();

        //过渡动画用
        private enum navigationAnimationType { image, gridView };
        private navigationAnimationType _navigationType;
        private ActorsInfo _storeditem;
        private Image _storedimage;

        public ActorsPage()
        {
            this.InitializeComponent();

            BasicGridView.ItemsSource = actorinfo;
            CarouselControl.ItemsSource = actorPartInfo;
            Page_Loaded();
        }

        private async void Page_Loaded()
        {
            ProgressRing.IsActive = true;
            Dictionary<string, List<string>> ActorsInfoDict = new();
            List<VideoInfo> VideoInfoList = await DataAccess.LoadVideoInfo(-1);

            foreach (var VideoInfo in VideoInfoList)
            {
                string actor_str = VideoInfo.actor;

                var actor_list = actor_str.Split(",");
                foreach (var actor in actor_list)
                {
                    //当前名称不存在
                    if (!ActorsInfoDict.ContainsKey(actor))
                    {
                        ActorsInfoDict.Add(actor, new List<string>());
                    }
                    ActorsInfoDict[actor].Add(VideoInfo.truename);
                }
            }

            List<ActorsInfo> tmpInfos = new();
            foreach (var item in ActorsInfoDict)
            {
                tmpInfos.Add(new ActorsInfo
                {
                    name = item.Key,
                    count = item.Value.Count,
                });
            }

            actorinfoList = tmpInfos.OrderByDescending(x => x.name == "").ThenBy(x => x.prifilePhotoPath).ToList();
            actorinfoList.ForEach(x => actorinfo.Add(x));

            LoadActorPartInfo();

            ProgressRing.IsActive = false;
        }

        private void LoadActorPartInfo(int count = 30)
        {
            if (actorinfoList == null) return;

            Random rnd = new Random();
            actorinfoList.Where(item=>item.prifilePhotoPath!= "ms-appx:///Assets/NoPicture.jpg").OrderByDescending(item => rnd.Next()).Take(count).ToList().ForEach(item=>actorPartInfo.Add(item));

        }

        /// <summary>
        /// 跳转至演员详情页
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BasicGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (BasicGridView.ContainerFromItem(e.ClickedItem) is GridViewItem container)
            {
                var actorinfo = container.Content as ActorsInfo;

                _navigationType = navigationAnimationType.gridView;
                _storeditem = actorinfo;
                BasicGridView.PrepareConnectedAnimation("ForwardConnectedAnimation", _storeditem, "ActorImage");

                GoToActorInfo(actorinfo);
            }
        }

        private void GoToActorInfo(ActorsInfo actorinfo)
        {
            Tuple<List<string>, string> TypeAndName = new(new() { "actor" }, actorinfo.name);
            Frame.Navigate(typeof(ActorInfoPage), TypeAndName);
        }

        private void Grid_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Hand);
        }

        private void Grid_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Arrow);
        }

        private void UpdateRandomCover_Click(object sender, RoutedEventArgs e)
        {
            actorPartInfo.Clear();
            LoadActorPartInfo();

            CarouselControl.SelectedIndex = 8;
        }

        private void Image_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (!(sender is Image image)) return;

            _navigationType = navigationAnimationType.image;
            _storedimage = image;
            ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("ForwardConnectedAnimation", _storedimage);

            if (image.DataContext is ActorsInfo actorinfo)
            {
                if (isActorInfoCurrentSelected(actorinfo))
                    GoToActorInfo(actorinfo);
            }
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
                        await BasicGridView.TryStartConnectedAnimationAsync(animation, _storeditem, "ActorImage");
                    }
                }
            }
        }

        private void Img_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (!(sender is Image image)) return;

            if (image.DataContext is ActorsInfo actorinfo)
            {
                if (isActorInfoCurrentSelected(actorinfo))
                    ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Hand);
            }
        }

        private void Img_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (!(sender is Image image)) return;

            if (image.DataContext is ActorsInfo actorinfo)
            {
                if (isActorInfoCurrentSelected(actorinfo))
                    ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Arrow);
            }
        }

        private bool isActorInfoCurrentSelected(ActorsInfo actorinfo)
        {
            if ((CarouselControl.SelectedItem == null && CarouselControl.SelectedIndex == 8) || CarouselControl.SelectedItem != null && CarouselControl.SelectedItem == actorinfo)
                return true;
            else
                return false;
        }
    }


}
