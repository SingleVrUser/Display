using Microsoft.Windows.AppNotifications.Builder;
using Microsoft.Windows.AppNotifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using System.IO;
using System.Runtime.CompilerServices;
using Display.Views;

namespace Display.Notifications;

class ToastGetActorInfoWithProgressBar
{
    public const int NotifyId = 1;

    public const string c_tag = "进度";
    public const string c_group = "GetActorInfoProgress";

    public static int currentValue = 1;

    public static int allCount = 1;

    public static bool SendToast(int currentValue = 0, int allCount = 0)
    {
        if (currentValue != 0)
            ToastGetActorInfoWithProgressBar.currentValue = currentValue;
        else
            currentValue = ToastGetActorInfoWithProgressBar.currentValue;

        if (allCount != 0)
            ToastGetActorInfoWithProgressBar.allCount = allCount;
        else
            allCount = ToastGetActorInfoWithProgressBar.allCount;

        var appNotification = new AppNotificationBuilder()
            .AddArgument("action", "ToastClick")
            .AddArgument(Common.notificationTag, NotifyId.ToString())

            .SetAppLogoOverride(new System.Uri("file://" + Path.Combine(Package.Current.InstalledLocation.Path, "Assets/NoPicture.jpg")), AppNotificationImageCrop.Circle)
            .AddText("获取演员信息")
            .AddText($"数量：{allCount}")

            .AddProgressBar(new AppNotificationProgressBar()
                .BindTitle()
                .BindValue()
                .BindValueStringOverride()
                .BindStatus())
            .BuildNotification();

        appNotification.Tag = c_tag;
        appNotification.Group = c_group;

        AppNotificationProgressData data = new(1);
        data.Title = c_tag;
        data.Value = (double)currentValue / allCount;
        data.ValueStringOverride = $"{currentValue}/{allCount} 演员";
        data.Status = "正在获取演员信息...";

        appNotification.Progress = data;
        AppNotificationManager.Default.Show(appNotification);

        return appNotification.Id != 0; // return true (indicating success) if the toast was sent (if it has an Id)
    }

    public static async Task<bool> AddValue(int i,int allCount)
    {
        ToastGetActorInfoWithProgressBar.allCount = allCount;

        AppNotificationProgressData data = new(2);
        data.Title = c_tag;
        data.Value = (double)i / allCount;
        data.ValueStringOverride = $"{i}/{allCount} 演员";

        if (i == allCount)
        {
            currentValue = 1;
            data.Status = "完成";
            await Task.Delay(100);
        }
        else
        {
            currentValue = i;
            data.Status = "正在获取演员信息...";
        }

        System.Diagnostics.Debug.WriteLine("更新通知信息");
        var result = await AppNotificationManager.Default.UpdateAsync(data, c_tag, c_group);

        return result == AppNotificationProgressResult.Succeeded;
    }

    public static void NotificationReceived(AppNotificationActivatedEventArgs notificationActivatedEventArgs)
    {
        App.ToForeground();

        ActorsPage.Current.ShowButtonWithShowToastAgain();
    }

}
