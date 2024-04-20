using Banking.Services;
using MvvmHelpers;
using MvvmHelpers.Commands;
using NBitcoin;
using System;
using Command = MvvmHelpers.Commands.Command;

namespace Banking.Models
{
    public class BitcoinTxDetailsViewModel : ViewModelBasecs
    {
        public ObservableRangeCollection<VinOut> VinVouts { get; set; }
        public ObservableRangeCollection<VinOut> Vins { get; set; }
        public ObservableRangeCollection<VinOut> Vouts { get; set; }

        public ObservableRangeCollection<Grouping<string, VinOut>> CoffeeGroups { get; }
        public string TransactionId { get; set; }
        public string ConfirmedBalance { get; set; }
        public AsyncCommand RefreshCommand { get; }
        public AsyncCommand<BitcoinTransaction> FavoriteCommand { get; }
        public AsyncCommand<BitcoinTransaction> SelectedCommand { get; }
        public Command LoadMoreCommand { get; }
        public Command DelayLoadMoreCommand { get; }
        public Command ClearCommand { get; }
        private readonly BitcoinWalletService bitcoinWalletService;


        public BitcoinTxDetailsViewModel()
        {
            bitcoinWalletService = BitcoinWalletService.Instance;

            Title = "Transaction Details";
            VinVouts = new ObservableRangeCollection<VinOut>();
            Vins = new ObservableRangeCollection<VinOut>();
            Vouts = new ObservableRangeCollection<VinOut>();
            LoadMoreCommand = new Command(getTransactionDetails);
            ClearCommand = new Command(Clear);
            DelayLoadMoreCommand = new Command(getTransactionDetails);

            getTransactionDetails();
        }



        public void getTransactionDetails()
        {
            var selectedTransaction = bitcoinWalletService.SelectedBitcoinTransaction;
            var selectedTrFromStorage = bitcoinWalletService.TransactionsForStore.Find(x => x.TransactionId == selectedTransaction.TransactionId);
            var selectedTrNBitcoin = Transaction.Parse(selectedTrFromStorage.TransactionHex, bitcoinWalletService.BitcoinNetwork);
            foreach (var input in selectedTrFromStorage.Inputs)
            {
                Vins.Add(new VinOut
                {
                    Adress = input.Address.ToString(),
                    Amount = input.Amount.ToString(),
                    IsVin = true
                });
            }
            foreach (var output in selectedTrNBitcoin.Outputs)
            {

                Vouts.Add(new VinOut
                {
                    Adress = output.ScriptPubKey.GetDestinationAddress(bitcoinWalletService.BitcoinNetwork).ToString(),
                    Amount = output.Value.ToDecimal(MoneyUnit.BTC).ToString("0.#######################"),
                    IsVin = false
                });
            }
            //OnPropertyChanged(nameof(getTransactionDetails));
            //OnPropertyChanged(nameof(TransactionId));
        }

        void Clear() => VinVouts.Clear();
    }
}