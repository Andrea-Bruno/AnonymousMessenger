using CustomViewElements;
using System;
using Anonymous.DesignHandler;
using Anonymous.Services;
using EncryptedMessaging;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.CommunityToolkit.Extensions;
using Utils;

namespace Anonymous.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EditItemPage : BasePage
    {
        private string _qrCode;
        private Contact _contact;

        public EditItemPage(string qrCode = null)
        {
            InitializeComponent();
            BindingContext = this;
            Name.Keyboard = Keyboard.Create(KeyboardFlags.CapitalizeWord);
            _qrCode = qrCode;
            InitView();
        }

        protected override void OnAppearing() => base.OnAppearing();

        private void InitView()
        {
            if (_qrCode != null)
            {
                try
                {
                    GetUserPublicKeyQRScan(_qrCode);
                    _qrCode = null;
                }
                catch (Exception)
                {
                    SetVisibility(true);
                    this.DisplayToastAsync(Localization.Resources.Dictionary.SomethingGoesWrongWithQrScanner);
                }
            }
            else
                SetVisibility(true);

        }

        private void PublicKeyTextChanged(object sender, TextChangedEventArgs args)
        {
            try
            {
                if (PublicKey.Text.Length > 50)
                    GetUserPublicKeyQRScan(PublicKey.Text);
            }
            catch (Exception)
            {
            }
        }

        private async void Scan_Clicked(object sender, EventArgs e)
        {
            sender.HandleButtonSingleClick();
            if (await PermissionManager.CheckCameraPermission())
            {
                var qrScanPage = new QRCodeScanPage();
                qrScanPage.PublicKey += GetUserPublicKeyQRScan;
                _ = Application.Current.MainPage.Navigation.PushAsync(qrScanPage, false);
            }
        }

        private ContactMessage _contactMessage = null;

        private void GetUserPublicKeyQRScan(string qrCode)
        {
            _contactMessage = ContactMessage.GetContactMessage(qrCode);
            if (_contactMessage != null)
            {
                SetVisibility(false);
                Name.Text = _contactMessage.Name;
                PublicKey.Text = Convert.ToBase64String(_contactMessage.Participants[0].Key);
            }
        }

        private void SetVisibility(bool isQrCodeEmpty)
        {
            PublicKeyFrame.IsVisible = PublicKey_Label.IsVisible = isQrCodeEmpty;
            UserName_Label.IsVisible = NameFrame.IsVisible = !isQrCodeEmpty;
        }

        private void Add_Clicked(object sender, EventArgs e)
        {
            sender.HandleButtonSingleClick();
            if (string.IsNullOrWhiteSpace(PublicKey.Text) || string.IsNullOrWhiteSpace(Name.Text))
            {
                this.DisplayToastAsync(Localization.Resources.Dictionary.PleaseFillAllTheBlanks);
            }
            else
            {
                ShowProgressDialog();
                var timer = new System.Threading.Timer((object obj) => { Device.BeginInvokeOnMainThread(() => AddContact()); }, null, 100, System.Threading.Timeout.Infinite);
            }
        }

        private async void AddContact()
        {
            if (_contactMessage != null)
            {
                _contactMessage.Name = Name.Text;
                _contact = NavigationTappedPage.Context.Contacts.AddContact(_contactMessage, App.Setting.SendContact ? Contacts.SendMyContact.Send : Contacts.SendMyContact.None);
            }
            else
            {
                if (_contact != null)
                {
                    if (_contact.PublicKeys == PublicKey.Text)
                    {
                        _contact.Name = Name.Text;
                        return;
                    }
                }
                _contact = NavigationTappedPage.Context.Contacts.AddContactByKeys(PublicKey.Text, Name.Text, default, App.Setting.SendContact ? Contacts.SendMyContact.Send : Contacts.SendMyContact.None);
            }
            if (_contact != null)
            {

               NavigationTappedPage.Context?.Messaging.SendContactStatus(_contact);
                try
                {
                    if (Device.RuntimePlatform == Device.Android)
                        DependencyService.Get<ISharedPreference>().AddContact(_contact.ChatId + "", _contact.Name, _contact.Os == Contact.RuntimePlatform.Android);
                    App.SendNotification(_contact, NotificationService.NotificationType.CONTACT);
                }
                catch (Exception)
                {

                }
                await Application.Current.MainPage.Navigation.PopToRootAsync();
                ((App)Application.Current).GetRootPage()?.ShowRequiredView(chatIdRequired: _contact.ChatId);
            }
            else
            {
                this.DisplayToastAsync(Localization.Resources.Dictionary.PublicKeyIsNotValid);
            }
            HideProgressDialog();
        }

        void CustomEntry_Focused(object sender, FocusEventArgs e)
        {
            (sender as CustomEntry).PlaceholderColor = DesignResourceManager.GetColorFromStyle("BackgroundSecondary");
        }

        void CustomEntry_Unfocused(object sender, FocusEventArgs e)
        {
            (sender as CustomEntry).PlaceholderColor = DesignResourceManager.GetColorFromStyle("WhiteColor");
        }

        private void Back_Clicked(object sender, EventArgs args)
        {
            sender.HandleButtonSingleClick();
            OnBackButtonPressed();
        }
    }
}
