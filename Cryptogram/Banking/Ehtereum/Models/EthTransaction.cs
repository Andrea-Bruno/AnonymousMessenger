using Banking.Ehtereum.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Banking.Ehtereum.Models
{
    public class EthTransaction
    {
        public DateTime Time { get; set; }
        public string TxFrom { get; set; }
        public string TxTo { get; set; }
        public long? Gas { get; set; }
        public long? GasPrice { get; set; }
        public int? Block { get; set; }
        public string TxHash { get; set; }
        public double Value { get; set; }
        public string ContractTo { get; set; }
        public string ContractValue { get; set; }
        public bool Status { get; set; }
        public EthTxSentStatus Sent { get; set; }
        public string Address => Sent == EthTxSentStatus.SENT ? (ContractTo.Length > 0 ? ContractTo :TxTo) : TxFrom;
        public string FromOrTo
        {
            get
            {
                switch (Sent)
                {
                    case EthTxSentStatus.SENT: return "↑";
                    case EthTxSentStatus.RECEIVED: return "↓";
                    case EthTxSentStatus.BOTH: return "↑↓";
                    default: return "?"; ;
                }
            }
        }

        public string TokenName
        {
            get
            {
                if (ContractTo != null && ContractTo.Length > 0)
                    return CryptoWallet.AbbrByTokenAddr.TryGetValue(TxTo, out var abbr) ? abbr : "UNKOWN";
                else
                    return "ETH";
            }
        }
    }


}

