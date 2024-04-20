using System;
using System.Collections.Generic;
using NotificationService;
using Anonymous.Services;
using UIKit;
using UserNotifications;
using Xamarin.Forms;

[assembly: Dependency(typeof(Anonymous.iOS.IOSNotificationManager))]
namespace Anonymous.iOS
{
	public class IOSNotificationManager : INotificationManager
	{
		private readonly int _messageId = 1;
		private bool _hasNotificationsPermission;
		public static IList<string> ChatIds = new List<string>();

		public IOSNotificationManager() { }

		public void Initialize()
		{
			// request the permission to use local notifications
			UNUserNotificationCenter.Current.RequestAuthorization(UNAuthorizationOptions.Alert | UNAuthorizationOptions.Sound | UNAuthorizationOptions.Badge, (approved, err) =>
			{
				_hasNotificationsPermission = approved;
			});
		}

		public int ScheduleNotification(string title, string message, string chatId, NotificationType notificationType)
		{
			// EARLY OUT: app doesn't have permissions
			if (!_hasNotificationsPermission)
			{
				return -1;
			}
			if (!ChatIds.Contains(chatId + ""))
				ChatIds.Add(chatId + "");

			var content = new UNMutableNotificationContent()
			{
				Title = title,
				Subtitle = "",
				Body = message,
				Badge = 0,

			};
			if (App.Setting != null && App.Setting.NotificationsToneVis)
				content.Sound = UNNotificationSound.Default;
			// Local notifications can be time or location based
			// Create a time-based trigger, interval is in seconds and must be greater than 0
			var trigger = UNTimeIntervalNotificationTrigger.CreateTrigger(0.25, false);

			var request = UNNotificationRequest.FromIdentifier(_messageId.ToString(), content, trigger);
			UNUserNotificationCenter.Current.AddNotificationRequest(request, (err) =>
			{
				if (err != null)
				{
					throw new Exception($"Failed to schedule notification: {err}");
				}
			});

			return _messageId;
		}

		public void CancelNotification()
		{
			ChatIds.Clear();
			UIApplication.SharedApplication.ApplicationIconBadgeNumber = 0;
			UIApplication.SharedApplication.CancelAllLocalNotifications();
		}

		public void DisableVibratorRinging()
		{
		}

		public void CancelCallNotification(string chatId = null)
		{
		}
	}
}