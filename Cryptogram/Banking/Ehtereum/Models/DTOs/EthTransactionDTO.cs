using Nethereum.Util;
using System.Text.Json.Serialization;

namespace Banking.Ehtereum.Models
{
    public class EthTransactionDTO
    {
        //[JsonPropertyName("time")]
        public int Time { get; set; }


        //[JsonPropertyName("txFrom")]
        public string TxFrom { get; set; }


        //[JsonPropertyName("txTo")]
        public string TxTo { get; set; }


        //[JsonPropertyName("gas")]
        public long Gas { get; set; }


        //[JsonPropertyName("gasPrice")]
        public long GasPrice { get; set; }


        //[JsonPropertyName("block")]
        public int Block { get; set; }


        //[JsonPropertyName("TxHash")]
        public string TxHash { get; set; }


        //[JsonPropertyName("Value")]
        public decimal Value { get; set; }


        //[JsonPropertyName("ContractTo")]
        public string ContractTo { get; set; }


        //[JsonPropertyName("ContractValue")]
        public string ContractValue { get; set; }


        //[JsonPropertyName("Status")]
        public bool Status { get; set; }

    }
}