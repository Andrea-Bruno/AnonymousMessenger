using Banking.Bitcoin.Views;
using Banking.Models;
using Banking.Services;
using Rg.Plugins.Popup.Services;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Banking.Bitcoin.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ShowBalancePage : BasePage
    {
        private readonly BitcoinWalletService bitcoinWalletService;
        public ShowBalancePage()
        {
            bitcoinWalletService = BitcoinWalletService.Instance; 
            InitializeComponent();
        }

        public async void Back_Clicked(object _, EventArgs e) => await Navigation.PopAsync(false);

        public void OnSendBitcoinClciked(object _, EventArgs e) => Navigation.PushAsync(new SendBitcoinPage());

        public void OnReceiveBitcoinClciked(object _, EventArgs e) => Navigation.PushAsync(new ShareAddressPage());

        private async void ListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            BitcoinTransaction selected = e.Item as BitcoinTransaction;
            ((ListView)sender).SelectedItem = null;

            bitcoinWalletService.SelectedBitcoinTransaction = selected;
            await PopupNavigation.Instance.PushAsync(new TxDetailsPopup(), true).ConfigureAwait(true);
        }
    }
}