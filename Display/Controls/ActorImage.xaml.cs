
using Display.Views;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using Display.Data;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.Controls
{
    public sealed partial class ActorImage : UserControl
    {
        public ActorInfo ActorInfo;

        private string releaseTime;


        public ActorImage(ActorInfo actorInfo, string releaseTime)
        {
            this.InitializeComponent();

            this.ActorInfo = actorInfo;
            this.releaseTime = releaseTime;

            //是否可点击
            if (string.IsNullOrEmpty(actorInfo.name)) return;

            //是否喜欢
            if (actorInfo.is_like == 1)
            {
                LikeFontIcon.Visibility = Visibility.Visible;
            }

            //右键菜单
            RootGrid.ContextFlyout = this.Resources["LikeMenuFlyout"] as MenuFlyout;


            //监听Pointer后改变鼠标
            RootGrid.PointerEntered += Grid_PointerEntered;
            RootGrid.PointerExited += Grid_PointerExited;

            //显示作品时年龄
            tryShowActorAge(actorInfo.birthday);
        }

        private void tryShowActorAge(string birthday)
        {
            if (FileMatch.CalculatTimeStrDiff(birthday, releaseTime) is TimeSpan dtDif)
            {
                if (dtDif != TimeSpan.Zero)
                {
                    WorkYearDiff_TextBlock.Text = ((int)dtDif.TotalDays / 365).ToString();
                    WorkYearDiff_TextBlock.Visibility = Visibility.Visible;
                }
            }
        }


        public event RoutedEventHandler Click;
        private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Click?.Invoke(sender, e);
        }

        private void Grid_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Hand);
        }

        private void Grid_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Hand);
        }

        private void LikeMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            int is_like = 0;

            //通过当前状态判断将要设置的值
            switch (LikeFontIcon.Visibility)
            {
                case Visibility.Visible:
                    LikeFontIcon.Visibility = Visibility.Collapsed;
                    break;
                case Visibility.Collapsed:
                    LikeFontIcon.Visibility = Visibility.Visible;
                    is_like = 1;
                    break;
            }

            DataAccess.UpdateSingleDataFromActorInfo(ActorInfo.id.ToString(), "is_like", is_like.ToString());
        }

        private async void GetInfoMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            GetActorInfoProgressRing.Visibility = Visibility.Visible;

            var newInfo = await ActorsPage.GetActorInfo(ActorInfo);
            if (newInfo == null)
            {
                GetActorInfoProgressRing.Visibility = Visibility.Collapsed;
                return;
            }

            //更新头像
            if (!string.IsNullOrEmpty(newInfo.prifile_path))
            {
                ActorInfo.prifile_path = newInfo.prifile_path;
            }

            //更新年龄
            tryShowActorAge(newInfo.birthday);

            GetActorInfoProgressRing.Visibility = Visibility.Collapsed;
        }
    }
}
