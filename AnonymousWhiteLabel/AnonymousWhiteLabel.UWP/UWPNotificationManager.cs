using System;
using Xamarin.Forms;
using Windows.UI.Notifications;
using Windows.Web.UI;

[assembly: Dependency(typeof(AnonymousWhiteLabel.UWP.UWPNotificationManager))]
namespace AnonymousWhiteLabel.UWP
{
	public class UWPNotificationManager : INotificationManager
	{
		int _messageId = -1;

		public int ScheduleNotification(string title, string message, int delaySec)
		{
			_messageId++;
			var toastXml =
				$@"<toast>
           <visual>
              <binding template='ToastGeneric'>
                <text>{title}</text>
                <text>{message}</text>
              </binding>
            </visual>
						<audio src =""ms-winsoundevent:Notification.Reminder""/>
         </toast>";

			//	< audio src = ""ms - winsoundevent:Notification.Mail"" loop = ""true"" />

			var xml = new Windows.Data.Xml.Dom.XmlDocument();
			xml.LoadXml(toastXml);
			var toast = new ScheduledToastNotification(xml, DateTime.Now.AddSeconds(delaySec))
			{
				Id = "IdTostone" + _messageId.ToString(System.Globalization.CultureInfo.InvariantCulture),
				Tag = "NotificationOne",
				Group = nameof(UWPNotificationManager)
			};
			ToastNotificationManager.CreateToastNotifier().AddToSchedule(toast);
			return _messageId;
		}

		public void Initialize()
		{
		}
		public int ScheduleNotification(string title, string message) => ScheduleNotification(title, message, 5);
		public event EventHandler NotificationReceived;
		public void ReceiveNotification(string title, string message)
		{
			var args = new NotificationEventArgs()
			{
				Title = title,
				Message = message
			};
			NotificationReceived?.Invoke(null, args);
		}
	}
}