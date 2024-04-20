namespace Banking.Models
{
    public class TransactionInput
    {
        public string TrId { get; set; }
        public int outputIdx { get; set; }
        public string Address { get; set; }
        public double Amount { get; set; }
    }
}