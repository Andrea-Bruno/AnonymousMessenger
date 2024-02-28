using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Media;
using Android.OS;
using Android.Support.V4.App;
using NotificationService;
using Telegraph.CallHandler.Helpers;
using Telegraph.Droid.Call;
using Telegraph.Services;
using Xamarin.Forms;
using AndroidApp = Android.App.Application;

[assembly: Dependency(typeof(Telegraph.Droid.Services.AndroidNotificationManager))]
namespace Telegraph.Droid.Services
{
    public class AndroidNotificationManager : INotificationManager
    {
        private static AndroidNotificationManager Instance;

        private const string MessageChannelId = "UUPMessageChannelId";
        private const string CallChannelId = "UUPCallChannelId";
        private const string MessageChannelName = "UUPMessageChannelName";
        private const string ChatChannelName = "UUPMessageChannelName";
        private const string _channelDescription = "The default channel for notifications.";
        private const int _pendingIntentId = 0;

        public const string TitleKey = "title";
        public const string MessageKey = "message";
        public const string NotificationType = "notificationType";
        public const string ChatId = "chatId";
        private int _messageId = 1;
        public NotificationManager Manager;
        private Vibrator Vibrator;
        private Ringtone Ringtone;

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

        public int ScheduleNotification(string title, string message, string chatId, NotificationType notificationType)
        {
            CreateNotificationChannel();

            _messageId = 1;
            var intent = new Intent(AndroidApp.Context, typeof(MainActivity));
            intent.PutExtra(TitleKey, title);
            intent.PutExtra(MessageKey, message);
            intent.PutExtra(NotificationType, notificationType.ToString());
            intent.PutExtra(ChatId, notificationType.ToString());
            var pendingIntent = PendingIntent.GetActivity(AndroidApp.Context, _pendingIntentId, intent, PendingIntentFlags.UpdateCurrent);

            Notification notification = new NotificationCompat.Builder(AndroidApp.Context, MessageChannelId)
                    .SetContentIntent(pendingIntent)
                    .SetContentTitle(title)
                    .SetContentText(message)
                    .SetAutoCancel(true)
                    .SetPriority(NotificationCompat.PriorityHigh)
                    .SetDefaults(NotificationCompat.DefaultSound | NotificationCompat.DefaultVibrate)
                    .SetLargeIcon(BitmapFactory.DecodeResource(AndroidApp.Context.Resources, Resource.Drawable.Company_logo))
                    .SetNumber(FirebaseMessageService.NumberOfUnreadedMessages)
                    .SetSmallIcon(Resource.Drawable.Company_logo).Build();
            Manager.Notify(_messageId, notification);
            return _messageId;
        }

        public int ScheduleCallNotification(string title, string message, string chatId, bool isVideoCall, string userName)
        {
            _messageId = (int)(Convert.ToUInt64(chatId)%1000000000);
            CreateCallNotificationChannel();

            var pendingIntent = new Intent(AndroidApp.Context, typeof(CallReceiveActivity));
            pendingIntent.PutExtra("chatId", chatId);
            pendingIntent.PutExtra("videoCallEnable", isVideoCall);
            pendingIntent.PutExtra("username", userName);

            var callPageIntent = new Intent(AndroidApp.Context, typeof(CallActionReceiver));
            callPageIntent.PutExtra("callCancelled", false);
            callPageIntent.PutExtra("videoCallEnable", isVideoCall);
            callPageIntent.PutExtra("chatId", chatId);
            callPageIntent.PutExtra("isCallingByMe", false);
            callPageIntent.PutExtra("username", userName);


            var cancelIntent = new Intent(AndroidApp.Context, typeof(CallActionReceiver));
            cancelIntent.PutExtra("callCancelled", true);
            cancelIntent.PutExtra("chatId", chatId);

            var pageIntent = PendingIntent.GetActivity(AndroidApp.Context, _pendingIntentId, pendingIntent, PendingIntentFlags.UpdateCurrent);
            var callPendingIntent = PendingIntent.GetBroadcast(AndroidApp.Context, _pendingIntentId, callPageIntent, PendingIntentFlags.OneShot);
            var cancelPendingIntent = PendingIntent.GetBroadcast(AndroidApp.Context, _pendingIntentId, cancelIntent, PendingIntentFlags.CancelCurrent);
            Bundle bundle = new Bundle();
            bundle.PutString(ChatId, chatId);

            NotificationCompat.Builder builder = new NotificationCompat.Builder(AndroidApp.Context, CallChannelId)
                    .SetContentIntent(pageIntent)
                    .SetContentTitle(title)
                    .SetContentText(message)
                    .SetPriority(NotificationCompat.PriorityHigh)
                    .SetCategory(NotificationCompat.CategoryCall)
                    .SetLargeIcon(BitmapFactory.DecodeResource(AndroidApp.Context.Resources, Resource.Drawable.Company_logo))
                    .SetSmallIcon(Resource.Drawable.Company_logo)
                    .AddAction(Resource.Drawable.Company_logo, "Accept Call", callPendingIntent)
                    .AddAction(Resource.Drawable.Company_logo, "Cancel call", cancelPendingIntent)
                    .SetAutoCancel(false)
                    .AddExtras(bundle)
                    .SetNumber(FirebaseMessageService.NumberOfUnreadedMessages)
                    .SetOngoing(true)
                    .SetFullScreenIntent(pageIntent, true);


            Notification notification = builder.Build();
            Manager.Notify(_messageId, notification);
            return _messageId;
        }

        private void CreateNotificationChannel()
        {

            var channelNameJava = new Java.Lang.String(MessageChannelName);
            var channel = new NotificationChannel(MessageChannelId, channelNameJava, NotificationImportance.High)
            {
                Description = _channelDescription
            };
            long[] vibrationPattern = { 100, 200, 300, 400 };
            channel.SetVibrationPattern(vibrationPattern);
            channel.EnableVibration(true);
            channel.SetShowBadge(true);
            Manager.Cancel(1);
            Manager.CreateNotificationChannel(channel);

        }

        private void CreateCallNotificationChannel()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var channelNameJava = new Java.Lang.String(ChatChannelName);
                var channel = new NotificationChannel(CallChannelId, channelNameJava, NotificationImportance.High)
                {
                    Description = _channelDescription

                };
                channel.SetSound(null, null);
                channel.EnableVibration(false);
                channel.SetShowBadge(true);
                SetVibrator();
                SetRingtone();
                Manager.CancelAll();
                Manager.CreateNotificationChannel(channel);
            }

        }

        public void CancelNotification()
        {
            Manager.CancelAll();
            FirebaseMessageService.NumberOfUnreadedMessages = 0;
            Manager.GetNotificationChannel("_channelId")?.SetShowBadge(false);
        }

        public void CancelCallNotification(string chatId = null)
        {
            Manager.Cancel((int)(Convert.ToUInt64(chatId) % 1000000000));
            DisableVibratorRinging();
        }

        private void SetVibrator()
        {
            long[] pattern = { 0, 100, 200, 300, 400 };

            Vibrator = (Vibrator)AndroidApp.Context.GetSystemService(AndroidApp.VibratorService);
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                Vibrator.Vibrate(VibrationEffect.CreateWaveform(pattern, 0),
                    new AudioAttributes.Builder()
                   .SetContentType(AudioContentType.Sonification)
                   .SetUsage(AudioUsageKind.Alarm)
                   .Build());
            }
            else
            {
                Vibrator.Vibrate(pattern, 0);
            }
        }

        private void SetRingtone()
        {
            Android.Net.Uri notification = RingtoneManager.GetDefaultUri(RingtoneType.Ringtone);
            Ringtone = RingtoneManager.GetRingtone(AndroidApp.Context, notification);
            Ringtone.Play();
            Ringtone.Looping = true;
        }

        public void CloseCallView(string chatId)
        {
            if (RoomActivity.Instance != null && AgoraSettings.Current.RoomName == chatId)  // there can be some delay on notification receive and roomactivity may be open.
            {
                RoomActivity.Instance.EndCall(false);
                return;
            }
            if (CallReceiveActivity.Context != null && CallReceiveActivity.Context.GetCurrentChatId() == chatId)
                CallReceiveActivity.Context.CloseCallDialing(chatId);

            CancelCallNotification(chatId);// remove call notification if founds any
        }


        public void DisableVibratorRinging()
        {
            if (Vibrator != null)
            {
                Vibrator.Cancel();
                Vibrator = null;
            }
            if (Ringtone != null && Ringtone.IsPlaying)
            {
                Ringtone.Stop();
                Ringtone = null;
            }
            CancelNotification();
        }
    }
}