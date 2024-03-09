using System;
namespace NotificationService
{
    public enum NotificationType
    {
        NONE,

        TEXT,
        IMAGE,
        AUDIO,
        LOCATION,
        DOCUMENT,
        CONTACT,
        PHONE_CONTACT,

        P2P_START_AUDIO_CALL,
        P2P_START_VIDEO_CALL,
        P2P_MISSED_AUDIO_CALL,
        P2P_MISSED_VIDEO_CALL,
        P2P_DECLINE_CALL,

        GROUP_START_AUDIO_CALL,
        GROUP_START_VIDEO_CALL,
        GROUP_END_CALL,

        RINGING,


    }
}
