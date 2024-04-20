namespace Banking.Models
{
    public class UnspentCoin
    {
        public string Amount { get; set; }
        public string TransactionId { get; set; }
        public string Address { get; set; }
        public bool Confirmed { get; set; }
    }
}