using Foundation;
using Cryptogram.CallHandler;
using Cryptogram.CallHandler.Helpers;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(Cryptogram.iOS.Call.CallConnector))]
namespace Cryptogram.iOS.Call
{
    public class CallConnector : IAudioCallConnector
    {
        UIWindow window;
        private UIViewController initialViewController;

        public CallConnector()
        {
        }

        public void Start(string channelName,string username, bool videoCallEnable, bool isCallingByMe, bool isGroupCall, byte[] avatar)
        {

            AgoraSettings.Current.IsAudioCall = !videoCallEnable;
            AgoraSettings.Current.RoomName = channelName;
            AgoraSettings.Current.EncryptionPhrase = null;
            AgoraSettings.Current.Username = username;
            AgoraSettings.Current.IsCallingByMe = isCallingByMe;
            AgoraSettings.Current.IsGroupCall = isGroupCall;
            AgoraSettings.Current.Avatar = avatar;
            window = UIApplication.SharedApplication.KeyWindow;
            if (window == null)
                window = UIApplication.SharedApplication.Delegate.GetWindow();
            if (window != null)
            {
                AppDelegate.UIViewController = window.RootViewController;
                initialViewController = new CallRoomViewController();
                window.RootViewController = initialViewController;
                window.AddSubview(initialViewController.View);
                window.MakeKeyAndVisible();
            }
            if(AppDelegate.Instance.CallManager.Calls.Count==0)
                AppDelegate.Instance.CallManager.Calls.Add(new ActiveCall(new NSUuid(), channelName, true, channelName, username, videoCallEnable));

        }
    }
}
