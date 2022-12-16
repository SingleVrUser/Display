using Data;
using Display.Views;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Windows.Media.Core;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.Control
{
    public sealed partial class MultipleCoverShow : UserControl
    {
        public static readonly DependencyProperty MoreButtonVisibilityProperty =
            DependencyProperty.Register("MoreButtonVisibility", typeof(Visibility), typeof(ActorInfoPage), PropertyMetadata.Create(() => Visibility.Collapsed ));

        //是否显示MoreButton
        public Visibility MoreButtonVisibility
        {
            get { return (Visibility)GetValue(MoreButtonVisibilityProperty); }
            set { SetValue(MoreButtonVisibilityProperty, value); }
        }


        public string ShowName { get; set; }

        public ObservableCollection<VideoCoverDisplayClass> CoverList { get; set; } = new();

        public ObservableCollection<CoverFlipItems> NewAddFlipItems = new();
        private int showCount = -1;
        private int _CoverCount = 0;

        public MultipleCoverShow()
        {
            this.InitializeComponent();
        }

        private void NewAddGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            tryUpdateCoverFlipItems();
        }

        //CoverList不变的情况下，更新封面显示数量
        private void tryUpdateCoverFlipItems()
        {
            if (CoverList == null) return;

            //最大的显示数量
            int count = ((int)NewAddGrid.RenderSize.Width - 20 - 3 * showCount) / 300;
            //修正item间距
            count = ((int)NewAddGrid.RenderSize.Width - 20 - 3 * count) / 300;

            //什么时候更新封面
            // CoverList增减 / 最适宜显示数量发生改变
            if (_CoverCount != CoverList.Count || (count != showCount && count != 0))
            {
                showCount = count;
                NewAddFlipItems.Clear();
                UpdateCoverFlipItems(showCount);
            }


            _CoverCount = CoverList.Count;
        }

        private  void UpdateCoverFlipItems(int count)
        {
            for (int i = 0; i < CoverList.Count; i += count)
            {
                ObservableCollection<VideoCoverDisplayClass> NewItems = new();
                for (int j = 0; j < count && i + j < CoverList.Count; j++)
                {
                    NewItems.Add(CoverList[i + j]);
                }
                NewAddFlipItems.Add(new CoverFlipItems() { CoverItems = NewItems});
            }
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
    }
}
