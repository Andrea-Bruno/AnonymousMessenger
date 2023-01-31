using CryptoWalletLibrary;
using CryptoWalletLibrary.Ehtereum;
using CryptoWalletLibrary.Ehtereum.Services;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CryptoWalletUI.Ehtereum.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SendERC20Page : BasePage
    {
        private readonly EthCommonService ethereumWalletService;
        private readonly EthTransferService ethTransferService;

        private readonly List<string> accounts;
        private readonly List<string> accountAndBalance;
        private readonly string tokenAbbr;
        private Dictionary<string, EthBalance> addrBalanceByToken;

        private readonly int autoGasPrice;
        private BigInteger autoGasLimit;
        private int parsedAmount;
        private string parsedAddress;
        private int parsedManualGasPrice;
        private int parsedManualGasLimit;


        private (bool, bool, bool, bool) Amount_Addr_ManualGasPrice_ManualGasLimit_Valid;


        public SendERC20Page(string tokenAbbr)
        {
            ethereumWalletService = EthCommonService.Instance;
            ethTransferService = EthTransferService.Instance;
            InitializeComponent();

            this.tokenAbbr = tokenAbbr;
            accountAndBalance = new List<string>();
            accounts = new List<string>();
            addrBalanceByToken = new Dictionary<string, EthBalance>();
            UpdateAccountsAndBalance();

            myPicker.ItemsSource = accountAndBalance;
            myPicker.SelectedIndexChanged += Account_SelectedIndexChanged;
            myPicker.SelectedIndex = 0;

            ManualGasLimit.Text = "";
            autoGasPrice = (int)((double)(ethereumWalletService.Fee.BaseFee + ethereumWalletService.Fee.MaxPriorityFeePerGas) / 1e9);
            AutoGasPrice.Text = autoGasPrice.ToString();
            TotalFee.Text = "-";

            Amount_Addr_ManualGasPrice_ManualGasLimit_Valid = (false, false, false, false);
        }

        //OnChangeMethods
        private void Account_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (myPicker.SelectedIndex != -1)
                EnableSendButton();
        }

        private void Amount_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!int.TryParse(Amount.Text, out parsedAmount) || parsedAmount <= 0)
            {
                parsedAmount = -1;
                SendButton.IsEnabled = false;
                AmountError.Text = Amount.Text != "" ? "Invalid Amount" : "";
                AutoGasLimit.Text = "-";
                if (!ManualGasLimitSelectionMode.IsToggled) TotalFee.Text = "-";
                autoGasLimit = -1;
                Amount_Addr_ManualGasPrice_ManualGasLimit_Valid.Item1 = false;

                return;
            }
            Amount_Addr_ManualGasPrice_ManualGasLimit_Valid.Item1 = true;
            AmountError.Text = "";
            UpdateGaslimit().ContinueWith(task =>
            {
                CalculateTotalFee();
                EnableSendButton();
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void Address_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!EthAdressService.CheckIfAddrValid(e.NewTextValue))
            {
                parsedAddress = "";
                SendButton.IsEnabled = false;
                AddressError.Text = Address.Text != "" ? "Invalid Address" : "";
                AutoGasLimit.Text = "-";
                if (!ManualGasLimitSelectionMode.IsToggled) TotalFee.Text = "-";
                autoGasLimit = -1;
                Amount_Addr_ManualGasPrice_ManualGasLimit_Valid.Item2 = false;

                return;
            }
            parsedAddress = Address.Text;

            Amount_Addr_ManualGasPrice_ManualGasLimit_Valid.Item2 = true;
            AddressError.Text = "";
            UpdateGaslimit().ContinueWith(task =>
            {
                CalculateTotalFee();
                EnableSendButton();
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void GasPrice_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!int.TryParse(ManualGasPrice.Text, out parsedManualGasPrice) || parsedManualGasPrice <= 0)
            {
                parsedManualGasPrice = -1;
                TotalFee.Text = "-";
                SendButton.IsEnabled = false;
                ManualGasPriceError.Text = ManualGasPrice.Text != "" ? "Invalid Gas Price" : "";
                Amount_Addr_ManualGasPrice_ManualGasLimit_Valid.Item3 = false;
                return;
            }
            Amount_Addr_ManualGasPrice_ManualGasLimit_Valid.Item3 = true;
            ManualGasPriceError.Text = "";
            EnableSendButton();
            CalculateTotalFee();
        }

        private void ManualGasPriceSelectionMode_Toggled(object sender, ToggledEventArgs e)
        {
            ManualGasPrice.IsEnabled = e.Value;
            if (e.Value)
            {
                ManualGasPriceError.Text = (Amount_Addr_ManualGasPrice_ManualGasLimit_Valid.Item3 || ManualGasPrice.Text == "" || ManualGasPrice.Text == null) ? "" : "Invalid Gas Price";
                AutoGasPrice.Text = "-";
            }
            else
            {
                AutoGasPrice.Text = autoGasPrice.ToString();
                ManualGasPriceError.Text = "";
            }

            CalculateTotalFee();
            EnableSendButton();
        }

        private void GasLimit_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!int.TryParse(ManualGasLimit.Text, out parsedManualGasLimit) || parsedManualGasLimit <= 0)
            {
                parsedManualGasLimit = -1;
                TotalFee.Text = "-";
                SendButton.IsEnabled = false;
                ManualGasLimitError.Text = ManualGasLimit.Text != "" ? "Invalid Gas Limit" : "";
                Amount_Addr_ManualGasPrice_ManualGasLimit_Valid.Item4 = false;
                return;
            }
            Amount_Addr_ManualGasPrice_ManualGasLimit_Valid.Item4 = true;
            ManualGasLimitError.Text = "";
            CalculateTotalFee();
            EnableSendButton();
        }

        private void ManualGasLimitSelectionMode_Toggled(object sender, ToggledEventArgs e)
        {
            ManualGasLimit.IsEnabled = e.Value;
            if (e.Value)
            {
                ManualGasLimitError.Text = (Amount_Addr_ManualGasPrice_ManualGasLimit_Valid.Item4 || ManualGasLimit.Text == "" || ManualGasLimit.Text == null) ? "" : "Invalid Gas Limit";
                AutoGasLimit.Text = "-";
            }
            else
            {
                AutoGasLimit.Text = Amount_Addr_ManualGasPrice_ManualGasLimit_Valid.Item1 && Amount_Addr_ManualGasPrice_ManualGasLimit_Valid.Item2 ? autoGasLimit.ToString() : "-";
                ManualGasLimitError.Text = "";
            }

            CalculateTotalFee();
            EnableSendButton();
        }


        //helpers
        private async Task UpdateGaslimit()
        {
            autoGasLimit = await CalculateGasLimitAsync();
            AutoGasLimit.Text = autoGasLimit > 0 && !ManualGasLimitSelectionMode.IsToggled ? autoGasLimit.ToString() : "-";
        }

        private async Task<BigInteger> CalculateGasLimitAsync()
        {
            if (Amount_Addr_ManualGasPrice_ManualGasLimit_Valid.Item1 && Amount_Addr_ManualGasPrice_ManualGasLimit_Valid.Item2)
            {
                var sourceAddress = accounts[myPicker.SelectedIndex];
                var gasLimit = await ethTransferService.EstimateContractGasLimit(sourceAddress, parsedAddress, parsedAmount, tokenAbbr);
                return gasLimit;
            }
            else return 0;
        }

        private void CalculateTotalFee()
        {
            var gasPrice = ManualGasPriceSelectionMode.IsToggled ? parsedManualGasPrice : autoGasPrice;
            var gasLimit = ManualGasLimitSelectionMode.IsToggled ? parsedManualGasLimit : autoGasLimit;
            if (gasPrice > 0 && gasLimit > 0)
            {
                TotalFee.Text = string.Format("{0:n6}", (double)(gasPrice * gasLimit) / 1e9);
            }
            else
                TotalFee.Text = "";
        }

        private void EnableSendButton()
        {
            if (!(
                Amount_Addr_ManualGasPrice_ManualGasLimit_Valid.Item1
                && Amount_Addr_ManualGasPrice_ManualGasLimit_Valid.Item2
                && (Amount_Addr_ManualGasPrice_ManualGasLimit_Valid.Item3 || !ManualGasPriceSelectionMode.IsToggled)
                && (Amount_Addr_ManualGasPrice_ManualGasLimit_Valid.Item4 || !ManualGasLimitSelectionMode.IsToggled)
                ))
            {
                SendButton.IsEnabled = false;
                return;
            }

            var gasPrice = ManualGasPriceSelectionMode.IsToggled ? parsedManualGasPrice : autoGasPrice;
            var gasLimit = ManualGasLimitSelectionMode.IsToggled ? parsedManualGasLimit : autoGasLimit;

            var amountPossesed = ethereumWalletService.AddrBalanceByToken[CryptoWalletLibInit.TokenAddrByAbbr[key: tokenAbbr]][accounts[myPicker.SelectedIndex]].Total;
            if (parsedAmount + ((double)(gasLimit * gasPrice) / 1e9) < amountPossesed)
                //if (parsedAmount + double.Parse(TotalFee.Text) < amountPossesed)
                SendButton.IsEnabled = true;
            else
                SendButton.IsEnabled = false;
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
            var gasPrice = ManualGasPriceSelectionMode.IsToggled ? parsedManualGasPrice : autoGasPrice;
            var gasLimit = ManualGasLimitSelectionMode.IsToggled ? parsedManualGasLimit : autoGasLimit;

            var (doneAndPendingTxCount, doneTxCount) = await ethTransferService.CheckPendingTxAsync(sourceAddress);

            int? nonce = null;
            if (doneAndPendingTxCount > doneTxCount)
            {
                string action = await DisplayActionSheet("Pending txs detected," +
                    " would you like to send this tx  on top of them " +
                    "(in this case this newly sent tx also will be pending  till previos txs will be included to blocks)" +
                    "or send by canceling theese pending txs? ", "Cancel", null, "on top of pending txs", "cancel pending txs and send");
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
            var txdId = await ethTransferService.SendToken(sourceAddress, parsedAddress, parsedAmount, gasPrice, (int)gasLimit, nonce, tokenAbbr);
            UpdateAccountsAndBalance();

            activIndic.IsVisible = false;
            activIndic.IsRunning = false;
            activIndicText.IsVisible = false;
            TransacSuccessStatus.IsVisible = true;
            ClearInputFields();

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
        }

        private void ClearInputFields()
        {
            Amount.Text = "";
            Address.Text = "";
            ManualGasPrice.Text = "";
            myPicker.SelectedIndex = 0;
            ManualGasPriceSelectionMode.IsToggled = false;
        }

        private void UpdateAccountsAndBalance()
        {
            if (ethereumWalletService.AddrBalanceByToken != null) ethereumWalletService.AddrBalanceByToken?.TryGetValue(CryptoWalletLibInit.TokenAddrByAbbr[key: tokenAbbr], out addrBalanceByToken);

            if (addrBalanceByToken != null)
            {
                foreach (var addrBalance in addrBalanceByToken)
                {
                    if (addrBalance.Value.Total != 0)
                    {
                        accountAndBalance.Add(addrBalance.Key.ToString() + " (Balance Confirmed: " + addrBalance.Value.Confirmed + ", Total: " + addrBalance.Value.Total + ")");
                        accounts.Add(addrBalance.Key.ToString());
                    }
                }
            }
        }

        public async void Back_Clicked(object _, EventArgs e) => await Navigation.PopAsync(false);

    }
}