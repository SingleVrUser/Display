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
        public ActorImage(string actorName)
        {
            this.InitializeComponent();

            //检查演员图片是否存在
            string imagePath = Path.Combine(AppSettings.ActorInfo_SavePath, actorName, "face.jpg");
            if (!File.Exists(imagePath))
            {
                imagePath = $"ms-appx:///Assets/NoPicture.jpg";
            }

            ShowImage.Source = new BitmapImage(new Uri(imagePath));

            ShowText.Text = actorName;
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
    }
}
