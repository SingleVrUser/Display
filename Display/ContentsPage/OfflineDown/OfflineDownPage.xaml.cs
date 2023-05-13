// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Display.Data;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using ABI.Windows.Networking.Proximity;

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

            if (WebApi.UploadInfo == null)
            {
                await _webApi.GetUploadInfo();
            }

            _uploadInfo = WebApi.UploadInfo;

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

            _matchLinkCollection = Regex.Matches(content, @"((((http|https|ftp|ed2k):\/\/)|(magnet:\?xt=urn:btih:)).{3,}?)([\r\n]|$)");

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

            var result = await _webApi.AddTaskUrl(links, downPath.file_id, downPath.user_id, _offlineSpaceInfo.sign, _offlineSpaceInfo.time);

            RequestCompleted?.Invoke(this,new RequestCompletedEventArgs(result));
        }

        public event EventHandler<FailLoadedEventArgs> FailLoaded;

        public event EventHandler<RequestCompletedEventArgs> RequestCompleted;
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