using System;
using System.Linq;
using System.Reactive.Linq;
using AVFoundation;
using CoreFoundation;
using Firebase.CloudMessaging;
using Foundation;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Plugin.AudioRecorder;
using Plugin.FirebasePushNotification;
using PushKit;
using Syncfusion.ListView.XForms.iOS;
using Cryptogram.CallHandler.Helpers;
using Cryptogram.DesignHandler;
using Cryptogram.iOS;
using Cryptogram.iOS.Call;
using Cryptogram.Services;
using UIKit;
using UserNotifications;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: Dependency(typeof(AppDelegate))]
namespace Cryptogram.iOS
{
    [Register("AppDelegate")]
    public partial class AppDelegate : FormsApplicationDelegate, IUNUserNotificationCenterDelegate, IMessagingDelegate, IPKPushRegistryDelegate, IEndCall
    {
        public static AppDelegate Instance;
        private IOSNotificationManager iOSNotificationManager;

        public static UIViewController UIViewController;
        public ActiveCallManager CallManager { get; set; }
        public ProviderDelegate CallProviderDelegate { get; set; }

        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            AppCenter.Start("33f12524-2737-4973-b2fb-b89d4ee6360c",
                   typeof(Analytics), typeof(Crashes));
            Instance = this;
            //Forms.SetFlags("SwipeView_Experimental");
            Rg.Plugins.Popup.Popup.Init();
            Forms.Init();
            //CachedImageRenderer.Init();
            //CachedImageRenderer.InitImageSourceHandler();
            InitSycnfusion();
            UNUserNotificationCenter.Current.Delegate = new iOSNotificationReceiver();
            ZXing.Net.Mobile.Forms.iOS.Platform.Init(); // qr code scanner
            InitAudioManager();
            InitNotificationManager(options);
            RegisterVoip();
            CallManager = new ActiveCallManager();
            CallProviderDelegate = new ProviderDelegate(CallManager);
            LoadApplication(new App());
            UIApplication.SharedApplication.IdleTimerDisabled = true; // disable standby
            SetNavigationBarColors();
            return base.FinishedLaunching(app, options);

        }

        private void InitNotificationManager(NSDictionary options)
        {
            iOSNotificationManager = new IOSNotificationManager();
            iOSNotificationManager.Initialize();
            Firebase.Core.App.Configure();

            FirebasePushNotificationManager.Initialize(options, true);

            if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
            {
                UNUserNotificationCenter.Current.Delegate = new iOSNotificationReceiver();
                Messaging.SharedInstance.Delegate = this;
            }
            else
            {
                var allNotificationTypes = UIUserNotificationType.Alert | UIUserNotificationType.Badge | UIUserNotificationType.Sound;
                var settings = UIUserNotificationSettings.GetSettingsForTypes(allNotificationTypes, null);
                UIApplication.SharedApplication.RegisterUserNotificationSettings(settings);

            }
            UIApplication.SharedApplication.RegisterForRemoteNotifications();
        }

        private void InitAudioManager()
        {
            AudioPlayer.RequestAVAudioSessionCategory(AVAudioSessionCategory.Playback);
            AudioRecorderService.RequestAVAudioSessionCategory(AVAudioSessionCategory.Record);
            UIApplication.SharedApplication.SetMinimumBackgroundFetchInterval(UIApplication.BackgroundFetchIntervalMinimum);
            AudioPlayer.OnPrepareAudioSession = audioSession =>
            {
                var success = audioSession.OverrideOutputAudioPort(AVAudioSessionPortOverride.Speaker, out NSError error);
            };
        }

        private void InitSycnfusion()
        {
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Mzc3MTg3QDMxMzgyZTM0MmUzMFk0TkIzd1YwUUEvNkJjcDNzd2dMYk9pU1ZFWVk2NEx1aUhyUytSZWl6Q3M9");
            Syncfusion.XForms.iOS.Buttons.SfSwitchRenderer.Init();
            Syncfusion.SfImageEditor.XForms.iOS.SfImageEditorRenderer.Init();
            Syncfusion.SfPdfViewer.XForms.iOS.SfPdfDocumentViewRenderer.Init();
            Syncfusion.SfRangeSlider.XForms.iOS.SfRangeSliderRenderer.Init();
            SfListViewRenderer.Init();
        }

        private void SetNavigationBarColors()
        {
            UIColor tintColor = DesignResourceManager.GetColorFromStyle("Color1").ToUIColor();
            UINavigationBar.Appearance.BarTintColor = tintColor;
            UINavigationBar.Appearance.TintColor = tintColor;
            UINavigationBar.Appearance.TitleTextAttributes = new UIStringAttributes() { ForegroundColor = tintColor };
            UINavigationBar.Appearance.Translucent = true;
        }

        public override bool OpenUrl(UIApplication application, NSUrl url, string sourceApplication, NSObject annotation)
        {
            LoadApplication(new App());
            return true;
        }

        public override void RegisteredForRemoteNotifications(UIApplication application, NSData deviceToken)
        {
            System.Diagnostics.Debug.WriteLine($" FCM device Token: { deviceToken } ");
            //App.NewDeviceToken = SplitDeviceToken(deviceToken);
             FirebasePushNotificationManager.DidRegisterRemoteNotifications(deviceToken);
        }

        //for notification
        public override void DidReceiveRemoteNotification(UIApplication application, NSDictionary userInfo, Action<UIBackgroundFetchResult> completionHandler)
        {
            FirebasePushNotificationManager.DidReceiveMessage(userInfo);
            System.Console.WriteLine(userInfo);

            completionHandler(UIBackgroundFetchResult.NewData);
        }


        public override void WillTerminate(UIApplication uiApplication)
        {
            App.DisableProximitySensor();
            base.WillTerminate(uiApplication);
        }

        [Export("messaging:didReceiveRegistrationToken:")]
        public void DidReceiveRegistrationToken(Messaging messaging, string token)
        {
            System.Diagnostics.Debug.WriteLine($" FCM Token: { token } ");
            App.UpdateFirebaseToken(token);
        }

        public override void FailedToRegisterForRemoteNotifications(UIApplication application, NSError error)
        {
            FirebasePushNotificationManager.RemoteNotificationRegistrationFailed(error);
        }

        public override void OnActivated(UIApplication application)
        {
            base.OnActivated(application);
            UIApplication.SharedApplication.ApplicationIconBadgeNumber = 0;
            UIApplication.SharedApplication.CancelAllLocalNotifications();
            CallRoomViewController.Instance?.ListenProximitySensor();
        }

        public override void PerformFetch(UIApplication application, Action<UIBackgroundFetchResult> completionHandler)
        {
            completionHandler(UIBackgroundFetchResult.NewData);
        }

        public static void ChangeTheme(bool isDarkTheme)
        {
            if (Instance.Window == null)
                Instance.Window = new UIWindow(UIScreen.MainScreen.Bounds);
            Instance.Window.OverrideUserInterfaceStyle = isDarkTheme ? UIUserInterfaceStyle.Dark : UIUserInterfaceStyle.Light;
        }

        public void DidUpdatePushCredentials(PKPushRegistry registry, PKPushCredentials credentials, string type)
        {
            if (credentials != null && credentials.Token != null)
                App.UpdateDeviceToken(SplitDeviceToken(credentials.Token));
        }

        public void DidReceiveIncomingPush(PKPushRegistry registry, PKPushPayload payload, string type)
        {
            App.ReEstablishConnection(true);

            var value = payload.DictionaryPayload["aps"].ValueForKey((NSString)"alert").ToString();
            var lst = value.Split("?>@#");
            if (!string.IsNullOrEmpty(lst[0]))
                CallProviderDelegate.ReportIncomingCall(new NSUuid(), lst[0], lst[1], Convert.ToBoolean(lst[2]), false);
            else
                CallProviderDelegate.ReportIncomingCall(new NSUuid(), "", lst[1], Convert.ToBoolean(lst[2]), true); //  cancel call if there is any
            if (!Forms.IsInitialized) // when app is killed, it is needed to init forms in order to receive end call message
            {
                Forms.Init();
                App.InitContext();
            }
            if (lst.Length >= 2)
                DependencyService.Get<ICallNotificationService>().SetRingingStatus(lst[1]); // send ringing status to caller.

        }

        public override void ReceivedLocalNotification(UIApplication application, UILocalNotification notification)
        {
            // show an alert
            UIAlertController okayAlertController = UIAlertController.Create(notification.AlertAction, notification.AlertBody, UIAlertControllerStyle.Alert);
            okayAlertController.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));

            // reset our badge
            UIApplication.SharedApplication.ApplicationIconBadgeNumber = 0;
            UIApplication.SharedApplication.CancelAllLocalNotifications();
        }

        void RegisterVoip()
        {
            var mainQueue = DispatchQueue.MainQueue;
            PKPushRegistry voipRegistry = new PKPushRegistry(mainQueue);
            voipRegistry.Delegate = this;
            voipRegistry.DesiredPushTypes = new NSSet(new string[] { PushKit.PKPushType.Voip });
        }


        public override bool ContinueUserActivity(UIApplication application, NSUserActivity userActivity, UIApplicationRestorationHandler completionHandler)
        {
            var handle = StartCallRequest.CallHandleFromActivity(userActivity);

            // Found?
            if (handle == null)
            {
                // No, report to system
                Console.WriteLine("Unable to get call handle from User Activity: {0}", userActivity);
                return false;
            }
            else
            {
                // Yes, start call and inform system
                CallManager.StartCall(handle);
                return true;
            }
        }

        private string SplitDeviceToken(NSData token)
        {
            byte[] bytes = token.ToArray();
            string[] hexArray = bytes.Select(b => b.ToString("x2")).ToArray();
            var fullToken = string.Join(string.Empty, hexArray);
            string splittedToken = null;
            for (int i = 0; i < fullToken.Length; i += 8)
            {
                splittedToken += fullToken.Substring(i, 8);
                if (i < fullToken.Length - 9)
                    splittedToken += " ";
            }
            return splittedToken;
        }

        public void FinishCall( string chatId, string remoteName = "", bool isVideoCall = false)
        {
            if (AgoraSettings.Current?.RoomName == chatId
                && CallRoomViewController.Instance != null
                && string.IsNullOrEmpty(remoteName)) // name is empty when voip for end call is sent
            {
                CallRoomViewController.Instance?.FinishCall(false, false);
                return;
            }
            foreach (ActiveCall activeCall in Instance.CallManager?.Calls) { //  during another call receive call and then receive end call event
                if (activeCall.ChatId == chatId)
                {
                    Instance?.CallManager?.EndCall(activeCall);
                    break;
                }
            }
            // ignore other cases

        }
    }

}