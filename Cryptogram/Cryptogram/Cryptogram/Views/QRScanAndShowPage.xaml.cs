using CustomViewElements;
using System;
using System.IO;
using System.Threading.Tasks;
using Cryptogram.DesignHandler;
using Cryptogram.Services;
using Utils;
using Xamarin.CommunityToolkit.Extensions;
using Xamarin.Essentials;
using Xamarin.Forms;
using ZXing;

namespace Cryptogram.Views
{
    public partial class QRScanAndShowPage : BasePage
    {
        private string _username;
        private byte[] _image;
        private string _publickey;
        public QRScanAndShowPage()
        {
            InitializeComponent();
        }

        public async Task ImageDisplayAsync()
        {
            try
            {
                if (_image != null)
                {
                    Task<ImageSource> result = Task<ImageSource>.Factory.StartNew(() => ImageSource.FromStream(() => new MemoryStream(_image)));
                    User_Photo.Source = await result;
                }
                else
                {
                    User_Photo.Source = DesignResourceManager.GetImageSource("ic_add_contact_profile.png");
                }
            }
            catch (Exception)
            {
            }
        }
    
        private void SetQRcode()
        {
            QRcode.BarcodeValue = EncryptedMessaging.ContactMessage.GetMyQrCode(NavigationTappedPage.Context);
        }

        public void Handle_OnScanResult(Result result)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                await Application.Current.MainPage.Navigation.PushAsync(new EditItemPage(result.Text));
            });
        }

        private async void CodeScanner_Clicked(object sender, EventArgs e)
        {
            sender.HandleButtonSingleClick();
            if (await PermissionManager.CheckCameraPermission())
            {
                var qrScanPage = new QRCodeScanPage();
                qrScanPage.PublicKey += GetUserPublicKeyQRScan;
                _ = Application.Current.MainPage.Navigation.PushAsync(qrScanPage, false);
            }
        }

        private void GetUserPublicKeyQRScan(string qrCode)
        {
            if (qrCode != null)
                Application.Current.MainPage.Navigation.PushAsync(new EditItemPage(qrCode), false);
        }

        private async void AddContact_Clicked(object sender, EventArgs e) => await Application.Current.MainPage.Navigation.PushAsync(new EditItemPage(), false);

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _username = NavigationTappedPage.Context.My.Name;
            _publickey = NavigationTappedPage.Context.My.GetPublicKey();
            Username.Text = _username;
            PublicKey.Text = _publickey;
            SetQRcode();
            if (_image == null)
            {
                _image = NavigationTappedPage.Context.My.GetAvatar();
                _ = ImageDisplayAsync();
            }
            else if (!_image.SequenceEqual(NavigationTappedPage.Context.My.GetAvatar()))
            {
                _image = NavigationTappedPage.Context.My.GetAvatar();
                _ = ImageDisplayAsync();
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            App.ReEstablishConnection();
        }

        private void Copy_Clicked(object sender, EventArgs e_)
        {
            sender.HandleButtonSingleClick(500);
            if (_publickey != null)
            {
                Clipboard.SetTextAsync(EncryptedMessaging.ContactMessage.GetMyQrCode(NavigationTappedPage.Context));
                this.DisplayToastAsync(Localization.Resources.Dictionary.CopiedToClipboard);
            }
        }

        private async void Send_Clicked(object sender, EventArgs e)
        {
            sender.HandleButtonSingleClick();
            await Share.RequestAsync(new ShareTextRequest
            {
                Text = EncryptedMessaging.ContactMessage.GetMyQrCode(NavigationTappedPage.Context)
            }).ConfigureAwait(true);
        }
    }
}