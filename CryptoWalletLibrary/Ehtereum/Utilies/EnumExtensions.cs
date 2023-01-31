using CryptoWalletLibrary.Ehtereum.Models;
using CryptoWalletLibrary.Ehtereum.Services;

namespace CryptoWalletLibrary.Ehtereum.Utilies
{
    internal static class EnumExtensions
    {
        public static EthTxSuccessStatus BoolToSuccessStatus(bool status) => status ? EthTxSuccessStatus.SUCCESS : EthTxSuccessStatus.FAIL;

        public static EthTxSentStatus BoolToSentStatus(bool sent) => sent ? EthTxSentStatus.SENT : EthTxSentStatus.RECEIVED;

        public static EthTxSentStatus AdressesToSSentStatus(string fromAddr, string toAddr)
        {
            if (fromAddr == toAddr
                || (EthAdressService.Instance.AdressBelongsToUser(fromAddr)
                && EthAdressService.Instance.AdressBelongsToUser(toAddr)))
                return EthTxSentStatus.BOTH;
            else if (EthAdressService.Instance.AdressBelongsToUser(fromAddr))
                return EthTxSentStatus.SENT;
            else
                return EthTxSentStatus.RECEIVED;
        }
    }
}
