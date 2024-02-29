using Android.Content;
using DT.Xamarin.Agora;
using Telegraph.CallHandler;
using Telegraph.CallHandler.Helpers;
using Telegraph.Droid.Call;
using Xamarin.Forms;
using AndroidApp = Android.App.Application;

[assembly: Dependency(typeof(CallConnector))]
namespace Telegraph.Droid.Call
{
    public class CallConnector : IAudioCallConnector
    {
        protected RtcEngine AgoraEngine;
        public CallConnector()
        {
            AgoraEngine = RtcEngine.Create(AndroidApp.Context, AgoraTestConstants.AgoraAPI, new AgoraQualityHandler());
            AgoraEngine.EnableWebSdkInteroperability(true);
            AgoraEngine.EnableLastmileTest();
        }

        public void Start(string channelName, string username, bool videoCall, bool isCallingByMe, bool isGroupCall, byte[] avatar)
        {
            AgoraSettings.Current.RoomName = channelName;
            AgoraSettings.Current.EncryptionPhrase = null;
            AgoraSettings.Current.Username = username;
            AgoraSettings.Current.IsCallingByMe = isCallingByMe;
            AgoraSettings.Current.IsAudioCall = !videoCall;
            AgoraSettings.Current.IsGroupCall = isGroupCall;
            AgoraSettings.Current.Avatar = avatar;

            var myIntent = new Intent(AndroidApp.Context, typeof(RoomActivity));
            myIntent.AddFlags(ActivityFlags.NewTask);
            AndroidApp.Context.StartActivity(myIntent);
        }
    }
}
