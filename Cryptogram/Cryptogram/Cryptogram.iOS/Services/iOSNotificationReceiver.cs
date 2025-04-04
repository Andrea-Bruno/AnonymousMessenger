using System;
using NotificationService;
using Cryptogram.CallHandler.Helpers;
using Cryptogram.iOS.Call;
using Cryptogram.Models;
using UserNotifications;

namespace Cryptogram.iOS
{
	public class iOSNotificationReceiver : UNUserNotificationCenterDelegate
	{
		public override void WillPresentNotification(UNUserNotificationCenter center, UNNotification notification, Action<UNNotificationPresentationOptions> completionHandler)
		{
			try
			{
 				App.ReEstablishConnection(true);
				var userInfo = notification.Request.Content.UserInfo;
				Console.WriteLine(userInfo);
				var userId = notification.Request.Content.UserInfo["userId"];
				var chatId = notification.Request.Content.UserInfo["chatId"];
				var notificationTypeAsString = notification.Request.Content.UserInfo["notificationType"] ;
				var notificationType = (NotificationType)Enum.Parse(typeof(NotificationType), notificationTypeAsString.ToString());
				long timeDiff = Math.Abs(new DateTimeOffset((DateTime)notification.Date).ToUnixTimeMilliseconds() - new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds());
				if (notificationType == NotificationType.RINGING && timeDiff <= 5000)
					UpdateRingingStatus(chatId.ToString());
				else if (((App)Xamarin.Forms.Application.Current) == null
					|| App.IsAppInSleepMode
					|| userId.ToString() != Xamarin.Essentials.Preferences.Get("AppId", null)
					&& App.CurrentChatId != Convert.ToUInt64(chatId.ToString()))
				{
					completionHandler(UNNotificationPresentationOptions.Alert);
				}

				
			}
			catch (Exception e)
			{

			}
		}

		
		public override void DidReceiveNotificationResponse(UNUserNotificationCenter center, UNNotificationResponse response, Action completionHandler)
		{
			try
			{
				App.ReEstablishConnection(true);
				var userInfo = response.Notification.Request.Content.UserInfo;
				var token = userInfo["userId"];
				var chatId = userInfo["chatId"];
				NotificationType notificationType = (NotificationType)Enum.Parse(typeof(NotificationType), userInfo["notificationType"].ToString());
				if (((App)Xamarin.Forms.Application.Current) == null || App.IsAppInSleepMode
						|| token.ToString() != Xamarin.Essentials.Preferences.Get("AppId", null)
						&& App.CurrentChatId != Convert.ToUInt64(chatId.ToString()))
				{
					App.GoToChat(Convert.ToUInt64(chatId.ToString()), notificationType);
				}

			}
			catch(Exception)
            {

            }
		}

		private void UpdateRingingStatus(string chatId)
		{
			if (AgoraSettings.Current?.RoomName == chatId)
			{
				CallRoomViewController.Instance?.UpdateRingingStatus();
			}
		}

	}
}