using CryptoWalletLibrary.Models;
using CryptoWalletLibrary.Services;
using NBitcoin;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using static CryptoWalletLibrary.Models.BitcoinTransactionViewModel;

namespace CryptoWalletLibrary.Bitcoin.Services
{
    public class BtcTransferService
    {
        public List<UnspentCoin> SelectedUnspentCoins { get; set; }
        public uint256 BroadcastedTransactionId { get; set; }
        public bool TrBuildFailed { get; set; }
        public bool NoConfirmedCoins { get; set; }
        public Money AutoFee { get; private set; }
        private List<Coin> nBitcoinSelectedUnspentCoins;
        private Money bitFeeRecommendedFastest;
        private List<BitcoinAddress> destinationAdresses;
        private Money amountToSend;

        private readonly BtcCommonService btcCommonService;
        private readonly BtcAddressService btcAddressService;
        private readonly BtcBalanceService btcBalanceService;

        public static BtcTransferService Instance { get; private set; }

        public static BtcTransferService CreateInstance(BtcCommonService btcCommonService, BtcAddressService btcAddressService, BtcBalanceService btcBalanceService)
        {
            Instance = new BtcTransferService(btcCommonService, btcAddressService, btcBalanceService);
            return Instance;
        }

        public BtcTransferService(BtcCommonService btcCommonService, BtcAddressService btcAddressService, BtcBalanceService btcBalanceService)
        {
            this.btcCommonService = btcCommonService;
            this.btcAddressService = btcAddressService;
            this.btcBalanceService = btcBalanceService;
            Task.Run(() => GetRecommendedFee());
        }

        /// <summary>
        /// Creates transaction based on amount and fee. Unspent tranaction outputs used for composing transaction are auto calcluted.
        /// if amount and fee is not greater than 0 or not enough there are not enough funds to build transaction exeption is thrown.
        /// </summary>
        /// <param name="amount"> amount needs to be sent</param>
        /// <param name="customFee">custom fee specified by user(if not specified fee will be autocalculated)</param>
        /// <param name="destinationAdressesStr">address to which amount needs to be sent</param>
        /// <returns>transaction built based on given amount and fee </returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="Exception"></exception>
        public Transaction PrepareTxCoinAutoSelection(Money amount, List<string> destinationAdressesStr, Money customFee = null)
        {
            destinationAdresses = new List<BitcoinAddress>();
            foreach (var address in destinationAdressesStr)
                destinationAdresses.Add(BitcoinAddress.Create(address, BtcCommonService.BitcoinNetwork));

            if (amount <= Money.Zero) throw new ArgumentOutOfRangeException(nameof(amount), "amount must be greater than 0");
            amountToSend = amount;

            var unspentCoins = new Dictionary<Coin, bool>();
            var utxos = btcCommonService.UtxosPerAddress.SelectMany(d => d.Value).ToList();
            foreach (var utxoDetailsElectrum in utxos)
            {
                var transaction = Transaction.Parse(utxoDetailsElectrum.TransactionHex, BtcCommonService.BitcoinNetwork);
                var coin = new Coin(transaction, (uint)utxoDetailsElectrum.TransactionPos);
                unspentCoins.Add(coin, utxoDetailsElectrum.Confirmed);
            }
            var unspentConfirmedCoins = new List<Coin>();
            var unspentUnconfirmedCoins = new List<Coin>();
            foreach (var coin in unspentCoins)
                if (coin.Value) unspentConfirmedCoins.Add(coin.Key);
                else unspentUnconfirmedCoins.Add(coin.Key);

            //GenerateNewChangeAdress();

            var coinsToSpend = new List<Coin>();

            if (customFee != null && customFee <= 0L) throw new ArgumentOutOfRangeException(nameof(customFee), "customFee must be greater than 0");
            Money fee = 0L;

            var builder = BtcCommonService.BitcoinNetwork.CreateTransactionBuilder();
            if (customFee != null)
            {
                fee = customFee;
                var haveEnough = SelectCoinsManualFee(ref coinsToSpend, amountToSend, fee, unspentConfirmedCoins);
                if (!haveEnough)
                    haveEnough = SelectCoinsManualFee(ref coinsToSpend, amountToSend, fee, unspentUnconfirmedCoins);
                if (!haveEnough)
                    throw new Exception("not enough funds");
            }
            else
            {
                var haveEnough = SelectCoinsAutoFee(ref coinsToSpend, amountToSend, ref fee, unspentConfirmedCoins);
                if (!haveEnough)
                    haveEnough = SelectCoinsAutoFee(ref coinsToSpend, amountToSend, ref fee, unspentUnconfirmedCoins);
                if (!haveEnough)
                    throw new Exception("not enough funds");
            }

            var signingKeys = PrepareSigningKeys(coinsToSpend);

            var tx = builder
                .AddCoins(coinsToSpend)
                .AddKeys(signingKeys.ToArray())
                .Send(destinationAdresses[0], amountToSend)
                .SetChange(btcCommonService.ChangeAddress)
                .SendFees(fee)
                .BuildTransaction(true);

            if (!builder.Verify(tx, out var errors))
            {
                Debug.WriteLine("Couldn't build the transaction.");
                TrBuildFailed = true;
                if (unspentConfirmedCoins.Count == 0)
                    NoConfirmedCoins = true;
                throw new Exception(errors.First().ToString());
            }

            TrBuildFailed = false;
            return tx;
        }

        /// <summary>
        /// Creates transaction based on amount and fee. Unspect tranction ouptuts needed to be used for tranction are previously selected by user.  
        /// if amount and fee is not greater than 0 or sum of those selected outputrs is less than fee + amount exeption is thrown.
        /// </summary>
        /// <param name="amount"> amount needs to be sent</param>
        /// <param name="customFee">custom fee specified by user(if not specified fee will be auto calculated)</param>
        /// <param name="destinationAdressesStr">address to which amount needs to be sent</param>
        /// <returns>transaction built based on given amount and fee </returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="Exception"></exception>
        public Transaction PrepareTxManualCoinSelection(Money amount, Money customFee, List<string> destinationAdressesStr)
        {
            destinationAdresses = new List<BitcoinAddress>();
            foreach (var address in destinationAdressesStr)
                destinationAdresses.Add(BitcoinAddress.Create(address, BtcCommonService.BitcoinNetwork));

            if (amount <= Money.Zero) throw new ArgumentOutOfRangeException(nameof(amount), "amount mustt be greater than 0");
            amountToSend = amount;

            nBitcoinSelectedUnspentCoins = new List<Coin>();

            if (SelectedUnspentCoins == null || (SelectedUnspentCoins != null && SelectedUnspentCoins.Count == 0))
                throw new Exception("no utxo selected");
            foreach (var selectedUnspentCoin in SelectedUnspentCoins)
            {
                var selectedCoin = btcCommonService.UtxosPerAddress.SelectMany(d => d.Value).ToList().Single(unspentCoin => unspentCoin.TransactionId.ToString() == selectedUnspentCoin.TransactionId && unspentCoin.Address == selectedUnspentCoin.Address);
                var transaction = Transaction.Parse(selectedCoin.TransactionHex, BtcCommonService.BitcoinNetwork);
                var coin = new Coin(transaction, (uint)selectedCoin.TransactionPos);
                nBitcoinSelectedUnspentCoins.Add(coin);
            }
            if (amountToSend > nBitcoinSelectedUnspentCoins.Sum(x => x.Amount)) throw new ArgumentException("amount must be less than sum of sellected utxos", nameof(amount));

            var signingKeys = PrepareSigningKeys(nBitcoinSelectedUnspentCoins);

            //GenerateNewChangeAdress();

            if (customFee != null && customFee <= 0L) throw new ArgumentOutOfRangeException(nameof(customFee), "customFee mustt be greater than 0");
            Money fee;
            if (customFee != null) fee = customFee;
            else fee = CalculateTransactionFee(nBitcoinSelectedUnspentCoins, signingKeys);

            if (amountToSend + fee > nBitcoinSelectedUnspentCoins.Sum(x => x.Amount)) throw new ArgumentException("amount + fee must be less than sum of sellected utxos", nameof(amount));
            var builder = BtcCommonService.BitcoinNetwork.CreateTransactionBuilder();
            var tx = builder
                .AddCoins(nBitcoinSelectedUnspentCoins)
                .AddKeys(signingKeys.ToArray())
                .Send(destinationAdresses[0], amountToSend)
                .SetChange(btcCommonService.ChangeAddress)
                .SendFees(fee)
                .BuildTransaction(true);

            if (!builder.Verify(tx))
            {
                Debug.WriteLine("Couldn't build the transaction.");
                TrBuildFailed = true;
                throw new Exception("Couldn't build the transaction");
            }

            return tx;
        }

        /// <summary>
        /// Selects unspent transaction outputs (coins) for building transaction based on given amount, fee and unspent transaction outputs
        /// from which coins for buidling transaction needs to be sellected. 
        /// </summary>
        /// <param name="coinsToSpend">  unspent transaction outputs which will be used for building tranction (reference type)</param>
        /// <param name="amount">amount needed to be sent</param>
        /// <param name="fee">specified fee</param>
        /// <param name="unspentCoins"> unspent transacion ouput list</param>
        /// <returns></returns>
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

        /// <summary>
        /// Selects unspent transaction outputs (coins) for building transaction based on given amount and unspent transaction outputs
        /// from which coins for buidling transaction needs to be sellected. Fee is autocalculated.
        /// </summary>
        /// <param name="coinsToSpend">  unspent transaction outputs which will be used for building tranction (reference type)</param>
        /// <param name="amount">amount needed to be sent</param>
        /// <param name="fee">specified fee</param>
        /// <param name="unspentCoins"> unspent transacion ouput list</param>
        /// <returns></returns>
        public bool SelectCoinsAutoFee(ref List<Coin> coinsToSpend, Money amount, ref Money fee, List<Coin> unspentCoins)
        {
            var haveEnough = false;
            var signingKeys = new List<Key>();
            foreach (var coin in unspentCoins.OrderByDescending(x => x.Amount))
            {
                coinsToSpend.Add(coin);
                // if doesn't reach amount, continue adding next coin
                if (coinsToSpend.Sum(x => x.Amount) < amount + fee) continue;
                signingKeys.AddRange(PrepareSigningKeys(coinsToSpend));
                fee = CalculateTransactionFee(coinsToSpend, signingKeys);
                if (coinsToSpend.Sum(x => x.Amount) < amount + fee) continue;
                haveEnough = true;
                break;
            }
            return haveEnough;
        }


        /// <summary>
        /// broadcasts given transaction to full node, adds this transaction to secure storage, generetes new change address, updates unspent transaction outputs and balance in UI.
        /// </summary>
        /// <param name="tx">transaction needed to be broadcasted</param>
        /// <returns></returns>
        public async Task BroadCastTransactionAsync(Transaction tx)
        {
            var broadcastResponse = await BtcCommonService.Client.BlockchainTransactionBroadcast(tx.ToHex());
            Debug.WriteLine("Transaction is successfully propagated on the network. TxId: " + broadcastResponse.Result, ConsoleColor.Green);
            if (broadcastResponse.Result == null)
            {
                BroadcastedTransactionId = null;
                TrBuildFailed = true;
                return;
            }
            TrBuildFailed = false;

            BroadcastedTransactionId = tx.GetHash();

            var txForStorage = await SaveTransactionAsync(tx);
            var bitcoinTransaction = BtcConverter.BtcTxForStoringToBtcTx(txForStorage, btcCommonService.ChangeAddress.ToString());

            btcAddressService.GenerateNewChangeAdress();
            btcCommonService.context.InvokeOnMainThread(() => BtcTxViewModelLocator.Instance.AddTransaction(bitcoinTransaction));
            btcBalanceService.UpdateUTXOListAndBalanceAfterBroadcasting(txForStorage);
        }

        /// <summary>
        /// Saves given transaction to secure sotrage.
        /// </summary>
        /// <param name="tx">transaction to save</param>
        /// <returns></returns>
        public async Task<BitcoinTransactionForStoring> SaveTransactionAsync(Transaction tx)
        {
            var address = btcCommonService.ChangeAddress.ToString();

            var trInputs = await BtcTransactionHelper.GetTransactionInputDetails(tx, address);
            var trOutputs = BtcTransactionHelper.GetTransactionOutputDetails(tx, address);
            var trDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var transactionFromStorage = new BitcoinTransactionForStoring()
            {
                TransactionHex = tx.ToHex(),
                TransactionId = tx.GetHash().ToString(),
                Date = trDate,
                Inputs = trInputs,
                Outputs = trOutputs,
                Address = address,
                Confirmed = false
            };

            foreach (var trOutput in transactionFromStorage.Outputs)
            {
                var outputAddr = BitcoinAddress.Create(trOutput.Address, BtcCommonService.BitcoinNetwork);
                if (btcCommonService.ChangeAddresses.Contains(outputAddr) || btcCommonService.MainAddresses.Contains(outputAddr))
                {
                    trOutput.IsUsersAddress = true;
                }
            }

            foreach (var trInput in transactionFromStorage.Inputs)
            {
                var inputAddr = BitcoinAddress.Create(trInput.Address, BtcCommonService.BitcoinNetwork);
                if (btcCommonService.ChangeAddresses.Contains(inputAddr) || btcCommonService.MainAddresses.Contains(inputAddr))
                {
                    trInput.IsUsersAddress = true;
                }
            }


            btcCommonService.TransactionsForStore.Add(transactionFromStorage);
            btcCommonService.context.SecureStorage.ObjectStorage.SaveObject(btcCommonService.TransactionsForStore, "BitcoinTransactions");
            return transactionFromStorage;
        }

        /// <summary>
        /// Gets recommended bit fee for making transaction.
        /// </summary>
        /// <exception cref="Exception">throws if can't get fee from external API</exception>
        public async Task GetRecommendedFee()
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

        /// <summary>
        ///  prepares signing keys for given unspent transaction outputs.
        /// </summary>
        /// <param name="SelectedUnspentCoins"> unspent transaction outputs</param>
        /// <returns>signing keys</returns>
        /// <exception cref="ArgumentException"> throws if for any of ouptuts singing key is not found </exception>
        public List<Key> PrepareSigningKeys(List<Coin> SelectedUnspentCoins)
        {
            btcAddressService.GenerateAddresses();
            var signingKeys = new List<Key>();
            foreach (var coin in SelectedUnspentCoins)
            {
                var address = coin.TxOut.ScriptPubKey.GetDestinationAddress(BtcCommonService.BitcoinNetwork);
                var addrIdxInMainAdrrList = btcCommonService.MainAddresses.FindIndex(addr => addr.ScriptPubKey.GetDestinationAddress(BtcCommonService.BitcoinNetwork) == address);
                var addrIdxInChangeAdrrList = btcCommonService.ChangeAddresses.FindIndex(addr => addr.ScriptPubKey.GetDestinationAddress(BtcCommonService.BitcoinNetwork) == address);

                var addrIdx = addrIdxInMainAdrrList != -1 ? addrIdxInMainAdrrList : addrIdxInChangeAdrrList;
                if (addrIdx == -1) throw new ArgumentException("Address not found in Device Storage");

                var signingKey = addrIdxInMainAdrrList != -1 ? btcCommonService.mainAdressesParentExtKey.Derive((uint)addrIdx).PrivateKey : btcCommonService.changeAdressesParentExtKey.Derive((uint)addrIdx).PrivateKey;
                signingKeys.Add(signingKey);
            }
            return signingKeys;
        }

        /// <summary>
        /// Calculates transaction fee based on selected unspent transaction outputs and recommended bit fee.
        /// </summary>
        /// <param name="SelectedUnspentCoins">unspent transaction outputs </param>
        /// <param name="signingKeys">signing keys for outputs</param>
        /// <returns>calculated fee</returns>
        public Money CalculateTransactionFee(List<Coin> SelectedUnspentCoins, List<Key> signingKeys)
        {
            var builder = BtcCommonService.BitcoinNetwork.CreateTransactionBuilder();
            var txForFeeCalculation = builder
                .AddCoins(SelectedUnspentCoins)
                .AddKeys(signingKeys.ToArray())
                .Send(destinationAdresses[0], amountToSend)
                .SetChange(btcCommonService.ChangeAddress)
                .SendFees(0L)
                .BuildTransaction(true);

            var fee = bitFeeRecommendedFastest * (txForFeeCalculation.GetVirtualSize() + SelectedUnspentCoins.Count);
            AutoFee = fee;
            return fee;
        }


        public bool CheckCoinTransferPosiblity(string amount, string customFee, List<UnspentCoin> selectedUnspentCoins)
        {
            nBitcoinSelectedUnspentCoins = new List<Coin>();
            Money fee;
            foreach (var selectedUnspentCoin in selectedUnspentCoins)
            {
                var selectedCoin = btcCommonService.UtxosPerAddress.SelectMany(d => d.Value).ToList().Single(unspentCoin => unspentCoin.TransactionId.ToString() == selectedUnspentCoin.TransactionId && unspentCoin.Address == selectedUnspentCoin.Address);
                var transaction = Transaction.Parse(selectedCoin.TransactionHex, BtcCommonService.BitcoinNetwork);
                var coin = new Coin(transaction, (uint)selectedCoin.TransactionPos);
                nBitcoinSelectedUnspentCoins.Add(coin);
            }

            if (customFee != null && customFee != "") fee = BitcoinHelper.ParseBtcString(customFee);
            else
            {
                var signingKeys = PrepareSigningKeys(nBitcoinSelectedUnspentCoins);
                fee = CalculateTransactionFee(nBitcoinSelectedUnspentCoins, signingKeys);
            }

            Money totalAmountOfSelectedCoins = nBitcoinSelectedUnspentCoins.Sum(x => x.Amount);
            amountToSend = BitcoinHelper.ParseBtcString(amount);

            if (totalAmountOfSelectedCoins < amountToSend + fee) return false;
            return true;
        }

        public bool CheckTransferPosiblityAuto(ref List<Coin> coinsToSpend, Money amount, ref Money fee, List<Coin> unspentCoins)
        {
            var haveEnough = false;
            var signingKeys = new List<Key>();
            foreach (var coin in unspentCoins.OrderByDescending(x => x.Amount))
            {
                coinsToSpend.Add(coin);
                // if doesn't reach amount, continue adding next coin
                if (coinsToSpend.Sum(x => x.Amount) < amount + fee) continue;
                signingKeys.AddRange(PrepareSigningKeys(coinsToSpend));
                fee = CalculateTransactionFee(coinsToSpend, signingKeys);
                if (coinsToSpend.Sum(x => x.Amount) < amount + fee) continue;
                haveEnough = true;
                break;
            }
            return haveEnough;
        }
    }
}
