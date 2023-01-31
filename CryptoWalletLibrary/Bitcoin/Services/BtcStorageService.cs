using CryptoWalletLibrary.Models;
using System.Collections.Generic;

namespace CryptoWalletLibrary.Bitcoin.Services
{
    internal class BtcStorageService
    {
        private readonly BtcCommonService btcCommonService;
        public static BtcStorageService Instance { get; private set; }

        public static BtcStorageService CreateInstance(BtcCommonService btcCommonService)
        {
            Instance = new BtcStorageService(btcCommonService);
            return Instance;
        }
        public BtcStorageService(BtcCommonService btcCommonService)
        {
            this.btcCommonService = btcCommonService;
            GetTransactionsFromStorage();
            GetMainAndChangeAddrCountFromStorage();
        }

        /// <summary>
        /// Loads transactions from secure storage
        /// </summary>
        public void GetTransactionsFromStorage()
        {
            btcCommonService.TransactionsForStore = btcCommonService.context.SecureStorage.ObjectStorage.
                LoadObject(typeof(List<BitcoinTransactionForStoring>), "BitcoinTransactions") as List<BitcoinTransactionForStoring>;
            if (btcCommonService.TransactionsForStore == null) btcCommonService.TransactionsForStore = new List<BitcoinTransactionForStoring>();
        }

        /// <summary>
        /// Loads balance from secure storage
        /// </summary>
        public void GetBalanceFromStorage()
        {
            var balance = btcCommonService.context.SecureStorage.ObjectStorage.
                LoadObject(typeof(BitcoinBalance), "BitcoinBalance") as BitcoinBalance;

            btcCommonService.TotalConfirmedBalance = balance != null ? balance.Confirmed : 0;
            btcCommonService.TotalUncnfirmedBalance = balance != null ? balance.Unconfirmed : 0;
        }

        /// <summary>
        /// Loads main and change address count from secure storage
        /// </summary>
        public void GetMainAndChangeAddrCountFromStorage()
        {
            btcCommonService.mainAddrCount = btcCommonService.context.SecureStorage.Values.Get("mainAddrCount", 0);
            btcCommonService.changeAddrCount = btcCommonService.context.SecureStorage.Values.Get("changeAddrCount", 0);
        }

        private void ClearStorage()
        {
            btcCommonService.context.SecureStorage.ObjectStorage.DeleteObject(typeof(List<BitcoinTransactionForStoring>), "BitcoinTransactions");
            btcCommonService.context.SecureStorage.ObjectStorage.DeleteObject(typeof(BitcoinBalance), "BitcoinBalance");
            btcCommonService.context.SecureStorage.Values.Set("mainAddrCount", 0);
            btcCommonService.context.SecureStorage.Values.Set("changeAddrCount", 0);
        }

        /// <summary>
        /// Stores confirmed, unconfirmed and total balance in secure storage
        /// </summary>
        internal void StoreBalance()
        {
            var bitcoinBalance = new BitcoinBalance()
            {
                Confirmed = btcCommonService.TotalConfirmedBalance,
                Unconfirmed = btcCommonService.TotalUncnfirmedBalance,
                Total = btcCommonService.TotalConfirmedBalance + btcCommonService.TotalUncnfirmedBalance
            };
            btcCommonService.context.SecureStorage.ObjectStorage.SaveObject(bitcoinBalance, "BitcoinBalance");
        }
    }
}
