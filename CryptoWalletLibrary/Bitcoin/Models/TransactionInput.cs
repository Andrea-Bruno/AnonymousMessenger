namespace CryptoWalletLibrary.Models
{
    public class TransactionInput
    {
        public string TrId { get; set; }
        public int OutputIdx { get; set; }
        public string Address { get; set; }
        public bool IsUsersAddress { get; set; }
        public double Amount { get; set; }
    }
}