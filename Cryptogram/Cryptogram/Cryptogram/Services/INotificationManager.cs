using System;
using System.Collections.Generic;
using NotificationService;

namespace Anonymous.Services
{
    public interface INotificationManager
    {
        void Initialize();

        int ScheduleNotification(string title, string message, string chatId, NotificationType notificationType);

        void CancelNotification();

        void DisableVibratorRinging();

        void CancelCallNotification(string chatId = null);

    }
}
