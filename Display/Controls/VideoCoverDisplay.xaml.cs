using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Display.Data;
using SharpCompress;
using Display.Models.IncrementalCollection;

namespace Display.Controls;

public sealed partial class VideoCoverDisplay : UserControl, INotifyPropertyChanged
{
    //标题
    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(nameof(Title), typeof(string), typeof(VideoCoverDisplay), null);

    //显示的是匹配成功的还是失败的
    public static readonly DependencyProperty IsShowFailListViewProperty =
        DependencyProperty.Register(nameof(IsShowFailListView), typeof(bool), typeof(VideoCoverDisplay), null);

    public static readonly DependencyProperty IsShowSearchListViewProperty =
        DependencyProperty.Register(nameof(IsShowSearchListView), typeof(bool), typeof(VideoCoverDisplay), PropertyMetadata.Create(() => false));

    private bool IsShowSucAndFailSwitchButton => !IsShowSearchListView;

    private ActorInfo _actorInfo;
    public ActorInfo ActorInfo
    {
        get => _actorInfo;
        set
        {
            if (_actorInfo == value) return;

            _actorInfo = value;

            ChangedHyperlink();

            OnPropertyChanged();
        }
    }

    private bool _isShowHeaderCover = false;
    public bool IsShowHeaderCover
    {
        get => _isShowHeaderCover;
        set
        {
            if (_isShowHeaderCover == value) return;

            _isShowHeaderCover = value;

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
    /// 是否显示成功列表
    /// </summary>
    private bool IsShowSuccessListView => !IsShowFailListView;

    /// <summary>
    /// 当前显示失败列表
    /// </summary>
    public bool IsShowFailListView
    {
        get => (bool)GetValue(IsShowFailListViewProperty);
        set
        {
            if (IsShowFailListView == value) return;

            SetValue(IsShowFailListViewProperty, value);

            OnPropertyChanged(nameof(IsShowSuccessListView));

        }
    }

    /// <summary>
    /// 是否显示的是搜索结果
    /// </summary>
    public bool IsShowSearchListView
    {
        get => (bool)GetValue(IsShowSearchListViewProperty);
        set => SetValue(IsShowSearchListViewProperty, value);
    }

    /// <summary>
    /// 切换排序Flyout(成功或失败)
    /// </summary>
    /// <param name="isShowSuccessFlyout"></param>
    private void ChangedOrderButtonFlyout(bool isShowSuccessFlyout)
    {
        //显示成功的排序
        if (isShowSuccessFlyout)
        {
            OrderButton.Flyout = this.Resources["SuccessOrderFlyout"] as Flyout;
        }
        //显示失败的排序
        else
        {
            OrderButton.Flyout = this.Resources["FailOrderFlyout"] as Flyout;
        }
    }

    /// <summary>
    /// 显示的数据
    /// 用于增量显示，成功列表
    /// </summary>
    private IncrementalLoadSuccessInfoCollection _successInfoCollection;
    public IncrementalLoadSuccessInfoCollection SuccessInfoCollection
    {
        get => _successInfoCollection;
        set
        {
            _successInfoCollection = value;

            OnPropertyChanged();
        }
    }

    /// <summary>
    /// 显示的数据
    /// 用于增量显示，失败列表（全部）
    /// </summary>
    private IncrementalLoadFailDatumInfoCollection _allFailInfoCollection;
    public IncrementalLoadFailDatumInfoCollection AllFailInfoCollection
    {
        get => _allFailInfoCollection;
        set
        {
            if (_allFailInfoCollection == value)
                return;

            _allFailInfoCollection = value;

            OnPropertyChanged();
        }
    }


    /// <summary>
    /// 显示的数据
    /// 用于增量显示，失败列表（喜欢/稍后观看）
    /// </summary>
    private IncrementalLoadFailInfoCollection _likeOrLookLaterFailInfoCollection;
    public IncrementalLoadFailInfoCollection LikeOrLookLaterFailInfoCollection
    {
        get => _likeOrLookLaterFailInfoCollection;
        set
        {
            if (_likeOrLookLaterFailInfoCollection == value) return;

            _likeOrLookLaterFailInfoCollection = value;

            OnPropertyChanged();
        }
    }


    public void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        // Raise the PropertyChanged event, passing the Name of the property whose value has changed.
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// 图片的最小值
    /// 与Slider对应
    /// </summary>
    private const double SliderMinValue = 200;

    /// <summary>
    /// 图片的最大值
    /// 与Slider对应
    /// </summary>
    private const double SliderMaxValue = 900;

    private bool _isFuzzyQueryActor = true;

    private List<string> _filterConditionList;
    private Dictionary<string, string> _filterRanges;
    private string _filterKeywords;

    public VideoCoverDisplay()
    {
        InitializeComponent();

        Loaded += PageLoaded;
    }

    private void PageLoaded(object sender, RoutedEventArgs e)
    {
        //展示的是搜索结果
        if (IsShowSearchListView)
        {
        }
        else
        {
            //展示的是失败列表
            ShowType_RadioButtons.SelectedIndex = IsShowFailListView ? 1 : 0;
        }

        Loaded -= PageLoaded;
    }

    /// <summary>
    /// 加载搜索结果
    /// </summary>
    public async void ReLoadSearchResult(List<string> types, string ShowName, bool isFuzzyQueryActor)
    {
        bool isShowHeaderCover = false;

        _isFuzzyQueryActor = isFuzzyQueryActor;

        if (types.Count == 1)
        {
            switch (types.FirstOrDefault())
            {
                case "is_like":
                    Title = "喜欢";
                    break;
                case "look_later":
                    Title = "稍后观看";
                    break;
                case "fail":
                    Title = ShowName;
                    localCheckText = ShowName;
                    IsShowFailListView = true;
                    FailInfoSuggestBox.Visibility = Visibility.Collapsed;
                    trySwitchToFailView();
                    return;
                case "actor":
                    Title = ShowName;

                    // 准确查询演员，一般来源于详情页和演员页
                    if (!isFuzzyQueryActor)
                    {
                        var actorInfos = await DataAccess.Get.GetActorInfo(filterList: new() { $"Name == '{ShowName}'" });

                        if (actorInfos.Length != 0)
                        {
                            ActorInfo = actorInfos.FirstOrDefault();

                            isShowHeaderCover = true;
                        }
                    }

                    break;
                default:
                    Title = ShowName;
                    break;
            }


        }
        else
        {
            Title = ShowName;
        }

        this.IsShowHeaderCover = isShowHeaderCover;

        _filterConditionList = types;
        _filterKeywords = ShowName;
        IsShowFailListView = false;
        trySwitchToSuccessView();
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
    /// <param Name="sender"></param>
    /// <param Name="e"></param>
    private void Slider_valueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
        _markSliderValue = e.NewValue;

        //动态调整图片大小
        if (IsAutoAdjustImageSize_ToggleButton.IsChecked == true)
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
        //失败列表不调整
        if (IsShowFailListView) return;

        if (gridWidth == -1)
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

        var height = width / 3 * 2;

        foreach (var t in SuccessInfoCollection)
        {
            t.ImageWidth = width;
            t.imageheight = height;
        }

        //更改应用设置
        ImageSize = new Tuple<double, double>(width, height);

        //当前匹配的是成功
        //更新获取图片大小的值
        if (IsShowSuccessListView)
        {
            SuccessInfoCollection.SetImageSize(width, height);
        }
    }


    private Tuple<double, double> _imageSize;
    private Tuple<double, double> ImageSize
    {
        get
        {
            _imageSize ??= new Tuple<double, double>(AppSettings.ImageWidth, AppSettings.ImageHeight);

            return _imageSize;
        }
        set
        {
            var imageWidth = value.Item1;
            var imageHeight = value.Item2;

            _imageSize = new Tuple<double, double>(imageWidth, imageHeight);

            AppSettings.ImageWidth = imageWidth;
            AppSettings.ImageHeight = imageHeight;
        }
    }

    public event RoutedEventHandler Click;
    private void MoreButton_Click(object sender, RoutedEventArgs e)
    {
        Click?.Invoke(sender, e);
    }

    /// <summary>
    /// 鼠标悬停在Grid，显示可操作按钮
    /// </summary>
    /// <param Name="sender"></param>
    /// <param Name="e"></param>
    private void Grid_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        if (!(sender is Grid grid)) return;
        grid.Children[1].Visibility = Visibility.Visible;
    }

    /// <summary>
    /// 鼠标移出在Grid，隐藏可操作按钮
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Grid_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        if (!(sender is Grid grid)) return;

        if (grid.Children[1] is not Grid collapsedGrid) return;

        collapsedGrid.Visibility = Visibility.Collapsed;

    }

    private void FailImageGrid_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        if (sender is not Grid grid) return;
        grid.Children[1].Visibility = Visibility.Visible;

        ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Hand);

        grid.Tapped += FailGrid_Tapped;
    }

    private void FailImageGrid_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        if (!(sender is Grid grid)) return;

        ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Arrow);

        if (grid.Children[1] is not Grid CollapsedGrid) return;
        CollapsedGrid.Visibility = Visibility.Collapsed;

        grid.Tapped -= FailGrid_Tapped;
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
    /// <param Name="sender"></param>
    /// <param Name="e"></param>
    private void LikeToggleButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not AppBarToggleButton Button) return;

        if (Button.DataContext is not VideoCoverDisplayClass videoInfo) return;

        videoInfo.is_like = (bool)Button.IsChecked ? 1 : 0;

        DataAccess.Update.UpdateSingleDataFromVideoInfo(videoInfo.truename, "is_like", videoInfo.is_like.ToString());
    }

    private void FailLikeToggleButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not AppBarToggleButton Button) return;

        if (Button.DataContext is not FailInfo info) return;

        info.IsLike = (bool)Button.IsChecked ? 1 : 0;

        DataAccess.Update.UpdateSingleFailInfo(info.PickCode, "is_like", info.IsLike.ToString());
    }

    /// <summary>
    /// 点击了稍后观看
    /// </summary>
    /// <param Name="sender"></param>
    /// <param Name="e"></param>
    private void LookLaterToggleButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not AppBarToggleButton Button) return;

        if (Button.DataContext is not VideoCoverDisplayClass videoInfo) return;

        videoInfo.look_later = (bool)Button.IsChecked ? DateTimeOffset.Now.ToUnixTimeSeconds() : 0;

        DataAccess.Update.UpdateSingleDataFromVideoInfo(videoInfo.truename, "look_later", videoInfo.look_later.ToString());
    }

    private void FailLookLaterToggleButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not AppBarToggleButton Button) return;

        if (Button.DataContext is not FailInfo info) return;

        info.LookLater = (bool)Button.IsChecked ? DateTimeOffset.Now.ToUnixTimeSeconds() : 0;

        DataAccess.Update.UpdateSingleFailInfo(info.PickCode, "look_later", info.LookLater.ToString());
    }

    /// <summary>
    /// 修改评分
    /// </summary>
    /// <param Name="sender"></param>
    /// <param Name="args"></param>
    private void RatingControl_ValueChanged(RatingControl sender, object args)
    {
        if (sender.DataContext is not VideoCoverDisplayClass videoInfo) return;

        string score_str = videoInfo.score == 0 ? "-1" : sender.Value.ToString();

        DataAccess.Update.UpdateSingleDataFromVideoInfo(videoInfo.truename, "score", score_str);

    }

    private void FailRatingControl_ValueChanged(RatingControl sender, object args)
    {
        if (sender.DataContext is not FailInfo Info) return;

        string score_str = Info.Score == 0 ? "-1" : sender.Value.ToString();

        DataAccess.Update.UpdateSingleFailInfo(Info.PickCode, "score", score_str);
    }

    public event RoutedEventHandler SingleVideoPlayClick;

    private void FailGrid_Tapped(object sender, TappedRoutedEventArgs e)
    {
        SingleVideoPlayClick?.Invoke(sender, e);
    }

    /// <summary>
    /// 点击播放键
    /// </summary>
    public event RoutedEventHandler VideoPlayClick;
    public event PropertyChangedEventHandler PropertyChanged;

    private void PlayButton_Click(object sender, RoutedEventArgs e)
    {
        VideoPlayClick?.Invoke(sender, e);
    }


    private string SuccessListOrderBy;
    private bool SuccessListIsDesc;
    /// <summary>
    /// 按类型排序（用于成功列表）
    /// </summary>
    /// <param Name="sender"></param>
    /// <param Name="e"></param>
    private void OrderSuccessListView_ItemClick(object sender, ItemClickEventArgs e)
    {
        ListView selectListView = (ListView)sender;
        var clickStackPanel = (e.ClickedItem as StackPanel);
        var selectTextBlock = clickStackPanel.Children.Where(x => x is TextBlock).First() as TextBlock;
        string selectOrderText = selectTextBlock.Text;

        FontIcon lastFontIcon = clickStackPanel.Children.Where(x => x is FontIcon).Last() as FontIcon;

        string upGlyph = "\xE014";
        string downGlyph = "\xE015";
        string newGlyph;

        //原图标
        bool isUpSort = lastFontIcon.Glyph == upGlyph;

        //更新降序或升序图标
        //注意：随机 无需升/降序
        if (selectListView.SelectedItem == e.ClickedItem && selectOrderText != "随机")
        {
            lastFontIcon.Glyph = isUpSort ? downGlyph : upGlyph;
        }

        //现图标
        isUpSort = lastFontIcon.Glyph == upGlyph;
        SuccessListIsDesc = !isUpSort;

        switch (selectOrderText)
        {
            case "名称":
                newGlyph = "\xE185";
                SuccessListOrderBy = "truename";
                //FileGrid = isUpSort ? new List<VideoCoverDisplayClass>(FileGrid.OrderBy(item => item.truename)) : new List<VideoCoverDisplayClass>(FileGrid.OrderByDescending(item => item.truename));

                break;
            case "演员":
                newGlyph = "\xE13D";
                SuccessListOrderBy = "actor";
                //FileGrid = isUpSort ? new List<VideoCoverDisplayClass>(FileGrid.OrderBy(item => item.actor)) : new List<VideoCoverDisplayClass>(FileGrid.OrderByDescending(item => item.actor));

                break;
            case "年份":
                newGlyph = "\xEC92";
                SuccessListOrderBy = "releasetime";
                //FileGrid = isUpSort ? new List<VideoCoverDisplayClass>(FileGrid.OrderBy(item => item.realeaseYear)) : new List<VideoCoverDisplayClass>(FileGrid.OrderByDescending(item => item.realeaseYear));

                break;
            case "随机":
                newGlyph = "\xF463";
                SuccessListOrderBy = "random";
                //Random rnd = new Random();
                //FileGrid = new List<VideoCoverDisplayClass>(FileGrid.OrderByDescending(item => rnd.Next()));
                break;
            default:
                newGlyph = "\xE185";
                SuccessListOrderBy = "truename";
                break;
        }

        //更新首图标
        FontIcon orderFontIcon = OrderButton.Content as FontIcon;
        if (orderFontIcon.Glyph != newGlyph)
        {
            OrderButton.Content = new FontIcon() { FontFamily = new FontFamily("Segoe Fluent Icons"), Glyph = newGlyph };
        }

        LoadDstSuccessInfoCollection();
    }


    private string FailListOrderBy;
    private bool FailListIsDesc;

    /// <summary>
    /// 按类型排序（用于失败列表）
    /// </summary>
    /// <param Name="sender"></param>
    /// <param Name="e"></param>
    private async void OrderFailListView_ItemClick(object sender, ItemClickEventArgs e)
    {
        ListView selectListView = (ListView)sender;
        var clickStackPanel = (e.ClickedItem as StackPanel);
        var selectTextBlock = clickStackPanel.Children.First(x => x is TextBlock) as TextBlock;
        string selectOrderText = selectTextBlock.Text;
        FontIcon lastFontIcon = clickStackPanel.Children.Last(x => x is FontIcon) as FontIcon;

        string upGlyph = "\xE014";
        string downGlyph = "\xE015";
        string newGlyph;

        //原图标是否是升序
        bool isUpSort = lastFontIcon.Glyph == upGlyph;

        //更新降序或升序图标
        //注意：随机 无需升/降序
        if (selectListView.SelectedItem == e.ClickedItem && selectOrderText != "随机")
        {
            lastFontIcon.Glyph = isUpSort ? downGlyph : upGlyph;
        }

        //现在是否是升序
        isUpSort = lastFontIcon.Glyph == upGlyph;
        FailListIsDesc = !isUpSort;

        switch (selectOrderText)
        {
            case "名称":
                newGlyph = "\xE185";
                FailListOrderBy = "n";
                break;
            case "大小":
                newGlyph = "\xEB05";
                FailListOrderBy = "s";
                break;
            case "时间":
                newGlyph = "\xE2AD";
                FailListOrderBy = "tp";
                break;
            case "随机":
                newGlyph = "\xF463";
                FailListOrderBy = "random";
                break;
            default:
                newGlyph = "\xE185";
                FailListOrderBy = "n";
                break;
        }
        //更新首图标
        FontIcon orderFontIcon = OrderButton.Content as FontIcon;
        if (orderFontIcon.Glyph != newGlyph)
        {
            OrderButton.Content = new FontIcon() { FontFamily = new FontFamily("Segoe Fluent Icons"), Glyph = newGlyph };
        }

        //更新数据
        AllFailInfoCollection.Clear();

        //考虑当前是不是搜索过的
        if (!string.IsNullOrEmpty(localCheckText))
            AllFailInfoCollection.filterName = localCheckText;

        AllFailInfoCollection.OrderBy = FailListOrderBy;
        AllFailInfoCollection.IsDesc = FailListIsDesc;
        var lists = await DataAccess.Get.GetFailFileInfoWithDatum(0, 30, localCheckText, orderBy: FailListOrderBy, isDesc: FailListIsDesc);
        lists.ForEach(AllFailInfoCollection.Add);


    }

    /// <summary>
    /// 点击了删除按钮
    /// </summary>
    /// <param Name="sender"></param>
    /// <param Name="e"></param>
    private async void deleteAppBarButton_Click(object sender, RoutedEventArgs e)
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

        if (result == ContentDialogResult.Primary)
        {
            if (sender is AppBarButton appBarButton)
            {
                var item = appBarButton.DataContext as VideoCoverDisplayClass;
                //从数据库中删除
                DataAccess.Delete.DeleteDataInVideoInfoTable(item.truename);

                //删除存储的文件夹
                string savePath = Path.Combine(AppSettings.ImageSavePath, item.truename);
                if (Directory.Exists(savePath))
                {
                    Directory.Delete(savePath, true);
                }

                //FileGrid.Remove(item);
                SuccessInfoCollection.Remove(item);
            }
        }
    }

    //开始动画
    public async void StartAnimation(ConnectedAnimation animation, VideoCoverDisplayClass item)
    {
        if (BasicGridView.Items.Contains(item))
        {
            //开始动画
            await BasicGridView.TryStartConnectedAnimationAsync(animation, item, "showImage");
        }

    }

    public void PrepareAnimation(VideoCoverDisplayClass item)
    {
        BasicGridView.PrepareConnectedAnimation("ForwardConnectedAnimation", item, "showImage");
    }

    private void ShowType_ToggleSwitch_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        if (!(sender is ToggleSwitch toggleSwitch))
        {
            return;
        }

        toggleSwitch.Opacity = 1;
    }

    private void ShowType_ToggleSwitch_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        if (!(sender is ToggleSwitch toggleSwitch))
        {
            return;
        }

        toggleSwitch.Opacity = 0.3;
    }

    /// <summary>
    /// Toggle按钮改变时，切换匹配成功或失败列表
    /// </summary>
    /// <param Name="sender"></param>
    /// <param Name="e"></param>
    private void ShowType_ToggleSwitch_Toggled(object sender, RoutedEventArgs e)
    {
        var toggleswitch = sender as ToggleSwitch;

        //匹配失败
        if (toggleswitch.IsOn)
        {
            trySwitchToFailView();
        }
        //匹配成功
        else
        {
            trySwitchToSuccessView();
        }
    }


    private void ShowType_RadioButtons_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems[0] is not RadioButton button) return;


        //匹配成功
        if (button.Name == nameof(SuccessData_RadioButton))
        {
            IsShowFailListView = false;
            trySwitchToSuccessView();
        }
        //匹配失败
        else if (button.Name == nameof(FailData_RadioButton))
        {
            IsShowFailListView = true;
            trySwitchToFailView();
        }
    }

    /// <summary>
    /// 切换到失败视图
    /// </summary>
    private void trySwitchToFailView()
    {
        //更新GridView的来源（全部/喜欢/稍后观看）
        InitFailCollection();

        //停止监听调整图片大小的Slider
        CloseListeningSliderValueChanged();

        //停止监听动态调整图片大小
        CloseListeningGridSizeChanged();

        //更改排序的Flyout
        ChangedOrderButtonFlyout(false);

    }

    /// <summary>
    /// 切换到成功视图
    /// </summary>
    private async void trySwitchToSuccessView()
    {
        //更新GridView的来源
        var imgSize = ImageSize;

        SuccessInfoCollection = new(imgSize.Item1, imgSize.Item2);
        SuccessInfoCollection.SetFilter(_filterConditionList, _filterKeywords, _isFuzzyQueryActor);
        await SuccessInfoCollection.LoadData();

        BasicGridView.ItemsSource = SuccessInfoCollection;

        if (FailGridView.Visibility == Visibility.Visible) FailGridView.Visibility = Visibility.Collapsed;
        if (BasicGridView.Visibility == Visibility.Collapsed) BasicGridView.Visibility = Visibility.Visible;

        //初始化Slider的值
        _markSliderValue = imgSize.Item1;
        ImageSizeChangeSlider.Value = imgSize.Item1;

        //开始监听调整图片大小的Slider
        StartListeningSliderValueChanged();

        //是否需要动态调整图片大小
        tryStartListeningGridSizeChanged();

        if (OrderButton.Visibility == Visibility.Collapsed) OrderButton.Visibility = Visibility.Visible;

        //更改排序的Flyout
        ChangedOrderButtonFlyout(true);
    }

    private void FailGrid_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        if (sender is not Grid grid) return;

        ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Hand);

        grid.Scale = new System.Numerics.Vector3(1.01f);
    }

    private void FailGrid_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        if (sender is not Grid grid) return;

        ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Arrow);

        grid.Scale = new System.Numerics.Vector3(1.0f);
    }

    /// <summary>
    /// 记录失败列表搜索框里的数据值
    /// 翻遍失败列表的增量加载
    /// </summary>
    private string localCheckText;

    /// <summary>
    /// 失败列表输入框的值发生改变
    /// </summary>
    /// <param Name="sender"></param>
    /// <param Name="args"></param>
    private async void FileAutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        localCheckText = sender.Text;

        System.Diagnostics.Debug.WriteLine($"失败项的搜索框输入：{localCheckText}");
        AllFailInfoCollection = new IncrementalLoadFailDatumInfoCollection { filterName = localCheckText, OrderBy = FailListOrderBy, IsDesc = FailListIsDesc };

        FailGridView.ItemsSource = AllFailInfoCollection;
        await AllFailInfoCollection.LoadData();
    }

    /// <summary>
    /// 移至顶部
    /// </summary>
    /// <param Name="sender"></param>
    /// <param Name="e"></param>
    private void ToTopButton_Click(object sender, RoutedEventArgs e)
    {
        if (IsShowFailListView)
        {
            if (FailGridView.ItemsSource is IncrementalLoadFailDatumInfoCollection allFailCollection)
                FailGridView.ScrollIntoView(allFailCollection.First());
            else if (FailGridView.ItemsSource is IncrementalLoadFailInfoCollection failCollection)
                FailGridView.ScrollIntoView(failCollection.First());
        }
        else
        {
            if (BasicGridView.ItemsSource is IncrementalLoadSuccessInfoCollection successCollection)
                BasicGridView.ScrollIntoView(successCollection.First());
        }

    }

    /// <summary>
    /// 打开对Grid大小的监听，以动态调整图片大小
    /// </summary>
    public void tryStartListeningGridSizeChanged()
    {
        if (IsAutoAdjustImageSize_ToggleButton.IsChecked == true)
        {
            //监听前先调整
            AutoAdjustImageSize();

            //开始监听
            BasicGridView.SizeChanged += BasicGridView_SizeChanged;
        }
    }

    /// <summary>
    /// 开始监听调整图片大小的Slider
    /// </summary>
    public void StartListeningSliderValueChanged()
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
    /// 停止监听调整图片大小的Slider
    /// </summary>
    private void CloseListeningSliderValueChanged()
    {
        ImageSizeChangeSlider.ValueChanged -= Slider_valueChanged;
    }

    /// <summary>
    /// 图片模式下根据Grid调整大小
    /// </summary>
    /// <param Name="sender"></param>
    /// <param Name="e"></param>
    private void BasicGridView_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        double newGridWidth = e.NewSize.Width;

        AutoAdjustImageSize(newGridWidth, true);

    }

    /// <summary>
    /// 仅仅调整Slider的数值
    /// </summary>
    /// <param Name="newImageWidth"></param>
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
    /// <param Name="sender"></param>
    /// <param Name="e"></param>
    private void AutoAdjustImageSize_ToggleButton_Checked(object sender, RoutedEventArgs e)
    {
        //开始监听
        tryStartListeningGridSizeChanged();

        //初次调整大小
        AutoAdjustImageSize();
    }

    /// <summary>
    /// 关闭图片大小的动态调整
    /// </summary>
    /// <param Name="sender"></param>
    /// <param Name="e"></param>
    private void AutoAdjustImageSize_ToggleButton_UnChecked(object sender, RoutedEventArgs e)
    {
        CloseListeningGridSizeChanged();
        //System.Diagnostics.Debug.WriteLine("关闭图片大小的动态调整");
    }

    private void InfoListFilter_SplitButton_Click(SplitButton sender, SplitButtonClickEventArgs args)
    {
        if (!(sender is ToggleSplitButton button)) return;

        //选中
        if (button.IsChecked)
        {
            switch (sender.Tag)
            {
                case "Year":
                    if (_filterRanges == null)
                        _filterRanges = new();
                    _filterRanges["Year"] = InfosFilter.Year;
                    break;
                case "Score":
                    if (_filterRanges == null)
                        _filterRanges = new();
                    _filterRanges["Score"] = InfosFilter.Score.ToString();
                    break;
                case "Type":
                    if (_filterRanges == null)
                        _filterRanges = new();
                    _filterRanges["Type"] = InfosFilter.Type;
                    break;
            }
        }
        //取消选中
        else
        {
            _filterRanges.Remove(sender.Tag.ToString());
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
        SuccessInfoCollection = new IncrementalLoadSuccessInfoCollection(ImageSize.Item1, ImageSize.Item2);
        BasicGridView.ItemsSource = SuccessInfoCollection;

        SuccessInfoCollection.SetOrder(SuccessListOrderBy, SuccessListIsDesc);
        SuccessInfoCollection.SetRange(_filterRanges);
        SuccessInfoCollection.SetFilter(_filterConditionList, _filterKeywords, _isFuzzyQueryActor);
        await SuccessInfoCollection.LoadData();
    }

    private void Filter_ToggleButton_Unchecked(object sender, RoutedEventArgs e)
    {
        if (_filterRanges is not {Count:>0}) return;

        _filterRanges = null;
        InfosFilter.UncheckAllToggleSplitButton();
        LoadDstSuccessInfoCollection();
    }

    private void LikeButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not ToggleButton isLikeButton) return;

        if (isLikeButton.Tag is not int actorId) return;

        if (isLikeButton.IsChecked == null) return;

        var isLike = (bool)isLikeButton.IsChecked ? 1 : 0;

        DataAccess.Update.UpdateSingleDataFromActorInfo(actorId.ToString(), "is_like", isLike.ToString());
    }

    private void ChangedHyperlink()
    {
        if (!string.IsNullOrEmpty(ActorInfo.BlogUrl))
        {
            blog_HyperLink.NavigateUri = new Uri(ActorInfo.BlogUrl);
        }

        if (!string.IsNullOrEmpty(ActorInfo.InfoUrl))
        {
            info_HyperLink.NavigateUri = new Uri(ActorInfo.InfoUrl);
        }
    }

    private void ShowData_RadioButtons_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems[0] is RadioButton)
        {
            //IsShowFailListView = true;
            InitFailCollection();
        }
    }

    private async void InitFailCollection()
    {
        if (FailShowChanged_RadioButtons.SelectedItem is not RadioButton radioButton) return;


        bool isShowAllFail;

        //更新GridView的来源
        switch (radioButton.Name)
        {
            //喜欢
            case nameof(FailLike_RadioButton):
                if (LikeOrLookLaterFailInfoCollection == null)
                {
                    LikeOrLookLaterFailInfoCollection = new IncrementalLoadFailInfoCollection(FailInfoShowType.like);
                }
                else if (LikeOrLookLaterFailInfoCollection.ShowType != FailInfoShowType.like)
                {
                    LikeOrLookLaterFailInfoCollection.SetShowType(FailInfoShowType.like);
                }
                FailGridView.ItemsSource = LikeOrLookLaterFailInfoCollection;

                isShowAllFail = false;

                break;
            //稍后观看
            case nameof(FailLookLater_RadioButton):
                if (LikeOrLookLaterFailInfoCollection == null)
                {
                    LikeOrLookLaterFailInfoCollection = new(FailInfoShowType.look_later);
                }
                else if (LikeOrLookLaterFailInfoCollection.ShowType != FailInfoShowType.look_later)
                {
                    LikeOrLookLaterFailInfoCollection.SetShowType(FailInfoShowType.look_later);
                }
                FailGridView.ItemsSource = LikeOrLookLaterFailInfoCollection;

                isShowAllFail = false;

                break;
            //默认全部
            default:
                if (AllFailInfoCollection == null || !string.IsNullOrEmpty(localCheckText))
                {
                    AllFailInfoCollection = new();
                    AllFailInfoCollection.SetOrder(FailListOrderBy, FailListIsDesc);
                    AllFailInfoCollection.SetFilter(localCheckText);
                    await AllFailInfoCollection.LoadData();
                }
                FailGridView.ItemsSource = AllFailInfoCollection;

                isShowAllFail = true;

                break;
        }

        if (isShowAllFail)
        {
            //排列按钮
            if (OrderButton.Visibility == Visibility.Collapsed) OrderButton.Visibility = Visibility.Visible;

            //显示当前数量
            if (AllFailShowCountControl.Visibility == Visibility.Collapsed) AllFailShowCountControl.Visibility = Visibility.Visible;
            if (LikeOrLookLaterFailShowCountControl.Visibility == Visibility.Visible) LikeOrLookLaterFailShowCountControl.Visibility = Visibility.Collapsed;

            //搜索框
            if (FailInfoSuggestBox.Visibility == Visibility.Collapsed) FailInfoSuggestBox.Visibility = Visibility.Visible;

        }
        else
        {
            //排列按钮
            if (OrderButton.Visibility == Visibility.Visible) OrderButton.Visibility = Visibility.Collapsed;

            //显示当前数量
            if (LikeOrLookLaterFailShowCountControl.Visibility == Visibility.Collapsed) LikeOrLookLaterFailShowCountControl.Visibility = Visibility.Visible;
            if (AllFailShowCountControl.Visibility == Visibility.Visible) AllFailShowCountControl.Visibility = Visibility.Collapsed;

            //搜索框
            if (FailInfoSuggestBox.Visibility == Visibility.Visible) FailInfoSuggestBox.Visibility = Visibility.Collapsed;
        }
    }

    private async void FailItemAddToLikeButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuFlyoutItem item) return;

        if (item.DataContext is not Datum datum) return;

        var pickCode = datum.PickCode;

        var failInfo = DataAccess.Get.GetSingleFailInfoByPickCode(pickCode);

        if (failInfo == null)
        {
            DataAccess.Add.AddOrReplaceFailList_IsLike_LookLater(new FailInfo
            {
                PickCode = pickCode,
                IsLike = 1
            });
            ShowTeachingTip("已添加进喜欢");
        }
        //已添加进数据库但不是喜欢
        //标记为喜欢
        else
        {
            switch (failInfo.IsLike)
            {
                case 0:
                    DataAccess.Update.UpdateSingleFailInfo(pickCode, "is_like", "1");
                    ShowTeachingTip("已添加进喜欢");
                    break;
                default:
                    ShowTeachingTip("已存在于喜欢，忽略该操作");
                    break;
            }
        }
    }

    private async void FailItemAddToLookLaterButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuFlyoutItem item) return;

        if (item.DataContext is not Datum datum) return;

        var pickCode = datum.PickCode;

        var failInfo = DataAccess.Get.GetSingleFailInfoByPickCode(pickCode);

        if (failInfo == null)
        {
            DataAccess.Add.AddOrReplaceFailList_IsLike_LookLater(new()
            {
                PickCode = pickCode,
                LookLater = 1
            });
            ShowTeachingTip("已添加进稍后观看");
        }
        //已添加进数据库但不是稍后观看
        //标记为稍后观看
        else
        {

            switch (failInfo.LookLater)
            {
                case 0:
                    DataAccess.Update.UpdateSingleFailInfo(pickCode, "look_later", "1");
                    ShowTeachingTip("已添加进稍后观看");
                    break;
                default:
                    ShowTeachingTip("已存在于稍后观看，忽略该操作");
                    break;
            }
        }
    }

    private async void ShowTeachingTip(string subTitle)
    {
        //之前的通知存在，先隐藏
        if (FailList_TeachingTip.IsOpen) FailList_TeachingTip.IsOpen = false;

        FailList_TeachingTip.Subtitle = subTitle;
        FailList_TeachingTip.IsOpen = true;

        await Task.Delay(1000);

        if (FailList_TeachingTip.IsOpen) FailList_TeachingTip.IsOpen = false;
    }

}

/// <summary>
/// GridView样式选择
/// </summary>
public class CoverItemTemplateSelector : DataTemplateSelector
{
    public DataTemplate ImageTemplate { get; set; }
    public DataTemplate FailCoverTemplate { get; set; }
    public DataTemplate LikeOrLookLaterInFailCoverTemplate { get; set; }
    public DataTemplate WithoutImageTemplate { get; set; }

    protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
    {
        if (item is VideoCoverDisplayClass info)
        {
            if (info.series == "fail")
            {
                return FailCoverTemplate;
            }
            else
            {
                return ImageTemplate;
            }
        }
        else if (item is FailInfo)
        {
            return LikeOrLookLaterInFailCoverTemplate;
        }
        else
        {
            return WithoutImageTemplate;
        }
    }
}