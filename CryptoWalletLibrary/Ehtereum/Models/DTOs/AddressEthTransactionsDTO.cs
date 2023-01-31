using System.Collections.Generic;

namespace CryptoWalletLibrary.Ehtereum.Models
{
    public class AddressEthTransactionsDTO
    {
        public string Address { get; set; }
        public List<EthTransactionDTO> EthTransactions { get; set; }
    }
}