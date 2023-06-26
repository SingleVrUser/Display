
using Display.Views;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System.Collections.ObjectModel;
using Display.Data;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.Controls
{
    public sealed partial class MultipleCoverShow : UserControl
    {
        public static readonly DependencyProperty MoreButtonVisibilityProperty =
            DependencyProperty.Register(nameof(MoreButtonVisibility), typeof(Visibility), typeof(ActorInfoPage), PropertyMetadata.Create(() => Visibility.Collapsed));
        
        public static readonly DependencyProperty RefreshButtonVisibilityProperty =
            DependencyProperty.Register(nameof(RefreshButtonVisibility), typeof(Visibility), typeof(ActorInfoPage), PropertyMetadata.Create(() => Visibility.Collapsed));

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

        private Visibility isContentNull(int coverCount)
        {
            return coverCount == 0 ? Visibility.Visible : Visibility.Collapsed;
        }


        public event RoutedEventHandler MoreClick;
        private void MoreButton_Click(object sender, RoutedEventArgs e)
        {
            MoreClick?.Invoke(sender, e);
        }

        public event RoutedEventHandler RefreshClick;
        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            RefreshClick?.Invoke(sender,e);
        }
    }
}
