﻿using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation.Metadata;
using Windows.Storage;
using Windows.Storage.Streams;
using FontFamily = Microsoft.UI.Xaml.Media.FontFamily;
using Display.Views;
using Windows.Foundation;
using Display.CustomWindows;
using Display.Helper.FileProperties.Name;
using Display.Helper.UI;
using Display.Models.Data;
using Display.Models.Data.Enums;
using Display.Views.DetailInfo;
using Display.Views.More.DatumList;
using Display.Views.SearchLink;
using DownDialogContent = Display.Views.DetailInfo.DownDialogContent;
using FileInfoInCidSmoke = Display.Views.DetailInfo.FileInfoInCidSmoke;
using FindInfoAgainSmoke = Display.Views.DetailInfo.FindInfoAgainSmoke;
using Display.Constants;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.Controls
{
    public sealed partial class VideoDetails : UserControl
    {
        public VideoCoverDisplayClass ResultInfo
        {
            get => (VideoCoverDisplayClass)GetValue(ResultInfoProperty);
            set => SetValue(ResultInfoProperty, value);
        }

        public static readonly DependencyProperty ResultInfoProperty =
            DependencyProperty.Register(nameof(ResultInfo), typeof(string), typeof(VideoDetails), null);

        public VideoDetails()
        {
            this.InitializeComponent();
        }

        private readonly ObservableCollection<string> _showImageList = new();

        private async void LoadData()
        {
            if (ResultInfo == null) return;

            //标题
            TitleTextBlock.Text = ResultInfo.Title;

            //演员
            //之前有数据，清空
            if (ActorStackPanel.Children.Count != 0) ActorStackPanel.Children.Clear();

            ////查询该视频对应的演员列表
            var actorList = await DataAccess.Get.GetActorInfoByVideoName(ResultInfo.trueName);

            for (var i = 0; i < actorList?.Length; i++)
            {
                var actor = actorList[i];
                var actorImageControl = new Controls.ActorImage(actor, ResultInfo.ReleaseTime);

                if (!string.IsNullOrEmpty(actor.Name))
                    actorImageControl.Click += ActorButtonOnClick;

                ActorStackPanel.Children.Insert(i, actorImageControl);
            }

            //标签
            //之前有数据，清空
            if (CategoryWrapPanel.Children.Count != 0) CategoryWrapPanel.Children.Clear();
            var categoryList = ResultInfo.Category?.Split(",");
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

            //缩略图
            //检查缩略图是否存在
            List<string> thumbnailList = new();

            //来源为本地
            if (AppSettings.ThumbnailOriginType == (int)ThumbnailOriginType.Local)
            {
                var folderFullName = Path.Combine(AppSettings.ImageSavePath, ResultInfo.trueName);
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

            }
            //来源为网络
            else if (AppSettings.ThumbnailOriginType == ThumbnailOriginType.Web)
            {
                var videoInfo = DataAccess.Get.GetSingleVideoInfoByTrueName(ResultInfo.trueName);

                var sampleImageListStr = videoInfo?.SampleImageList;
                if (!string.IsNullOrEmpty(sampleImageListStr))
                {
                    thumbnailList = sampleImageListStr.Split(",").ToList();
                }
            }

            if (thumbnailList.Count > 0)
            {
                thumbnailList = thumbnailList.OrderByNatural(emp => emp.ToString()).ToList();
                ThumbnailGridView.ItemsSource = thumbnailList;
                ThumbnailStackPanel.Visibility = Visibility;
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
            string name = ResultInfo.trueName;
            var videoinfoList = DataAccess.Get.GetSingleFileInfoByTrueName(name);

            videoinfoList = videoinfoList.OrderBy(item => item.Name).ToList();

            var downDialogContent = new DownDialogContent(videoinfoList);

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

            var downVideoInfoList = new List<Datum>();

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

            string savePath = AppSettings.BitCometSavePath;


            //下载全部
            if (result == ContentDialogResult.Primary)
            {
                //下载数量大于1则下载在新文件夹下
                string topFolderName = null;
                if (videoinfoList.Count > 1)
                    topFolderName = name;
                bool isOk = await webApi.RequestDown(videoinfoList, downType, savePath, topFolderName);

                if (isOk)
                    ShowTeachingTip("发送下载请求成功");
                else
                    ShowTeachingTip("发送下载请求失败");
            }

            //下载选中
            else if (result == ContentDialogResult.Secondary)
            {
                var stackPanel = downDialogContent.Content as StackPanel;
                foreach (var item in stackPanel.Children)
                {
                    if (item is CheckBox)
                    {
                        CheckBox fileBox = item as CheckBox;
                        if (fileBox.IsChecked == true)
                        {
                            var selectVideoInfo = videoinfoList.FirstOrDefault(x => x.PickCode == fileBox.Name);
                            if (selectVideoInfo != null)
                            {
                                downVideoInfoList.Add(selectVideoInfo);
                            }
                        }
                    }
                }

                //下载数量大于1则下载在新文件夹下
                string topFolderName = null;
                if (downVideoInfoList.Count > 1)
                    topFolderName = name;

                bool isOk = await webApi.RequestDown(downVideoInfoList, downType, savePath, topFolderName);

                ShowTeachingTip(isOk ? "发送下载请求成功" : "发送下载请求失败");
            }
            else
            {
                //DialogResult.Text = "User cancelled the dialog";
            }
        }

        private void UpdateLookLater(bool? val)
        {
            var lookLaterT = val == true ? DateTimeOffset.Now.ToUnixTimeSeconds() : 0;

            ResultInfo.LookLater = lookLaterT;
            DataAccess.Update.UpdateSingleDataFromVideoInfo(ResultInfo.trueName, "look_later", lookLaterT.ToString());
        }

        private void UpdateLike(bool? val)
        {
            var isLike = val == true ? 1 : 0;

            ResultInfo.IsLike = isLike;
            DataAccess.Update.UpdateSingleDataFromVideoInfo(ResultInfo.trueName, "is_like", isLike.ToString());
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

                _findInfoAgainSmoke = new FindInfoAgainSmoke(ResultInfo.trueName);

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
            if (!(sender is Button button)) return;

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
                await GetInfoFromNetwork.DownloadFile(videoInfo.ImageUrl,
                    Path.Combine(AppSettings.ImageSavePath, videoInfo.trueName), videoInfo.trueName, true);
            }

            //更新数据库
            DataAccess.Update.UpdateDataFromVideoInfo(videoInfo);

            //更新ResultInfo数据
            foreach (var item in videoInfo.GetType().GetProperties())
            {
                var name = item.Name;
                //忽略自定义数据
                if (name is "look_later" or "score" or "is_like")
                    continue;

                var value = item.GetValue(videoInfo);

                var newItem = ResultInfo.GetType().GetProperty(name);
                newItem?.SetValue(ResultInfo, value);
            }

            //图片地址不变，但内容变了
            //为了图片显示能够变化
            var oldPath = ResultInfo.ImagePath;
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
            var EditPage = new EditInfo(ResultInfo);
            ContentDialog dialog = new()
            {
                XamlRoot = this.XamlRoot,
                Title = "编辑",
                PrimaryButtonText = "确定",
                SecondaryButtonText = "返回",
                DefaultButton = ContentDialogButton.Primary,
                Content = EditPage,
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                //确定修改
                var VideoCoverDisplayClassInfo = EditPage.GetInfoAfterEdit();

                VideoInfo newInfo = new();

                //构建构建新的VideoInfo
                //更新UI
                foreach (var item in VideoCoverDisplayClassInfo.GetType().GetProperties())
                {
                    var name = item.Name;

                    //忽略自定义数据
                    if (name == "look_later" || name == "score" || name == "is_like")
                        continue;

                    //新值
                    var newValue = item.GetValue(VideoCoverDisplayClassInfo);

                    //为新的Videoinfo赋值
                    var newItem = newInfo.GetType().GetProperty(name);

                    if (newItem != null)
                    {
                        newItem.SetValue(newInfo, newValue);
                    }

                    //原先的旧值
                    var oldItem = ResultInfo.GetType().GetProperty(name);
                    var oldValue = oldItem.GetValue(ResultInfo);

                    //与新值比较，判断是否需要更新正在显示的ResultInfo数据
                    if (newValue != oldValue)
                    {
                        oldItem.SetValue(ResultInfo, newValue);
                    }
                }

                await UpdateInfo(newInfo);
            }
        }

        private void OpenDirectory_Click(object sender, RoutedEventArgs e)
        {
            var imagePath = Path.GetDirectoryName(ResultInfo.ImagePath);
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

        private void RatingControl_ValueChanged(RatingControl sender, object args)
        {
            string score_str = sender.Value == 0 ? "-1" : sender.Value.ToString();

            DataAccess.Update.UpdateSingleDataFromVideoInfo(ResultInfo.trueName, "score", score_str);
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

            FileInfoInCidSmokePage = new FileInfoInCidSmoke(ResultInfo.trueName);
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
            ;

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
            var tupleResult = await SearchLinkPage.ShowInContentDialog(ResultInfo.trueName, XamlRoot);

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
}
