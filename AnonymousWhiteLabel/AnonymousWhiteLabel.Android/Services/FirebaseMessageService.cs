using System;
using Android.App;
using AnonymousWhiteLabel;
using AnonymousWhiteLabel.Droid;
using Firebase.Messaging;
using TelegraphWhiteLabel.Droid.Services;
using Xamarin.Forms;

[assembly: Dependency(typeof(FirebaseMessageService))]
namespace TelegraphWhiteLabel.Droid.Services
{
    [Service(Exported = true)]
    [IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
    public class FirebaseMessageService : FirebaseMessagingService
	{
		private readonly AndroidNotificationManager notificationManager = AndroidNotificationManager.GetInstance();
		public override void OnMessageReceived(RemoteMessage message)
		{
			App.ReEstablishConnection(true);
			try
			{
				notificationManager.ScheduleNotification(Localization.Resources.Dictionary.StrictlyConfidentialMessage, Localization.Resources.Dictionary.NewMessage);
			}
			catch (Exception e)
			{
				// this can happen when notification is sent from firebase console.
				Console.WriteLine(e.Message);
			}


		}
	}
}
