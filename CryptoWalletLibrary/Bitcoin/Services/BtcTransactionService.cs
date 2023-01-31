using CryptoWalletLibrary.Models;
using CryptoWalletLibrary.Services;
using ElectrumXClient.Response;
using NBitcoin;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using static CryptoWalletLibrary.Models.BitcoinTransactionViewModel;
using static ElectrumXClient.Response.BlockchainScripthashGetHistoryResponse;

namespace CryptoWalletLibrary.Bitcoin.Services
{
    /// <summary>
    /// Service to get user's transaction history.
    /// </summary>
    internal class BtcTransactionService
    {
        private readonly BtcCommonService btcCommonService;
        private readonly BtcStorageService btcStorageService;
        private readonly BtcBalanceService btcBalanceService;
        public static BtcTransactionService Instance { get; private set; }

        public static BtcTransactionService CreateInstance(BtcCommonService btcCommonService, BtcStorageService btcStorageService, BtcBalanceService btcBalanceService)
        {
            Instance = new BtcTransactionService(btcCommonService, btcStorageService, btcBalanceService);
            return Instance;
        }

        internal BtcTransactionService(BtcCommonService btcCommonService, BtcStorageService btcStorageService, BtcBalanceService btcBalanceService)
        {
            this.btcCommonService = btcCommonService;
            this.btcStorageService = btcStorageService;
            this.btcBalanceService = btcBalanceService;
        }

        /// <summary>
        /// Gets transactions of user's main and cnahge adresses. The querying proccess (deriving child addresses and getting transactions of those addresses) 
        /// continues untill maximum number of empty adresses is reached (i.e. MAX_RANGE_EMPTY_ADRR specified while intiilzing service). Derivation of new addresses is done accorrding to <a href="https://github.com/bitcoin/bips/blob/master/bip-0044.mediawiki">BIP-44 standart</a>, as a result path used for 
        /// generating new addresses are follwing  "44'/1'/0'/0/x" for main addresses and "44'/1'/0'/1/x" for change adresses. ( m / purpose' / coin_type' / account' / change / address_index, for more info on coin types see<a href="https://github.com/satoshilabs/slips/blob/master/slip-0044.md"> SLIP-44 standartd</a> ) 
        /// </summary>
        /// <returns></returns>
        public async Task GetTransactionsAsync()
        {
            await GetTransactionsElectrumxAsync(true);
            await GetTransactionsElectrumxAsync(false);
            btcStorageService.StoreBalance();
        }

        private async Task GetTransactionsElectrumxAsync(bool isMainAddr)
        {
            var nSequentEmptyAdrr = 0;
            var count = 0;
            while (true)
            {
                var addressTmp = (isMainAddr ? btcCommonService.mainAdressesParentExtPubKey : btcCommonService.changeAdressesParentExtPubKey).Derive((uint)count).GetPublicKey().GetAddress(ScriptPubKeyType.Legacy, BtcCommonService.BitcoinNetwork);
                count++;

                var scriptHash = addressTmp.ScriptPubKey.WitHash.ToString();
                var reversedScriptHash = BitcoinHelper.RevertScriptHash(scriptHash);

                var transactionsResult = new List<BlockchainScripthashGetHistoryResult>();
                try
                {
                    transactionsResult = (await BtcCommonService.Client.GetBlockchainScripthashGetHistory(reversedScriptHash)).Result;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("bitcoin back-end issue: " + ex.Message);
                    btcCommonService.TransactionsForStore.FindAll(tr => tr.Address == addressTmp.ToString()).
                        ForEach(x => transactionsResult.Add(new BlockchainScripthashGetHistoryResult() { TxHash = x.TransactionId }));
                }
                nSequentEmptyAdrr = transactionsResult.Count == 0 ? nSequentEmptyAdrr + 1 : 0;

                if (nSequentEmptyAdrr > BtcCommonService.MAX_RANGE_EMPTY_ADRR)
                {
                    if (nSequentEmptyAdrr > BtcCommonService.MAX_RANGE_EMPTY_ADRR)
                        count -= BtcCommonService.MAX_RANGE_EMPTY_ADRR + 1;
                    if (isMainAddr)
                    {
                        btcCommonService.mainAddrCount = count;
                        btcCommonService.context.SecureStorage.Values.Set("mainAddrCount", btcCommonService.mainAddrCount);
                        btcCommonService.MainAddress = btcCommonService.mainAdressesParentExtPubKey.Derive((uint)btcCommonService.mainAddrCount).GetPublicKey().GetAddress(ScriptPubKeyType.Legacy, BtcCommonService.BitcoinNetwork);
                    }
                    else
                    {
                        btcCommonService.changeAddrCount = count;
                        btcCommonService.context.SecureStorage.Values.Set("changeAddrCount", btcCommonService.changeAddrCount);
                        btcCommonService.ChangeAddress = btcCommonService.changeAdressesParentExtPubKey.Derive((uint)btcCommonService.changeAddrCount).GetPublicKey().GetAddress(ScriptPubKeyType.Legacy, BtcCommonService.BitcoinNetwork);
                    }
                    break;
                }
                if (transactionsResult.Count > 0) await GetTransactionDetailsOfAddr(transactionsResult,  addressTmp);
            }
        }

        /// <summary>
        /// Gets details of adress trsnaction list using their IDs (tx hashes). While querying for adress transactions ElectrumX server return list of IDs. After getting those 
        /// for each ID we look at storage and if there is no transaction stored with such ID (or transaction data needs to be updated, e.g. status needs to be confirmed)
        /// we fetch that transaction details from ElectrumX server.
        /// </summary>
        /// <param name="trResults">list of transaction objects containing IDs</param>
        /// <param name="bitcoinAddress">address to which this transaction IDs belong</param>
        /// <returns></returns>
        private async Task GetTransactionDetailsOfAddr(List<BlockchainScripthashGetHistoryResult> trResults, BitcoinAddress bitcoinAddress)
        {
            var address = bitcoinAddress.ToString();
            var transactionsOfAddr = new List<BitcoinTransactionForStoring>();
            foreach (var transaction in trResults)
            {
                var transactionFromStorage = btcCommonService.TransactionsForStore.Find(tr => tr.TransactionId == transaction.TxHash);

                if (transactionFromStorage != null && !transactionFromStorage.Confirmed)
                {
                    var transactrionResponse = new BlockchainTransactionGetResponse();
                    try
                    {
                        transactrionResponse = await BtcCommonService.Client.GetBlockchainTransactionGet(transaction.TxHash);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("bitcoin back-end issue: " + ex.Message);
                        transactionsOfAddr.Add(transactionFromStorage);
                        continue;
                    }
                    var trDetail = transactrionResponse.Result;
                    var confirmed = trDetail.Confirmations >= 6;
                    if (confirmed)
                    {
                        transactionFromStorage.Confirmed = confirmed;
                        btcCommonService.context.SecureStorage.ObjectStorage.SaveObject(btcCommonService.TransactionsForStore, "BitcoinTransactions");
                        btcCommonService.context.InvokeOnMainThread(() => BtcTxViewModelLocator.Instance.ConfirmTransactionStatus(transactionFromStorage.TransactionId));
                    }
                    if (trDetail.Time != 0 && transactionFromStorage.Date.Year == 1970)
                    {
                        transactionFromStorage.Date = DateTimeOffset.FromUnixTimeSeconds(trDetail.Time).DateTime;
                        btcCommonService.context.SecureStorage.ObjectStorage.SaveObject(btcCommonService.TransactionsForStore, "BitcoinTransactions");
                        btcCommonService.context.InvokeOnMainThread(() => BtcTxViewModelLocator.Instance.UpdateTrasnsactionDate(transactionFromStorage.TransactionId, transactionFromStorage.Date));
                    }
                }

                if (transactionFromStorage != null && transactionFromStorage.Address != address)
                {
                    var outputAdrr = transactionFromStorage.Outputs.FirstOrDefault(x => x.Address == address);
                    if (outputAdrr != null && !outputAdrr.IsUsersAddress)
                    {
                        outputAdrr.IsUsersAddress = true;
                        btcCommonService.context.SecureStorage.ObjectStorage.SaveObject(btcCommonService.TransactionsForStore, "BitcoinTransactions");
                        var bitcoinTransaction = BtcConverter.BtcTxForStoringToBtcTx(transactionFromStorage, address);
                        btcCommonService.context.InvokeOnMainThread(() => BtcTxViewModelLocator.Instance.UpdateTransaction(transactionFromStorage.TransactionId, bitcoinTransaction));
                    }

                    var inputAddr = transactionFromStorage.Inputs.FirstOrDefault(x => x.Address == address);
                    if (inputAddr != null && !inputAddr.IsUsersAddress)
                    {
                        inputAddr.IsUsersAddress = true;
                        btcCommonService.context.SecureStorage.ObjectStorage.SaveObject(btcCommonService.TransactionsForStore, "BitcoinTransactions");
                        var bitcoinTransaction = BtcConverter.BtcTxForStoringToBtcTx(transactionFromStorage, address);
                        btcCommonService.context.InvokeOnMainThread(() => BtcTxViewModelLocator.Instance.UpdateTransaction(transactionFromStorage.TransactionId, bitcoinTransaction));
                    }
                }

                if (transactionFromStorage == null)
                {
                    var transactrionResponse = new BlockchainTransactionGetResponse();
                    try
                    {
                        transactrionResponse = await BtcCommonService.Client.GetBlockchainTransactionGet(transaction.TxHash);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("bitcoin back-end issue: " + ex.Message);
                        continue;
                    }
                    var trDetail = transactrionResponse.Result;
                    var transactionNBitcoin = Transaction.Parse(trDetail.Hex, BtcCommonService.BitcoinNetwork);
                    var trInputs = await BtcTransactionHelper.GetTransactionInputDetails(transactionNBitcoin, address);
                    var trOutputs = BtcTransactionHelper.GetTransactionOutputDetails(transactionNBitcoin, address);
                    var trDate = DateTimeOffset.FromUnixTimeSeconds(trDetail.Time).DateTime;

                    transactionFromStorage = new BitcoinTransactionForStoring()
                    {
                        TransactionHex = trDetail.Hex,
                        TransactionId = trDetail.Txid,
                        Date = trDate,
                        Inputs = trInputs,
                        Outputs = trOutputs,
                        Address = address,
                        Confirmed = trDetail.Confirmations > 6
                    };
                    btcCommonService.TransactionsForStore.Add(transactionFromStorage);
                    btcCommonService.context.SecureStorage.ObjectStorage.SaveObject(btcCommonService.TransactionsForStore, "BitcoinTransactions");
                    var bitcoinTransaction = BtcConverter.BtcTxForStoringToBtcTx(transactionFromStorage, address);
                    btcCommonService.context.InvokeOnMainThread(() => BtcTxViewModelLocator.Instance.AddTransaction(bitcoinTransaction));
                }

                transactionsOfAddr.Add(transactionFromStorage);
            }
            btcBalanceService.GetUTXOsAndBalance(address, transactionsOfAddr);
        }
    }
}
