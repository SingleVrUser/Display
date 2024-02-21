// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using System.Collections.Generic;
using Display.Models.Data;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.Views.OfflineDown
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FailListPage : Page
    {
        private List<AddTaskUrlInfo> UrlInfos;

        public FailListPage(List<AddTaskUrlInfo> urlInfos)
        {
            this.InitializeComponent();

            this.UrlInfos = urlInfos;

        }
    }
}
