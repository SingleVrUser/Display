using Data;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.Control
{
    public sealed partial class MultipleCoverShow : UserControl
    {
        public string ShowName { get; set; }

        //private ObservableCollection<VideoCoverDisplayClass> _coverList = new();
        public ObservableCollection<VideoCoverDisplayClass> CoverList { get; set; } = new();

        //public ObservableCollection<VideoCoverDisplayClass> CoverList
        //{
        //    get
        //    {
        //        return (ObservableCollection<VideoCoverDisplayClass>)GetValue(CoverListProperty);
        //    }
        //    set
        //    {
        //        SetValue(CoverListProperty, value);
        //        tryUpdateCoverFlipItems();
        //    }
        //}
        //public static readonly DependencyProperty CoverListProperty =
        //    DependencyProperty.Register("CoverList", typeof(ObservableCollection<VideoCoverDisplayClass>), typeof(MultipleCoverShow), null);

        public ObservableCollection<CoverFlipItems> NewAddFlipItems = new();
        private int showCount = -1;
        private int _CoverCount = 0;

        public MultipleCoverShow()
        {
            this.InitializeComponent();
        }

        private void NewAddStackPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            tryUpdateCoverFlipItems();
        }

        //CoverList不变的情况下，更新封面显示数量
        private void tryUpdateCoverFlipItems()
        {
            if (CoverList == null) return;


            //最大的显示数量
            int count = ((int)NewAddStackPanel.RenderSize.Width - 20 - 3 * showCount) / 300;
            //修正item间距
            count = ((int)NewAddStackPanel.RenderSize.Width - 20 - 3 * count) / 300;

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
    }
}
