
using System;
using Windows.System;
using Display.Models.Entities;
using Display.Models.Vo.Github;

namespace Display.Views.Pages.Settings;

internal sealed partial class UpdateAppPage
{
    public LatestReleaseCheck ReleaseInfo { get; }

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