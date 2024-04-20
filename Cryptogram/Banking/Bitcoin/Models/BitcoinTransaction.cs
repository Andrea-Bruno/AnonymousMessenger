namespace Banking.Models
{
    public class BitcoinTransaction
    {
        public string TransactionId { get; set; }
        public string Date { get; set; }
        public decimal Amount { get; set; }
        public bool Sent { get; set; }
        public string FromOrTo => Sent ? " ↑" : " ↓";
        public string Address { get; set; }

    }
}