using Data;
using Display.Model;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Documents;
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
using Windows.System;

namespace Display.Control;

public sealed partial class VideoCoverDisplay : UserControl, INotifyPropertyChanged
{
    //标题
    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register("Title", typeof(string), typeof(VideoCoverDisplay), null);

    //显示的是匹配成功的还是失败的
    public static readonly DependencyProperty IsShowFailListViewProperty =
        DependencyProperty.Register("IsShowFailListView", typeof(bool), typeof(VideoCoverDisplay), null);

    public static readonly DependencyProperty IsShowSearchListViewProperty =
        DependencyProperty.Register("IsShowSearchListView", typeof(bool), typeof(VideoCoverDisplay), PropertyMetadata.Create(() => false));

    private bool IsShowSucAndFailSwitchButton
    {
        get => !IsShowSearchListView;
    }

    private ActorInfo _actorInfo;
    public ActorInfo actorInfo
    {
        get=> _actorInfo;
        set
        {
            if(_actorInfo == value) return;

            _actorInfo = value;

            ChangedHyperlink();

            OnPropertyChanged();
        }
    }

    private bool _isShowHeaderCover = false;
    public bool isShowHeaderCover
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
        get { return (string)GetValue(TitleProperty); }
        set { SetValue(TitleProperty, value); }
    }

    /// <summary>
    /// 是否显示成功列表
    /// </summary>
    private bool IsShowSuccessListView
    {
        get => !IsShowFailListView;
    }

    /// <summary>
    /// 当前显示失败列表
    /// </summary>
    public bool IsShowFailListView
    {
        get { return (bool)GetValue(IsShowFailListViewProperty); }
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
        get { return (bool)GetValue(IsShowSearchListViewProperty); }
        set { SetValue(IsShowSearchListViewProperty, value); }
    }

    /// <summary>
    /// 切换排序Flyout(成功或失败)
    /// </summary>
    /// <param Name="IsShowSuccessFlyout"></param>
    private void ChangedOrderButtonFlyout(bool IsShowSuccessFlyout)
    {
        //显示成功的排序
        if (IsShowSuccessFlyout)
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
    private Model.IncrementalLoadSuccessInfoCollection _successInfocollection;
    public Model.IncrementalLoadSuccessInfoCollection SuccessInfoCollection
    {
        get => _successInfocollection;
        set
        {
            _successInfocollection = value;

            OnPropertyChanged();
        }
    }

    /// <summary>
    /// 显示的数据
    /// 用于增量显示，失败列表（全部）
    /// </summary>
    private Model.IncrementalLoadFailDatumInfoCollection _allfailInfocollection;
    public Model.IncrementalLoadFailDatumInfoCollection AllFailInfoCollection
    {
        get => _allfailInfocollection;
        set
        {
            if (_allfailInfocollection == value)
                return;

            _allfailInfocollection = value;

            OnPropertyChanged();
        }
    }


    /// <summary>
    /// 显示的数据
    /// 用于增量显示，失败列表（喜欢/稍后观看）
    /// </summary>
    private IncrementallLoadFailInfoCollection _likeOrLookLater_failInfocollection;
    public IncrementallLoadFailInfoCollection LikeOrLookLater_failInfocollection
    {
        get => _likeOrLookLater_failInfocollection;
        set
        {
            if (_likeOrLookLater_failInfocollection == value)
                return;

            _likeOrLookLater_failInfocollection = value;

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
    private double SliderMinValue = 200;

    /// <summary>
    /// 图片的最大值
    /// 与Slider对应
    /// </summary>
    private double SliderMaxValue = 900;

    private bool isFuzzyQueryActor = true;

    List<string> filterConditionList;
    Dictionary<string,string> filterRanges;
    string filterKeywords;

    public VideoCoverDisplay()
    {
        this.InitializeComponent();

        this.Loaded += PageLoaded;
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
            if (IsShowFailListView)
            {
                ShowType_RadioButtons.SelectedIndex = 1;
            }
            else
            {
                ShowType_RadioButtons.SelectedIndex = 0;
            }
        }

        this.Loaded -= PageLoaded;
    }

    /// <summary>
    /// 加载搜索结果
    /// </summary>
    public async void ReLoadSearchResult(List<string> types, string ShowName, bool isFuzzyQueryActor)
    {
        bool isShowHeaderCover = false;

        this.isFuzzyQueryActor = isFuzzyQueryActor;

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
                        var actorinfos = await DataAccess.LoadActorInfo(filterList: new() { $"Name == '{ShowName}'" });

                        if (actorinfos.Count != 0)
                        {
                            actorInfo = actorinfos.FirstOrDefault();

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

        this.isShowHeaderCover = isShowHeaderCover;

        filterConditionList = types;
        filterKeywords = ShowName;
        IsShowFailListView = false;
        trySwitchToSuccessView();
    }


    /// <summary>
    /// 用于动态调整图片大小的值
    /// 经验（(padding + 4 或 5)*2）
    /// ( 4 + 5 )*2
    /// </summary>
    double HorizontalPadding = 18;

    /// <summary>
    /// 标记Slider的值
    /// </summary>
    double markSliderValue;

    /// <summary>
    /// Slider值改变后，调整图片大小
    /// </summary>
    /// <param Name="sender"></param>
    /// <param Name="e"></param>
    private void Slider_valueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
        markSliderValue = e.NewValue;

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
    /// <param Name="GridWidth"></param>
    /// <param Name="AdjustSliderValue"></param>
    private void AutoAdjustImageSize(double GridWidth = -1, bool AdjustSliderValue = false)
    {
        //失败列表不调整
        if (IsShowFailListView) return;

        if (GridWidth == -1)
            GridWidth = BasicGridView.ActualWidth;

        var ImageCountPerRow = Math.Floor(GridWidth / (markSliderValue + HorizontalPadding));
        if (ImageCountPerRow <= 0) ImageCountPerRow = 1;
        //System.Diagnostics.Debug.WriteLine($"每行图片数量：{ImageCountPerRow}");

        double newImageWidth = GridWidth / ImageCountPerRow - HorizontalPadding;
        //System.Diagnostics.Debug.WriteLine($"推算出的图片宽度应为：{newImageWidth}");

        //必须要在一定范围内（Slider的最大最小值）
        if (SliderMinValue <= newImageWidth && newImageWidth <= SliderMaxValue)
        {
            // SliderValue的0.5~1.5倍
            // 又或者，每行图片最大为1的话，可以缩小到最小值
            if ((markSliderValue * 0.5 <= newImageWidth && newImageWidth <= markSliderValue * 1.5)||ImageCountPerRow == 1)
            {
                AdjustImageSize(newImageWidth);

                if (AdjustSliderValue) AdjustSliderValueOnly(newImageWidth);

            }
        }
    }

    /// <summary>
    /// 固定值调整图片大小
    /// </summary>
    /// <param Name="width"></param>
    private void AdjustImageSize(double width)
    {
        if (SuccessInfoCollection == null) return;

        var height = width / 3 * 2;

        for (int i = 0; i < SuccessInfoCollection.Count; i++)
        {
            SuccessInfoCollection[i].imagewidth = width;
            SuccessInfoCollection[i].imageheight = height;
        }

        //更改应用设置
        ImageSize = new(width, height);

        //当前匹配的是成功
        //更新获取图片大小的值
        if (IsShowSuccessListView)
        {
            SuccessInfoCollection.SetImageSize(width, height);
        }
    }

    double _imagewidth;
    double _imageheight;
    private Tuple<double, double> ImageSize
    {
        get
        {
            if (_imagewidth == 0 || _imageheight == 0)
            {
                var imageSize = Data.AppSettings.ImageSize;

                _imagewidth = imageSize.Item1;

                _imageheight = imageSize.Item2;
            }

            return new(_imagewidth, _imageheight);
        }
        set
        {
            this._imagewidth = value.Item1;
            this._imageheight = value.Item2;
            Data.AppSettings.ImageSize = value;
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
    /// <param Name="sender"></param>
    /// <param Name="e"></param>
    private void Grid_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        if (!(sender is Grid grid)) return;

        if (grid.Children[1] is not Grid CollapsedGrid) return;

        CollapsedGrid.Visibility = Visibility.Collapsed;

    }
    
    private void FailImageGrid_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        if (!(sender is Grid grid)) return;
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

        DataAccess.UpdateSingleDataFromVideoInfo(videoInfo.truename, "is_like", videoInfo.is_like.ToString());
    }

    private void FailLikeToggleButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not AppBarToggleButton Button) return;

        if (Button.DataContext is not FailInfo info) return;

        info.is_like = (bool)Button.IsChecked ? 1 : 0;

        DataAccess.UpdateSingleFailInfo(info.pc, "is_like", info.is_like.ToString());
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

        DataAccess.UpdateSingleDataFromVideoInfo(videoInfo.truename, "look_later", videoInfo.look_later.ToString());
    }

    private void FailLookLaterToggleButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not AppBarToggleButton Button) return;

        if (Button.DataContext is not FailInfo info) return;

        info.look_later = (bool)Button.IsChecked ? DateTimeOffset.Now.ToUnixTimeSeconds() : 0;

        DataAccess.UpdateSingleFailInfo(info.pc, "look_later", info.look_later.ToString());
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

        DataAccess.UpdateSingleDataFromVideoInfo(videoInfo.truename, "score", score_str);

    }

    private void FailRatingControl_ValueChanged(RatingControl sender, object args)
    {
        if (sender.DataContext is not FailInfo Info) return;

        string score_str = Info.score == 0 ? "-1" : sender.Value.ToString();

        DataAccess.UpdateSingleFailInfo(Info.pc, "score", score_str);
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
        var selectTextBlock = clickStackPanel.Children.Where(x => x is TextBlock).First() as TextBlock;
        string selectOrderText = selectTextBlock.Text;
        FontIcon lastFontIcon = clickStackPanel.Children.Where(x => x is FontIcon).Last() as FontIcon;

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
        List<Datum> lists = await DataAccess.LoadFailFileInfoWithDatum(0, 30, localCheckText, orderBy: FailListOrderBy, isDesc: FailListIsDesc);
        lists.ForEach(datum => AllFailInfoCollection.Add(datum));

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
            XamlRoot = this.XamlRoot,
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
                DataAccess.DeleteDataInVideoInfoTable(item.truename);

                //删除存储的文件夹
                string savePath = Path.Combine(AppSettings.Image_SavePath, item.truename);
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
        SuccessInfoCollection.SetFilter(filterConditionList, filterKeywords, isFuzzyQueryActor);
        await SuccessInfoCollection.LoadData();

        BasicGridView.ItemsSource = SuccessInfoCollection;

        if (FailGridView.Visibility == Visibility.Visible) FailGridView.Visibility = Visibility.Collapsed;
        if (BasicGridView.Visibility == Visibility.Collapsed) BasicGridView.Visibility = Visibility.Visible;

        //初始化Slider的值
        markSliderValue = imgSize.Item1;
        ImageSizeChangeSlider.Value = imgSize.Item1;

        //开始监听调整图片大小的Slider
        StartListeningSliderValueChanged();

        //是否需要动态调整图片大小
        tryStartListeningGridSizeChanged();

        if(OrderButton.Visibility == Visibility.Collapsed) OrderButton.Visibility = Visibility.Visible;

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
        if (!(sender is AutoSuggestBox suggestBox))
            return;

        localCheckText = suggestBox.Text;

        AllFailInfoCollection = new() { filterName = localCheckText, OrderBy = FailListOrderBy, IsDesc = FailListIsDesc };

        await AllFailInfoCollection.LoadData();
        BasicGridView.ItemsSource = AllFailInfoCollection;

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
            else if (FailGridView.ItemsSource is IncrementallLoadFailInfoCollection failCollection)
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
                    if (filterRanges == null)
                        filterRanges = new();
                    filterRanges["Year"]= InfosFilter.Year;
                    break;
                case "Score":
                    if (filterRanges == null)
                        filterRanges = new();
                    filterRanges["Score"]= InfosFilter.Score.ToString();
                    break;
                case "Type":
                    if (filterRanges == null)
                        filterRanges = new();
                    filterRanges["Type"] = InfosFilter.Type;
                    break;
            }
        }
        //取消选中
        else
        {
            filterRanges.Remove(sender.Tag.ToString());
        }

        LoadDstSuccessInfoCollection();
    }   

    private void InfoListFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!(sender is RadioButtons radioButtons)) return;
        if (!(radioButtons.SelectedItem is RadioButton radioButton)) return;

        var Key = radioButtons.Tag.ToString();
        var Value = radioButton.Tag != null ? radioButton.Tag.ToString() : radioButton.Content.ToString();
        if (filterRanges == null)
            filterRanges = new();

        filterRanges[Key] =Value;

        LoadDstSuccessInfoCollection();
    }

    private void InfoListFilter_TextChanged(object sender, TextChangedEventArgs e)
    {
        var Key = "Year";
        var Value = InfosFilter.Year;
        if (filterRanges == null)
            filterRanges = new();

        filterRanges[Key] = Value;
        LoadDstSuccessInfoCollection();
    }

    private async void LoadDstSuccessInfoCollection()
    {
        var imgSize = this.ImageSize;
        SuccessInfoCollection = new(imgSize.Item1, imgSize.Item2);
        BasicGridView.ItemsSource = SuccessInfoCollection;

        SuccessInfoCollection.SetOrder(SuccessListOrderBy, SuccessListIsDesc);
        SuccessInfoCollection.SetRange(filterRanges);
        SuccessInfoCollection.SetFilter(filterConditionList, filterKeywords, isFuzzyQueryActor);
        await SuccessInfoCollection.LoadData();
    }

    private void Filter_ToggleButton_Unchecked(object sender, RoutedEventArgs e)
    {
        if (filterRanges == null) return;
        else if (filterRanges.Count == 0) return;
        else
            filterRanges = null;

        InfosFilter.UncheckAllToggleSplitButton();
        LoadDstSuccessInfoCollection();
    }

    private void LikeButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not ToggleButton isLikeButton) return;

        if (isLikeButton.Tag is not int actorId) return;

        var is_like = (bool)isLikeButton.IsChecked ? 1 : 0;

        DataAccess.UpdateSingleDataFromActorInfo(actorId.ToString(), "is_like", is_like.ToString());
    }

    private void ChangedHyperlink()
    {
        if (!string.IsNullOrEmpty(actorInfo.blog_url))
        {
            blog_HyperLink.NavigateUri = new Uri(actorInfo.blog_url);
        }

        if (!string.IsNullOrEmpty(actorInfo.info_url))
        {
            info_HyperLink.NavigateUri = new Uri(actorInfo.info_url);
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
                if (LikeOrLookLater_failInfocollection == null)
                {
                    LikeOrLookLater_failInfocollection = new(FailInfoShowType.like);
                }
                else if(LikeOrLookLater_failInfocollection.ShowType != FailInfoShowType.like)
                {
                    LikeOrLookLater_failInfocollection.SetShowType(FailInfoShowType.like);
                }
                FailGridView.ItemsSource = LikeOrLookLater_failInfocollection;

                isShowAllFail = false;

                break;
            //稍后观看
            case nameof(FailLookLater_RadioButton):
                if (LikeOrLookLater_failInfocollection == null)
                {
                    LikeOrLookLater_failInfocollection = new(FailInfoShowType.look_later);
                }
                else if (LikeOrLookLater_failInfocollection.ShowType != FailInfoShowType.look_later)
                {
                    LikeOrLookLater_failInfocollection.SetShowType(FailInfoShowType.look_later);
                }
                FailGridView.ItemsSource = LikeOrLookLater_failInfocollection;

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

        //if (BasicGridView.Visibility == Visibility.Visible) BasicGridView.Visibility = Visibility.Collapsed;
        //if (FailGridView.Visibility == Visibility.Collapsed) FailGridView.Visibility = Visibility.Visible;

        if (isShowAllFail)
        {
            //排列按钮
            if(OrderButton.Visibility == Visibility.Collapsed) OrderButton.Visibility = Visibility.Visible;

            //显示当前数量
            if (AllFailShowCountControl.Visibility == Visibility.Collapsed) AllFailShowCountControl.Visibility = Visibility.Visible;
            if (LikeOrLookLaterFailShowCountControl.Visibility == Visibility.Visible) LikeOrLookLaterFailShowCountControl.Visibility = Visibility.Collapsed;

            //搜索框
            if (FailInfoSuggestBox.Visibility == Visibility.Collapsed) FailInfoSuggestBox.Visibility = Visibility.Visible;

        }
        else if(!isShowAllFail)
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

        string PickCode = datum.pc;

        var failInfo = await DataAccess.LoadSingleFailInfo(PickCode);

        if (failInfo == null)
        {
            DataAccess.AddOrReplaceFailList_islike_looklater(new()
            {
                pc = PickCode,
                is_like = 1
            });
            ShowTeachingTip("已添加进喜欢");
        }
        //已添加进数据库但不是喜欢
        //标记为喜欢
        else
        {
            switch (failInfo.is_like)
            {
                case 0:
                    DataAccess.UpdateSingleFailInfo(PickCode, "is_like", "1");
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

        string PickCode = datum.pc;

        var failInfo = await DataAccess.LoadSingleFailInfo(PickCode);

        if (failInfo == null)
        {
            DataAccess.AddOrReplaceFailList_islike_looklater(new()
            {
                pc = PickCode,
                look_later = 1
            });
            ShowTeachingTip("已添加进稍后观看");
        }
        //已添加进数据库但不是稍后观看
        //标记为稍后观看
        else
        {

            switch (failInfo.look_later)
            {
                case 0:
                    DataAccess.UpdateSingleFailInfo(PickCode, "look_later", "1");
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

        if(FailList_TeachingTip.IsOpen) FailList_TeachingTip.IsOpen = false;
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
        else if(item is FailInfo)
        {
            return LikeOrLookLaterInFailCoverTemplate;
        }
        else
        {
            return WithoutImageTemplate;
        }
    }
}