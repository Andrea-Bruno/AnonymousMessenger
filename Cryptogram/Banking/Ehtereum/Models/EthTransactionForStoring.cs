using Banking.Ehtereum.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Banking.Ehtereum
{

    public class EthTransactionForStoring
    {
        public int Time { get; set; }
        public string TxFrom { get; set; }
        public string TxTo { get; set; }
        public long Gas { get; set; }
        public long GasPrice { get; set; }
        public int Block { get; set; }
        public string TxHash { get; set; }
        public decimal Value { get; set; }
        public string ContractTo { get; set; }
        public string ContractValue { get; set; }
        public bool Status { get; set; }
        public EthTxSentStatus Sent { get; set; }
    }
}