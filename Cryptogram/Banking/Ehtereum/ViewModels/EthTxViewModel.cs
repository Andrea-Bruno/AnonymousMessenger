using Banking.Ehtereum.Models;
using Banking.Ehtereum.Services;
using Banking.Ehtereum.Views;
using Banking.Models;
using Banking.Services;
using MvvmHelpers;
using MvvmHelpers.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banking.Ehtereum.ViewModels
{
    public class EthTxViewModel : ViewModelBasecs
    {
        public ObservableRangeCollection<EthTransaction> Transactions { get; set; }
        public EthBalance Balance { get; set; }
        public string ShareAddress { get; set; }
        public bool IsDataLoading { get; set; }
        public bool isDataEmpty { get; set; }
        public AsyncCommand RefreshCommand { get; }
        private EthereumWalletService EthereumWalletService;

        public EthTxViewModel()
        {
            Transactions = new ObservableRangeCollection<EthTransaction>();
            Balance = new EthBalance() { Confirmed = .0, Total = .0 };
            EthereumWalletService = EthereumWalletService.Instance;
            RefreshCommand = new AsyncCommand(Refresh);

            EthereumWalletService.GetLastUsedAddressIdxFromStorage();
            EthereumWalletService.GetNewAddress();

            Task.Run(async () => await LoadTransactionsData());
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
            var etherTxs = EthereumWalletService.EthTransactionFromStorage.Where(tx =>tx.ContractTo.Length == 0).ToList();
            foreach (var txFromStorage in etherTxs)
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

        private void AddTransaction(EthTransaction ethTransaction) => Transactions.Add(ethTransaction);
        private void AddTransactions(List<EthTransaction> ethTransactions) => Transactions.AddRange(ethTransactions);
        private void RemoveTransaction(EthTransactionForStoring ethTransactionForStoring)
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
            Balance.Confirmed = EthereumWalletService.TotalConfirmedBalance;
            Balance.Total = EthereumWalletService.TotalBalance;
        }
        public void UpdateShareAddress()
        {
            ShareAddress = EthereumWalletService.ShareAddress;
            OnPropertyChanged(nameof(ShareAddress));
        }
    }

    public static class EthTxViewModelLocator
    {
        public static EthTxViewModel EthTxViewModel { get; } = new EthTxViewModel();
    }
}
