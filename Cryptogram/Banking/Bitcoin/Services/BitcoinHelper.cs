using NBitcoin;
using QBitNinja.Client;
using QBitNinja.Client.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Xamarin.Forms;

namespace Banking.Services
{
    class BitcoinHelper
    {
        public static Dictionary<Coin, int> UnspentCoins { get; set; }

        private readonly BitcoinWalletService bitcoinWalletService;
        
        public static string RevertScriptHash(string script)
        {
            var cArray = script.ToCharArray();
            var reverse = string.Empty;
            for (var i = cArray.Length - 1; i > -1; i -= 2)
            {
                reverse += cArray[i - 1];
                reverse += cArray[i];
            }
            return reverse;
        }

        public static Money ParseBtcString(string value)
        {
            if (!decimal.TryParse(value.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var amount))
                Console.WriteLine("Wrong btc amount format.");
            return new Money(amount, MoneyUnit.BTC);
        }





        // ---------------------Old Implmenetations---------------------

        //public static Dictionary<BitcoinAddress, List<BalanceOperation>> QueryOperationsPerSafeAddresses(List<BitcoinAddress> addresses)
        //{
        //    var operationsPerAddresses = new Dictionary<BitcoinAddress, List<BalanceOperation>>();
        //    var unusedKeyCount = 0;
        //    foreach (var elem in QueryOperationsPerAddresses(addresses))
        //    {
        //        operationsPerAddresses.Add(elem.Key, elem.Value);
        //        if (elem.Value.Count == 0) unusedKeyCount++;
        //    }
        //    return operationsPerAddresses;
        //}

        //public static Dictionary<BitcoinAddress, List<BalanceOperation>> QueryOperationsPerAddresses(IEnumerable<BitcoinAddress> addresses)
        //{
        //    var operationsPerAddresses = new Dictionary<BitcoinAddress, List<BalanceOperation>>();
        //    var client = new QBitNinjaClient(BitcoinWalletService.BitcoinNetwork);
        //    foreach (var addr in addresses)
        //    {
        //        var operations = client.GetBalance(addr, unspentOnly: false).Result.Operations;
        //        operationsPerAddresses.Add(addr, operations);
        //    }
        //    return operationsPerAddresses;
        //}


        public static Dictionary<uint256, List<BalanceOperation>> GetOperationsPerTransactions(Dictionary<BitcoinAddress, List<BalanceOperation>> operationsPerAddresses)
        {
            // 1. Get all the unique operations
            var opSet = new HashSet<BalanceOperation>();
            foreach (var elem in operationsPerAddresses)
                foreach (var op in elem.Value)
                    opSet.Add(op);

            // 2. Get all operations, grouped by transactions
            var operationsPerTransactions = new Dictionary<uint256, List<BalanceOperation>>();
            foreach (var op in opSet)
            {
                var txId = op.TransactionId;
                if (operationsPerTransactions.TryGetValue(txId, out var ol))
                {
                    ol.Add(op);
                    operationsPerTransactions[txId] = ol;
                }
                else operationsPerTransactions.Add(txId, new List<BalanceOperation> { op });
            }
            return operationsPerTransactions;
        }

        public static void GetBalances(IEnumerable<AddressHistoryRecord> addressHistoryRecords, out Money confirmedBalance, out Money unconfirmedBalance)
        {
            confirmedBalance = Money.Zero;
            unconfirmedBalance = Money.Zero;
            foreach (var record in addressHistoryRecords)
            {
                if (record.Confirmed) confirmedBalance += record.Amount;
                else unconfirmedBalance += record.Amount;
            }
        }


        //public static Dictionary<Coin, bool> GetUnspentCoins(BitcoinSecret bitcoin)
        //{
        //    var unspentCoins = new Dictionary<Coin, bool>();

        //    var client = new QBitNinjaClient(BitcoinWalletService.BitcoinNetwork);
        //    string walletName;
        //    if (!Application.Current.Properties.ContainsKey("walletName"))
        //    {
        //        var walletModel = client.CreateWallet("mywallet123qa").Result;
        //        Application.Current.Properties["walletName"] = walletModel.Name;
        //        Application.Current.SavePropertiesAsync();
        //        walletName = walletModel.Name;
        //    }
        //    else
        //    {
        //        walletName = Application.Current.Properties["walletName"].ToString();
        //    }
        //    walletName = Application.Current.Properties["walletName"].ToString();
        //    var walletClient = client.GetWalletClient(walletName);
        //    var walletBalanceModel = walletClient.GetBalance().Result;

        //    //var destination = bitcoin.PrivateKey.ScriptPubKey.GetDestinationAddress(BitcoinNetwork);
        //    //var client = new QBitNinjaClient(BitcoinNetwork);
        //    //var balanceModel = client.GetBalance(destination, unspentOnly: true).Result;
        //    foreach (var operation in walletBalanceModel.Operations)
        //        foreach (var elem in operation.ReceivedCoins.Select(coin => coin as Coin))
        //            unspentCoins.Add(elem, operation.Confirmations > 0);

        //    return unspentCoins;
        //}

        //public static Dictionary<Coin, int> GetUnspentCoinsForSellection(BitcoinSecret bitcoin)
        //{
        //    UnspentCoins = new Dictionary<Coin, int>();

        //    //var destination = bitcoin.PrivateKey.ScriptPubKey.GetDestinationAddress(BitcoinNetwork);
        //    //var balanceModel = client.GetBalance(destination, unspentOnly: true).Result;
        //    var client = new QBitNinjaClient(BitcoinWalletService.BitcoinNetwork);
        //    string walletName;
        //    if (!Application.Current.Properties.ContainsKey("walletName"))
        //    {
        //        var walletModel = client.CreateWallet("mywallet123qa").Result;
        //        Application.Current.Properties["walletName"] = walletModel.Name;
        //        Application.Current.SavePropertiesAsync();

        //        walletName = walletModel.Name;
        //    }
        //    else
        //    {
        //        walletName = Application.Current.Properties["walletName"].ToString();
        //    }

        //    var walletClient = client.GetWalletClient(walletName);
        //    var x = walletClient.GetAddresses().Result;
        //    foreach (var z in x)
        //    {
        //        var d = z.Address.ToString();
        //    }
        //    var walletBalanceModel = walletClient.GetBalance().Result;

        //    foreach (var operation in walletBalanceModel.Operations)
        //        foreach (var elem in operation.ReceivedCoins.Select(coin => coin as Coin))
        //            UnspentCoins.Add(elem, operation.Confirmations);

        //    return UnspentCoins;
        //}







    }

}