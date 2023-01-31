using CryptoWalletLibrary.Ehtereum.Models;
using CryptoWalletLibrary.Ehtereum.Services;

namespace CryptoWalletLibrary.Ehtereum
{
    public class EthTransactionForStoring
    {
        public ulong Time { get; set; }
        public string TxFrom { get; set; }
        public string TxTo { get; set; }
        public ulong? GasUsed { get; set; }
        public ulong GasPrice { get; set; }
        public ulong? Block { get; set; }
        public string TxHash { get; set; }
        public decimal Value { get; set; }
        public string ContractTo { get; set; }
        public string ContractValue { get; set; }
        public EthTxSuccessStatus SuccessStatus { get; set; }
        public EthTxSentStatus Sent { get; set; }
        public ERC? Erc { get; set; }
        public ulong Nonce { get; set; }
        public ulong? ConfirmedBlockN { get; set; }
        public EthTxConfirmationStatus ConfirmationStatus
        {
            get
            {
                switch (ConfirmedBlockN)
                {
                    case null: return EthTxConfirmationStatus.PENDING;
                    case < EthCommonService.BLOCKS_NEEDED_FOR_TX_CONFIRM: return EthTxConfirmationStatus.UNCONFIRMED;
                    default: return EthTxConfirmationStatus.CONFIRMED;
                }
            }
        }
    }
}