using Android;
using Android.Content;
using DT.Xamarin.Agora;
using Telegraph.CallHandler;
using Telegraph.CallHandler.Helpers;
using Telegraph.Droid.Call;
using Xamarin.Forms;
using AndroidApp = Android.App.Application;

[assembly: Dependency(typeof(Connector))]
namespace Telegraph.Droid.Call
{
    class Connector : IAudioCallConnector
    {
        protected const int REQUEST_ID = 0;
        protected string[] REQUEST_PERMISSIONS = new string[] {
            Manifest.Permission.Camera,
            Manifest.Permission.WriteExternalStorage,
            Manifest.Permission.RecordAudio,
            Manifest.Permission.ModifyAudioSettings,
            Manifest.Permission.Internet,
            Manifest.Permission.AccessNetworkState
        };
        protected RtcEngine AgoraEngine;
        protected AgoraQualityHandler AgoraHandler;
        protected const string QualityFormat = "Current Connection - {0}";
        protected const string VersionFormat = " {0}";
        public Connector()
        {
            AgoraHandler = new AgoraQualityHandler();
            AgoraEngine = RtcEngine.Create(Android.App.Application.Context, AgoraTestConstants.AgoraAPI, AgoraHandler);
            AgoraEngine.EnableWebSdkInteroperability(true);
            AgoraEngine.EnableLastmileTest();
        }

        public void Start(string channelName, string username, bool videoCall, bool openCallPage, bool isCallingByMe)
        {
            AgoraSettings.Current.RoomName = channelName;
            AgoraSettings.Current.EncryptionPhrase = null;
            AgoraSettings.Current.Username = username;
            CheckPermissionsAndStartCall(username, videoCall, openCallPage, isCallingByMe);
        }

        private void CheckPermissionsAndStartCall(string username, bool videoCallEnable, bool openCallPage, bool isCallingByMe)
        {
            if (openCallPage)
            {
                var myIntent = new Intent(AndroidApp.Context, typeof(RoomActivity));
                myIntent.AddFlags(ActivityFlags.NewTask);
                myIntent.PutExtra("videoCallEnable", videoCallEnable);
                myIntent.PutExtra("chatId", AgoraSettings.Current.RoomName);
                myIntent.PutExtra("isInitialized", true);
                myIntent.PutExtra("username", username);
                myIntent.PutExtra("isCallingByMe", isCallingByMe);

                AndroidApp.Context.StartActivity(myIntent);
            }
        }
    }
}
