using Banking.Ehtereum.Models;
using Banking.Ehtereum.Services;
using MvvmHelpers.Commands;
using MvvmHelpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Banking.Models;
using System.Linq;

namespace Banking.Ehtereum.ViewModels
{
    public class ERC20ViewModel : ViewModelBasecs
    {
        public ObservableRangeCollection<EthTransaction> Transactions { get; set; }
        public EthBalance Balance { get; set; }
        public string ShareAddress { get; set; }
        public bool IsDataLoading { get; set; }
        public bool isDataEmpty { get; set; }
        public AsyncCommand RefreshCommand { get; }
        private readonly EthereumWalletService EthereumWalletService;
        public string TokenName { get; set; }



        public ERC20ViewModel(string abbr)
        {
            TokenName = abbr;
            Transactions = new ObservableRangeCollection<EthTransaction>();
            Balance = new EthBalance() { Confirmed = .0, Total = .0 };
            EthereumWalletService = EthereumWalletService.Instance;
            RefreshCommand = new AsyncCommand(Refresh);

            EthereumWalletService.GetLastUsedAddressIdxFromStorage();
            EthereumWalletService.GetNewAddress();

            //Task.Run(async () => await LoadTransactionsData());
            EthereumWalletService.GetTransactionsFromStorage();
            FormatTransactions();
            UpdateBalance();
            UpdateShareAddress();
        }

        public async Task LoadTransactionsData()
        {
            IsDataLoading = true;
            OnPropertyChanged(nameof(IsDataLoading));

            await Task.Run(() => GetTransactions());

            IsDataLoading = false;
            isDataEmpty = Transactions.Count == 0;
            OnPropertyChanged(nameof(IsDataLoading));
            OnPropertyChanged(nameof(IsDataLoading));
        }

        public async Task GetTransactions()
        {
            EthereumWalletService.GetTransactionsFromStorage();
            FormatTransactions();
            UpdateBalance();
            UpdateShareAddress();

            await EthereumWalletService.GetTransactionsAsync(AddTransaction, RemoveTransaction);
            UpdateBalance();
            UpdateShareAddress();
        }

        public void FormatTransactions()
        {
            var formatedTxs = new List<EthTransaction>();
            var erc20Txs = EthereumWalletService.EthTransactionFromStorage.
                Where(txFromStorage => txFromStorage.TxTo.Equals(CryptoWallet.TokenAddrByAbbr[TokenName], StringComparison.OrdinalIgnoreCase)).ToList();

            foreach (var txFromStorage in erc20Txs)
            {
                var ethTransaction = EthereumWalletService.StorageTxToTx(txFromStorage);
                formatedTxs.Add(ethTransaction);
            }
            AddTransactions(formatedTxs);
        }

        private async Task Refresh()
        {
            IsBusy = true;
            Transactions.Clear();
            await Task.Run(() => GetTransactions());
            IsBusy = false;
            isDataEmpty = Transactions.Count == 0;
            OnPropertyChanged(nameof(isDataEmpty));
        }

        public void AddTransaction(EthTransaction ethTransaction) => Transactions.Add(ethTransaction);
        private void AddTransactions(List<EthTransaction> ethTransactions) => Transactions.AddRange(ethTransactions);
        public void RemoveTransaction(EthTransactionForStoring ethTransactionForStoring)
        {
            foreach (var tx in Transactions)
            {
                if (tx.TxHash.Equals(ethTransactionForStoring.TxHash))
                {
                    Transactions.Remove(tx);
                    break;
                }
            }
        }

        private void UpdateBalance()
        {
            Balance.Confirmed = EthereumWalletService.TotalConfirmedBalanceByToken.TryGetValue(TokenName, out var confirmedBalance) ? confirmedBalance : 0;
            Balance.Total = EthereumWalletService.TotalBalanceByToken.TryGetValue(TokenName, out var totalBalance) ? totalBalance : 0;
        }
        public void UpdateShareAddress()
        {
            ShareAddress = EthereumWalletService.ShareAddress;
            OnPropertyChanged(nameof(ShareAddress));
        }
    }
}

