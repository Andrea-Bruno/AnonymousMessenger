using CryptoWalletLibrary.Models;

namespace CryptoWalletLibrary.Bitcoin.Services
{
    public class BtcConverter
    {

        public static BitcoinTransaction BtcTxForStoringToBtcTx(BitcoinTransactionForStoring transactionForStoring, string address)
        {
            var sent = false;
            var myTotalReceived = .0;
            var myTotalSent = .0;
            var realReceiversTotal = .0;
            foreach (var input in transactionForStoring.Inputs)
            {
                if (input.IsUsersAddress)
                {
                    myTotalSent += input.Amount;
                    sent = true;
                }
            }
            foreach (var output in transactionForStoring.Outputs)
            {
                if (output.IsUsersAddress) myTotalReceived += output.Amount;
                else
                {
                    realReceiversTotal += output.Amount;
                    if (sent) address = output.Address;
                }
            }

            var amount = .0;
            if (myTotalReceived == 0)
                amount = myTotalSent;
            else if (myTotalSent == 0)
                amount = myTotalReceived;
            else if (myTotalSent != 0 && myTotalReceived != 0)
                amount = realReceiversTotal != 0 ? realReceiversTotal : myTotalReceived;

            var bitcoinTransaction = new BitcoinTransaction()
            {
                Address = address.ToString(),
                Amount = (decimal)amount,
                Date = transactionForStoring.Date.Year == 1970 ? "N/A" : transactionForStoring.Date.ToString(),
                Sent = sent,
                TransactionId = transactionForStoring.TransactionId,
                Confirmed = transactionForStoring.Confirmed
            };
            return bitcoinTransaction;
        }
    }
}
