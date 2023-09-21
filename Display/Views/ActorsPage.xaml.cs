using Display.Data;
using Display.Models.IncrementalCollection;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using SharpCompress;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ActorsPage : Page
    {
        IncrementalLoadActorInfoCollection actorinfo;
        ObservableCollection<ActorInfo> actorPartInfo = new();

        public static ActorsPage Current;

        //过渡动画用
        private enum navigationAnimationType { image, gridView };
        private navigationAnimationType _navigationType;
        private ActorInfo _storeditem;
        private Image _storedimage;

        public ActorsPage()
        {
            this.InitializeComponent();

            Current = this;

            Page_Loaded();
        }

        private async void Page_Loaded()
        {
            ProgressRing.IsActive = true;

            actorinfo = new IncrementalLoadActorInfoCollection(new Dictionary<string, bool> { { "is_like", true }, { "prifile_path", true } });
            BasicGridView.ItemsSource = actorinfo;
            await actorinfo.LoadData();

            TotalCount_TextBlock.Text = actorinfo.AllCount.ToString();

            LoadActorPartInfo();

            //上次获取信息是否已经完成
            if (AppSettings.GetActorInfoLastIndex == -1)
            {
                GetActorInfoButton.Visibility = Visibility.Visible;
            }
            else
            {
                ContinueGetActorInfoTaskButton.Visibility = Visibility.Visible;
            }

            ProgressRing.IsActive = false;
        }
        
        private async void LoadActorPartInfo(int count = 30)
        {
            var infos = await DataAccess.Get.GetActorInfo(count, 0, orderByList: new() { { "RANDOM()", false } },
                                                                filterList: new() { "prifile_path != ''" });

            infos?.ForEach(info => actorPartInfo.Add(info));

        }

        /// <summary>
        /// 跳转至演员详情页
        /// </summary>
        /// <param Name="sender"></param>
        /// <param Name="e"></param>
        private void BasicGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (BasicGridView.ContainerFromItem(e.ClickedItem) is GridViewItem container)
            {
                var actorinfo = container.Content as ActorInfo;

                _navigationType = navigationAnimationType.gridView;
                _storeditem = actorinfo;
                //BasicGridView.PrepareConnectedAnimation("ForwardConnectedAnimation", _storeditem, "ActorImage");

                GoToActorInfo(actorinfo);
            }
        }

        private void GoToActorInfo(ActorInfo actorinfo)
        {
            Tuple<List<string>, string, bool> TypeAndName = new(new() { "actor" }, actorinfo.Name, false);
            Frame.Navigate(typeof(ActorInfoPage), TypeAndName);
        }

        private void Grid_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Hand);
        }

        private void Grid_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Arrow);
        }

        private void UpdateRandomCover_Click(object sender, RoutedEventArgs e)
        {
            actorPartInfo.Clear();
            LoadActorPartInfo();

            CarouselControl.SelectedIndex = actorPartInfo.Count / 2;
        }

        private void Image_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (!(sender is Image image)) return;

            _navigationType = navigationAnimationType.image;
            _storedimage = image;

            ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("ForwardConnectedAnimation", _storedimage);

            if (image.DataContext is ActorInfo actorinfo)
            {
                if (isActorInfoCurrentSelected(actorinfo))
                    GoToActorInfo(actorinfo);
            }
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // 过渡动画
            ConnectedAnimation animation = ConnectedAnimationService.GetForCurrentView().GetAnimation("BackConnectedAnimation");
            if (animation != null)
            {
                if (_navigationType == navigationAnimationType.image)
                {
                    if (_storedimage != null)
                    {
                        animation.TryStart(_storedimage);
                    }
                }
                else if (_navigationType == navigationAnimationType.gridView)
                {
                    //开始动画
                    if (_storeditem != null)
                    {
                        //开始动画
                        await BasicGridView.TryStartConnectedAnimationAsync(animation, _storeditem, "ActorImage");
                    }
                }
            }
        }

        private void Img_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (!(sender is Image image)) return;

            if (image.DataContext is ActorInfo actorinfo)
            {
                if (isActorInfoCurrentSelected(actorinfo))
                    ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Hand);
            }
        }

        private void Img_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (!(sender is Image image)) return;

            if (image.DataContext is ActorInfo actorinfo)
            {
                if (isActorInfoCurrentSelected(actorinfo))
                    ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Arrow);
            }
        }

        private bool isActorInfoCurrentSelected(ActorInfo actorinfo)
        {
            if ((CarouselControl.SelectedItem == null && CarouselControl.SelectedIndex == 8) || CarouselControl.SelectedItem != null && CarouselControl.SelectedItem == actorinfo)
                return true;
            else
                return false;
        }

        private async void GetActorInfoButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button) return;
            button.IsEnabled = false;

            string baseUrl = AppSettings.MinnanoAvBaseUrl;

            //检查搜刮页是否可用
            bool canUse = await GetInfoFromNetwork.CheckUrlUseful(baseUrl);

            if (!canUse)
            {
                button.IsEnabled = true;
                ContentDialog dialog = new ContentDialog()
                {
                    XamlRoot = XamlRoot,
                    Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                    Title = "访问出错",
                    Content = $"{baseUrl} 不可访问，请检查网络设置",
                    CloseButtonText = "返回"
                };
                await dialog.ShowAsync();
                return;
            }

            var infos = await DataAccess.Get.GetActorInfo(-1);

            int allCount = infos.Length;
            if (allCount == 0) return;

            if (!Notifications.ToastGetActorInfoWithProgressBar.SendToast(allCount)) return;

            //创建断点续传文件
            string savePath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "ActorInfo");
            if (!Directory.Exists(savePath)) Directory.CreateDirectory(savePath);
            StorageFolder storageFolder = await StorageFolder.GetFolderFromPathAsync(savePath);
            StorageFile sampleFile = await storageFolder.CreateFileAsync("getting.json", CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(sampleFile, JsonSerializer.Serialize(infos));

            await GetActorsInfo(infos);

            button.IsEnabled = true;
        }
        private async void ContinueGetActorInfoTaskButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button) return;
            button.IsEnabled = false;

            //反序列化
            var filePath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "ActorInfo", "getting.json");
            if (File.Exists(filePath))
            {
                string jsonString = await File.ReadAllTextAsync(filePath);
                var infos = JsonSerializer.Deserialize<ActorInfo[]>(jsonString);

                await GetActorsInfo(infos, AppSettings.GetActorInfoLastIndex);

                //删除文件
                File.Delete(filePath);

            }
            //文件不存在，重新开始
            else
            {
                AppSettings.GetActorInfoLastIndex = -1;
            }


            button.Visibility = Visibility.Collapsed;
            GetActorInfoButton.Visibility = Visibility.Visible;

        }

        private static async Task GetActorsInfo(ActorInfo[] infos, int startIndex = 0)
        {
            int allCount = infos.Length;
            if (allCount == 0) return;

            if (!Notifications.ToastGetActorInfoWithProgressBar.SendToast(startIndex, allCount)) return;

            bool isStart = false;
            for (int i = 0; i < allCount; i++)
            {
                if (!isStart)
                {
                    if (i == startIndex) isStart = true;
                    else continue;
                }

                //记录当前索引，用于续传
                AppSettings.GetActorInfoLastIndex = i;

                var info = infos[i];

                //有数据说明已经搜索过了
                if (!string.IsNullOrEmpty(info.Bwh))
                {
                    System.Diagnostics.Debug.WriteLine($"{i} 已经搜索过了");
                    await Notifications.ToastGetActorInfoWithProgressBar.AddValue(i + 1, allCount);
                    continue;
                }

                await GetActorInfo(info);

                await Notifications.ToastGetActorInfoWithProgressBar.AddValue(i + 1, allCount);

                //等待1~2秒
                await GetInfoFromNetwork.RandomTimeDelay(1, 2);
            }

            //获取完成，初始化续传索引
            AppSettings.GetActorInfoLastIndex = -1;
        }

        public static async Task<ActorInfo> GetActorInfo(ActorInfo info)
        {
            var actorName = info.Name;

            //跳过无效名称
            if (string.IsNullOrEmpty(actorName)) return null;

            //含有数字的一般搜索不到，跳过
            if (Regex.Match(actorName, @"\d+").Success) return null;

            //从网站中获取信息
            var actorinfo = await GetActorInfoFromNetwork.SearchInfoFromMinnanoAv(actorName);
            if (actorinfo == null) return null;

            var actorId = DataAccess.Get.GetIdInActor_Names(actorName);
            if (actorId == -1) return null;

            DataAccess.Update.UpdateActorInfo(actorId, actorinfo);

            //获取到的信息有头像
            if (!string.IsNullOrEmpty(actorinfo.ImageUrl))
            {
                //查询本地数据库中的数据
                var actorInfos = await DataAccess.Get.GetActorInfo(1, filterList: new List<string> { $"id == '{actorId}'" });

                if (actorInfos != null && actorInfos.Length != 0)
                {
                    var firstOrDefault = actorInfos.FirstOrDefault();

                    //数据库中无头像
                    if (firstOrDefault is { ProfilePath: Const.FileType.NoPicturePath })
                    {
                        var filePath = Path.Combine(AppSettings.ActorInfoSavePath, actorName);

                        Uri infoUri = new(actorinfo.InfoUrl);

                        var prifilePath = await GetInfoFromNetwork.DownloadFile(actorinfo.ImageUrl, filePath, "face", headers: new()
                        {
                            {"Host",infoUri.Host },
                            {"Referer", actorinfo.InfoUrl }
                        });

                        //更新头像
                        DataAccess.Update.UpdateActorInfoProfilePath(actorId, prifilePath);

                        actorinfo.ProfilePath = prifilePath;
                    }
                    //数据库中有头像
                    else if (firstOrDefault != null)
                    {
                         actorinfo.ProfilePath = firstOrDefault.ProfilePath;
                    }

                }

            }

            //更新别名
            //别名
            if (actorinfo.OtherNames != null && actorinfo.OtherNames.Count > 0)
            {
                foreach (var otherName in actorinfo.OtherNames)
                {
                    DataAccess.Add.AddOrIgnoreActor_Names(actorId, otherName);
                }
            }

            //更新bwh
            if (!string.IsNullOrEmpty(actorinfo.Bwh))
            {
                DataAccess.Add.AddOrIgnoreBwh(actorinfo.Bwh, actorinfo.Bust, actorinfo.Waist, actorinfo.Hips);
            }

            return actorinfo;
        }

        public void ShowButtonWithShowToastAgain()
        {
            if (DispatcherQueue.HasThreadAccess)
            {
                ShowProgressButton.Visibility = Visibility.Visible;
            }
            else
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    ShowProgressButton.Visibility = Visibility.Visible;
                });
            }
        }

        private void ShowProgressButton_Click(object sender, RoutedEventArgs e)
        {
            Notifications.ToastGetActorInfoWithProgressBar.SendToast();

            //点击后隐藏
            ShowProgressButton.Visibility = Visibility.Collapsed;
        }
    }

}
