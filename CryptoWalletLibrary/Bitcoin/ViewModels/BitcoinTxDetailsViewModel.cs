using CryptoWalletLibrary.Bitcoin.Services;
using MvvmHelpers;
using MvvmHelpers.Commands;
using NBitcoin;
using Command = MvvmHelpers.Commands.Command;

namespace CryptoWalletLibrary.Models
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
        private readonly BtcCommonService btcCommonService;


        public BitcoinTxDetailsViewModel()
        {
            btcCommonService = BtcCommonService.Instance;

            Title = "Transaction Details";
            VinVouts = new ObservableRangeCollection<VinOut>();
            Vins = new ObservableRangeCollection<VinOut>();
            Vouts = new ObservableRangeCollection<VinOut>();
            LoadMoreCommand = new Command(GetTransactionDetails);
            ClearCommand = new Command(Clear);
            DelayLoadMoreCommand = new Command(GetTransactionDetails);

            GetTransactionDetails();
        }



        public void GetTransactionDetails()
        {
            var selectedTransaction = btcCommonService.SelectedBitcoinTransaction;
            var selectedTrFromStorage = btcCommonService.TransactionsForStore.Find(x => x.TransactionId == selectedTransaction.TransactionId);
            var selectedTrNBitcoin = Transaction.Parse(selectedTrFromStorage.TransactionHex, BtcCommonService.BitcoinNetwork);
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
                    Adress = output.ScriptPubKey.GetDestinationAddress(BtcCommonService.BitcoinNetwork).ToString(),
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