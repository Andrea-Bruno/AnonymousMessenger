using System.Collections.Generic;

namespace CryptoWalletLibrary.Ehtereum.Models
{
    public class TxsOfAddressesDTO
    {
        public List<AddressEthTransactionsDTO> AddressesEthTransactions { get; set; }
    }
}
