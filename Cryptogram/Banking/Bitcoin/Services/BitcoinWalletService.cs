using Banking.Models;
using ElectrumXClient;
using NBitcoin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QBitNinja.Client;
using QBitNinja.Client.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;
using static ElectrumXClient.Response.BlockchainScripthashGetHistoryResponse;
using static ElectrumXClient.Response.BlockchainScripthashGetBalanceResponse;
using static ElectrumXClient.Response.BlockchainTransactionGetResponse;
using MvvmHelpers;
using Xamarin.Essentials;
using EncryptedMessaging;


namespace Banking.Services
{
    public class BitcoinWalletService
    {
        private readonly Context Context;
        private BitcoinSecret secretKey;
        private ExtKey masterKey;
        private Dictionary<string, List<BlockchainTransactionGetResult>> transactionsPerAddress;
        private int changeAddrCount;
        private int mainAddrCount;
        private Dictionary<string, BlockchainScripthashGetBalanceResult> balancePerAddress;
        public BitcoinAddress MainAddress { get; set; }
        private BitcoinAddress changeAddress;
        private List<BitcoinAddress> mainAddresses;
        private List<BitcoinAddress> changeAddresses;

        private ExtKey mainAdressesParentExtKey;
        private ExtKey changeAdressesParentExtKey;
        private ExtPubKey mainAdressesParentExtPubKey;
        private ExtPubKey changeAdressesParentExtPubKey;

        private List<Coin> nBitcoinSelectedUnspentCoins;
        private Money bitFeeRecommendedFastest;
        private List<BitcoinAddress> destinationAdresses;
        private Money amountToSend;

        private Client _client;
        private string host;
        private int port;
        private bool useSSL;
        private const int MAX_RANGE_EMPTY_ADRR = 5;

        public List<BitcoinTransactionForStoring> TransactionsForStore { get; set; }
        public Dictionary<string, List<UtxoDetailsElectrumx>> UtxosPerAddress { get; set; }
        public double TotalConfirmedBalance { get; set; }
        public double TotalUncnfirmedBalance { get; set; }
        public List<UnspentCoin> SelectedUnspentCoins { get; set; }
        public BitcoinTransaction SelectedBitcoinTransaction { get; set; }
        public uint256 BroadcastedTransactionId { get; set; }
        public bool TrBuildFailed { get; set; }
        public bool NoConfirmedCoins { get; set; }
        public Network BitcoinNetwork { get; set; }

        //public delegate void GetTransactionResponse(BitcoinTransaction bitcoinTransaction);
        //private readonly GetTransactionResponse _transactionResponseReceived;

        public static BitcoinWalletService Instance { get; private set; }
        public static BitcoinWalletService CreateInstance( Context context)
        {
            Instance = new BitcoinWalletService( context);
            return Instance;
        }


        public BitcoinWalletService(Context context)
        {
            Context = context;
            BitcoinNetwork = Network.TestNet;

            Debug.WriteLine("BTCWalletService!: " + new Key().GetWif(BitcoinNetwork));
            //Application.Current.Properties.Clear();
            //Application.Current.SavePropertiesAsync();
            //---- Generating Ext and ExtPub Keys ----
            var secretKeyStr = "L4VpjddASNaAKqfD7A3giyFcBoa7NrZizX1RNFskwwVcovpEojkZ";
            secretKey = new BitcoinSecret(secretKeyStr);
            masterKey = new ExtKey(secretKey.PrivateKey, new byte[32]);
            var hardenedPathMainAdresses = new KeyPath("44'/0'/0'/0");
            var hardenedPathChangeAdresses = new KeyPath("44'/0'/0'/1");

            mainAdressesParentExtKey = masterKey.Derive(hardenedPathMainAdresses);
            changeAdressesParentExtKey = masterKey.Derive(hardenedPathChangeAdresses);
            mainAdressesParentExtPubKey = masterKey.Derive(hardenedPathMainAdresses).Neuter();
            changeAdressesParentExtPubKey = masterKey.Derive(hardenedPathChangeAdresses).Neuter();

            mainAddrCount = 0;
            changeAddrCount = 0;

            //---- Client for Electrumx ----
            //host = "192.168.1.112";
            //host = "80.235.115.68";
            host = "90.191.43.19";
            port = 8001;
            useSSL = false;
            _client = new Client(host, port, useSSL);

            //transactionsNBitcoinPerAddr = new Dictionary<string, List<Transaction>>();
            mainAddresses = new List<BitcoinAddress>();
            changeAddresses = new List<BitcoinAddress>();
            TransactionsForStore = new List<BitcoinTransactionForStoring>();
            //transactionsPerAddrForStore = new Dictionary<string, List<BitcoinTransactionForStoring>>();
            UtxosPerAddress = new Dictionary<string, List<UtxoDetailsElectrumx>>();

            Task.Run(() => getRecommendedFee());
        }


        // ------------------------- Get Transactions, UTXO, Balance  -------------------------
        public async Task GetTransactionsMainAddrElectrumxAsync(Action<BitcoinTransaction> addTransaction, Action<BitcoinTransactionForStoring> removeTransaction, BitcoinBalance Balance)
        {
            TotalConfirmedBalance = 0;
            TotalUncnfirmedBalance = 0;

            var nSequentEmptyAdrr = 0;
            while (true)
            {
                MainAddress = mainAdressesParentExtPubKey.Derive((uint)mainAddrCount).GetPublicKey().GetAddress(BitcoinNetwork);
                mainAddrCount++;

                var bitcoinAddress = MainAddress;
                var scriptHash = bitcoinAddress.ScriptPubKey.WitHash.ToString();
                var reversedScriptHash = BitcoinHelper.RevertScriptHash(scriptHash);

                var transactionsResult = (await _client.GetBlockchainScripthashGetHistory(reversedScriptHash)).Result;
                nSequentEmptyAdrr = transactionsResult.Count == 0 ? nSequentEmptyAdrr + 1 : 0;

                if (nSequentEmptyAdrr > MAX_RANGE_EMPTY_ADRR)
                {
                    mainAddrCount -= (MAX_RANGE_EMPTY_ADRR + 1);
                    break;
                }
                if (transactionsResult.Count > 0) await getTransactionDetailsOfAddr(transactionsResult, addTransaction, removeTransaction, Balance, MainAddress);
            }
        }

        public async Task GetTransactionsChangeAddrElectrumxAsync(Action<BitcoinTransaction> addTransaction, Action<BitcoinTransactionForStoring> removeTransaction, BitcoinBalance Balance)
        {
            var nSequentEmptyAdrr = 0;
            while (true)
            {
                changeAddress = changeAdressesParentExtPubKey.Derive((uint)changeAddrCount).GetPublicKey().GetAddress(BitcoinNetwork);
                changeAddrCount++;

                var scriptHash = changeAddress.ScriptPubKey.WitHash.ToString();
                var reversedScriptHash = BitcoinHelper.RevertScriptHash(scriptHash);

                var transactionsResult = (await _client.GetBlockchainScripthashGetHistory(reversedScriptHash)).Result;
                nSequentEmptyAdrr = transactionsResult.Count == 0 ? nSequentEmptyAdrr + 1 : 0;
                if (nSequentEmptyAdrr > MAX_RANGE_EMPTY_ADRR)
                {
                    changeAddrCount -= (MAX_RANGE_EMPTY_ADRR + 1);
                    break;
                }
                if (transactionsResult.Count > 0) await getTransactionDetailsOfAddr(transactionsResult, addTransaction, removeTransaction, Balance, changeAddress);
            }
            // Saving Balance to storage
            var bitcoinBalance = new BitcoinBalance() { Confirmed = TotalConfirmedBalance, Unconfirmed = TotalUncnfirmedBalance, Total = (TotalConfirmedBalance + TotalUncnfirmedBalance)};
            Context.SecureStorage.ObjectStorage.SaveObject(bitcoinBalance, "BitcoinBalance");
        }

        private async Task getTransactionDetailsOfAddr(List<BlockchainScripthashGetHistoryResult> trResults, Action<BitcoinTransaction> addTransaction, Action<BitcoinTransactionForStoring> removeTransaction, BitcoinBalance Balance, BitcoinAddress bitcoinAddress)
        {
            var address = bitcoinAddress.ToString();
            var transactionsOfAddr = new List<BitcoinTransactionForStoring>();
            foreach (var transaction in trResults)
            {
                var transactionForStore = TransactionsForStore.Find(tr => tr.TransactionId == transaction.TxHash);
                if (transactionForStore == null || !transactionForStore.Confirmed)
                {
                    var transactrionResponse = await _client.GetBlockchainTransactionGet(transaction.TxHash);
                    var trDetail = transactrionResponse.Result;
                    var transactionNBitcoin = Transaction.Parse(trDetail.Hex, BitcoinNetwork);
                    var trInputs = await getTransactionInputDetails(transactionNBitcoin);
                    var trOutputs = getTransactionOutputDetails(transactionNBitcoin);
                    var trDate = DateTimeOffset.FromUnixTimeSeconds(trDetail.Time).DateTime;

                    if (transactionForStore != null)
                    {
                        TransactionsForStore.Remove(transactionForStore);
                        removeTransaction(transactionForStore);
                    }
                    transactionForStore = new BitcoinTransactionForStoring()
                    {
                        TransactionHex = trDetail.Hex,
                        TransactionId = trDetail.Txid,
                        Date = trDate,
                        Inputs = trInputs,
                        Outputs = trOutputs,
                        Address = address,
                        Confirmed = trDetail.Confirmations > 6
                    };
                    TransactionsForStore.Add(transactionForStore);
                    Context.SecureStorage.ObjectStorage.SaveObject(TransactionsForStore, "BitcoinTransactions");
                    var bitcoinTransaction = FormatBitcoinTrasaction(transactionForStore, address);
                    MainThread.BeginInvokeOnMainThread(() => addTransaction(bitcoinTransaction));
                }
                transactionsOfAddr.Add(transactionForStore);
            }
            GetUTXOsAndBalance(address, transactionsOfAddr, Balance);
        }

        private async Task<List<TransactionInput>> getTransactionInputDetails(Transaction transaction)
        {
            var trInputs = new List<TransactionInput>();
            foreach (var vin in transaction.Inputs)
            {
                var transactrionResponse = await _client.GetBlockchainTransactionGet(vin.PrevOut.Hash.ToString());
                var trDetails = transactrionResponse.Result;
                var vout = trDetails.VoutValue.Find(v => v.N == vin.PrevOut.N);
                trInputs.Add(new TransactionInput()
                {
                    Address = BitcoinAddress.Create(vout.ScriptPubKey.Addresses.First(), BitcoinNetwork).ToString(),
                    Amount = vout.Value,
                    outputIdx = (int)vin.PrevOut.N,
                    TrId = vin.PrevOut.Hash.ToString()
                });
            }
            return trInputs;
        }

        public List<TransactionOutput> getTransactionOutputDetails(Transaction transaction)
        {
            var trOutputs = new List<TransactionOutput>();
            foreach (var output in transaction.Outputs)
            {
                trOutputs.Add(new TransactionOutput()
                {
                    Address = output.ScriptPubKey.GetDestinationAddress(BitcoinNetwork).ToString(),
                    Amount = output.Value.Satoshi
                });
            }
            return trOutputs;
        }

        public void GetUTXOsAndBalance(string address, List<BitcoinTransactionForStoring> transactionsOfAddr, BitcoinBalance Balance)
        {
            //First I collected all transactions of given address and only then start cheking if this tr is spent or not.
            //Otherwise later u can receive tr whcih spends tr u already indiacted as unspent. (or u need update utxo list each time u receive tr which spends your utxo)
            var confirmedBalance = .0;
            var unConfirmedBalance = .0;
            var receivedTransactionsOfAddr = transactionsOfAddr.Where(tr => tr.Outputs.Exists(output => output.Address == address)).ToList();
            foreach (var transaction in receivedTransactionsOfAddr)
            {
                if (UtxosPerAddress.ContainsKey(address) && UtxosPerAddress[address].Exists(utxo => utxo.TransactionId == transaction.TransactionId)) continue;
                if (TransactionsForStore.Exists(tr => tr.Inputs.Exists(input => input.TrId == transaction.TransactionId))) continue;

                var outputIdx = transaction.Outputs.FindIndex(output => output.Address == address);
                var utxoDetail = new UtxoDetailsElectrumx()
                {
                    Address = address,
                    Confirmed = transaction.Confirmed,
                    TransactionHex = transaction.TransactionHex,
                    TransactionId = transaction.TransactionId,
                    TransactionPos = outputIdx
                };
                if (UtxosPerAddress.ContainsKey(address)) UtxosPerAddress[address].Add(utxoDetail);
                else UtxosPerAddress[address] = new List<UtxoDetailsElectrumx>() { utxoDetail };

                if (transaction.Confirmed) confirmedBalance += transaction.Outputs[outputIdx].Amount;
                else unConfirmedBalance += transaction.Outputs[outputIdx].Amount;
            }

            confirmedBalance /= Math.Pow(10, 8);
            unConfirmedBalance /= Math.Pow(10, 8);
            TotalConfirmedBalance += confirmedBalance;
            TotalUncnfirmedBalance += unConfirmedBalance;
            //MainThread.BeginInvokeOnMainThread(() =>
            //{
            //    Balance.Confirmed = (double.Parse(Balance.Confirmed) + confirmedBalance).ToString();
            //    Balance.Unconfirmed = (double.Parse(Balance.Unconfirmed) + unConfirmedBalance).ToString();
            //    Balance.Total = (double.Parse(Balance.Confirmed) + double.Parse(Balance.Unconfirmed)).ToString();
            //});
        }

        public void getTransactionsFromStorage()
        {
            TransactionsForStore = Context.SecureStorage.ObjectStorage.
                LoadObject(typeof(List<BitcoinTransactionForStoring>), "BitcoinTransactions") as List<BitcoinTransactionForStoring>;
            if (TransactionsForStore == null) TransactionsForStore = new List<BitcoinTransactionForStoring>();
        }

        public void getBalanceFromStorage()
        {
            var balance = Context.SecureStorage.ObjectStorage.
                LoadObject(typeof(BitcoinBalance), "BitcoinBalance") as BitcoinBalance;

            TotalConfirmedBalance = balance != null ? balance.Confirmed : 0;
            TotalUncnfirmedBalance = balance != null ? balance.Unconfirmed : 0;
        }


        public BitcoinTransaction FormatBitcoinTrasaction(BitcoinTransactionForStoring transactionForStoring, string address)
        {
            Money transactionAmount = 0L;
            var transactionNBitcoin = Transaction.Parse(transactionForStoring.TransactionHex, BitcoinNetwork);
            var Outputs = new List<string>();
            var Inputs = new List<string>();

            var outputs = transactionNBitcoin.Outputs;
            foreach (var output in outputs)
            {
                var paymentScript = output.ScriptPubKey;
                var receiverAddress = paymentScript.GetDestinationAddress(BitcoinNetwork);
                Outputs.Add(receiverAddress.ToString());

                var outputAdress = output.ScriptPubKey.GetDestinationAddress(BitcoinNetwork).ToString();
                if (outputAdress == address) transactionAmount = output.Value;
            }
            if (transactionAmount == 0L) transactionAmount = transactionNBitcoin.TotalOut;

            var inputs = transactionNBitcoin.Inputs;
            foreach (TxIn input in inputs)
            {
                var senderAddress = input.GetSigner().ScriptPubKey.GetDestinationAddress(BitcoinNetwork);
                Inputs.Add(senderAddress.ToString());
            }
            var sent = Inputs.Contains(address.ToString());

            var bitcoinTransaction = new BitcoinTransaction()
            {
                Address = address.ToString(),
                Amount = transactionAmount.ToDecimal(MoneyUnit.BTC),
                Date = transactionForStoring.Date.ToString(),
                Sent = sent,
                TransactionId = transactionNBitcoin.GetHash().ToString()
            };
            return bitcoinTransaction;
        }
        // ------------------------- Get Transactions, UTXO, Balance  -------------------------


        public void GenerateAddresses()
        {
            mainAddresses = new List<BitcoinAddress>();
            changeAddresses = new List<BitcoinAddress>();
            for (var i = 0; i < mainAddrCount; i++)
            {
                MainAddress = mainAdressesParentExtPubKey.Derive((uint)i).GetPublicKey().GetAddress(BitcoinNetwork);
                mainAddresses.Add(MainAddress);
            }
            for (var i = 0; i < changeAddrCount; i++)
            {
                changeAddress = changeAdressesParentExtPubKey.Derive((uint)i).GetPublicKey().GetAddress(BitcoinNetwork);
                changeAddresses.Add(changeAddress);
            }
        }

        public void RemoveGeneratedAddresses()
        {
            mainAddresses.Clear();
            changeAddresses.Clear();
        }

        // Make Transaction
        public async Task SendCoinAutoSelection(Money amount, Money customFee, List<string> destinationAdressesStr)
        {
            destinationAdresses = new List<BitcoinAddress>();
            foreach (var address in destinationAdressesStr)
                destinationAdresses.Add(BitcoinAddress.Create(address, BitcoinNetwork));

            amountToSend = amount;
            Dictionary<Coin, bool> unspentCoins = new Dictionary<Coin, bool>();
            var utxos = UtxosPerAddress.SelectMany(d => d.Value).ToList();
            foreach (var utxoDetailsElectrum in utxos)
            {
                Transaction transaction = Transaction.Parse(utxoDetailsElectrum.TransactionHex, BitcoinNetwork);
                Coin coin = new Coin(transaction, (uint)utxoDetailsElectrum.TransactionPos);
                unspentCoins.Add(coin, utxoDetailsElectrum.Confirmed);
            }
            var unspentConfirmedCoins = new List<Coin>();
            var unspentUnconfirmedCoins = new List<Coin>();
            foreach (var coin in unspentCoins)
                if (coin.Value) unspentConfirmedCoins.Add(coin.Key);
                else unspentUnconfirmedCoins.Add(coin.Key);

            generateNewChangeAdress();

            var coinsToSpend = new List<Coin>();
            Money fee = 0L;

            var builder = BitcoinNetwork.CreateTransactionBuilder();
            if (customFee > 0L)
            {
                fee = customFee;
                bool haveEnough = SelectCoinsManualFee(ref coinsToSpend, amountToSend, fee, unspentConfirmedCoins);
                if (!haveEnough)
                    haveEnough = SelectCoinsManualFee(ref coinsToSpend, amountToSend, fee, unspentUnconfirmedCoins);
                if (!haveEnough)
                    throw new Exception("Not enough funds.");
            }
            else
            {
                bool haveEnough = SelectCoinsAutoFee(ref coinsToSpend, amountToSend, ref fee, unspentConfirmedCoins);
                if (!haveEnough)
                    haveEnough = SelectCoinsAutoFee(ref coinsToSpend, amountToSend, ref fee, unspentUnconfirmedCoins);
                if (!haveEnough)
                    throw new Exception("Not enough funds.");
            }

            var signingKeys = prepareSigningKeys(coinsToSpend);

            var tx = builder
                .AddCoins(coinsToSpend)
                .AddKeys(signingKeys.ToArray())
                .Send(destinationAdresses[0], amountToSend)
                .SetChange(changeAddress)
                .SendFees(fee)
                .BuildTransaction(true);

            if (!builder.Verify(tx))
            {
                Debug.WriteLine("Couldn't build the transaction.");
                TrBuildFailed = true;
                if (unspentConfirmedCoins.Count == 0)
                {
                    NoConfirmedCoins = true;
                }
            }

            Debug.WriteLine($"Transaction Id: {tx.GetHash()}");
            await broadCastTransactionAsync(tx);
            BroadcastedTransactionId = tx.GetHash();
        }


        public async Task SendCoinsManualSelectionAsync(Money amount, Money customFee, List<string> destinationAdressesStr)
        {
            destinationAdresses = new List<BitcoinAddress>();
            foreach (var address in destinationAdressesStr)
                destinationAdresses.Add(BitcoinAddress.Create(address, BitcoinNetwork));

            amountToSend = amount;
            var signingKeys = prepareSigningKeys(nBitcoinSelectedUnspentCoins);

            generateNewChangeAdress();

            Money fee;
            if (customFee > 0L) fee = customFee;
            else fee = CalculateTransactionFee(nBitcoinSelectedUnspentCoins, signingKeys);

            var builder = BitcoinNetwork.CreateTransactionBuilder();
            var tx = builder
                .AddCoins(nBitcoinSelectedUnspentCoins)
                .AddKeys(signingKeys.ToArray())
                .Send(destinationAdresses[0], amountToSend)
                .SetChange(changeAddress)
                .SendFees(fee)
                .BuildTransaction(true);

            if (!builder.Verify(tx))
            {
                Debug.WriteLine("Couldn't build the transaction.");
                TrBuildFailed = true;
            }
            Debug.WriteLine($"Transaction Id: {tx.GetHash()}");
            await broadCastTransactionAsync(tx);
            BroadcastedTransactionId = tx.GetHash();
        }


        public static bool SelectCoinsManualFee(ref List<Coin> coinsToSpend, Money amount, Money fee, List<Coin> unspentCoins)
        {
            var haveEnough = false;
            foreach (var coin in unspentCoins.OrderByDescending(x => x.Amount))
            {
                coinsToSpend.Add(coin);
                // if doesn't reach amount, continue adding next coin
                if (coinsToSpend.Sum(x => x.Amount) < amount + fee) continue;
                else
                {
                    haveEnough = true;
                    break;
                }
            }
            return haveEnough;
        }

        public bool SelectCoinsAutoFee(ref List<Coin> coinsToSpend, Money amount, ref Money fee, List<Coin> unspentCoins)
        {
            var haveEnough = false;
            var signingKeys = new List<Key>();
            foreach (var coin in unspentCoins.OrderByDescending(x => x.Amount))
            {
                coinsToSpend.Add(coin);
                // if doesn't reach amount, continue adding next coin
                if (coinsToSpend.Sum(x => x.Amount) < amount + fee) continue;
                signingKeys.AddRange(prepareSigningKeys(coinsToSpend));
                fee = CalculateTransactionFee(coinsToSpend, signingKeys);
                if (coinsToSpend.Sum(x => x.Amount) < amount + fee) continue;
                haveEnough = true;
                break;
            }
            return haveEnough;
        }


        public async Task broadCastTransactionAsync(Transaction tx)
        {
            var broadcastResponse = await _client.BlockchainTransactionBroadcast(tx.ToHex());
            Debug.WriteLine("Transaction is successfully propagated on the network. TxId: " + broadcastResponse.Result, ConsoleColor.Green);
        }


        public async Task getRecommendedFee()
        {
            try
            {
                var client = new HttpClient();
                const string request = @"https://bitcoinfees.earn.com/api/v1/fees/recommended";
                var result = await client.GetAsync(request, HttpCompletionOption.ResponseContentRead);
                var json = JObject.Parse(result.Content.ReadAsStringAsync().Result);
                var fastestSatoshiPerByteFee = json.Value<decimal>("fastestFee");
                var fee = new Money(fastestSatoshiPerByteFee, MoneyUnit.Satoshi);

                bitFeeRecommendedFastest = fee;
            }
            catch
            {
                Debug.WriteLine("Couldn't calculate transaction fee, try it again later.");
                throw new Exception("Can't get tx fee");
            }
        }

        public List<Key> prepareSigningKeys(List<Coin> SelectedUnspentCoins)
        {
            GenerateAddresses();
            var signingKeys = new List<Key>();
            foreach (var coin in SelectedUnspentCoins)
            {
                var address = coin.TxOut.ScriptPubKey.GetDestinationAddress(BitcoinNetwork);
                var addrIdxInMainAdrrList = mainAddresses.FindIndex(addr => addr.ScriptPubKey.GetDestinationAddress(BitcoinNetwork) == address);
                var addrIdxInChangeAdrrList = changeAddresses.FindIndex(addr => addr.ScriptPubKey.GetDestinationAddress(BitcoinNetwork) == address);

                int addrIdx = addrIdxInMainAdrrList != -1 ? addrIdxInMainAdrrList : addrIdxInChangeAdrrList;
                if (addrIdx == -1) throw new ArgumentException("Address not found in Device Storage");

                var signingKey = addrIdxInMainAdrrList != -1 ? mainAdressesParentExtKey.Derive((uint)addrIdx).PrivateKey : changeAdressesParentExtKey.Derive((uint)addrIdx).PrivateKey;
                signingKeys.Add(signingKey);
            }
            return signingKeys;
        }

        public Money CalculateTransactionFee(List<Coin> SelectedUnspentCoins, List<Key> signingKeys)
        {
            var builder = BitcoinNetwork.CreateTransactionBuilder();
            var txForFeeCalculation = builder
                .AddCoins(SelectedUnspentCoins)
                .AddKeys(signingKeys.ToArray())
                .Send(destinationAdresses[0], amountToSend)
                .SetChange(changeAddress)
                .SendFees(0L)
                .BuildTransaction(true);

            var fee = bitFeeRecommendedFastest * (txForFeeCalculation.GetVirtualSize() + SelectedUnspentCoins.Count);
            return fee;
        }


        public bool CheckCoinTransferPosiblity(string amount, string customFee, List<UnspentCoin> selectedUnspentCoins)
        {
            nBitcoinSelectedUnspentCoins = new List<Coin>();
            Money fee;
            foreach (var selectedUnspentCoin in selectedUnspentCoins)
            {
                var selectedCoin = UtxosPerAddress.SelectMany(d => d.Value).ToList().Single(unspentCoin => unspentCoin.TransactionId.ToString() == selectedUnspentCoin.TransactionId && unspentCoin.Address == selectedUnspentCoin.Address);
                Transaction transaction = Transaction.Parse(selectedCoin.TransactionHex, BitcoinNetwork);
                Coin coin = new Coin(transaction, (uint)selectedCoin.TransactionPos);
                nBitcoinSelectedUnspentCoins.Add(coin);
            }

            if (customFee != null && customFee != "") fee = BitcoinHelper.ParseBtcString(customFee);
            else fee = bitFeeRecommendedFastest * 255;

            Money totalAmountOfSelectedCoins = nBitcoinSelectedUnspentCoins.Sum(x => x.Amount);
            amountToSend = BitcoinHelper.ParseBtcString(amount);

            if (totalAmountOfSelectedCoins < amountToSend + fee) return false;
            return true;
        }

        public void generateNewMainAdress()
        {
            MainAddress = mainAdressesParentExtPubKey.Derive((uint)mainAddrCount).GetPublicKey().GetAddress(BitcoinNetwork);
            mainAddrCount++;
            Application.Current.Properties["mainAddrCount"] = mainAddrCount;
            Application.Current.SavePropertiesAsync();
        }

        public void generateNewChangeAdress()
        {
            changeAddress = changeAdressesParentExtPubKey.Derive((uint)changeAddrCount).GetPublicKey().GetAddress(BitcoinNetwork);
            changeAddrCount++;
            Application.Current.Properties["changeAddrCount"] = changeAddrCount;
            Application.Current.SavePropertiesAsync();
        }















        //------------------------- Old implementations ------------------------------
        public async Task GetUTXOsElectrumxAsync_OldImplementation()
        {
            GenerateAddresses();
            var mainAndChangeAddresses = mainAddresses.Concat(changeAddresses);

            UtxosPerAddress = new Dictionary<string, List<UtxoDetailsElectrumx>>();
            foreach (var address in mainAndChangeAddresses)
            {
                var scriptHash = address.ScriptPubKey.WitHash.ToString();
                var reversedScriptHash = BitcoinHelper.RevertScriptHash(scriptHash);
                //var UTXOsResponse = Task.Run(() => _client.GetBlockchainListunspent(reversedScriptHash)).Result;
                var UTXOsResponse = await _client.GetBlockchainListunspent(reversedScriptHash);

                //------ GET UTXO DETAILS -------
                var utxoDetails = new List<UtxoDetailsElectrumx>();
                foreach (var utxo in UTXOsResponse.Result)
                {
                    var transactionResponse = await _client.GetBlockchainTransactionGet(utxo.TxHash);
                    utxoDetails.Add(new UtxoDetailsElectrumx()
                    {
                        TransactionId = transactionResponse.Result.Txid,
                        TransactionHex = transactionResponse.Result.Hex,
                        TransactionPos = utxo.TxPos,
                        Confirmed = transactionResponse.Result.Confirmations > 6,
                    });
                }
                UtxosPerAddress[address.ToString()] = utxoDetails;
            }
        }

        //public static List<string> getaddressesfromstorage()
        //{
        //    var mainaddresses = jsonconvert.deserializeobject<list<string>>(application.current.properties["mainaddresses"].tostring());
        //    var changeaddresses = new list<string>();
        //    if (application.current.properties.containskey("changeaddresses")) changeaddresses = jsonconvert.deserializeobject<list<string>>(application.current.properties["changeaddresses"].tostring());
        //    var mainandchangeaddresses = mainaddresses.concat(changeaddresses).tolist();
        //    return mainandchangeaddresses;
        //}



        public async Task recoverMainAddressesAsync()
        {
            var mainAddresses = new List<string>();
            var nSequentEmptyAdrr = 0;

            while (true)
            {
                MainAddress = mainAdressesParentExtPubKey.Derive((uint)mainAddrCount).GetPublicKey().GetAddress(BitcoinNetwork);
                mainAddresses.Add(MainAddress.ToString());
                mainAddrCount++;

                var scriptHash = MainAddress.ScriptPubKey.WitHash.ToString();
                var reversedScriptHash = BitcoinHelper.RevertScriptHash(scriptHash);
                var transactrionsResponse = await _client.GetBlockchainScripthashGetHistory(reversedScriptHash);
                Debug.WriteLine("address bing recovered: " + MainAddress);
                nSequentEmptyAdrr = transactrionsResponse.Result.Count == 0 ? nSequentEmptyAdrr + 1 : 0;

                if (nSequentEmptyAdrr > MAX_RANGE_EMPTY_ADRR)
                {
                    mainAddresses.RemoveRange(mainAddresses.Count - nSequentEmptyAdrr, nSequentEmptyAdrr);
                    mainAddrCount -= 6;
                    break;
                }
            };

            Application.Current.Properties["mainAddresses"] = JsonConvert.SerializeObject(mainAddresses);
            Application.Current.Properties["mainAddrCount"] = mainAddrCount;
            Application.Current.SavePropertiesAsync();
        }

        public async Task recoverChangeAddressesAsync()
        {
            var changeAddresses = new List<string>();
            var nSequentEmptyAdrr = 0;


            while (true)
            {
                changeAddress = changeAdressesParentExtPubKey.Derive((uint)changeAddrCount).GetPublicKey().GetAddress(BitcoinNetwork);
                changeAddresses.Add(changeAddress.ToString());
                changeAddrCount++;

                var scriptHash = changeAddress.ScriptPubKey.WitHash.ToString();
                var reversedScriptHash = BitcoinHelper.RevertScriptHash(scriptHash);
                var transactrionsResponse = await _client.GetBlockchainScripthashGetHistory(reversedScriptHash);
                Debug.WriteLine("address bing recovered: " + changeAddress);


                nSequentEmptyAdrr = transactrionsResponse.Result.Count == 0 ? nSequentEmptyAdrr + 1 : 0;

                if (nSequentEmptyAdrr > MAX_RANGE_EMPTY_ADRR)
                {
                    changeAddresses.RemoveRange(changeAddresses.Count - nSequentEmptyAdrr, nSequentEmptyAdrr);
                    changeAddrCount -= 6;
                    break;
                }
            };

            Application.Current.Properties["changeAddresses"] = JsonConvert.SerializeObject(changeAddresses);
            Application.Current.Properties["changeAddrCount"] = changeAddrCount;
            Application.Current.SavePropertiesAsync();
        }

        public async Task recoverWallet()
        {
            mainAddrCount = 0;
            changeAddrCount = 0;
            await recoverMainAddressesAsync();
            await recoverChangeAddressesAsync();
        }

        public async Task OldGetTransactionsElectrumxAsync()
        {
            transactionsPerAddress = new Dictionary<string, List<BlockchainTransactionGetResult>>();

            GenerateAddresses();
            var mainAndChangeAddresses = mainAddresses.Concat(changeAddresses);

            foreach (var address in mainAndChangeAddresses)
            {
                var scriptHash = address.ScriptPubKey.WitHash.ToString();
                var reversedScriptHash = BitcoinHelper.RevertScriptHash(scriptHash);

                var transactrionsResponse = await _client.GetBlockchainScripthashGetHistory(reversedScriptHash);
                //------ GET TRANSACTIONS  -------
                var transactionDetails = new List<BlockchainTransactionGetResult>();
                foreach (BlockchainScripthashGetHistoryResult transaction in transactrionsResponse.Result)
                {
                    var transactrionResponse = await _client.GetBlockchainTransactionGet(transaction.TxHash);
                    transactionDetails.Add(transactrionResponse.Result);
                }
                transactionsPerAddress[address.ToString()] = transactionDetails;
            };
        }



        public async Task GetBalanceElectrumxAsync()
        {
            balancePerAddress = new Dictionary<string, BlockchainScripthashGetBalanceResult>();
            TotalConfirmedBalance = 0;
            TotalUncnfirmedBalance = 0;

            GenerateAddresses();
            var mainAndChangeAddresses = mainAddresses.Concat(changeAddresses);


            foreach (var address in mainAndChangeAddresses)
            {
                var scriptHash = address.ScriptPubKey.WitHash.ToString();
                var reversedScriptHash = BitcoinHelper.RevertScriptHash(scriptHash);
                var balance = await _client.GetBlockchainScripthashGetBalance(reversedScriptHash);
                balancePerAddress[address.ToString()] = balance.Result;

                TotalConfirmedBalance += Int32.Parse(balance.Result.Confirmed);
                TotalUncnfirmedBalance += Int32.Parse(balance.Result.Unconfirmed);
            }
        }


        // ------------------------- Get Transactions Called  NOT first time ------------------
        public async Task OldGetMainAddrTransactionsElectrumxAsync(Action<BitcoinTransaction> addTransaction, Action<BitcoinTransactionForStoring> removeTransaction, BitcoinBalance Balance)
        {
            var nSequentEmptyAdrr = 0;
            mainAddrCount = 0;
            while (true)
            {
                MainAddress = mainAdressesParentExtPubKey.Derive((uint)mainAddrCount).GetPublicKey().GetAddress(BitcoinNetwork);
                mainAddrCount++;
                var scriptHash = MainAddress.ScriptPubKey.WitHash.ToString();
                var reversedScriptHash = BitcoinHelper.RevertScriptHash(scriptHash);

                var transactionsResult = (await _client.GetBlockchainScripthashGetHistory(reversedScriptHash)).Result;
                nSequentEmptyAdrr = transactionsResult.Count == 0 ? nSequentEmptyAdrr + 1 : 0;
                if (nSequentEmptyAdrr > MAX_RANGE_EMPTY_ADRR)
                {
                    mainAddrCount -= (MAX_RANGE_EMPTY_ADRR + 1);
                    break;
                }
                if (transactionsResult.Count > 0) await getTransactionDetailsOfAddr(transactionsResult, addTransaction, removeTransaction, Balance, MainAddress);
            }
        }

        public async Task OldGetChangeAddrTransactionsElectrumxAsync(Action<BitcoinTransaction> addTransaction, Action<BitcoinTransactionForStoring> removeTransaction, BitcoinBalance Balance)
        {
            var nSequentEmptyAdrr = 0;
            while (true)
            {
                changeAddress = changeAdressesParentExtPubKey.Derive((uint)changeAddrCount).GetPublicKey().GetAddress(BitcoinNetwork);
                changeAddrCount++;
                var scriptHash = changeAddress.ScriptPubKey.WitHash.ToString();
                var reversedScriptHash = BitcoinHelper.RevertScriptHash(scriptHash);

                var transactionsResult = (await _client.GetBlockchainScripthashGetHistory(reversedScriptHash)).Result;
                Debug.WriteLine("address is recovered: " + changeAddress);

                nSequentEmptyAdrr = transactionsResult.Count == 0 ? nSequentEmptyAdrr + 1 : 0;
                if (nSequentEmptyAdrr > MAX_RANGE_EMPTY_ADRR)
                {
                    changeAddrCount -= (MAX_RANGE_EMPTY_ADRR + 1);
                    break;
                }
                if (transactionsResult.Count > 0) await getTransactionDetailsOfAddr(transactionsResult, addTransaction, removeTransaction, Balance, changeAddress);

            }
        }

        // ------------------------- Get Transactions Called  NOT first time ------------------



        // ------------------------- Get UTXOs  -------------------------
        private Dictionary<string, List<Transaction>> transactionsNBitcoinPerAddr;

        public void GetUTXOsElectrumxAsync()
        {
            UtxosPerAddress = new Dictionary<string, List<UtxoDetailsElectrumx>>();
            foreach (var addr in transactionsNBitcoinPerAddr.Keys)
            {
                var receivedTransactions = transactionsNBitcoinPerAddr[addr]
                    .Where(tr => tr.Outputs.Exists(output => output.ScriptPubKey.GetDestinationAddress(BitcoinNetwork).ToString() == addr))
                    .ToList();

                foreach (var receivedTransaction in receivedTransactions)
                {
                    if (!transactionsNBitcoinPerAddr[addr].Exists(tr => tr.Inputs.Exists(input => input.PrevOut.Hash == receivedTransaction.GetHash())))
                    {
                        var outputIdx = receivedTransaction.Outputs.FindIndex(output => output.ScriptPubKey.GetDestinationAddress(BitcoinNetwork).ToString() == addr);
                        var utxoDetail = new UtxoDetailsElectrumx()
                        {
                            Address = addr,
                            Confirmed = true,
                            TransactionHex = receivedTransaction.ToHex(),
                            TransactionId = receivedTransaction.GetHash().ToString(),
                            TransactionPos = outputIdx
                        };
                        if (UtxosPerAddress.ContainsKey(addr))
                            UtxosPerAddress[addr].Add(utxoDetail);
                        else
                            UtxosPerAddress[addr] = new List<UtxoDetailsElectrumx>() { utxoDetail };
                    }
                }
            }
        }

        // ------------------------- Get UTXOs  -------------------------

        public async Task<double> GetBalanceOfAdrrElectrumxAsync(BitcoinAddress address)
        {
            var scriptHash = address.ScriptPubKey.WitHash.ToString();
            var reversedScriptHash = BitcoinHelper.RevertScriptHash(scriptHash);
            var balance = await _client.GetBlockchainScripthashGetBalance(reversedScriptHash);

            var confirmedBalance = double.Parse(balance.Result.Confirmed);
            var uncnfirmedBalance = double.Parse(balance.Result.Unconfirmed);
            return confirmedBalance + uncnfirmedBalance;
        }

        public void ChangeDisplayBalance(BitcoinAddress address, BitcoinBalance Balance)
        {
            var receivedTransactions = TransactionsForStore
                .Where(tr => tr.Outputs.Exists(output => output.Address == address.ToString()))
                .ToList();

            var balance = .0;
            foreach (var receivedTransaction in receivedTransactions)
            {
                //var x = transactionsNBitcoinPerAddr[addr].Find(tr => tr.Inputs.Exists(input => input.PrevOut.Hash == receivedTransaction.GetHash()));
                //var y = transactionsNBitcoinPerAddr[addr].Exists(tr => tr.Inputs.Exists(input => input.PrevOut.Hash == receivedTransaction.GetHash()));
                if (!TransactionsForStore.Exists(tr => tr.Inputs.Exists(input => input.TrId == receivedTransaction.TransactionId)))
                {
                    var outputIdx = receivedTransaction.Outputs.FindIndex(output => output.Address == address.ToString());
                    var utxoDetail = new UtxoDetailsElectrumx()
                    {
                        Address = address.ToString(),
                        Confirmed = true,
                        TransactionHex = receivedTransaction.TransactionHex,
                        TransactionId = receivedTransaction.TransactionId,
                        TransactionPos = outputIdx
                    };
                    balance += receivedTransaction.Outputs[outputIdx].Amount;
                    if (UtxosPerAddress.ContainsKey(address.ToString()))
                        UtxosPerAddress[address.ToString()].Add(utxoDetail);
                    else
                        UtxosPerAddress[address.ToString()] = new List<UtxoDetailsElectrumx>() { utxoDetail };
                }
            }
            balance /= Math.Pow(10, 8);
            //MainThread.BeginInvokeOnMainThread(() => Balance.Total = (double.Parse(Balance.Total) + balance).ToString());
        }

        public void OldGenerateNewChangeAdress()
        {
            var changeAddresses = Application.Current.Properties.ContainsKey("changeAddresses") ? JsonConvert.DeserializeObject<List<string>>(Application.Current.Properties["changeAddresses"].ToString()) : new List<string>();
            changeAddress = changeAdressesParentExtPubKey.Derive((uint)changeAddrCount).GetPublicKey().GetAddress(BitcoinNetwork);
            changeAddrCount++;
            changeAddresses.Add(changeAddress.ToString());

            Application.Current.Properties["changeAddresses"] = JsonConvert.SerializeObject(changeAddresses);
            Application.Current.Properties["changeAddrCount"] = changeAddrCount;
            Application.Current.SavePropertiesAsync();
        }

        public async Task oldBroadCastTransactionAsync(Transaction tx)
        {
            var qBitClient = new QBitNinjaClient(BitcoinNetwork);
            // QBit's success response is buggy so check manually too		
            BroadcastResponse broadcastResponse;
            var success = false;
            var tried = 0;
            var maxTry = 7;
            do
            {
                tried++;
                Debug.WriteLine($"Try broadcasting transaction... ({tried})");
                broadcastResponse = await qBitClient.Broadcast(tx);
                var getTxResp = await qBitClient.GetTransaction(tx.GetHash());
                if (getTxResp == null)
                {
                    Thread.Sleep(3000);
                    continue;
                }
                else
                {
                    success = true;
                    break;
                }
            } while (tried <= maxTry);

            if (!success)
            {
                if (broadcastResponse.Error != null) Debug.WriteLine($"Error code: {broadcastResponse.Error.ErrorCode} Reason: {broadcastResponse.Error.Reason}");
                Debug.WriteLine($"The transaction might not have been successfully broadcasted. Please check the Transaction ID in a block explorer.", ConsoleColor.Blue);
            }
            else Debug.WriteLine("Transaction is successfully propagated on the network.", ConsoleColor.Green);

        }
    }
}
