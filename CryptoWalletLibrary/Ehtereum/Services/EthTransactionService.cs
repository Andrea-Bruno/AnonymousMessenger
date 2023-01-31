using CryptoWalletLibrary.Ehtereum.Models;
using CryptoWalletLibrary.Ehtereum.Utilies;
using CryptoWalletLibrary.Ehtereum.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CryptoWalletLibrary.Ehtereum.Services
{
    internal class EthTransactionService
    {
        private readonly EthCommonService ethCommonService;
        private readonly EthBalanceService ethBalanceService;
        private readonly EthAdressService ethAdressService;

        private static HttpClient httpClient;

        private const int N_ADDRESS_TO_FETCH = 10; // number of address to fetch in 1 request
        private const int MAX_RANGE_EMPTY_ADRR = 3; // number of empty address encountered to stop querying
        private const string TX_INDEXER_BASE_URL = "https://90.191.43.19:8080";

        public static EthTransactionService CreateInstance(EthCommonService ethCommonService, EthBalanceService ethBalanceService, EthAdressService ethAdressService) => new(ethCommonService, ethBalanceService, ethAdressService);
        public static EthTransactionService Instance { get; private set; }

        public EthTransactionService(EthCommonService ethCommonService, EthBalanceService ethBalanceService, EthAdressService ethAdressService)
        {
            this.ethCommonService = ethCommonService;
            this.ethBalanceService = ethBalanceService;
            this.ethAdressService = ethAdressService;
            var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            httpClient = new HttpClient(handler);

            Instance = this;
        }

        /// <summary>
        /// Gets transactions of user's adresses. The querying proccess (deriving child addresses and getting transactions of those adresses) 
        /// continues untill maximum number of empty adresses is reached(i.e. MAX_RANGE_EMPTY_ADRR specified while intiilzing service). Derivation of new addresses is accorrding to <a href="https://github.com/bitcoin/bips/blob/master/bip-0044.mediawiki">BIP-44 standart</a> , as a result path used for 
        /// generating new addresses is  follwing  m/44'/60'/0'/0/x ( m / purpose' / coin_type' / account' / change / address_index, for more on coin types see<a href="https://github.com/satoshilabs/slips/blob/master/slip-0044.md"> SLIP-44 standartd</a> ) 
        /// </summary>
        /// <returns></returns>
        public async Task GetTransactionsAsync()
        {
            var nSequentEmptyAdrr = 0; // used to determine when to stop querying
            var fetchCount = 0; // number of made requests
            var addressListToFetch = new List<string>();

            while (true)
            {
                for (var i = fetchCount * N_ADDRESS_TO_FETCH; i < (fetchCount + 1) * N_ADDRESS_TO_FETCH; i++)
                    addressListToFetch.Add(ethCommonService.Wallet.GetAccount(i).Address);
                fetchCount++;

                var txsOfAddresses = await FetchAddressesBatchAsync(addressListToFetch);

                GetEtherTxs(txsOfAddresses);
                ProccessERC20Txs(txsOfAddresses);
                ProccessERC721Txs(txsOfAddresses);

                //checking if adress fetching needs to stop
                for (var i = N_ADDRESS_TO_FETCH - 1; i >= 0; i--)
                    if (txsOfAddresses.AddressesEthTransactions[i].EthTransactions.Count == 0) nSequentEmptyAdrr++;
                    else break;
                if (nSequentEmptyAdrr > MAX_RANGE_EMPTY_ADRR)
                {
                    ethCommonService.LastUsedAddressIdx = (fetchCount * N_ADDRESS_TO_FETCH) - nSequentEmptyAdrr - 1;
                    ethCommonService.context.SecureStorage.Values.Set("lastUsedAddressIdx", ethCommonService.LastUsedAddressIdx);
                    break;
                }

                //cleaning for next iterration
                addressListToFetch.Clear();
                nSequentEmptyAdrr = 0;
            }

            _ = Task.Run(() => GetTokensFromContractsAsync(ERC721ViewModelLocator.ERC721ViewModel.AddNFT, ERC721ViewModelLocator.ERC721ViewModel.RemoveNFT, ERC721ViewModelLocator.ERC721ViewModel.UpdateNFT));
            ethBalanceService.GetBalanceOfAddresses();
            ethAdressService.GetNewAddress();
        }

        /// <summary>
        /// Fetchs transaction of given addresses.
        /// </summary>
        /// <param name="addressListToFetch"></param>
        /// <returns> addresses with their tranactions respectively </returns>
        private static async Task<TxsOfAddressesDTO> FetchAddressesBatchAsync(List<string> addressListToFetch)
        {
            var addrDic = new Dictionary<string, List<string>>() { { "addresses", addressListToFetch } };
            var json = JsonSerializer.Serialize(addrDic);
            var data = new StringContent(json, Encoding.UTF8, "application/json");
            var response = new HttpResponseMessage();
            try
            {
                response = await httpClient.PostAsync(TX_INDEXER_BASE_URL + "/transactions", data);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ethereum back-end issue: " + ex.Message);
            }
            var result = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
            };
            var txsOfAddresses = JsonSerializer.Deserialize<TxsOfAddressesDTO>(result, options);
            return txsOfAddresses;
        }

        /// <summary>
        /// Filters Ether transactions from given  transactions, then stores them and updates UI.
        /// </summary>
        /// <param name="txsOfAddresses">adresses with their transactions</param>
        private void GetEtherTxs(TxsOfAddressesDTO txsOfAddresses)
        {
            var addressesEtherTxs = new List<AddressEthTransactionsDTO>();
            foreach (var addressTxs in txsOfAddresses.AddressesEthTransactions)
            {
                //!!! change to: check enum ERC property instead of contract length
                var etherTxs = addressTxs.EthTransactions.Where(tx => tx.Erc == ERC.ETH).ToList();
                if (etherTxs.Count > 0)
                    addressesEtherTxs.Add(new AddressEthTransactionsDTO { Address = addressTxs.Address, EthTransactions = etherTxs });
            }

            UpdateUI(addressesEtherTxs, EthTxViewModelLocator.EthTxViewModel.AddTransaction,
                EthTxViewModelLocator.EthTxViewModel.UpdateTransaction, EthTxViewModelLocator.EthTxViewModel.RemoveTransaction);
        }

        /// <summary>
        /// Filters ERC20 transactions from given  transactions, then stores them and updates UI.
        /// </summary>
        /// <param name="txsOfAddresses"> adresses with their transactions</param>
        private void ProccessERC20Txs(TxsOfAddressesDTO txsOfAddresses)
        {
            var addressesERC20Txs = new List<AddressEthTransactionsDTO>();
            foreach (var abbrAddr in CryptoWalletLibInit.TokenAddrByAbbr)
            {
                foreach (var addressTxs in txsOfAddresses.AddressesEthTransactions)
                {
                    var erc20txs = addressTxs.EthTransactions.Where(tx => tx.TxTo != null && tx.TxTo.Equals(abbrAddr.Value, StringComparison.OrdinalIgnoreCase)).ToList();
                    if (erc20txs.Count > 0)
                        addressesERC20Txs.Add(new AddressEthTransactionsDTO { Address = addressTxs.Address, EthTransactions = erc20txs });
                }

                if (CryptoWalletLibInit.ERC20PageMethodsByAbbr.ContainsKey(abbrAddr.Key))
                {
                    var addTransactionERC20 = (Action<EthTransactionForStoring>)CryptoWalletLibInit.ERC20PageMethodsByAbbr[abbrAddr.Key]["addTransaction"];
                    var updateTransactionERC20 = (Action<EthTransactionForStoring>)CryptoWalletLibInit.ERC20PageMethodsByAbbr[abbrAddr.Key]["updateTransaction"];
                    var removeTransactionERC20 = (Action<string>)CryptoWalletLibInit.ERC20PageMethodsByAbbr[abbrAddr.Key]["removeTransaction"];
                    UpdateUI(addressesERC20Txs, addTransactionERC20, updateTransactionERC20, removeTransactionERC20);

                }

                //saving to store if token is not added by user
                else
                {
                    foreach (var addressERC20Txs in addressesERC20Txs)
                        foreach (var erc20Tx in addressERC20Txs.EthTransactions)
                            if (!ethCommonService.TransactionsFromStorage.Exists(tx => tx.TxHash == erc20Tx.TxHash))
                                ethCommonService.TransactionsFromStorage.Add(EthConverter.TxDTOToStorageTx(erc20Tx));
                    _ = ethCommonService.context.SecureStorage.ObjectStorage.SaveObject(ethCommonService.TransactionsFromStorage, "etcTransactions");
                }
                addressesERC20Txs.Clear();
            }

            //saving ERC20 which are not in application ERC20 token name list
            foreach (var addressTxs in txsOfAddresses.AddressesEthTransactions)
            {
                var uknownERC20txs = addressTxs.EthTransactions.Where(tx => tx.TxTo != null && !CryptoWalletLibInit.TokenAddrByAbbr.Values.Contains(tx.TxTo) && tx.Erc == ERC.ERC20).ToList();
                if (uknownERC20txs.Count > 0)
                {
                    foreach (var uknonwnERC20Tx in uknownERC20txs)
                        ethCommonService.TransactionsFromStorage.Add(EthConverter.TxDTOToStorageTx(uknonwnERC20Tx));
                    _ = ethCommonService.context.SecureStorage.ObjectStorage.SaveObject(ethCommonService.TransactionsFromStorage, "etcTransactions");
                }
            }
        }

        /// <summary>
        /// Filters ERC721 transactions from given  transactions and stores them if needed.
        /// </summary>
        /// <param name="txsOfAddresses"> adresses with their transactions</param>
        private void ProccessERC721Txs(TxsOfAddressesDTO txsOfAddresses)
        {
            var addressesERC721Txs = new List<AddressEthTransactionsDTO>();
            foreach (var addressTxs in txsOfAddresses.AddressesEthTransactions)
            {
                var erc721txs = addressTxs.EthTransactions.Where(tx => tx.Erc == ERC.ERC721).ToList();
                if (erc721txs.Count > 0)
                {
                    GetNFTCollections(erc721txs, addressTxs.Address);
                }
                foreach (var erc721tx in erc721txs)
                    if (!ethCommonService.TransactionsFromStorage.Exists(tx => tx.TxHash == erc721tx.TxHash))
                        ethCommonService.TransactionsFromStorage.Add(EthConverter.TxDTOToStorageTx(erc721tx));
                _ = ethCommonService.context.SecureStorage.ObjectStorage.SaveObject(ethCommonService.TransactionsFromStorage, "etcTransactions");
                addressesERC721Txs.Add(new AddressEthTransactionsDTO { Address = addressTxs.Address, EthTransactions = erc721txs });
            }
        }

        /// <summary>
        /// Updates UI based on new comming/changed/removed transactions. In additons stores them to storage if needed.
        /// </summary>
        /// <param name="addressesEthTransactions"></param>
        /// <param name="addTransaction"></param>
        /// <param name="updateTransaction"></param>
        /// <param name="removeTransaction"></param>
        private void UpdateUI(List<AddressEthTransactionsDTO> addressesEthTransactions, Action<EthTransactionForStoring> addTransaction, Action<EthTransactionForStoring> updateTransaction, Action<string> removeTransaction)
        {
            foreach (var addressEthTransactions in addressesEthTransactions)
            {
                ulong maxNonce = 0;
                foreach (var tx in addressEthTransactions.EthTransactions)
                {
                    if (tx.TxFrom == addressEthTransactions.Address && tx.Nonce > maxNonce) maxNonce = tx.Nonce;

                    var transactionFromStorage = ethCommonService.TransactionsFromStorage.Find(txFromStore => txFromStore.TxHash == tx.TxHash);
                    // check if it is internal tx
                    if (transactionFromStorage != null)
                    {
                        if ((transactionFromStorage.Sent == EthTxSentStatus.SENT && (addressEthTransactions.Address == transactionFromStorage.TxTo
                            || addressEthTransactions.Address.Equals(transactionFromStorage.ContractTo, StringComparison.OrdinalIgnoreCase)))
                            || (transactionFromStorage.Sent == EthTxSentStatus.RECEIVED && addressEthTransactions.Address == transactionFromStorage.TxFrom))
                        {
                            MakeTxInternal(transactionFromStorage, updateTransaction);
                            continue;
                        }
                    }

                    // if tx is new
                    if (transactionFromStorage == null)
                    {
                        var transactionForStorage = EthConverter.TxDTOToStorageTx(tx);
                        ethCommonService.TransactionsFromStorage.Add(transactionForStorage);
                        _ = ethCommonService.context.SecureStorage.ObjectStorage.SaveObject(ethCommonService.TransactionsFromStorage, "etcTransactions");

                        if (addTransaction != null) ethCommonService.context.InvokeOnMainThread(() => addTransaction(transactionForStorage));
                        continue;
                    }
                    //if tx got confirmed or pending tx was included in block
                    if (transactionFromStorage.ConfirmedBlockN != tx.ConfirmedBlockN)
                    {
                        var oldConfirmStatus = transactionFromStorage.ConfirmationStatus;
                        transactionFromStorage.ConfirmedBlockN = tx.ConfirmedBlockN;
                        if (oldConfirmStatus == EthTxConfirmationStatus.PENDING)
                        {
                            transactionFromStorage.GasUsed = tx.GasUsed;
                            transactionFromStorage.SuccessStatus = EnumExtensions.BoolToSuccessStatus(tx.Status);
                        }
                        _ = ethCommonService.context.SecureStorage.ObjectStorage.SaveObject(ethCommonService.TransactionsFromStorage, "etcTransactions");
                        if (oldConfirmStatus != transactionFromStorage.ConfirmationStatus)
                            if (updateTransaction != null) ethCommonService.context.InvokeOnMainThread(() => updateTransaction(transactionFromStorage));
                    }

                }

                //Remove pending txs which were canceled since their nonce lower than tx nonce which was included in block
                var pendingTxsToCancel = ethCommonService.TransactionsFromStorage.Where(tx => tx.TxFrom == addressEthTransactions.Address && tx.ConfirmedBlockN == null && tx.Nonce <= maxNonce).ToList();
                foreach (var tx in pendingTxsToCancel)
                {
                    ethCommonService.TransactionsFromStorage.Remove(tx);
                    _ = ethCommonService.context.SecureStorage.ObjectStorage.SaveObject(ethCommonService.TransactionsFromStorage, "etcTransactions");
                    if (removeTransaction != null) ethCommonService.context.InvokeOnMainThread(() => removeTransaction(tx.TxHash));
                }
            }
        }

        /// <summary>
        /// Changes transaction "sent" status to BOTH(i.e. received and sent by user), saves transaction to storage and updates UI.
        /// </summary>
        /// <param name="transactionFromStorage">transaction which sent status needs to be changed</param>
        /// <param name="updateTransaction">method to update transaction in UI</param>
        private void MakeTxInternal(EthTransactionForStoring transactionFromStorage, Action<EthTransactionForStoring> updateTransaction = null)
        {
            transactionFromStorage.Sent = EthTxSentStatus.BOTH;
            _ = ethCommonService.context.SecureStorage.ObjectStorage.SaveObject(ethCommonService.TransactionsFromStorage, "etcTransactions");
            if (updateTransaction != null) ethCommonService.context.InvokeOnMainThread(() => updateTransaction(transactionFromStorage));
        }

        /// <summary>
        /// Gets user's NFTs from contract addresses detected while getting user's transactions, saves them to storage and updates UI.
        /// </summary>
        /// <param name="addNFT">method to add NFT to UI</param>
        /// <param name="removeNFT">method to remove NFT from UI</param>
        /// <param name="updateNFT">method to update NFT in UI</param>
        /// <returns></returns>
        public async Task GetTokensFromContractsAsync(Action<NFTAsset> addNFT, Action<NFTAssetForStoring> removeNFT, Action<NFTAsset> updateNFT)
        {
            foreach (var nftCollection in ethCommonService.NFTCollectionsFromStorage)
            {
                var contract = EthCommonService.Web3Clinet.Eth.GetContract(ContractABI.contractABI, nftCollection.ContractAddr);
                var balanceOfFunction = contract.GetFunction("balanceOf");
                var tokenOfOwnerByIndexFunction = contract.GetFunction("tokenOfOwnerByIndex");
                var tokenURIFunction = contract.GetFunction("tokenURI");
                var collectionNameFunction = contract.GetFunction("name");
                var collectionAbbrFunction = contract.GetFunction("symbol");

                var collectionName = await collectionNameFunction.CallAsync<string>();
                var collectionAbbr = await collectionAbbrFunction.CallAsync<string>();
                foreach (var ownerAddr in nftCollection.OwnerAdresses)
                {
                    var ownedNftCount = await balanceOfFunction.CallAsync<int>(ownerAddr);
                    var ownedTokens = new List<int>();
                    for (var i = 0; i < ownedNftCount; i++)
                    {
                        var tokenId = await tokenOfOwnerByIndexFunction.CallAsync<int>(ownerAddr, i);
                        var tokenURI = await tokenURIFunction.CallAsync<string>(tokenId);
                        ownedTokens.Add(tokenId);
                        var nftFromStorage = ethCommonService.NftAssetsFromStorage.FirstOrDefault(nftAsset => nftAsset.ContractAddr == nftCollection.ContractAddr && nftAsset.TokenId == tokenId);
                        if (nftFromStorage == null)
                        {
                            var newNftForStorage = new NFTAssetForStoring()
                            {
                                TokenId = tokenId,
                                TokenURI = tokenURI,
                                ContractAddr = nftCollection.ContractAddr,
                                OwnerAddr = ownerAddr,
                                CollcetionName = collectionName,
                                CollcetionAbbr = collectionAbbr,
                            };
                            ethCommonService.NftAssetsFromStorage.Add(newNftForStorage);
                            ethCommonService.context.InvokeOnMainThread(() => addNFT(EthConverter.StorageNFTtoNFT(newNftForStorage)));

                        }
                        else if (tokenURI != nftFromStorage.TokenURI || collectionName != nftFromStorage.CollcetionName || collectionAbbr != nftFromStorage.CollcetionAbbr)
                        {
                            ethCommonService.NftAssetsFromStorage.Remove(nftFromStorage);

                            var newNftForStorage = new NFTAssetForStoring()
                            {
                                TokenId = tokenId,
                                TokenURI = tokenURI,
                                ContractAddr = nftCollection.ContractAddr,
                                OwnerAddr = ownerAddr,
                                CollcetionName = collectionName,
                                CollcetionAbbr = collectionAbbr,
                            };
                            ethCommonService.NftAssetsFromStorage.Add(newNftForStorage);
                            ethCommonService.context.InvokeOnMainThread(() => updateNFT(EthConverter.StorageNFTtoNFT(newNftForStorage)));
                        }
                    }
                    var nftAssetsToDelete = ethCommonService.NftAssetsFromStorage.Where(nftAsset => nftAsset.ContractAddr == nftCollection.ContractAddr
                    && nftAsset.OwnerAddr == ownerAddr && !ownedTokens.Contains(nftAsset.TokenId)).ToList();
                    foreach (var nftAsset in nftAssetsToDelete)
                    {
                        ethCommonService.NftAssetsFromStorage.Remove(nftAsset);
                        ethCommonService.context.InvokeOnMainThread(() => removeNFT(nftAsset));
                    }
                }
                ethCommonService.context.SecureStorage.ObjectStorage.SaveObject(ethCommonService.NftAssetsFromStorage, "nftAssets");
            }
        }

        /// <summary>
        /// extracts nft collections from ERC721 and saves them to storage.
        /// </summary>
        /// <param name="erc721txs">ERC721 transactions</param>
        /// <param name="address">address to which collections will belong</param>
        private void GetNFTCollections(List<EthTransactionDTO> erc721txs, string address)
        {
            foreach (var erc721tx in erc721txs)
            {
                var nftCollection = ethCommonService.NFTCollectionsFromStorage.FirstOrDefault(collection => collection.ContractAddr == erc721tx.TxTo);
                if (nftCollection == null)
                {
                    nftCollection = new NFTCollectionForStoring()
                    {
                        ContractAddr = erc721tx.TxTo,
                        OwnerAdresses = new List<string>() { }
                    };
                    ethCommonService.NFTCollectionsFromStorage.Add(nftCollection);
                }
                if (!nftCollection.OwnerAdresses.Contains(address))
                    nftCollection.OwnerAdresses.Add(address);
            }
            ethCommonService.context.SecureStorage.ObjectStorage.SaveObject(ethCommonService.NFTCollectionsFromStorage, "nftCollections");
        }
    }
}
