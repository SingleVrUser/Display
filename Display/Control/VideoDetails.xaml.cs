using Data;
using Display.Views;
using Microsoft.UI.Input;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.UI.Text;

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

        private void loadData()
        {
            if (resultinfo == null) return;

            //标题
            Title_TextBlock.Text = resultinfo.title;

            //演员
            var actorList = resultinfo.actor.Split(',');
            for (var i = 0; i < actorList.Length; i++)
            {
                //// 定义button
                //var hyperButton = new HyperlinkButton();
                //hyperButton.Content = actorList[i];
                //hyperButton.Click += ActorButtonOnClick;

                //// stackpanel内添加button
                //ActorSatckPanel.Children.Insert(i, hyperButton);

                var actorImageControl = new Control.ActorImage(actorList[i]);
                actorImageControl.Click += ActorButtonOnClick;

                ActorSatckPanel.Children.Insert(i, actorImageControl);
            }

            //标签
            var categoryList = resultinfo.category.Split(",");
            for (var i = 0; i < categoryList.Length; i++)
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

                ThumbnailList = VideoInfo.sampleImageList.Split(",").ToList();

            }

            if(ThumbnailList.Count > 0)
            {
                ThumbnailList = ThumbnailList.OrderByNatural(emp => emp.ToString()).ToList();
                ThumbnailGridView.ItemsSource = ThumbnailList;
                ThumbnailGridView.Visibility = Visibility;
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

        //点击了多集中的具体集数
        public event ItemClickEventHandler MultisetListClick;
        private void StationsList_OnItemClick(object sender, ItemClickEventArgs e)
        {
            MultisetListClick?.Invoke(sender, e);
        }

        private async void DownButton_Click(object sender, RoutedEventArgs e)
        {
            string name = resultinfo.truename;
            var videoinfoList = DataAccess.loadVideoInfoByTruename(name);

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

            var DownDialogContent = new ContentsPage.DownDialogContent(videoinfoList);

            dialog.Content = DownDialogContent;
            var result = await dialog.ShowAsync();

            List<Datum> downVideoInfoList = new List<Datum>();

            WebApi webapi = new();

            //下载全部
            if (result == ContentDialogResult.Primary)
            {
                webapi.RequestDown(videoinfoList);
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
                webapi.RequestDown(downVideoInfoList);
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
            SmokeGrid.Children.Add(destinationElement);

            SmokeCancelGrid.Tapped -= SmokeCancelGrid_Tapped;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            SmokeGridCancel();
        }

        private async void SmokeGridCancel()
        {
            if (!destinationElement.IsLoaded) return;
            ConnectedAnimation animation = ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("backwardsAnimation", destinationElement);
            SmokeGrid.Children.Remove(destinationElement);

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

            // Get the collection item corresponding to the clicked item.
            if (ThumbnailGridView.ContainerFromItem(e.ClickedItem) is GridViewItem container)
            {
                // Stash the clicked item for use later. We'll need it when we connect back from the detailpage.
                _storedItem = container.Content as string;

                // Prepare the connected animation.
                // Notice that the stored item is passed in, as well as the name of the connected element. 
                // The animation will actually start on the Detailed info page.
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

            //ShowImage.Source = new BitmapImage(new Uri(iamgePath));

            //ShoeImageName.Text = Path.GetFileName(iamgePath);
            SmokeGrid.Visibility = Visibility.Visible;
            animation.Completed += Animation_Completed1;

            animation.TryStart(destinationElement);

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

        //private void ActorScrollViewer_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        //{
        //    var scv = (ScrollViewer)sender;
        //    scv.ScrollToHorizontalOffset(scv.HorizontalOffset);
        //    e.Handled = true;
        //}

        //private void CategoryScrollViewer_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        //{
        //    var scv = (ScrollViewer)sender;
        //    scv.ScrollToHorizontalOffset(scv.HorizontalOffset);
        //    e.Handled = true;
        //}

        private string GetFileNameFromFullPath(object fullpath)
        {
            return Path.GetFileName(fullpath as string);
        }

        public event RoutedEventHandler DeleteClick;
        private void DeletedAppBarButton_Click(object sender, RoutedEventArgs e)
        {

            DeleteClick?.Invoke(sender, e);
            
        }

        private void OpenDirectory_Click(object sender, RoutedEventArgs e)
        {
            string ImagePath = Path.GetDirectoryName(resultinfo.imagepath);
            FileMatch.LaunchFolder(ImagePath);
        }
    }
}
