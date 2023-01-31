using CryptoWalletLibrary.Models;
using ElectrumXClient;
using EncryptedMessaging;
using NBitcoin;
using System.Collections.Generic;
using System.Diagnostics;

namespace CryptoWalletLibrary.Bitcoin.Services
{
    public class BtcCommonService
    {
        internal readonly Context context;
        //private readonly BitcoinSecret secretKey;
        private readonly ExtKey masterKey;
        internal int changeAddrCount;
        internal int mainAddrCount;

        public BitcoinAddress MainAddress { get; set; }
        internal BitcoinAddress ChangeAddress { get; set; }
        internal List<BitcoinAddress> MainAddresses { get; set; }
        internal List<BitcoinAddress> ChangeAddresses { get; set; }

        internal readonly ExtKey mainAdressesParentExtKey;
        internal readonly ExtKey changeAdressesParentExtKey;
        internal readonly ExtPubKey mainAdressesParentExtPubKey;
        internal readonly ExtPubKey changeAdressesParentExtPubKey;

        internal static Client Client;
        private const string HOST = "90.191.43.19";
        private const int PORT = 8001;
        private const bool USE_SSL = false;
        public static Network BitcoinNetwork { get; set; }

        internal const int MAX_RANGE_EMPTY_ADRR = 5;

        public List<BitcoinTransactionForStoring> TransactionsForStore { get; set; }
        public Dictionary<string, List<UtxoDetailsElectrumx>> UtxosPerAddress { get; set; }
        public double TotalConfirmedBalance { get; set; }
        public double TotalUncnfirmedBalance { get; set; }
        public BitcoinTransaction SelectedBitcoinTransaction { get; set; }

        public static BtcCommonService Instance { get; private set; }

        public static BtcCommonService CreateInstance(Context context)
        {
            Instance = new BtcCommonService(context);
            return Instance;
        }

        public BtcCommonService(Context context)
        {
            this.context = context;
            BitcoinNetwork = Network.TestNet;

            Debug.WriteLine("BTCWalletService!: " + new Key().GetWif(BitcoinNetwork));

            //---- Clear Storage for Testing ----
            //ClearStorage();

            //---- Generating Ext and ExtPub Keys ----
            //var secretKeyStr = "L4VpjddASNaAKqfD7A3giyFcBoa7NrZizX1RNFskwwVcovpEojkZ";
            //secretKeyStr = "cMkyk7e3PBpHvSo5Bd6DnFipUrLkacsYSf8GUTNPRt33Gi1DU1bZ";
            //secretKey = new BitcoinSecret(secretKeyStr, Network.TestNet);
            //masterKey = new ExtKey(secretKey.PrivateKey, new byte[32]);

            var mnemo = new Mnemonic("thought bike index caught gaze brand soul diamond deputy claim syrup tiger", Wordlist.English);
            mnemo = new Mnemonic("melody nice sure tenant dad rough original crunch circle step verb reform", Wordlist.English);
            mnemo = new Mnemonic("asthma attend bus original science leaf deputy nuclear ticket valley vacuum tornado", Wordlist.English);

            //var key = new Key(context.My.GetPrivatKeyBinary());
            //masterKey = new ExtKey(key, new byte[32]);
            masterKey = mnemo.DeriveExtKey();

            var hardenedPathMainAdresses = new KeyPath("44'/1'/0'/0");
            var hardenedPathChangeAdresses = new KeyPath("44'/1'/0'/1");

            mainAdressesParentExtKey = masterKey.Derive(hardenedPathMainAdresses);
            changeAdressesParentExtKey = masterKey.Derive(hardenedPathChangeAdresses);
            mainAdressesParentExtPubKey = masterKey.Derive(hardenedPathMainAdresses).Neuter();
            changeAdressesParentExtPubKey = masterKey.Derive(hardenedPathChangeAdresses).Neuter();

            mainAddrCount = 0;
            changeAddrCount = 0;

            //---- Client for Electrumx ----
            Client = new Client(HOST, PORT, USE_SSL);

            MainAddresses = new List<BitcoinAddress>();
            ChangeAddresses = new List<BitcoinAddress>();
            TransactionsForStore = new List<BitcoinTransactionForStoring>();
            UtxosPerAddress = new Dictionary<string, List<UtxoDetailsElectrumx>>();
        }
    }
}
