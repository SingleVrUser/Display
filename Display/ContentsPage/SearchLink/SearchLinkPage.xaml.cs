// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using System;
using Display.Data;
using Display.Models;
using Display.Spider;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Display.Views;
using static System.Int64;
using Display.Services.Upload;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.ContentsPage.SearchLink
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SearchLinkPage : Page
    {
        private readonly string _searchContent;

        public SearchLinkPage(string searchContent)
        {
            this.InitializeComponent();

            _searchContent = searchContent;

            InitLoad();
        }

        private async void InitLoad()
        {
            var links =  await X1080X.GetMatchInfosFromCid(_searchContent);

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

        public async Task<bool> OfflineDownSelectedLink()
        {
            if (LinksListView.SelectedItem is not Forum1080SearchResult info) return false;

            var url = info.Url;

            var attachmentInfos = await X1080X.GetDownLinkFromUrl(url);
            Debug.WriteLine($"获取到下载链接({attachmentInfos.Count})");

            //选择第一个不需要点数的
            var attmnInfo = attachmentInfos.FirstOrDefault(x => x.Expense == 0);

            if (attmnInfo == null) return false;

            // 磁力
            if (attmnInfo.Type == AttmnType.Magnet)
            {
                return await RequestOfflineDown(new List<string>() { attmnInfo.Url });
            }
            else if (attmnInfo.Type == AttmnType.Rar)
            {
                var rarPath = await X1080X.TryDownAttmn(attmnInfo.Url,attmnInfo.Name);

                if(rarPath == null) return false;

                Debug.WriteLine($"附件已保存到：{rarPath}");

                // 解析Rar
                var rarInfo = await X1080X.GetRarInfoFromRarPath(rarPath);
                if (rarInfo == null) return false;

                var down115LinkList = Get115DownUrlsFromAttmnInfo(rarInfo);
                if(down115LinkList.Count==0) return false;

                return await RequestOfflineDown(down115LinkList,rarInfo.SrtPath);
            }else if (attmnInfo.Type == AttmnType.Txt)
            {
                var txtPath = await X1080X.TryDownAttmn(attmnInfo.Url, attmnInfo.Name);
                if (txtPath == null) return false;

                Debug.WriteLine($"附件已保存到：{txtPath}");

                var txtInfo = await X1080X.GetRarInfoFromTxtPath(txtPath);

                var down115LinkList = Get115DownUrlsFromAttmnInfo(txtInfo);
                if (down115LinkList.Count == 0) return false;

                return await RequestOfflineDown(down115LinkList);
            }else if (attmnInfo.Type == AttmnType.Torrent)
            {
                var torrentPath = await X1080X.TryDownAttmn(attmnInfo.Url, attmnInfo.Name);
                if (torrentPath == null) return false;
                Debug.WriteLine($"附件已保存到：{torrentPath}");

                return await WebApi.GlobalWebApi.CreateTorrentOfflineDown(torrentPath);
            }

            return true;
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

            var noRarList = down115LinkList.Where(x=>!x.Contains(".rar")).ToList();
            return noRarList.Count > 0 ? noRarList : down115LinkList;
        }

        private async Task<bool> RequestOfflineDown(List<string> links,string srtPath=null)
        {
            var webApi = WebApi.GlobalWebApi;

            var uploadInfo = await webApi.GetUploadInfo();
            if (uploadInfo == null) return false;

            var offlineSpaceInfo = await webApi.GetOfflineSpaceInfo(uploadInfo.userkey, uploadInfo.user_id);

            TryParse(AppSettings.SavePath115Cid, out var cid);

            var addTaskUrlInfo = await webApi.AddTaskUrl(links, cid, uploadInfo.user_id, offlineSpaceInfo.sign, offlineSpaceInfo.time);

            // 需要验证账号
            if (addTaskUrlInfo is { errcode: Const.Common.AccountAnomalyCode })
            {
                var window = WebApi.CreateWindowToVerifyAccount();
                
                var result = false;

                if (window.Content is not VerifyAccountPage page) return false;

                var isClosed = false;
                page.VerifyAccountCompleted += async (_, iSucceeded) =>
                {
                    if (!iSucceeded)
                    {
                        result = false;
                        isClosed = true;
                        return;
                    }

                    result = await RequestOfflineDown(links);
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

            return addTaskUrlInfo is { state: true };

        }

        private void OpenInBrowserClick(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuFlyoutItem { DataContext: Forum1080SearchResult result }) return;

            Windows.System.Launcher.LaunchUriAsync(new Uri(result.Url));
        }
    }
}
