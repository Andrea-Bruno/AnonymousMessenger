using NBitcoin;
using Nethereum.Web3;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using System.Numerics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EncryptedMessaging;
using System.Diagnostics;
using Nethereum.HdWallet;
using Xamarin.Essentials;
using System.Net.Http;
using System.Text.Json;
using Banking.Ehtereum.Models;
using System.Linq;
using Nethereum.RPC.Fee1559Suggestions;
using Nethereum.Signer;
using Banking.Ehtereum.ViewModels;

namespace Banking.Ehtereum.Services
{
    public class EthereumWalletService : IEthereumWalletService
    {
        private readonly Context Context;
        public List<EthTransactionForStoring> EthTransactionFromStorage { get; private set; }
        public double TotalBalance { get; private set; }
        public double TotalConfirmedBalance { get; private set; }
        public Dictionary<string, double> TotalBalanceByToken { get; private set; }
        public Dictionary<string, double> TotalConfirmedBalanceByToken { get; private set; }
        public Dictionary<string, EthBalance> BalanceByAddress { get; private set; }
        private static readonly StringComparer comparer = StringComparer.OrdinalIgnoreCase;
        public Dictionary<string, Dictionary<string, EthBalance>> AddrBalanceByToken { get; private set; }

        private int lastUsedAddressIdx;

        private const int N_ADDRESS_TO_FETCH = 10;
        private const int MAX_RANGE_EMPTY_ADRR = 3;

        private static Web3 web3Clinet;
        private readonly Wallet wallet;
        private static HttpClient httpClient;
        private const string TX_INDEXER_URL = "http://localhost:8080";
        //private const string FULL_NODE_URL = "https://rinkeby.infura.io/v3/f6fee9b4b19841ab9db18b6ec2bb193f";
        private const string FULL_NODE_URL = "https://rinkeby.infura.io/v3/27cf7d3b72cf4537a2110e6713920fab";

        public static EthereumWalletService Instance { get; private set; }
        public string ShareAddress { get; private set; }
        public Fee1559 Fee { get; private set; }
        public static EthereumWalletService CreateInstance(Context context) => new EthereumWalletService(context);

        public EthereumWalletService(Context context)
        {
            Context = context;
            //Context.SecureStorage.ObjectStorage.DeleteObject(typeof(List<EthTransactionForStoring>), "etcTransactions");
            //Context.SecureStorage.Values.Set("lastUsedAddressIdx", -1);
            Debug.WriteLine("ETHWalletService!");

            web3Clinet = new Web3(FULL_NODE_URL);
            httpClient = new HttpClient();

            var words = "share width eye layer issue tenant tree acid hour around sight text";
            wallet = new Wallet(words, "");

            EthTransactionFromStorage = new List<EthTransactionForStoring>();
            lastUsedAddressIdx = 0;
            ShareAddress = wallet.GetAccount(0).Address;
            Instance = this;

            Task.Run(() => GetRecommendedFee());
            Task.Run(() => MintNFT(null, null, 1, null, null, null));

        }

        public async Task GetRecommendedFee()
        {
            Fee = await web3Clinet.FeeSuggestion.GetSimpleFeeSuggestionStrategy().SuggestFeeAsync();
            // var a = web3Clinet.Eth.GasPrice.SendRequestAsync().Result;
            // var x = web3Clinet.FeeSuggestion.GetMedianPriorityFeeHistorySuggestionStrategy().SuggestFeeAsync().Result;
            // var y = web3Clinet.FeeSuggestion.GeTimePreferenceFeeSuggestionStrategy().SuggestFeeAsync().Result;
        }




        public async Task<string> SendEther(string sourceAddress, string destinationAddress, decimal ethAmount, int? gasPrice, int? gasLimit)
        {
            var defaultGasLimit = 21000;
            await GetRecommendedFee();
            var recommendedGasPrice = (double)(Fee.BaseFee + Fee.MaxPriorityFeePerGas) / Math.Pow(10, 19);

            var web3 = new Web3(wallet.GetAccount(sourceAddress, chainId: (int)Chain.Rinkeby), url: FULL_NODE_URL);
            var transaction = await web3.Eth.GetEtherTransferService()
                   .TransferEtherAndWaitForReceiptAsync(destinationAddress, ethAmount, gas: gasLimit ?? defaultGasLimit, gasPriceGwei: gasPrice ?? (int)recommendedGasPrice);
            return transaction.BlockHash;
        }

        public async Task<string> SendToken(string sourceAddress, string destinationAddress, int tokenAmount, int? gasPrice, int? gasLimit, string tokenAbbr)
        {
            var web3 = new Web3(wallet.GetAccount(sourceAddress, chainId: (int)Chain.Rinkeby), url: FULL_NODE_URL);
            var contractAddr = CryptoWallet.TokenAddrByAbbr[tokenAbbr];
            var transferHandler = web3.Eth.GetContractTransactionHandler<TransferFunction>();
            var transfer = new TransferFunction()
            {
                To = destinationAddress,
                TokenAmount = tokenAmount
            };
            var transaction = await transferHandler.SendRequestAndWaitForReceiptAsync(contractAddr, transfer);
            return transaction.BlockHash;
        }

        public async Task<string> MintNFT(string sourceAddress, string destinationAddress, int tokenAmount, int? gasPrice, int? gasLimit, string tokenAbbr)
        {
            sourceAddress = "0xFdBd76F867D6bE7863f181C7D667079edef951c7";
            destinationAddress = "0x76b24618aB2613443832aD306D72dD7a002099e9";
            var web3 = new Web3(wallet.GetAccount(sourceAddress, chainId: (int)Chain.Rinkeby), url: FULL_NODE_URL);
            //var contractAddr = CryptoWallet.TokenAddrByAbbr[tokenAbbr];
            var transferHandler = web3.Eth.GetContractTransactionHandler<MintFunction>();
            var NFTContractAddr = "0x2F40DD9bD7656b4B6682944643F8F58cF8Bc347a";
            var abi = @"[ { ""inputs"": [ { ""internalType"": ""uint256"", ""name"": ""mintQuantity"", ""type"": ""uint256"" } ], ""name"": ""publicMintByUser"", ""outputs"": [], ""stateMutability"": ""payable"", ""type"": ""function"" }, { ""inputs"": [ { ""internalType"": ""address"", ""name"": ""from"", ""type"": ""address"" }, { ""internalType"": ""address"", ""name"": ""to"", ""type"": ""address"" }, { ""internalType"": ""uint256"", ""name"": ""tokenId"", ""type"": ""uint256"" } ], ""name"": ""transferFrom"", ""outputs"": [], ""stateMutability"": ""nonpayable"", ""type"": ""function"" } ]";
            var transfer = new MintFunction() { MintQuantity = 1 };
            var contract = web3.Eth.GetContract(abi, NFTContractAddr);
            var transferFunction = contract.GetFunction("publicMintByUser");
            //var gas = await transferFunction.EstimateGasAsync(from: sourceAddress, gas: null, value: null, 1);
            var gas = await transferFunction.EstimateGasAsync(sourceAddress, new Nethereum.Hex.HexTypes.HexBigInteger(21000), new Nethereum.Hex.HexTypes.HexBigInteger(100));
            var receiptSecondAmountSend = await transferFunction.SendTransactionAndWaitForReceiptAsync(sourceAddress, null, null, null, destinationAddress, 0.0002);

            Nethereum.Hex.HexTypes.HexBigInteger hexBigInteger = new Nethereum.Hex.HexTypes.HexBigInteger((BigInteger)0.07);
            var receiptAmountSend =
            await transferFunction.SendTransactionAndWaitForReceiptAsync(sourceAddress, gas: null, value: hexBigInteger);


            return receiptAmountSend.BlockHash;
        }

        public void GetTransactionsFromStorage()
        {
            EthTransactionFromStorage = Context.SecureStorage.ObjectStorage.
                LoadObject(typeof(List<EthTransactionForStoring>), "etcTransactions") as List<EthTransactionForStoring>;
            if (EthTransactionFromStorage == null) EthTransactionFromStorage = new List<EthTransactionForStoring>();
            GetBalanceOfAddresses();
        }

        public void GetLastUsedAddressIdxFromStorage() => lastUsedAddressIdx = Context.SecureStorage.Values.Get("lastUsedAddressIdx", -1);

        public async Task GetTransactionsAsync(Action<EthTransaction> addTransaction, Action<EthTransactionForStoring> removeTransaction)
        {
            var nSequentEmptyAdrr = 0;
            var fetchCount = 0;
            var addressListToFetch = new List<string>();
            while (true)
            {
                for (var i = fetchCount * N_ADDRESS_TO_FETCH; i < (fetchCount + 1) * N_ADDRESS_TO_FETCH; i++)
                    addressListToFetch.Add(wallet.GetAccount(i).Address);
                fetchCount++;

                var addrDic = new Dictionary<string, List<string>>() { { "addresses", addressListToFetch } };

                var json = JsonSerializer.Serialize(addrDic);
                var data = new StringContent(json, Encoding.UTF8, "application/json");
                var url = TX_INDEXER_URL + "/transactions";
                var response = await httpClient.PostAsync(url, data);
                var result = await response.Content.ReadAsStringAsync();

                //close out the client
                //httpClient.Dispose();

                var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                var txsOfAddresses = JsonSerializer.Deserialize<TxsOfAddressesDTO>(result, options);
                var addressesERC20Txs = new List<AddressEthTransactionsDTO>();
                foreach (var abbrAddr in CryptoWallet.TokenAddrByAbbr)
                {
                    foreach (var addressTxs in txsOfAddresses.AddressesEthTransactions)
                    {
                        var erc20txs = addressTxs.EthTransactions.Where(tx => tx.TxTo.Equals(abbrAddr.Value, StringComparison.OrdinalIgnoreCase)).ToList();
                        if (erc20txs.Count > 0)
                            addressesERC20Txs.Add(new AddressEthTransactionsDTO { Address = addressTxs.Address, EthTransactions = erc20txs });
                    }
                    Action<EthTransaction> addTx = null;
                    Action<EthTransactionForStoring> removeTx = null;

                    if (CryptoWallet.PagesByAbbr.ContainsKey(abbrAddr.Key))
                    {
                         addTx = (CryptoWallet.PagesByAbbr[abbrAddr.Key].RootPage.BindingContext as ERC20ViewModel).AddTransaction;
                        removeTx = (CryptoWallet.PagesByAbbr[abbrAddr.Key].RootPage.BindingContext as ERC20ViewModel).RemoveTransaction;
                    }
                    UpdateUI(addressesERC20Txs, addTx, removeTx);
                    addressesERC20Txs.Clear();
                }

                var addressesEtherTxs = new List<AddressEthTransactionsDTO>();
                foreach (var addressTxs in txsOfAddresses.AddressesEthTransactions)
                {
                    var etherTxs = addressTxs.EthTransactions.Where(tx => tx.ContractTo.Length == 0).ToList();
                    if (etherTxs.Count > 0)
                        addressesEtherTxs.Add(new AddressEthTransactionsDTO { Address = addressTxs.Address, EthTransactions = etherTxs });
                }
                UpdateUI(addressesEtherTxs, addTransaction, removeTransaction);

                for (var i = N_ADDRESS_TO_FETCH - 1; i >= 0; i--)
                    if (txsOfAddresses.AddressesEthTransactions[i].EthTransactions.Count == 0) nSequentEmptyAdrr++;
                    else break;
                if (nSequentEmptyAdrr > MAX_RANGE_EMPTY_ADRR)
                {
                    lastUsedAddressIdx = (fetchCount * N_ADDRESS_TO_FETCH) - nSequentEmptyAdrr - 1;
                    Context.SecureStorage.Values.Set("lastUsedAddressIdx", lastUsedAddressIdx);
                    break;
                }
                addressListToFetch.Clear();
                nSequentEmptyAdrr = 0;
                //!!! REMOVE, its for adding delay while testing
                System.Threading.Thread.Sleep(1000);
            }

            GetBalanceOfAddresses();
            GetNewAddress();
        }

        private void UpdateUI(List<AddressEthTransactionsDTO> addressesEthTransactions, Action<EthTransaction> addTransaction, Action<EthTransactionForStoring> removeTransaction)
        {
            foreach (var addressEthTransactions in addressesEthTransactions)
            {
                foreach (var tx in addressEthTransactions.EthTransactions)
                {
                    var transactionFromStorage = EthTransactionFromStorage.Find(txFromStore => txFromStore.TxHash == tx.TxHash);
                    // check if it is internal tx
                    if (transactionFromStorage != null)
                    {
                        if ((transactionFromStorage.Sent == EthTxSentStatus.SENT && (addressEthTransactions.Address == transactionFromStorage.TxTo || addressEthTransactions.Address.Equals(transactionFromStorage.ContractTo, StringComparison.OrdinalIgnoreCase))) ||
                            (transactionFromStorage.Sent == EthTxSentStatus.RECEIVED && addressEthTransactions.Address == transactionFromStorage.TxFrom))
                        {
                            MakeTxInternal(transactionFromStorage, addTransaction, removeTransaction);
                            continue;
                        }
                    }

                    // if tx was not in storage or tx got confifmed
                    if (transactionFromStorage == null || (!transactionFromStorage.Status && tx.Status))
                    {
                        // if itx status got confirmed delete
                        if (transactionFromStorage != null)
                        {
                            if (!EthTransactionFromStorage.Remove(transactionFromStorage)) Debug.WriteLine("EthTx not found in Storage!");
                            removeTransaction(transactionFromStorage);
                        }
                        transactionFromStorage = TxDTOToStorageTx(tx, addressEthTransactions.Address);
                        EthTransactionFromStorage.Add(transactionFromStorage);
                        _ = Context.SecureStorage.ObjectStorage.SaveObject(EthTransactionFromStorage, "etcTransactions");
                        var ethTransaction = StorageTxToTx(transactionFromStorage);
                        if (addTransaction != null) MainThread.BeginInvokeOnMainThread(() => addTransaction(ethTransaction));
                    }
                }
            }
        }


        private void MakeTxInternal(EthTransactionForStoring transactionFromStorage, Action<EthTransaction> addTransaction = null, Action<EthTransactionForStoring> removeTransaction = null)
        {
            if (removeTransaction != null) MainThread.BeginInvokeOnMainThread(() => removeTransaction(transactionFromStorage));
            transactionFromStorage.Sent = EthTxSentStatus.BOTH;
            _ = Context.SecureStorage.ObjectStorage.SaveObject(EthTransactionFromStorage, "etcTransactions");
            var ethTransaction = StorageTxToTx(transactionFromStorage);
            if (addTransaction != null) MainThread.BeginInvokeOnMainThread(() => addTransaction(ethTransaction));
        }

        private static EthTransactionForStoring TxDTOToStorageTx(EthTransactionDTO ethTransactionDTO, string address)
        {
            var transactionFromStorage = new EthTransactionForStoring()
            {
                Block = ethTransactionDTO.Block,
                ContractTo = ethTransactionDTO.ContractTo,
                ContractValue = ethTransactionDTO.ContractValue,
                Gas = ethTransactionDTO.Gas,
                GasPrice = ethTransactionDTO.GasPrice,
                Status = ethTransactionDTO.Status,
                Time = ethTransactionDTO.Time,
                TxFrom = ethTransactionDTO.TxFrom,
                TxTo = ethTransactionDTO.TxTo,
                TxHash = ethTransactionDTO.TxHash,
                Value = ethTransactionDTO.Value,
                Sent = ethTransactionDTO.TxFrom.Equals(address) ? EthTxSentStatus.SENT : EthTxSentStatus.RECEIVED,
            };
            return transactionFromStorage;
        }

        public static EthTransaction StorageTxToTx(EthTransactionForStoring transactionForStoring)
        {
            var ethTransaction = new EthTransaction()
            {
                Block = transactionForStoring.Block,
                Gas = transactionForStoring.Gas,
                GasPrice = transactionForStoring.GasPrice,
                Status = transactionForStoring.Status,
                Time = DateTimeOffset.FromUnixTimeSeconds((long)transactionForStoring.Time).DateTime,
                TxFrom = transactionForStoring.TxFrom,
                TxTo = transactionForStoring.TxTo,
                TxHash = transactionForStoring.TxHash,
                ContractTo = transactionForStoring.ContractTo,
                ContractValue = transactionForStoring.ContractValue,
                Value = decimal.ToDouble(transactionForStoring.Value) / Math.Pow(10, 18),
                Sent = transactionForStoring.Sent
            };
            return ethTransaction;
        }

        private void GetBalance()
        {
            TotalConfirmedBalance = 0;
            TotalBalance = 0;
            foreach (var tx in EthTransactionFromStorage)
            {
                if (tx.Sent == EthTxSentStatus.RECEIVED)
                {
                    var receivedAmount = decimal.ToDouble(tx.Value) + tx.Gas * tx.GasPrice;
                    if (tx.Status)
                        TotalConfirmedBalance += receivedAmount;
                    TotalBalance += receivedAmount;
                }
                else if (tx.Sent == EthTxSentStatus.SENT)
                {
                    var sentAmount = decimal.ToDouble(tx.Value);
                    if (tx.Status)
                        TotalConfirmedBalance -= sentAmount;
                    TotalBalance -= sentAmount;
                }
                // case when tx is internal
                else
                {
                    var txFee = tx.Gas * tx.GasPrice;
                    if (tx.Status)
                        TotalConfirmedBalance -= txFee;
                    TotalBalance -= txFee;
                }
            }
            TotalConfirmedBalance /= Math.Pow(10, 18);
            TotalBalance /= Math.Pow(10, 18);
        }

        private void GetBalanceOfAddresses()
        {
            TotalBalanceByToken = new Dictionary<string, double>();
            TotalConfirmedBalanceByToken = new Dictionary<string, double>();

            var txByToken = EthTransactionFromStorage.Where(tx => tx.ContractTo.Length > 0).GroupBy(tx => tx.TxTo);
            foreach (var TokenTx in txByToken)
            {
                GetERC20TokenBalanceOfAddresses(TokenTx.Key);
            }
            GetEtherBalanceOfAddresses();
        }

        private void GetEtherBalanceOfAddresses()
        {
            TotalBalance = 0;
            TotalConfirmedBalance = 0;
            BalanceByAddress = new Dictionary<string, EthBalance>(comparer);

            void txReceived(EthTransactionForStoring tx)
            {
                var address = tx.TxTo;
                var amountReceived = decimal.ToDouble(tx.Value) / Math.Pow(10, 18);
                if (BalanceByAddress.TryGetValue(address, out var balance))
                {
                    if (tx.Status)
                        balance.Confirmed += amountReceived;
                    balance.Total += amountReceived;
                }
                else
                {
                    BalanceByAddress[address] = tx.Status
                        ? new EthBalance() { Confirmed = amountReceived, Total = amountReceived }
                        : new EthBalance() { Confirmed = .0, Total = amountReceived };
                }
            }

            void txSent(EthTransactionForStoring tx)
            {
                var address = tx.TxFrom;
                var amountSpent = (decimal.ToDouble(tx.Value) + tx.GasPrice * tx.Gas) / Math.Pow(10, 18);
                if (BalanceByAddress.TryGetValue(address, out var balance))
                {
                    if (tx.Status)
                        balance.Confirmed -= amountSpent;
                    balance.Total -= amountSpent;
                }
                else
                {
                    BalanceByAddress[address] = tx.Status
                        ? new EthBalance() { Confirmed = -amountSpent, Total = -amountSpent }
                        : new EthBalance() { Confirmed = .0, Total = -amountSpent };
                }
            }

            foreach (var tx in EthTransactionFromStorage)
            {
                if (tx.Sent == EthTxSentStatus.RECEIVED)
                    txReceived(tx);
                else if (tx.Sent == EthTxSentStatus.SENT)
                    txSent(tx);
                else
                {
                    txReceived(tx);
                    txSent(tx);
                }
            }

            TotalBalance = BalanceByAddress.Sum(x => x.Value.Total);
            TotalConfirmedBalance = BalanceByAddress.Sum(x => x.Value.Confirmed);
        }


        private void GetERC20TokenBalanceOfAddresses(string tokenAddr)
        {
            var tokenAbbr = CryptoWallet.AbbrByTokenAddr[tokenAddr];
            var erc20TokenTxs = EthTransactionFromStorage.Where(tx => tx.TxTo == tokenAddr).ToList();
            TotalBalanceByToken[tokenAbbr] = 0;
            TotalConfirmedBalanceByToken[tokenAbbr] = 0;
            AddrBalanceByToken = new Dictionary<string, Dictionary<string, EthBalance>>();
            AddrBalanceByToken[tokenAbbr] = new Dictionary<string, EthBalance>(comparer);
            void txReceived(EthTransactionForStoring tx)
            {
                var address = tx.ContractTo;
                var amountReceived = double.Parse(tx.ContractValue);
                if (AddrBalanceByToken[tokenAbbr].TryGetValue(address, out var balance))
                {
                    if (tx.Status)
                        balance.Confirmed += amountReceived;
                    balance.Total += amountReceived;
                }
                else
                {
                    AddrBalanceByToken[tokenAbbr][address] = tx.Status
                        ? new EthBalance() { Confirmed = amountReceived, Total = amountReceived }
                        : new EthBalance() { Confirmed = .0, Total = amountReceived };
                }
            }

            void txSent(EthTransactionForStoring tx)
            {
                var address = tx.TxFrom;
                var amountSpent = double.Parse(tx.ContractValue);
                if (AddrBalanceByToken[tokenAbbr].TryGetValue(address, out var balance))
                {
                    if (tx.Status)
                        balance.Confirmed -= amountSpent;
                    balance.Total -= amountSpent;
                }
                else
                {
                    AddrBalanceByToken[tokenAbbr][address] = tx.Status
                        ? new EthBalance() { Confirmed = -amountSpent, Total = -amountSpent }
                        : new EthBalance() { Confirmed = .0, Total = -amountSpent };
                }
            }


            foreach (var tx in erc20TokenTxs)
            {
                if (tx.Sent == EthTxSentStatus.RECEIVED)
                    txReceived(tx);
                else if (tx.Sent == EthTxSentStatus.SENT)
                    txSent(tx);
                else //if tx is internal
                {
                    txReceived(tx);
                    txSent(tx);
                }
            }

            TotalBalanceByToken[tokenAbbr] = AddrBalanceByToken[tokenAbbr].Sum(x => x.Value.Total);
            TotalConfirmedBalanceByToken[tokenAbbr] = AddrBalanceByToken[tokenAbbr].Sum(x => x.Value.Confirmed);
        }

        public void GetNewAddress()
        {
            ShareAddress = wallet.GetAccount(lastUsedAddressIdx + 1).Address;
            lastUsedAddressIdx++;
        }
    }

    [Function("transfer", "bool")]
    public class TransferFunction : FunctionMessage
    {
        [Parameter("address", "_to", 1)]
        public string To { get; set; }

        [Parameter("uint256", "_value", 2)]
        public BigInteger TokenAmount { get; set; }
    }

    [Function("publicMintByUser")]
    public class MintFunction : FunctionMessage
    {
        [Parameter("uint256", "mintQuantity", 1)]
        public BigInteger MintQuantity { get; set; }
    }
}

