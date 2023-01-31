using MvvmHelpers;

namespace CryptoWalletLibrary.Models
{
    public class BitcoinBalance : ObservableObject
    {
        private double confirmed;
        public double Confirmed
        {
            get => confirmed;
            set => SetProperty(ref confirmed, value);
        }

        private double unconfirmed;
        public double Unconfirmed
        {
            get => unconfirmed;
            set => SetProperty(ref unconfirmed, value);
        }

        private double total;
        public double Total
        {
            get => total;
            set => SetProperty(ref total, value);
        }
    }
}