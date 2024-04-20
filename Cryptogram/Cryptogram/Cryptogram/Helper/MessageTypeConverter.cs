using System;
using NotificationService;
using static EncryptedMessaging.MessageFormat;

namespace Anonymous.Helper
{
    /// <summary>
    /// This is for converting message type to notification type. 
    /// </summary>
    public class MessageTypeConverter
    {
        public static NotificationType GetNotificationType(MessageType messageType)
        {
            switch (messageType)
            {
                case MessageType.Text:
                    return NotificationType.TEXT;
                case MessageType.Audio:
                    return NotificationType.AUDIO;
                case MessageType.Image:
                    return NotificationType.IMAGE;
                case MessageType.Location:
                    return NotificationType.LOCATION;
                case MessageType.PdfDocument:
                    return NotificationType.DOCUMENT;
                case MessageType.Contact:
                    return NotificationType.CONTACT;
                case MessageType.PhoneContact:
                    return NotificationType.PHONE_CONTACT;
                case MessageType.StartAudioGroupCall:
                    return NotificationType.GROUP_START_AUDIO_CALL;
                case MessageType.StartVideoGroupCall:
                    return NotificationType.GROUP_START_VIDEO_CALL;
                case MessageType.EndCall:
                    return NotificationType.GROUP_END_CALL;
                default:
                    return NotificationType.NONE;


            }
        }
    }
}
