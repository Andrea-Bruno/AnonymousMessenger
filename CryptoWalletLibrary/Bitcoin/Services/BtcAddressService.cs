using NBitcoin;
using System.Collections.Generic;

namespace CryptoWalletLibrary.Bitcoin.Services
{
    public class BtcAddressService
    {
        private readonly BtcCommonService btcCommonService;
        public static BtcAddressService Instance { get; private set; }

        public static BtcAddressService CreateInstance(BtcCommonService btcCommonService)
        {
            Instance = new BtcAddressService(btcCommonService);
            return Instance;
        }
        public BtcAddressService(BtcCommonService btcCommonService)
        {
            this.btcCommonService = btcCommonService;
            GenerateAddresses();
        }

        /// <summary>
        /// Generates new main address accrondng to BIP 44 standart
        /// </summary>
        public void GenerateNewMainAdress()
        {
            btcCommonService.mainAddrCount++;
            btcCommonService.MainAddress = btcCommonService.mainAdressesParentExtPubKey.Derive((uint)btcCommonService.mainAddrCount).GetPublicKey().GetAddress(ScriptPubKeyType.Legacy, BtcCommonService.BitcoinNetwork);
            btcCommonService.context.SecureStorage.Values.Set("mainAddrCount", btcCommonService.mainAddrCount);
        }


        /// <summary>
        /// Generates new main address accrondng to BIP 44 standart
        /// </summary>
        public void GenerateNewChangeAdress()
        {
            btcCommonService.changeAddrCount++;
            btcCommonService.ChangeAddress = btcCommonService.changeAdressesParentExtPubKey.Derive((uint)btcCommonService.changeAddrCount).GetPublicKey().GetAddress(ScriptPubKeyType.Legacy, BtcCommonService.BitcoinNetwork);
            btcCommonService.context.SecureStorage.Values.Set("changeAddrCount", btcCommonService.changeAddrCount);
        }

        /// <summary>
        /// generates all main and change address until main and change address count  is reached and sotres in properties
        /// </summary>
        public void GenerateAddresses()
        {
            btcCommonService.MainAddresses = new List<BitcoinAddress>();
            btcCommonService.ChangeAddresses = new List<BitcoinAddress>();
            for (var i = 0; i < btcCommonService.mainAddrCount; i++)
            {
                var generatedMainAddr = btcCommonService.mainAdressesParentExtPubKey.Derive((uint)i).GetPublicKey().GetAddress(ScriptPubKeyType.Legacy, BtcCommonService.BitcoinNetwork);
                btcCommonService.MainAddresses.Add(generatedMainAddr);
            }
            btcCommonService.MainAddress = btcCommonService.mainAdressesParentExtPubKey.Derive((uint)btcCommonService.mainAddrCount).GetPublicKey().GetAddress(ScriptPubKeyType.Legacy, BtcCommonService.BitcoinNetwork);
            for (var i = 0; i < btcCommonService.changeAddrCount; i++)
            {
                var generatedChangeAddr = btcCommonService.changeAdressesParentExtPubKey.Derive((uint)i).GetPublicKey().GetAddress(ScriptPubKeyType.Legacy, BtcCommonService.BitcoinNetwork);
                btcCommonService.ChangeAddresses.Add(generatedChangeAddr);
            }
            btcCommonService.ChangeAddress = btcCommonService.changeAdressesParentExtPubKey.Derive((uint)btcCommonService.changeAddrCount).GetPublicKey().GetAddress(ScriptPubKeyType.Legacy, BtcCommonService.BitcoinNetwork);
        }

        public void RemoveGeneratedAddresses()
        {
            btcCommonService.MainAddresses.Clear();
            btcCommonService.ChangeAddresses.Clear();
        }
    }
}
