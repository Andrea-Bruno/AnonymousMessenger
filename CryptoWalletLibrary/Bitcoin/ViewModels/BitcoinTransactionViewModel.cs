using CryptoWalletLibrary.Bitcoin.Services;
using MvvmHelpers;
using MvvmHelpers.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Command = MvvmHelpers.Commands.Command;

namespace CryptoWalletLibrary.Models
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
        public ObservableRangeCollection<BitcoinTransaction> FormatedTransactions { get; set; }
        public string ConfirmedBalance { get; set; }
        public AsyncCommand RefreshCommand { get; }
        public AsyncCommand<BitcoinTransaction> FavoriteCommand { get; }
        public AsyncCommand<BitcoinTransaction> SelectedCommand { get; }
        public Command LoadMoreCommand { get; }
        public Command DelayLoadMoreCommand { get; }
        public Command ClearCommand { get; }

        private readonly BtcCommonService btcCommonService;
        private readonly BtcTransactionService btcTransactionService;

        public BitcoinTransactionViewModel()
        {
            btcCommonService = BtcCommonService.Instance;
            btcTransactionService = BtcTransactionService.Instance;
            Balance = new BitcoinBalance()
            {
                Total = 0,
                Confirmed = 0,
                Unconfirmed = 0
            };

            IsDataLoading = false;
            IfListEmpty = false;
            IsWalletRecovering = false;
            IsWeAreQueryingBChain = false;

            Transactions = new ObservableRangeCollection<BitcoinTransaction>();
            FormatedTransactions = new ObservableRangeCollection<BitcoinTransaction>();
            ConfirmedBalance = "";

            RefreshCommand = new AsyncCommand(Refresh);
            ClearCommand = new Command(Clear);

            AssignBalance();

            SortTxsFromStorage();

            Task.Run(async () => await GetTransactionsAsync()).
                ContinueWith(task => SortTxsFromNetwork(), TaskScheduler.FromCurrentSynchronizationContext());
        }


        private async Task GetTransactionsAsync()
        {
            IsWalletRecovering = false;
            IsDataLoading = true;
            IsWeAreQueryingBChain = true;
            OnPropertyChanged(nameof(IsWalletRecovering));
            OnPropertyChanged(nameof(IsDataLoading));
            OnPropertyChanged(nameof(IsWeAreQueryingBChain));

            await Task.Run(async () =>
            {
                await btcTransactionService.GetTransactionsAsync();
            });


            AssignBalance();

            IsDataLoading = false;
            IsWeAreQueryingBChain = false;
            IfListEmpty = Transactions.Count == 0;
            OnPropertyChanged(nameof(IsDataLoading));
            OnPropertyChanged(nameof(IsWeAreQueryingBChain));
            OnPropertyChanged(nameof(IfListEmpty));
        }

        public async Task Refresh()
        {
            IsBusy = true;

            await Task.Run(async () =>
            {
                await btcTransactionService.GetTransactionsAsync();
            });

            AssignBalance();

            IsBusy = false;

            IfListEmpty = Transactions.Count == 0;
            OnPropertyChanged(nameof(IfListEmpty));
        }

        private List<BitcoinTransaction> FormatTransactionsFromStorage()
        {
            var formatedTxs = new List<BitcoinTransaction>();
            foreach (var txFromStorage in btcCommonService.TransactionsForStore)
            {
                var bitcoinTransaction = BtcConverter.BtcTxForStoringToBtcTx(txFromStorage, txFromStorage.Address);
                formatedTxs.Add(bitcoinTransaction);
            }
            return formatedTxs;
        }

        // Methods to Modify Transactions
        public void AddTransaction(BitcoinTransaction bitcoinTransaction) => Transactions.Insert(0, bitcoinTransaction);
        public void AddTransactions(List<BitcoinTransaction> bitcoinTransaction) => Transactions.AddRange(bitcoinTransaction);
        public void RemoveTransaction(BitcoinTransactionForStoring bitcoinTransactionForStoring)
        {
            foreach (var tx in Transactions)
            {
                if (tx.TransactionId.Equals(bitcoinTransactionForStoring.TransactionId))
                {
                    Transactions.Remove(tx);
                    break;
                }
            }
        }
        public void ConfirmTransactionStatus(string TxId)
        {
            var transaction = Transactions.FirstOrDefault(tx => tx.TransactionId == TxId);
            if (transaction != null) transaction.Confirmed = true;

            var idx = Transactions.IndexOf(transaction);
            Transactions.Remove(transaction);
            Transactions.Insert(idx, transaction);
        }
        public void UpdateTrasnsactionDate(string TxId, DateTime newDate)
        {
            var transaction = Transactions.FirstOrDefault(tx => tx.TransactionId == TxId);
            if (transaction != null) transaction.Date = newDate.ToString();

            var idx = Transactions.IndexOf(transaction);
            Transactions.Remove(transaction);
            Transactions.Insert(idx, transaction);
        }

        public void UpdateTransaction(string TxId, BitcoinTransaction bitcoinTransaction)
        {
            var transaction = Transactions.FirstOrDefault(tx => tx.TransactionId == TxId);
            var idx = Transactions.IndexOf(transaction);
            Transactions.Remove(transaction);
            Transactions.Insert(idx, bitcoinTransaction);
            //transaction.Amount = bitcoinTransaction.Amount;
            //transaction.Address  = bitcoinTransaction.Address;
            //transaction.Sent = bitcoinTransaction.Sent;
        }




        public void AssignBalance()
        {
            Balance.Confirmed = btcCommonService.TotalConfirmedBalance;
            Balance.Confirmed = btcCommonService.TotalUncnfirmedBalance;
            Balance.Total = (btcCommonService.TotalConfirmedBalance + btcCommonService.TotalUncnfirmedBalance);
        }

        void Clear() => Transactions.Clear();

        private void SortTxsFromStorage()
        {
            var formatedTxs = FormatTransactionsFromStorage();
            AddTransactions(formatedTxs.Where(fTx => fTx.Date == "N/A").ToList());
            AddTransactions(formatedTxs.Where(fTx => DateTime.TryParse(fTx.Date, out var dTime)).OrderByDescending(tx => DateTime.Parse(tx.Date)).ToList());

        }
        private void SortTxsFromNetwork()
        {
            var tmpTransactions = Transactions.ToList();
            Transactions.Clear();
            AddTransactions(tmpTransactions.Where(fTx => fTx.Date == "N/A").ToList());
            AddTransactions(tmpTransactions.Where(fTx => DateTime.TryParse(fTx.Date, out var dTime)).OrderByDescending(tx => DateTime.Parse(tx.Date)).ToList());
        }

        public static class BtcTxViewModelLocator
        {
            public static BitcoinTransactionViewModel Instance { get; } = new BitcoinTransactionViewModel();
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