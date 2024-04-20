using Banking.Services;
using MvvmHelpers;
using MvvmHelpers.Commands;
using NBitcoin;
using System.Linq;
using System.Threading.Tasks;
using Command = MvvmHelpers.Commands.Command;

namespace Banking.Models
{
    public class UnspentCoinViewModel : ViewModelBasecs
    {
        public ObservableRangeCollection<UnspentCoin> UnspentCoins { get; set; }
        public ObservableRangeCollection<UnspentCoin> SelectedUnspentCoins { get; set; }
        public ObservableRangeCollection<Grouping<string, UnspentCoin>> UnspentCoinGroups { get; }
        public AsyncCommand RefreshCommand { get; }
        public AsyncCommand<UnspentCoin> SelectedCommand { get; }
        public Command LoadMoreCommand { get; }
        public Command DelayLoadMoreCommand { get; }
        public Command ClearCommand { get; }
        public bool IsDataLoading { get; set; }
        public bool IfListEmpty { get; set; }
        private readonly BitcoinWalletService bitcoinWalletService;


        public UnspentCoinViewModel()
        {
            bitcoinWalletService = BitcoinWalletService.Instance;

            Title = "UTXOs";
            UnspentCoins = new ObservableRangeCollection<UnspentCoin>();
            SelectedUnspentCoins = new ObservableRangeCollection<UnspentCoin>();
            UnspentCoinGroups = new ObservableRangeCollection<Grouping<string, UnspentCoin>>();
            IfListEmpty = false;
            IsDataLoading = true;

            RefreshCommand = new AsyncCommand(Refresh);
            ClearCommand = new Command(Clear);
            DelayLoadMoreCommand = new Command(DelayLoadMore);


            Task.Run(() => LoadCoins());
        }

        async Task Refresh()
        {
            IsBusy = true;
            UnspentCoins.Clear();
            await LoadCoins();
            IsBusy = false;
        }

        async Task LoadCoins()
        {
            IsDataLoading = true;
            UnspentCoins.Clear();
            await Task.Run(() => getCoins());
            IsDataLoading = false;
            IfListEmpty = UnspentCoins.Count == 0;
            OnPropertyChanged(nameof(IsDataLoading));
            OnPropertyChanged(nameof(IfListEmpty));
        }


        private void getCoins()
        {
            var utxosPerAddress = bitcoinWalletService.UtxosPerAddress;
            var utxos = utxosPerAddress.SelectMany(d => d.Value).ToList();
            foreach (var utxoDetailsElectrum in utxos)
            {
                Transaction transaction = Transaction.Parse(utxoDetailsElectrum.TransactionHex, bitcoinWalletService.BitcoinNetwork);
                Coin coin = new Coin(transaction, (uint)utxoDetailsElectrum.TransactionPos);
                UnspentCoins.Add(new UnspentCoin { Amount = coin.Amount.ToDecimal(MoneyUnit.BTC).ToString("0.#######################"), Confirmed = utxoDetailsElectrum.Confirmed, Address = utxoDetailsElectrum.Address, TransactionId = coin.Outpoint.Hash.ToString() });
            }
        }

        private void DelayLoadMore()
        {
            if (UnspentCoins.Count <= 10) return;
            getCoins();
        }

        private void Clear()
        {
            UnspentCoins.Clear();
            UnspentCoinGroups.Clear();
        }
    }
}