using System;
using Cryptogram.Services;
using Cryptogram.Views;
using EncryptedMessaging;
using System.Collections.Generic;
using Xamarin.Forms;
using XamarinShared;
using System.Threading.Tasks;
using Plugin.Sensors;
using static EncryptedMessaging.MessageFormat;
using Cryptogram.CallHandler;
using NotificationService;
using NotificationService.Services;
using Cryptogram.Helper;
using System.Globalization;
using XamarinShared.ViewCreator;
using static XamarinShared.ViewCreator.MessageViewCreator;
using MessageCompose;
using Cryptogram.Services.GoogleTranslationService;
using MessageCompose.Model;
using Cryptogram.DesignHandler;
using Xamarin.CommunityToolkit.Extensions;
using System.Threading;
using Cryptogram.Backup;
using Cryptogram.Models;
using static Cryptogram.CryptogramUtils;
using System.Diagnostics;

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

namespace Cryptogram
{
	public partial class App : Application
	{
		// Begin
		// For handling video uploading chat
		public static ChatRoom ChatRoomVideoUploading = null;
		public static Contact ContactVideoUploading = null;
		public static bool IsVideoUploading = false;
		// For handling video uploading chat
		// End

		public static ulong CurrentChatId;
		public static bool IsAppInSleepMode = true;
		public static bool IsLogged;
		public static Setup.Settings Setting;
		public static string Passphrase = null;
		public static string DeviceToken;
		public static string FirebaseToken;
		public CallManager CallManager;

		internal static byte[] SharedData;
		internal static MessageType SharedMessageType;

		private static bool _isAppFullyLoaded = false;
		
		private static IDisposable ProximitySensorObservable;
		private bool _isAppInitialized;
		private static NotificationManager _notificationService;
		public static IDriveService DriveService;


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
					OnVideoMessageClicked,
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
               MainPage = new NavigationPage(new CryptogramLoginSignupPage()); // first startup
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

		private void OnVideoMessageClicked(string videoPath)
		{
			Current.MainPage.Navigation.PushAsync(new VideoPreviewPage(videoPath));
		}

		private void OnToastRequested(String message)
		{
			Application.Current.MainPage.DisplayToastAsync(message);
		}


        private static void OnTranslateRequested(Message message,Action onTranslationSuccess)
		{
			message.Translate(onTranslationSuccess);
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
			DependencyService.Get<IStatusBarColor>().SetStatusbarColor(DesignResourceManager.GetColorFromStyle("Color1"));
		}

		public ImageSource ProvideIcon(string key)
        {
			return DesignResourceManager.GetImageSource(key);
        }

		public static void ShareData(byte[] sharedData = null, SharedMessageType _sharedMessageType = Cryptogram.SharedMessageType.IMAGE, string fileName = null)
		{
			SharedData = sharedData;
			if (_sharedMessageType == Cryptogram.SharedMessageType.PDF)
				sharedData = Utils.Utils.ObjectToByteArray(new SerializableFileData(sharedData, fileName));
			SharedMessageType = ConvertSharedMessageTypeToMessageType(_sharedMessageType);
			((App)Current).GetRootPage()?.ShowRequiredView(sharedData);
		}

		private static MessageType ConvertSharedMessageTypeToMessageType(SharedMessageType sharedMessageType)
        {
			switch(sharedMessageType)
            {
				case Cryptogram.SharedMessageType.IMAGE:
					return MessageType.Image;
				case Cryptogram.SharedMessageType.AUDIO:
					return MessageType.Audio;
				case Cryptogram.SharedMessageType.PDF:
					return MessageType.PdfDocument;
				case Cryptogram.SharedMessageType.VIDEO:
					return MessageType.ShareEncryptedContent;
				default: return MessageType.Image;
			}
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


		public static async Task SendNotification(Contact _contact, MessageType type) {
			if (_contact== null || _contact.ImBlocked) return;
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
			if (_isAppFullyLoaded) Context.ReEstablishConnection(iMSureThereIsConnection);
		}

		public static void UpdateDeviceToken(string token = null)
		{
			if (string.IsNullOrWhiteSpace(token)) return;
			DeviceToken = token;
			if (NavigationTappedPage.Context != null)
				NavigationTappedPage.Context.My.DeviceToken = token;
		}

		public static void UpdateFirebaseToken(string token = null)
		{
			if (string.IsNullOrWhiteSpace(token)) return;
			FirebaseToken = token;
			if (NavigationTappedPage.Context != null  )
				NavigationTappedPage.Context.My.FirebaseToken = token;
		}

		private static List<string> GetAndroidGroupParticipantsRegistrationIds(Contact contact)
		{

			List<string> registration_ids = new List<string>();
			foreach (var key in contact.Participants)
			{
				if (key != null && Convert.ToBase64String(key) != NavigationTappedPage.Context.My.GetPublicKey())
				{
					Contact participant = NavigationTappedPage.Context.Contacts.GetParticipant(key);
					if (participant != null && !string.IsNullOrWhiteSpace(participant.FirebaseToken) && participant.Os == Contact.RuntimePlatform.Android)
						registration_ids.Add(participant.FirebaseToken);
				}
			}
			return registration_ids;
		}

		private static List<string> GetIOSGroupParticipantsRegistrationIds(Contact contact)
		{

			List<string> registration_ids = new List<string>();
			foreach (var key in contact.Participants)
			{
				if (key != null && Convert.ToBase64String(key) != NavigationTappedPage.Context.My.GetPublicKey())
				{
					Contact participant = NavigationTappedPage.Context.Contacts.GetParticipant(key);
					if (participant != null && !string.IsNullOrWhiteSpace(participant.FirebaseToken) && participant.Os == Contact.RuntimePlatform.iOS)
						registration_ids.Add(participant.FirebaseToken);
				}
			}
			return registration_ids;
		}

		public static async void SendNotification(Contact _contact, NotificationType notificationType)
        {
			if (_contact == null || _contact.ImBlocked) return;
			string contactName = !_contact.IsGroup ? _contact.MyRemoteName ?? NavigationTappedPage.Context?.My.Name : _contact.Name;
			InitNotificationService();

			var androidRegistrationIds = GetAndroidGroupParticipantsRegistrationIds(_contact);
			if(androidRegistrationIds.Count>0)
				await _notificationService.SendNotification(contactName, _contact.ChatId + "", _contact.IsGroup,
					true, GetAndroidGroupParticipantsRegistrationIds(_contact),
					(int)_contact.RemoteUnreaded, notificationType, _contact.Language).ConfigureAwait(false);

			var iosRegistrationIds = GetIOSGroupParticipantsRegistrationIds(_contact);
			if(iosRegistrationIds.Count>0)
				await _notificationService.SendNotification(contactName, _contact.ChatId + "", _contact.IsGroup,
					false, GetIOSGroupParticipantsRegistrationIds(_contact),
					(int)_contact.RemoteUnreaded, notificationType, _contact.Language).ConfigureAwait(false);

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


		public static void DriveLoginFailed()
		{
			Setup.RemoveSecureValue("Backup");
			DriveService = null;
		}

		public static void RestoreBackup()
		{
			Device.BeginInvokeOnMainThread(() =>
			{
				((App)Current).GetRootPage()?.ShowProgressDialog();
			});
			Thread thread = new Thread(new ThreadStart(DoRestore));
			thread.Start();

			void DoRestore()
			{
				string parentFolderId = DriveService.CreateFolder(NavigationTappedPage.Context.My.GetId() + "", null);
				NavigationTappedPage.Context.Contacts.ForEachContact(contact =>
				{
					string folderId = Device.RuntimePlatform == Device.iOS ? contact.ChatId + "" : DriveService.CreateFolder(contact.ChatId + "", parentFolderId);
					List<FileData> fileDatas = DriveService.GetFilesByFolderId(folderId);
					foreach (FileData file in fileDatas)
					{
						var data = DriveService.ReadFileContent(file.Id, folderId);
						if (data != null)
							contact.SetPost(data, new DateTime(Convert.ToInt64(file.Name)));
					}
				});
				Device.BeginInvokeOnMainThread(() =>
				{
					((App)Current).GetRootPage()?.HideProgressDialog();
					((App)Current).GetRootPage()?.DisplayToastAsync(Localization.Resources.Dictionary.RestartApplication);
				});

			}
		}

		public static void UploadBackup()
		{
			try {
				if (Setup.GetSecureValue("Backup") == null
					|| (Setup.GetSecureValue("LastBackupTime") != null && new DateTime(Convert.ToInt64(Setup.GetSecureValue("LastBackupTime"))).DayOfYear == DateTime.Now.DayOfYear))
					return;

				Thread thread = new Thread(new ThreadStart(DoBackup));
				thread.Start();

				void DoBackup()
				{
					try
					{
						string parentFolderId = DriveService.CreateFolder(NavigationTappedPage.Context.My.GetId() + "", null);
						NavigationTappedPage.Context.Contacts.ForEachContact(contact =>
						{
							string folderId = Device.RuntimePlatform == Device.iOS ? contact.ChatId + "" : DriveService.CreateFolder(contact.ChatId + "", parentFolderId);
							List<FileData> fileDatas = DriveService.GetFilesByFolderId(folderId);
							List<DateTime> excludedTimes = new List<DateTime>();
							foreach (FileData file in fileDatas)
							{
								try
								{
									excludedTimes.Add(new DateTime(Convert.ToInt64(file.Name)));
								}
								catch (Exception e)
								{
									Console.WriteLine("File name convert error: " + e.Message);

								}
							}
							contact.GetPosts((post, receptionDate) => { DriveService.CreateOrUpdateFile(folderId, receptionDate.Ticks + "", post); }, excludedTimes, DateTime.Now);
						});
					}
					catch(Exception e)
                    {

                    }
					Setup.SetSecureValue("LastBackupTime", DateTime.Now.Ticks + "");

				}
			}
			catch(Exception e)
            {
				Debugger.Break();
				Console.WriteLine("UploadBackup error: " + e.Message);
            }

		}


	}
	public enum SharedMessageType
    {
		IMAGE,
		PDF,
		AUDIO,
		VIDEO
	}
}