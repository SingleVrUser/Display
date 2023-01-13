using Data;
using Display.ContentsPage;
using Display.ContentsPage.DetailInfo;
using Microsoft.UI.Input;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation.Metadata;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System.Profile;
using static Display.ContentsPage.SpiderVideoInfo.FileStatistics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.Control
{
    public sealed partial class VideoDetails : UserControl
    {
        public VideoCoverDisplayClass resultinfo
        {
            get { return (VideoCoverDisplayClass)GetValue(resultinfoProperty); }
            set { SetValue(resultinfoProperty, value); }
        }

        public static readonly DependencyProperty resultinfoProperty =
            DependencyProperty.Register("resultinfo", typeof(string), typeof(VideoDetails), null);

        public VideoDetails()
        {
            this.InitializeComponent();
        }

        ObservableCollection<string> ShowImageList = new();

        private async void loadData()
        {
            if (resultinfo == null) return;

            //标题
            Title_TextBlock.Text = resultinfo.title;

            //演员
            //之前有数据，清空
            if (ActorSatckPanel.Children.Count != 0) ActorSatckPanel.Children.Clear();

            ////查询该视频对应的演员列表
            var actorList = await DataAccess.LoadActorInfoByVideoName(resultinfo.truename);

            for (var i = 0; i < actorList.Count; i++)
            {
                var actor = actorList[i];
                var actorImageControl = new Control.ActorImage(actor, resultinfo.releasetime);

                if(!string.IsNullOrEmpty(actor.name))
                    actorImageControl.Click += ActorButtonOnClick;

                ActorSatckPanel.Children.Insert(i, actorImageControl);
            }

            //标签
            //之前有数据，清空
            if ( CategorySatckPanel.Children.Count != 0) CategorySatckPanel.Children.Clear();
            var categoryList = resultinfo.category?.Split(",");
            for (var i = 0; i < categoryList?.Length; i++)
            {
                // 定义button
                var hyperButton = new HyperlinkButton() { FontFamily= new FontFamily("霞鹜文楷"),FontWeight = FontWeights.Light };
                hyperButton.Content = categoryList[i];
                hyperButton.Click += LabelButtonOnClick;

                // stackpanel内添加button
                CategorySatckPanel.Children.Insert(i, hyperButton);
            }

            //缩略图
            //检查缩略图是否存在
            List<string> ThumbnailList = new();

            //来源为本地
            if(AppSettings.ThumbnailOrigin == (int)AppSettings.Origin.Local)
            {
                string folderFullName = Path.Combine(AppSettings.Image_SavePath, resultinfo.truename);
                DirectoryInfo TheFolder = new DirectoryInfo(folderFullName);

                if (TheFolder.Exists)
                {
                    //文件
                    foreach (FileInfo NextFile in TheFolder.GetFiles())
                    {
                        if (NextFile.Name.Contains("Thumbnail_"))
                        {
                            ThumbnailList.Add(NextFile.FullName);
                        }
                    }
                }

            }
            //来源为网络
            else if (AppSettings.ThumbnailOrigin == (int)AppSettings.Origin.Web)
            {
                VideoInfo VideoInfo = DataAccess.LoadOneVideoInfoByCID(resultinfo.truename);

                var sampleImageListStr = VideoInfo.sampleImageList;
                if(!string.IsNullOrEmpty(sampleImageListStr))
                {
                    ThumbnailList = sampleImageListStr.Split(",").ToList();
                }
            }

            if(ThumbnailList.Count > 0)
            {
                ThumbnailList = ThumbnailList.OrderByNatural(emp => emp.ToString()).ToList();
                ThumbnailGridView.ItemsSource = ThumbnailList;
                ThumbnailStackPanel.Visibility = Visibility;
            }

        }

        private void GridlLoaded(object sender, RoutedEventArgs e)
        {
            loadData();
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
            string name = resultinfo.truename;
            var videoinfoList = DataAccess.loadFileInfoByTruename(name);

            ContentDialog dialog = new ContentDialog();

            // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
            dialog.XamlRoot = this.XamlRoot;
            dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
            dialog.Title = "下载";
            dialog.PrimaryButtonText = "下载全部";
            dialog.SecondaryButtonText = "下载选中项";
            dialog.CloseButtonText = "返回";
            dialog.DefaultButton = ContentDialogButton.Primary;

            videoinfoList = videoinfoList.OrderBy(item => item.n).ToList();

            var DownDialogContent = new ContentsPage.DetailInfo.DownDialogContent(videoinfoList);

            dialog.Content = DownDialogContent;
            var result = await dialog.ShowAsync();

            List<Datum> downVideoInfoList = new List<Datum>();

            WebApi webapi = new();

            //下载方式
            WebApi.downType downType;
            switch (DownDialogContent.DownMethod)
            {
                case "比特彗星":
                    downType = WebApi.downType.bc;
                    if (AppSettings.BitCometSettings == null)
                        ShowTeachingTip("还没有完成比特彗星的设置，请移步到“设置->下载方式->BitComet”完成相关配置");
                    break;
                case "aria2":
                    downType = WebApi.downType.aria2;
                    if (AppSettings.Aria2Settings == null)
                        ShowTeachingTip("还没有完成Aria2的设置，请移步到“设置->下载方式->Aria2”完成相关配置");
                    break;
                default:
                    downType = WebApi.downType._115;
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
                bool isOk = await webapi.RequestDown(videoinfoList, downType, savePath, topFolderName);

                if (isOk)
                    ShowTeachingTip("发送下载请求成功");
                else
                    ShowTeachingTip("发送下载请求失败");
            }

            //下载选中
            else if (result == ContentDialogResult.Secondary)
            {
                var stackPanel = DownDialogContent.Content as StackPanel;
                foreach (var item in stackPanel.Children)
                {
                    if (item is CheckBox)
                    {
                        CheckBox fileBox = item as CheckBox;
                        if (fileBox.IsChecked == true)
                        {
                            var selectVideoInfo = videoinfoList.Where(x => x.pc == fileBox.Name).First();
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

                bool isOk = await webapi.RequestDown(downVideoInfoList, downType, savePath, topFolderName);

                if (isOk)
                    ShowTeachingTip("发送下载请求成功");
                else
                    ShowTeachingTip("发送下载请求失败");
            }
            else
            {
                //DialogResult.Text = "User cancelled the dialog";
            }
        }

        private void updateLookLater(bool? val)
        {
            long lookLater_t = val == true ? DateTimeOffset.Now.ToUnixTimeSeconds() : 0;

            resultinfo.look_later = lookLater_t;
            DataAccess.UpdateSingleDataFromVideoInfo(resultinfo.truename, "look_later", lookLater_t.ToString());
        }

        private void updateLike(bool? val)
        {
            int is_like = val == true ? 1 : 0;

            resultinfo.is_like = is_like;
            DataAccess.UpdateSingleDataFromVideoInfo(resultinfo.truename, "is_like", is_like.ToString());
        }

        private void Animation_Completed(ConnectedAnimation sender, object args)
        {
            SmokeGrid.Visibility = Visibility.Collapsed;
            SmokeGrid.Children.Add(destinationImageElement);
            destinationImageElement.Visibility = Visibility.Collapsed;

            SmokeCancelGrid.Tapped -= SmokeCancelGrid_Tapped;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            SmokeGridCancel();
        }

        ContentsPage.DetailInfo.FindInfoAgainSmoke FindInfoAgainSmoke;
        private void FindAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            //第一次启动FindInfoAgainSmoke
            if(FindInfoAgainSmoke == null)
            {

                FindInfoAgainSmoke = new(resultinfo.truename);

                FindInfoAgainSmoke.ConfirmClick += FindInfoAgainSmoke_ConfirmClick;
            }

            if (!SmokeGrid.Children.Contains(FindInfoAgainSmoke))
            {
                SmokeGrid.Children.Add(FindInfoAgainSmoke);
                SmokeCancelGrid.Tapped += FindInfoAgainSmokeCancelGrid_Tapped;
            }

            SmokeGrid.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// 重新搜刮后选择了选项，并按下了确认键
        /// </summary>
        /// <param Name="sender"></param>
        /// <param Name="e"></param>
        private async void FindInfoAgainSmoke_ConfirmClick(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button button)) return;

            //获取最新的信息
            if(!(button.DataContext is VideoInfo videoInfo)) return;

            await UpdateInfo(videoInfo);

            //退出Smoke
            FindInfoAgainSmokeCancel();
        }

        private async Task UpdateInfo(VideoInfo videoInfo)
        {
            //重新下载图片
            if (!string.IsNullOrEmpty(videoInfo.imageurl))
            {
                await GetInfoFromNetwork.downloadFile(videoInfo.imageurl, Path.Combine(AppSettings.Image_SavePath, videoInfo.truename), videoInfo.truename, true);
            }

            //更新数据库
            DataAccess.UpdateDataFromVideoInfo(videoInfo);

            //更新ResultInfo数据
            foreach (var item in videoInfo.GetType().GetProperties())
            {
                var name = item.Name;
                //忽略自定义数据
                if (name == "look_later" || name == "score" || name == "is_like")
                    continue;

                var value = item.GetValue(videoInfo);

                var newItem = resultinfo.GetType().GetProperty(name);
                newItem.SetValue(resultinfo, value);
            }

            //图片地址不变，但内容变了
            //为了图片显示能够变化
            string oldPath = resultinfo.imagepath;
            string newPath = videoInfo.imagepath;
            //string NoPictruePath = "ms-appx:///Assets/NoPicture.jpg";
            if (!oldPath.Contains("ms-appx:") && File.Exists(newPath))
            {
                StorageFile file = await StorageFile.GetFileFromPathAsync(newPath);

                using (IRandomAccessStream fileStream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read))
                {
                    // Set the image source to the selected bitmap
                    BitmapImage bitmapImage = new BitmapImage();

                    await bitmapImage.SetSourceAsync(fileStream);
                    Cover_Image.Source = bitmapImage;
                }
            }


            //重新加载数据
            loadData();
        }

        private void FindInfoAgainSmokeCancelGrid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FindInfoAgainSmokeCancel();

            SmokeCancelGrid.Tapped -= FindInfoAgainSmokeCancelGrid_Tapped;
        }

        private void FindInfoAgainSmokeCancel()
        {
            SmokeGrid.Visibility = Visibility.Collapsed;

            if (SmokeGrid.Children.Contains(FindInfoAgainSmoke))
            {
                SmokeGrid.Children.Remove(FindInfoAgainSmoke);
            }
        }

        private async void SmokeGridCancel()
        {
            if (!destinationImageElement.IsLoaded) return;

            ConnectedAnimation animation = ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("backwardsAnimation", destinationImageElement);
            SmokeGrid.Children.Remove(destinationImageElement);

            // Collapse the smoke when the animation completes.
            animation.Completed += Animation_Completed;

            // If the connected item appears outside the viewport, scroll it into view.
            ThumbnailGridView.ScrollIntoView(_storedItem, ScrollIntoViewAlignment.Default);
            ThumbnailGridView.UpdateLayout();

            // Use the Direct configuration to go back (if the API is available). 
            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 7))
            {
                animation.Configuration = new DirectConnectedAnimationConfiguration();
            }

            // Play the second connected animation. 
            await ThumbnailGridView.TryStartConnectedAnimationAsync(animation, _storedItem, "Thumbnail_Image");

        }

        private string _storedItem;
        private void ThumbnailGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            ConnectedAnimation animation = null;

            if (ThumbnailGridView.ContainerFromItem(e.ClickedItem) is GridViewItem container)
            {
                _storedItem = container.Content as string;

                animation = ThumbnailGridView.PrepareConnectedAnimation("forwardAnimation", _storedItem, "Thumbnail_Image");
            }

            string iamgePath = e.ClickedItem as string;

            //之前未赋值
            if (ShowImageList.Count == 0)
            {
                var ThumbnailList = ThumbnailGridView.ItemsSource as List<string>;
                for (int i = 0; i < ThumbnailList.Count; i++)
                {
                    var image = ThumbnailList[i];

                    ShowImageList.Add(image);
                    if(image == iamgePath)
                    {
                        ShowImageFlipView.SelectedIndex = i;
                    }

                }
            }
            //之前已赋值
            else
            {
                var index = ShowImageList.IndexOf(iamgePath);
                ShowImageFlipView.SelectedIndex = index;
            }

            ShoeImageName.Text = GetFileIndex();

            SmokeGrid.Visibility = Visibility.Visible;
            destinationImageElement.Visibility = Visibility.Visible;

            animation.Completed += Animation_Completed1;

            animation.TryStart(destinationImageElement);

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
            return $"{ShowImageFlipView.SelectedIndex+1}/{ShowImageList.Count}";
        }

        public event RoutedEventHandler DeleteClick;
        private void DeletedAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            DeleteClick?.Invoke(sender, e);
        }

        private async void EditAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            var EditPage = new EditInfo(resultinfo);
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

                    if(newItem!= null)
                    {
                        newItem.SetValue(newInfo, newValue);
                    }

                    //原先的旧值
                    var oldItem = resultinfo.GetType().GetProperty(name);
                    var oldValue = oldItem.GetValue(resultinfo);

                    //与新值比较，判断是否需要更新正在显示的ResultInfo数据
                    if (newValue != oldValue)
                    {
                        oldItem.SetValue(resultinfo, newValue);
                    }
                }

                await UpdateInfo(newInfo);
            }
        }

        private void OpenDirectory_Click(object sender, RoutedEventArgs e)
        {
            string ImagePath = Path.GetDirectoryName(resultinfo.imagepath);
            FileMatch.LaunchFolder(ImagePath);
        }

        private void ShowImageFlipView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ShoeImageName.Text = GetFileIndex();
        }

        private void RatingControl_ValueChanged(RatingControl sender, object args)
        {
            string score_str = sender.Value == 0 ? "-1" : sender.Value.ToString();

            DataAccess.UpdateSingleDataFromVideoInfo(resultinfo.truename, "score", score_str);
        }

        private void ShowTeachingTip(string subtitle, string content = null)
        {
            LightDismissTeachingTip.Subtitle = subtitle;
            if (content != null)
                LightDismissTeachingTip.Content = content;

            LightDismissTeachingTip.IsOpen = true;
        }

        ContentsPage.DetailInfo.FileInfoInCidSmoke FileInfoInCidSmokePage;
        private void MoreInfoAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            SmokeGrid.Visibility = Visibility.Visible;

            FileInfoInCidSmokePage = new(resultinfo.truename);
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

            SmokeCancelGrid.Tapped -= FileInfoInCidSmokeCancelGrid_Tapped; ;

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
            Cover_Grid.PointerEntered += Cover_Image_PointerEntered;
            Cover_Grid.PointerExited += Cover_Image_PointerExited;
            Cover_Grid.Tapped += Cover_Tapped;
        }


    }
}
