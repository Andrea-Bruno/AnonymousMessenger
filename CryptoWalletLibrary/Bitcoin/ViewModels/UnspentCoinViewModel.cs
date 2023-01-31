using CryptoWalletLibrary.Bitcoin.Services;
using MvvmHelpers;
using MvvmHelpers.Commands;
using NBitcoin;
using System.Linq;
using System.Threading.Tasks;
using Command = MvvmHelpers.Commands.Command;

namespace CryptoWalletLibrary.Models
{
    public class UnspentCoinViewModel : ViewModelBasecs
    {
        public ObservableRangeCollection<UnspentCoin> UnspentCoins { get; set; }
        public ObservableRangeCollection<object> SelectedUnspentCoins { get; set; }
        public ObservableRangeCollection<Grouping<string, UnspentCoin>> UnspentCoinGroups { get; }
        public AsyncCommand RefreshCommand { get; }
        public AsyncCommand<UnspentCoin> SelectedCommand { get; }
        public Command LoadMoreCommand { get; }
        public Command DelayLoadMoreCommand { get; }
        public Command ClearCommand { get; }
        public bool IsDataLoading { get; set; }
        public bool IfListEmpty { get; set; }

        private readonly BtcCommonService btcCommonService;
        private readonly BtcTransferService btcTransferService;


        public UnspentCoinViewModel()
        {
            btcCommonService = BtcCommonService.Instance;
            btcTransferService = BtcTransferService.Instance;

            Title = "UTXOs";
            UnspentCoins = new ObservableRangeCollection<UnspentCoin>();
            UnspentCoinGroups = new ObservableRangeCollection<Grouping<string, UnspentCoin>>();

            IfListEmpty = false;
            IsDataLoading = true;

            RefreshCommand = new AsyncCommand(Refresh);
            ClearCommand = new Command(Clear);
            DelayLoadMoreCommand = new Command(DelayLoadMore);


            LoadCoins();
            //Task.Run(() => LoadCoins());
            //finding pre-selected utxos
            SelectedUnspentCoins = new ObservableRangeCollection<object>();
            btcTransferService.SelectedUnspentCoins?.ForEach(selectedUtxo => SelectedUnspentCoins.Add(UnspentCoins.FirstOrDefault(x => x.TransactionId == selectedUtxo.TransactionId)));

            //bitcoinWalletService.SelectedUnspentCoins?.ForEach(selectedUtxo => SelectedUnspentCoins.Add(UnspentCoins.FirstOrDefault(x => x == selectedUtxo)));


        }

        async Task Refresh()
        {
            IsBusy = true;
            UnspentCoins.Clear();
            LoadCoins();
            IsBusy = false;
        }

        void LoadCoins()
        {
            IsDataLoading = true;
            UnspentCoins.Clear();
            GetCoins();
            //await Task.Run(() => GetCoins());
            IsDataLoading = false;
            IfListEmpty = UnspentCoins.Count == 0;
            OnPropertyChanged(nameof(IsDataLoading));
            OnPropertyChanged(nameof(IfListEmpty));
        }


        private void GetCoins()
        {
            var utxosPerAddress = btcCommonService.UtxosPerAddress;
            var utxos = utxosPerAddress.SelectMany(d => d.Value).ToList();
            foreach (var utxoDetailsElectrum in utxos)
            {
                Transaction transaction = Transaction.Parse(utxoDetailsElectrum.TransactionHex, BtcCommonService.BitcoinNetwork);
                Coin coin = new Coin(transaction, (uint)utxoDetailsElectrum.TransactionPos);
                var unspentCoin = new UnspentCoin { Amount = coin.Amount.ToDecimal(MoneyUnit.BTC).ToString("0.#######################"), Confirmed = utxoDetailsElectrum.Confirmed, Address = utxoDetailsElectrum.Address, TransactionId = coin.Outpoint.Hash.ToString() };
                UnspentCoins.Add(unspentCoin);
            }
        }

        private void DelayLoadMore()
        {
            if (UnspentCoins.Count <= 10) return;
            GetCoins();
        }

        private void Clear()
        {
            UnspentCoins.Clear();
            UnspentCoinGroups.Clear();
        }
    }
}