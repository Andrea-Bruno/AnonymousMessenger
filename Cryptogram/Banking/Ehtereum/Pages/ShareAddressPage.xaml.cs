using Banking.Ehtereum.Services;
using Banking.Ehtereum.ViewModels;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Banking.Ehtereum.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ShareAddressPage : ContentPage
    {
        EthereumWalletService EthereumWalletService;

        public ShareAddressPage()
        {
            BindingContext = EthTxViewModelLocator.EthTxViewModel;
            EthereumWalletService = EthereumWalletService.Instance;
            InitializeComponent();
        }
        public void OnRefreshButtonClicked(object _, EventArgs e)
        {
            EthereumWalletService.GetNewAddress();
            ShareAddress.Text = EthereumWalletService.ShareAddress;
            (BindingContext as EthTxViewModel).UpdateShareAddress();
        }
    }
}