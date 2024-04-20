using Banking.Models;
using Banking.Services;
using Rg.Plugins.Popup.Services;
using System;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;


namespace Banking.Bitcoin.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CoinSelectionPopup : Rg.Plugins.Popup.Pages.PopupPage
    {
        private readonly BitcoinWalletService bitcoinWalletService;
        public CoinSelectionPopup()
        {
            bitcoinWalletService = BitcoinWalletService.Instance;
            InitializeComponent();
        }


        private void CollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((CollectionView)sender).SelectedItems != null)
                bitcoinWalletService.SelectedUnspentCoins = ((CollectionView)sender).SelectedItems.Cast<UnspentCoin>().ToList();
        }

        public async void Confirm_Clicked(object _, EventArgs e)
        {
            MessagingCenter.Send(this, "CoinSelected");
            await PopupNavigation.Instance.PopAsync(false);
        }

        private async void Back_Clicked(object sender, EventArgs e) => await PopupNavigation.Instance.PopAsync(false);

        private void ListView_ItemSelected(object sender, SelectedItemChangedEventArgs e) { }

        private void ListView_ItemTapped(object sender, ItemTappedEventArgs e) { }
    }
}