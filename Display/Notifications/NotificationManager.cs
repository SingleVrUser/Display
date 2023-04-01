using Microsoft.Windows.AppNotifications;
using System;
using System.Collections.Generic;

namespace Display.Notifications;

internal class NotificationManager
{
    private bool m_isRegistered;

    private Dictionary<int, Action<AppNotificationActivatedEventArgs>> c_notificationHandlers;

    public NotificationManager()
    {
        m_isRegistered = false;

        // When adding new a scenario, be sure to add its notification handler here.
        c_notificationHandlers = new Dictionary<int, Action<AppNotificationActivatedEventArgs>>();
        c_notificationHandlers.Add(ToastGetActorInfoWithProgressBar.NotifyId, ToastGetActorInfoWithProgressBar.NotificationReceived);
    }

    ~NotificationManager()
    {
        Unregister();
    }

    public void Init()
    {
        // To ensure all Notification handling happens in this process instance, register for
        // NotificationInvoked before calling Register(). Without this a new process will
        // be launched to handle the notification.
        AppNotificationManager notificationManager = AppNotificationManager.Default;

        notificationManager.NotificationInvoked += OnNotificationInvoked;

        notificationManager.Register();
        m_isRegistered = true;
    }

    public void Unregister()
    {
        if (m_isRegistered)
        {
            AppNotificationManager.Default.Unregister();
            m_isRegistered = false;
        }
    }

    public void ProcessLaunchActivationArgs(AppNotificationActivatedEventArgs notificationActivatedEventArgs)
    {
        // Complete in Step 5
    }

    void OnNotificationInvoked(object sender, AppNotificationActivatedEventArgs notificationActivatedEventArgs)
    {
        System.Diagnostics.Debug.WriteLine("接受消息");

        if (!DispatchNotification(notificationActivatedEventArgs))
        {
            System.Diagnostics.Debug.WriteLine("接受消息失败");
        }
    }

    public static async void RemoveGetActorInfoProgessToast()
    {
        //删除获取演员信息的进度通知
        await AppNotificationManager.Default.RemoveByGroupAsync(ToastGetActorInfoWithProgressBar.c_group);
    }

    public bool DispatchNotification(AppNotificationActivatedEventArgs notificationActivatedEventArgs)
    {
        if (!notificationActivatedEventArgs.Arguments.ContainsKey(Common.notificationTag)) return false;

        var notificationId = notificationActivatedEventArgs.Arguments[Common.notificationTag];
        if (notificationId.Length != 0)
        {
            try
            {
                c_notificationHandlers[int.Parse(notificationId)](notificationActivatedEventArgs);
                return true;
            }
            catch
            {
                return false; // Couldn't find a NotificationHandler for scenarioId.
            }
        }
        else
        {
            return false; // No scenario specified in the notification
        }
    }

}

