using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Banking.Ehtereum.Models
{
    public class AddressEthTransactionsDTO
    {
        //[JsonPropertyName("address")]
        public string Address { get; set; }


        //[JsonPropertyName("ethTransactions")]
        public List<EthTransactionDTO> EthTransactions { get; set; }
    }
}