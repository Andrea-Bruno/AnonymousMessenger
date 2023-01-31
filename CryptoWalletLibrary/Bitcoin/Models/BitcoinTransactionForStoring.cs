using System;
using System.Collections.Generic;

namespace CryptoWalletLibrary.Models
{
    public class BitcoinTransactionForStoring
    {
        public string TransactionHex { get; set; }
        public DateTime Date { get; set; }
        public string TransactionId { get; set; }
        public List<TransactionOutput> Outputs { get; set; }
        public List<TransactionInput> Inputs { get; set; }
        public string Address { get; set; }
        public bool Confirmed { get; set; }
    }
}