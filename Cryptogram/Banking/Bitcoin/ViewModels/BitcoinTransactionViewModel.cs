using Banking.Services;
using MvvmHelpers;
using MvvmHelpers.Commands;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Command = MvvmHelpers.Commands.Command;

namespace Banking.Models
{
    public class BitcoinTransactionViewModel : ViewModelBasecs
    {
        public ObservableRangeCollection<BitcoinTransaction> Transactions { get; set; }
        public BitcoinBalance Balance { get; set; }
        public string BalanceUnconfirmed { get; set; }
        public bool IsDataLoading { get; set; }
        public bool IfListEmpty { get; set; }
        public bool IsWalletRecovering { get; set; }
        public bool IsWeAreQueryingBChain { get; set; }
        public ObservableRangeCollection<Grouping<string, BitcoinTransaction>> TransactionGroups { get; }
        public ObservableRangeCollection<BitcoinTransaction> FormatedTransactions { get; set; }
        public string ConfirmedBalance { get; set; }
        public AsyncCommand RefreshCommand { get; }
        public AsyncCommand<BitcoinTransaction> FavoriteCommand { get; }
        public AsyncCommand<BitcoinTransaction> SelectedCommand { get; }
        public Command LoadMoreCommand { get; }
        public Command DelayLoadMoreCommand { get; }
        public Command ClearCommand { get; }

        private BitcoinWalletService bitcoinWalletService;


        public BitcoinTransactionViewModel()
        {
            bitcoinWalletService = BitcoinWalletService.Instance;

            Balance = new BitcoinBalance()
            {
                Total = 0,
                Confirmed = 0,
                Unconfirmed = 0
            };

            Title = "All Transactions";
            IsDataLoading = false;
            IfListEmpty = false;
            IsWalletRecovering = false;
            IsWeAreQueryingBChain = false;


            Transactions = new ObservableRangeCollection<BitcoinTransaction>();
            FormatedTransactions = new ObservableRangeCollection<BitcoinTransaction>();
            TransactionGroups = new ObservableRangeCollection<Grouping<string, BitcoinTransaction>>();
            ConfirmedBalance = "";

            RefreshCommand = new AsyncCommand(Refresh);
            ClearCommand = new Command(Clear);


            //BitcoinWalletService.GetInstance("L4VpjddASNaAKqfD7A3giyFcBoa7NrZizX1RNFskwwVcovpEojkZ");
            //BitcoinWalletService.Context.SecureStorage.ObjectStorage.DeleteObject(typeof(List<BitcoinTransactionForStoring>), "BitcoinTransactions");
            //var isInitcall = BitcoinWalletService.Context.SecureStorage.ObjectStorage.LoadObject(typeof(List<BitcoinTransactionForStoring>), "BitcoinTransactions") == null;
            Task.Run(async () => await LoadTransactionsData());
        }
        

        async Task Refresh()
        {
            IsBusy = true;
            Transactions.Clear();
            await Task.Run(() => getTransactions());
            IsBusy = false;
            IfListEmpty = Transactions.Count == 0;
            OnPropertyChanged(nameof(IfListEmpty));
        }

        public async Task LoadTransactionsData()
        {
            IsWalletRecovering = false;
            IsDataLoading = true;
            IsWeAreQueryingBChain = true;
            OnPropertyChanged(nameof(IsWalletRecovering));
            OnPropertyChanged(nameof(IsDataLoading));
            OnPropertyChanged(nameof(IsWeAreQueryingBChain));


            await Task.Run(() => getTransactions());

            IsDataLoading = false;
            IsWeAreQueryingBChain = false;
            IfListEmpty = Transactions.Count == 0;
            OnPropertyChanged(nameof(IsDataLoading));
            OnPropertyChanged(nameof(IsWeAreQueryingBChain));
            OnPropertyChanged(nameof(IfListEmpty));
        }

        public async Task getTransactions()
        {
            bitcoinWalletService.getTransactionsFromStorage();
            bitcoinWalletService.getBalanceFromStorage();
            UpdateBalance();
            FormatTransactions();

            await bitcoinWalletService.GetTransactionsMainAddrElectrumxAsync(AddTransaction, RemoveTransaction, Balance);
            await bitcoinWalletService.GetTransactionsChangeAddrElectrumxAsync(AddTransaction, RemoveTransaction, Balance);
            UpdateBalance();

        }

        public void FormatTransactions()
        {
            var formatedTxs = new List<BitcoinTransaction>();
            foreach (var txFromStorage in bitcoinWalletService.TransactionsForStore)
            {
                var bitcoinTransaction = bitcoinWalletService.FormatBitcoinTrasaction(txFromStorage, txFromStorage.Address);
                formatedTxs.Add(bitcoinTransaction);
            }
            AddTransactions(formatedTxs);
        }



        private void AddTransaction(BitcoinTransaction bitcoinTransaction) => Transactions.Add(bitcoinTransaction);
        private void AddTransactions(List<BitcoinTransaction> bitcoinTransaction) => Transactions.AddRange(bitcoinTransaction);
        private void RemoveTransaction(BitcoinTransactionForStoring bitcoinTransactionForStoring)
        {
            foreach (var tx in Transactions)
            {
                if (tx.TransactionId.Equals(bitcoinTransactionForStoring.TransactionId))
                    Transactions.Remove(tx);
            }
        }

        private void UpdateBalance()
        {
            Balance.Confirmed = bitcoinWalletService.TotalConfirmedBalance;
            Balance.Confirmed = bitcoinWalletService.TotalUncnfirmedBalance;
            Balance.Total = (bitcoinWalletService.TotalConfirmedBalance + bitcoinWalletService.TotalUncnfirmedBalance);
        }

        void Clear()
        {
            Transactions.Clear();
            TransactionGroups.Clear();
        }




        //void sortTransactionsThenToString()
        //{
        //    var txHistoryRecords = new List<Tuple<DateTimeOffset, Money, int, string, bool, List<BitcoinAddress>, List<BitcoinAddress>, Tuple<string>>>();
        //    BitcoinWalletService.transactionsNBitcoin = new List<Transaction>();
        //    foreach (var addrTransacPair in BitcoinWalletService.transactionsPerAddress)
        //    {
        //        Money transactionAmount = Money.Zero;
        //        foreach (var transaction in addrTransacPair.Value)
        //        {
        //            Transaction transactionNBitcoin = Transaction.Parse(transaction.Hex, BitcoinWalletService.BitcoinNetwork);
        //            if (BitcoinWalletService.transactionsNBitcoin.Where(x => x.GetHash() == transactionNBitcoin.GetHash()).ToList().Count == 0)
        //                BitcoinWalletService.transactionsNBitcoin.Add(transactionNBitcoin);
        //            var Outputs = new List<BitcoinAddress>();
        //            var Inputs = new List<BitcoinAddress>();
        //            {
        //                var outputs = transactionNBitcoin.Outputs;
        //                foreach (TxOut output in outputs)
        //                {
        //                    var paymentScript = output.ScriptPubKey;
        //                    var receiverAddress = paymentScript.GetDestinationAddress(BitcoinWalletService.BitcoinNetwork);
        //                    Outputs.Add(receiverAddress);

        //                    var outputAdress = output.ScriptPubKey.GetDestinationAddress(BitcoinWalletService.BitcoinNetwork).ToString();
        //                    if (outputAdress == addrTransacPair.Key)
        //                    {
        //                        transactionAmount = output.Value;
        //                    }
        //                }

        //                var inputs = transactionNBitcoin.Inputs;
        //                foreach (TxIn input in inputs)
        //                {
        //                    var senderAddress = input.GetSigner().ScriptPubKey.GetDestinationAddress(BitcoinWalletService.BitcoinNetwork);
        //                    Inputs.Add(senderAddress);
        //                }
        //            }

        //            //var sent = Inputs.First() == BitcoinWalletService.bitcoinAdresses.First().ToString();
        //            var sent = Inputs.Contains(addrTransacPair.Key);

        //            txHistoryRecords.
        //                //.Add(new Tuple<DateTimeOffset, Money, int, string, bool, List<String>, List<String>, string>(
        //                Add(Tuple.Create(
        //                    DateTimeOffset.FromUnixTimeSeconds(transaction.Time),
        //                    transactionAmount,
        //                    (int)transaction.Confirmations,
        //                    addrTransacPair.Key,
        //                    sent,
        //                    Outputs,
        //                    Inputs,
        //                    transactionNBitcoin.GetHash().ToString()));
        //        }
        //    }

        //    // 4. Order the records by confirmations and time (Simply time does not work, because of a QBitNinja bug)
        //    var orderedTxHistoryRecords = txHistoryRecords
        //        .OrderByDescending(x => x.Item3) // Confirmations
        //        .ThenBy(x => x.Item1); // FirstSeen
        //    Debug.WriteLine("Bitcoin Transactions");

        //    foreach (var record in txHistoryRecords)
        //    {
        //        string FromOrToAdress;
        //        if (record.Item5)
        //        {
        //            var outputs = record.Item6;

        //            foreach (var addr in mainAndChangeAddresses)
        //            {
        //                if (outputs.Count > 0) outputs.Remove(addr);
        //            }
        //            //in Future Dislay all address to which user sent btc.
        //            if (outputs.Count != 0)
        //            {
        //                FromOrToAdress = outputs.First();
        //            }
        //            else
        //            {
        //                FromOrToAdress = "Uknown";
        //            }
        //        }
        //        else
        //        {
        //            FromOrToAdress = record.Item7.First();
        //        }

        //        FormatedTransactions.Add(new BitcoinTransaction { TransactionId = $"{record.Rest.Item1}", Date = $"{record.Item1.DateTime}", Amount = $"{record.Item2}" + " BTC", Sent = record.Item5, Address = FromOrToAdress });
        //        Debug.WriteLine($"{record.Item1.DateTime}\t{record.Item2}\t{record.Item3 > 0}\t\t{record.Item4}");
        //    }
        //}

        //private async Task RecoverWallet()
        //{
        //    IsDataLoading = true;
        //    IsWalletRecovering = true;
        //    OnPropertyChanged(nameof(IsWalletRecovering));
        //    OnPropertyChanged(nameof(IsDataLoading));
        //    Transactions.Clear();

        //    await Task.Run(() => BitcoinWalletService.recoverWallet());
        //    IsDataLoading = false;
        //    IsWalletRecovering = false;

        //    OnPropertyChanged(nameof(IsWalletRecovering));
        //    OnPropertyChanged(nameof(IsDataLoading));
        //}

        //public async Task InitialGetTransactions()
        //{
        //    await BitcoinWalletService.GetTransactionsAsync(Transactions, Balance);
        //    await BitcoinWalletService.GetTransactionsChangeAddrElectrumxAsync(Transactions, Balance);
        //}


        //public event PropertyChangedEventHandler PropertyChanged;
        //protected void NotifyPropertyChanged(String info)
        //{
        //    if (PropertyChanged != null)
        //    {
        //        PropertyChanged(this, new PropertyChangedEventArgs(info));
        //    }
        //}

    }
}