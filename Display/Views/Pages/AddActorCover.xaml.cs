using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Windows.Foundation.Metadata;
using Display.Helper.Date;
using Display.Helper.Network;
using Display.Models.Data.IncrementalCollection;
using Display.Models.Entities.OneOneFive;
using Display.Models.Enums;
using Display.Providers;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static System.String;


namespace Display.Views.Pages;

public sealed partial class AddActorCover
{
    private IncrementalLoadActorInfoCollection _actorInfo;

    private readonly ObservableCollection<string> _showImageList = [];

    private readonly ObservableCollection<string> _failList = [];

    private bool _isCheckGit = true;

    private readonly CancellationTokenSource _sCts = new();

    public AddActorCover()
    {
        InitializeComponent();
    }

    private async void Grid_Loaded(object sender, RoutedEventArgs e)
    {
        Loaded -= Grid_Loaded;

        _actorInfo = new IncrementalLoadActorInfoCollection(new Dictionary<string, bool> { { "prifile_path", false } });
        _actorInfo.SetFilter(["Name != ''"]);
        await _actorInfo.LoadData();
        BasicGridView.ItemsSource = _actorInfo;

    }

    private ActorInfo _storedItem;
    private async void BasicGridView_ItemClick(object sender, ItemClickEventArgs e)
    {
        ConnectedAnimation animation = null;

        if (BasicGridView.ContainerFromItem(e.ClickedItem) is GridViewItem container)
        {
            _storedItem = container.Content as ActorInfo;
            animation = BasicGridView.PrepareConnectedAnimation("forwardAnimation", _storedItem, "connectedElement");

            animation.Completed += Animation_Completed1;
        }

        if (e.ClickedItem is not ActorInfo actorInfo) return;

        var filePath = AppSettings.ActorFileTreeSavePath;

        if (_isCheckGit)
        {
            progress_TextBlock.Text = "正在获取GFriend仓库信息……";
            if (!await TryUpdateFileTree(filePath))
            {
                progress_TextBlock.Text = "获取GFriend仓库信息失败!";
                return;
            }
            progress_TextBlock.Text = "获取仓库信息成功";

            _isCheckGit = false;
        }

        var imageUrlList = GetImageUrlListFromFileTreeAsync(actorInfo.Name, filePath: filePath);

        _showImageList.Clear();

        foreach (var imagePath in imageUrlList)
        {
            _showImageList.Add(imagePath);
        }

        ShoeActorName.Text = actorInfo.Name;
        SmokeGrid.Visibility = Visibility.Visible;

        animation?.TryStart(destinationElement);
        progress_TextBlock.Text = Empty;
    }

    //防止动画开始时，双击触发退出事件
    private void Animation_Completed1(ConnectedAnimation sender, object args)
    {
        SmokeCancelGrid.Tapped += SmokeCancelGrid_Tapped;
    }


    //JObject json;
    private List<string> GetImageUrlListFromFileTreeAsync(string actorName, int maxCount = -1, string filePath = null)
    {
        if (IsNullOrEmpty(filePath)) filePath = AppSettings.ActorFileTreeSavePath;

        return GetImageUrlFormFileTree(filePath, actorName, maxCount);
    }

    private void CheckBox_Checked(object sender, RoutedEventArgs e)
    {
        BasicGridView.SelectAll();
    }

    private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
        BasicGridView.SelectedItems.Clear();
    }

    private void BasicGridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var selectedItemCount = BasicGridView.SelectedItems.Count;

        selectedCheckBox.Content = $"共选 {selectedItemCount} 项";
    }

    private async void StartButton_Click(object sender, RoutedEventArgs e)
    {
        if (BasicGridView.SelectedItems.Count == 0) return;

        _actorInfo.HasMoreItems = false;

        UpdateGridViewShow();

        var startTime = DateTimeOffset.Now;

        //进度
        var progress = new Progress<ProgressClass>(info =>
        {
            if (!progress_TextBlock.IsLoaded)
            {
                _sCts.Cancel();
                return;
            }

            progress_TextBlock.Text = info.Text;

            if (info.Index == -1)
            {
                return;
            }

            var item = _actorInfo[info.Index];

            item.Status = info.Status;

            if (item.Status == Status.Error)
            {
                _failList.Add(item.Name);
            }

            if (!IsNullOrEmpty(info.ImagePath))
            {
                item.ProfilePath = info.ImagePath;
            }

            //完成
            if (item.Status != Status.Doing && _actorInfo.Count == info.Index + 1)
            {
                progress_TextBlock.Text = $"任务已完成，耗时{DateHelper.ConvertDoubleToLengthStr((DateTimeOffset.Now - startTime).TotalSeconds)}";
            }
        });

        await Task.Run(() => GetActorCoverByGit([.. _actorInfo], progress, _sCts));

        BasicGridView.ItemClick += BasicGridView_ItemClick;

    }

    private async Task GetActorCoverByGit(IReadOnlyList<ActorInfo> actorInfos, IProgress<ProgressClass> progress, CancellationTokenSource cts)
    {
        var filePath = AppSettings.ActorFileTreeSavePath;

        if (_isCheckGit)
        {
            progress.Report(new ProgressClass() { Text = "正在获取GFriend仓库信息……" });

            if (!await TryUpdateFileTree(filePath))
            {
                progress.Report(new ProgressClass() { Text = "获取GFriend仓库信息失败!" });
                return;
            }

            _isCheckGit = false;
        }

        for (var i = 0; i < actorInfos.Count; i++)
        {
            if (cts.IsCancellationRequested)
            {
                break;
            }

            ProgressClass progressInfo = new()
            {
                Index = i,
                Status = Status.Doing
            };

            var item = actorInfos[i];

            //演员名
            var actorName = item.Name;

            if (actorName.Contains("?")) continue;

            var indexText = $"【{i + 1}/{_actorInfo.Count}】 ";

            progressInfo.Text = $"{indexText}{actorName}";

            progress.Report(progressInfo);

            var imageSavePath = Path.Combine(AppSettings.ActorInfoSavePath, actorName, "face.jpg");

            if (!File.Exists(imageSavePath))
            {
                var imageUrl = Empty;
                var imageUrlList = GetImageUrlListFromFileTreeAsync(actorName, 1);
                if (imageUrlList.Count > 0) imageUrl = imageUrlList[0];

                if (IsNullOrEmpty(imageUrl))
                {
                    progressInfo.Status = Status.Error;
                    progress.Report(progressInfo);
                    continue;
                }

                var downResult = await DownFile(imageUrl, imageSavePath);

                if (!downResult)
                {
                    progressInfo.Status = Status.Error;
                    progress.Report(progressInfo);
                    continue;
                }
            }

            progressInfo.ImagePath = imageSavePath;
            progressInfo.Status = Status.BeforeStart;
            progress.Report(progressInfo);

            var actorId = DataAccess.Get.GetIdInActor_Names(actorName);
            if (actorId != -1)
            {
                //更新数据库
                DataAccess.Update.UpdateActorInfoProfilePath(actorId, imageSavePath);
            }
        }

    }

    private static List<string> GetImageUrlFormFileTree(string filePath, string actorName, int maxCount)
    {
        List<string> urlList = [];

        var key = Empty;
        var path = Empty;

        foreach (var line in File.ReadLines(filePath))
        {
            var content = line.Trim();

            if (IsNullOrEmpty(key))
            {
                var matchKey = Regex.Match(content, "\"(.*)\":");

                if (matchKey.Success)
                {
                    key = matchKey.Groups[1].Value;
                }
            }
            else
            {
                if (IsNullOrEmpty(path))
                {
                    var matchPath = Regex.Match(content, "\"(.*)\":");

                    if (matchPath.Success)
                    {
                        path = matchPath.Groups[1].Value;
                    }
                    else
                    {
                        if (content.Contains('}'))
                        {
                            key = Empty;
                        }
                    }
                }
                else
                {
                    var matchName = Regex.Match(content, "\"(.*)\":\"(.*)\"");

                    if (matchName.Success)
                    {
                        var name = matchName.Groups[1].Value;
                        var url = matchName.Groups[2].Value;

                        if (!name.Contains($"{actorName}.")) continue;

                        var imageUrl = $"https://raw.githubusercontent.com/gfriends/gfriends/master/{key}/{path}/{url}";
                        if (urlList.Contains(imageUrl)) continue;

                        urlList.Add(imageUrl);

                        if (urlList.Count == maxCount)
                        {
                            break;
                        }

                    }
                    else
                    {
                        if (content.Contains('}'))
                        {
                            path = Empty;
                        }
                    }
                }
            }
        }
        return urlList;
    }

    private async Task<bool> TryUpdateFileTree(string filePath)
    {
        var isNeedDownFile = false;

        if (!File.Exists(filePath))
        {
            isNeedDownFile = true;
        }
        else
        {
            //本地文件信息
            var fileWriteTime = File.GetLastWriteTime(filePath);

            //仓库信息
            var dateStr = await GetGitUpdateDateStr();

            //获取到最新时间
            if (!IsNullOrEmpty(dateStr))
            {
                var gitUpdateDate = Convert.ToDateTime(dateStr);

                if (gitUpdateDate > fileWriteTime)
                {
                    isNeedDownFile = true;
                }
            }
        }

        if (!isNeedDownFile) return true;

        const string filetreeDownUrl = @"https://raw.githubusercontent.com/gfriends/gfriends/master/Filetree.json";

        return await DownFile(filetreeDownUrl, filePath, true);
    }

    /// <summary>
    /// 获取仓库最新的时间
    /// </summary>
    /// <returns></returns>
    private async Task<string> GetGitUpdateDateStr()
    {
        var client = NetworkHelper.CommonClient;

        const string gitInfoUrl = @"https://api.github.com/repos/gfriends/gfriends";

        var updateDateStr = Empty;

        HttpResponseMessage resp;
        string strResult;
        try
        {
            resp = await client.GetAsync(gitInfoUrl);
            strResult = await resp.Content.ReadAsStringAsync();
        }
        catch (HttpRequestException)
        {
            return updateDateStr;
        }

        if (!resp.IsSuccessStatusCode) return updateDateStr;
        var json = JsonConvert.DeserializeObject<JObject>(strResult);
        updateDateStr = json["updated_at"]?.ToString();

        return updateDateStr;
    }

    HttpClient _client;
    private async Task<bool> DownFile(string url, string filePath, bool isNeedReplace = false)
    {
        if (IsNullOrEmpty(filePath)) return false;

        _client ??= NetworkHelper.CommonClient;

        var directoryName = Path.GetDirectoryName(filePath);
        if (!IsNullOrEmpty(directoryName) && !File.Exists(directoryName))
        {
            Directory.CreateDirectory(directoryName);
        }

        if (File.Exists(filePath) && !isNeedReplace) return true;

        try
        {
            var rep = await _client.GetAsync(HttpUtility.UrlPathEncode(url));
            if (rep.IsSuccessStatusCode)
            {
                await using var stream = await rep.Content.ReadAsStreamAsync();
                await using var newStream = File.Create(filePath);
                await stream.CopyToAsync(newStream);

                return true;
            }
        }
        catch
        {
            // ignore
        }

        return false;
    }

    /// <summary>
    /// 更新GridView显示:
    /// 只保留选中项
    /// 退出选择模式
    /// </summary>
    private void UpdateGridViewShow()
    {
        List<ActorInfo> actorList = [];

        actorList.AddRange(BasicGridView.SelectedItems.Cast<ActorInfo>());

        _actorInfo.Clear();
        foreach (var actor in actorList)
        {
            _actorInfo.Add(actor);
        }

        BasicGridView.SelectionMode = ListViewSelectionMode.None;

        selectedCheckBox.Visibility = Visibility.Collapsed;

        StartButton.Visibility = Visibility.Collapsed;

        modifyToggleSwitch.IsEnabled = false;
    }

    private async void CancelSmokeGrid()
    {
        if (!destinationElement.IsLoaded) return;

        var animation = ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("backwardsAnimation", destinationElement);
        SmokeGrid.Children.Remove(destinationElement);

        // Collapse the smoke when the animation completes.
        animation.Completed += Animation_Completed;

        // If the connected item appears outside the viewport, scroll it into view.
        BasicGridView.ScrollIntoView(_storedItem, ScrollIntoViewAlignment.Default);
        BasicGridView.UpdateLayout();

        // Use the Direct configuration to go back (if the API is available). 
        if (ApiInformation.IsApiContractPresent($"{nameof(Windows)}.Foundation.UniversalApiContract", 7))
        {
            animation.Configuration = new DirectConnectedAnimationConfiguration();
        }

        // Play the second connected animation. 
        await BasicGridView.TryStartConnectedAnimationAsync(animation, _storedItem, "connectedElement");
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        CancelSmokeGrid();
    }

    private void Animation_Completed(ConnectedAnimation sender, object args)
    {
        SmokeGrid.Visibility = Visibility.Collapsed;
        SmokeGrid.Children.Add(destinationElement);
        SmokeCancelGrid.Tapped -= SmokeCancelGrid_Tapped;
    }

    private static Visibility IsShowFailList(ICollection list)
    {
        return list.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
    {
        FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
    }

    private class ProgressClass
    {
        public int Index { get; init; } = -1;
        public string ImagePath { get; set; }
        public string Text { get; set; }
        public Status Status { get; set; }
    }

    private void modifyToggleSwitch_Toggled(object sender, RoutedEventArgs e)
    {
        if (sender is not ToggleSwitch toggleSwitch) return;

        //添加模式
        if (toggleSwitch.IsOn)
        {
            StartButton.IsEnabled = true;
            selectedCheckBox.Visibility = Visibility.Visible;
            BasicGridView.SelectionMode = ListViewSelectionMode.Multiple;
            BasicGridView.ItemClick -= BasicGridView_ItemClick;
        }
        //修改模式
        else
        {
            StartButton.IsEnabled = false;
            selectedCheckBox.Visibility = Visibility.Collapsed;
            BasicGridView.SelectionMode = ListViewSelectionMode.None;
            BasicGridView.ItemClick += BasicGridView_ItemClick;
        }
    }

    private void SmokeCancelGrid_Tapped(object sender, TappedRoutedEventArgs e)
    {
        CancelSmokeGrid();
    }

    private async void ModifyActorImage_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not HyperlinkButton { DataContext: string imageUrl }) return;

        var actorName = ShoeActorName.Text;

        var savePath = Path.Combine(AppSettings.ActorInfoSavePath, actorName);
        await DbNetworkHelper.DownloadFile(imageUrl, savePath, "face", true);

        foreach (var item in _actorInfo)
        {
            if (item.Name != actorName) continue;
            item.ProfilePath = imageUrl;
            break;
        }
    }

    private void connectedElement_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Hand);
    }

    private void connectedElement_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Arrow);
    }
}