using CustomViewElements;
using Syncfusion.XForms.Buttons;
using System;
using Telegraph.DesignHandler;
using Telegraph.PopupViews;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Telegraph.Views
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
            NavigationTappedPage.Context.Contacts.RestoreContactFromCloud();
        }

        private async void Share_PublicKey_Clicked(object sender, EventArgs e)
        {
            await Share.RequestAsync(new ShareTextRequest
            {
                Text = EncryptedMessaging.ContactMessage.GetMyQrCode(NavigationTappedPage.Context)
            }).ConfigureAwait(true);
        }

        private async void PrivacyPolicy_Clicked(object sender, System.EventArgs e)
        {
            Uri uri = new Uri("https://uupsocial.tech/pp.html");
            await Browser.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
        }      

        private void MenuText_Clicked(object sender, EventArgs e)
        {
            TestLogs.IsVisible = true;
        }

        private void VerifyPrivateKey_Clicked(object sender, EventArgs e)
        {
            Application.Current.MainPage.Navigation.PushAsync(new ShowPublicKeyPage(Preferences.Get("LoggedTime", DateTime.UtcNow)), false);
        }

        private void CheckSwitchCase()
        {
            AppLock1.IsOn = XamarinShared.Setup.GetSecureValue("LockPin") != null;
            SendContact1.IsOn = App.Setting.SendContact != false;
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

        private void SfSwitch_SendContactStateChanged(object sender, SwitchStateChangedEventArgs e)
        {
            App.Setting.SendContact = (bool)SendContact1.IsOn;
            XamarinShared.Setup.Settings.Save();
        }

        private void MessageLimits_Clicked(object sender, EventArgs _)
        {
            var clicked_Button =  sender as Button;
            MessageLimits_Zero.BorderColor = MessageLimits_Ten.BorderColor = MessageLimits_Twenty.BorderColor = MessageLimits_Thirty.BorderColor = Color.Transparent;
            NavigationTappedPage.Context.Setting.KeepPost = Convert.ToInt32(clicked_Button.Text);
            clicked_Button.BorderColor = DesignResourceManager.GetColorFromStyle("Theme");
        }

        private void MessageDuration_Clicked(object sender, EventArgs e)
        {
            var clicked_Button = sender as Button;
            MessageDuration_Zero.BorderColor =  MessageDuration_Ten.BorderColor = MessageDuration_Twenty.BorderColor = MessageDuration_Thirty.BorderColor = Color.Transparent;
            NavigationTappedPage.Context.Setting.PostPersistenceDays = Convert.ToInt32(clicked_Button.Text);
            clicked_Button.BorderColor = DesignResourceManager.GetColorFromStyle("Theme");
        }

        private void Language_Clicked(object sender, EventArgs e) => Application.Current.MainPage.Navigation.PushAsync(new LanguagePage(), false);

        private void FAQ_Clicked(object sender, EventArgs e) => Application.Current.MainPage.Navigation.PushAsync(new FAQPage(), false);

        private void TestLogs_Clicked(object sender, EventArgs e) => Application.Current.MainPage.Navigation.PushAsync(new LogsPage(), false);

        private void ChatFontSize_Clicked(object sender, EventArgs e) => Application.Current.MainPage.Navigation.PushAsync(new TextSizePage(), false);
    }
}
