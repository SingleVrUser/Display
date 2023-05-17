// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Display.Models;
using Display.Spider;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

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

            DispatcherQueue.TryEnqueue(InitLoad);

        }

        private async void InitLoad()
        {
            var links =  await X1080X.GetMatchInfosFromCid(_searchContent);

            if (links is { Count: > 0 })
            {
                LinksListView.ItemsSource = links;
            }
            else
            {
                WithoutResultTextBlock.Visibility = Visibility.Visible;
            }
            
            MyProgressBar.Visibility = Visibility.Collapsed;
        }

        public async void OfflineDownSelectedLink()
        {
            var selectedList = LinksListView.SelectedItems.Cast<Forum1080SearchResult>();

            List<string> links = new();
            foreach (var info in selectedList)
            {
                var url = info.Url;

                var downLink = await X1080X.GetDownLinkFromUrl(url);

                Debug.WriteLine($"获取到下载链接({downLink.Count})：{string.Join(",", downLink)}");
            }
        }
    }
}
