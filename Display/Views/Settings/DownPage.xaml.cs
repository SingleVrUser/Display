using Display.Providers.Downloader;
using Microsoft.UI.Xaml.Controls;
using SharpCompress;

namespace Display.Views.Settings;

public sealed partial class DownPage
{
    private readonly BaseDownloader[] _downloaders = Managers.DownloadManager.Downloads;

    public DownPage()
    {
        InitializeComponent();

        _downloaders.ForEach(download
            => download.ShowMessageAction += ShowMessageAction);
    }

    private void ShowMessageAction(string subtitle, string content = null, InfoBarSeverity severity = InfoBarSeverity.Informational)
    {
        ShowTeachingTip(subtitle, content, severity);
    }
}