using CryptoWalletLibrary.Ehtereum.Models;
using CryptoWalletLibrary.Ehtereum.ViewModels;
using Rg.Plugins.Popup.Services;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CryptoWalletUI.Ehtereum.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ERC20BalancePage : BasePage
    {
        private readonly string tokenAbbr;
        public ERC20BalancePage(string abbr)
        {
            InitializeComponent();
            tokenAbbr = abbr;
            BindingContext = ERC20ViewModelLocator.ERC20ViewModelConstruc(abbr);

            if (Device.RuntimePlatform == Device.UWP)
            {
                var refreshButton = new Button
                {
                    Text = "Refresh"
                };

                refreshButton.Clicked += async (object _, EventArgs e) =>
                {
                    //TxList.BeginRefresh();
                    refreshButton.IsEnabled = false;
                    await EthTxViewModelLocator.EthTxViewModel.Refresh();
                    refreshButton.IsEnabled = true;

                };
                RefreshLayout.Children.Add(refreshButton);
            }
        }

        private async void ListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            var selectedEth = (EthTransaction)e.Item;
            ((ListView)sender).SelectedItem = null;
            await PopupNavigation.Instance.PushAsync(new ERC20TxDetailsPopup(selectedEth), true).ConfigureAwait(true);
        }

        public void OnShareAddressClciked(object _, EventArgs e) => Navigation.PushAsync(new ShareAddressPage(), false);

        public void OnSendEtherClciked(object _, EventArgs e) => Navigation.PushAsync(new SendERC20Page(tokenAbbr), false);

    }
}