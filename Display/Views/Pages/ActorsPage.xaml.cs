using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;
using DataAccess.Dao.Interface;
using DataAccess.Models.Entity;
using Display.Helper.Network;
using Display.Helper.Notifications;
using Display.Models.Vo.IncrementalCollection;
using Display.Providers;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Newtonsoft.Json;

namespace Display.Views.Pages;

public sealed partial class ActorsPage
{
    private IncrementalLoadActorInfoCollection _actorInfo;
    private readonly ObservableCollection<ActorInfo> _actorPartInfo = [];

    private static readonly IActorInfoDao ActorInfoDao = App.GetService<IActorInfoDao>();
    
    public static ActorsPage Current;

    //过渡动画用
    private enum NavigationAnimationType { Image, GridView };
    private NavigationAnimationType _navigationType;
    private ActorInfo _storedItem;
    private Image _storedImage;

    public ActorsPage()
    {
        InitializeComponent();

        Current = this;

        Page_Loaded();
    }

    private async void Page_Loaded()
    {
        ProgressRing.IsActive = true;

        _actorInfo = new IncrementalLoadActorInfoCollection(new Dictionary<string, bool> { { "is_like", true }, { "prifile_path", true } });
        BasicGridView.ItemsSource = _actorInfo;
        await _actorInfo.LoadDataAsync();

        TotalCountTextBlock.Text = _actorInfo.AllCount.ToString();

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

    private void LoadActorPartInfo(int count = 14)
    {
        var infos = ActorInfoDao.List(0, count);

        infos.ForEach(_actorPartInfo.Add);

        CarouselControl.SelectedIndex = _actorPartInfo.Count / 2;

    }

    /// <summary>
    /// 跳转至演员详情页
    /// </summary>
    /// <param name="sender"></param>
    /// <param name ="e"></param>
    private void BasicGridView_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (BasicGridView.ContainerFromItem(e.ClickedItem) is not GridViewItem container) return;

        var actorInfo = container.Content as ActorInfo;

        _navigationType = NavigationAnimationType.GridView;
        _storedItem = actorInfo;

        GoToActorInfo(actorInfo);
    }

    private void GoToActorInfo(ActorInfo actorInfo)
    {
        Tuple<List<string>, string, bool> typeAndName = new(["actor"], actorInfo.Name, false);
        Frame.Navigate(typeof(VideoCoverPage), typeAndName);
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
        _actorPartInfo.Clear();
        LoadActorPartInfo();

        CarouselControl.SelectedIndex = _actorPartInfo.Count / 2;
    }

    private void Image_Tapped(object sender, TappedRoutedEventArgs e)
    {
        if (sender is not Image image) return;

        _navigationType = NavigationAnimationType.Image;
        _storedImage = image;

        ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("ForwardConnectedAnimation", _storedImage);

        if (image.DataContext is ActorInfo actorInfo)
        {
            if (IsActorInfoCurrentSelected(actorInfo))
                GoToActorInfo(actorInfo);
        }
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        // 过渡动画
        var animation = ConnectedAnimationService.GetForCurrentView().GetAnimation("BackConnectedAnimation");
        if (animation == null) return;

        if (_navigationType == NavigationAnimationType.Image)
        {
            if (_storedImage != null)
            {
                animation.TryStart(_storedImage);
            }
        }
        else if (_navigationType == NavigationAnimationType.GridView)
        {
            //开始动画
            if (_storedItem != null)
            {
                //开始动画
                await BasicGridView.TryStartConnectedAnimationAsync(animation, _storedItem, "ActorImage");
            }
        }
    }

    private void Img_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        if (sender is not Image image) return;

        if (image.DataContext is ActorInfo actorInfo)
        {
            if (IsActorInfoCurrentSelected(actorInfo))
                ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Hand);
        }
    }

    private void Img_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        if (sender is not Image image) return;

        if (image.DataContext is ActorInfo actorInfo)
        {
            if (IsActorInfoCurrentSelected(actorInfo))
                ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Arrow);
        }
    }

    private bool IsActorInfoCurrentSelected(ActorInfo actorInfo)
    {
        return (CarouselControl.SelectedItem == null && CarouselControl.SelectedIndex == 8)
               || CarouselControl.SelectedItem != null && CarouselControl.SelectedItem == actorInfo;
    }

    private async void GetActorInfoButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button) return;
        button.IsEnabled = false;

        var baseUrl = AppSettings.MinnanoAvBaseUrl;

        //检查搜刮页是否可用
        var canUse = await NetworkHelper.CheckUrlUseful(baseUrl);

        if (!canUse)
        {
            button.IsEnabled = true;
            var dialog = new ContentDialog
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
        
        // TODO 获取演员信息
        //
        // var infos = await DataAccessLocal.Get.GetActorInfoAsync(-1);
        //
        // var allCount = infos.Length;
        // if (allCount == 0) return;
        //
        // if (!ToastGetActorInfoWithProgressBar.SendToast(allCount)) return;
        //
        // //创建断点续传文件
        // var savePath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "ActorInfo");
        // if (!Directory.Exists(savePath)) Directory.CreateDirectory(savePath);
        // var storageFolder = await StorageFolder.GetFolderFromPathAsync(savePath);
        // var sampleFile = await storageFolder.CreateFileAsync("getting.json", CreationCollisionOption.ReplaceExisting);
        // await FileIO.WriteTextAsync(sampleFile, JsonConvert.SerializeObject(infos));
        //
        // await GetActorsInfo(infos);
        //
        // button.IsEnabled = true;
    }
    private async void ContinueGetActorInfoTaskButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button) return;
        button.IsEnabled = false;

        //反序列化
        var filePath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "ActorInfo", "getting.json");
        if (File.Exists(filePath))
        {
            var jsonString = await File.ReadAllTextAsync(filePath);

            var infos = JsonConvert.DeserializeObject<ActorInfo[]>(jsonString);

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

    private async Task GetActorsInfo(ActorInfo[] infos, int startIndex = 0)
    {
        var allCount = infos.Length;
        if (allCount == 0) return;

        if (!ToastGetActorInfoWithProgressBar.SendToast(startIndex, allCount)) return;

        var isStart = false;
        for (var i = 0; i < allCount; i++)
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
            if (info.Bwh != null)
            {
                System.Diagnostics.Debug.WriteLine($"{i} 已经搜索过了");
                await ToastGetActorInfoWithProgressBar.AddValue(i + 1, allCount);
                continue;
            }

            // await UpdateActorInfo(info);
            // TODO 更新演员信息

            await ToastGetActorInfoWithProgressBar.AddValue(i + 1, allCount);

            //等待1~2秒
            await NetworkHelper.RandomTimeDelay(1, 2);
        }

        //获取完成，初始化续传索引
        AppSettings.GetActorInfoLastIndex = -1;
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
        ToastGetActorInfoWithProgressBar.SendToast();

        //点击后隐藏
        ShowProgressButton.Visibility = Visibility.Collapsed;
    }
}