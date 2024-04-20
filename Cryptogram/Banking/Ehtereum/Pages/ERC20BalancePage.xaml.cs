using Banking.Ehtereum.Models;
using Banking.Ehtereum.ViewModels;
using Rg.Plugins.Popup.Services;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Banking.Ehtereum.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ERC20BalancePage : ContentPage
    {
        private readonly string tokenAbbr;
        public ERC20BalancePage(string abbr)
        {
            InitializeComponent();
            tokenAbbr = abbr;
            BindingContext = new ERC20ViewModel(abbr);
        }
        private async void ListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            var selectedEth = (EthTransaction)e.Item;
            ((ListView)sender).SelectedItem = null;
            await PopupNavigation.Instance.PushAsync(new ERC20TxDetailsPopup(selectedEth), true).ConfigureAwait(true);
        }
        public void OnShareAddressClciked(object _, EventArgs e) => Navigation.PushAsync(new ShareAddressPage());
        public void OnSendEtherClciked(object _, EventArgs e) => Navigation.PushAsync(new SendERC20Page(tokenAbbr));

    }
}