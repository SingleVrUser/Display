using Display.Models.Image;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using ScrollViewerViewChangedEventArgs = Microsoft.UI.Xaml.Controls.ScrollViewerViewChangedEventArgs;
using UserControl = Microsoft.UI.Xaml.Controls.UserControl;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.Controls
{
    public sealed partial class ImageViewer : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(nameof(ItemsSource), typeof(object), typeof(ImageViewer), null);

        public static readonly DependencyProperty SelectedIndexProperty =
            DependencyProperty.Register(nameof(SelectedIndex), typeof(int), typeof(ImageViewer), new PropertyMetadata(-1));

        public event EventHandler<int> SelectionChanged;

        public object ItemsSource
        {
            get => GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        private int TotalCount
        {
            get
            {
                if (ItemsSource is ICollectionView collectionView)
                {
                    return collectionView.Count;
                }

                return ItemsSource is not IList list ? 0 : list.Count;
            }
        }

        private bool HavePrevious => SelectedIndex != 0;

        private bool HaveNext => SelectedIndex != TotalCount - 1;

        public BaseImage CurrentItemSource
        {
            get
            {
                if (SelectedIndex == -1) return null;

                if (ItemsSource is ICollectionView collectionView)
                {
                    return (BaseImage)collectionView.CurrentItem;
                }

                if (ItemsSource is not IList list) return null;

                // IList<T extends BaseImage>ÀàÐÍ
                if (list.Count == 0) return null;
                var aObject = list[0];
                if (aObject is BaseImage)
                {
                    return (BaseImage)list[SelectedIndex];
                }

                return null;
            }
        }

        public int SelectedIndex
        {
            get => (int)GetValue(SelectedIndexProperty);
            set
            {
                if (ItemsSource == null) return;
                if (value >= TotalCount || SelectedIndex == value) return;

                SetValue(SelectedIndexProperty, value);

                if (value < 0)
                {
                    ShowImage.Source = null;
                    return;
                }

                SelectionChanged?.Invoke(this, value);
                OnPropertyChanged(nameof(CurrentItemSource));
            }
        }

        private System.Timers.Timer _timer;

        public ImageViewer()
        {
            this.InitializeComponent();
        }

        public void ChangedImage(int index)
        {
            Debug.WriteLine($"ÇÐ»»µ½{index}");

            var image = CurrentItemSource;
            if (image is null) return;

            BitmapImage bitmapImage;
            if (image.Thumbnail == null)
            {
                return;
                //bitmapImage = new BitmapImage();

                //var filePath = LocalCacheHelper.GetCacheFilePath(image.FileInfo.Name);
                //var file = await StorageFile.GetFileFromPathAsync(filePath);

                //using var fileStream = await file.OpenAsync(FileAccessMode.Read);
                //await bitmapImage.SetSourceAsync(fileStream);

                //image.Thumbnail = bitmapImage;
            }
            else
            {
                bitmapImage = image.Thumbnail;
            }

            var height = bitmapImage.PixelHeight;
            var width = bitmapImage.PixelWidth;

            var factor = Math.Min(MyScrollViewer.ViewportHeight / height, MyScrollViewer.ViewportWidth / width);

            ShowImage.Source = bitmapImage;
            MyScrollViewer.ChangeView(null, null, factor > 1 ? 1 : (float)factor);  // disableZoomAnimal»áµ¼ÖÂËõ·ÅÊ§Ð§
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (!HaveNext) return;
            SelectedIndex++;

            if (HaveNext) return;

            RightButton.Visibility = Visibility.Collapsed;
            ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Arrow);
        }

        private void LastButton_Click(object sender, RoutedEventArgs e)
        {
            if (!HavePrevious) return;
            SelectedIndex--;

            if (HavePrevious) return;
            LeftButton.Visibility = Visibility.Collapsed;
            ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Arrow);
        }

        #region ÐüÍ£ÑÓ³ÙÒþ²Ø

        private void ScrollViewer_OnViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (_timer == null)
            {
                _timer = new System.Timers.Timer(1000);
                _timer.Elapsed += (_, _) =>
                {
                    _timer.Stop();

                    DispatcherQueue.TryEnqueue(() =>
                    {
                        AdditionalContentBorder.Visibility = Visibility.Collapsed;
                    });
                };
            }
            else
            {
                _timer.Stop();
            }
            _timer.Start();

            AdditionalContentBorder.Visibility = Visibility.Visible;
        }

        private void LeftButton_PointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (!HavePrevious) return;
            LeftButton.Visibility = Visibility.Visible;
            ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Hand);
        }

        private void LeftButton_PointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (!HavePrevious) return;
            LeftButton.Visibility = Visibility.Collapsed;
            ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Arrow);
        }
        private void RightButton_PointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (!HaveNext) return;
            RightButton.Visibility = Visibility.Visible;
            ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Hand);
        }

        private void RightButton_PointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (!HaveNext) return;
            RightButton.Visibility = Visibility.Collapsed;
            ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Arrow);
        }

        #endregion

        #region ¼üÅÌ¿ì½Ý¼ü

        private void KeyboardAcceleratorLeft_OnInvoked(Microsoft.UI.Xaml.Input.KeyboardAccelerator sender, Microsoft.UI.Xaml.Input.KeyboardAcceleratorInvokedEventArgs args)
        {
            if (!HavePrevious) return;
            SelectedIndex--;
        }

        private void KeyboardAcceleratorRight_OnInvoked(Microsoft.UI.Xaml.Input.KeyboardAccelerator sender, Microsoft.UI.Xaml.Input.KeyboardAcceleratorInvokedEventArgs args)
        {
            if (!HaveNext) return;
            SelectedIndex++;
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
