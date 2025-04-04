using CustomViewElements;
using Syncfusion.XForms.Buttons;
using System;
using Cryptogram.Backup;
using Cryptogram.DesignHandler;
using Cryptogram.PopupViews;
using Cryptogram.Services;
using Utils;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Cryptogram.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MenuPage : BasePage
    {
        public MenuPage()
        {
            InitializeComponent();
            Passphrase.IsVisible = NavigationTappedPage.Context.My.GetPassphrase().Split(' ').Length == 12;
            var tapGestureRecognizer = new TapGestureRecognizer() { NumberOfTapsRequired = 2 };
            tapGestureRecognizer.Tapped += (s, e) => {
                MenuText_Clicked(s, e);
            };
            Toolbar.TitleLabel.GestureRecognizers.Add(tapGestureRecognizer);
            InitMessageDuration();
            InitMessageLimit();
        }

        protected override void OnAppearing()
        {
            CheckSwitchCase();
            if (Preferences.Get("isPassphrase", false))
                PrivateKey.Text = Localization.Resources.Dictionary.Passphrase;
        }

        protected override void OnDisappearing() => base.OnDisappearing();

        private void Refresh_Clicked(object sender, EventArgs e)
        {
            sender.HandleButtonSingleClick();
            NavigationTappedPage.Context.Contacts.RestoreMyContactFromCloud();
        }

        private async void Share_PublicKey_Clicked(object sender, EventArgs e)
        {
            sender.HandleButtonSingleClick();
            await Share.RequestAsync(new ShareTextRequest
            {
                Text = EncryptedMessaging.ContactMessage.GetMyQrCode(NavigationTappedPage.Context)
            }).ConfigureAwait(true);
        }

        private async void PrivacyPolicy_Clicked(object sender, System.EventArgs e)
        {
            sender.HandleButtonSingleClick();
            Uri uri = new Uri("https://Cryptogramsocial.tech/pp.html");
            await Browser.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
        }      

        private void MenuText_Clicked(object sender, EventArgs e)
        {
            TestLogs.IsVisible = true;
        }

        private void VerifyPrivateKey_Clicked(object sender, EventArgs e)
        {
            sender.HandleButtonSingleClick();
            Application.Current.MainPage.Navigation.PushAsync(new ShowPublicKeyPage(Preferences.Get("LoggedTime", DateTime.UtcNow)), false);
        }

        private async void Logout_Clicked(object sender, EventArgs e)
        {
            var action = await DisplayAlert(Localization.Resources.Dictionary.Logout, Localization.Resources.Dictionary.DoYouWantToLogout, Localization.Resources.Dictionary.Yes, Localization.Resources.Dictionary.No);
            if (action)
            {
                Preferences.Clear();
                Xamarin.Essentials.SecureStorage.RemoveAll();
                DependencyService.Get<ICloseApplication>().CloseApplication();
            }

        }
        private void CheckSwitchCase()
        {
            AppLock1.IsOn = XamarinShared.Setup.GetSecureValue("LockPin") != null;
            Backup.IsOn = XamarinShared.Setup.GetSecureValue("Backup") != null;
        }

        private async void SfSwitch_AppLockStateChanged(object sender, SwitchStateChangedEventArgs e)
        {
            if (AppLock1.IsOn == true && XamarinShared.Setup.GetSecureValue("LockPin") ==null)
            {
                await Application.Current.MainPage.Navigation.PushAsync(new CreatePinPage(), false);
            }
            else if (AppLock1.IsOn == false && XamarinShared.Setup.GetSecureValue("LockPin")!=null)
            {
                XamarinShared.Setup.RemoveSecureValue("LockPin");
                XamarinShared.Setup.RemoveSecureValue("LastAttemptTime");
                XamarinShared.Setup.RemoveSecureValue("NumberOfAttempts");
            }
        }

        private void MessageLimits_Clicked(object sender, EventArgs _)
        {
            sender.HandleButtonSingleClick();
            var clicked_Button =  sender as Button;
            MessageLimits_Zero.BorderColor = MessageLimits_Ten.BorderColor = MessageLimits_Twenty.BorderColor = MessageLimits_Thirty.BorderColor = Color.Transparent;
            try
            {
                NavigationTappedPage.Context.Setting.KeepPost = Convert.ToInt32(clicked_Button.Text);
            }
            catch (Exception)
            {
                NavigationTappedPage.Context.Setting.KeepPost = 100000;
            }
            clicked_Button.BorderColor = DesignResourceManager.GetColorFromStyle("Theme");
        }

        private void MessageDuration_Clicked(object sender, EventArgs e)
        {
            sender.HandleButtonSingleClick();
            var clicked_Button = sender as Button;
            MessageDuration_Zero.BorderColor =  MessageDuration_Ten.BorderColor = MessageDuration_Twenty.BorderColor = MessageDuration_Thirty.BorderColor = Color.Transparent;
            try
            {
                NavigationTappedPage.Context.Setting.PostPersistenceDays = Convert.ToInt32(clicked_Button.Text);
            }
            catch(Exception)
            {
                NavigationTappedPage.Context.Setting.PostPersistenceDays = 100000;
            }
            clicked_Button.BorderColor = DesignResourceManager.GetColorFromStyle("Theme");
        }

        private void Language_Clicked(object sender, EventArgs e)
        {
            sender.HandleButtonSingleClick();
            Application.Current.MainPage.Navigation.PushAsync(new LanguagePage(), false);
        }

        private void FAQ_Clicked(object sender, EventArgs e)
        {
            sender.HandleButtonSingleClick();
            Application.Current.MainPage.Navigation.PushAsync(new FAQPage(), false);
        }

        private void TestLogs_Clicked(object sender, EventArgs e)
        {
            sender.HandleButtonSingleClick();
            Application.Current.MainPage.Navigation.PushAsync(new LogsPage(), false);
        }

        private void ChatFontSize_Clicked(object sender, EventArgs e)
        {
            sender.HandleButtonSingleClick();
            Application.Current.MainPage.Navigation.PushAsync(new TextSizePage(), false);
        }

        private void SfSwitch_BackupStateChanged(object sender, SwitchStateChangedEventArgs e)
        {

            if ((bool)Backup.IsOn)
            {
                XamarinShared.Setup.SetSecureValue("Backup", "true");
                if (App.DriveService == null)
                    App.DriveService = DependencyService.Get<IDriveService>();
                App.DriveService.Init();
            }
            else
                XamarinShared.Setup.RemoveSecureValue("Backup");
        }

        private void InitMessageDuration()
        {
            switch (NavigationTappedPage.Context.Setting.PostPersistenceDays)
            {
                case 0:
                    MessageDuration_Clicked(MessageDuration_Zero, null);
                    break;
                case 10:
                    MessageDuration_Clicked(MessageDuration_Ten, null);
                    break;
                case 20:
                    MessageDuration_Clicked(MessageDuration_Twenty, null);
                    break;
                default:
                    MessageDuration_Clicked(MessageDuration_Thirty, null);
                    break;
            }
        }

        private void InitMessageLimit()
        {
            switch (NavigationTappedPage.Context.Setting.KeepPost)
            {
                case 0:
                    MessageLimits_Clicked(MessageLimits_Zero, null);
                    break;
                case 10:
                    MessageLimits_Clicked(MessageLimits_Ten, null);
                    break;
                case 20:
                    MessageLimits_Clicked(MessageLimits_Twenty, null);
                    break;
                default:
                    MessageLimits_Clicked(MessageLimits_Thirty, null);
                    break;
            }
        }
    }
}
