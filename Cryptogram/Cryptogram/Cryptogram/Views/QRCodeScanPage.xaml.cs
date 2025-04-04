using System;
using CustomViewElements;
using Utils;
using Xamarin.Forms;
using ZXing;

namespace Cryptogram.Views
{
    public partial class QRCodeScanPage : BasePage
    {
        public delegate void AttachPublicKeyHandler(string qrCode);
        public event AttachPublicKeyHandler PublicKey;
        public QRCodeScanPage() => InitializeComponent();

        public void Handle_OnScanResult(Result result)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                PublicKey(result.Text);
                qrScanner.IsScanning = false;
                Navigation.RemovePage(this);
            });
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            qrScanner.IsScanning = false;
        }

        protected override void OnAppearing() => base.OnAppearing();

        private void Back_Clicked(object sender, EventArgs e)
        {
            sender.HandleButtonSingleClick();
            OnBackButtonPressed();
        }
    }
}