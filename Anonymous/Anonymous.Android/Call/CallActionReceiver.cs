using System;
using Android.Content;
using Telegraph.CallHandler;
using Telegraph.CallHandler.Helpers;
using Telegraph.Droid.Services;
using Telegraph.Services;
using Xamarin.Forms;
using AndroidApp = Android.App.Application;

namespace Telegraph.Droid.Call
{
    [BroadcastReceiver(Enabled = true)]
    public class CallActionReceiver : BroadcastReceiver
    {
        public CallActionReceiver()
        {
        }

        public override void OnReceive(Context context, Intent intent)
        {

            bool callCancelled = intent.GetBooleanExtra("callCancelled", false);
            Intent it = new Intent(Intent.ActionCloseSystemDialogs);
            context.SendBroadcast(it);
            AndroidNotificationManager.GetInstance().CancelNotification();
            AndroidNotificationManager.GetInstance().DisableVibratorRinging();
            if (!Forms.IsInitialized)
                Forms.Init(context, new Android.OS.Bundle());
            if (callCancelled)
            {
                DependencyService.Get<ICallNotificationService>().DeclineCall(intent.GetStringExtra("chatId"), true); // click to cancel call on notification
            }
            else
            {
                AndroidNotificationManager.GetInstance().CloseCallView(AgoraSettings.Current?.RoomName); // click to accept call on notification
                DependencyService.Get<IAudioCallConnector>().Start(intent.GetStringExtra("chatId"),
                    intent.GetStringExtra("username"),
                    intent.GetBooleanExtra("videoCallEnable", false),
                    intent.GetBooleanExtra("isCallingByMe", false),
                    false,
                    null);
            }
        }

    }
}
