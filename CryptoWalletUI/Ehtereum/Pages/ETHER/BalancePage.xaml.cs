using CryptoWalletLibrary.Ehtereum.Models;
using CryptoWalletLibrary.Ehtereum.ViewModels;
using CryptoWalletUI.Ehtereum.Pages;
using Rg.Plugins.Popup.Services;
using System;


using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CryptoWalletUI.Ehtereum.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BalancePage : BasePage
    {
        public BalancePage()
        {
            BindingContext = EthTxViewModelLocator.EthTxViewModel;
            InitializeComponent();

            if (Device.RuntimePlatform == Device.UWP)
            {
                var refreshButton = new Button { Text = "Refresh" };

                refreshButton.Clicked += async (object _, EventArgs e) =>
                {
                    refreshButton.IsEnabled = false;
                    await (BindingContext as EthTxViewModel).Refresh();
                    refreshButton.IsEnabled = true;
                };
                RefreshLayout.Children.Add(refreshButton);
            }
        }

        public void OnShareAddressClciked(object _, EventArgs e) => Navigation.PushAsync(new ShareAddressPage(), false);

        public void OnSendEtherClciked(object _, EventArgs e) => Navigation.PushAsync(new SendEtherPage(), false);

        private async void ListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            var selectedEth = (EthTransaction)e.Item;
            ((ListView)sender).SelectedItem = null;
            await PopupNavigation.Instance.PushAsync(new TxDetailsPopup(selectedEth), true).ConfigureAwait(true);
        }
    }
}