using System.Linq;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using DT.Xamarin.Agora;
using Telegraph.CallHandler;
using Telegraph.CallHandler.Helpers;
using Telegraph.Droid.CallServices;
using Xamarin.Forms;

[assembly: Dependency(typeof(Connector))]
namespace Telegraph.Droid.CallServices
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

        public Connector()
        {
            AgoraHandler = new AgoraQualityHandler();
            AgoraEngine = RtcEngine.Create(Android.App.Application.Context, AgoraTestConstants.AgoraAPI, AgoraHandler);
            AgoraEngine.EnableWebSdkInteroperability(true);
            AgoraEngine.EnableLastmileTest();
        }
        protected RtcEngine AgoraEngine;
        protected AgoraQualityHandler AgoraHandler;
        protected const string QualityFormat = "Current Connection - {0}";
        protected const string VersionFormat = " {0}";
        public void Start(string channelName, bool videoCall )
        {
            AgoraSettings.Current.RoomName = channelName;
            AgoraSettings.Current.EncryptionPhrase = null;
            CheckPermissionsAndStartCall(videoCall);
        }

        private void CheckPermissionsAndStartCall(bool videoCallEnable)
        {
            if (CheckPermissions(false))
            {
                var myIntent = new Intent(MainActivity.Context, typeof(RoomActivity));
                myIntent.PutExtra("videoCallEnable", videoCallEnable);
                MainActivity.Context.StartActivity(myIntent);
            }
        }
        protected bool CheckPermissions(bool requestPermissions = true)
        {
            var isGranted = REQUEST_PERMISSIONS.Select(permission => ContextCompat.CheckSelfPermission(Android.App.Application.Context, permission) == (int)Permission.Granted).All(granted => granted);
            if (requestPermissions && !isGranted)
            {
                ActivityCompat.RequestPermissions((Activity)Android.App.Application.Context, REQUEST_PERMISSIONS, REQUEST_ID);
            }
            return isGranted;
        }


    }
}
