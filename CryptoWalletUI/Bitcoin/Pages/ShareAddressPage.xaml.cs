using CryptoWalletLibrary.Bitcoin.Services;
using System;
using Xamarin.Essentials;

using Xamarin.Forms.Xaml;
namespace CryptoWalletUI.Bitcoin.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ShareAddressPage : BasePage
    {
        private readonly BtcCommonService btcCommonService;
        private readonly BtcAddressService btcAddressService;
        public ShareAddressPage()
        {
            btcCommonService = BtcCommonService.Instance;
            btcAddressService = BtcAddressService.Instance;
            InitializeComponent();
            Address.Text = btcCommonService.MainAddress.ToString();
        }
        public async void Back_Clicked(object _, EventArgs e) => await Navigation.PopAsync(false);
        public void OnRefreshButtonClicked(object _, EventArgs e)
        {
            btcAddressService.GenerateNewMainAdress();
            Address.Text = btcCommonService.MainAddress.ToString();
        }

        public async void OnCopyToClipBoardClicked(object _, EventArgs e) => await Clipboard.SetTextAsync(Address.Text);
    }
}