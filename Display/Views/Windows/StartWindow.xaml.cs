// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Display.Providers;
using Microsoft.UI.Xaml;

namespace Display.Views.Windows;

public sealed partial class StartWindow
{
    public StartWindow()
    {
        InitializeComponent();

        Title = "升级";

        Closed += StartWindow_Closed;
    }

    private void StartWindow_Closed(object sender, WindowEventArgs args)
    {
        App.CreateActivateMainWindow();
    }

    private async void Grid_Loaded(object sender, RoutedEventArgs e)
    {
        //升级数据库，v0.15开始
        await Task.Run(DataAccess.Update.UpdateDatabaseFrom14);

        AppSettings.IsUpdatedDataAccessFrom014 = true;

        //关闭窗口
        this.Close();
    }

}