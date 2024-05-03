using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Metadata;
using Windows.Storage;
using Windows.Storage.Streams;
using DataAccess;
using DataAccess.Dao.Interface;
using DataAccess.Models.Entity;
using Display.Helper.FileProperties.Name;
using Display.Helper.Network;
using Display.Helper.UI;
using Display.Models.Enums;
using Display.Models.Vo;
using Display.Providers;
using Display.Views.Pages;
using Display.Views.Pages.DetailInfo;
using Display.Views.Pages.More.DatumList;
using Display.Views.Pages.SearchLink;
using Display.Views.Windows;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
using FileInfoInCidSmoke = Display.Views.Pages.DetailInfo.FileInfoInCidSmoke;
using FindInfoAgainSmoke = Display.Views.Pages.DetailInfo.FindInfoAgainSmoke;
using FontFamily = Microsoft.UI.Xaml.Media.FontFamily;
using DataAccess.Dao.Impl;
using FileInfo = DataAccess.Models.Entity.FileInfo;

namespace Display.Controls.UserController;

public sealed partial class VideoDetails
{
    public VideoCoverVo ResultCover
    {
        get => (VideoCoverVo)GetValue(ResultCoverProperty);
        set => SetValue(ResultCoverProperty, value);
    }

    public static readonly DependencyProperty ResultCoverProperty =
        DependencyProperty.Register(nameof(ResultCover), typeof(string), typeof(VideoDetails), null);

    private readonly IVideoInfoDao _videoInfoDao = App.GetService<IVideoInfoDao>();
    private readonly IActorInfoDao _actorInfoDao = App.GetService<IActorInfoDao>();
    
    public VideoDetails()
    {
        InitializeComponent();
    }

    private readonly ObservableCollection<string> _showImageList = [];

    private void LoadData()
    {
        if (ResultCover == null) return;

        //标题
        TitleTextBlock.Text = ResultCover.Title;

        //缩略图
        //检查缩略图是否存在
        List<string> thumbnailList = [];
        switch (AppSettings.ThumbnailOriginType)
        {
            //来源为网络
            case ThumbnailOriginType.Web:
            {
                var videoInfo = _videoInfoDao.GetOneByName(ResultCover.Name);

                var sampleImageListStr = videoInfo?.SampleImages;
                if (!string.IsNullOrEmpty(sampleImageListStr))
                {
                    thumbnailList = sampleImageListStr.Split(",").ToList();
                }

                break;
                }
            //来源为本地
            case (int)ThumbnailOriginType.Local:
                {
                    var folderFullName = Path.Combine(AppSettings.ImageSavePath, ResultCover.Name);
                    var theFolder = new DirectoryInfo(folderFullName);

                    if (theFolder.Exists)
                    {
                        //文件
                        foreach (var nextFile in theFolder.GetFiles())
                        {
                            if (nextFile.Name.Contains("Thumbnail_"))
                            {
                                thumbnailList.Add(nextFile.FullName);
                            }
                        }
                    }

                    break;
                }
        }

        if (thumbnailList.Count > 0)
        {
            thumbnailList = thumbnailList.OrderByNatural(emp => emp.ToString()).ToList();
            ThumbnailGridView.ItemsSource = thumbnailList;
            ThumbnailStackPanel.Visibility = Visibility; 
        }

        //演员
        //之前有数据，清空
        if (ActorStackPanel.Children.Count != 0) ActorStackPanel.Children.Clear();

        //查询该视频对应的演员列表
        var actorList = _actorInfoDao.GetPartListByVideoName(ResultCover.Name);

        for (var i = 0; i < actorList.Count; i++)
        {
            var actor = actorList[i];
            var actorImageControl = new ActorImage(actor, ResultCover.ReleaseYear);

            if (!string.IsNullOrEmpty(actor.Name))
                actorImageControl.Click += ActorButtonOnClick;

            ActorStackPanel.Children.Insert(i, actorImageControl);
        }

        //标签
        //之前有数据，清空
        if (CategoryWrapPanel.Children.Count != 0) CategoryWrapPanel.Children.Clear();
        var categoryList = ResultCover.Category?.Split(",");
        for (var i = 0; i < categoryList?.Length; i++)
        {
            var content = categoryList[i];

            if (string.IsNullOrEmpty(content)) continue;

            // 定义button
            var button = new Button()
            {
                FontFamily = new FontFamily("霞鹜文楷"),
                Content = content,
                BorderThickness = new Thickness(0.5),
                Background =
                    Application.Current.Resources["CardBackgroundFillColorDefaultBrush"] as SolidColorBrush,

            };
            button.Click += LabelButtonOnClick;

            //Color.SkyBlue

            // stackpanel内添加button
            CategoryWrapPanel.Children.Insert(i, button);
        }
    }

    private void GridLoaded(object sender, RoutedEventArgs e)
    {
        LoadData();

        StartListCover_GridTapped();
    }

    // 点击了演员更多页
    public event RoutedEventHandler ActorClick;

    private void ActorButtonOnClick(object sender, RoutedEventArgs args)
    {
        ActorClick?.Invoke(sender, args);
    }

    // 点击了标签更多页
    public event RoutedEventHandler LabelClick;

    private void LabelButtonOnClick(object sender, RoutedEventArgs args)
    {
        LabelClick?.Invoke(sender, args);
    }

    //点击播放键
    public event RoutedEventHandler VideoPlayClick;

    private void VideoPlay_Click(object sender, RoutedEventArgs args)
    {
        VideoPlayClick?.Invoke(sender, args);
    }

    private async void DownButton_Click(object sender, RoutedEventArgs e)
    {
        var name = ResultCover.Name;
        var videoInfoList = DataAccessLocal.Get.GetSingleFileInfoByTrueName(name);

        videoInfoList = videoInfoList.OrderBy(item => item.Name).ToList();

        var downDialogContent = new DownDialogContent(videoInfoList);

        var dialog = new ContentDialog
        {
            // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
            XamlRoot = XamlRoot,
            Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
            Content = downDialogContent,
            Title = "下载",
            PrimaryButtonText = "下载全部",
            SecondaryButtonText = "下载选中项",
            CloseButtonText = "返回",
            DefaultButton = ContentDialogButton.Primary
        };

        var result = await dialog.ShowAsync();

        var downVideoInfoList = new List<FileInfo>();

        var webApi = WebApi.GlobalWebApi;

        //下载方式
        WebApi.DownType downType;
        switch (downDialogContent.DownMethod)
        {
            case "比特彗星":
                downType = WebApi.DownType.Bc;
                if (AppSettings.BitCometSettings == null)
                    ShowTeachingTip("还没有完成比特彗星的设置，请移步到“设置->下载方式->BitComet”完成相关配置");
                break;
            case "aria2":
                downType = WebApi.DownType.Aria2;
                if (AppSettings.Aria2Settings == null)
                    ShowTeachingTip("还没有完成Aria2的设置，请移步到“设置->下载方式->Aria2”完成相关配置");
                break;
            default:
                downType = WebApi.DownType._115;
                break;
        }

        var savePath = AppSettings.BitCometSavePath;

        //下载全部
        if (result == ContentDialogResult.Primary)
        {
            //下载数量大于1则下载在新文件夹下
            string topFolderName = null;
            if (videoInfoList.Count > 1)
                topFolderName = name;
            var isOk = await webApi.RequestDown(videoInfoList, downType, savePath, topFolderName);

            ShowTeachingTip(isOk ? "发送下载请求成功" : "发送下载请求失败");
        }

        //下载选中
        else if (result == ContentDialogResult.Secondary)
        {
            if (downDialogContent.Content is not StackPanel stackPanel) return;
            foreach (var item in stackPanel.Children)
            {
                if(item is not CheckBox fileBox || fileBox.IsChecked == false) continue;
                    
                var selectVideoInfo = videoInfoList.FirstOrDefault(x => x.PickCode == fileBox.Name);
                if (selectVideoInfo != null)
                {
                    downVideoInfoList.Add(selectVideoInfo);
                }
            }

            //下载数量大于1则下载在新文件夹下
            string topFolderName = null;
            if (downVideoInfoList.Count > 1)
                topFolderName = name;

            var isOk = await webApi.RequestDown(downVideoInfoList, downType, savePath, topFolderName);

            ShowTeachingTip(isOk ? "发送下载请求成功" : "发送下载请求失败");
        }
    }

    private void RatingControl_ValueChanged(RatingControl sender, object args)
    {
        var score = sender.Value == 0 ? -1 : sender.Value;
        ResultCover.Score = score;
        _videoInfoDao.ExecuteUpdateByTrueName(ResultCover.Name, info => info.Score = score);
    }

    private void UpdateLookLater(bool? val)
    {
        var lookLaterT = val == true;

        ResultCover.IsLookLater = lookLaterT;
        _videoInfoDao.ExecuteUpdateByTrueName(ResultCover.Name, info => info.LookLater = lookLaterT);
    }

    private void UpdateLike(bool? val)
    {
        var isLike = val == true;
        ResultCover.IsLike = isLike;
        _videoInfoDao.ExecuteUpdateByTrueName(ResultCover.Name, info => info.IsLike = isLike);
    }

    private void Animation_Completed(ConnectedAnimation sender, object args)
    {
        SmokeGrid.Visibility = Visibility.Collapsed;
        SmokeGrid.Children.Add(DestinationImageElement);
        DestinationImageElement.Visibility = Visibility.Collapsed;

        SmokeCancelGrid.Tapped -= SmokeCancelGrid_Tapped;
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        SmokeGridCancel();
    }

    private FindInfoAgainSmoke _findInfoAgainSmoke;

    private void FindAppBarButton_Click(object sender, RoutedEventArgs e)
    {
        //第一次启动FindInfoAgainSmoke
        if (_findInfoAgainSmoke == null)
        {

            _findInfoAgainSmoke = new FindInfoAgainSmoke(ResultCover.Name);

            _findInfoAgainSmoke.ConfirmClick += FindInfoAgainSmoke_ConfirmClick;
        }

        if (!SmokeGrid.Children.Contains(_findInfoAgainSmoke))
        {
            SmokeGrid.Children.Add(_findInfoAgainSmoke);
            SmokeCancelGrid.Tapped += FindInfoAgainSmokeCancelGrid_Tapped;
        }

        SmokeGrid.Visibility = Visibility.Visible;
    }

    /// <summary>
    /// 重新搜刮后选择了选项，并按下了确认键
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void FindInfoAgainSmoke_ConfirmClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button) return;

        //获取最新的信息
        if (button.DataContext is not VideoInfo videoInfo) return;

        await UpdateInfo(videoInfo);

        //退出Smoke
        FindInfoAgainSmokeCancel();
    }

    private async Task UpdateInfo(VideoInfo videoInfo)
    {
        //重新下载图片
        if (!string.IsNullOrEmpty(videoInfo.ImageUrl))
        {
            await DbNetworkHelper.DownloadFile(videoInfo.ImageUrl,
                Path.Combine(AppSettings.ImageSavePath, videoInfo.Name), videoInfo.Name, true);
        }
        
        //更新数据库
        DataAccessLocal.Add.UpdateDataFromVideoInfo(videoInfo);

        //更新ResultInfo数据
        foreach (var item in videoInfo.GetType().GetProperties())
        {
            var name = item.Name;
            //忽略自定义数据
            if (name is "look_later" or "score" or "is_like")
                continue;

            var value = item.GetValue(videoInfo);

            var newItem = ResultCover.GetType().GetProperty(name);
            newItem?.SetValue(ResultCover, value);
        }

        //图片地址不变，但内容变了
        //为了图片显示能够变化
        var oldPath = ResultCover.ImagePath;
        var newPath = videoInfo.ImagePath;
        if (!oldPath.Contains("ms-appx:") && File.Exists(newPath))
        {
            var file = await StorageFile.GetFileFromPathAsync(newPath);

            using IRandomAccessStream fileStream = await file.OpenReadAsync();

            // Set the image source to the selected bitmap
            var bitmapImage = new BitmapImage();

            await bitmapImage.SetSourceAsync(fileStream);
            CoverImage.Source = bitmapImage;
        }

        //重新加载数据
        LoadData();
    }

    private void FindInfoAgainSmokeCancelGrid_Tapped(object sender, TappedRoutedEventArgs e)
    {
        FindInfoAgainSmokeCancel();

        SmokeCancelGrid.Tapped -= FindInfoAgainSmokeCancelGrid_Tapped;
    }

    private void FindInfoAgainSmokeCancel()
    {
        SmokeGrid.Visibility = Visibility.Collapsed;

        if (SmokeGrid.Children.Contains(_findInfoAgainSmoke))
        {
            SmokeGrid.Children.Remove(_findInfoAgainSmoke);
        }
    }

    private async void SmokeGridCancel()
    {
        if (!DestinationImageElement.IsLoaded) return;

        var animation = ConnectedAnimationService.GetForCurrentView()
            .PrepareToAnimate("backwardsAnimation", DestinationImageElement);
        SmokeGrid.Children.Remove(DestinationImageElement);

        // Collapse the smoke when the animation completes.
        animation.Completed += Animation_Completed;

        if (ShowImageFlipView.SelectedItem is not string item) return;

        // If the connected item appears outside the viewport, scroll it into view.
        ThumbnailGridView.ScrollIntoView(item, ScrollIntoViewAlignment.Default);
        ThumbnailGridView.UpdateLayout();

        // Use the Direct configuration to go back (if the API is available). 
        if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 7))
        {
            animation.Configuration = new DirectConnectedAnimationConfiguration();
        }

        // Play the second connected animation. 
        await ThumbnailGridView.TryStartConnectedAnimationAsync(animation, item, "Thumbnail_Image");

    }

    private void ThumbnailGridView_ItemClick(object sender, ItemClickEventArgs e)
    {
        ConnectedAnimation animation = null;

        if (ThumbnailGridView.ContainerFromItem(e.ClickedItem) is GridViewItem container)
        {
            var item = container.Content as string;

            animation = ThumbnailGridView.PrepareConnectedAnimation("forwardAnimation", item,
                "Thumbnail_Image");
        }

        var imagePath = e.ClickedItem as string;

        //之前未赋值
        if (_showImageList.Count == 0)
        {
            if (ThumbnailGridView.ItemsSource is not List<string> thumbnailList) return;

            for (var i = 0; i < thumbnailList.Count; i++)
            {
                var image = thumbnailList[i];

                _showImageList.Add(image);
                if (image == imagePath)
                {
                    ShowImageFlipView.SelectedIndex = i;
                }

            }
        }
        //之前已赋值
        else
        {
            var index = _showImageList.IndexOf(imagePath);
            ShowImageFlipView.SelectedIndex = index;
        }

        ShoeImageName.Text = GetFileIndex();

        SmokeGrid.Visibility = Visibility.Visible;
        DestinationImageElement.Visibility = Visibility.Visible;

        if (animation != null)
        {
            animation.Completed += Animation_Completed1;

            animation.TryStart(DestinationImageElement);
        }
    }

    private void Animation_Completed1(ConnectedAnimation sender, object args)
    {
        SmokeCancelGrid.Tapped += SmokeCancelGrid_Tapped;
    }

    private void SmokeCancelGrid_Tapped(object sender, TappedRoutedEventArgs e)
    {
        SmokeGridCancel();
    }

    private void Thumbnail_Image_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Hand);
    }

    private void Thumbnail_Image_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Arrow);
    }

    private string GetFileIndex()
    {
        return $"{ShowImageFlipView.SelectedIndex + 1}/{_showImageList.Count}";
    }

    public event RoutedEventHandler DeleteClick;

    private void DeletedAppBarButton_Click(object sender, RoutedEventArgs e)
    {
        DeleteClick?.Invoke(sender, e);
    }

    private async void EditAppBarButton_Click(object sender, RoutedEventArgs e)
    {
        var editPage = new EditInfo(ResultCover);
        ContentDialog dialog = new()
        {   
            XamlRoot = this.XamlRoot,
            Title = "编辑",
            PrimaryButtonText = "确定",
            SecondaryButtonText = "返回",
            DefaultButton = ContentDialogButton.Primary,
            Content = editPage,
        };

        var result = await dialog.ShowAsync();

        if (result != ContentDialogResult.Primary) return;
        
        //确定修改
        var videoCoverDisplayClassInfo = editPage.GetInfoAfterEdit();

        VideoInfo newInfo = new(videoCoverDisplayClassInfo.Name);

        //构建构建新的VideoInfo
        //更新UI
        foreach (var item in videoCoverDisplayClassInfo.GetType().GetProperties())
        {
            var name = item.Name;

            //忽略自定义数据
            if (name is "look_later" or "score" or "is_like")
                continue;

            //新值
            var newValue = item.GetValue(videoCoverDisplayClassInfo);

            //为新的Videoinfo赋值
            var newItem = newInfo.GetType().GetProperty(name);

            if (newItem != null)
            {
                newItem.SetValue(newInfo, newValue);
            }

            //原先的旧值
            var oldItem = ResultCover.GetType().GetProperty(name);
            if (oldItem == null) continue;
                    
            var oldValue = oldItem.GetValue(ResultCover);

            //与新值比较，判断是否需要更新正在显示的ResultInfo数据
            if (newValue != oldValue)
            {
                oldItem.SetValue(ResultCover, newValue);
            }
        }

        await UpdateInfo(newInfo);
    }

    private void OpenDirectory_Click(object sender, RoutedEventArgs e)
    {
        var imagePath = Path.GetDirectoryName(ResultCover.ImagePath);
        if (imagePath == Constants.FileType.NoPicturePath)
        {
            return;
        }

        FileMatch.LaunchFolder(imagePath);
    }

    private void ShowImageFlipView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ShoeImageName.Text = GetFileIndex();
    }

    private void ShowTeachingTip(string subtitle, string content = null)
    {
        BasePage.ShowTeachingTip(LightDismissTeachingTip, subtitle, content);
    }

    private void ShowTeachingTip(string subtitle,
        string actionContent, TypedEventHandler<TeachingTip, object> actionButtonClick)
    {
        BasePage.ShowTeachingTip(LightDismissTeachingTip, subtitle, actionContent, actionButtonClick);
    }

    FileInfoInCidSmoke FileInfoInCidSmokePage;

    private void MoreInfoAppBarButton_Click(object sender, RoutedEventArgs e)
    {
        SmokeGrid.Visibility = Visibility.Visible;

        FileInfoInCidSmokePage = new FileInfoInCidSmoke(ResultCover.Name);
        SmokeGrid.Children.Add(FileInfoInCidSmokePage);

        SmokeCancelGrid.Tapped += FileInfoInCidSmokeCancelGrid_Tapped;
    }

    private void FileInfoInCidSmokeCancelGrid_Tapped(object sender, TappedRoutedEventArgs e)
    {
        SmokeGrid.Visibility = Visibility.Collapsed;

        if (SmokeGrid.Children.Contains(FileInfoInCidSmokePage))
        {
            SmokeGrid.Children.Remove(FileInfoInCidSmokePage);
        }

        SmokeCancelGrid.Tapped -= FileInfoInCidSmokeCancelGrid_Tapped;

    }

    private void Cover_Image_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Hand);
        VideoPlayIconInCover.Visibility = Visibility.Visible;
    }

    private void Cover_Image_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Arrow);
        VideoPlayIconInCover.Visibility = Visibility.Collapsed;
    }

    //点击封面
    public event TappedEventHandler CoverTapped;

    private void Cover_Tapped(object sender, TappedRoutedEventArgs e)
    {
        CoverTapped?.Invoke(sender, e);
    }

    //动画结束后开始监听CoverGrid的pointer
    public void ForwardConnectedAnimationCompleted(ConnectedAnimation sender, object args)
    {
        StartListCover_GridTapped();
    }

    public void StartListCover_GridTapped()
    {
        CoverGrid.PointerEntered += Cover_Image_PointerEntered;
        CoverGrid.PointerExited += Cover_Image_PointerExited;
        CoverGrid.Tapped += Cover_Tapped;
    }

    private void EnlargeButton_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        if (sender is not Button { DataContext: string imagePath }) return;

        ProtectedCursor = CursorHelper.GetZoomCursor();

        EnLargeGrid.Visibility = Visibility.Visible;

        EnlargeImage.Source = new BitmapImage(new Uri(imagePath));
    }

    private void EnlargeButton_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        ProtectedCursor = null;

        EnLargeGrid.Visibility = Visibility.Collapsed;
    }

    private void EnLargeImage_OnPointerEntered(object sender, PointerRoutedEventArgs e)
    {
        if (e.Pointer.PointerDeviceType is PointerDeviceType.Mouse or PointerDeviceType.Pen)
        {
            VisualStateManager.GoToState(sender as Control, "EnlargeButtonShown", true);
        }
    }

    private void EnLargeImage_OnPointerExited(object sender, PointerRoutedEventArgs e)
    {
        if (e.Pointer.PointerDeviceType is PointerDeviceType.Mouse or PointerDeviceType.Pen)
        {
            VisualStateManager.GoToState(sender as Control, "EnlargeButtonHidden", true);
        }
    }

    public void CoverImageAddEnterAnimation()
    {
        CoverImage.Transitions.Add(new EntranceThemeTransition());
    }

    private async void FindVideoAppBarButton_Click(object sender, RoutedEventArgs e)
    {
        var tupleResult = await SearchLinkPage.ShowInContentDialog(ResultCover.Name, XamlRoot);

        // 用户取消操作
        if (tupleResult == null) return;

        var (isSucceed, msg) = tupleResult;

        if (isSucceed)
        {
            ShowTeachingTip(msg, "打开所在目录", (_, _) =>
            {
                // 打开所在目录
                CommonWindow.CreateAndShowWindow(new FileListPage(AppSettings.SavePath115Cid));
            });
        }
        else
        {
            ShowTeachingTip(msg);
        }
    }
}