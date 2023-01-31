using CryptoWalletLibrary.Ehtereum.Models;
using CryptoWalletLibrary.Ehtereum.Services;
using CryptoWalletLibrary.Models;
using MvvmHelpers;
using MvvmHelpers.Commands;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CryptoWalletLibrary.Ehtereum.ViewModels
{
    public class EthTxViewModel : ViewModelBasecs
    {
        public ObservableRangeCollection<EthTransaction> Transactions { get; set; }
        public EthBalance Balance { get; set; }
        public string ShareAddress { get; set; }
        public bool IsDataLoading { get; set; }
        public bool IsDataEmpty { get; set; }
        public AsyncCommand RefreshCommand { get; }

        private readonly EthTransactionService ethTransactionService;
        private readonly EthStorageService ethStorageService;
        private readonly EthAdressService ethAdressService;
        private readonly EthCommonService ethereumWalletService;

        public EthTxViewModel()
        {
            ethTransactionService = EthTransactionService.Instance;
            ethStorageService = EthStorageService.Instance;
            ethereumWalletService = EthCommonService.Instance;
            ethAdressService = EthAdressService.Instance;

            Transactions = new ObservableRangeCollection<EthTransaction>();
            Balance = new EthBalance() { Confirmed = .0, Total = .0 };

            RefreshCommand = new AsyncCommand(Refresh);

            ethStorageService.GetLastUsedAddressIdxFromStorage();
            ethAdressService.GetNewAddress();


            DisplayTxsFromStorage();
            Task.Run(async () => await GetNewTransactions());
        }

        public void DisplayTxsFromStorage()
        {
            FormatTransactions();
            UpdateBalance();
            UpdateShareAddress();
        }

        private async Task GetNewTransactions()
        {
            IsDataLoading = true;
            OnPropertyChanged(nameof(IsDataLoading));

            await ethTransactionService.GetTransactionsAsync();
            UpdateBalance();
            UpdateShareAddress();

            IsDataLoading = false;
            IsDataEmpty = Transactions.Count == 0;
            OnPropertyChanged(nameof(IsDataLoading));
            OnPropertyChanged(nameof(IsDataLoading));
        }

        public async Task Refresh()
        {
            IsBusy = true;

            await ethTransactionService.GetTransactionsAsync();
            UpdateBalance();
            UpdateShareAddress();

            IsBusy = false;
            IsDataEmpty = Transactions.Count == 0;
            OnPropertyChanged(nameof(IsDataEmpty));
        }

        public void FormatTransactions()
        {
            var formatedTxs = new List<EthTransaction>();
            var etherTxs = ethereumWalletService.TransactionsFromStorage.Where(tx => tx.ContractTo.Length == 0).ToList();
            foreach (var txFromStorage in etherTxs)
            {
                var ethTransaction = EthConverter.StorageTxToTx(txFromStorage);
                formatedTxs.Add(ethTransaction);
            }
            formatedTxs.Reverse();
            AddTransactions(formatedTxs);
        }

        public void AddTransaction(EthTransactionForStoring ethTransactionForStoring)
        {
            var ethTransaction = EthConverter.StorageTxToTx(ethTransactionForStoring);
            Transactions.Insert(0, ethTransaction);
        }
        public void AddTransactions(List<EthTransaction> ethTransactions) => Transactions.AddRange(ethTransactions);

        public void UpdateTransaction(EthTransactionForStoring ethTransactionForStoring)
        {
            var transaction = Transactions.FirstOrDefault(tx => tx.TxHash == ethTransactionForStoring.TxHash);
            EthConverter.StorageTxToTx(ethTransactionForStoring, ref transaction);
        }

        public void RemoveTransaction(string txHash)
        {
            foreach (var tx in Transactions)
            {
                if (tx.TxHash.Equals(txHash))
                {
                    Transactions.Remove(tx);
                    break;
                }
            }
        }

        public void UpdateBalance()
        {
            Balance.Confirmed = ethereumWalletService.TotalConfirmedBalance;
            Balance.Total = ethereumWalletService.TotalBalance;
        }
        public void UpdateShareAddress()
        {
            ShareAddress = ethereumWalletService.ShareAddress;
            OnPropertyChanged(nameof(ShareAddress));
        }
    }

    public static class EthTxViewModelLocator
    {
        public static EthTxViewModel EthTxViewModel { get; } = new EthTxViewModel();
    }
}
