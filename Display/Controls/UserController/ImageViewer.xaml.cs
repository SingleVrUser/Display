using System;
using System.Collections;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using BaseImage = Display.Models.Media.BaseImage;
using ScrollViewerViewChangedEventArgs = Microsoft.UI.Xaml.Controls.ScrollViewerViewChangedEventArgs;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.Controls.UserController;

public sealed partial class ImageViewer : INotifyPropertyChanged
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
        var image = CurrentItemSource;
        if (image is null) return;

        if (image.Thumbnail == null) return;

        var bitmapImage = image.Thumbnail;

        var height = bitmapImage.PixelHeight;
        var width = bitmapImage.PixelWidth;

        var factor = Math.Min(MyScrollViewer.ViewportHeight / height, MyScrollViewer.ViewportWidth / width);

        ShowImage.Source = bitmapImage;
        MyScrollViewer.ChangeView(null, null, factor > 1 ? 1 : (float)factor);
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

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}