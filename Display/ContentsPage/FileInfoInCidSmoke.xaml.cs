// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Data;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.ContentsPage
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FileInfoInCidSmoke : Page
    {
        private string truename;

        public FileInfoInCidSmoke(string truename)
        {
            this.InitializeComponent();

            this.truename = truename;

            this.Loaded += PageLoad;
        }

        private async void PageLoad(object sender, RoutedEventArgs e)
        {
            var VideoInfos = await DataAccess.FindFileInfoByTrueName(truename);

            InfosListView.ItemsSource= VideoInfos;
        }
    }
}
