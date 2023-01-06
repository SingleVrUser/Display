using Data;
using Display.Model;
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
        //public ObservableCollection<ActorsInfo> actorinfo = new();

        //List<ActorsInfo> actorinfoList;

        IncrementallLoadActorInfoCollection actorinfo;
        ObservableCollection<ActorInfo> actorPartInfo = new();

        //过渡动画用
        private enum navigationAnimationType { image, gridView };
        private navigationAnimationType _navigationType;
        private ActorInfo _storeditem;
        private Image _storedimage;

        public ActorsPage()
        {
            this.InitializeComponent();

        }

        private async void Page_Loaded()
        {
            ProgressRing.IsActive = true;

            actorinfo = new(new() { { "is_like", true }, { "prifile_path", true } });

            await actorinfo.LoadData();

            TotalCount_TextBlock.Text = actorinfo.Count.ToString();
            BasicGridView.ItemsSource = actorinfo;

            LoadActorPartInfo();

            ProgressRing.IsActive = false;
        }

        private void CarouselControl_Loaded(object sender, RoutedEventArgs e)
        {
            Page_Loaded();
        }

        private async void LoadActorPartInfo(int count = 30)
        {
            var infos = await DataAccess.LoadActorInfo(count,0,orderByList: new (){ { "RANDOM()", false } },
                                                                filterList: new (){ "prifile_path != ''" });

            infos.ForEach(info => actorPartInfo.Add(info));

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
                var actorinfo = container.Content as ActorInfo;

                _navigationType = navigationAnimationType.gridView;
                _storeditem = actorinfo;
                BasicGridView.PrepareConnectedAnimation("ForwardConnectedAnimation", _storeditem, "ActorImage");

                GoToActorInfo(actorinfo);
            }
        }

        private void GoToActorInfo(ActorInfo actorinfo)
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

            CarouselControl.SelectedIndex = actorPartInfo.Count/2;
        }

        private void Image_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (!(sender is Image image)) return;

            _navigationType = navigationAnimationType.image;
            _storedimage = image;
            ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("ForwardConnectedAnimation", _storedimage);

            if (image.DataContext is ActorInfo actorinfo)
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

            if (image.DataContext is ActorInfo actorinfo)
            {
                if (isActorInfoCurrentSelected(actorinfo))
                    ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Hand);
            }
        }

        private void Img_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (!(sender is Image image)) return;

            if (image.DataContext is ActorInfo actorinfo)
            {
                if (isActorInfoCurrentSelected(actorinfo))
                    ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Arrow);
            }
        }

        private bool isActorInfoCurrentSelected(ActorInfo actorinfo)
        {
            if ((CarouselControl.SelectedItem == null && CarouselControl.SelectedIndex == 8) || CarouselControl.SelectedItem != null && CarouselControl.SelectedItem == actorinfo)
                return true;
            else
                return false;
        }

    }


}
