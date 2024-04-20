using System;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Diagnostics;
using XamarinShared;
using static Anonymous.App;
using CustomViewElements;
using System.Threading;
using FontSizeConverter = XamarinShared.ViewCreator.FontSizeConverter;
using Anonymous.Services;
using Plugin.LatestVersion;
using Anonymous.Backup;
using Anonymous.Config;

namespace Anonymous.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NavigationTappedPage : CustomTabbedPage
    {
        public static EncryptedMessaging.Context Context;

        private bool isLatestVersion;
        private static string _userName;
        private static FileResult _mediaFile;
        public static bool InitContextStarted;
        public NavigationTappedPage(string userName = null, FileResult mediaFile = null, bool isAccountRestored = false)
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

            if (isAccountRestored)
                CheckRestoreMessages();

            else if (Setup.GetSecureValue("Backup") != null)
                InitCloudDriverService();
        }

        private async void CheckRestoreMessages()
        {
            var action = await DisplayAlert(Localization.Resources.Dictionary.Restore, Localization.Resources.Dictionary.DoYouWantToRestoreMessages, Localization.Resources.Dictionary.Yes, Localization.Resources.Dictionary.No);
            if (action)
                InitCloudDriverService();
        }

        private void InitCloudDriverService()
        {
            DriveService = DependencyService.Get<IDriveService>();
            DriveService.Init();
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
            var palette = new PaletteSetting()
            {
                ForegroundColor = Color.White,
                MainBackgroundColor = Color.Black,
                SecondaryBackgroundColor = Color.FromHex("#8D8D8D"),
                BackgroundColor = Color.FromHex("#201F24"),
                CommonBackgroundColor = Color.FromHex("#E5E5E5"),
                SecondaryTextColor = Color.FromHex("#14131A"),
                ThemeColor = Color.FromHex("#FFD62C"),
            };
            Context = Setup.Initialize(
                OnMessageArrived,
                OnNewMessageAddedToView,
                Config.Connection.EntryPoint,
                Config.Connection.NetworkName,
                Config.ChatUI.MultipleChatModes,
                Config.ChatUI.NewMessageOnTop,
                Passphrase,
                palette
                );
            Console.WriteLine("Context " + Context);
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
            UpdateFirebaseToken(FirebaseToken);

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

            // if (!isLatestVersion) _ = CheckAppLastVersionAsync();
            ShowRequiredView(SharedData, ChatIdRequired);
        }

        private async void SetSignupUserDetails()
        {
            Preferences.Set("IsLogged", true);
            Preferences.Set("LoggedTime", DateTime.UtcNow);

            if (_mediaFile != null)
            {
                var stream = await _mediaFile.OpenReadAsync();
                var image = DependencyService.Get<IImageCompressionService>().CompressImage(Utils.Utils.StreamToByteArray(stream), 25);
                Context.My.SetAvatar(image);
            }

            if (_userName != null)
                Context.My.Name = _userName.Trim();

            if (FirebaseToken != null && Context.My.FirebaseToken != FirebaseToken)
                Context.My.FirebaseToken = FirebaseToken;

            if (DeviceToken != null && Context.My.DeviceToken != DeviceToken)
                Context.My.DeviceToken = DeviceToken;
            Setup.Settings.Save();
        }

        public void ShowRequiredView(byte[] sharedData = null, ulong? chatIdRequired = null)
        {
            if (!Preferences.Get("isPassphrase", false) && (!Preferences.Get("isSkip", false)
               || (DateTime.Now - Preferences.Get("LoggedTime", DateTime.UtcNow)).TotalDays > 7))
            {
                ShowPassphrasePage();
            }
            if (sharedData != null)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    Application.Current.MainPage.Navigation.PushAsync(new GroupUserSelectPage(sharedData, App.SharedMessageType));
                });
                SharedData = null;
            }

            else if (chatIdRequired != null)
            {
                var contact = Context.Contacts.GetContact((ulong)chatIdRequired);
                ChatIdRequired = null;
                if (contact != null)
                {

                    Device.BeginInvokeOnMainThread(() =>
                    {
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
                MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Application.Current.MainPage.Navigation.PushAsync(new ShowPublicKeyPage(Preferences.Get("LoggedTime", DateTime.UtcNow)));
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
            FontSizeConverter.DefaultSelectedFontRatio = Preferences.Get("FontRatio", 1f);
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