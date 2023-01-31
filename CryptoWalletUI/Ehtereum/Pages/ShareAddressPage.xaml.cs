using CryptoWalletLibrary.Ehtereum.Services;
using CryptoWalletLibrary.Ehtereum.ViewModels;
using System;
using Xamarin.Essentials;
using Xamarin.Forms.Xaml;

namespace CryptoWalletUI.Ehtereum.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ShareAddressPage : BasePage
    {
        readonly EthCommonService EthereumWalletService;
        private readonly EthAdressService ethAdressService;

        public ShareAddressPage()
        {
            BindingContext = EthTxViewModelLocator.EthTxViewModel;
            EthereumWalletService = EthCommonService.Instance;
            ethAdressService = EthAdressService.Instance;
            InitializeComponent();
        }
        public void OnRefreshButtonClicked(object _, EventArgs e)
        {
            ethAdressService.GetNewAddress();
            ShareAddress.Text = EthereumWalletService.ShareAddress;
            (BindingContext as EthTxViewModel).UpdateShareAddress();
        }

        public async void OnCopyToClipBoardClicked(object _, EventArgs e)
        {
            await Clipboard.SetTextAsync(ShareAddress.Text);
        }

        public async void Back_Clicked(object _, EventArgs e) => await Navigation.PopAsync(false);

    }
}