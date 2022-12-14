// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Data;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.WindowView
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class StartWindow : WinUIEx.WindowEx
    {
        public StartWindow()
        {
            this.InitializeComponent();

            this.Title = "升级";

            this.Closed += StartWindow_Closed;
        }

        private void StartWindow_Closed(object sender, WindowEventArgs args)
        {
            AppSettings.IsUpdatedDataAccessFrom014 = true ;
            App.CreateActivateMainWindow();
        }

        private async void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            //升级数据库，v0.15开始
            await Task.Run(async () => await DataAccess.UpdateDatabaseFrom14());

            //关闭窗口
            this.Close();
        }

    }
}
