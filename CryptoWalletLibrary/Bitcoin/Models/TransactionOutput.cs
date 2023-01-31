namespace CryptoWalletLibrary.Models
{
    public class TransactionOutput
    {
        public string Address { get; set; }
        public double Amount { get; set; }
        public bool IsUsersAddress { get; set; }
    }
}