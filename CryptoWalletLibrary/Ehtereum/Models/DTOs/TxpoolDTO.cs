using System.Collections.Generic;

namespace CryptoWalletLibrary.Ehtereum.Models.DTOs
{
    public class TxpoolDTO
    {
        public Dictionary<string, Dictionary<string, string>> Pending { get; set; }
        public Dictionary<string, Dictionary<string, string>> Queued { get; set; }
    }
}
