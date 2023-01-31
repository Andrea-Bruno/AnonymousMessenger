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
    public class ERC721ViewModel : ViewModelBasecs
    {
        public ObservableRangeCollection<NFTAsset> NFTAssets { get; set; }
        public string ShareAddress { get; set; }
        public bool IsDataLoading { get; set; }
        public bool IsDataEmpty { get; set; }
        public AsyncCommand RefreshCommand { get; }
        private readonly EthCommonService ethereumWalletService;
        private readonly EthTransactionService ethTransactionService;

        public ERC721ViewModel()
        {
            NFTAssets = new ObservableRangeCollection<NFTAsset>();
            ethereumWalletService = EthCommonService.Instance;
            ethTransactionService = EthTransactionService.Instance;
            FormatNFTAssets();
            RefreshCommand = new AsyncCommand(RefreshAsync);

        }

        public async Task RefreshAsync()
        {
            IsBusy = true;

            await ethTransactionService.GetTransactionsAsync();

            IsBusy = false;
        }
        public void FormatNFTAssets()
        {
            var formatedNFTs = new List<NFTAsset>();
            foreach (var txFromStorage in ethereumWalletService.NftAssetsFromStorage)
            {
                var ethTransaction = EthConverter.StorageNFTtoNFT(txFromStorage);
                formatedNFTs.Add(ethTransaction);
            }
            AddNFTs(formatedNFTs);
        }

        public void AddNFT(NFTAsset nftAsset) => NFTAssets.Add(nftAsset);

        private void AddNFTs(List<NFTAsset> nftAssets) => NFTAssets.AddRange(nftAssets);

        public void RemoveNFT(NFTAssetForStoring nftAssetForStoring)
        {
            foreach (var nftAsset in NFTAssets)
            {
                if (nftAsset.ContractAddr == nftAssetForStoring.ContractAddr && nftAsset.TokenId == nftAssetForStoring.TokenId)
                {
                    NFTAssets.Remove(nftAsset);
                    break;
                }
            }
        }

        public void UpdateNFT(NFTAsset nftAsset)
        {
            var nftAssetForUpdate = NFTAssets.FirstOrDefault(nft => nft.TokenId == nftAsset.TokenId
            && nft.ContractAddr == nftAsset.ContractAddr && nft.OwnerAddr == nftAsset.OwnerAddr);
            var idx = NFTAssets.IndexOf(nftAssetForUpdate);
            NFTAssets.Remove(nftAssetForUpdate);
            NFTAssets.Insert(idx, nftAsset);
        }
    }

    public static class ERC721ViewModelLocator
    {
        public static ERC721ViewModel ERC721ViewModel { get; } = new ERC721ViewModel();
    }
}

