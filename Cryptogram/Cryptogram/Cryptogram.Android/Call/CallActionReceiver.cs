using Android.Content;
using Cryptogram.CallHandler;
using Cryptogram.CallHandler.Helpers;
using Cryptogram.Droid.Services;
using Cryptogram.Services;
using Xamarin.Forms;

namespace Cryptogram.Droid.Call
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
            bool isCallOnGoing = intent.GetBooleanExtra("isCallOnGoing", false);
            Intent it = new Intent(Intent.ActionCloseSystemDialogs);
            context.SendBroadcast(it);
            AndroidNotificationManager.GetInstance().CancelCallNotification(intent.GetStringExtra("chatId"));
            AndroidNotificationManager.GetInstance().DisableVibratorRinging();
            if (!Forms.IsInitialized)
                Forms.Init(context, new Android.OS.Bundle());
            if (isCallOnGoing)
            {
                AndroidNotificationManager.GetInstance().CancelOnGoingCallNotification();
                RoomActivity.Instance?.EndCall(true);
            }
            else if (callCancelled)
            {
                DependencyService.Get<ICallNotificationService>().DeclineCall(intent.GetStringExtra("chatId"), true); // click to cancel call on notification
            }
            else if (!callCancelled)
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
