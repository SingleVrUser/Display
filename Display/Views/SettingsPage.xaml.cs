
using CommunityToolkit.WinUI.Behaviors;
using Display.Helper.FileProperties.Name;
using Display.Helper.Network.Spider;
using Display.Models.Data;
using Display.Views.Settings;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using Display.Models.Data.Enums;
using Display.Models.Settings.Options;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.Views;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class SettingsPage
{

    public SettingsPage()
    {
        InitializeComponent();
    }

    private void ShowTeachingTip(string subtitle, string content = null, InfoBarSeverity severity = InfoBarSeverity.Informational)
    {
        var notification = new Notification
        {
            Title = subtitle,
            Message = content,
            Severity = severity,
            Duration = TimeSpan.FromSeconds(2)
        };
        NotificationQueue.Show(notification);
    }

    private void ShowTeachingTip(UIElement content, InfoBarSeverity severity = InfoBarSeverity.Informational, int durationSeconds = 2)
    {
        var notification = new Notification
        {
            Title = "提示",
            Content = content,
            Severity = severity
        };

        if (durationSeconds != 0)
        {
            notification.Duration = TimeSpan.FromSeconds(durationSeconds);
        }

        NotificationQueue.Show(notification);
    }

}