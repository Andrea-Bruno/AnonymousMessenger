using System;
using CustomViewElements;
using Utils;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;
using ZXing.Net.Maui;

namespace Cryptogram.Views
{
    public partial class QRCodeScanPage : BasePage
    {
        public delegate void AttachPublicKeyHandler(string qrCode);
        public event AttachPublicKeyHandler PublicKey;
        public QRCodeScanPage() => InitializeComponent();

        public void Handle_OnScanResult(object sender, BarcodeDetectionEventArgs e)
        {
            if (e.Results == null || e.Results.Length == 0)
                return;
            string text = e.Results[0].Value;
            MainThread.BeginInvokeOnMainThread(() =>
            {
                PublicKey(text);
                qrScanner.IsDetecting = false;
                Navigation.RemovePage(this);
            });
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            qrScanner.IsDetecting = false;
        }

        protected override void OnAppearing() => base.OnAppearing();

        private void Back_Clicked(object sender, EventArgs e)
        {
            sender.HandleButtonSingleClick();
            OnBackButtonPressed();
        }
    }
}
