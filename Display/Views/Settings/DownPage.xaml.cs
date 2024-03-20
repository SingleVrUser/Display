using System;
using System.Net;
using Display.Models.Data;
using Microsoft.UI.Xaml;
using System.Text.RegularExpressions;
using Windows.Storage.Pickers;
using Display.Helper.FileProperties.Name;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Display.Providers.Downloader;
using SharpCompress;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.Views.Settings;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class DownPage : RootPage
{
    private readonly BaseDownloader[] _downloaders = Managers.DownloadManager.Downloads;

    public DownPage()
    {
        this.InitializeComponent();

        _downloaders.ForEach(download
            => download.ShowMessageAction += ShowMessageAction);
    }

    private void ShowMessageAction(string subtitle, string content = null, InfoBarSeverity severity = InfoBarSeverity.Informational)
    {
        ShowTeachingTip(subtitle, content, severity);
    }
}