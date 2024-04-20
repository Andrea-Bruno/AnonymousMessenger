using Banking.Ehtereum.Services;
using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Banking.Ehtereum.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SendERC20Page : ContentPage
    {
        private readonly EthereumWalletService ethereumWalletService;
        private readonly List<string> accounts;
        private readonly List<string> accountAndBalance;
        private readonly string tokenAbbr;
        public SendERC20Page(string tokenAbbr)
        {
            ethereumWalletService = EthereumWalletService.Instance;
            this.tokenAbbr = tokenAbbr;
            accountAndBalance = new List<string>();
            accounts = new List<string>();
            InitializeComponent();
            ethereumWalletService.AddrBalanceByToken.TryGetValue(tokenAbbr, out var addrBalanceByToken);
            if (addrBalanceByToken != null)
            {
                foreach (var addrBalance in addrBalanceByToken)
                {
                    accountAndBalance.Add(addrBalance.Key.ToString() + " (Balance Confirmed: " + addrBalance.Value.Confirmed + ", Total: " + addrBalance.Value.Total + ")");
                    accounts.Add(addrBalance.Key.ToString());
                }
            }

            var pickerSource = new Picker { Title = "Accounts", TitleColor = Color.Red };
            myPicker.ItemsSource = accountAndBalance;
            myPicker.SelectedIndexChanged += Account_SelectedIndexChanged;
            GasLimit.Text = "21000";
            GasPrice.Text = string.Format("{0:n5}", ((double)(ethereumWalletService.Fee.BaseFee + ethereumWalletService.Fee.MaxPriorityFeePerGas) / 1e9));
        }
        private void Account_SelectedIndexChanged(object sender, EventArgs e) => EnableSendButton();

        private void Amount_TextChanged(object sender, TextChangedEventArgs e) => EnableSendButton();
        private void GasPrice_TextChanged(object sender, TextChangedEventArgs e)
        {

            if (double.TryParse(GasPrice.Text, out var parsedAGasPrice) && double.TryParse(GasLimit.Text, out var parsedGasLimit))
                Fee.Text = string.Format("{0:n6}", parsedAGasPrice * parsedGasLimit / 1e9);
            EnableSendButton();
        }

        private void GasLimit_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (double.TryParse(GasPrice.Text, out var parsedAGasPrice) && double.TryParse(GasLimit.Text, out var parsedGasLimit))
                Fee.Text = (parsedAGasPrice * parsedGasLimit).ToString();
            EnableSendButton();
        }

        private void Address_TextChanged(object sender, TextChangedEventArgs e) => EnableSendButton();

        private void EnableSendButton()
        {
            var isEnabled = false;
            if (double.TryParse(Amount.Text, out var parsedAmount) && double.TryParse(Fee.Text, out var parsedFee) && (Address.Text != null && Address.Text != ""))
            {
                ethereumWalletService.AddrBalanceByToken.TryGetValue(tokenAbbr, out var addrBalancesByToken);
                var amountPossesed = addrBalancesByToken != null ? addrBalancesByToken [accounts[myPicker.SelectedIndex]].Total : 0;
                if (parsedAmount < amountPossesed && parsedFee < ethereumWalletService.BalanceByAddress[accounts[myPicker.SelectedIndex]].Total && parsedAmount > 0 && parsedFee >= 0) isEnabled = true;
            }
            SendButton.IsEnabled = isEnabled;
        }

        public async void Confirm_Clicked(object _, EventArgs e)
        {
            succTransac.IsVisible = false;
            var destionationAddress = Address.Text;
            var amount = Amount.Text;
            var gasPrice = GasPrice.Text;
            var gasLimit = GasLimit.Text;

            if (double.TryParse(gasLimit, out var parsedFee) && int.TryParse(amount, out var parsedAmount) && double.TryParse(gasPrice, out var parsedGasPrice) && int.TryParse(gasLimit, out var parsedGasLimit))
            {
                activIndic.IsVisible = true;
                activIndic.IsRunning = true;
                activIndicText.IsVisible = true;

                var sourceAddress = accounts[myPicker.SelectedIndex];
                transacId.Text = await ethereumWalletService.SendToken(sourceAddress, destionationAddress, parsedAmount, (int)Math.Floor(parsedGasPrice), parsedGasLimit, tokenAbbr);

                activIndic.IsVisible = false;
                activIndic.IsRunning = false;
                activIndicText.IsVisible = false;
                succTransac.IsVisible = true;

                //transacId.Text = bitcoinWalletService.BroadcastedTransactionId.ToString();
                //if (bitcoinWalletService.TrBuildFailed)
                //    succTransac.Text = "There might be errors broadcasting your transaction";
            }
        }
    }
}