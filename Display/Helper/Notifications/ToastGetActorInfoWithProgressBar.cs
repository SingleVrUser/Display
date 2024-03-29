using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Display.Views.Pages;

namespace Display.Helper.Notifications;

internal class ToastGetActorInfoWithProgressBar
{
    public const int NotifyId = 1;

    private const string CTag = "进度";
    public const string CGroup = "GetActorInfoProgress";

    private static int _curValue = 1;

    private static int _allCount = 1;

    public static bool SendToast(int currentValue = 0, int allCount = 0)
    {
        if (currentValue != 0)
            _curValue = currentValue;
        else
            currentValue = _curValue;

        if (allCount != 0)
            _allCount = allCount;
        else
            allCount = _allCount;

        var appNotification = new AppNotificationBuilder()
            .AddArgument("action", "ToastClick")
            .AddArgument(NotifyConstant.NotificationTag, NotifyId.ToString())

            .SetAppLogoOverride(new Uri("file://" + Path.Combine(Package.Current.InstalledLocation.Path, "Assets/NoPicture.jpg")), AppNotificationImageCrop.Circle)
            .AddText("获取演员信息")
            .AddText($"数量：{allCount}")

            .AddProgressBar(new AppNotificationProgressBar()
                .BindTitle()
                .BindValue()
                .BindValueStringOverride()
                .BindStatus())
            .BuildNotification();

        appNotification.Tag = CTag;
        appNotification.Group = CGroup;

        AppNotificationProgressData data = new(1)
        {
            Title = CTag,
            Value = (double)currentValue / allCount,
            ValueStringOverride = $"{currentValue}/{allCount} 演员",
            Status = "正在获取演员信息..."
        };

        appNotification.Progress = data;
        AppNotificationManager.Default.Show(appNotification);

        return appNotification.Id != 0; // return true (indicating success) if the toast was sent (if it has an Id)
    }

    public static async Task AddValue(int i, int allCount)
    {
        _allCount = allCount;

        AppNotificationProgressData data = new(2)
        {
            Title = CTag,
            Value = (double)i / allCount,
            ValueStringOverride = $"{i}/{allCount} 演员"
        };

        if (i == allCount)
        {
            _curValue = 1;
            data.Status = "完成";
            await Task.Delay(100);
        }
        else
        {
            _curValue = i;
            data.Status = "正在获取演员信息...";
        }

        System.Diagnostics.Debug.WriteLine("更新通知信息");
        await AppNotificationManager.Default.UpdateAsync(data, CTag, CGroup);
    }

    public static void NotificationReceived(AppNotificationActivatedEventArgs _)
    {
        App.ToForeground();

        ActorsPage.Current.ShowButtonWithShowToastAgain();
    }

}
