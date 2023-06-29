// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Display.Data;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using ABI.Windows.Networking.Proximity;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Display.Services.Upload;
using Display.Views;
using Windows.Foundation;
using Display.WindowView;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.ContentsPage.OfflineDown
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class OfflineDownPage : Page
    {
        private readonly WebApi _webApi = WebApi.GlobalWebApi;

        private MatchCollection _matchLinkCollection;

        private UploadInfo _uploadInfo;

        private OfflineSpaceInfo _offlineSpaceInfo;

        public OfflineDownPage(string defaultLink)
        {
            this.InitializeComponent();

            InitData();

            LinkTextBox.Text = defaultLink;
            CheckLinkCount(defaultLink);
        }

        private async void InitData()
        {
            var downPathRequest = await _webApi.GetOfflineDownPath();

            if (!(downPathRequest?.data?.Length > 0)) return;

            foreach (var datum in downPathRequest.data)
            {
                DownPathComboBox.Items.Add(datum);
            }

            DownPathComboBox.SelectedIndex = 0;

            _uploadInfo = await _webApi.GetUploadInfo();

            if (_uploadInfo == null)
            {
                // 无法获取离线下载信息
                FailLoaded?.Invoke(this, new FailLoadedEventArgs("无法获取UploadInfo"));
                return;
            }

            _offlineSpaceInfo = await _webApi.GetOfflineSpaceInfo(_uploadInfo.userkey, _uploadInfo.user_id);
        }

        private void TextBox_OnSelectionChanged(object sender, RoutedEventArgs e)
        {
            if (sender is not TextBox textBox) return;

            CheckLinkCount(textBox.Text);
        }

        private void CheckLinkCount(string content)
        {
            _matchLinkCollection = Regex.Matches(content,
                @"((((http|https|ftp|ed2k):\/\/)|(magnet:\?xt=urn:btih:)).{3,}?)([\r\n]|$)");

            LinksCountRun.Text = _matchLinkCollection.Count.ToString();
        }

        public async void CreateOfflineDownRequest()
        {
            if (_matchLinkCollection.Count == 0) return;

            // 链接
            List<string> links = new();
            foreach (Match match in _matchLinkCollection)
            {
                links.Add(match.Groups[1].Value);
            }

            // 保存路径
            if (DownPathComboBox.SelectedItem is not OfflineDownPathData downPath) return;

            var result = await _webApi.AddTaskUrl(links, downPath.file_id, downPath.user_id, _offlineSpaceInfo.sign,
                _offlineSpaceInfo.time);

            RequestCompleted?.Invoke(this, new RequestCompletedEventArgs(result));
        }

        public event EventHandler<FailLoadedEventArgs> FailLoaded;

        public event EventHandler<RequestCompletedEventArgs> RequestCompleted;

        private void RootGrid_OnDragOver(object sender, DragEventArgs e)
        {
            //获取拖入文件信息
            if (!e.DataView.Contains(StandardDataFormats.StorageItems)) return;

            e.AcceptedOperation = DataPackageOperation.Link;
            e.DragUIOverride.Caption = "拖拽torrent后开始任务";
        }


        private async void RootGrid_OnDrop(object sender, DragEventArgs e)
        {
            // 获取拖入文件信息
            if (!e.DataView.Contains(StandardDataFormats.StorageItems)) return;

            // 获取保存路径
            if (DownPathComboBox.SelectedItem is not OfflineDownPathData downPath) return;

            var items = await e.DataView.GetStorageItemsAsync();

            var rarItems = items.Where(i => i.IsOfType(StorageItemTypes.File) && i is StorageFile { FileType: ".torrent" }).Select(x=>x as StorageFile).ToArray();

            var length = rarItems.Length;
            
            var isSucceed = false;
            string content;
            switch (length)
            {
                case 0:
                    content = "未发现torrent文件";
                    break;
                // 单个torrent
                case 1:
                    (isSucceed,content) = await WebApi.GlobalWebApi.CreateTorrentOfflineDown(downPath.file_id, rarItems.First().Path);
                    break;
                // 多个
                default:
                {
                    var failCount = 0;
                    for(var i = 1; i <= length;i++)
                    {
                        var file = rarItems[i];

                        Debug.WriteLine(file.FileType);

                        ShowTeachingTip($"添加torrent任务中：{i}/{length}" + (failCount > 0 ? "，失败数:{failCount}" : string.Empty));

                        var (isCurrentSucceed, _)= await WebApi.GlobalWebApi.CreateTorrentOfflineDown(
                            downPath.file_id, file.Path);
                                
                        if (!isCurrentSucceed) failCount++;
                    }

                    isSucceed = length != failCount;
                    content = $"添加torrent任务完成 （{length}个）" + (failCount > 0 ? "，失败数:{failCount}" : string.Empty);
                    break;
                }
            }

            // 有成功项的话，添加打开目录的按钮
            if (isSucceed)
            {
                ShowTeachingTip(content, "打开所在目录", (_, _) =>
                {
                    // 打开所在目录
                    CommonWindow.CreateAndShowWindow(new DatumList.FileListPage(downPath.file_id));
                });
            }
            else
            {
                ShowTeachingTip(content);
            }
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

    }

    public class FailLoadedEventArgs : EventArgs
    {
        public string Message { get; set; }

        public FailLoadedEventArgs(string message)
        {
            this.Message = message;
        }
    }

    public class RequestCompletedEventArgs : EventArgs
    {
        public AddTaskUrlInfo Info { get; set; }

        public RequestCompletedEventArgs(AddTaskUrlInfo result)
        {
            this.Info = result;
        }
    }
}