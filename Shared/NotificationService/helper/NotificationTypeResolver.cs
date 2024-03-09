using System;
using NotificationService.Enums;

namespace NotificationService.helper
{
    public class NotificationTypeResolver
    {
        public static GeneralNotificationType GetGeneralNotificationTypes(NotificationType notificationType)
        {
            switch (notificationType)
            {
                case NotificationType.TEXT:
                case NotificationType.AUDIO:
                case NotificationType.IMAGE:
                case NotificationType.LOCATION:
                case NotificationType.DOCUMENT:
                case NotificationType.CONTACT:
                case NotificationType.PHONE_CONTACT:
                    return GeneralNotificationType.INFO;

                case NotificationType.P2P_START_AUDIO_CALL:
                case NotificationType.P2P_START_VIDEO_CALL:
                case NotificationType.P2P_MISSED_AUDIO_CALL:
                case NotificationType.P2P_MISSED_VIDEO_CALL:
                case NotificationType.P2P_DECLINE_CALL:
                case NotificationType.RINGING:
                    return GeneralNotificationType.P2P_CALL;

                case NotificationType.GROUP_START_AUDIO_CALL:
                case NotificationType.GROUP_START_VIDEO_CALL:
                case NotificationType.GROUP_END_CALL:
                    return GeneralNotificationType.GROUP_CALL;

                default:
                    return GeneralNotificationType.INFO;

            }
        }

    }
}
