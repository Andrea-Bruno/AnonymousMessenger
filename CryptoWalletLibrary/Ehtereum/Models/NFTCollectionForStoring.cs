using System.Collections.Generic;

namespace CryptoWalletLibrary.Ehtereum.Models
{
    public class NFTCollectionForStoring
    {
        public string CollcetionName { get; set; }
        public string CollcetionAbbr { get; set; }
        public string ContractAddr { get; set; }
        public List<string> OwnerAdresses { get; set; }
    }
}
