// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.ContentsPage.SearchLink
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SearchLinkPage : Page
    {
        private string _searchContent;

        public SearchLinkPage(string searchContent)
        {
            this.InitializeComponent();

            _searchContent = searchContent;

            InitLoad();
        }

        private async void InitLoad()
        {
            await Spider.X1080X.SearchDownLinkFromCid(_searchContent);


        }


    }
}
