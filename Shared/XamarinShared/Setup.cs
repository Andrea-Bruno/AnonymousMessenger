using System;
using System.Diagnostics;
using EncryptedMessaging;
using Xamarin.Forms;
using XamarinShared.ViewCreator;
          
namespace XamarinShared
{
    public static class Setup
    {
        internal static Context Context;

        public static MessageReadStatus MessageReadStatus;
        // public static Context Initialize(Context.OnMessageArrived onNotification, ChatPageSupport.OnNewMessageAddedToView onNewMessageAddedToView, string entryPoint, string networkName, bool multipleChatModes, bool newMessageOnTop, string privateKeyOrPassphrase = null, PaletteSetting paletteSetting = null)
        public static Context Initialize(Context.OnMessageArrived onNotification, ChatPageSupport.OnNewMessageAddedToView onNewMessageAddedToView, string entryPoint, string networkName, bool multipleChatModes, bool newMessageOnTop, string privateKeyOrPassphrase = null, PaletteSetting paletteSetting = null, Action<Context> OnInitialized = null, Func<string> getFirebaseToken = null, Func<string> getAppleDeviceToken = null, OEM oem = null)

        {
            if (paletteSetting == null)
                Palette.Colors = new PaletteSetting() { ForegroundColor = Color.FromHex("#E2E8F3"), BackgroundColor = Color.FromHex("#B7CBF2") };
            else Palette.Colors = paletteSetting;
            if (Context != null)
            {
                Debugger.Break(); // You don't have to go in here! There is something conceptually wrong with creating the interface! Correct the UI logic! It must not attempt to instantiate the context twice
            }
            else
            {
                MessageReadStatus = new MessageReadStatus();
                //messageReadStatus for status label
                MessageViewCreator.Instance.SetReadStatus(MessageReadStatus);
                ChatPageSupport.Initialize(multipleChatModes, newMessageOnTop, onNewMessageAddedToView);
                // ChatPageSupport.Initialize(multipleChatModes, newMessageOnTop, messageScrollView, onNewMessageAddedToView);
                var currentInternetAccess = Xamarin.Essentials.Connectivity.NetworkAccess == Xamarin.Essentials.NetworkAccess.Internet || Xamarin.Essentials.Connectivity.NetworkAccess == Xamarin.Essentials.NetworkAccess.ConstrainedInternet;
                Context.OnContextIsInitialized += OnInitialized;

                Context = new Context(entryPoint, networkName, multipleChatModes, privateKeyOrPassphrase, Modality.Client, currentInternetAccess, Device.BeginInvokeOnMainThread, GetSecureValue, SetSecureValue, getFirebaseToken, getAppleDeviceToken, null, oem);
                Context.ViewMessage += ChatPageSupport.ViewMessage;
                Context.OnContactEvent +=ChatPageSupport.OnContactEvent;
                Context.OnLastReadedTimeChange += MessageReadStatus.OnLastReadedTimeChange;
                Context.OnMessageDelivered += MessageReadStatus.OnMessageDelivered;
                Context.OnNotification += onNotification;
                // Bind the event to change the connection when the connectivity changes
                Xamarin.Essentials.Connectivity.ConnectivityChanged += (o, c) => Context.OnConnectivityChange(c.NetworkAccess == Xamarin.Essentials.NetworkAccess.Internet || Xamarin.Essentials.Connectivity.NetworkAccess == Xamarin.Essentials.NetworkAccess.ConstrainedInternet, CommunicationChannel.Channel.ConnectivityType.Internet);
                // Set the current connection status
                Context.OnConnectivityChange(Xamarin.Essentials.Connectivity.NetworkAccess == Xamarin.Essentials.NetworkAccess.Internet || Xamarin.Essentials.Connectivity.NetworkAccess == Xamarin.Essentials.NetworkAccess.ConstrainedInternet, CommunicationChannel.Channel.ConnectivityType.Internet);
            }
            Settings.Load();
            Calls.GetInstance().SetMyId(Context.My.GetId());

            //Context.Contacts.ForEachContact(x => x.Save(true));
            //TelegrahLibrary.Cloud.SendCloudCommands.GetAllObject(Context, "Contact");
            //TelegrahLibrary.Cloud.SendCloudCommands.PostPushNotification(Context, "740f4707 bebcf74f 9b7c25d4 8e335894 5f6aa01d a5ddb387 462c7eaf 61bb78ad", 12345678910, true, "Andrea");

            //Context.My.SetAvatar(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 });

            //Context.Contacts.RestoreContactFromCloud();


            return Context;
        }

        public static void SetSecureValue(string key, string value) => _ = Xamarin.Essentials.SecureStorage.SetAsync(key, value);

        public static void RemoveSecureValue(string key) => _ = Xamarin.Essentials.SecureStorage.Remove(key);

        public static string GetSecureValue(string key)
        {
            System.Threading.Tasks.Task<string> task = System.Threading.Tasks.Task.Run(async () => await Xamarin.Essentials.SecureStorage.GetAsync(key));
            return task.Result;
        }

        public static Settings Setting;
        public class Settings
        {
            public string Pseudonym = null;
            public bool NameVis = true;
            public bool PicVis = true;
            public bool ThemeVis = false;
            public bool MessagePreloading = true;
            public bool SendContact = true;
            public bool AllowOtherApp = true;
            public bool KeyBoardVis = false;
            public bool NotificationsVis = true;
            public bool NotificationsToneVis = true;
            public bool VibrateVis = true;
            public bool PaymentTick = false;
            public bool FirebaseNotificationVis = true;
            public bool CheckBoxTick_Visa = true;
            public bool CheckBoxTick_Master = true;
            public float FontRatio = 1f;

            public static void Load()
            {
                Setting = (Settings)Context.SecureStorage.ObjectStorage.LoadObject(typeof(Settings), "Setting");
                if (Setting == null)
                    Setting = new Settings();

            }
            public static void Save() => Context.SecureStorage.ObjectStorage.SaveObject(Setting, "Setting");
        }
    }
}