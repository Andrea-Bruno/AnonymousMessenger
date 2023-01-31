using CryptoWalletLibrary.Ehtereum.Models;
using CryptoWalletLibrary.Ehtereum.Services;
using CryptoWalletLibrary.Models;
using MvvmHelpers;
using MvvmHelpers.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CryptoWalletLibrary.Ehtereum.ViewModels
{
    public class ERC20ViewModel : ViewModelBasecs
    {
        public ObservableRangeCollection<EthTransaction> Transactions { get; set; }
        public EthBalance Balance { get; set; }
        public string ShareAddress { get; set; }
        public bool IsDataLoading { get; set; }
        public bool IsDataEmpty { get; set; }
        public AsyncCommand RefreshCommand { get; }
        private readonly EthTransactionService ethTransactionService;
        private readonly EthStorageService ethStorageService;
        private readonly EthCommonService ethereumWalletService;

        public string TokenName { get; set; }


        public ERC20ViewModel(string abbr)
        {
            TokenName = abbr;
            Transactions = new ObservableRangeCollection<EthTransaction>();
            Balance = new EthBalance() { Confirmed = .0, Total = .0 };
            ethTransactionService = EthTransactionService.Instance;
            ethStorageService = EthStorageService.Instance;
            ethereumWalletService = EthCommonService.Instance;

            RefreshCommand = new AsyncCommand(Refresh);

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
            IsDataEmpty = Transactions.Count == 0;
            OnPropertyChanged(nameof(IsDataLoading));
            OnPropertyChanged(nameof(IsDataLoading));
        }

        public async Task GetTransactions()
        {
            ethStorageService.GetTransactionsFromStorage();
            FormatTransactions();
            UpdateBalance();
            UpdateShareAddress();

            await ethTransactionService.GetTransactionsAsync();
            UpdateBalance();
            UpdateShareAddress();
        }

        public void FormatTransactions()
        {
            var formatedTxs = new List<EthTransaction>();
            var erc20Txs = ethereumWalletService.TransactionsFromStorage.
                Where(txFromStorage => txFromStorage.TxTo != null && txFromStorage.TxTo.Equals(CryptoWalletLibInit.TokenAddrByAbbr[TokenName], StringComparison.OrdinalIgnoreCase)).ToList();

            foreach (var txFromStorage in erc20Txs)
            {
                var ethTransaction = EthConverter.StorageTxToTx(txFromStorage);
                formatedTxs.Add(ethTransaction);
            }
            formatedTxs.Reverse();
            AddTransactions(formatedTxs);
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

        public void AddTransaction(EthTransactionForStoring ethTransactionForStoring)
        {
            var ethTransaction = EthConverter.StorageTxToTx(ethTransactionForStoring);
            Transactions.Insert(0, ethTransaction);
        }

        public void UpdateTransaction(EthTransactionForStoring ethTransactionForStoring)
        {
            var transaction = Transactions.FirstOrDefault(tx => tx.TxHash == ethTransactionForStoring.TxHash);
            EthConverter.StorageTxToTx(ethTransactionForStoring, ref transaction);
        }

        public void AddTransactions(List<EthTransaction> ethTransactions) => Transactions.AddRange(ethTransactions);

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
            Balance.Confirmed = ethereumWalletService.TotalConfirmedBalanceByToken.TryGetValue(CryptoWalletLibInit.TokenAddrByAbbr[key: TokenName], out var confirmedBalance) ? confirmedBalance : 0;
            Balance.Total = ethereumWalletService.TotalBalanceByToken.TryGetValue(CryptoWalletLibInit.TokenAddrByAbbr[key: TokenName], out var totalBalance) ? totalBalance : 0;
        }
        public void UpdateShareAddress()
        {
            ShareAddress = ethereumWalletService.ShareAddress;
            OnPropertyChanged(nameof(ShareAddress));
        }
    }


    public static class ERC20ViewModelLocator
    {
        private static readonly Dictionary<string, ERC20ViewModel> InstanceByAbbr = new();
        public static ERC20ViewModel ERC20ViewModelConstruc(string abbr)
        {
            if (InstanceByAbbr.ContainsKey(abbr)) return InstanceByAbbr[abbr];
            else
            {
                InstanceByAbbr[abbr] = new ERC20ViewModel(abbr);
                return InstanceByAbbr[abbr];
            }
        }
    }
}

