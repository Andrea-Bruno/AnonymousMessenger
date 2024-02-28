
using AnonymousWhiteLabel.DesignHandler;
using AnonymousWhiteLabel.Pages;
using EncryptedMessaging;
using MessageCompose;
using MessageCompose.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AnonymousWhiteLabel.Helper;
using NotificationService;
using Utils;
using Xamarin.Forms;
using XamarinShared.ViewCreator;
#if DEBUG_RAM
//using CryptoWalletLibrary.Ehtereum.Services;
//using CryptoWalletLibrary.Services;
#endif

namespace AnonymousWhiteLabel
{
    public partial class App : Application
    {
        public static Context Context;
        public App()
        {
            InitializeComponent();
            SetStyle();
            //Xamarin.Essentials.Preferences.Clear();
            if (Xamarin.Essentials.Preferences.Get("firstStartup", true) == true)
            {
                var restore = new RestoreAccount(Start);
                MainPage = restore;
            }
            else
            {
                Start();
            }
        }

        private void SetStyle()
        {
            Current.Resources = DesignResourceManager.ChangeTheme();
            Icons.IconProvider += ProvideImageSource;

            //must be replaced with new Sources .
            //Current.Resources = DesignResourceManager.ChangeTheme();

            //CustomViewElements.Palette.CommonBackgroundColor = DesignResourceManager.GetColorFromStyle("Color1");
            //CustomViewElements.Palette.BackIcon = DesignResourceManager.GetImageSource("ic_new_back_icon.png");
            //CustomViewElements.Palette.SearchIcon = DesignResourceManager.GetImageSource("ic_toolbar_search.png");
            //CustomViewElements.Palette.CreateConversationIcon = DesignResourceManager.GetImageSource("ic_add_new_chat.png");
            //CustomViewElements.Palette.DefaultGroupIcon = DesignResourceManager.GetImageSource("ic_new_addGroup_contact.png");
            //CustomViewElements.Palette.NoItemIcon = DesignResourceManager.GetImageSource("ic_noResult.png");
            //CustomViewElements.Palette.NoResultIcon = DesignResourceManager.GetImageSource("ic_noItem.png");
            //CustomViewElements.Palette.SearchClearIcon = DesignResourceManager.GetImageSource("ic_toolbar_search_clear.png");


            //DependencyService.Get<IStatusBarColor>().SetStatusbarColor(DesignResourceManager.GetColorFromStyle("Color1"));
        }
        public ImageSource ProvideImageSource(string key)
        {
            return DesignHandler.DesignResourceManager.GetImageSource(key);
        }


        public void Start() => Start(null);
        public void Start(string passPhrase)
        {
            MessageViewCreator.Instance.Setup(NotificationService.SendNotification, fileDetailsProvider: ProvideFileDetails, composerStateProvider: ProvideComposerState, contactDetailsProvider: ProvideContactDetails);
            var oem = new OEM(Config.Connection.LicenseOEM);
            Context = XamarinShared.Setup.Initialize(OnMessageArrived, OnNewMessageAddedToView, Config.Connection.EntryPoint, Config.Connection.NetworkName, Config.ChatUI.MultipleChatModes, Config.ChatUI.NewMessageOnTop, ChatPage.MessageContainer, OnInitialized: OnContextInitialized, privateKeyOrPassphrase: passPhrase, oem: oem);

            var main = new MainTabsPage(Context);
            //MainPage = main;
            MainPage = new NavigationPage(main);

#if DEBUG_RAM
            CryptoWalletUI.CryptoWalletUIinit.Initialize(main, Context);   
#endif

#if DEBUG_RAM_SOCIAL
            // add Community functions to main
            CommunityClient.Social.Initialize(main, Context);
#if false
			// ====================== START TEST BANKING CLOUD FUNCTIONS =================================
			// add Banking functions to main
			Banking.BankCloud.Initialize(Context);
			void RunOnConfirmation(List<byte[]> values) // We create a function that is executed when the cloud responds to our command (this code is not mandatory, it is used to manage the vloud responses)
			{
				if (values == null)
				{
					Debug.WriteLine("Timeout is occurred: The device o the cloud it is not online");
					return;
				}
				var status = (Banking.Communication.Feedback)values[0][0]; // The cloud as the first parameter gives us a byte that indicates the outcome of the operation
				Debug.WriteLine("Operation " + status.ToString());
				if (status == Banking.Communication.Feedback.Successful)
				{
					// If you want to have something run when the cloud has finished processing the request, write the code here
				}
				else
				{
					// There was an error, write the code to warn the user about the type of error that occurred.
				}
			};
			//			Communication.CreateErc20Token(RunOnConfirmation, "name", "symbol", 100000, 100000);
			// ====================== END TEST BANKING CLOUD FUNCTIONS  =================================			
#endif
#endif
        }



        private static void OnContextInitialized(Context context)
        {
            // ===================================== START web application server =====================================
#if DEBUG_ALI
#if DEBUG
            var isDebug = true;
#else
            var isDebug = false;
#endif

            //WebSupport.ProxyWebConnection.Initialize(Context, isDebug);
            //#if DEBUG
            //            //        Attention: Receive the public key from the proxy.In production the device must scan the public key shown by the browser!
            //            WebSupport.ProxyWebConnection.SetWebProxy(context, true); // Start the connection to the proxy. This is only needed in debugging
            //            context.OnContactEvent += (message) =>
            //            {
            //                if (message.Type == MessageFormat.MessageType.SubApplicationCommandWithData)
            //                {
            //                    if (message.GetSubApplicationCommandWithData(out ushort appId, out ushort command, out byte[] data))
            //                    {
            //                        if (appId == BitConverter.ToUInt16(System.Text.Encoding.ASCII.GetBytes("web"), 0) && (WebSupport.Communication.Command)command == WebSupport.Communication.Command.SetEncryptionKey)
            //                        {
            //                            var proxyPubblicKey = System.Text.Encoding.ASCII.GetString(data);
            //                            WebSupport.ProxyWebConnection.Initialize(Context, Convert.FromBase64String(proxyPubblicKey));
            //                        }
            //                    }
            //                }
            //            };
            //#endif
#endif
            // ===================================== END   web application server =====================================
        }



        protected override void OnStart()
        {
            // The code here on Android is started even when the screen is rotated, or with the application already started

        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }

        //The code in here is executed when a message is received
        private void OnMessageArrived(Message message) { }
        public static Action IgnoreBatteryOptimizations { get; set; }

        private static void OnNewMessageAddedToView()
        {
            // Scroll to End;
            ChatPage.MessageContainer.ScrollToAsync(0, ChatPage.MessageContainer.ContentSize.Height, false);
        }


        private bool ProvideComposerState(MessageViewCreator.RequiredComposerState requiredState, int newValue)
        {

            if (requiredState == MessageViewCreator.RequiredComposerState.AudioSend)
            {
                switch (newValue)
                {
                    case 0:
                        Composer.IsAudioSendCancelled = false;
                        break;
                    case 1:
                        Composer.IsAudioSendCancelled = true;
                        break;
                }

                return Composer.IsAudioSendCancelled;
            }
            else
            {
                if (newValue == 0)
                {
                    Composer.IsAudioRecordCancelled = false;
                }
                else if (newValue == 1)
                {
                    Composer.IsAudioRecordCancelled = true;
                }
                return Composer.IsAudioRecordCancelled;

            }
        }

        private Tuple<string, string> ProvideContactDetails(byte[] data)
        {
            ContactDetails details = Utils.Utils.ByteArrayToObject(data) as ContactDetails;
            return Tuple.Create(details.Name, details.PhoneNumber);

        }

        private Tuple<byte[], string> ProvideFileDetails(byte[] data)
        {
            SerializableFileData details = Utils.Utils.ByteArrayToObject(data) as SerializableFileData;
            return Tuple.Create(details.Data, details.FileName);
        }

        public static void ReEstablishConnection(bool iMSureThereIsConnection = false)
        {
            if (iMSureThereIsConnection == false)
                iMSureThereIsConnection = Xamarin.Essentials.Connectivity.NetworkAccess == Xamarin.Essentials.NetworkAccess.Internet
                    || Xamarin.Essentials.Connectivity.NetworkAccess == Xamarin.Essentials.NetworkAccess.ConstrainedInternet;
            Context.ReEstablishConnection(iMSureThereIsConnection);
        }

        public static string FirebaseToken;
        public static void UpdateFirebaseToken(string token = null)
        {
            if (string.IsNullOrWhiteSpace(token)) return;
            FirebaseToken = token;
            if (Context != null)
                Context.My.FirebaseToken = token;
        }
    }
}
