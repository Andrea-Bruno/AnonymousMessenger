using System;
using Android.App;
using Android.Content;
using Android.OS;
using Cryptogram.Droid.Services;
using Xamarin.Forms;

[assembly: Dependency(typeof(TaskKilledService))]
namespace Cryptogram.Droid.Services
{
    [Service(Enabled = true, Exported = false)]
    public class TaskKilledService : IntentService
    {
        public TaskKilledService()
        {
        }

        public override void OnTaskRemoved(Intent rootIntent)
        {
            base.OnTaskRemoved(rootIntent);
            AndroidNotificationManager.GetInstance().CancelOnGoingCallNotification();
        }

        protected override void OnHandleIntent(Intent intent)
        {
        }
    }
}
