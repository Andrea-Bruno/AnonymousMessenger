using CryptoWalletLibrary.Bitcoin.Services;
using CryptoWalletLibrary.Models;
using CryptoWalletLibrary.Services;
using NBitcoin;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CryptoWalletUI.Bitcoin.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SendBitcoinPage : BasePage
    {
        private readonly BtcTransferService btcTransferService;
        private List<string> adresses;
        private Transaction tx;
        private ObservableCollection<UnspentCoin> _selectedUtxos;
        private (bool, bool, bool) AmountAddrFeeValid;
        public ObservableCollection<UnspentCoin> SelectedUtxos => _selectedUtxos ?? (_selectedUtxos = new ObservableCollection<UnspentCoin>());
        public SendBitcoinPage()
        {
            btcTransferService = BtcTransferService.Instance;

            MessagingCenter.Subscribe<CoinSelectionPopup>(this, "CoinSelected", (sender) => EnableSendButton());
            InitializeComponent();
        }

        public async void Back_Clicked(object _, EventArgs e) => await Navigation.PopAsync(false);

        public async void ManualCoinSeletion_Clicked(object _, EventArgs e) => await PopupNavigation.Instance.PushAsync(new CoinSelectionPopup(), false).ConfigureAwait(true);

        private void Address_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (e.NewTextValue == "")
            {
                AmountAddrFeeValid.Item2 = false;
                AddressError.Text = "";
                return;
            }
            try
            {
                BitcoinAddress.Create(e.NewTextValue, BtcCommonService.BitcoinNetwork);
            }
            catch
            {
                SendButton.IsEnabled = false;
                AddressError.Text = "Invalid Address";
                invalidUserInput.Text = "";
                AmountAddrFeeValid.Item2 = false;
                return;
            }
            AmountAddrFeeValid.Item2 = true;
            AddressError.Text = "";
            EnableSendButton();
        }

        private void Amount_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (e.NewTextValue == "")
            {
                AmountAddrFeeValid.Item1 = false;
                AmountError.Text = "";
                return;
            }
            if (!double.TryParse(Amount.Text, out var parsedAmount) || parsedAmount <= 0)
            {
                SendButton.IsEnabled = false;
                AmountError.Text = "Invalid Amount";
                invalidUserInput.Text = "";
                AmountAddrFeeValid.Item1 = false;
                return;
            }

            AmountAddrFeeValid.Item1 = true;
            AmountError.Text = "";
            EnableSendButton();
        }

        private void Fee_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (e.NewTextValue == "")
            {
                AmountAddrFeeValid.Item3 = false;
                FeeError.Text = "";
                return;
            }
            if (!double.TryParse(Fee.Text, out var parsedFee) || parsedFee <= 0)
            {
                SendButton.IsEnabled = false;
                FeeError.Text = "Invalid Fee";
                invalidUserInput.Text = "";
                AmountAddrFeeValid.Item3 = false;
                return;
            }

            AmountAddrFeeValid.Item3 = true;
            FeeError.Text = "";
            EnableSendButton();
        }

        private void ManualFeeSwitch_Toggled(object sender, ToggledEventArgs e)
        {
            Fee.IsEnabled = e.Value;
            if (e.Value)
            {
                FeeError.Text = (AmountAddrFeeValid.Item3 || Fee.Text == "" || Fee.Text == null) ? "" : "Invalid Fee";
                AutoFee.Text = "-";
                invalidUserInput.Text = "";
            }
            else
            {
                AutoFee.Text = "-";
                FeeError.Text = "";
            }
            EnableSendButton();
        }

        private void ManualCoinSwitch_Toggled(object sender, ToggledEventArgs e)
        {
            CoinSelection.IsEnabled = e.Value;
            EnableSendButton();
        }

        private void EnableSendButton()
        {
            if (!(AmountAddrFeeValid.Item1 && AmountAddrFeeValid.Item2 && (AmountAddrFeeValid.Item3 || !ManualFeeSelectionMode.IsToggled)))
            {
                SendButton.IsEnabled = false;
                AutoFee.Text = "-";
                return;
            }

            var btcAmount = BitcoinHelper.ParseBtcString(Amount.Text);
            var btcFee = Fee.IsEnabled ? BitcoinHelper.ParseBtcString(Fee.Text) : null;
            var address = Address.Text;
            adresses = new List<string> { address };

            try
            {
                tx = ManualCoinSelectionMode.IsToggled
                    ? btcTransferService.PrepareTxManualCoinSelection(btcAmount, btcFee, adresses)
                    : btcTransferService.PrepareTxCoinAutoSelection(btcAmount, adresses, btcFee);
                SendButton.IsEnabled = true;
                invalidUserInput.Text = "";
            }
            catch (Exception e)
            {
                SendButton.IsEnabled = false;

                var nextLineIdx = e.Message.IndexOf('\r');
                invalidUserInput.Text = nextLineIdx == -1 ? e.Message : e.Message.Substring(0, nextLineIdx);
            }
            if (!ManualFeeSelectionMode.IsToggled) AutoFee.Text = btcTransferService.AutoFee != null ? btcTransferService.AutoFee.ToString() : AutoFee.Text;
        }

        public async void Confirm_Clicked(object _, EventArgs e)
        {
            succTransac.IsVisible = false;
            var address = Address.Text;
            adresses = new List<string> { address };

            if (tx != null)
            {
                activIndic.IsVisible = true;
                activIndic.IsRunning = true;
                activIndicText.IsVisible = true;

                await Task.Run(() => btcTransferService.BroadCastTransactionAsync(tx));

                activIndic.IsVisible = false;
                activIndic.IsRunning = false;
                activIndicText.IsVisible = false;
                succTransac.IsVisible = true;

                if (btcTransferService.BroadcastedTransactionId != null) transacId.Text = btcTransferService.BroadcastedTransactionId.ToString();
                if (btcTransferService.TrBuildFailed) succTransac.Text = "There might be errors broadcasting your transaction";
                else
                {
                    Fee.Text = "";
                    FeeError.Text = "";
                    Amount.Text = "";
                    AmountError.Text = "";
                    Address.Text = "";
                    AddressError.Text = "";

                    ManualCoinSelectionMode.IsToggled = false;
                    ManualFeeSelectionMode.IsToggled = false;

                    btcTransferService.SelectedUnspentCoins?.Clear();
                }
            }
        }
    }
}