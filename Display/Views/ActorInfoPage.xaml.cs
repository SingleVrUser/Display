using Data;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.Views
{

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ActorInfoPage : Page
    {
        //为返回动画做准备（需启动缓存）
        VideoCoverDisplayClass _storeditem;

        //ShowName需要实时更新
        private string ShowName
        {
            get { return (string)GetValue(ShowNameProperty); }
            set { SetValue(ShowNameProperty, value); }
        }

        private static readonly DependencyProperty ShowNameProperty =
            DependencyProperty.Register("ShowName", typeof(string), typeof(ActorInfoPage), null);

        public ActorInfoPage()
        {
            //启动缓存（为了返回无需过长等待，也为了返回动画）
            NavigationCacheMode = NavigationCacheMode.Enabled;

            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            //过渡动画
            if (e.NavigationMode == NavigationMode.Back)
            {
                ConnectedAnimation animation = ConnectedAnimationService.GetForCurrentView().GetAnimation("BackConnectedAnimation");
                if (animation != null)
                {
                    videoControl.StartAnimation(animation, _storeditem);
                }
            }
            else if (e.NavigationMode == NavigationMode.New)
            {
                var item = e.Parameter;
                LoadShowInfo(item);

                ConnectedAnimation animation = ConnectedAnimationService.GetForCurrentView().GetAnimation("ForwardConnectedAnimation");
                if (animation != null)
                {
                    animation.TryStart(showNameTextBlock);

                }
            }
        }

        private void LoadShowInfo(object item)
        {
            if (item == null) return;
            string[] typeAndName = item as string[];

            string type = typeAndName[0];
            ShowName = typeAndName[1];

            //显示的是演员还是标签
            List<VideoInfo> VideoInfoList = FileMatch.getVideoInfoFromType(type, ShowName);

            if (videoControl.FileGrid == null)
            {
                videoControl.FileGrid = new();
            }

            var newFileGrid = FileMatch.getFileGrid(VideoInfoList);

            videoControl.FileGrid = newFileGrid;
        }

        /// <summary>
        /// 选项选中后跳转至详情页
        /// </summary>
        private void OnClicked(object sender, RoutedEventArgs e)
        {
            VideoCoverDisplayClass item = (sender as Button).DataContext as VideoCoverDisplayClass;

            //准备动画
            videoControl.PrepareAnimation(item);
            _storeditem = item;
            Frame.Navigate(typeof(DetailInfoPage), item, new SuppressNavigationTransitionInfo());
        }

    }

}
