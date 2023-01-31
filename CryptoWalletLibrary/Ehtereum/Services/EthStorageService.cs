using CryptoWalletLibrary.Ehtereum.Models;
using System.Collections.Generic;

namespace CryptoWalletLibrary.Ehtereum.Services
{
    internal class EthStorageService
    {
        private readonly EthCommonService ethCommonService;
        private readonly EthBalanceService ethBalanceService;
        public static EthStorageService CreateInstance(EthCommonService ethCommonService, EthBalanceService ethBalanceService) => new(ethCommonService, ethBalanceService);
        public static EthStorageService Instance { get; private set; }

        public EthStorageService(EthCommonService ethCommonService, EthBalanceService ethBalanceService)
        {
            this.ethCommonService = EthCommonService.Instance;
            this.ethBalanceService = EthBalanceService.Instance;
            Instance = this;

            //ClearStorage();

            GetLastUsedAddressIdxFromStorage();
            GetTransactionsFromStorage();
            GetNFTAssetsFromStorage();
            GetNFTCollectionsFromStorage();
        }

        /// <summary>
        /// Gets transactions and balance from secure storage
        /// </summary>
        public void GetTransactionsFromStorage()
        {
            ethCommonService.TransactionsFromStorage = ethCommonService.context.SecureStorage.ObjectStorage.
                LoadObject(typeof(List<EthTransactionForStoring>), "etcTransactions") as List<EthTransactionForStoring>;
            if (ethCommonService.TransactionsFromStorage == null) ethCommonService.TransactionsFromStorage = new List<EthTransactionForStoring>();
            ethBalanceService.GetBalanceOfAddresses();
        }

        /// <summary>
        /// Gets NFT assets from secure storage
        /// </summary>
        public void GetNFTAssetsFromStorage()
        {
            ethCommonService.NftAssetsFromStorage = ethCommonService.context.SecureStorage.ObjectStorage.
                LoadObject(typeof(List<NFTAssetForStoring>), "nftAssets") as List<NFTAssetForStoring> ?? new List<NFTAssetForStoring>();
        }

        /// <summary>
        /// Gets NFT collections from secure storage
        /// </summary>
        public void GetNFTCollectionsFromStorage()
        {
            ethCommonService.NFTCollectionsFromStorage = ethCommonService.context.SecureStorage.ObjectStorage.
               LoadObject(typeof(List<NFTCollectionForStoring>), "nftCollections") as List<NFTCollectionForStoring> ?? new List<NFTCollectionForStoring>();
        }

        /// <summary>
        /// Gets user's last used adress index from secure sotrage
        /// </summary>
        public void GetLastUsedAddressIdxFromStorage() => ethCommonService.LastUsedAddressIdx = ethCommonService.context.SecureStorage.Values.Get("lastUsedAddressIdx", -1);

        /// <summary>
        /// Clears secure storage from ethereum transactions, nft assets and collcetions, erc20 and erc721 pages, last used user's address index. Used for testing purposes.
        /// </summary>
        private void ClearStorage()
        {
            ethCommonService.context.SecureStorage.ObjectStorage.DeleteObject(typeof(List<EthTransactionForStoring>), "etcTransactions");
            ethCommonService.context.SecureStorage.ObjectStorage.DeleteObject(typeof(List<NFTAssetForStoring>), "nftAssets");
            ethCommonService.context.SecureStorage.ObjectStorage.DeleteObject(typeof(List<NFTCollectionForStoring>), "nftCollections");
            ethCommonService.context.SecureStorage.Values.Set("lastUsedAddressIdx", -1);
            ethCommonService.context.SecureStorage.ObjectStorage.DeleteObject(typeof(List<string>), "tokensPages");
            ethCommonService.context.SecureStorage.Values.Set("nftPage", false);
        }
    }
}
