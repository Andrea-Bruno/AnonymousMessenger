using System;

namespace CryptoWalletLibrary.Ehtereum.Models
{
    public class EthTransaction
    {
        public DateTime Time { get; set; }
        public string TxFrom { get; set; }
        public string TxTo { get; set; }
        public ulong? Gas { get; set; }
        public ulong? GasPrice { get; set; }
        public ulong? Block { get; set; }
        public string TxHash { get; set; }
        public decimal Value { get; set; }
        public string ContractTo { get; set; }
        public string ContractValue { get; set; }
        public EthTxSuccessStatus Status { get; set; }
        public ulong? ConfirmedBlockN { get; set; }
        public EthTxSentStatus Sent { get; set; }
        public string Address => Sent == EthTxSentStatus.SENT ? (ContractTo.Length > 0 ? ContractTo : TxTo) : TxFrom;
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
        public EthTxConfirmationStatus ConfirmationStatus
        {
            get
            {
                switch (ConfirmedBlockN)
                {
                    case null: return EthTxConfirmationStatus.PENDING;
                    case < 6: return EthTxConfirmationStatus.UNCONFIRMED;
                    default: return EthTxConfirmationStatus.CONFIRMED;
                }
            }
        }
        public string TokenName
        {
            get
            {
                if (ContractTo != null && ContractTo.Length > 0)
                    return CryptoWalletLibInit.TokenAbbrByAddr.TryGetValue(TxTo, out var abbr) ? abbr : "UNKOWN";
                else
                    return "ETH";
            }
        }
    }
}

