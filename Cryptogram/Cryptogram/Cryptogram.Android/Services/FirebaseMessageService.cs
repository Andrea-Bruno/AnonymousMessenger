using System;
using Android.App;
using Android.Content;
using Firebase.Messaging;
using NotificationService;
using NotificationService.Enums;
using NotificationService.helper;
using Anonymous.CallHandler.Helpers;
using Anonymous.Droid.Call;
using Anonymous.Droid.Services;
using Anonymous.Services;
using Xamarin.Forms;

[assembly: Dependency(typeof(FirebaseMessageService))]
namespace Anonymous.Droid.Services
{

	[Service(Exported = true)]
	[IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
	public class FirebaseMessageService : FirebaseMessagingService, IEndCall
	{
		public static int NumberOfUnreadedMessages = 0;
		private readonly AndroidNotificationManager notificationManager = AndroidNotificationManager.GetInstance();
		public override void OnMessageReceived(RemoteMessage message)
		{
			App.ReEstablishConnection(true);
			try
			{
				var appId = Xamarin.Essentials.Preferences.Get("AppId", null);
				message.Data.TryGetValue("userId", out string senderUserId);
				if (appId != senderUserId)
				{
					message.Data.TryGetValue("notificationType", out string typeAsString);
					NotificationType notificationType = (NotificationType)Enum.Parse(typeof(NotificationType), typeAsString);
					GeneralNotificationType generalNotificationType = NotificationTypeResolver.GetGeneralNotificationTypes(notificationType);

					UpdateBadge(generalNotificationType);
					HandleNotificationData(generalNotificationType, notificationType, message);

				}
			}catch(Exception e)
            {
				// this can happen when notification is sent from firebase console.
				Console.WriteLine(e.Message);
            }

			
		}

		private void HandleNotificationData(GeneralNotificationType generalNotificationType, NotificationType notificationType, RemoteMessage message)
        {
			message.Data.TryGetValue("body", out string body);
			message.Data.TryGetValue("title", out string title);
			message.Data.TryGetValue("chatId", out string chatId);
			message.Data.TryGetValue("remoteName", out string remoteName);
			long timeDiff = Math.Abs(message.SentTime - new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds());
			switch (generalNotificationType)
			{
				case GeneralNotificationType.P2P_CALL:
					if (notificationType == NotificationType.P2P_MISSED_AUDIO_CALL || notificationType == NotificationType.P2P_MISSED_VIDEO_CALL)
						HandleMissedCall(title, body, chatId);

					else if ((notificationType == NotificationType.P2P_START_AUDIO_CALL || notificationType == NotificationType.P2P_START_VIDEO_CALL)
						&& timeDiff <= 15000)
						HandleCallReceive(title, body, chatId, notificationType == NotificationType.P2P_START_VIDEO_CALL, remoteName);

					else if (notificationType == NotificationType.P2P_DECLINE_CALL)
						FinishCall(chatId);

					else if (notificationType == NotificationType.RINGING && timeDiff <= 5000)
						UpdateRingingStatus(chatId);

					break;

				default:
					if(App.CurrentChatId != Convert.ToUInt64(chatId.ToString()))
						notificationManager.ScheduleNotification(title, body, chatId, notificationType);
					break;
			}
		}

		private void HandleMissedCall(string title, string body, string chatId)
		{
			notificationManager.CloseCallView(chatId);
			notificationManager.ScheduleNotification(title, body, chatId, NotificationType.NONE); // show missed call notification
		}

	
		private void HandleCallReceive(string title, string body, string chatId, bool isVideoCall, string remoteName)
		{
			if (CallReceiveActivity.Context != null)
				CallReceiveActivity.Context.CloseCallDialing(CallReceiveActivity.Context.GetCurrentChatId());
			else if (AndroidNotificationManager.GetInstance().Manager.GetActiveNotifications() !=null && AndroidNotificationManager.GetInstance().Manager.GetActiveNotifications().Length > 0)
            {
				Notification notification = AndroidNotificationManager.GetInstance().Manager.GetActiveNotifications()[0].Notification;
				if(notification.Extras.GetString(AndroidNotificationManager.ChatId) !=null)
					DependencyService.Get<ICallNotificationService>().DeclineCall(notification.Extras.GetString(AndroidNotificationManager.ChatId), true); // end call when new call received
			}

			InitContext(); // this is needed when call notification is received when app is killed. it is needed to connect to socket to receive end call message. 
			if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Q)
				notificationManager.ScheduleCallNotification(title, body, chatId, isVideoCall, remoteName);
			else
				OpenCallReceiveActivity(chatId, isVideoCall, remoteName);

			DependencyService.Get<ICallNotificationService>().SetRingingStatus(chatId); // send ringing status to caller.
		}
		//[0].Notification.Extras.GetString(ChatId)

		private void InitContext()
        {
			if (!Forms.IsInitialized)
			{
				Forms.Init(this, new Android.OS.Bundle());
				App.InitContext();
			}
		}

		private void UpdateBadge(GeneralNotificationType generalNotificationType)
        {
			if ((((App)Xamarin.Forms.Application.Current) == null || App.IsAppInSleepMode) && generalNotificationType != GeneralNotificationType.P2P_CALL)
				NumberOfUnreadedMessages++;
		}

		private void OpenCallReceiveActivity(string chatId, bool isVideoCall, string username)
		{
			var intent = new Intent(ApplicationContext, typeof(CallReceiveActivity));
			intent.AddFlags(ActivityFlags.NewTask);
			intent.PutExtra("chatId", chatId);
			intent.PutExtra("videoCallEnable", isVideoCall);
			intent.PutExtra("username", username);
			ApplicationContext.StartActivity(intent);
		}

        public void FinishCall(string chatId, string remoteName = "", bool isVideoCall = false)
        {
			notificationManager.CloseCallView(chatId);
		}

        private void UpdateRingingStatus(string chatId)
        {
			if (AgoraSettings.Current?.RoomName == chatId)
				RoomActivity.Instance?.UpdateRingingStatus();
		}
	}
}
