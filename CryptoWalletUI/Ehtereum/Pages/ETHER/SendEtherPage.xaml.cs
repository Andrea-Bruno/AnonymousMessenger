using CryptoWalletLibrary.Ehtereum.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Numerics;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CryptoWalletUI.Ehtereum.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SendEtherPage : BasePage
    {
        private readonly EthCommonService ethCommonService;
        private readonly EthTransferService ethTransferService;

        private readonly List<string> accounts;
        private readonly ObservableCollection<string> accountAndBalance;
        private (bool, bool, bool, bool) Amount_Addr_GasPrice_GasLimit_Valid;
        private readonly int autoGasPrice;
        private BigInteger autoGasLimit;
        private double parsedAmount;
        private string parsedAddress;
        private int parsedManualGasPrice;
        private int parsedManualGasLimit;

        public SendEtherPage()
        {
            ethCommonService = EthCommonService.Instance;
            ethTransferService = EthTransferService.Instance;

            InitializeComponent();

            accountAndBalance = new ObservableCollection<string>();
            accounts = new List<string>();
            UpdateAccountsAndBalance();
            var pickerSource = new Picker { Title = "Accounts", TitleColor = Color.Red };
            myPicker.ItemsSource = accountAndBalance;
            myPicker.SelectedIndex = 0;
            myPicker.SelectedIndexChanged += Account_SelectedIndexChanged;

            GasLimit.Text = "21000";
            autoGasPrice = (int)((double)(ethCommonService.Fee.BaseFee + ethCommonService.Fee.MaxPriorityFeePerGas) / 1e9);
            AutoGasPrice.Text = autoGasPrice.ToString();
            Fee.Text = string.Format("{0:n6}", (double)(autoGasPrice * 21000 / 1e9));

            Amount_Addr_GasPrice_GasLimit_Valid = (false, false, false, true);
        }

        private void Account_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (myPicker.SelectedIndex != -1)
                EnableSendButton();
        }

        private void Amount_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!double.TryParse(Amount.Text, out parsedAmount) || parsedAmount <= 0)
            {
                parsedAmount = -1;
                SendButton.IsEnabled = false;
                AmountError.Text = Amount.Text != "" ? "Invalid Amount" : "";
                Amount_Addr_GasPrice_GasLimit_Valid.Item1 = false;

                return;
            }
            Amount_Addr_GasPrice_GasLimit_Valid.Item1 = true;
            AmountError.Text = "";
            EnableSendButton();
        }

        private void Address_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!EthAdressService.CheckIfAddrValid(e.NewTextValue))
            {
                parsedAddress = "";
                SendButton.IsEnabled = false;
                AddressError.Text = Address.Text != "" ? "Invalid Address" : "";
                Amount_Addr_GasPrice_GasLimit_Valid.Item2 = false;

                return;
            }
            parsedAddress = Address.Text;
            Amount_Addr_GasPrice_GasLimit_Valid.Item2 = true;
            AddressError.Text = "";
            EnableSendButton();
        }

        private void GasPrice_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!int.TryParse(GasPrice.Text, out parsedManualGasPrice) || parsedManualGasPrice <= 0)
            {
                parsedManualGasPrice = -1;
                SendButton.IsEnabled = false;
                GasPriceError.Text = GasPrice.Text != "" ? "Invalid GasPrice" : "";
                Amount_Addr_GasPrice_GasLimit_Valid.Item3 = false;
                return;
            }
            Amount_Addr_GasPrice_GasLimit_Valid.Item3 = true;
            GasPriceError.Text = "";
            EnableSendButton();
            CalculateTotalFee();

            if (double.TryParse(GasLimit.Text, out var parsedGasLimit))
                Fee.Text = string.Format("{0:n6}", parsedManualGasPrice * parsedGasLimit / 1e9);
        }
        private void GasLimit_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!int.TryParse(GasLimit.Text, out parsedManualGasLimit) || parsedManualGasLimit <= 0)
            {
                SendButton.IsEnabled = false;
                GasLimitError.Text = GasLimit.Text != "" ? "Invalid GasLimit" : "";
                //invalidUserInput.Text = "";
                Amount_Addr_GasPrice_GasLimit_Valid.Item4 = false;
                return;
            }
            Amount_Addr_GasPrice_GasLimit_Valid.Item4 = true;
            GasLimitError.Text = "";
            EnableSendButton();
            if (double.TryParse(GasPrice.Text, out var parsedGasPrice))
                Fee.Text = string.Format("{0:n6}", parsedManualGasLimit * parsedGasPrice / 1e9);
        }

        private void ManualGasPriceSelectionMode_Toggled(object sender, ToggledEventArgs e)
        {
            GasPrice.IsEnabled = e.Value;
            if (e.Value)
            {
                GasPriceError.Text = (Amount_Addr_GasPrice_GasLimit_Valid.Item3 || GasPrice.Text == "" || GasPrice.Text == null) ? "" : "Invalid Gas Price";
                AutoGasPrice.Text = "-";
                //invalidUserInput.Text = "";
            }
            else
            {
                AutoGasPrice.Text = autoGasPrice.ToString();
                GasPriceError.Text = "";
            }
            EnableSendButton();
        }


        private void CalculateTotalFee()
        {
            var gasPrice = ManualGasPriceSelectionMode.IsToggled ? parsedManualGasPrice : autoGasPrice;
            if (gasPrice > 0 && autoGasLimit > 0)
            {
                TotalFee.Text = string.Format("{0:n6}", (double)(gasPrice * autoGasLimit) / 1e9);
            }
            else
                TotalFee.Text = "";
        }

        public async void Confirm_Clicked(object _, EventArgs e)
        {
            activIndic.IsVisible = true;
            activIndic.IsRunning = true;
            activIndicText.IsVisible = true;
            SendButton.IsEnabled = false;
            TransacSuccessStatus.IsVisible = false;
            transacId.Text = "";

            var sourceAddress = accounts[myPicker.SelectedIndex];
            var destionationAddress = Address.Text;
            var amount = decimal.Parse(Amount.Text);
            var gasPrice = ManualGasPriceSelectionMode.IsToggled ? int.Parse(GasPrice.Text) : int.Parse(AutoGasPrice.Text);
            var gasLimit = int.Parse(GasLimit.Text);

            var (doneAndPendingTxCount, doneTxCount) = await ethTransferService.CheckPendingTxAsync(sourceAddress);

            int? nonce = null;
            if (doneAndPendingTxCount > doneTxCount)
            {
                string action = await DisplayActionSheet("Pending txs detected," +
                    " would you like to send this tx  on top of them " +
                    "(in this case this newly sent tx also will be pending  till previos txs will be included to blocks)" +
                    "or send by canceling theese pending txs? ", "Cancel", null,
                     "cancel pending txs and send",
                    "on top of pending txs(please note that this transaction might be canceled in future if previous txs spending will leave your adress with insufficient balance for this transaction)");
                switch (action)
                {
                    case "Cancel":
                        return;
                    case "on top of pending txs":
                        nonce = doneAndPendingTxCount;
                        break;
                    case "cancel pending txs and send":
                        nonce = doneTxCount;
                        break;
                    default:
                        break;
                }
            }
            var txdId = await ethTransferService.SendEther(sourceAddress, destionationAddress, amount, gasPrice, gasLimit, nonce);
            UpdateAccountsAndBalance();

            activIndic.IsVisible = false;
            activIndic.IsRunning = false;
            activIndicText.IsVisible = false;
            TransacSuccessStatus.IsVisible = true;

            if (txdId != null)
            {
                TransacSuccessStatus.Text = "Transaction is sent, your Transaction ID:";
                transacId.IsVisible = true;
                transacId.Text = txdId;
            }
            else
            {
                TransacSuccessStatus.Text = "Something went wrong, please try again!";
            }

            ClearInputFields();
        }


        private void EnableSendButton()
        {
            if (!(
                Amount_Addr_GasPrice_GasLimit_Valid.Item1
                && Amount_Addr_GasPrice_GasLimit_Valid.Item2
                && (Amount_Addr_GasPrice_GasLimit_Valid.Item3 || !ManualGasPriceSelectionMode.IsToggled)
                && Amount_Addr_GasPrice_GasLimit_Valid.Item4
                ))
            {
                SendButton.IsEnabled = false;
                return;
            }

            var parsedAmount = double.Parse(Amount.Text);
            var parsedGasPrice = ManualGasPriceSelectionMode.IsToggled ? int.Parse(GasPrice.Text) : int.Parse(AutoGasPrice.Text);
            var parsedGasLimit = int.Parse(GasLimit.Text);

            var amountPossesed = ethCommonService.BalanceByAddress[accounts[myPicker.SelectedIndex]].Total;
            if (parsedAmount + (parsedGasLimit * parsedGasPrice / 1e9) < amountPossesed)
                SendButton.IsEnabled = true;
            else
                SendButton.IsEnabled = false;
        }

        private void ClearInputFields()
        {
            Amount.Text = "";
            Address.Text = "";
            GasPrice.Text = "";
            myPicker.SelectedIndex = 0;
            ManualGasPriceSelectionMode.IsToggled = false;
        }

        private void UpdateAccountsAndBalance()
        {
            accountAndBalance.Clear();
            accounts.Clear();
            foreach (var addrBalance in ethCommonService.BalanceByAddress)
            {
                if (addrBalance.Value.Total != 0)
                {
                    accountAndBalance.Add(addrBalance.Key.ToString() + " (Balance Confirmed: " + addrBalance.Value.Confirmed + ", Total: " + addrBalance.Value.Total + ")");
                    accounts.Add(addrBalance.Key.ToString());
                }
            }
            myPicker.SelectedIndex = 0;
        }

        public async void Back_Clicked(object _, EventArgs e) => await Navigation.PopAsync(false);

    }
}