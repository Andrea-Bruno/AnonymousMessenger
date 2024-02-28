using System;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Plugin.LatestVersion;
using System.Diagnostics;
using XamarinShared;
using Plugin.Toast;
using static Telegraph.App;
using Plugin.Media.Abstractions;
using CustomViewElements;
using Utils;
using System.Threading;

namespace Telegraph.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class NavigationTappedPage : CustomTabbedPage
	{
		public static EncryptedMessaging.Context Context;

		private bool isLatestVersion;
		private static string _userName;
		private static MediaFile _mediaFile;
		public static bool InitContextStarted;
		public NavigationTappedPage(string userName = null, MediaFile mediaFile = null)
		{
			_userName = userName;
			_mediaFile = mediaFile;
			SetChatTextFontSize();
			if (Context != null && !Config.ChatUI.MultipleChatModes)
				LoadPosts();
			else
				InitContext();
			InitParams();
			InitializeComponent();
			HideProgressDialog();
			if (FirebaseToken != null && Context.My.FirebaseToken != FirebaseToken)
				Context.My.FirebaseToken = FirebaseToken;
		}

		public static void InitContext(bool loadMessages = true)
		{
			Console.WriteLine("InitContext");

			if (!loadMessages)
                Config.ChatUI.MultipleChatModes = false;
			if (Context != null || InitContextStarted)
			{
				Debugger.Break(); // You don't have to go in here! There is something conceptually wrong with creating the interface! Correct the UI logic! It must not attempt to instantiate the context twice
				return;
			}
			InitContextStarted = true;
			Context = Setup.Initialize( OnMessageArrived, OnNewMessageAddedToView, Config.Connection.EntryPoint, Config.Connection.NetworkName, Config.ChatUI.MultipleChatModes, Config.ChatUI.NewMessageOnTop, ChatRoom.MessageContainer, Passphrase, new PaletteSetting()
			{
				ForegroundColor = Color.White,
				MainBackgroundColor = Color.Black,
				SecondaryBackgroundColor = Color.FromHex("#8D8D8D"),
				BackgroundColor = Color.FromHex("#201F24"),
				CommonBackgroundColor = Color.FromHex("#E5E5E5"),
				SecondaryTextColor = Color.FromHex("#14131A"),
				ThemeColor = Color.FromHex("#FFD62C"),
			});
			Console.WriteLine("Context "+ Context);
		}

		private void LoadPosts()
		{
			Config.ChatUI.MultipleChatModes = true;
			Context.Messaging.SetMultipleChatModes(true);
			ChatPageSupport.SetMultipleChatModes(true);
			Context.Contacts.ReadPosts();
		}

		private void InitParams()
		{
			Setting = Setup.Setting;
			InitNotificationService();
			if (!IsLogged)
				SetSignupUserDetails();
			UpdateDeviceToken(DeviceToken);
			if (!Context.SecureStorage.SecureKeyValueCapability)
				CrossToastPopUp.Current.ShowToastMessage(Localization.Resources.Dictionary.DeviceCannotSecurelySaveData);

			Preferences.Set("AppId", Context.My.GetId() + "");
			Thread t = new Thread(StartViewLoadThread);
			t.Start();
		}

        protected override void OnAppearing() => base.OnAppearing();

        private void StartViewLoadThread()
		{
            while (!IsVisible)
            {
                Thread.Sleep(100);
            }
            Thread.Sleep(100);

			if (!isLatestVersion) _ = CheckAppLastVersionAsync();
			ShowRequiredView(SharedImage, ChatIdRequired);
		}

		private void SetSignupUserDetails()
        {
            Preferences.Set("IsLogged", true);
            Preferences.Set("LoggedTime", DateTime.UtcNow);

            if (_mediaFile != null)
                Context.My.SetAvatar(Utils.Utils.StreamToByteArray(_mediaFile.GetStream()));

            if (_userName != null)  // on restore case
                Context.My.Name = _userName.Trim();

            if (FirebaseToken != null && Context.My.FirebaseToken != FirebaseToken)
                Context.My.FirebaseToken = FirebaseToken;

            if (DeviceToken != null && Context.My.DeviceToken != DeviceToken)
                Context.My.DeviceToken = DeviceToken;
            Setup.Settings.Save();
        }
	
		public void ShowRequiredView(byte[] sharedImage = null, ulong? chatIdRequired = null)
		{
 			if (!Preferences.Get("isPassphrase", false) && (!Preferences.Get("isSkip", false)
				|| (DateTime.Now -  Preferences.Get("LoggedTime", DateTime.UtcNow)).TotalDays > 7))
			{
				ShowPassphrasePage();
			}
			else if (sharedImage != null)
			{
				Device.BeginInvokeOnMainThread(() => {
					Application.Current.MainPage.Navigation.PushAsync((new GroupUserSelectPage(sharedImage)));
				});
				SharedImage = null;
			}
			else if (chatIdRequired != null)
			{
				var contact = Context.Contacts.GetContact((ulong)chatIdRequired);
				ChatIdRequired = null;
				if (contact != null)
				{

					Device.BeginInvokeOnMainThread(() => {
						Application.Current.MainPage.Navigation.PopToRootAsync();
						Application.Current.MainPage.Navigation.PushAsync(new ChatRoom(contact));
					});
				}
			}
		}

		private void ShowPassphrasePage()
		{
			Preferences.Set("isSkip", false);
			if (Preferences.Get("LoggedTime", DateTime.MinValue) == DateTime.MinValue)
				Preferences.Set("LoggedTime", DateTime.UtcNow);
			var passphrase = Context.My.GetPassphrase();
			if (!string.IsNullOrEmpty(passphrase) && passphrase.Split(' ').Length == 12)
			{
				Device.BeginInvokeOnMainThread(() =>
				{
					Application.Current.MainPage.Navigation.PushAsync(new ShowPublicKeyPage(Preferences.Get("LoggedTime", DateTime.UtcNow)));
				});
			}
			else
				Preferences.Set("isPassphrase", true);
		}

		public void ChangeToMainPage()
		{
			var firstPage = Children[0];
			if (CurrentPage != firstPage)
				CurrentPage = firstPage;
		}

		protected override bool OnBackButtonPressed()
		{
			var firstPage = Children[0];
			if (CurrentPage == firstPage)
			{
				return base.OnBackButtonPressed();
			}

			CurrentPage = firstPage;
			return true;
		}

		private void SetChatTextFontSize()
		{
			Defaults.DefaultSelectedFontRatio = Preferences.Get("FontRatio", 1f);
		}

		private async System.Threading.Tasks.Task CheckAppLastVersionAsync()
		{
			isLatestVersion = await CrossLatestVersion.Current.IsUsingLatestVersion();

			if (!isLatestVersion)
			{
				var update = await DisplayAlert(Localization.Resources.Dictionary.NewVersion, Localization.Resources.Dictionary.UpdateApplication, null, Localization.Resources.Dictionary.Yes);
				if (!update)
				{
					await CrossLatestVersion.Current.OpenAppInStore();
				}
			}
		}
        protected override void OnTabIndexPropertyChanged(int oldValue, int newValue)
        {
            base.OnTabIndexPropertyChanged(oldValue, newValue);
			ReEstablishConnection();
        }
    }

}