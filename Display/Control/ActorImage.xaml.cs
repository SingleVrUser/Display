using Data;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.Control
{
    public sealed partial class ActorImage : UserControl
    {
        private static readonly string noPictruePath = "ms-appx:///Assets/NoPicture.jpg";

        private string actorName;

        private int actorId;

        public ActorImage(string actorName)
        {
            this.InitializeComponent();

            //检查演员图片是否存在
            string imagePath = Path.Combine(AppSettings.ActorInfo_SavePath, actorName, "face.jpg");
            if (!File.Exists(imagePath))
            {
                imagePath = noPictruePath;
            }

            ShowImage.Source = new BitmapImage(new Uri(imagePath));

            this.actorName = actorName;
        }

        public ActorImage(ActorInfo actorInfo)
        {
            this.InitializeComponent();

            string imagePath;
            if (string.IsNullOrEmpty(actorInfo.prifile_path))
            {
                imagePath = noPictruePath;
            }
            else
            {
                imagePath = actorInfo.prifile_path;
            }
            ShowImage.Source = new BitmapImage(new Uri(imagePath));

            //是否喜欢
            if (actorInfo.is_like == 1)
            {
                LikeFontIcon.Visibility = Visibility.Visible;
            }

            this.actorName = actorInfo.name;
            this.actorId = actorInfo.id;
            //ShowText.Text = actorInfo.name;
        }


        public event RoutedEventHandler Click;
        private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Click?.Invoke(sender,e);
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

            DataAccess.UpdateSingleDataFromActorInfo(actorId.ToString(), "is_like", is_like.ToString());
        }
    }
}
