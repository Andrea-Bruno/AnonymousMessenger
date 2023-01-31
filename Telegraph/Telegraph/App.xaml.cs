using System;
using Telegraph.Services;
using Telegraph.Views;
using EncryptedMessaging;
using System.Collections.Generic;
using Xamarin.Forms;
using XamarinShared;
using System.Threading.Tasks;
using Plugin.Sensors;
using static EncryptedMessaging.MessageFormat;
using Telegraph.CallHandler;
using NotificationService;
using NotificationService.Services;
using Telegraph.Helper;
using CustomViewElements;
using System.Globalization;
using System.Text;
using XamarinShared.ViewCreator;
using static XamarinShared.ViewCreator.MessageViewCreator;
using MessageCompose;
using Plugin.Toast;
using Telegraph.Services.GoogleTranslationService;
using MessageCompose.Model;
using Telegraph.DesignHandler;

[assembly: ExportFont("Lato-Regular.ttf", Alias = "LatoRegular")]
[assembly: ExportFont("Lato-BoldItalic.ttf", Alias = "LatoBoldItalic")]
[assembly: ExportFont("Lato-Black.ttf", Alias = "LatoBlack")]

[assembly: ExportFont("PoppinsBlack.ttf", Alias = "PoppinsBlack")]
[assembly: ExportFont("PoppinsBold.ttf", Alias = "PoppinsBold")]
[assembly: ExportFont("PoppinsItalic.ttf", Alias = "PoppinsItalic")]
[assembly: ExportFont("PoppinsLight.ttf", Alias = "PoppinsLight")]
[assembly: ExportFont("PoppinsMedium.ttf", Alias = "PoppinsMedium")]
[assembly: ExportFont("PoppinsRegular.ttf", Alias = "PoppinsRegular")]
[assembly: ExportFont("PoppinsSemiBold.ttf", Alias = "PoppinsSemiBold")]

[assembly: ExportFont("Roboto-Black.ttf", Alias = "RobotoBlack")]
[assembly: ExportFont("Roboto-Bold.ttf", Alias = "RobotoBold")]
[assembly: ExportFont("Roboto-Italic.ttf", Alias = "RobotoItalic")]
[assembly: ExportFont("Roboto-Light.ttf", Alias = "RobotoLight")]
[assembly: ExportFont("Roboto-Medium.ttf", Alias = "RobotoMedium")]
[assembly: ExportFont("Roboto-Regular.ttf", Alias = "RobotoRegular")]

namespace Telegraph
{
	public partial class App : Application
	{

		public static ulong CurrentChatId;
		public static bool IsAppInSleepMode = true;
		public static bool IsLogged;
		public static Setup.Settings Setting;
		public static string Passphrase = null;
		public static string DeviceToken;
		public static string FirebaseToken;
		public CallManager CallManager;

		internal static byte[] SharedImage;

		private static bool _isAppFullyLoaded = false;
		
		private static IDisposable ProximitySensorObservable;
		private bool _isAppInitialized;
		private static NotificationManager _notificationService;

#if DEBUG_RAM
		private Banking.Services.BitcoinWalletService BitcoinWalletService;
#endif


		public App()
		{
#if DEBUG_RAM
			//BitcoinWalletService = new Banking.Services.BitcoinWalletService("L4VpjddASNaAKqfD7A3giyFcBoa7NrZizX1RNFskwwVcovpEojkZ");
			//Task.Run(() => BitcoinWalletService.recoverWalletAsync());
#endif
			if (!_isAppInitialized)
			{
				InitializeComponent();
				_isAppInitialized = true;
				InitServices();
				InitLocale();
				MessageViewCreator.Instance.Setup(
					SendNotification,
					OnImageMessageClicked,
					OnLocationMessageClicked,
					OnPdfMessageClicked,
					OnContactAdded, 
					OnMessageInfoClicked,
					ListenProximitySensor,
					DisableProximitySensor,
					OnToastRequested,
					OnTranslateRequested,
					OnComposerStateRequested,
					contactDetailsProvider: ContactProvider,
					fileDetailsProvider: FileProvider,
					finishCallEvent: OnFinishCallEvent
					);

		
					SetStyle();
				OpenNavigationPage();
				CallManager = new CallManager();
				_isAppFullyLoaded = true;
			}
		}

		// JanD4rk : Must be refactored
        private void OnFinishCallEvent(Message message) => FinishCall(message);

        public static void InitContext()
        {
			NavigationTappedPage.InitContext(false);
        }

		private void InitLocale()
		{
			string lang = Xamarin.Essentials.Preferences.Get("language", null);
			if (lang != null)
			{
				CultureInfo.CurrentCulture.ClearCachedData();
				CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo(lang);
			}
		}

        private void OnMessageInfoClicked(List<Contact> contacts)
        {
			MainPage.Navigation.PushAsync(new MessageInfoPage(contacts), false);
		}


        private void InitServices()
        {
			DependencyService.Register<IProgressInterface>();
			DependencyService.Register<ISecurityFlag>();
		}

		private void OpenNavigationPage()
        {
            IsLogged = Xamarin.Essentials.Preferences.Get("IsLogged", false);
            if (!IsLogged)
               MainPage = new NavigationPage(new UupLoginSignupPage()); // first startup
            else if (Setup.GetSecureValue("LockPin") != null)
                MainPage = new NavigationPage(new CreatePinPage()); // run with lock screen
            else
                MainPage = new NavigationPage(new NavigationTappedPage()); // normal runn
		}

  //      internal static void OnViewMessage(Message message, bool isMy, out View content)
		//{
		//	MessageView.OnViewMessage(message, isMy, out content);
		//	if (!isMy)
		//		FinishCall(message);

		//}

		private static void FinishCall(Message message)
        {
			try
			{
				Device.BeginInvokeOnMainThread(() =>
				{
					DependencyService.Get<IEndCall>().FinishCall(message.ChatId + "");
				});
			}catch(Exception e)
            {

            }
		}

		private Tuple<string,string> ContactProvider(byte[] data)
		{
			ContactDetails details = Utils.Utils.ByteArrayToObject(data) as ContactDetails;
			return Tuple.Create(details.Name, details.PhoneNumber);
		}

		private Tuple<byte[], string> FileProvider(byte[] data)
		{
			SerializableFileData details = Utils.Utils.ByteArrayToObject(data) as SerializableFileData;
			return Tuple.Create(details.Data, details.FileName);
		}

		private void OnContactAdded(Contact contact)
        {
			if (Device.RuntimePlatform == Device.Android)
				DependencyService.Get<ISharedPreference>().AddContact(contact.ChatId + "", contact.Name, contact.Os == Contact.RuntimePlatform.Android);
		}

        private void OnPdfMessageClicked(byte[] data)
        {
			Current.MainPage.Navigation.PushAsync(new PdfViewPage(data));
		}

        private void OnLocationMessageClicked(double lat, double lng)
        {
            _ = OpenMapAsync(lat, lng);
		}

        private void OnImageMessageClicked(byte[] data)
        {
			Current.MainPage.Navigation.PushAsync(new ImagePreviewView(data));
		}

		private void OnToastRequested(String message)
		{
			CrossToastPopUp.Current.ShowToastMessage(message);
		}


		private string OnTranslateRequested(String text)
		{
			return GoogleTranslateService.Translate(text, CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
		}

		private bool OnComposerStateRequested(RequiredComposerState state,int val)
		{

			if(state == RequiredComposerState.AudioSend )
            {
				if (val == 0)
                {
					Composer.IsAudioSendCancelled = false;
				}else if(val == 1)
                {
					Composer.IsAudioSendCancelled = true;
				}
				return Composer.IsAudioSendCancelled;
            }
            else {
				if (val == 0)
				{
					Composer.IsAudioRecordCancelled = false;
				}
				else if (val == 1)
				{
					Composer.IsAudioRecordCancelled = true;
				}
				return Composer.IsAudioRecordCancelled;

			}
		}





		public static void ListenProximitySensor(IAudioPlayer CurrentAudioPlayer = null)
		{
			if (ProximitySensorObservable == null)
				ProximitySensorObservable = CrossSensors
						.Proximity
						.WhenReadingTaken()
						.Subscribe(x =>
						{
							if (CurrentAudioPlayer != null && CurrentAudioPlayer.IsPlaying)// media player speaker
								CurrentAudioPlayer.ChangeSpeaker(x, (int)CurrentAudioPlayer.CurrentPosition);
							DependencyService.Get<IAudioPlayerSpeakerService>().ChangeSpeaker(x);
						});
		}

		public static void DisableProximitySensor()
		{
			if (ProximitySensorObservable != null)
			{
				ProximitySensorObservable.Dispose();
				ProximitySensorObservable = null;
			}
		}

		public static void InitNotificationService()
        {
			if(_notificationService == null)
				_notificationService = new NotificationManager(NavigationTappedPage.Context?.My.GetId() + "");
		}
		private void SetStyle()
        {
			Current.Resources = DesignResourceManager.ChangeTheme(true);
			Utils.Icons.IconProvider +=ProvideIcon;
			
			CustomViewElements.Palette.CommonBackgroundColor = DesignResourceManager.GetColorFromStyle("Color1");
			//CustomViewElements.Palette.BackIcon=DesignResourceManager.GetImageSource("ic_new_back_icon.png");
			//CustomViewElements.Palette.SearchIcon=DesignResourceManager.GetImageSource("ic_toolbar_search.png");
			//CustomViewElements.Palette.CreateConversationIcon=DesignResourceManager.GetImageSource("ic_add_new_chat.png");
			//CustomViewElements.Palette.DefaultGroupIcon=DesignResourceManager.GetImageSource("ic_new_addGroup_contact.png");
			//CustomViewElements.Palette.NoItemIcon=DesignResourceManager.GetImageSource("ic_noResult.png");
			//CustomViewElements.Palette.NoResultIcon=DesignResourceManager.GetImageSource("ic_noItem.png");
			//CustomViewElements.Palette.SearchClearIcon=DesignResourceManager.GetImageSource("ic_toolbar_search_clear.png");


			DependencyService.Get<IStatusBarColor>().SetStatusbarColor(DesignResourceManager.GetColorFromStyle("Color1"));

		}

		public ImageSource ProvideIcon(string key)
        {
			return DesignResourceManager.GetImageSource(key);
        }

		public static void ShareImage(byte[] sharedImage = null)
		{
			SharedImage = sharedImage;
			((App)Current).GetRootPage()?.ShowRequiredView(SharedImage);
		}

		internal static ulong? ChatIdRequired;
        internal static object Instance;

        public static void GoToChat(ulong chatId, NotificationType notificationType = NotificationType.NONE)
		{
			ChatIdRequired = chatId;

			((App)Current).GetRootPage()?.ShowRequiredView(chatIdRequired: ChatIdRequired);

			if (notificationType == NotificationType.GROUP_START_AUDIO_CALL || notificationType == NotificationType.GROUP_START_VIDEO_CALL)
			{
				Contact contact = NavigationTappedPage.Context.Contacts.GetContact(chatId);
				if (ChatPageSupport.GetContactViewItems(contact).IsCallGoingOn)
					((App)Current)?.CallManager?.StartCall(contact, notificationType == NotificationType.GROUP_START_VIDEO_CALL, true);
			}
		}

		public static void ChangeSecureFlag(bool _isEnabled)
		{
			if (Device.RuntimePlatform == Device.Android)
			{
				if (_isEnabled)
					DependencyService.Get<ISecurityFlag>().DisableSecureFlag();
				else
					DependencyService.Get<ISecurityFlag>().EnableSecureFlag();
			}
		}

		internal static void OnNewMessageAddedToView()
		{
		}

		internal static void OnMessageArrived(Message message)
		{
			if(Device.RuntimePlatform == Device.Android && !Config.ChatUI.MultipleChatModes)
				FinishCall(message);
		}

		protected override void OnSleep()
		{
			IsAppInSleepMode = true;
			if (ChatRoom.Instance != null)
				ChatRoom.Instance.ResetAudioRecorderView();
			if (Device.RuntimePlatform == Device.iOS)
				DisableProximitySensor();
		}

		protected override void OnResume()
		{
			IsAppInSleepMode = false;
			ReEstablishConnection();
		}

		protected override void OnStart()
		{
			base.OnStart();
			IsAppInSleepMode = false;
			ReEstablishConnection();
		}

		public NavigationTappedPage GetRootPage()
        {
			return ((NavigationTappedPage)((NavigationPage)Application.Current.MainPage).RootPage);
		}


		private async Task SendNotification(Contact _contact, MessageType type) {
			if (_contact.ImBlocked) return;
			var messageType = MessageTypeConverter.GetNotificationType(type);
			if (messageType == NotificationType.NONE || (!_contact.IsGroup && (type == MessageType.EndCall || type == MessageType.DeclinedCall))) return;
				SendNotification(_contact, messageType);

		}

		private static async Task OpenMapAsync(double lat, double lng)
		{
			var supportsUri = await Xamarin.Essentials.Launcher.CanOpenAsync("comgooglemaps://");
			try
			{
				if (supportsUri)
					await Xamarin.Essentials.Launcher.OpenAsync($"comgooglemaps://?q={lat},{lng}({"Address"})");
				else
					await Xamarin.Essentials.Map.OpenAsync(lat, lng, new Xamarin.Essentials.MapLaunchOptions { Name = "Address" });
			}
			catch (Exception) { }
		}

		public static void ReEstablishConnection(bool iMSureThereIsConnection = false)
		{
			if (iMSureThereIsConnection == false)
				iMSureThereIsConnection = Xamarin.Essentials.Connectivity.NetworkAccess == Xamarin.Essentials.NetworkAccess.Internet
					|| Xamarin.Essentials.Connectivity.NetworkAccess == Xamarin.Essentials.NetworkAccess.ConstrainedInternet;
			if (_isAppFullyLoaded) EncryptedMessaging.Context.ReEstablishConnection(iMSureThereIsConnection);
		}

		public static void UpdateDeviceToken(string token)
		{
			if (token != null)
				DeviceToken = token;
			if (NavigationTappedPage.Context != null && NavigationTappedPage.Context.My.Name != null && token != null && NavigationTappedPage.Context.My.DeviceToken != token)
				NavigationTappedPage.Context.My.DeviceToken = token;
		}

		public static void UpdateFirebaseToken(string token)
		{
			if (NavigationTappedPage.Context != null && token != null && NavigationTappedPage.Context.My.FirebaseToken != token)
				NavigationTappedPage.Context.My.FirebaseToken = token;
		}

		private static List<string> GetGroupParticipantsRegistrationIds(Contact contact)
		{

			List<string> registration_ids = new List<string>();
			foreach (var key in contact.Participants)
			{
				if (key != null && Convert.ToBase64String(key) != NavigationTappedPage.Context.My.GetPublicKey())
				{
					Contact participant = NavigationTappedPage.Context.Contacts.GetParticipant(key);
					if (participant != null && !string.IsNullOrWhiteSpace(participant.FirebaseToken))
						registration_ids.Add(participant.FirebaseToken);
				}
			}
			return registration_ids;
		}

		public static async void SendNotification(Contact _contact, NotificationType notificationType)
        {
			if (_contact.ImBlocked) return;
			string contactName = !_contact.IsGroup ? _contact.MyRemoteName ?? NavigationTappedPage.Context?.My.Name : _contact.Name;
			InitNotificationService();
			await _notificationService.SendNotification(contactName, _contact.ChatId + "", _contact.IsGroup,
				_contact.Os == Contact.RuntimePlatform.Android, GetGroupParticipantsRegistrationIds(_contact),
				(int)_contact.RemoteUnreaded, notificationType, _contact.Language);
		}

		public int NavigationStackCount
		{
			get
			{
				NavigationPage mainPage = MainPage as NavigationPage;
				if (mainPage != null)
				{
					return mainPage.Navigation.NavigationStack.Count;
				}
				return 0;
			}
		}
	}
}