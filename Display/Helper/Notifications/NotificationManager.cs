using Microsoft.Windows.AppNotifications;
using System;
using System.Collections.Generic;

namespace Display.Helper.Notifications;

internal class NotificationManager
{
    private bool _mIsRegistered;

    private readonly Dictionary<int, Action<AppNotificationActivatedEventArgs>> _cNotificationHandlers = new()
    {
        { ToastGetActorInfoWithProgressBar.NotifyId, ToastGetActorInfoWithProgressBar.NotificationReceived }
    };

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
        _mIsRegistered = true;
    }

    public void Unregister()
    {
        if (_mIsRegistered)
        {
            AppNotificationManager.Default.Unregister();
            _mIsRegistered = false;
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
        if (!notificationActivatedEventArgs.Arguments.ContainsKey(NotifyConstant.NotificationTag)) return false;

        var notificationId = notificationActivatedEventArgs.Arguments[NotifyConstant.NotificationTag];
        if (notificationId.Length != 0)
        {
            try
            {
                _cNotificationHandlers[int.Parse(notificationId)](notificationActivatedEventArgs);
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

