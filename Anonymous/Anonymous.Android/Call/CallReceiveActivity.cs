using System;
using Android;
using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using Telegraph.CallHandler;
using Telegraph.Droid.Services;
using Telegraph.Services;
using Xamarin.Forms;
namespace Telegraph.Droid.Call
{
    [Activity(Label = "Uup",
     Theme = "@style/Theme.CallReceive", NoHistory = true , ShowForAllUsers = true, ShowWhenLocked = true, ExcludeFromRecents =false, LaunchMode = Android.Content.PM.LaunchMode.SingleInstance)]
    public class CallReceiveActivity : Activity
    {
        private ImageView decline, accept;
        private TextView name, callType;
        private string chatId;
        private bool _videoCallEnable;
        private static bool isCallingContinue;
        private int timer = 0;
        public static CallReceiveActivity Context;
        private Vibrator Vibrator;
        private Ringtone Ringtone;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.CallReceive);
            if(!Forms.IsInitialized)
                Forms.Init(this, savedInstanceState);
            decline = FindViewById<ImageView>(Resource.Id.decline);
            accept = FindViewById<ImageView>(Resource.Id.accept);
            name = FindViewById<TextView>(Resource.Id.name);
            callType = FindViewById<TextView>(Resource.Id.call_type);
            _videoCallEnable = Intent.GetBooleanExtra("videoCallEnable", false);
            chatId = Intent.GetStringExtra("chatId");
            callType.Text = _videoCallEnable ? Localization.Resources.Dictionary.VideoCall : Localization.Resources.Dictionary.AudioCall;
            name.Text = Intent.GetStringExtra("username");
            timer = 0;
            isCallingContinue = true;
            Device.StartTimer(TimeSpan.FromSeconds(5), CheckTimeout);
            decline.Click += delegate
            {
                EndCall();
            };
            accept.Click += delegate
            {
                DisableVibratorRinging();
                AskMicrophonePermission();
            };
            AndroidNotificationManager.GetInstance().DisableVibratorRinging();
            AndroidNotificationManager.GetInstance().CancelNotification();
            SetVibrator();
            SetRingtone();
            DisableStandby();
        }

        private void FinishPage()
        {
            if (IsTaskRoot)
                FinishAndRemoveTask();
            else
                Finish();
        }

        private void EndCall()
        {
            isCallingContinue = false;
            DisableVibratorRinging();
            DependencyService.Get<ICallNotificationService>().DeclineCall(chatId, true);
            Context.Finish();
            Context = null;
            FinishPage(); ;

        }

        private bool CheckTimeout()
        {
            if (timer < 30)
                timer += 5;

            else
                EndCall();

            return isCallingContinue;
        }

        private void ConnectCall()
        {
            DependencyService.Get<IAudioCallConnector>().Start(chatId, name.Text, _videoCallEnable, false, false, null);
            isCallingContinue = false;
            DisableVibratorRinging();
            Context.Finish();
            Context = null;
            Finish();
        }

        public override void OnAttachedToWindow()
        {
            Window.AddFlags(WindowManagerFlags.ShowWhenLocked |
                            WindowManagerFlags.KeepScreenOn |
                            WindowManagerFlags.DismissKeyguard |
                            WindowManagerFlags.TurnScreenOn);
        }
        private void AskMicrophonePermission()
        {
            if (ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.RecordAudio))
            {
                ActivityCompat.RequestPermissions(this, new string[] { Manifest.Permission.RecordAudio, Manifest.Permission.Camera, Manifest.Permission.WriteExternalStorage }, 1);
            }
            else
                ConnectCall();
        }
        public override async void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            switch (requestCode)
            {
                case 1:
                    {
                        if (grantResults[0] == (int)Android.Content.PM.Permission.Granted && grantResults[1] == (int)Android.Content.PM.Permission.Granted)
                            ConnectCall();
                        else
                            Toast.MakeText(this, Localization.Resources.Dictionary.RequestedPermissionIsNeeded, ToastLength.Short).Show();
                    }
                    break;
            }
            //base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        public string GetCurrentChatId()
        {
            return chatId;
        }

        public void CloseCallDialing(string _chatId)
        {
            if (Context != null && chatId == _chatId)
                EndCall();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            CloseCallDialing(chatId);
            Context = null;
        }

        protected override void OnPause()
        {
            base.OnPause();
            DisableVibratorRinging();
        }
        protected override void OnResume()
        {
            base.OnResume();
            Context = this;
        }
        private void SetVibrator()
        {
            long[] pattern = { 0, 100, 200, 300, 400 };
            Vibrator = (Vibrator)GetSystemService(VibratorService);
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
            Ringtone = RingtoneManager.GetRingtone(this, notification);
            Ringtone.Play();
            Ringtone.Looping = true;
        }
        private void DisableVibratorRinging()
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
        }
        private void DisableStandby()
        {
            Window.AddFlags(WindowManagerFlags.KeepScreenOn);
        }
        public override void OnBackPressed()
        {
            // base.OnBackPressed();
        }
    }
}





