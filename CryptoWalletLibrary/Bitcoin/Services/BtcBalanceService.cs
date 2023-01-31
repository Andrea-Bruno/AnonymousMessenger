using CryptoWalletLibrary.Models;
using System.Collections.Generic;
using System.Linq;
using static CryptoWalletLibrary.Models.BitcoinTransactionViewModel;
namespace CryptoWalletLibrary.Bitcoin.Services
{
    /// <summary>
    /// Service to get user's balance.
    /// </summary>
    public class BtcBalanceService
    {
        private readonly BtcCommonService btcCommonService;
        public static BtcBalanceService Instance { get; private set; }

        public static BtcBalanceService CreateInstance(BtcCommonService btcCommonService)
        {
            Instance = new BtcBalanceService(btcCommonService);
            return Instance;
        }

        public BtcBalanceService(BtcCommonService btcCommonService)
        {
            this.btcCommonService = btcCommonService;
            RestoreUTXOsAndBalanceFromStorage();
        }

        /// <summary>
        /// Based on fetched address transcations calculates unspent tranacion outputs, confirmed (transactions with comfied status) and total
        /// (transactions with confirmed + unconfirmed status) balance for user's each adress. In addtion total  and confirmed balance of all addresses are calculated.
        /// </summary>
        /// <param name="address">address to which transactions belong</param>
        /// <param name="transactionsOfAddr">transactions</param>
        public void GetUTXOsAndBalance(string address, List<BitcoinTransactionForStoring> transactionsOfAddr)
        {
            //First I collected all transactions of given address and only then start cheking if this tr is spent or not.
            //Otherwise later u can receive tr whcih spends tr u already indiacted as unspent. (or u need update utxo list each time u receive tr which spends your utxo)
            var confirmedBalance = .0;
            var unConfirmedBalance = .0;
            var receivedTransactionsOfAddr = transactionsOfAddr.Where(tr => tr.Outputs.Exists(output => output.Address == address)).ToList();
            foreach (var transaction in receivedTransactionsOfAddr)
            {
                var outputIdx = transaction.Outputs.FindIndex(output => output.Address == address);
                if (btcCommonService.TransactionsForStore.Exists(tr => tr.Inputs.Exists(input => input.TrId == transaction.TransactionId && input.OutputIdx == outputIdx)))
                {
                    if (btcCommonService.UtxosPerAddress.ContainsKey(address))
                    {
                        if (btcCommonService.UtxosPerAddress[address].RemoveAll(utxo => utxo.TransactionId == transaction.TransactionId) > 0)
                        {
                            if (transaction.Confirmed) confirmedBalance -= transaction.Outputs[outputIdx].Amount;
                            else unConfirmedBalance -= transaction.Outputs[outputIdx].Amount;
                        }
                    }
                    continue;
                }
                if (btcCommonService.UtxosPerAddress.ContainsKey(address) && btcCommonService.UtxosPerAddress[address].Exists(utxo => utxo.TransactionId == transaction.TransactionId)) continue;

                var utxoDetail = new UtxoDetailsElectrumx()
                {
                    Address = address,
                    Confirmed = transaction.Confirmed,
                    TransactionHex = transaction.TransactionHex,
                    TransactionId = transaction.TransactionId,
                    TransactionPos = outputIdx
                };
                if (btcCommonService.UtxosPerAddress.ContainsKey(address)) btcCommonService.UtxosPerAddress[address].Add(utxoDetail);
                else btcCommonService.UtxosPerAddress[address] = new List<UtxoDetailsElectrumx>() { utxoDetail };

                if (transaction.Confirmed) confirmedBalance += transaction.Outputs[outputIdx].Amount;
                else unConfirmedBalance += transaction.Outputs[outputIdx].Amount;
            }


            btcCommonService.TotalConfirmedBalance += confirmedBalance;
            btcCommonService.TotalUncnfirmedBalance += unConfirmedBalance;
            //MainThread.BeginInvokeOnMainThread(() =>
            //{
            //    Balance.Confirmed = (double.Parse(Balance.Confirmed) + confirmedBalance).ToString();
            //    Balance.Unconfirmed = (double.Parse(Balance.Unconfirmed) + unConfirmedBalance).ToString();
            //    Balance.Total = (double.Parse(Balance.Confirmed) + double.Parse(Balance.Unconfirmed)).ToString();
            //});
        }

        /// <summary>
        /// Restores unspent trsanction outputs and balance using transaction list which was restored from secure storagre. As a result  confimred and total(confimred + unconfimred) balance of user's each address are obtained.
        /// In addtion total and confirmed balance of all addresses are calculated.
        /// </summary>
        public void RestoreUTXOsAndBalanceFromStorage()
        {
            var confirmedBalance = .0;
            var unConfirmedBalance = .0;
            foreach (var transaction in btcCommonService.TransactionsForStore)
            {
                var outputIdx = -1;
                string address;
                foreach (var output in transaction.Outputs)
                {
                    outputIdx++;
                    if (output.IsUsersAddress) address = output.Address;
                    else continue;
                    if (btcCommonService.TransactionsForStore.Exists(tr => tr.Inputs.Exists(input => input.TrId == transaction.TransactionId && input.OutputIdx == outputIdx)))
                    {
                        if (btcCommonService.UtxosPerAddress.ContainsKey(address))
                        {
                            if (btcCommonService.UtxosPerAddress[address].RemoveAll(utxo => utxo.TransactionId == transaction.TransactionId) > 0)
                            {
                                if (transaction.Confirmed) confirmedBalance -= transaction.Outputs[outputIdx].Amount;
                                else unConfirmedBalance -= transaction.Outputs[outputIdx].Amount;
                            }
                        }
                        continue;
                    }
                    if (btcCommonService.UtxosPerAddress.ContainsKey(address) && btcCommonService.UtxosPerAddress[address].Exists(utxo => utxo.TransactionId == transaction.TransactionId)) continue;

                    var utxoDetail = new UtxoDetailsElectrumx()
                    {
                        Address = address,
                        Confirmed = transaction.Confirmed,
                        TransactionHex = transaction.TransactionHex,
                        TransactionId = transaction.TransactionId,
                        TransactionPos = outputIdx
                    };
                    if (btcCommonService.UtxosPerAddress.ContainsKey(address)) btcCommonService.UtxosPerAddress[address].Add(utxoDetail);
                    else btcCommonService.UtxosPerAddress[address] = new List<UtxoDetailsElectrumx>() { utxoDetail };

                    if (transaction.Confirmed) confirmedBalance += transaction.Outputs[outputIdx].Amount;
                    else unConfirmedBalance += transaction.Outputs[outputIdx].Amount;
                }
            }

            btcCommonService.TotalConfirmedBalance += confirmedBalance;
            btcCommonService.TotalUncnfirmedBalance += unConfirmedBalance;
        }

        /// <summary>
        /// Updates unspent trsanction outputs list and balance after trasanction is broadcasted. Updates balance in UI.
        /// </summary>
        /// <param name="tx">broadcasted transaction</param>
        internal void UpdateUTXOListAndBalanceAfterBroadcasting(BitcoinTransactionForStoring tx)
        {
            //adding new utxo
            var outputIdx = 0;
            foreach (var txOut in tx.Outputs)
            {
                var outputAddr = tx.Address;
                if (txOut.IsUsersAddress)
                {
                    UtxoDetailsElectrumx utxoDetail = new UtxoDetailsElectrumx()
                    {
                        Address = outputAddr.ToString(),
                        Confirmed = false,
                        TransactionHex = tx.TransactionHex,
                        TransactionId = tx.TransactionId,
                        TransactionPos = outputIdx,
                    };
                    if (btcCommonService.UtxosPerAddress.ContainsKey(outputAddr.ToString())) btcCommonService.UtxosPerAddress[outputAddr.ToString()].Add(utxoDetail);
                    else btcCommonService.UtxosPerAddress[outputAddr.ToString()] = new List<UtxoDetailsElectrumx>() { utxoDetail };

                    if (tx.Confirmed) btcCommonService.TotalConfirmedBalance += txOut.Amount;
                    else btcCommonService.TotalUncnfirmedBalance += txOut.Amount;
                    btcCommonService.context.InvokeOnMainThread(() => BtcTxViewModelLocator.Instance.AssignBalance());

                }
                outputIdx++;
            }

            //removing spent ones
            foreach (var txInput in tx.Inputs)
            {
                btcCommonService.UtxosPerAddress[txInput.Address].RemoveAll(utxo => utxo.TransactionId == txInput.TrId);
                if (tx.Confirmed) btcCommonService.TotalConfirmedBalance -= txInput.Amount;
                else btcCommonService.TotalUncnfirmedBalance -= txInput.Amount;
            }
        }
    }
}
