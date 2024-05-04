using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using DataAccess.Dao.Interface;
using DataAccess.Models.Entity;
using Display.Models.Vo;
using Display.Models.Vo.IncrementalCollection;
using Display.Providers;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using VideoCoverVo = Display.Models.Vo.Video.VideoCoverVo;

namespace Display.Controls.UserController;

public sealed partial class VideoCoverDisplay : INotifyPropertyChanged
{
    //标题
    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(nameof(Title), typeof(string), typeof(VideoCoverDisplay), null);

    private readonly IVideoInfoDao _videoInfoDao = App.GetService<IVideoInfoDao>();
    private readonly IActorInfoDao _actorInfoDao = App.GetService<IActorInfoDao>();

    private ActorInfo _actorInfo;
    public ActorInfo ActorInfo
    {
        get => _actorInfo;
        private set
        {
            if (_actorInfo == value) return;

            _actorInfo = value;

            ChangedHyperlink();

            OnPropertyChanged();
        }
    }

    /// <summary>
    /// 显示的标题
    /// </summary>
    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }


    /// <summary>
    /// 显示的数据
    /// 用于增量显示，成功列表
    /// </summary>
    private IncrementalLoadSuccessInfoCollection _successInfoCollection;
    public IncrementalLoadSuccessInfoCollection SuccessInfoCollection
    {
        get => _successInfoCollection;
        private set
        {
            _successInfoCollection = value;

            OnPropertyChanged();
        }
    }

    /// <summary>
    /// 图片的最小值
    /// 与Slider对应
    /// </summary>
    public const double SliderMinValue = 200;

    /// <summary>
    /// 图片的最大值
    /// 与Slider对应
    /// </summary>
    public const double SliderMaxValue = 900;

    private bool _isFuzzyQueryActor = true;

    private List<string> _filterConditionList;
    private Dictionary<string, string> _filterRanges;
    private string _filterKeywords;

    public VideoCoverDisplay()
    {
        InitializeComponent();
    }

    /// <summary>
    /// 用于动态调整图片大小的值
    /// 经验（(padding + 4 或 5)*2）
    /// ( 4 + 5 )*2
    /// </summary>
    private const double HorizontalPadding = 18;

    /// <summary>
    /// 标记Slider的值
    /// </summary>
    private double _markSliderValue;

    /// <summary>
    /// Slider值改变后，调整图片大小
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Slider_valueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
        _markSliderValue = e.NewValue;

        //动态调整图片大小
        if (IsAutoAdjustImageSizeToggleButton.IsChecked == true)
        {
            AutoAdjustImageSize();
        }
        //图片大小固定
        else
        {
            AdjustImageSize(e.NewValue);
        }
    }

    /// <summary>
    /// 动态调整图片大小
    /// </summary>
    /// <param name="gridWidth"></param>
    /// <param name="adjustSliderValue"></param>
    private void AutoAdjustImageSize(double gridWidth = -1, bool adjustSliderValue = false)
    {
        if (gridWidth <= 0)
            gridWidth = BasicGridView.ActualWidth;

        var imageCountPerRow = Math.Floor(gridWidth / (_markSliderValue + HorizontalPadding));
        if (imageCountPerRow <= 0) imageCountPerRow = 1;
        //System.Diagnostics.Debug.WriteLine($"每行图片数量：{ImageCountPerRow}");

        var newImageWidth = gridWidth / imageCountPerRow - HorizontalPadding;
        //System.Diagnostics.Debug.WriteLine($"推算出的图片宽度应为：{newImageWidth}");

        //必须要在一定范围内（Slider的最大最小值）
        if (newImageWidth is >= SliderMinValue and <= SliderMaxValue)
        {
            // SliderValue的0.5~1.5倍
            // 又或者，每行图片最大为1的话，可以缩小到最小值
            if ((_markSliderValue * 0.5 <= newImageWidth && newImageWidth <= _markSliderValue * 1.5) || imageCountPerRow == 1)
            {
                AdjustImageSize(newImageWidth);

                if (adjustSliderValue) AdjustSliderValueOnly(newImageWidth);

            }
        }
    }

    /// <summary>
    /// 固定值调整图片大小
    /// </summary>
    /// <param name="width"></param>
    private void AdjustImageSize(double width)
    {
        if (SuccessInfoCollection == null) return;

        //var height = width / 3 * 2;

        foreach (var t in SuccessInfoCollection)
        {
            t.ImageWidth = width;
        }

        //更改应用设置
        ImageWidth = width;

        //当前匹配的是成功
        //更新获取图片大小的值
        SuccessInfoCollection.SetImageSize(width);
    }

    private double? _imageWidth;

    private double ImageWidth
    {
        get
        {
            _imageWidth ??= AppSettings.ImageWidth;
            return _imageWidth.Value;
        }
        set
        {
            _imageWidth = value;
            AppSettings.ImageWidth = value;
        }

    }

    private void MoreButton_Click(object sender, RoutedEventArgs e)
    {
        MoreButtonClick?.Invoke(sender, e);
    }

    /// <summary>
    /// 鼠标悬停在Grid，显示可操作按钮
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Grid_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        if (sender is not Grid grid) return;
        grid.Children[1].Visibility = Visibility.Visible;
    }

    /// <summary>
    /// 鼠标移出在Grid，隐藏可操作按钮
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Grid_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        if (sender is not Grid grid) return;

        if (grid.Children[1] is not Grid collapsedGrid) return;

        collapsedGrid.Visibility = Visibility.Collapsed;

    }

    private void Button_OnPointerEntered(object sender, PointerRoutedEventArgs e)
    {
        ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Hand);
    }

    private void Button_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Arrow);
    }

    /// <summary>
    /// 点击了喜欢按钮
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void LikeToggleButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not AppBarToggleButton { DataContext: VideoCoverVo videoInfo } button) return;

        var isLike = button.IsChecked == true;
        videoInfo.IsLike = isLike;
        _videoInfoDao.ExecuteUpdate(i=>i.Id.Equals(videoInfo.Id), info
            => info.Interest.IsLike = isLike);
    }
    
    /// <summary>
    /// 点击了稍后观看
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void LookLaterToggleButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not AppBarToggleButton { DataContext: VideoCoverVo videoInfo } button) return;

        var isLookLater = button.IsChecked == true;
        videoInfo.IsLike = isLookLater;
        _videoInfoDao.ExecuteUpdate(i=>i.Id.Equals(videoInfo.Id), info
            => info.Interest.IsLookAfter = isLookLater);
    }

    /// <summary>
    /// 修改成功列表的评分
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void RatingControl_ValueChanged(RatingControl sender, object args)
    {
        if (sender.DataContext is not VideoCoverVo videoInfo) return;

        var score = videoInfo.Score == 0 ? -1 : sender.Value;
        
        _videoInfoDao.ExecuteUpdate(i=>i.Id.Equals(videoInfo.Id), info
            => info.Interest.Score = score);

    }

    /// <summary>
    /// 点击播放键
    /// </summary>

    private void PlayButton_Click(object sender, RoutedEventArgs e)
    {
        VideoPlayClick?.Invoke(sender, e);
    }

    private string _successListOrderBy;
    private bool _successListIsDesc;
    
    /// <summary>
    /// 按类型排序（用于成功列表）
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OrderSuccessListView_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (sender is not ListView selectListView) return;

        if (e.ClickedItem is not StackPanel clickStackPanel) return;
        
        if (clickStackPanel.Children.First(x => x is TextBlock)
            is not TextBlock selectTextBlock) return;

        if (clickStackPanel.Children.Last(x => x is FontIcon)
            is not FontIcon lastFontIcon) return;
        
        var selectOrderText = selectTextBlock.Text;

        var upGlyph = "\xE014";
        var downGlyph = "\xE015";
        string newGlyph;

        //原图标
        var isUpSort = lastFontIcon.Glyph == upGlyph;

        //更新降序或升序图标
        //注意：随机 无需升/降序
        if (selectListView.SelectedItem == e.ClickedItem && selectOrderText != "随机")
        {
            lastFontIcon.Glyph = isUpSort ? downGlyph : upGlyph;
        }

        //现图标
        isUpSort = lastFontIcon.Glyph == upGlyph;
        _successListIsDesc = !isUpSort;

        switch (selectOrderText)
        {
            case "名称":
                newGlyph = "\xE185";
                _successListOrderBy = "truename";
                break;
            case "演员":
                newGlyph = "\xE13D";
                _successListOrderBy = "actor";
                break;
            case "年份":
                newGlyph = "\xEC92";
                _successListOrderBy = "releasetime";
                break;
            case "随机":
                newGlyph = "\xF463";
                _successListOrderBy = "random";
                break;
            default:
                newGlyph = "\xE185";
                _successListOrderBy = "truename";
                break;
        }

        //更新首图标
        if (OrderButton.Content is string content && content != newGlyph)
        {
            OrderButton.Content = newGlyph;
        }

        LoadDstSuccessInfoCollection();
    }

    /// <summary>
    /// 点击了删除按钮
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void DeleteAppBarButton_Click(object sender, RoutedEventArgs e)
    {
        ContentDialog dialog = new ContentDialog()
        {
            XamlRoot = XamlRoot,
            Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
            Title = "确认",
            PrimaryButtonText = "删除",
            CloseButtonText = "返回",
            DefaultButton = ContentDialogButton.Close,
            Content = "该操作只删除本地数据库数据，不对115服务器进行操作，确认删除？"
        };

        var result = await dialog.ShowAsync();

        if (result != ContentDialogResult.Primary) return;

        if (sender is not AppBarButton { DataContext: VideoCoverVo item }) return;

        //从数据库中删除
        _videoInfoDao.ExecuteDeleteById(item.Id);

        //删除存储的文件夹
        var savePath = Path.Combine(AppSettings.ImageSavePath, item.Name);
        if (Directory.Exists(savePath))
        {
            Directory.Delete(savePath, true);
        }

        //FileGrid.Remove(item);
        SuccessInfoCollection.Remove(item);
    }

    //开始动画
    public async void StartAnimation(ConnectedAnimation animation, VideoCoverVo item)
    {
        if (BasicGridView.Items.Contains(item))
        {
            //开始动画
            await BasicGridView.TryStartConnectedAnimationAsync(animation, item, "showImage");
        }
    }

    private void ShowType_RadioButtons_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems[0] is not ListViewItem item) return;

        switch (item.Name)
        {
            //匹配成功
            case nameof(SuccessData_RadioButton):
                TrySwitchToSuccessView();
                break;
        }
    }

    /// <summary>
    /// 切换到成功视图
    /// </summary>
    private async void TrySwitchToSuccessView()
    {
        //更新GridView的来源

        SuccessInfoCollection ??= new IncrementalLoadSuccessInfoCollection(ImageWidth);
        SuccessInfoCollection.SetFilter(_filterConditionList, _filterKeywords, _isFuzzyQueryActor);
        await SuccessInfoCollection.LoadData();

        BasicGridView.ItemsSource = SuccessInfoCollection;

        if (BasicGridView.Visibility == Visibility.Collapsed) BasicGridView.Visibility = Visibility.Visible;

        //初始化Slider的值
        _markSliderValue = ImageWidth;
        ImageSizeChangeSlider.Value = ImageWidth;

        //开始监听调整图片大小的Slider
        StartListeningSliderValueChanged();

        //是否需要动态调整图片大小
        TryStartListeningGridSizeChanged();

        if (OrderButton.Visibility == Visibility.Collapsed) OrderButton.Visibility = Visibility.Visible;

    }

    /// <summary>
    /// 移至顶部
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ToTopButton_Click(object sender, RoutedEventArgs e)
    {
        if (BasicGridView.ItemsSource is IncrementalLoadSuccessInfoCollection { Count: > 0 } successCollection)
        {
            BasicGridView.ScrollIntoView(successCollection.First());
        }

    }

    /// <summary>
    /// 打开对Grid大小的监听，以动态调整图片大小
    /// </summary>
    private void TryStartListeningGridSizeChanged()
    {
        if (IsAutoAdjustImageSizeToggleButton.IsChecked != true) return;

        //监听前先调整
        AutoAdjustImageSize();

        //开始监听
        BasicGridView.SizeChanged += BasicGridView_SizeChanged;
    }

    /// <summary>
    /// 开始监听调整图片大小的Slider
    /// </summary>
    private void StartListeningSliderValueChanged()
    {
        ImageSizeChangeSlider.ValueChanged += Slider_valueChanged;
    }

    /// <summary>
    /// 停止监听动态调整图片大小
    /// </summary>
    private void CloseListeningGridSizeChanged()
    {
        BasicGridView.SizeChanged -= BasicGridView_SizeChanged;
    }

    /// <summary>
    /// 图片模式下根据Grid调整大小
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void BasicGridView_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        var newGridWidth = e.NewSize.Width;

        AutoAdjustImageSize(newGridWidth, true);

    }

    /// <summary>
    /// 仅仅调整Slider的数值
    /// </summary>
    /// <param name="newImageWidth"></param>
    private void AdjustSliderValueOnly(double newImageWidth)
    {
        //更改Slider数值
        ImageSizeChangeSlider.ValueChanged -= Slider_valueChanged;
        ImageSizeChangeSlider.Value = newImageWidth;
        ImageSizeChangeSlider.ValueChanged += Slider_valueChanged;
    }

    /// <summary>
    /// 启用图片大小的动态调整
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void AutoAdjustImageSize_ToggleButton_Checked(object sender, RoutedEventArgs e)
    {
        //开始监听
        TryStartListeningGridSizeChanged();

        //初次调整大小
        AutoAdjustImageSize();
    }

    /// <summary>
    /// 关闭图片大小的动态调整
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void AutoAdjustImageSize_ToggleButton_UnChecked(object sender, RoutedEventArgs e)
    {
        CloseListeningGridSizeChanged();
    }

    private void InfoListFilter_SplitButton_Click(SplitButton sender, SplitButtonClickEventArgs args)
    {
        if (sender is not ToggleSplitButton button) return;

        //选中
        if (button.IsChecked)
        {
            switch (sender.Tag)
            {
                case "Year":
                    _filterRanges ??= new Dictionary<string, string>();
                    _filterRanges["Year"] = InfosFilter.Year;
                    break;
                case "Score":
                    _filterRanges ??= new Dictionary<string, string>();
                    _filterRanges["Score"] = InfosFilter.Score.ToString();
                    break;
                case "Type":
                    _filterRanges ??= new Dictionary<string, string>();
                    _filterRanges["Type"] = InfosFilter.Type;
                    break;
            }
        }
        //取消选中
        else
        {
            _filterRanges.Remove(sender.Tag.ToString() ?? string.Empty);
        }

        LoadDstSuccessInfoCollection();
    }

    private void InfoListFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is not RadioButtons radioButtons) return;
        if (radioButtons.SelectedItem is not RadioButton radioButton) return;

        var key = radioButtons.Tag.ToString();
        if (string.IsNullOrEmpty(key)) return;

        var value = radioButton.Tag != null ? radioButton.Tag.ToString() : radioButton.Content.ToString();
        _filterRanges ??= new Dictionary<string, string>();

        _filterRanges[key] = value;

        LoadDstSuccessInfoCollection();
    }

    private void InfoListFilter_TextChanged(object sender, TextChangedEventArgs e)
    {
        const string key = "Year";
        var value = InfosFilter.Year;
        _filterRanges ??= new Dictionary<string, string>();

        _filterRanges[key] = value;
        LoadDstSuccessInfoCollection();
    }

    private async void LoadDstSuccessInfoCollection()
    {
        SuccessInfoCollection = new IncrementalLoadSuccessInfoCollection(ImageWidth);
        BasicGridView.ItemsSource = SuccessInfoCollection;

        SuccessInfoCollection.SetOrder(_successListOrderBy, _successListIsDesc);
        SuccessInfoCollection.SetRange(_filterRanges);
        SuccessInfoCollection.SetFilter(_filterConditionList, _filterKeywords, _isFuzzyQueryActor);
        await SuccessInfoCollection.LoadData();
    }

    private void Filter_ToggleButton_Unchecked(object sender, RoutedEventArgs e)
    {
        if (_filterRanges is not { Count: > 0 }) return;

        _filterRanges = null;
        InfosFilter.UncheckAllToggleSplitButton();
        LoadDstSuccessInfoCollection();
    }

    private void LikeActorButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not ToggleButton isLikeButton) return;

        if (isLikeButton.Tag is not int actorId) return;

        if (isLikeButton.IsChecked == null) return;

        var isLike = isLikeButton.IsChecked == true;
        
        _actorInfoDao.ExecuteUpdate(i => Equals(actorId, i.Id),
            i => i.Interest.IsLike = isLike);
    }

    private void ChangedHyperlink()
    {
        if (!string.IsNullOrEmpty(ActorInfo.BlogUrl))
        {
            BlogHyperLink.NavigateUri = new Uri(ActorInfo.BlogUrl);
        }

        if (!string.IsNullOrEmpty(ActorInfo.InfoUrl))
        {
            InfoHyperLink.NavigateUri = new Uri(ActorInfo.InfoUrl);
        }
    }

    private void ShowData_RadioButtons_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        Debug.WriteLine("失败列表");
    }

    public event RoutedEventHandler VideoPlayClick;
    public event RoutedEventHandler SingleVideoPlayClick;
    public event RoutedEventHandler MoreButtonClick;
    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}