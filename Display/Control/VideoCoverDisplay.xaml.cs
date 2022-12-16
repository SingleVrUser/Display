using Data;
using Display.Model;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Windows.Storage;

namespace Display.Control;

public sealed partial class VideoCoverDisplay : UserControl,INotifyPropertyChanged
{
    //显示的是匹配成功的还是失败的
    public static readonly DependencyProperty IsShowFailListViewProperty =
    DependencyProperty.Register("IsShowFailListView", typeof(bool), typeof(VideoCoverDisplay), null);
    public static readonly DependencyProperty IsShowFailListButtonProperty =
    DependencyProperty.Register("IsShowFailListButton", typeof(bool), typeof(VideoCoverDisplay), null);
    public bool IsShowSuccessListView
    {
        get => !IsShowFailListView;
    }
    public bool IsShowFailListView
    {
        get { return (bool)GetValue(IsShowFailListViewProperty); }
        set
        {
            SetValue(IsShowFailListViewProperty, value);
            OnPropertyChanged(nameof(IsShowSuccessListView));
        }
    }
    public bool IsShowFailListButton
    {
        get { return (bool)GetValue(IsShowFailListButtonProperty); }
        set { SetValue(IsShowFailListButtonProperty, value); }
    }

    //本地默认设置
    ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
    ObservableCollection<AccountContentInPage> AccountInPage;
    ApplicationDataCompositeValue composite = (ApplicationDataCompositeValue)ApplicationData.Current.LocalSettings.Values["DisplaySettings"];

    //全部数据（匹配成功或失败）
    private List<VideoCoverDisplayClass> _FileGrid;
    public List<VideoCoverDisplayClass> FileGrid
    {
        get
        {
            return _FileGrid;
        }
        set
        {
            if (_FileGrid == value)
                return;

            _FileGrid = value;

            tryDisplayInfo(0);

            OnPropertyChanged();
        }
    }

    /// <summary>
    /// 显示的数据（数量小于或等于全部数据）
    /// 用于分页显示
    /// </summary>
    public ObservableCollection<VideoCoverDisplayClass> FileGrid_part;

    /// <summary>
    /// 显示的数据
    /// 用于增量显示，成功列表
    /// </summary>
    private Model.IncrementalLoadingdVideoFileCollection _incrementalFileGrid;
    public Model.IncrementalLoadingdVideoFileCollection IncrementalFileGrid
    {
        get => _incrementalFileGrid;
        set
        {
            _incrementalFileGrid = value;

            OnPropertyChanged();
        }
    }

    /// <summary>
    /// 显示的数据
    /// 用于增量显示，失败列表
    /// </summary>
    private Model.IncrementalLoadingdFileCollection _failFileGrid_part;
    public Model.IncrementalLoadingdFileCollection FailFileGrid_part
    {
        get => _failFileGrid_part;
        set
        {
            if (_failFileGrid_part == value)
                return;

            _failFileGrid_part = value;

            OnPropertyChanged();
        }
    }

    public void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        // Raise the PropertyChanged event, passing the name of the property whose value has changed.
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    //item开始Index
    private int startValue = 0;
    //item开始Index
    private int nowPage = 1;
    //每页显示的最大数量
    private int _showCountInPage = 30;
    private int showCountInPage
    {
        get
        {
            int count = 0;
            if (composite == null)
            {
                composite = new ApplicationDataCompositeValue();
            }
            else
            {
                count = Convert.ToInt32(composite["CountPerPage"]);
            }

            if (count == 0)
            {
                count = _showCountInPage;
            }

            return count;
        }
        set
        {
            composite["CountPerPage"] = value;
        }
    }
    //总页数
    private int totalPageCount = 1;

    //图片的最小值
    private double SliderMinValue = 200;
    //图片的最大值
    private double SliderMaxValue = 900;

    public VideoCoverDisplay()
    {
        this.InitializeComponent();
    }

    /// <summary>
    /// 初始化每页显示数量选项
    /// </summary>
    private void TryInitShowCountPerPage()
    {
        if (AccountInPage == null)
            AccountInPage = new();

        if (AccountInPage.Count == 0)
        {
            for (var i = 10; i < 100; i += 20)
            {
                AccountInPage.Add(new AccountContentInPage()
                {
                    ContentAcount = i,
                });
            }
        }

        if (ContentAcountListView.ItemsSource == null)
        {
            //显示UI
            ContentAcountListView.ItemsSource = AccountInPage;
        }

    }

    /// <summary>
    /// 加载（更新）文件信息，根据startValue[0:FileGrid.Count]
    /// </summary>
    private void tryDisplayInfo(int newStartValue)
    {
        //加载全部
        if(LoadAll_ToggleButton.IsChecked == true)
        {
            if (IncrementalFileGrid != null)
            {
                IncrementalFileGrid = new(FileGrid);
                BasicGridView.ItemsSource = IncrementalFileGrid;

                foreach (var item in FileGrid.Skip(0).Take(30))
                {
                    IncrementalFileGrid.Add(item);
                }
            }
            else if (FileGrid.Count != 0)
            {
                IncrementalFileGrid = new(FileGrid);

                foreach (var item in FileGrid.Skip(0).Take(30))
                {
                    IncrementalFileGrid.Add(item);
                }
                BasicGridView.ItemsSource = IncrementalFileGrid;
            }
        }
        else
        {
            if (ContentAcountListView.SelectedItem == null)
            {
                TryInitShowCountPerPage();
                LoadDefaultSettings();
            }

            PageControl_StackPanel.Visibility = Visibility.Visible;
            FileGrid_part = new();
            BasicGridView.ItemsSource = FileGrid_part;
            totalPageCount = (int)Math.Ceiling((double)FileGrid.Count / showCountInPage);
            DisplayInfoPerPage(newStartValue);
        }

    }

    private void DisplayInfoPerPage(int newStartValue)
    {
        var newEndIndex = newStartValue + showCountInPage - 1;

        //举例：
        //    FileGrid.Count = 50
        //    Value           0-49
        if (newEndIndex >= FileGrid.Count)
        {
            newEndIndex = FileGrid.Count - 1;
        }
        else if (newEndIndex < FileGrid.Count && newStartValue >= 0)
        {
        }
        else
        {
            return;
        }
        //当前页数
        int newNowPage = newStartValue / showCountInPage + 1;

        double maxPageCount = Math.Ceiling((double)FileGrid.Count / showCountInPage);
        //当前页数必须小于或等于最大页数，且大于0
        if (newNowPage <= maxPageCount && newNowPage > 0)
        {
            nowPage = newNowPage;
        }
        else
        {
            return;
        }

        //是否需要更新页数显示
        if (Convert.ToInt32(nowPageTextBox.Text) != newNowPage)
        {
            nowPageTextBox.Text = nowPage.ToString();
        }

        //更新startValue
        startValue = newStartValue;

        // 删除原有显示
        if (FileGrid_part != null)
        {
            FileGrid_part.Clear();
        }
        else
        {
            FileGrid_part = new();
        }

        for (int i = newStartValue; i <= newEndIndex; i++)
        {
            var item = FileGrid[i];
            FileGrid_part.Add(item);
        }

        InfoBar.Message = $"总数量：{FileGrid.Count} 总页数：{totalPageCount}，当前显示 {newStartValue + 1} 到 {newEndIndex + 1} 内容";
    }

    /// <summary>
    /// 初始化应用设置(每页最大显示数量，图片大小)
    /// </summary>
    private void LoadDefaultSettings()
    {
        // 默认值
        // 图片大小，每页显示的最大数量
        double ImageSize = 350;
        int CountPerPage = 10;

        // 加载应用设置，有则使用，没有则添加
        if (composite != null)
        {
            if (composite.ContainsKey("ImageSize"))
            {
                var ImageSizeValue = composite["ImageSize"];

                if (ImageSizeValue is int i)
                    ImageSize = Convert.ToDouble(i);
                else if (ImageSizeValue is double d)
                    ImageSize = d;
            }
            //ImageSize = composite.ContainsKey("ImageSize") ? (double)composite["ImageSize"] : ImageSize;
            CountPerPage = composite.ContainsKey("CountPerPage") ? (int)composite["CountPerPage"] : CountPerPage;
        }
        else
        {
            composite = new ApplicationDataCompositeValue();
            composite["ImageSize"] = ImageSize;
            composite["CountPerPage"] = CountPerPage;
            localSettings.Values["DisplaySettings"] = composite;
        }

        ImageSizeChangeSlider.Value = ImageSize;

        var item = AccountInPage.Where(i => i.ContentAcount == CountPerPage);

        // 无该最大显示数量
        if (!item.Any())
        {
            tryAddNewAcountInPage(CountPerPage);
            item = AccountInPage.Where(i => i.ContentAcount == CountPerPage);
        }

        ContentAcountListView.SelectedIndex = AccountInPage.IndexOf(item.First());

        ContentAcountListView.SelectionChanged += ContentAcountListView_SelectionChanged;
    }

    /// <summary>
    /// 添加一页显示数量
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void AddAccountPage_NumberBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        int newValue = (int)args.NewValue;
        tryAddNewAcountInPage(newValue);
    }

    private void tryAddNewAcountInPage(int newValue)
    {
        if (newValue != double.NaN && newValue > 0)
        {
            AccountContentInPage newAccount = new AccountContentInPage()
            {
                ContentAcount = newValue
            };

            //排除重复项
            var item = AccountInPage.Where(i => i.ContentAcount == newValue);
            if (item.Count() == 0)
            {
                AccountInPage.Add(newAccount);
            }

            // 排序后选择项可能会丢失，记录排序项，然后排序后重新添加
            var selectIndex = ContentAcountListView.SelectedIndex;

            // 排序
            var sortedItemsList = AccountInPage.OrderBy(i => i.ContentAcount).ToList();
            foreach (var sortedItem in sortedItemsList)
            {
                var moveIndex = AccountInPage.IndexOf(sortedItem);
                var movedIndex = sortedItemsList.IndexOf(sortedItem);

                AccountInPage.Move(moveIndex, movedIndex);
            }

            if (ContentAcountListView.SelectedIndex == -1)
            {
                // 重新选择，若影响，选择项+1（因为添加项为1）
                ContentAcountListView.SelectedIndex = selectIndex + 1;
            }
        }
    }

    double HorizontalPadding = 6;
    double markSliderValue;
    /// <summary>
    /// Slider值改变后，调整图片大小
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Slider_valueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
        markSliderValue = e.NewValue;

        if (IsAutoAdjustImageSize_ToggleButton.IsChecked == true)
        {
            AutoAdjustImageSize();
        }
        else
        {
            AdjustImageSize(e.NewValue);
        }
    }

    private void AutoAdjustImageSize(double GridWidth = -1,bool AdjustSliderValue = false)
    {
        if(GridWidth == -1)
            GridWidth = BasicGridView.ActualWidth;

        var ImageCountPerRow = Math.Floor(GridWidth / (markSliderValue + HorizontalPadding));
        if (ImageCountPerRow <= 0) ImageCountPerRow = 1;
        System.Diagnostics.Debug.WriteLine($"每行图片数量：{ImageCountPerRow}");

        double newImageWidth = GridWidth / ImageCountPerRow - HorizontalPadding - 2;
        System.Diagnostics.Debug.WriteLine($"推算出的图片宽度应为：{newImageWidth}");

        //必须要在一定范围内

        if (SliderMinValue <= newImageWidth && newImageWidth <= SliderMaxValue)
        {
            if (markSliderValue * 0.5 <= newImageWidth && newImageWidth <= markSliderValue * 1.5)
            {
                AdjustImageSize(newImageWidth);

                if(AdjustSliderValue) AdjustSliderValueOnly(newImageWidth);
            }

            //每行图片最大为1的话，可以缩小到最小值
            else if (ImageCountPerRow == 1)
            {
                AdjustImageSize(newImageWidth);

                if (AdjustSliderValue) AdjustSliderValueOnly(newImageWidth);
            }
        }
    }

    private void AdjustImageSize(double width)
    {
        if (FileGrid == null) return;

        for (int i = 0; i < FileGrid.Count; i++)
        {
            FileGrid[i].imagewidth = width;
            FileGrid[i].imageheight = width / 3 * 2;
        }

        //更改应用设置
        composite["ImageSize"] = width;
        localSettings.Values["DisplaySettings"] = composite;
    }

    public event RoutedEventHandler Click;
    private void MoreButton_Click(object sender, RoutedEventArgs e)
    {
        Click?.Invoke(sender, e);
    }

    /// <summary>
    /// 删除当前页的最大显示数量
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        var item = (sender as Button).DataContext as AccountContentInPage;

        //删除项为选中项
        if (item == ContentAcountListView.SelectedItem)
        {
            //数量两个以上
            if (AccountInPage.Count() > 1)
            {
                //存在下一项
                if (ContentAcountListView.SelectedIndex + 2 <= AccountInPage.Count)
                {
                    ContentAcountListView.SelectedIndex++;
                }
                else
                {
                    ContentAcountListView.SelectedIndex--;
                }
            }

        }
        AccountInPage.Remove(item);

        //数量为零，添加一项
        if (AccountInPage.Count() == 0)
        {
            AccountInPage.Add(new AccountContentInPage()
            {
                ContentAcount = 50,
            });
            ContentAcountListView.SelectedIndex = 0;

        }
    }

    private void ContentAcountListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ContentAcountListView.SelectedItem == null) return;
        if (FileGrid == null || FileGrid.Count == 0) return;

        // 更新应用设置
        var pageCountItem = ContentAcountListView.SelectedItem as AccountContentInPage;

        //每页显示的数量
        showCountInPage = pageCountItem.ContentAcount;

        localSettings.Values["DisplaySettings"] = composite;

        // 重新显示（按照新的显示数量）
        tryDisplayInfo(startValue);
    }


    /// <summary>
    /// 显示上一页内容
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void PreviousPageButton(object sender, RoutedEventArgs e)
    {
        if (startValue - showCountInPage < 0)
        {
            LightDismissTeachingTip.Subtitle = "最小了，不能再减了";
            LightDismissTeachingTip.Target = sender as Button;
            LightDismissTeachingTip.IsOpen = true;
            return;
        }

        tryDisplayInfo(startValue - showCountInPage);
    }

    /// <summary>
    /// 显示下一页内容
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void NextPageButton(object sender, RoutedEventArgs e)
    {
        if(startValue + showCountInPage >= totalPageCount * showCountInPage)
        {
            LightDismissTeachingTip.Subtitle = "最大了，不能再加了";
            LightDismissTeachingTip.Target = sender as Button;
            LightDismissTeachingTip.IsOpen = true;
            return;
        }

        tryDisplayInfo(startValue + showCountInPage);
    }

    /// <summary>
    /// 鼠标悬停在Grid，显示可操作按钮
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Grid_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        if(!(sender is Grid grid)) return;
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
        var CollapsedGrid = grid.Children[1] as Grid;

        var CommandBarControl = CollapsedGrid.Children.Where(x => x is CommandBar).FirstOrDefault() as CommandBar;
        if (CommandBarControl.IsOpen == false)
        {
            CollapsedGrid.Visibility = Visibility.Collapsed;
        }
    }

    private void button_OnPointerEntered(object sender, PointerRoutedEventArgs e)
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
        AppBarToggleButton isLikeButton = (AppBarToggleButton)sender;

        var videoInfo = (VideoCoverDisplayClass)isLikeButton.DataContext;

        videoInfo.is_like = (bool)isLikeButton.IsChecked ? 1 : 0;

        DataAccess.UpdateSingleDataFromVideoInfo(videoInfo.truename, "is_like", videoInfo.is_like.ToString());
    }

    /// <summary>
    /// 点击了稍后观看
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void LookLaterToggleButton_Click(object sender, RoutedEventArgs e)
    {
        AppBarToggleButton isLikeButton = (AppBarToggleButton)sender;

        var videoInfo = (VideoCoverDisplayClass)isLikeButton.DataContext;

        videoInfo.look_later = (bool)isLikeButton.IsChecked ? DateTimeOffset.Now.ToUnixTimeSeconds() : 0;

        DataAccess.UpdateSingleDataFromVideoInfo(videoInfo.truename, "look_later", videoInfo.look_later.ToString());
    }

    /// <summary>
    /// 修改评分
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void RatingControl_ValueChanged(RatingControl sender, object args)
    {
        var videoInfo = sender.DataContext as VideoCoverDisplayClass;

        string score_str = videoInfo.score == 0 ? "-1" : sender.Value.ToString();

        DataAccess.UpdateSingleDataFromVideoInfo(videoInfo.truename, "score", score_str);

    }

    public event RoutedEventHandler SingleVideoPlayClick;
    private void SingleVideoButton_Click(object sender, RoutedEventArgs e)
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

    /// <summary>
    /// 手动输入当前页数
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void nowPageTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        //如果不是回车
        if (e.Key != Windows.System.VirtualKey.Enter)
        {
            return;
        }

        //检查页数合法性
        string pageText = nowPageTextBox.Text;

        int n;
        bool is_num = Int32.TryParse(pageText, out n);

        //非数字
        if (!is_num)
        {
            HintTeachingTip.Title = "提示";
            HintTeachingTip.Subtitle = "输入有误，只允许数字，请重新输入";
            HintTeachingTip.Target = nowPageTextBox;
            HintTeachingTip.IconSource = new FontIconSource() { FontFamily = new FontFamily("Segoe Fluent Icons"), Glyph = "\xE946" };
            HintTeachingTip.IsOpen = true;

            //回复原值
            nowPageTextBox.Text = nowPage.ToString();
            e.Handled = true;
        }else if(n > totalPageCount)
        {
            nowPageTextBox.Text = totalPageCount.ToString();
            int newStartValue = (totalPageCount - 1) * showCountInPage;
            // 重新显示（按照新的显示数量）
            tryDisplayInfo(newStartValue);
        }else if(n <= 0)
        {
            nowPageTextBox.Text = "1";
            // 重新显示（按照新的显示数量）
            tryDisplayInfo(0);
        }
        else
        {
            int newStartValue = (n - 1) * showCountInPage;
            // 重新显示（按照新的显示数量）
            tryDisplayInfo(newStartValue);
        }

    }

    /// <summary>
    /// 选择当前页的最大显示数量
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void orderListView_ItemClick(object sender, ItemClickEventArgs e)
    {
        ListView selectListView = (ListView)sender;
        var clickStackPanel = (e.ClickedItem as StackPanel);
        var selectTextBlock = clickStackPanel.Children.Where(x => x is TextBlock).First() as TextBlock;
        string selectOrderText = selectTextBlock.Text;

        FontIcon lastFontIcon = clickStackPanel.Children.Where(x => x is FontIcon).Last() as FontIcon;

        string upGlyph = "\xE014";
        string downGlyph = "\xE015";
        string newGlyph = "\xE174";

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

        switch (selectOrderText)
        {
            case "名称":
                newGlyph = "\xE185";
                FileGrid = isUpSort ? new List<VideoCoverDisplayClass>(FileGrid.OrderBy(item => item.truename)) : new List<VideoCoverDisplayClass>(FileGrid.OrderByDescending(item => item.truename));
                //tryDisplayInfo(0);
                break;
            case "演员":
                newGlyph = "\xE13D";
                FileGrid = isUpSort ? new List<VideoCoverDisplayClass>(FileGrid.OrderBy(item => item.actor)) : new List<VideoCoverDisplayClass>(FileGrid.OrderByDescending(item => item.actor));
                //tryDisplayInfo(0);
                break;
            case "年份":
                newGlyph = "\xEC92";
                FileGrid = isUpSort ? new List<VideoCoverDisplayClass>(FileGrid.OrderBy(item => item.realeaseYear)) : new List<VideoCoverDisplayClass>(FileGrid.OrderByDescending(item => item.realeaseYear));
                //tryDisplayInfo(0);
                break;
            case "随机":
                newGlyph = "\xF463";
                Random rnd = new Random();
                FileGrid = new List<VideoCoverDisplayClass>(FileGrid.OrderByDescending(item => rnd.Next()));
                //tryDisplayInfo(0);
                break;
        }

        //更新首图标
        FontIcon orderFontIcon = orderButton.Content as FontIcon;
        if (orderFontIcon.Glyph != newGlyph)
        {
            orderButton.Content = new FontIcon() { FontFamily = new FontFamily("Segoe Fluent Icons"), Glyph = newGlyph };
        }

    }

    /// <summary>
    /// 点击了删除按钮
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
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

        if(result == ContentDialogResult.Primary)
        {
            if(sender is AppBarButton appBarButton)
            {
                var item = appBarButton.DataContext as VideoCoverDisplayClass;
                //从数据库中删除
                DataAccess.DeleteDataInVideoInfoTable(item.truename);

                //删除存储的文件夹
                string savePath = Path.Combine(AppSettings.Image_SavePath, item.truename);
                if (Directory.Exists(savePath))
                {
                    Directory.Delete(savePath,true);
                }

                FileGrid.Remove(item);
                FileGrid_part.Remove(item);

            }
        }
    }

    //开始动画
    public async void StartAnimation(ConnectedAnimation animation,VideoCoverDisplayClass item)
    {
        if(BasicGridView.Items.Contains(item))
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
        if(!(sender is ToggleSwitch toggleSwitch))
        {
            return;
        }

        toggleSwitch.Opacity= 1;
    }

    private void ShowType_ToggleSwitch_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        if (!(sender is ToggleSwitch toggleSwitch))
        {
            return;
        }

        toggleSwitch.Opacity = 0.3;
    }

    private async void ShowType_ToggleSwitch_Toggled(object sender, RoutedEventArgs e)
    {
        var toggleswitch = sender as ToggleSwitch;

        //匹配失败
        if (toggleswitch.IsOn)
        {
            IsShowFailListView = true;
            ShowCount_Control.Visibility = Visibility.Collapsed;

            if (FailFileGrid_part == null)
            {
                var AllCount = await DataAccess.CheckFailFilesCount();
                FailShowCountControl.AllCount = AllCount;
                FailFileGrid_part = new() { AllCount = AllCount };
            }

            BasicGridView.ItemsSource = FailFileGrid_part;

            if (FailFileGrid_part.Count == 0)
            {
                var lists = await DataAccess.LoadFailFileInfo(0, 30);

                lists.ForEach(datum => FailFileGrid_part.Add(datum));
            }
            CloseListeningSizeChanged();
        }
        //匹配成功
        else
        {
            //显示
            IsShowFailListView = false;

            //是否需要动态调整图片大小
            tryStartListeningSizeChanged();

            //添加数据
            if (FileGrid == null)
            {
                List<VideoInfo> VideoInfoList = DataAccess.LoadAllVideoInfo(-1);
                FileGrid = FileMatch.getFileGrid(VideoInfoList);

                //增量加载
                if (LoadAll_ToggleButton.IsChecked == true)
                    ShowCount_Control.Visibility = Visibility.Visible;
                //每页固定加载数
                else
                    ShowCount_Control.Visibility = Visibility.Collapsed;

            }
            else
            {
                //增量加载
                if (LoadAll_ToggleButton.IsChecked == true)
                {
                    ShowCount_Control.Visibility = Visibility.Visible;
                    BasicGridView.ItemsSource = IncrementalFileGrid;
                }
                //每页固定加载数
                else
                {
                    ShowCount_Control.Visibility = Visibility.Collapsed;
                    BasicGridView.ItemsSource = FileGrid_part;
                }
            }

        }
    }

    private void FailGrid_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        if (!(sender is Grid grid))
        {
            return;
        }

        grid.Scale = new System.Numerics.Vector3(1.01f);
    }

    private void FailGrid_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        if (!(sender is Grid grid))
        {
            return;
        }

        grid.Scale = new System.Numerics.Vector3(1.0f);
    }

    private async void FileAutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (!(sender is AutoSuggestBox suggestBox))
            return;

        var checkTest = suggestBox.Text;

        FailFileGrid_part = new() { filterName= checkTest};
        var lists = await DataAccess.LoadFailFileInfo(0,30,checkTest);
        FailShowCountControl.AllCount = await DataAccess.CheckFailFilesCount(checkTest);
        BasicGridView.ItemsSource = FailFileGrid_part;
        lists.ForEach(datum => FailFileGrid_part.Add(datum));

    }

    //加载全部
    private void LoadAll_ToggleButton_Checked(object sender, RoutedEventArgs e)
    {
        if (ShowCount_Control == null)
            return;

        if (IsShowFailListView)
            return;

        ShowCount_Control.Visibility = Visibility.Visible;
        PageControl_StackPanel.Visibility = Visibility.Collapsed;


        BasicGridView.ItemsSource = IncrementalFileGrid;
        tryDisplayInfo(0);


    }

    //限制每页数量
    private void LoadAll_ToggleButton_UnChecked(object sender, RoutedEventArgs e)
    {
        if (PageControl_StackPanel == null)
            return;

        if (IsShowFailListView)
            return;

        PageControl_StackPanel.Visibility = Visibility.Visible;
        ShowCount_Control.Visibility = Visibility.Collapsed;

        TryInitShowCountPerPage();
        BasicGridView.ItemsSource = FileGrid_part;
        tryDisplayInfo(0);
    }

    private void ToTopButton_Click(object sender, RoutedEventArgs e)
    {
        if (BasicGridView.ItemsSource is IncrementalLoadingdVideoFileCollection successCollection)
            BasicGridView.ScrollIntoView(successCollection.First());

        if (BasicGridView.ItemsSource is Model.IncrementalLoadingdFileCollection failCollection)
            BasicGridView.ScrollIntoView(failCollection.First());

    }

    /// <summary>
    /// 打开对Grid大小的监听，以动态调整图片大小
    /// </summary>
    public void tryStartListeningSizeChanged()
    {
        if(IsAutoAdjustImageSize_ToggleButton.IsChecked == true)
        {
            //ImageSizeChangeSlider.IsEnabled= false;
            BasicGridView.SizeChanged += BasicGridView_SizeChanged;
        }
    }

    /// <summary>
    /// 关闭对Grid大小的监听
    /// </summary>
    private void CloseListeningSizeChanged()
    {
        //ImageSizeChangeSlider.IsEnabled = true;
        BasicGridView.SizeChanged -= BasicGridView_SizeChanged;
    }

    /// <summary>
    /// 图片模式下根据Grid调整大小
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void BasicGridView_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        double newGridWidth = e.NewSize.Width;

        AutoAdjustImageSize(newGridWidth,true);

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
        tryStartListeningSizeChanged();
        System.Diagnostics.Debug.WriteLine("启用图片大小的动态调整");
    }

    /// <summary>
    /// 关闭图片大小的动态调整
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void AutoAdjustImageSize_ToggleButton_UnChecked(object sender, RoutedEventArgs e)
    {
        CloseListeningSizeChanged();
        System.Diagnostics.Debug.WriteLine("关闭图片大小的动态调整");
    }

}


/// <summary>
/// GridView样式选择
/// </summary>
public class CoverItemTemplateSelector : DataTemplateSelector
{
public DataTemplate ImageTemplate { get; set; }
public DataTemplate WithoutImageTemplate { get; set; }

protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
{
    if(item is VideoCoverDisplayClass)
    {
        return ImageTemplate;
    }
    else
    {
        return WithoutImageTemplate;
    }
}
}