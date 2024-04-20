using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Banking.Ehtereum.Models
{
    public class TxsOfAddressesDTO
    {
        //[JsonPropertyName("addressesEthTransactions")]
        public List<AddressEthTransactionsDTO> AddressesEthTransactions { get; set; }
    }
}
