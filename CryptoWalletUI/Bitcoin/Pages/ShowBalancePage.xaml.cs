using CryptoWalletLibrary.Bitcoin.Services;
using CryptoWalletLibrary.Models;
using Rg.Plugins.Popup.Services;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using static CryptoWalletLibrary.Models.BitcoinTransactionViewModel;

namespace CryptoWalletUI.Bitcoin.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ShowBalancePage : BasePage
    {
        private readonly BtcCommonService btcCommonService;
        public ShowBalancePage()
        {
            btcCommonService = BtcCommonService.Instance;
            BindingContext = BtcTxViewModelLocator.Instance;

            InitializeComponent();
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
                    await (BindingContext as BitcoinTransactionViewModel).Refresh();
                    //Task.Run(async () => await (BindingContext as BitcoinTransactionViewModel).Refresh());
                    refreshButton.IsEnabled = true;

                };
                RefreshLayout.Children.Add(refreshButton);
            }
        }

        public async void Back_Clicked(object _, EventArgs e) => await Navigation.PopAsync(false);

        public void OnSendBitcoinClciked(object _, EventArgs e) => Navigation.PushAsync(new SendBitcoinPage(), false);

        public void OnReceiveBitcoinClciked(object _, EventArgs e) => Navigation.PushAsync(new ShareAddressPage(), false);

        private async void ListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            BitcoinTransaction selected = e.Item as BitcoinTransaction;
            ((ListView)sender).SelectedItem = null;

            btcCommonService.SelectedBitcoinTransaction = selected;
            await PopupNavigation.Instance.PushAsync(new TxDetailsPopup(), false).ConfigureAwait(true);

        }
    }
}