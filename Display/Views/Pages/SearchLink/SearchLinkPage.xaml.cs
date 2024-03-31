// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.System;
using Display.Helper.Notifications;
using Display.Models.Enums;
using Display.Models.Vo.Forum;
using Display.Providers;
using Display.Providers.Searcher;
using Display.Services.Upload;
using Display.Views.Pages.Settings.Account;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using NetworkHelper = Display.Helper.Network.NetworkHelper;
using Display.Models.Api.OneOneFive.Upload;

namespace Display.Views.Pages.SearchLink;

public sealed partial class SearchLinkPage : INotifyPropertyChanged
{
    private readonly string _searchContent;

    private static long _lastRequestTime;
    private const int SpacingSecond = 15;

    private WebApi _webApi = WebApi.GlobalWebApi;

    private static long NowDate => DateTimeOffset.Now.ToUnixTimeSeconds();

    public SearchLinkPage(string searchContent)
    {
        InitializeComponent();

        _searchContent = searchContent;
    }

    private long _leftTime;

    private long LeftTime
    {
        get => _leftTime;
        set
        {
            if (_leftTime == value) return;
            _leftTime = value;

            OnPropertyChanged();
        }
    }

    private Visibility IsShowTimeCountdown(long time)
    {
        return time == 0 ? Visibility.Collapsed : Visibility.Visible;
    }

    public async Task StartCountdownIfNecessary()
    {
        // 第一次
        if (_lastRequestTime == 0) return;

        // 与上次等待间隙不足15s，则等待（3s余量）
        var leftSecond = SpacingSecond - NowDate + _lastRequestTime + 3;
        if (leftSecond <= 0) return;

        LeftTime = leftSecond;
        for (var i = 1; i <= leftSecond; i++)
        {
            await Task.Delay(1000);
            LeftTime = leftSecond - i;
        }
    }

    public static async Task<Tuple<bool, string>> ShowInContentDialog(string searchContent, XamlRoot xamlRoot)
    {
        var cid = AppSettings.SavePath115Cid;

        var page = new SearchLinkPage(searchContent);
        ContentDialog dialog = new()
        {
            XamlRoot = xamlRoot,
            Title = "搜索结果",
            PrimaryButtonText = "下载选中",
            CloseButtonText = "返回",
            DefaultButton = ContentDialogButton.Primary,
            Content = page
        };

        dialog.Opened += async (_, _) =>
        {
            await page.StartCountdownIfNecessary();
            page.InitLoad();
        };

        var result = await dialog.ShowAsync();
        if (result != ContentDialogResult.Primary) return null;

        return await page.OfflineDownSelectedLink(cid);
    }

    private async void InitLoad()
    {

        var links = await X1080X.GetMatchInfosFromCid(_searchContent);
        _lastRequestTime = NowDate;

        if (links is { Count: > 0 })
        {
            LinksListView.ItemsSource = links;
            LinksListView.SelectedItem = 0;
        }
        else
        {
            WithoutResultTextBlock.Visibility = Visibility.Visible;
        }

        MyProgressBar.Visibility = Visibility.Collapsed;
    }

    public async Task<Tuple<bool, string>> OfflineDownSelectedLink(long cid)
    {
        var infos = LinksListView.SelectedItems.Cast<Forum1080SearchResult>().ToArray();
        if (!infos.Any()) return null;

        List<string> offlineDownList = [];
        List<string> torrentDownList = [];
        List<string> srtUploadList = [];

        for (var i = 0; i < infos.Length; i++)
        {
            var url = infos[i].Url;
            var attachmentInfos = await X1080X.GetDownLinkFromUrl(url);

            Debug.WriteLine($"获取到下载链接({attachmentInfos.Length})");

            // 最后一个不需要等待（当数量为1时，第一个就是最后一个）
            if(infos.Length != i + 1) await NetworkHelper.RandomTimeDelay(1,5);

            //选择第一个不需要点数的
            var attmnInfo = attachmentInfos.FirstOrDefault(x => x.Expense == 0);

            if (attmnInfo == null)
            {
                Toast.TryToast("1080", "获取到附件下载链接时出错");
                continue;
            }

            switch (attmnInfo.TypeEnum)
            {
                // 磁力
                case AttmnTypeEnum.Magnet:
                {
                    offlineDownList.Add(attmnInfo.Url);
                    break;
                }
                case AttmnTypeEnum.Rar:
                {
                    var rarPath = await X1080X.TryDownAttmn(attmnInfo.Url, attmnInfo.Name);

                    if (rarPath == null) return new Tuple<bool, string>(false, "下载附件(.rar)时出错");

                    Debug.WriteLine($"附件已保存到：{rarPath}");

                    // 解析Rar
                    var rarInfo = await X1080X.GetRarInfoFromRarPath(rarPath);
                    if (rarInfo == null) return new Tuple<bool, string>(false, "解析附件时出错");

                    var down115LinkList = Get115DownUrlsFromAttmnInfo(rarInfo);
                    if (down115LinkList.Count == 0)
                    {
                        Toast.TryToast("1080", "附件中未找到115下载链接");
                        continue;
                    }

                    srtUploadList.Add(rarInfo.SrtPath);
                    offlineDownList.AddRange(down115LinkList);
                    break;
                }
                case AttmnTypeEnum.Txt:
                {
                    var txtPath = await X1080X.TryDownAttmn(attmnInfo.Url, attmnInfo.Name);
                    if (txtPath == null) return new Tuple<bool, string>(false, "下载附件(.txt)时出错");

                    Debug.WriteLine($"附件已保存到：{txtPath}");

                    var txtInfo = await X1080X.GetRarInfoFromTxtPath(txtPath);

                    var down115LinkList = Get115DownUrlsFromAttmnInfo(txtInfo);
                    if (down115LinkList.Count == 0)
                    {
                        Toast.TryToast("1080", "附件中未找到115下载链接");
                        continue;
                    }

                    offlineDownList.AddRange(down115LinkList);
                    break;
                }
                case AttmnTypeEnum.Torrent:
                {
                    var torrentPath = await X1080X.TryDownAttmn(attmnInfo.Url, attmnInfo.Name);
                    if (torrentPath == null)
                    {
                        Toast.TryToast("1080", "下载附件(.torrent)时出错");
                        continue;
                    }

                    Debug.WriteLine($"附件已保存到：{torrentPath}");

                    torrentDownList.Add(torrentPath);
                    break;
                }
            }
        }

        var uploadInfo = await _webApi.GetUploadInfo();
        if (uploadInfo == null) return new Tuple<bool, string>(false, "获取115上传信息时出错");

        await RequestUploadSrtPath(cid, uploadInfo, srtUploadList);
        await RequestTorrentDown(cid, torrentDownList);
        var isAllDone = await RequestOfflineDown(cid, uploadInfo, offlineDownList);

        return isAllDone ? new Tuple<bool, string>(true, "已添加") : new Tuple<bool, string>(false, "添加失败");
    }

    private List<string> Get115DownUrlsFromAttmnInfo(Forum1080AttmnFileInfo forum1080AttmnInfo)
    {
        var down115LinkList = new List<string>();

        // 115下载只需要用一种方式 
        var down115Method = string.Empty;

        //其他链接
        foreach (var linkDict in forum1080AttmnInfo.Links)
        {
            switch (linkDict.Key)
            {
                case "ed2k" or "magnet" or "直链":
                    if (down115Method == string.Empty || down115Method == linkDict.Key)
                    {
                        down115LinkList.AddRange(linkDict.Value);

                        down115Method = linkDict.Key;
                    }

                    break;
            }
        }

        var noRarList = down115LinkList.Where(x => !x.Contains(".rar")).ToList();
        return noRarList.Count > 0 ? noRarList : down115LinkList;
    }

    private async Task RequestTorrentDown(long cid, IList<string> torrentPathList)
    {
        foreach (var torrentPath in torrentPathList)
        {
            await WebApi.GlobalWebApi.CreateTorrentOfflineDown(cid, torrentPath);
        }
    }

    private async Task RequestUploadSrtPath(long cid, UploadInfoResult uploadInfo, IEnumerable<string> srtPathList)
    {
        foreach (var srtPath in srtPathList.Where(srtPath => !string.IsNullOrEmpty(srtPath)))
        {
            await FileUploadService.SimpleUpload(srtPath, cid, uploadInfo.UserId, uploadInfo.UserKey);
        }
    }

    private async Task<bool> RequestOfflineDown(long cid, UploadInfoResult uploadInfo, List<string> links)
    {
        var offlineSpaceInfo = await _webApi.GetOfflineSpaceInfo(uploadInfo.UserKey, uploadInfo.UserId);

        var addTaskUrlInfo = await _webApi.AddTaskUrl(links, cid, uploadInfo.UserId, offlineSpaceInfo.Sign, offlineSpaceInfo.Time);

        var isDone = false;

        // 需要验证账号
        if (addTaskUrlInfo is not { ErrCode: Constants.Account.AccountAnomalyCode })
            return addTaskUrlInfo is { State: true };

        var window = WebApi.CreateWindowToVerifyAccount();

        if (window.Content is not VerifyAccountPage page) return false;

        var isClosed = false;
        page.VerifyAccountCompleted += async (_, iSucceeded) =>
        {
            if (!iSucceeded)
            {
                isClosed = true;
                return;
            }

            isDone = await RequestOfflineDown(cid, uploadInfo, links);
            isClosed = true;
        };

        window.Activate();

        //堵塞，直到关闭输入验证码的window
        while (!isClosed)
        {
            await Task.Delay(2000);
        }

        return isDone;

    }

    private void OpenInBrowserClick(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuFlyoutItem { DataContext: Forum1080SearchResult result }) return;

        async void Callback()
        {
            await Launcher.LaunchUriAsync(new Uri(result.Url));
        }

        DispatcherQueue.TryEnqueue(Callback);
    }

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }


    private record UrlsAndSrtPath(List<string> Urls, string SrtPath);
}