
namespace Banking.Models
{
    public class UtxoDetailsElectrumx
    {
        public string TransactionId { get; set; }
        public int TransactionPos { get; set; }
        public string TransactionHex { get; set; }
        public bool Confirmed { get; set; }
        public string Address { get; set; }
    }
}
