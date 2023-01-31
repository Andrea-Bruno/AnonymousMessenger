using CryptoWalletLibrary.Bitcoin.Services;
using CryptoWalletLibrary.Ehtereum.Services;
using EncryptedMessaging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CryptoWalletLibrary
{

    public static class CryptoWalletLibInit
    {
        public static Dictionary<string, Dictionary<string, object>> ERC20PageMethodsByAbbr { get; private set; }
        public static IReadOnlyDictionary<string, string> TokenAddrByAbbr { get; private set; }
        public static IReadOnlyDictionary<string, string> TokenAbbrByAddr { get; private set; }

        /// <summary>
        /// Initializes Ethereum and Bitcoin services.
        /// </summary>
        /// <param name="context"></param>
        public static void Initialize(Context context)
        {
            ERC20PageMethodsByAbbr = new Dictionary<string, Dictionary<string, object>>();

            var comparer = StringComparer.OrdinalIgnoreCase;
            TokenAddrByAbbr = new Dictionary<string, string>(comparer) { { "RMT", "0xF48B2fECfc655F9F186424C69bdd80d78F0D5F7E" },
                /* { "RNB", "0x8a75f985d5316b1a98bc11fe5364abbad55e1c7a" }*/ };
            TokenAbbrByAddr = TokenAddrByAbbr.ToDictionary((i) => i.Value, (i) => i.Key, comparer);

            //BitcoinWalletService.CreateInstance(context);
            InitBitcoinServices(context);
            InitEtherServices(context);
        }

        public static void AddERC20PageMethodsForTxModif(Dictionary<string, Dictionary<string, object>> erc20PageMethods)
        {
            foreach (var pageAbbr in erc20PageMethods.Keys)
                ERC20PageMethodsByAbbr.Add(pageAbbr, erc20PageMethods[pageAbbr]);
        }

        /// <summary>
        /// Initializes Ehtereum services
        /// </summary>
        /// <param name="context"></param>
        private static void InitEtherServices(Context context)
        {
            var ethCommonService = EthCommonService.CreateInstance(context);
            var ethBalanceService = EthBalanceService.CreateInstance(ethCommonService);
            var ethAddressService = EthAdressService.CreateInstance(ethCommonService);
            EthTransactionService.CreateInstance(ethCommonService, ethBalanceService, ethAddressService);
            EthStorageService.CreateInstance(ethCommonService, ethBalanceService);
            EthTransferService.CreateInstance(ethCommonService, ethBalanceService);
        }

        /// <summary>
        /// Initializes Bitcoin services
        /// </summary>
        /// <param name="context"></param>
        private static void InitBitcoinServices(Context context)
        {
            var btcCommonService = BtcCommonService.CreateInstance(context);
            var btcStorageService = BtcStorageService.CreateInstance(btcCommonService);
            var btcBalanceService = BtcBalanceService.CreateInstance(btcCommonService);
            var btcAddressService = BtcAddressService.CreateInstance(btcCommonService);
            BtcTransactionService.CreateInstance(btcCommonService, btcStorageService, btcBalanceService);
            BtcTransferService.CreateInstance(btcCommonService, btcAddressService, btcBalanceService);

        }
    }
}
