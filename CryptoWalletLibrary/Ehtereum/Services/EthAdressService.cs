using Nethereum.Signer;
using Nethereum.Web3;

namespace CryptoWalletLibrary.Ehtereum.Services
{
    public class EthAdressService
    {
        private readonly EthCommonService ethCommonService;
        public static EthAdressService CreateInstance(EthCommonService ethCommonService) => new(ethCommonService);
        public static EthAdressService Instance { get; private set; }

        public EthAdressService(EthCommonService ethCommonService)
        {
            this.ethCommonService = ethCommonService;
            Instance = this;
        }

        /// <summary>
        /// Based on length and checksum verifies if the given address is a valid Ethereum address
        /// </summary>
        /// <param name="address"></param>
        /// <returns>  true if valid, otherwise false </returns>
        public static bool CheckIfAddrValid(string address)
        {
            if (address.Length == 42)
            {
                var checkSumAddr = Web3.ToChecksumAddress(address);
                return Web3.IsChecksumAddress(checkSumAddr);
            }
            else return false;
        }

        /// <summary>
        /// Generates new address for user based on user's last used address index.
        /// </summary>
        public void GetNewAddress()
        {
            ethCommonService.LastUsedAddressIdx++;
            ethCommonService.ShareAddress = ethCommonService.Wallet.GetAccount(ethCommonService.LastUsedAddressIdx).Address;
        }

        /// <summary>
        /// Checks if given address belongs to user. Check is performed untill user's last address index stored in application is reached.
        /// </summary>
        /// <param name="address"></param>
        /// <returns> true if it's user's address, otherwise false</returns>
        public bool AdressBelongsToUser(string address)
        {
            return ethCommonService.Wallet.GetAccount(address, chainId: (int)Chain.Rinkeby, maxIndexSearch: ethCommonService.LastUsedAddressIdx + 1) != null;
        }
    }
}
