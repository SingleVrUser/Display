// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using System;
using Windows.System;
using Display.Models.Entities;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.Views.Pages.Settings;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class UpdateAppPage : Page
{
    public LatestReleaseCheck ReleaseInfo { get; set; }

    public UpdateAppPage(LatestReleaseCheck releaseInfo)
    {
        this.InitializeComponent();

        ReleaseInfo = releaseInfo;
    }

    private async void MarkdownTextBlock_LinkClicked(object sender, CommunityToolkit.WinUI.UI.Controls.LinkClickedEventArgs e)
    {
        if (Uri.TryCreate(e.Link, UriKind.Absolute, out var link))
        {
            await Launcher.LaunchUriAsync(link);
        }
    }
}