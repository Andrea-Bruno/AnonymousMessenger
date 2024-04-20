using Banking.Services;
using System;

using Xamarin.Forms.Xaml;
namespace Banking.Bitcoin.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ShareAddressPage : BasePage
    {
        private readonly BitcoinWalletService bitcoinWalletService;
        public ShareAddressPage()
        {
            bitcoinWalletService = BitcoinWalletService.Instance;
            InitializeComponent();
            ChangeAddress.Text = bitcoinWalletService.MainAddress.ToString();
        }
        public async void Back_Clicked(object _, EventArgs e) => await Navigation.PopAsync(false);
        public void OnRefreshButtonClicked(object _, EventArgs e)
        {
            bitcoinWalletService.generateNewMainAdress();
            ChangeAddress.Text = bitcoinWalletService.MainAddress.ToString();
        }
    }
}