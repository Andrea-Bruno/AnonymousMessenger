using CryptoWalletLibrary.Ehtereum.Models;
using Rg.Plugins.Popup.Services;
using System;
using Xamarin.Forms.Xaml;

namespace CryptoWalletUI.Ehtereum.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ERC20TxDetailsPopup : Rg.Plugins.Popup.Pages.PopupPage
    {
        public ERC20TxDetailsPopup(EthTransaction ethTransaction)
        {
            InitializeComponent();
            Hash.Text = ethTransaction.TxHash.Substring(2, ethTransaction.TxHash.Length - 2);
            From.Text = ethTransaction.TxFrom;
            To.Text = ethTransaction.ContractTo;
            ContractAddress.Text = ethTransaction.TxTo;
            Amount.Text = ethTransaction.ContractValue;
            TokenName.Text = ethTransaction.TokenName;
            Time.Text = ethTransaction.Time.ToString();
            Status.Text = ethTransaction.Status.ToString();
            ConfirmationStatus.Text = ethTransaction.ConfirmationStatus.ToString();
            Gas.Text = ethTransaction.Gas.ToString() ?? "NOT USED YET";
            GasPrice.Text = (ethTransaction.GasPrice / Math.Pow(10, 9)).ToString();
            Fee.Text = string.Format("{0:n6}", ethTransaction.GasPrice / Math.Pow(10, 18) * ethTransaction.Gas);
        }
        private async void Back_Clicked(object sender, EventArgs e) => await PopupNavigation.Instance.PopAsync(false);

    }
}