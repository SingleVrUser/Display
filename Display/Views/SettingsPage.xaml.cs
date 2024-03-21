
using CommunityToolkit.WinUI.Behaviors;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

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