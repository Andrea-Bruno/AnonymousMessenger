using EncryptedMessaging;
using System;
using System.Diagnostics;
using Xamarin.Forms;
using XamarinShared.ViewCreator;

namespace XamarinShared
{
    /// <summary>
    /// This class setup the context and setting of the application.
    /// </summary>
    public static class Setup
    {
        internal static Context Context;
        // Remove onViewMessage parameter

        /// <summary>
        /// Initializer of context and common parts in Xamarin
        /// </summary>
        /// <param name="onNotification"></param>
        /// <param name="onNewMessageAddedToView"></param>
        /// <param name="entryPoint"></param>
        /// <param name="networkName"></param>
        /// <param name="multipleChatModes"></param>
        /// <param name="newMessageOnTop"></param>
        /// <param name="messageScrollView"></param>
        /// <param name="privateKeyOrPassphrase"></param>
        /// <param name="getFirebaseToken">Function to get FirebaseToken (the function is passed and not the value, so as not to block the main thread as this sometimes takes a long time). FirebaseToken is used by firebase, to send notifications to a specific device. The sender needs this information to make the notification appear to the recipient.</param>
        /// <param name="getAppleDeviceToken">Function to get AppleDeviceToken (the function is passed and not the value, so as not to block the main thread as this sometimes takes a long time). In ios AppleDeviceToken is used to generate notifications for the device. Whoever sends the encrypted message needs this data to generate a notification on the device of who will receive the message.</param>
        /// <param name="paletteSetting"></param>
        /// <returns>Context of the application</returns>
        public static Context Initialize(Context.OnMessageArrived onNotification, ChatPageSupport.OnNewMessageAddedToView onNewMessageAddedToView, string entryPoint, string networkName, bool multipleChatModes, bool newMessageOnTop, ScrollView messageScrollView, string privateKeyOrPassphrase = null, PaletteSetting paletteSetting = null, Action<Context> OnInitialized = null, Func<string> getFirebaseToken = null, Func<string> getAppleDeviceToken = null, OEM oem = null)
        {

#if DEBUG_RAM
            //privatKeyOrPassphrase = "engage lizard foam just reform way agent silver equip stomach imitate spike";
            //privatKeyOrPassphrase = "gadget kick where list boil ivory alert able pride six dust loyal";
            privatKeyOrPassphrase = "spike garage secret until wise lab ball mesh exhibit twice wife sea";
#endif
            if (paletteSetting == null)
                Palette.Colors = new PaletteSetting() { ForegroundColor = Color.FromHex("#E2E8F3"), BackgroundColor = Color.FromHex("#B7CBF2") };
            else Palette.Colors = paletteSetting;
            if (Context != null)
            {
                Debugger.Break(); // You don't have to go in here! There is something conceptually wrong with creating the interface! Correct the UI logic! It must not attempt to instantiate the context twice
            }
            else
            {
                MessageReadStatus messageReadStatus = new MessageReadStatus();
                //messageReadStatus for status label
                MessageViewCreator.Instance.SetReadStatus(messageReadStatus);
                ChatPageSupport.Initialize(multipleChatModes, newMessageOnTop, messageScrollView, onNewMessageAddedToView);
                var currentInternetAccess = Xamarin.Essentials.Connectivity.NetworkAccess == Xamarin.Essentials.NetworkAccess.Internet || Xamarin.Essentials.Connectivity.NetworkAccess == Xamarin.Essentials.NetworkAccess.ConstrainedInternet;
                Context.OnContextIsInitialized += OnInitialized;
 
                Context = new Context(entryPoint, networkName, multipleChatModes, privateKeyOrPassphrase, Modality.Client, currentInternetAccess, Device.BeginInvokeOnMainThread, GetSecureValue, SetSecureValue, getFirebaseToken, getAppleDeviceToken, null, oem);
                //privateKeyOrPassphrase = "base cup tape theory segment document spare dove slush absurd enough February";
                Context.ViewMessage += ChatPageSupport.ViewMessage;
                Context.OnContactEvent += ChatPageSupport.OnContactEvent;
                Context.OnLastReadedTimeChange += messageReadStatus.OnLastReadedTimeChange;
                Context.OnMessageDelivered += messageReadStatus.OnMessageDelivered;
                Context.OnNotification += onNotification;
#if (DEBUG_AND || DEBUG_RAM)
                CloudClient.CloudClientConnection.Initialize(Context);
# endif
                // Bind the event to change the connection when the connectivity changes
                Xamarin.Essentials.Connectivity.ConnectivityChanged += (o, c) => Context.OnConnectivityChange(c.NetworkAccess == Xamarin.Essentials.NetworkAccess.Internet || Xamarin.Essentials.Connectivity.NetworkAccess == Xamarin.Essentials.NetworkAccess.ConstrainedInternet);
                // Set the current connection status
                Context.OnConnectivityChange(Xamarin.Essentials.Connectivity.NetworkAccess == Xamarin.Essentials.NetworkAccess.Internet || Xamarin.Essentials.Connectivity.NetworkAccess == Xamarin.Essentials.NetworkAccess.ConstrainedInternet);
            }
            Settings.Load();
            Calls.GetInstance().SetMyId(Context.My.Id);

            //Context.Contacts.ForEachContact(x => x.Save(true));
            //EncryptedMessaging.Cloud.SendCloudCommands.GetAllObject(Context, "Contact");
            //EncryptedMessaging.Cloud.SendCloudCommands.PostPushNotification(Context, "740f4707 bebcf74f 9b7c25d4 8e335894 5f6aa01d a5ddb387 462c7eaf 61bb78ad", 12345678910, true, "Andrea");

            //Context.My.SetAvatar(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 });

            //Context.Contacts.RestoreContactFromCloud();


            return Context;
        }

        /// <summary>
        /// Used to set default key value in secure storage.
        /// </summary>
        /// <param name="key"> Key to be storage</param>
        /// <param name="value">Encrypted Key</param>
        public static void SetSecureValue(string key, string value)
        {
            //Xamarin.Essentials.SecureStorage.SetAsync("0.test", "");
            _ = Xamarin.Essentials.SecureStorage.SetAsync(key, value);
        }

        /// <summary>
        /// Remove the set default key value.
        /// </summary>
        /// <param name="key">Key to be removed</param>
        public static void RemoveSecureValue(string key) => _ = Xamarin.Essentials.SecureStorage.Remove(key);

        /// <summary>
        /// Get the key value from the secure storage.
        /// </summary>
        /// <param name="key"> Key to get</param>
        /// <returns>key value</returns>
        public static string GetSecureValue(string key)
        {
            System.Threading.Tasks.Task<string> task = System.Threading.Tasks.Task.Run(async () => await Xamarin.Essentials.SecureStorage.GetAsync(key));
            return task.Result;
        }

        /// <summary>
        /// Application setting object
        /// </summary>
        public static Settings Setting;

        /// <summary>
        /// Internal class that setup all the relevant setting for the application.
        /// </summary>
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

            /// <summary>
            /// Get all the save setting from secure storage.
            /// </summary>
            public static void Load()
            {
                Setting = (Settings)Context.SecureStorage.ObjectStorage.LoadObject(typeof(Settings), "Setting");
                if (Setting == null)
                    Setting = new Settings();

            }
            /// <summary>
            /// save any changes in the setting to secure storage.
            /// </summary>
            public static void Save() => Context.SecureStorage.ObjectStorage.SaveObject(Setting, "Setting");
        }
    }
}