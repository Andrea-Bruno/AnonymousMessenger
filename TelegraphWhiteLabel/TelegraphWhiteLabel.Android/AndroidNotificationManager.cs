using System;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.App;
using AndroidX.Core.App;
using Xamarin.Forms;
using AndroidApp = Android.App.Application;

[assembly: Dependency(typeof(AnonymousWhiteLabel.Droid.AndroidNotificationManager))]
namespace AnonymousWhiteLabel.Droid
{
    public class AndroidNotificationManager
    {
        private static AndroidNotificationManager Instance;

        private const string MessageChannelId = "ChannelId";
        private const string ChatChannelName = "MessageChannelName";
        private const string _channelDescription = "The default channel for notifications.";
        private const int _pendingIntentId = 0;

        public const string TitleKey = "title";
        public const string MessageKey = "message";
        public const string NotificationType = "notificationType";
        public const string ChatId = "chatId";
        private int _messageId = 1;
        public NotificationManager Manager;

        private AndroidNotificationManager()
        {
            Manager = (NotificationManager)AndroidApp.Context.GetSystemService(AndroidApp.NotificationService);
        }

        public static AndroidNotificationManager GetInstance()
        {
            if (Instance == null)
                Instance = new AndroidNotificationManager();
            return Instance;
        }

        public void Initialize()
        {
        }

        public int ScheduleNotification(string title, string message)
        {
            CreateNotificationChannel();

            _messageId = 1;
            var intent = new Intent(AndroidApp.Context, typeof(MainActivity));
            intent.PutExtra(TitleKey, title);
            intent.PutExtra(MessageKey, message);
            var pendingIntent = PendingIntent.GetActivity(AndroidApp.Context, _pendingIntentId, intent, PendingIntentFlags.UpdateCurrent);

            Notification notification = new NotificationCompat.Builder(AndroidApp.Context, MessageChannelId)
                    .SetContentIntent(pendingIntent)
                    .SetContentTitle(title)
                    .SetContentText(message)
                    .SetAutoCancel(true)
                    .SetPriority(NotificationCompat.PriorityMax)
                    .SetDefaults(NotificationCompat.DefaultSound | NotificationCompat.DefaultVibrate)
                    //.SetLargeIcon(BitmapFactory.DecodeResource(AndroidApp.Context.Resources, AnonymousWhiteLabel.Droid.Resource.Drawable.app_logo))
                    //.SetSmallIcon(AnonymousWhiteLabel.Droid.Resource.Drawable.app_logo)
                    .Build();
            Manager.Notify(_messageId, notification);
            return _messageId;
        }

        private void CreateNotificationChannel()
        {

            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            {
                // Notification channels are new in API 26 (and not a part of the
                // support library). There is no need to create a notification 
                // channel on older versions of Android.
                return;
            }

            var channel = new NotificationChannel(MessageChannelId, new Java.Lang.String(ChatChannelName), NotificationImportance.High)
            {
                Description = _channelDescription
            };
            channel.SetShowBadge(true);
            Manager.CancelAll();
            Manager.CreateNotificationChannel(channel);
        }

        public void CancelNotification()
        {
            Manager.Cancel(1);
            Manager.GetNotificationChannel("_channelId")?.SetShowBadge(false);
        }
    }
}