using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using NotificationSamples;

public class NotificationsScript : MonoBehaviour
{

    [SerializeField] private GameNotificationsManager notificationsManager;
    private int notificationsDelay;

    private void NotificationsInit()
    {
        GameNotificationChannel channel = new GameNotificationChannel("bddnotifications", "BaldErDash", "Test not");
        notificationsManager.Initialize(channel);
    }

    private void Notify(string title, string text, DateTime time, string smallIcon = "icon_0", string largeIcon = "icon_0")
    {
        IGameNotification notification = notificationsManager.CreateNotification();
        if (notification != null)
        {
            notification.Title = title;
            notification.Body = text;
            notification.DeliveryTime = time;
            notification.SmallIcon = smallIcon;
            notification.LargeIcon = largeIcon;
            notificationsManager.ScheduleNotification(notification);
        }
    }
}

