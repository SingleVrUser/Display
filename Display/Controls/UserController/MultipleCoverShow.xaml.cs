using System.Collections.ObjectModel;
using Display.Models.Dto.OneOneFive;
using Display.Views.Pages;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace Display.Controls.UserController;

public sealed partial class MultipleCoverShow
{
    public static readonly DependencyProperty MoreButtonVisibilityProperty =
        DependencyProperty.Register(nameof(MoreButtonVisibility), typeof(Visibility), typeof(VideoCoverPage), PropertyMetadata.Create(() => Visibility.Collapsed));

    public static readonly DependencyProperty RefreshButtonVisibilityProperty =
        DependencyProperty.Register(nameof(RefreshButtonVisibility), typeof(Visibility), typeof(VideoCoverPage), PropertyMetadata.Create(() => Visibility.Collapsed));

    //是否显示MoreButton
    public Visibility MoreButtonVisibility
    {
        get => (Visibility)GetValue(MoreButtonVisibilityProperty);
        set => SetValue(MoreButtonVisibilityProperty, value);
    }

    //是否显示RefreshButton
    public Visibility RefreshButtonVisibility
    {
        get => (Visibility)GetValue(RefreshButtonVisibilityProperty);
        set => SetValue(RefreshButtonVisibilityProperty, value);
    }

    public string ShowName { get; set; }

    public ObservableCollection<VideoCoverDisplayClass> CoverList { get; set; } = new();

    public MultipleCoverShow()
    {
        InitializeComponent();
    }

    //点击了图片
    public event ItemClickEventHandler ItemClick;
    private void GridView_ItemClick(object sender, ItemClickEventArgs e)
    {
        ItemClick?.Invoke(sender, e);

    }

    private void Image_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Hand);
    }

    private void Image_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Arrow);
    }

    public event RoutedEventHandler MoreClick;
    private void MoreButton_Click(object sender, RoutedEventArgs e)
    {
        MoreClick?.Invoke(sender, e);
    }

    public event RoutedEventHandler RefreshClick;
    private void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        RefreshClick?.Invoke(sender, e);
    }
}