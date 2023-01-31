using CryptoWalletLibrary.Ehtereum.Models;
using EncryptedMessaging;
using Nethereum.HdWallet;
using Nethereum.RPC.Fee1559Suggestions;
using Nethereum.Web3;
using System.Collections.Generic;
using System.Diagnostics;


namespace CryptoWalletLibrary.Ehtereum.Services
{
    public class EthCommonService
    {
        internal readonly Context context;

        internal int LastUsedAddressIdx { get; set; }

        public List<EthTransactionForStoring> TransactionsFromStorage { get; internal set; }
        public List<NFTAssetForStoring> NftAssetsFromStorage { get; internal set; }
        public List<NFTCollectionForStoring> NFTCollectionsFromStorage { get; internal set; }

        public double TotalBalance { get; internal set; }
        public double TotalConfirmedBalance { get; internal set; }
        public Dictionary<string, double> TotalBalanceByToken { get; internal set; }
        public Dictionary<string, double> TotalConfirmedBalanceByToken { get; internal set; }
        public Dictionary<string, EthBalance> BalanceByAddress { get; internal set; }
        public Dictionary<string, Dictionary<string, EthBalance>> AddrBalanceByToken { get; internal set; }


        internal const int N_ADDRESS_TO_FETCH = 10; // number of address to fetch in 1 request
        internal const int MAX_RANGE_EMPTY_ADRR = 3; // number of empty address encountered to stop querying

        internal static Web3 Web3Clinet { get; private set; }
        internal Wallet Wallet { get; private set; }
        internal const string FULL_NODE_URL = "http://90.191.43.19:8545";
        //private const string FULL_NODE_URL = "https://rinkeby.infura.io/v3/27cf7d3b72cf4537a2110e6713920fab";

        public static EthCommonService Instance { get; private set; }
        public string ShareAddress { get; internal set; }
        public Fee1559 Fee { get; internal set; }
        public const int BLOCKS_NEEDED_FOR_TX_CONFIRM = 6;
        public static EthCommonService CreateInstance(Context context) => new(context);

        public EthCommonService(Context context)
        {
            this.context = context;

            Debug.WriteLine("ETHWalletService!");

            Web3Clinet = new Web3(url: FULL_NODE_URL);

            //var words = "share width eye layer issue tenant tree acid hour around sight text";
            var words = "route tennis where admit core secret strong wolf truly surface mother stadium";
            words = "canoe misery credit fork quality uphold goose fury arrange fly become defy";
            words = "idle must inherit fun adapt ketchup abandon praise west glimpse lift cave";
            //words = Context.My.GetPassphrase();
            Wallet = new Wallet(words, "");
            TransactionsFromStorage = new List<EthTransactionForStoring>();
            LastUsedAddressIdx = 0;
            ShareAddress = Wallet.GetAccount(0).Address;
            Instance = this;
        }
    }
}

