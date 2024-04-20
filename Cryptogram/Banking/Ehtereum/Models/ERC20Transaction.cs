using System;
using System.Collections.Generic;
using System.Text;

namespace Banking.Ehtereum.Models
{
    class ERC20Transaction
    {
        public DateTime Time { get; set; }
        public string TxFrom { get; set; }
        public string TxTo { get; set; }
        public long? Gas { get; set; }
        public long? GasPrice { get; set; }
        public int? Block { get; set; }
        public string TxHash { get; set; }
        public double Value { get; set; }
        public bool Status { get; set; }
        public EthTxSentStatus Sent { get; set; }
        public string Address => Sent == EthTxSentStatus.SENT ? TxTo : TxFrom;
        public string FromOrTo
        {
            get
            {
                switch (Sent)
                {
                    case EthTxSentStatus.SENT: return "↑";
                    case EthTxSentStatus.RECEIVED: return "↓";
                    case EthTxSentStatus.BOTH: return "↑↓";
                    default:
                        return "?"; ;
                }
            }
        }
    }
}
