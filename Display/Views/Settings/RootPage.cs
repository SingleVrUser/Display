using System;
using CommunityToolkit.WinUI.Behaviors;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Display.Views.Settings;

public abstract class RootPage : Page
{
    private StackedNotificationsBehavior _notificationQueue;

    protected void SetNotificationQueue(StackedNotificationsBehavior notificationQueue)
        => _notificationQueue = notificationQueue;

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is StackedNotificationsBehavior notificationQueue)
        {
            SetNotificationQueue(notificationQueue);
        }
    }

    protected void ShowTeachingTip(string subtitle, string content = null, InfoBarSeverity severity = InfoBarSeverity.Informational)
    {
        if (_notificationQueue is null) return;

        var notification = new Notification
        {
            Title = subtitle,
            Message = content,
            Severity = severity,
            Duration = TimeSpan.FromSeconds(2)
        };
        _notificationQueue.Show(notification);
    }

    protected void ShowTeachingTip(UIElement content, InfoBarSeverity severity = InfoBarSeverity.Informational, int durationSeconds = 2)
    {
        if (_notificationQueue is null) return;

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

        _notificationQueue.Show(notification);
    }
}