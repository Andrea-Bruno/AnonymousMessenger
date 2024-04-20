using Banking.Ehtereum.Models;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using static XamarinShared.MessageReadStatus;

namespace Banking.Ehtereum.Pages
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
            Status.Text = ethTransaction.Status ? "Confirmed" : "Unconfmired";
            Gas.Text = ethTransaction.Gas.ToString();
            GasPrice.Text = (ethTransaction.GasPrice / Math.Pow(10, 9)).ToString();
            Fee.Text = string.Format("{0:n6}", ethTransaction.GasPrice / Math.Pow(10, 18) * ethTransaction.Gas);
        }
        private async void Back_Clicked(object sender, EventArgs e) => await PopupNavigation.Instance.PopAsync(false);

    }
}