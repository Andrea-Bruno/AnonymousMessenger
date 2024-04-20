using Banking.Ehtereum.Models;
using Banking.Ehtereum.Pages;
using Banking.Ehtereum.ViewModels;
using Rg.Plugins.Popup.Services;
using System;


using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Banking.Ehtereum.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BalancePage : ContentPage
    {
        public BalancePage()
        {
            BindingContext = EthTxViewModelLocator.EthTxViewModel;
            InitializeComponent();
        }


        public void OnShareAddressClciked(object _, EventArgs e) => Navigation.PushAsync(new ShareAddressPage());

        public void OnSendEtherClciked(object _, EventArgs e) => Navigation.PushAsync(new SendEtherPage());

        private async void ListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            var selectedEth = (EthTransaction) e.Item;
            ((ListView)sender).SelectedItem = null;
            await PopupNavigation.Instance.PushAsync(new TxDetailsPopup(selectedEth), true).ConfigureAwait(true);
        }
    }
}