// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Display.Data;
using Display.Models;
using Display.Services.Upload;
using Display.Spider;
using Display.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using static System.Int64;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.ContentsPage.SearchLink
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SearchLinkPage : Page, INotifyPropertyChanged
    {
        private readonly string _searchContent;

        private static long _lastRequestTime;
        private const int SpacingSecond = 15;

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

            // 与上次等待间隙不足15s，则等待（1s余量）
            var leftSecond = SpacingSecond - NowDate + _lastRequestTime + 1;
            if (leftSecond <= 0) return;

            LeftTime = leftSecond;
            for (var i = 1; i <= leftSecond; i++)
            {
                await Task.Delay(1000);
                LeftTime = leftSecond - i;
            }
        }

        public static async Task<string> ShowInContentDialog(string searchContent, XamlRoot xamlRoot)
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

            var downResult = await page.OfflineDownSelectedLink(cid);
            return string.IsNullOrEmpty(downResult) ? "下载失败" : downResult;
        }

        private async void InitLoad()
        {
            _lastRequestTime = NowDate;

            var links = await X1080X.GetMatchInfosFromCid(_searchContent);

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

        public async Task<string> OfflineDownSelectedLink(long cid)
        {
            if (LinksListView.SelectedItem is not Forum1080SearchResult info) return null;

            var url = info.Url;
            var attachmentInfos = await X1080X.GetDownLinkFromUrl(url);
            Debug.WriteLine($"获取到下载链接({attachmentInfos.Count})");

            //选择第一个不需要点数的
            var attmnInfo = attachmentInfos.FirstOrDefault(x => x.Expense == 0);

            if (attmnInfo == null) return "获取到附件下载链接时出错";

            // 磁力
            if (attmnInfo.Type == AttmnType.Magnet)
            {
                return await RequestOfflineDown(cid, new List<string> { attmnInfo.Url });
            }
            else if (attmnInfo.Type == AttmnType.Rar)
            {
                var rarPath = await X1080X.TryDownAttmn(attmnInfo.Url, attmnInfo.Name);

                if (rarPath == null) return "下载附件(.rar)时出错";

                Debug.WriteLine($"附件已保存到：{rarPath}");

                // 解析Rar
                var rarInfo = await X1080X.GetRarInfoFromRarPath(rarPath);
                if (rarInfo == null) return "解析附件时出错";

                var down115LinkList = Get115DownUrlsFromAttmnInfo(rarInfo);
                if (down115LinkList.Count == 0) return "附件中未找到115下载链接";

                return await RequestOfflineDown(cid, down115LinkList, rarInfo.SrtPath);
            }
            else if (attmnInfo.Type == AttmnType.Txt)
            {
                var txtPath = await X1080X.TryDownAttmn(attmnInfo.Url, attmnInfo.Name);
                if (txtPath == null) return "下载附件(.txt)时出错";

                Debug.WriteLine($"附件已保存到：{txtPath}");

                var txtInfo = await X1080X.GetRarInfoFromTxtPath(txtPath);

                var down115LinkList = Get115DownUrlsFromAttmnInfo(txtInfo);
                if (down115LinkList.Count == 0) return "附件中未找到115下载链接";

                return await RequestOfflineDown(cid, down115LinkList);
            }
            else if (attmnInfo.Type == AttmnType.Torrent)
            {
                var torrentPath = await X1080X.TryDownAttmn(attmnInfo.Url, attmnInfo.Name);
                if (torrentPath == null) return "下载附件(.torrent)时出错";

                Debug.WriteLine($"附件已保存到：{torrentPath}");

                var(_,msg)= await WebApi.GlobalWebApi.CreateTorrentOfflineDown(cid, torrentPath);

                return msg;
            }

            return "当前附件格式不受支持";
        }

        private List<string> Get115DownUrlsFromAttmnInfo(AttmnFileInfo attmnInfo)
        {
            var down115LinkList = new List<string>();

            // 115下载只需要用一种方式 
            var down115Method = string.Empty;

            //其他链接
            foreach (var linkDict in attmnInfo.Links)
            {
                switch (linkDict.Key)
                {
                    case ("ed2k" or "magnet" or "直链"):
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

        private async Task<string> RequestOfflineDown(long cid, List<string> links, string srtPath = null)
        {
            var webApi = WebApi.GlobalWebApi;

            var uploadInfo = await webApi.GetUploadInfo();
            if (uploadInfo == null) return "获取115上传信息时出错";

            var offlineSpaceInfo = await webApi.GetOfflineSpaceInfo(uploadInfo.userkey, uploadInfo.user_id);

            var addTaskUrlInfo = await webApi.AddTaskUrl(links, cid, uploadInfo.user_id, offlineSpaceInfo.sign, offlineSpaceInfo.time);

            // 需要验证账号
            if (addTaskUrlInfo is { errcode: Const.Common.AccountAnomalyCode })
            {
                var window = WebApi.CreateWindowToVerifyAccount();

                if (window.Content is not VerifyAccountPage page) return null;

                string result = null;
                var isClosed = false;
                page.VerifyAccountCompleted += async (_, iSucceeded) =>
                {
                    if (!iSucceeded)
                    {
                        result = "验证账号未成功";
                        isClosed = true;
                        return;
                    }

                    result = await RequestOfflineDown(cid, links);
                    isClosed = true;
                };

                window.Activate();

                //堵塞，直到关闭输入验证码的window
                while (!isClosed)
                {
                    await Task.Delay(2000);
                }

                return result;
            }

            // 上传字幕，如果需要
            if (!string.IsNullOrEmpty(srtPath))
            {
                await FileUpload.SimpleUpload(srtPath, cid, uploadInfo.user_id, uploadInfo.userkey);
            }

            return addTaskUrlInfo is { state: true } ? "任务添加成功" : (!string.IsNullOrEmpty(addTaskUrlInfo.error_msg) ? addTaskUrlInfo.error_msg : "任务添加失败");
        }

        private void OpenInBrowserClick(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuFlyoutItem { DataContext: Forum1080SearchResult result }) return;

            Windows.System.Launcher.LaunchUriAsync(new Uri(result.Url));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
