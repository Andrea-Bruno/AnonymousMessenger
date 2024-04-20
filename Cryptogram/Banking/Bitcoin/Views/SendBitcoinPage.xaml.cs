using Banking.Models;
using Banking.Services;
using NBitcoin;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Rg.Plugins.Popup.Services;
using System.Threading.Tasks;
using System.Linq;

namespace Banking.Bitcoin.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SendBitcoinPage : BasePage
    {
        private readonly BitcoinWalletService bitcoinWalletService;

        List<string> adresses;
        string amount;
        string fee;
        public ObservableCollection<BitcoinTransaction> transactions { get; set; }
        public SendBitcoinPage()
        {
            bitcoinWalletService =  BitcoinWalletService.Instance;
            MessagingCenter.Subscribe<CoinSelectionPopup>(this, "CoinSelected", (sender) =>
            {
                var address = Address.Text;
                adresses = new List<string> { address };
                amount = Amount.Text;
                fee = Fee.Text;
                if (amount.Equals('0') && adresses != null)
                    SendButton.IsEnabled = bitcoinWalletService.CheckCoinTransferPosiblity(amount, fee, bitcoinWalletService.SelectedUnspentCoins);
            });
            InitializeComponent();
        }


        public async void Back_Clicked(object _, EventArgs e) => await Navigation.PopAsync(false);

        public async void ManualCoinSeletion_Clicked(object _, EventArgs e) => await PopupNavigation.Instance.PushAsync(new CoinSelectionPopup(), true).ConfigureAwait(true);

        private void Amount_TextChanged(object sender, TextChangedEventArgs e) => EnableSendButton();

        private void Fee_TextChanged(object sender, TextChangedEventArgs e) => EnableSendButton();

        private void Address_TextChanged(object sender, TextChangedEventArgs e) => EnableSendButton();

        private void EnableSendButton()
        {
            var isEnabled = false;
            if (double.TryParse(Amount.Text, out var parsedAmount) && double.TryParse(Fee.Text, out var parsedFee) && (Address.Text != null && Address.Text != ""))
            {
                double amountPossesed;
                if (bitcoinWalletService.SelectedUnspentCoins != null)
                    amountPossesed = bitcoinWalletService.SelectedUnspentCoins.Sum(unspentCoin => double.Parse(unspentCoin.Amount));
                else
                    amountPossesed = bitcoinWalletService.TotalConfirmedBalance + bitcoinWalletService.TotalUncnfirmedBalance;
                if (parsedAmount + parsedFee < amountPossesed && parsedAmount > 0 && parsedFee >= 0) isEnabled = true;
            }
            SendButton.IsEnabled = isEnabled;
        }

        public async void Confirm_Clicked(object _, EventArgs e)
        {
            succTransac.IsVisible = false;
            var address = Address.Text;
            adresses = new List<string> { address };
            amount = Amount.Text;
            fee = Fee.Text;

            if (double.TryParse(fee, out var parsedFee) && double.TryParse(amount, out var parsedAmount))
            {
                Money btcFeee = BitcoinHelper.ParseBtcString(fee);
                Money btcAmount = BitcoinHelper.ParseBtcString(amount);

                activIndic.IsVisible = true;
                activIndic.IsRunning = true;
                activIndicText.IsVisible = true;
                if (bitcoinWalletService.SelectedUnspentCoins != null && bitcoinWalletService.SelectedUnspentCoins.Count != 0)
                    await Task.Run(() => bitcoinWalletService.SendCoinsManualSelectionAsync(btcAmount, btcFeee, adresses));
                else
                    await Task.Run(() => bitcoinWalletService.SendCoinAutoSelection(btcAmount, btcFeee, adresses));

                activIndic.IsVisible = false;
                activIndic.IsRunning = false;
                activIndicText.IsVisible = false;
                succTransac.IsVisible = true;
                transacId.Text = bitcoinWalletService.BroadcastedTransactionId.ToString();

                if (bitcoinWalletService.TrBuildFailed)
                    succTransac.Text = "There might be errors broadcasting your transaction";
            }
        }

        private void ListView_ItemSelected(object sender, SelectedItemChangedEventArgs e) { }

        private void ListView_ItemTapped(object sender, ItemTappedEventArgs e) { }
    }
}